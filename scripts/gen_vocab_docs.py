#!/usr/bin/env python3
"""Generate the reader-facing Vocabulary browser from data/vocabulary/.

Single source of truth = data/vocabulary/ (the hand-maintained normative value sets). The markdown
under docs/model/vocabulary/ is a DERIVED ARTIFACT — do not hand-edit; edit data/vocabulary/, then:

    python scripts/gen_vocab_docs.py

Purpose: surface the actual value sets a dancer / DJ / domain expert cares about. Reads cleanly as
raw markdown on GitHub; the mkdocs Material site adds search, a TOC sidebar, and audio players as
progressive enhancement.

Layout (per the UX analysis): dimensions whose values carry rich, sibling-comparing descriptions
(genre, mood, rhythmic_feel, song_form, instrumentation, instrument) render each value as a
self-contained ENTRY — an H3 heading (emoji + name + code), a `·`-joined axis "chip" line, the
description with its "Distinguishing trait:" clause bolded, an optional audio line, and an aliases
line — grouped under H2 sections (family / tier / size). Short dimensions (vocality, theme,
recording_context, recording_quality_era) keep a compact table. The chip line is the scan layer;
the paragraph is the read layer — the two registers never share a cell, row, or bullet.
"""
from __future__ import annotations
import pathlib
import re
import shutil
import sys

try:
    import yaml
except ImportError:
    sys.exit("PyYAML required: pip install pyyaml")

from _csharp_emit import cs_string, header, pascal_case, write_cs, xml_escape, xmldoc
from _field_dimensions import backing_field
from gen_field_docs import first_sentence, folder_for

ROOT = pathlib.Path(__file__).resolve().parent.parent
VOCAB = ROOT / "data" / "vocabulary"
OUT = ROOT / "docs" / "model" / "vocabulary"
SRC = ROOT / "dotnet" / "src"

GEN_NOTE = ("_Generated from [`data/vocabulary/`]"
            "(https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/data/vocabulary) by "
            "`scripts/gen_vocab_docs.py` — do not hand-edit; edit `data/vocabulary/`, then re-run this._")

# Dimensions rendered as per-value ENTRIES (rich, sibling-comparing descriptions); the rest are tables.
# Overridable per dimension via a `doc_layout: entries|table` key on the core/<dim>.yaml header.
ENTRY_DIMS = {"genre", "mood", "rhythmic_feel", "song_form", "instrumentation", "instrument"}

# Axis "chips" shown on a value's entry (a value's full fingerprint; empty axes are omitted).
CHIP_AXES = {
    "genre": ["family", "era", "dance_relevance", "bpm_range"],
    "mood": ["energy", "valence"],
    "rhythmic_feel": ["pulse", "subdivision", "accent"],   # no global tier — ranking is scene-relative
    "song_form": ["bars", "pattern", "meter", "tier"],
    "instrumentation": ["size"],
    "instrument": ["tier"],
}
CHIP_LABEL = {"dance_relevance": "dance", "bpm_range": "bpm", "subdivision": "subdiv"}

# Columns for the TABLE-layout (short) dimensions: between Code and Description.
COLS = {
    "vocality": ["aliases"],
    "theme": ["aliases"],
    "recording_context": ["aliases"],
    "recording_quality_era": ["era", "aliases"],
    "routine": ["aliases", "typical_dances"],
}
COL_HEADER = {"era": "Era", "aliases": "Aliases", "typical_dances": "Dances"}

# Which axis each dimension is grouped by, the section order, and how a section is titled.
GROUP_BY = {
    "genre": "family",
    "rhythmic_feel": "tier",
    "song_form": "tier",
    "instrumentation": "size",
    "instrument": "tier",
}
GROUP_ORDER = {
    "rhythmic_feel": [1, 2, 3],
    "song_form": [1, 2, 3],
    "instrumentation": ["solo", "duo", "trio", "small", "medium", "large"],
    "instrument": ["specific", "generic"],
}
TIER_LEGEND = {
    # Generic across scenes, NOT swing-centric: tier is "how foundational within its own scene" — e.g. a
    # clave feel is tier 1 to a Latin floor. (Per-scene prominence comes with the scene pages.)
    "rhythmic_feel": {1: "foundational — core to a scene's groove", 2: "extended / secondary", 3: "niche / regional"},
    "song_form": {1: "foundational", 2: "extended & variant", 3: "rare / edge-case"},
}

# Optional per-dimension explanatory note rendered under the axes legend.
DIM_NOTE = {
    "vocality": "Codes read as `V` (vocal) + an optional **type** (`X` featured · `S` scat · `W` "
                "spoken · `G` group · `D` duet; omit for a plain lead) + an optional **gender suffix** "
                "(`M` male · `F` female). So `VM` = male lead, `VX` = featured (any gender), `VXM` = "
                "featured male, `VSF` = scat female; `V` alone is a generic vocal and `INS` is "
                "instrumental. Gendered variants are well-formed even where no separate value exists yet.",
}


def slug(s: str) -> str:
    return re.sub(r"[^a-z0-9]+", "-", s.lower()).strip("-")


def section_title(dim: str, key) -> str:
    if dim in TIER_LEGEND:
        return f"Tier {key} — {TIER_LEGEND[dim].get(key, '')}".rstrip(" —")
    if dim == "mood":
        return f"{humanize(key)} energy"
    if dim == "instrumentation":
        return f"{humanize(key)} ensembles"
    if dim == "instrument":
        return {"specific": "Specific instruments", "generic": "Generic families"}.get(key, humanize(key))
    return humanize(key)


def load(rel: str) -> dict:
    return yaml.safe_load((VOCAB / rel).read_text(encoding="utf-8")) or {}


def humanize(s: str) -> str:
    return str(s).replace("-", " ").replace("_", " ").strip().title()


def cell(v) -> str:
    if isinstance(v, list):
        v = ", ".join(str(x) for x in v)
    return str(v if v is not None else "").replace("|", "\\|").replace("\n", " ").strip()


def aliases(v: dict) -> str:
    al = v.get("aliases") or []
    return " · ".join(f"`{a}`" for a in al)


def code(v: dict) -> str:
    """The short stable per-term code (display.code, e.g. LH / JOY)."""
    c = (v.get("display") or {}).get("code")
    return f"`{c}`" if c else ""


def field_ref(dim: str, up: str = "../../") -> str | None:
    """The field-reference doc of the SDTS field a dimension supplies, or None when no field draws on it."""
    field = backing_field(dim)
    return f"{up}standard/fields/{folder_for(field)}/{field}.md" if field else None


def layout_for(dim: str, doc: dict) -> str:
    override = doc.get("doc_layout")
    if override in ("entries", "table"):
        return override
    return "entries" if dim in ENTRY_DIMS else "table"


def col_cell(v: dict, key: str) -> str:
    """Render one value's cell/chip value for a given axis key."""
    if key == "aliases":
        return aliases(v)
    if key == "era":
        s, e = v.get("era_start"), v.get("era_end")
        if not (s or e):
            return ""
        return f"{cell(s) or '?'}–{cell(e)}" if e else f"{cell(s)}–"
    if key == "bpm_range":
        val = v.get("bpm_range")
        if isinstance(val, list) and len(val) == 2:
            return f"{val[0]}–{val[1]}"
        return cell(val)
    return cell(v.get(key))


GLOSSARY = VOCAB / "glossary.en.yaml"
_GLOSS: dict | None = None


def gloss() -> dict:
    """Hand-authored axis-value + genre-family glossary (see glossary.en.yaml)."""
    global _GLOSS
    if _GLOSS is None:
        _GLOSS = (yaml.safe_load(GLOSSARY.read_text(encoding="utf-8")) if GLOSSARY.is_file() else {}) or {}
    return _GLOSS


REVIEW_PENDING = "expert-review-pending"
_REVIEW: dict[str, frozenset[str]] = {}


def review_pending(dim: str) -> frozenset[str]:
    """Attributes a core dimension's top-level `validation:` map lists as expert-review-pending."""
    if dim not in _REVIEW:
        is_core = (VOCAB / "core" / f"{dim}.yaml").is_file()
        marks = (load(f"core/{dim}.yaml").get("validation") if is_core else None) or {}
        _REVIEW[dim] = frozenset(key for key, status in marks.items() if status == REVIEW_PENDING)
    return _REVIEW[dim]


# (attribute, label on the page, page its canonical references link to)
TYPICAL_LINKS = (("typical_rhythmic_feels", "Typical feels", "rhythmic_feel"),
                 ("typical_song_forms", "Typical song forms", "song_form"))


def review_banner(subject: str) -> list[str]:
    """The page-level expert-review flag; flags elsewhere link to its anchor. `subject` ends in "are"."""
    return [f'<a id="{slug(REVIEW_PENDING)}"></a>', "",
            f"> ⚠ **Expert review pending** — {subject} SDTS-authored drafts that a domain expert has not "
            f"confirmed yet; treat them as a starting point, not as authoritative.", ""]


def review_note(dim: str) -> list[str]:
    """The page-level flag for a dimension's review-pending attributes; each flagged line links back here."""
    label_for = {key: label for key, label, _ in TYPICAL_LINKS}
    names = [f"**{label_for.get(key, humanize(key))}**" for key in sorted(review_pending(dim))]
    if not names:
        return []
    listed = names[0] if len(names) == 1 else ", ".join(names[:-1]) + " and " + names[-1]
    return review_banner(f"{listed} on this page are")


def ax_anchor(axis: str, value) -> str:
    return f"ax-{axis}-{slug(str(value))}"


def chip_md(v: dict, dim: str, k: str) -> str:
    """One axis chip; links its value to a definition where one exists — family → its section on
    this page, other axes → the page's Axis reference appendix."""
    val = col_cell(v, k)
    if not val:
        return ""
    label = CHIP_LABEL.get(k, k.replace("_", " "))
    if k == "family":
        return f"{label}: [{val}](#fam-{slug(val)})"
    if str(val) in gloss().get("axes", {}).get(k, {}):
        return f"{label}: [{val}](#{ax_anchor(k, val)})"
    return f"{label}: {val}"


def axis_reference(dim: str) -> list[str]:
    """An 'Axis reference' appendix defining every glossary-backed axis value the dimension uses
    (each an anchor target the chips and the axes legend link to). No-op for dims with no such axes."""
    g = gloss().get("axes", {})
    axes = [k for k in CHIP_AXES.get(dim, []) if k in g]
    if not axes:
        return []
    out = ["## Axis reference", ""]
    for ax in axes:
        out += [f"**{humanize(ax)}**", ""]
        for val, desc in g[ax].items():
            out.append(f'- <a id="{ax_anchor(ax, val)}"></a>`{val}` — {desc}')
        out.append("")
    return out


# Parse a "Distinguishing trait" clause written in the leading "vs <Sibling> <difference>" style into
# rows. Only that style (common in rhythmic_feel) — the parenthetical "(vs X)" style (genre/mood) is
# left as prose, since splitting on its in-sentence "vs" would mangle it.
_QUAL = r"(?:(?:the|a|an|plain|generic|classic|early|modern|social|competition)\s+)*"
_VS_SPLIT = re.compile(r"\s*[;,]?\s*(?:and\s+)?\bvs\.?\s+(?=" + _QUAL + r"[A-Z])", re.IGNORECASE)
_SIB = re.compile(r"^(" + _QUAL + r"[A-Z][\w'’.\-]*(?:\s+(?:[A-Z][\w'’.\-]*|\d[\w'’.\-]*))*)\s*(.*)$", re.S)


def trait_table(trait: str) -> tuple[str, list]:
    """(lead_text, [(sibling, difference), …]); rows empty if it is not the leading-'vs' style."""
    if "(vs" in trait or not _VS_SPLIT.search(trait):
        return trait, []
    parts = _VS_SPLIT.split(trait)
    rows = []
    for part in parts[1:]:
        part = re.sub(r"[;,]?\s*(?:and)?\s*$", "", part.strip()).strip()
        m = _SIB.match(part)
        if not m:
            return trait, []
        sib = re.sub(r"^(?:the|a|an)\s+", "", m.group(1).strip(), flags=re.IGNORECASE)
        diff = m.group(2).strip().lstrip(",").strip()
        if not sib or len(sib.split()) > 6 or not diff:
            return trait, []
        rows.append((sib, diff))
    return parts[0].strip().rstrip(",;").strip(), rows


# ── Entry layout (rich dimensions) ──────────────────────────────────────────────────────────────
def value_entry(v: dict, dim: str) -> list[str]:
    disp = v.get("display") or {}
    emoji = disp.get("emoji")
    name = cell(v.get("canonical", v.get("id")))
    head = "### " + (f"{emoji} " if emoji else "") + name + (f" {code(v)}" if code(v) else "")
    if v.get("deprecated"):
        head += " ⚠"
    # Stable anchor (slug of the canonical) so other pages can deep-link to this value — e.g. a genre's
    # "typical feels" linking to the feel's entry. Same slug is used on the link side.
    out = [f'<a id="{slug(v.get("canonical") or v.get("id") or "")}"></a>', "", head, ""]

    chips = [c for c in (chip_md(v, dim, k) for k in CHIP_AXES.get(dim, [])) if c]
    if chips:
        out += [" · ".join(chips), ""]

    if v.get("deprecated"):
        repl = v.get("replaced_by")
        out += [f"_⚠ Deprecated{f' → use {repl}' if repl else ''}._", ""]

    desc = v.get("description")
    if desc:
        lead, sep, trait = str(desc).partition("Distinguishing trait:")
        out += [lead.strip(), ""]
        if sep:
            t_lead, rows = trait_table(trait.strip())
            if rows:   # render the "vs X / how it differs" comparisons as a table
                out += [f"**Distinguishing trait** — {t_lead}".rstrip(" —"), ""]
                out += ["| vs | how it differs |", "|---|---|"]
                out += [f"| **{cell(sib)}** | {cell(diff)} |" for sib, diff in rows]
                out += [""]
            else:
                out += [f"**Distinguishing trait:** {trait.strip()}", ""]

    # Typical rhythmic feels / song forms (genres only), each deep-linked to its entry on the feel / form
    # page (canonical name → the slug anchor value_entry emits above).
    for key, label, target in TYPICAL_LINKS:
        references = v.get(key) or []
        if references:
            flag = f' [⚠](#{slug(REVIEW_PENDING)} "Expert review pending")' if key in review_pending(dim) else ""
            out += [f"**{label}:**{flag} "
                    + " · ".join(f"[{cell(x)}]({target}.md#{slug(x)})" for x in references), ""]

    audio = v.get("audio")
    if audio:
        if audio.get("clip"):   # the synth-clip link is upgraded to an inline looping player on the site
            clip = audio["clip"]
            line = f'🔊 **Hear it** — <span class="sdt-audio">[synthesized rhythm](../../audio/{clip})</span>'
            if clip.endswith(".mp3"):   # the editable MIDI source sits beside the rendered clip
                line += f' · [MIDI](../../audio/{clip[:-4]}.mid)'
            out += [line, ""]
            if clip.endswith(".mp3"):   # the beat-grid SVG visualises the pattern
                out += [f'![beat grid](../../audio/{clip[:-4]}.svg)', ""]
        if audio.get("youtube"):   # the real recording, on its own line
            out += [f'▶ **On record** — [{cell(audio.get("youtube_label") or "YouTube example")}]({audio["youtube"]})', ""]

    if v.get("aliases"):
        out += [f"<sub>Aliases: {' · '.join(cell(a) for a in v['aliases'])}</sub>", ""]
    return out


# ── Table layout (short dimensions) ─────────────────────────────────────────────────────────────
def col_header(key: str) -> str:
    return COL_HEADER.get(key, humanize(key))


def value_table(values: list, dim: str) -> list[str]:
    cols = COLS.get(dim, ["aliases"])
    out = ["| Value | Code | " + "".join(f"{col_header(c)} | " for c in cols) + "Description |",
           "|---|---|" + "---|" * len(cols) + "---|"]
    for v in values:
        dep = " ⚠" if v.get("deprecated") else ""
        desc = cell(v.get("description"))
        if v.get("deprecated"):
            repl = v.get("replaced_by")
            desc = (f"⚠ deprecated{f' → use {repl}' if repl else ''}. " + desc).strip()
        cells = "".join(f"{col_cell(v, c)} | " for c in cols)
        out.append(f"| **{cell(v.get('canonical', v.get('id')))}**{dep} | {code(v)} | {cells}{desc} |")
    return out


def grouped(values: list, key: str, order: list) -> list:
    buckets: dict = {}
    for v in values:
        buckets.setdefault(v.get(key), []).append(v)
    keys = [k for k in order if k in buckets] + [k for k in buckets if k not in order]
    return [(k, buckets[k]) for k in keys]


# ── Dances: per-pack pages under a "Dances" section (presentation grouping; SDTS docs authority) ──
# The dance dimension is emitted as one landing page (dances.md, the section index) plus one page per
# pack (dances/<pack>.md), so the left nav and each page's TOC surface the pack→family hierarchy instead
# of one 100+-line flat dump. Pure presentation: family membership and pack assignment are catalog-owned
# (data/vocabulary/packs/**) and untouched — this only decides how families are SHOWN within a pack.
#
# PACK_PRESENTATION groups a pack's families into a primary partner-dance flow and a de-emphasised
# secondary group, and lets a thin 1-member family fold into a sibling (no standalone heading peer):
#   fold:      {family: into_family} — family's members are merged into into_family's section (the family
#              heading is dropped). Used to fold the 1-member "boogie-rocknroll" under Swing partner.
#   secondary: [family, …]           — rendered last, under a "Beyond the partner floor" heading with an
#              out-of-primary-scope note. Used to de-emphasise solo-rhythmic (solo jazz / Charleston / tap)
#              — still valid and taggable, just not a prominent peer of the partner families.
PACK_PRESENTATION = {
    "swing": {
        "fold": {"boogie-rocknroll": "swing-partner"},
        "secondary": ["solo-rhythmic"],
    },
}
SECONDARY_NOTE = ("These styles are valid and taggable but sit **outside the primary partner-dance "
                  "scope** — they are mostly solo / rhythmic, so they are grouped here rather than as "
                  "peers of the partner dances above.")


GROUPS_SECTION = "groups"
GROUPS_NOTE = ("A group value names a set of related dances and has no steps of its own; the dances below "
               "point at it through their **Group** column. Tag it when a track suits the whole set.")


def cross_pack_parent_names(pack_name: str) -> dict:
    """Canonical names of the parents that live in another pack (West Coast Swing -> Swing), so a pack
    page can name a group it does not itself define."""
    names: dict = {}
    for other in load("manifest.yaml").get("packs", []):
        if other != pack_name:
            names.update({v["id"]: v["canonical"] for v in load(f"packs/{other}/dances.yaml").get("values", [])})
    return names


def dance_table(vals: list, parent_names: dict) -> list[str]:
    """The Dance | Code | Aliases | Group | Tempo | Typical genres table for one family section, plus any
    deprecation callouts. `parent_names` maps a parent id to its canonical name. (Used by both the
    primary-family and secondary-group renderers.)"""
    out = ["| Dance | Code | Aliases | Group | Tempo (BPM) | Typical genres |", "|---|---|---|---|---|---|"]
    for v in vals:
        tempo = (f"{v['tempo_min']}–{v['tempo_max']}"
                 if v.get("tempo_min") and v.get("tempo_max") else "")
        dep = " ⚠" if v.get("deprecated") else ""
        out.append(f"| **{cell(v.get('canonical'))}**{dep} | {code(v)} | {aliases(v)} | "
                   f"{cell(parent_names.get(v.get('parent')))} | {tempo} | "
                   f"{cell(v.get('typical_genres'))} |")
    out.append("")
    for v in [x for x in vals if x.get("deprecated")]:
        repl = v.get("replaced_by")
        suffix = f" → use {repl}" if repl else ""
        out += [f"> ⚠ **{cell(v.get('canonical'))}** is deprecated{suffix}.", ""]
    return out


def dance_pack_page(pack_name: str) -> str:
    """One page per dance pack (dances/<pack>.md): families as H2 sections so the nav + per-page TOC
    show the pack→family hierarchy. Presentation grouping per PACK_PRESENTATION (fold / secondary)."""
    meta = load(f"packs/{pack_name}/pack.yaml")
    doc = load(f"packs/{pack_name}/dances.yaml")
    pres = PACK_PRESENTATION.get(pack_name, {})
    fold = pres.get("fold", {})           # thin family -> sibling family it folds into
    secondary = pres.get("secondary", []) # families shown last, de-emphasised

    parent_names = {v["id"]: v["canonical"] for v in doc.get("values", [])}
    parent_names.update(cross_pack_parent_names(pack_name))
    by_fam: dict[str, list] = {}
    for v in doc.get("values", []):
        family = v.get("family", GROUPS_SECTION)   # a group value has no family of its own
        by_fam.setdefault(fold.get(family, family), []).append(v)
    declared = [GROUPS_SECTION] + [f for f in meta.get("families", []) if f not in fold]
    fam_order = declared + [f for f in by_fam if f not in declared]
    primary = [f for f in fam_order if f not in secondary]
    secondary = [f for f in fam_order if f in secondary]

    title = meta.get("display_name", humanize(pack_name))
    out = [f"# {title} dances", ""]
    if meta.get("description"):
        out += [cell(meta["description"]), ""]
    out += [f"→ Field reference: [Dance]({field_ref('dances', up='../../../')}) · "
            "[all dance packs](../dances.md) · the [music & dance model](../../index.md)", ""]
    if meta.get("validation") == REVIEW_PENDING:
        out += review_banner("the dances in this pack are")

    for fam in primary:
        vals = by_fam.get(fam)
        if not vals:
            continue
        out += [f"## {humanize(fam)}", ""]
        if fam == GROUPS_SECTION:
            out += [GROUPS_NOTE, ""]
        out += dance_table(vals, parent_names)

    sec_present = [f for f in secondary if by_fam.get(f)]
    if sec_present:
        out += ["## Beyond the partner floor", "", SECONDARY_NOTE, ""]
        for fam in sec_present:
            out += [f"### {humanize(fam)}", ""]
            out += dance_table(by_fam[fam], parent_names)

    out += [GEN_NOTE, ""]
    return "\n".join(out)


def dances_index_page(manifest: dict) -> str:
    """The Dances landing page (dances.md) — the "Dances" nav section index. Explains the pack/family
    model and the tempo/genre patterns, then links out to one page per pack. (Kept at dances.md so the
    inbound links from README, docs/README and the generated field docs still resolve.)"""
    packs = manifest.get("packs", [])
    out = ["# Dances",
           "",
           "Which social dances each track serves — the danceability axis. Tempo bands are the "
           "DJ's first filter; a track fits a dance only inside its band **and** with the right feel.",
           "",
           f"→ Field reference: [Dance]({field_ref('dances')}) · "
           "the [music & dance model](../index.md)",
           "",
           "## How the dance vocabulary is organised",
           "",
           "Dances are partitioned into **packs** — one per dance scene (Swing, Blues, …) — so a "
           "library only loads the floors it actually plays. Within a pack, dances are grouped into "
           "**families** of close relatives (e.g. the swing-partner family holds Lindy Hop, Balboa, "
           "the Shags). A dance carries a **tempo band** (its danceable BPM range) and its **typical "
           "genres** — the two filters a DJ reaches for first: a track fits a dance only inside its "
           "band *and* with the right feel and genre.",
           "",
           "## Packs",
           ""]
    for pack_name in packs:
        meta = load(f"packs/{pack_name}/pack.yaml")
        title = meta.get("display_name", humanize(pack_name))
        desc = cell(meta.get("description")) or ""
        line = f"- **[{title}](dances/{pack_name}.md)**"
        if meta.get("validation") == REVIEW_PENDING:
            line += f" [⚠](dances/{pack_name}.md#{slug(REVIEW_PENDING)} \"Expert review pending\")"
        out.append(f"{line} — {desc}" if desc else line)
    deferred = manifest.get("deferred_dance_families", [])
    if deferred:
        out += ["",
                "Deferred dance families (not yet built): "
                + ", ".join(humanize(f) for f in deferred) + "."]
    out += ["", GEN_NOTE, ""]
    return "\n".join(out)


def _band3(n, axis):
    """Fold a 1–10 mood axis coordinate to its 3-bucket band ([1-4]/[5-6]/[7-10]) for the docs grid.
    The stored value is the 1–10 number; 3×3 and 5×5 are non-normative consumer discretisations."""
    if n is None:
        return None
    n = int(n)
    lo, mid, hi = ("dark", "neutral", "bright") if axis == "valence" else ("low", "medium", "high")
    return lo if n <= 4 else mid if n <= 6 else hi


def mood_grid(values: list) -> list[str]:
    energy = ["high", "medium", "low"]
    valence = ["bright", "neutral", "dark"]
    cellmap: dict[tuple, list] = {}
    for v in values:
        key = (_band3(v.get("energy"), "energy"), _band3(v.get("valence"), "valence"))
        cellmap.setdefault(key, []).append(cell(v.get("canonical")))
    out = ["Each value carries a 1–10 **energy × valence** coordinate on the "
           "[Russell circumplex](https://en.wikipedia.org/wiki/Emotion_classification#Circumplex_model) "
           "plane; this grid folds them to the 3×3 view — a 5×5 or finer view buckets the same 1–10, since "
           "discretisation is the consumer's choice, not the standard's:", "",
           "| energy ↓ / valence → | " + " | ".join(humanize(x) for x in valence) + " |",
           "|---|" + "---|" * len(valence)]
    for e in energy:
        row = [humanize(e)] + [", ".join(cellmap.get((e, val), [])) for val in valence]
        out.append("| " + " | ".join(row) + " |")
    out.append("")
    return out


SCENE_ORDER = ["swing", "blues", "tango", "ballroom", "street-latin"]
SCENE_TITLE = {"swing": "Swing", "blues": "Blues", "tango": "Tango (Argentine)",
               "ballroom": "Ballroom (International)", "street-latin": "Street Latin"}
_FEEL_SCENES: dict | None = None


def feel_scenes() -> dict:
    """Scene-relative feel ranking overlay (feel_scenes.yaml): id → {scenes: {scene: tier}, anchors}."""
    global _FEEL_SCENES
    if _FEEL_SCENES is None:
        p = VOCAB / "feel_scenes.yaml"
        _FEEL_SCENES = ((yaml.safe_load(p.read_text(encoding="utf-8")) if p.is_file() else {}) or {}).get("feels", {})
    return _FEEL_SCENES


def feel_by_scene(values: list) -> list[str]:
    """Group rhythmic feels by dance scene — each under the scene(s) whose dances use it, anchor feels
    first; the full entry under its primary scene, a back-reference elsewhere; unanchored feels last."""
    fs = feel_scenes()
    byid = {v["id"]: v for v in values}
    primary = {fid: min(info["scenes"], key=lambda s: (info["scenes"][s], SCENE_ORDER.index(s) if s in SCENE_ORDER else 9))
               for fid, info in fs.items() if info.get("scenes")}
    out: list[str] = []
    for scene in SCENE_ORDER:
        members = sorted(((fid, info["scenes"][scene]) for fid, info in fs.items()
                          if fid in byid and scene in (info.get("scenes") or {})),
                         key=lambda x: (x[1], byid[x[0]].get("canonical", "")))
        if not members:
            continue
        out += [f"## {SCENE_TITLE.get(scene, humanize(scene))}", ""]
        for fid, _tier in members:
            v = byid[fid]
            if primary.get(fid) == scene:
                out += [f'<a id="feel-{fid}"></a>', ""] + value_entry(v, "rhythmic_feel")
            else:
                ps = primary.get(fid)
                out += [f"- **{cell(v['canonical'])}** {code(v)} — also danced here; see "
                        f"[{SCENE_TITLE.get(ps, ps)}](#feel-{fid}).", ""]
    extra = [v for v in values if v["id"] not in fs or not fs[v["id"]].get("scenes")]
    if extra:
        out += ["## Cross-genre & listening (no anchoring dance)", "",
                "Feels with no social dance in the current packs — a flavour or listening groove, not a floor feel.", ""]
        for v in extra:
            out += [f'<a id="feel-{v["id"]}"></a>', ""] + value_entry(v, "rhythmic_feel")
    return out


_GENRE_SCENES: dict | None = None
_DR_RANK = {"high": 0, "medium": 1, "low": 2, "none": 3}


def genre_scenes() -> dict:
    """Genre → dance-scene overlay (genre_scenes.yaml): id → {scenes: [...], primary: scene}."""
    global _GENRE_SCENES
    if _GENRE_SCENES is None:
        p = VOCAB / "genre_scenes.yaml"
        _GENRE_SCENES = ((yaml.safe_load(p.read_text(encoding="utf-8")) if p.is_file() else {}) or {}).get("genres", {})
    return _GENRE_SCENES


def genre_by_scene(values: list) -> list[str]:
    """Group genres by dance scene — each under the scene(s) it is danced to (full entry under its
    primary scene, a back-reference elsewhere); listening/crossover genres gather at the end."""
    gs = genre_scenes()
    byid = {v["id"]: v for v in values}

    def by_rel(vid: str) -> tuple:
        v = byid[vid]
        return (_DR_RANK.get(v.get("dance_relevance"), 4), v.get("canonical", ""))

    out: list[str] = []
    for scene in SCENE_ORDER:
        members = [vid for vid, info in gs.items() if vid in byid and scene in (info.get("scenes") or [])]
        if not members:
            continue
        out += [f"## {SCENE_TITLE.get(scene, humanize(scene))}", ""]
        for vid in sorted([m for m in members if gs[m].get("primary") == scene], key=by_rel):
            out += [f'<a id="genre-{vid}"></a>', ""] + value_entry(byid[vid], "genre")
        sec = sorted([m for m in members if gs[m].get("primary") != scene], key=by_rel)
        if sec:
            links = ", ".join(f"**{cell(byid[v].get('canonical'))}** "
                              f"([{SCENE_TITLE.get(gs[v]['primary'], gs[v]['primary'])}](#genre-{v}))" for v in sec)
            out += [f"_Also danced here:_ {links}", ""]
    extra = sorted((v for v in values if not (gs.get(v["id"], {}).get("scenes"))),
                   key=lambda v: (v.get("family", "~"), _DR_RANK.get(v.get("dance_relevance"), 4), v.get("canonical", "")))
    if extra:
        out += ["## Cross-genre & listening", "",
                "Genres a DJ may tag but that anchor no social-dance floor of their own — listening, "
                "crossover and adjacent styles, ordered by musical family.", ""]
        for v in extra:
            out += [f'<a id="genre-{v["id"]}"></a>', ""] + value_entry(v, "genre")
    return out


def family_reference(values: list) -> list[str]:
    """A 'Genre families' appendix defining each musical family the page uses — the `#fam-*` anchors
    the family chips link to (scene grouping replaced the old per-family sections)."""
    fams = gloss().get("families", {})
    present: list[str] = []
    for v in values:
        f = v.get("family")
        if f and f not in present:
            present.append(f)
    if not present:
        return []
    out = ["## Genre families", "", "What each `family` chip groups.", ""]
    for f in sorted(present):
        out.append(f'- <a id="fam-{slug(f)}"></a>**{humanize(f)}** — {fams.get(f, humanize(f))}')
    out.append("")
    return out


def dimension_page(dim: str, doc: dict) -> str:
    """One page per shared music dimension (genre, mood, …)."""
    name = humanize(dim)
    values = doc.get("values", [])
    layout = layout_for(dim, doc)
    group_key = GROUP_BY.get(dim)
    ref = field_ref(dim)
    out = [f"# {name}",
           "",
           f"The **{name.lower()}** vocabulary — each value's canonical name (what is stored), the "
           f"aliases that resolve to it, and the axes that make it queryable.",
           "",
           "→ " + (f"Field reference: [{name}]({ref}) · " if ref else "")
           + "[all dimensions](README.md) · [dances](dances.md)",
           ""]
    axes = doc.get("axes") or []
    if axes:
        g = gloss().get("axes", {})
        parts = []
        for a in axes:
            nm, vals = a.get("name", ""), a.get("values", [])
            rng = a.get("range")
            if rng:   # a ranged (ordinal 1..N) axis: show the range + orientation, not a token list
                orient = a.get("orientation", "")
                parts.append(f"**{nm}** ({rng[0]}–{rng[1]}" + (f", {orient}" if orient else "") + ")")
                continue
            if not vals or (dim == "rhythmic_feel" and nm == "tier"):   # tier dropped for feels
                continue
            shown = ", ".join(
                f"[{x}](#{ax_anchor(nm, x)})" if str(x) in g.get(nm, {}) else str(x)
                for x in vals)
            parts.append(f"**{nm}** ({shown})")
        if parts:
            out += [f"Axes — {'; '.join(parts)}.", ""]
    out += review_note(dim)

    if dim == "rhythmic_feel":   # grouped by dance scene, not by a swing-biased global tier
        out += ["Grouped by **dance scene** — each feel appears under the scene(s) whose dances use it, "
                "anchor feels first; importance is scene-relative, so there is no global tier.", ""]
        out += feel_by_scene(values)
        out += axis_reference(dim)
        out += [GEN_NOTE, ""]
        return "\n".join(out)

    if dim == "genre":   # grouped by dance scene (the de-swing rework), not by a swing-biased family order
        gs = genre_scenes()
        present = [s for s in SCENE_ORDER if any(s in (gs.get(v["id"], {}).get("scenes") or []) for v in values)]
        jump = " · ".join(f"[{SCENE_TITLE[s]}](#{slug(SCENE_TITLE[s])})" for s in present)
        out += ["Grouped by **dance scene** — each genre appears under the scene(s) it is danced to; "
                "listening and crossover genres gather under Cross-genre at the end.", "",
                f"{jump} · [Cross-genre & listening](#{slug('Cross-genre & listening')})", ""]
        out += genre_by_scene(values)
        out += family_reference(values)
        out += axis_reference(dim)
        out += [GEN_NOTE, ""]
        return "\n".join(out)

    groups = grouped(values, group_key, GROUP_ORDER.get(dim, [])) if group_key else []
    if group_key:
        out += [f"Grouped by **{group_key.replace('_', ' ')}**.", ""]
        if layout == "entries" and len(groups) > 6:   # genre's 16 families: a jump-list for GitHub readers
            out += [" · ".join(f"[{section_title(dim, k)}](#{slug(section_title(dim, k))})" for k, _ in groups), ""]
    if DIM_NOTE.get(dim):
        out += [DIM_NOTE[dim], ""]
    if dim == "mood":
        out += mood_grid(values)

    def render(vals: list) -> list[str]:
        if layout == "entries":
            block: list[str] = []
            for v in vals:
                block += value_entry(v, dim)
            return block
        return value_table(vals, dim) + [""]

    if group_key:
        for key, vals in groups:
            out += [f"## {section_title(dim, key)}", ""]
            if dim == "genre":   # one-line family definition under its section heading
                fam = gloss().get("families", {}).get(key)
                if fam:
                    out += [f"_{fam}_", ""]
            out += render(vals)
    else:
        out += render(values)

    out += axis_reference(dim)
    out += [GEN_NOTE, ""]
    return "\n".join(out)


def index_page(manifest: dict) -> str:
    version = manifest.get("vocabulary_version", "")
    packs = manifest.get("packs", [])
    dims = manifest.get("core", {}).get("dimensions", [])
    out = ["# Vocabulary",
           "",
           f"The normative value sets a track is tagged with (baseline `{version}`). "
           "This is a generated, browsable view of the hand-maintained value sets in `data/vocabulary/`.",
           "",
           "- **[Dances](dances.md)** — the dance packs (" + ", ".join(humanize(p) for p in packs)
           + "): each dance with its tempo band, aliases, and typical genres.",
           "- **Musical dimensions** — the shared music vocabulary, one page per dimension:",
           "  " + " · ".join(f"[{humanize(d)}]({d}.md)" for d in dims),
           "",
           "Deferred dance families (not yet built): "
           + ", ".join(humanize(f) for f in manifest.get("deferred_dance_families", [])) + ".",
           "",
           "How a value is used and stored is the [field reference](../../standard/fields/README.md); "
           "this page is about the values themselves.",
           "",
           GEN_NOTE, ""]
    return "\n".join(out)


def _pack_members(pack_cls: str, values: list[dict]) -> list[str]:
    """Member names inside a pack's nested catalog class. A member may not share the enclosing class's name
    (CS0542) — a one-dance pack named after its dance (`west-coast-swing`) would — so it takes a `Dance` suffix."""
    names = [pascal_case(v.get("id") or v.get("canonical") or "") for v in values]
    return _dedup([f"{n}Dance" if n == pack_cls else n for n in names])


def _dedup(names: list[str]) -> list[str]:
    """Make member names unique within a class (PascalCase can collide); suffix + warn."""
    seen: dict[str, int] = {}
    out = []
    for raw in names:
        if raw in seen:
            seen[raw] += 1
            out.append(f"{raw}_{seen[raw]}")
            print(f"  WARN: duplicate generated member {raw} -> {raw}_{seen[raw]}")
        else:
            seen[raw] = 1
            out.append(raw)
    return out


# Per-dimension C# type names (record type, catalog class). The dimension set is fixed, so these
# canonical domain type names are a generator-config decision; the term DATA still comes wholly from YAML.
DIM_TYPES = {
    "genre": ("Genre", "Genres"),
    "mood": ("Mood", "Moods"),
    "rhythmic_feel": ("RhythmicFeel", "RhythmicFeels"),
    "vocality": ("Vocality", "Vocalities"),
    "instrumentation": ("Instrumentation", "Instrumentations"),
    "instrument": ("Instrument", "Instruments"),
    "recording_context": ("RecordingContext", "RecordingContexts"),
    "recording_quality_era": ("RecordingQualityEra", "RecordingQualityEras"),
    "theme": ("Theme", "Themes"),
    "song_form": ("SongForm", "SongForms"),
    "credit_role": ("CreditRole", "CreditRoles"),
    "routine": ("Routine", "Routines"),
}


def _aliases_expr(v: dict) -> str:
    """A C# collection expression for a term's aliases (empty -> `[]`)."""
    return "[" + ", ".join(cs_string(a) for a in (v.get("aliases") or [])) + "]"


def _term_ctor(v: dict, code_type: str, schema: list = ()) -> str:
    """The `new(...)` initializer for one term record: code, canonical, aliases, description, plus the
    `{ Prop = … }` object-initializer carrying this value's typed attributes."""
    code = (v.get("display") or {}).get("code") or ""
    canonical = v.get("canonical", v.get("id", ""))
    return (f"new(new {code_type}({cs_string(code)}), {cs_string(canonical)}, "
            f"{_aliases_expr(v)}, {cs_string(v.get('description') or '')})"
            f"{_attr_initializer(v, schema)}")


def _term_summary(v: dict) -> str:
    """The XML <summary> body for a term member: `Canonical (CODE) — first sentence`."""
    canonical = xml_escape(str(v.get("canonical", v.get("id", ""))))
    c = (v.get("display") or {}).get("code")
    sent = xml_escape(first_sentence(str(v.get("description", ""))).strip())
    return canonical + (f" (<c>{xml_escape(c)}</c>)" if c else "") + (f" — {sent}" if sent else "")


def _term_types(record: str, code_type: str, noun: str, src: str, schema: list = ()) -> list[str]:
    """The shared preamble for a dimension's generated file: its <Dim>Code struct + <Dim> record
    (the record carries the dimension's typed attributes as init-only members)."""
    return [
        header("gen_vocab_docs.py", src),
        "using System.Collections.Generic;\n\n",
        "namespace SocialDanceTaggingStandard.Taxonomy;\n\n",
        f'/// <summary>The display code of a <see cref="{record}"/> term — its own type, so a {code_type}\n',
        "/// can't be passed where another dimension's code is expected (CLAUDE.md coding guideline 1).</summary>\n",
        f"public readonly record struct {code_type}(string Value)\n{{\n",
        "    /// <summary>The raw display-code string.</summary>\n",
        "    public override string ToString() => Value;\n}\n\n",
        f'/// <summary>A {noun} vocabulary term — its <see cref="{code_type}"/>, canonical value, aliases,\n',
        "/// description and the dimension's typed attributes. Its own type so terms of different dimensions\n",
        "/// never mix (CLAUDE.md guideline 1).</summary>\n",
        f"public sealed record {record}({code_type} Code, string Canonical, "
        "IReadOnlyList<string> Aliases, string Description)\n{\n",
        *_attr_members(schema),
        "    /// <summary>The canonical value as stored on a track.</summary>\n",
        "    public override string ToString() => Canonical;\n}\n\n",
    ]


def _value_types(record: str, code_type: str, catalog: str, noun: str) -> list[str]:
    """The per-dimension occurrence type (<Dim>Value) + frame reading (<Dim>Field). Strongly typed so
    values of different dimensions never mix (CLAUDE.md guideline 1)."""
    return [
        f'/// <summary>A parsed {noun} occurrence: a catalog <see cref="{record}"/> (or '
        f'<see cref="{catalog}.Generic"/>) plus the exact text it was\n',
        f"/// read from, tagged with how it entered the field. Its own type so a {record}Value can't be used\n",
        "/// where another dimension's value is expected (CLAUDE.md guideline 1).</summary>\n",
        f"public readonly record struct {record}Value({record} Term, string StoredValue, "
        "ValueOrigin Origin = ValueOrigin.Stored)\n{\n",
        f"    /// <summary>False when the value did not match the catalog (Term == {catalog}.Generic).</summary>\n",
        f"    public bool IsKnown => !ReferenceEquals(Term, {catalog}.Generic);\n",
        "    /// <summary>The catalog form; equals StoredValue for an unknown (Generic has no canonical).</summary>\n",
        "    public string Canonical => IsKnown ? Term.Canonical : StoredValue;\n}\n\n",
        f"/// <summary>A {noun} field reading: the parsed values plus the exact original frame string\n",
        "/// (preserved for byte-for-byte write-back).</summary>\n",
        f"public sealed record {record}Field(IReadOnlyList<{record}Value> Values, string RawFrame);\n\n",
    ]


def _catalog_resolution_members(record: str, code_type: str, members: list[str], values: list[dict]) -> list[str]:
    """The Generic null-object term, the id->constant lookup, and the runtime-term projection, appended
    inside the <Dim>s catalog class."""
    out = [
        f"    /// <summary>The generic representative for a {record.lower()} not in the catalog "
        "(null-object term).</summary>\n",
        f'    public static readonly {record} Generic = new(new {code_type}(""), "", [], "");\n\n',
        "    /// <summary>Catalog terms by stable id — resolves a runtime VocabularyTerm to its constant.</summary>\n",
        f"    public static readonly IReadOnlyDictionary<string, {record}> ById =\n"
        f"        new Dictionary<string, {record}>(System.StringComparer.OrdinalIgnoreCase)\n        {{\n",
    ]
    for member, v in zip(members, values):
        out.append(f"            [{cs_string(v.get('id') or v.get('canonical') or '')}] = {member},\n")
    out += [
        "        };\n\n",
        "    /// <summary>Project a runtime term (e.g. a custom/overlay term absent from this baseline\n",
        f"    /// catalog) to a {record}. Used by the typed view when ById has no entry.</summary>\n",
        f"    public static {record} FromTerm(VocabularyTerm t) =>\n"
        f'        new(new {code_type}(t.Display?.Code ?? ""), t.Canonical, t.Aliases, "");\n',
    ]
    return out


def _view_type(record: str, catalog: str, noun: str) -> list[str]:
    """The per-dimension typed resolver bound to a runtime Dimension (the matching engine stays
    Dimension.Resolve; this is a thin typed projection)."""
    return [
        f"/// <summary>Typed resolver for the {noun} dimension over a loaded (possibly customized)\n",
        "/// vocabulary — the matching engine stays Dimension.Resolve; this is a thin typed projection.</summary>\n",
        f"public sealed class {record}View\n{{\n",
        "    private readonly Dimension _dim;\n",
        "    private readonly string _delimiter;\n\n",
        '    /// <summary>Wrap a runtime dimension; <paramref name="delimiter"/> is the multi-value separator.</summary>\n',
        f'    public {record}View(Dimension dimension, string delimiter = "; ")\n',
        '    {\n        _dim = dimension;\n        _delimiter = string.IsNullOrEmpty(delimiter) ? "; " : delimiter;\n    }\n\n',
        f"    /// <summary>The generic (not-in-catalog) {noun} term.</summary>\n",
        f"    public {record} Generic => {catalog}.Generic;\n\n",
        "    /// <summary>Resolve one token; a miss returns the generic carrying the raw text. Never null.</summary>\n",
        f"    public {record}Value Resolve(string token)\n    {{\n",
        "        var term = _dim.Resolve(token);\n",
        f"        if (term is null) return new {record}Value({catalog}.Generic, token);\n",
        f"        var hit = {catalog}.ById.TryGetValue(term.Id, out var c) ? c : {catalog}.FromTerm(term);\n",
        f"        return new {record}Value(hit, token);\n    }}\n\n",
        "    /// <summary>Read a whole (multi-value) frame: per-value occurrences + the exact original frame.</summary>\n",
        f"    public {record}Field Read(string? rawFrame)\n    {{\n",
        '        var frame = rawFrame ?? "";\n',
        f"        var values = new List<{record}Value>();\n",
        "        foreach (var part in frame.Split([_delimiter], System.StringSplitOptions.None))\n",
        "        {\n            var token = part.Trim();\n            if (token.Length == 0) continue;\n",
        "            values.Add(Resolve(token));\n        }\n",
        f"        return new {record}Field(values, frame);\n    }}\n}}\n\n",
    ]


def emit_vocab_views(dims: list[str], packs: list[str]) -> None:
    """Generated vocab.<Dim>() accessors (C# extension members) so resolution is instance-bound and
    honors per-developer customization."""
    tax = SRC / "SocialDanceTaggingStandard.Taxonomy" / "Generated"
    body = [header("gen_vocab_docs.py", "data/vocabulary/"),
            "namespace SocialDanceTaggingStandard.Taxonomy;\n\n",
            "/// <summary>Typed, instance-bound resolution views — vocab.Genre(), vocab.Mood(), … — so a\n",
            "/// loaded (possibly customized) vocabulary resolves tokens to strongly-typed values.</summary>\n",
            "public static class VocabularyViews\n{\n",
            "    extension(SocialDanceVocabulary vocab)\n    {\n"]
    for dim in dims:
        record, _ = DIM_TYPES[dim]
        body.append(f"        /// <summary>Typed resolver for the {humanize(dim).lower()} dimension.</summary>\n")
        body.append(f"        public {record}View {record}() =>\n")
        body.append(f"            new(vocab.Dimension(DimensionIds.{record}) ?? throw new System.InvalidOperationException(\n")
        body.append(f'                "vocabulary has no \'{dim}\' dimension"));\n')
    body.append("\n        /// <summary>Typed resolver for the dances across the named packs (default: all enabled packs).</summary>\n")
    body.append("        public DanceView Dance(params PackId[] packs) =>\n")
    body.append("            new(vocab.Dances(packs.Length > 0 ? packs : System.Linq.Enumerable.ToArray(vocab.Packs.Keys)));\n")
    body.append("    }\n}\n")
    write_cs(tax / "VocabularyViews.g.cs", "".join(body))


def emit_vocab_ids(manifest: dict) -> tuple[int, int, int]:
    """Generate the typed vocabulary surface (CLAUDE.md guidelines 1 + 2) into
    SocialDanceTaggingStandard.Taxonomy/Generated/: DimensionIds, PackIds, and one strongly-typed term catalog per
    dimension (Genre/Mood/…/Dance, each with its own <Dim>Code) carrying Code, Canonical, Aliases and
    Description. A drift-gate test keeps the catalogs in step with the runtime SocialDanceVocabulary."""
    dims = manifest.get("core", {}).get("dimensions", [])
    packs = manifest.get("packs", [])
    tax = SRC / "SocialDanceTaggingStandard.Taxonomy" / "Generated"
    src = "data/vocabulary/"

    def simple(cls: str, summary: str, struct: str, ids: list[str], fname: str) -> None:
        rows = list(zip(_dedup([pascal_case(i) for i in ids]), ids))
        body = [header("gen_vocab_docs.py", src),
                "namespace SocialDanceTaggingStandard.Taxonomy;\n\n",
                f"/// <summary>{summary}</summary>\n",
                f"public static class {cls}\n{{\n"]
        body += [f"    public static readonly {struct} {m} = new({cs_string(v)});\n" for m, v in rows]
        body.append("}\n")
        write_cs(tax / fname, "".join(body))

    simple("DimensionIds", "Canonical DimensionId for every core vocabulary dimension (data/vocabulary/manifest.yaml).",
           "DimensionId", dims, "DimensionIds.g.cs")
    simple("PackIds", "Canonical PackId for every dance pack (data/vocabulary/manifest.yaml).",
           "PackId", packs, "PackIds.g.cs")

    n_terms = 0
    for dim in dims:
        record, catalog = DIM_TYPES[dim]
        code_type = f"{record}Code"
        noun = humanize(dim).lower()
        values = load(f"core/{dim}.yaml").get("values") or []
        members = _dedup([pascal_case(v.get("id") or v.get("canonical") or "") for v in values])
        schema = ATTR_SCHEMA.get(dim, [])
        body = _term_types(record, code_type, noun, src, schema)
        body += [f"/// <summary>The generated {record} catalog (data/vocabulary/core/{dim}.yaml), drift-gated\n",
                 '/// against the runtime <see cref="SocialDanceVocabulary"/>.</summary>\n',
                 f"public static class {catalog}\n{{\n"]
        for member, v in zip(members, values):
            body.append(xmldoc(_term_summary(v)))
            body.append(f"    public static readonly {record} {member} = {_term_ctor(v, code_type, schema)};\n")
        body.append(f"\n    /// <summary>All {len(members)} {noun} terms, in catalog order.</summary>\n")
        body.append(f"    public static readonly IReadOnlyList<{record}> All =\n    [\n")
        body += [f"        {m},\n" for m in members]
        body.append("    ];\n\n")
        body += _catalog_resolution_members(record, code_type, members, values)
        body.append("}\n\n")
        body += _value_types(record, code_type, catalog, noun)
        body += _view_type(record, catalog, noun)
        write_cs(tax / f"{record}.g.cs", "".join(body))
        n_terms += len(members)

    # The dances dimension: one Dance type, catalog nested by pack (two packs can share a dance name).
    dance_schema = ATTR_SCHEMA["dances"]
    body = _term_types("Dance", "DanceCode", "dance", src, dance_schema)
    body += ['/// <summary>The generated dance catalog grouped by pack (data/vocabulary/packs/), drift-gated\n',
             '/// against the runtime <see cref="SocialDanceVocabulary"/>.</summary>\n',
             "public static class Dances\n{\n"]
    all_refs: list[str] = []
    for pack in packs:
        values = load(f"packs/{pack}/dances.yaml").get("values") or []
        pcls = pascal_case(pack)
        members = _pack_members(pcls, values)
        body.append(f"    /// <summary>The <c>{pack}</c> pack's dances.</summary>\n")
        body.append(f"    public static class {pcls}\n    {{\n")
        for member, v in zip(members, values):
            body.append(xmldoc(_term_summary(v), indent=8))
            body.append(f"        public static readonly Dance {member} = {_term_ctor(v, 'DanceCode', dance_schema)};\n")
        body.append(f"\n        /// <summary>All {len(members)} {pack} dances, in catalog order.</summary>\n")
        body.append("        public static readonly IReadOnlyList<Dance> All =\n        [\n")
        body += [f"            {m},\n" for m in members]
        body.append("        ];\n    }\n\n")
        all_refs += [f"{pcls}.{m}" for m in members]
        n_terms += len(members)
    body.append("    /// <summary>Every dance across all packs (a dance shared by packs appears once per pack).</summary>\n")
    body.append("    public static readonly IReadOnlyList<Dance> All =\n    [\n")
    body += [f"        {r},\n" for r in all_refs]
    body.append("    ];\n\n")
    body.append("    /// <summary>The generic representative for a dance not in the catalog (null-object term).</summary>\n")
    body.append('    public static readonly Dance Generic = new(new DanceCode(""), "", [], "");\n\n')
    body.append("    /// <summary>Dance terms by stable id (first pack wins on a shared id).</summary>\n")
    body.append("    public static readonly IReadOnlyDictionary<string, Dance> ById =\n")
    body.append("        new Dictionary<string, Dance>(System.StringComparer.OrdinalIgnoreCase)\n        {\n")
    _seen_ids: set[str] = set()
    for pack in packs:
        pcls = pascal_case(pack)
        pvals = load(f"packs/{pack}/dances.yaml").get("values") or []
        pmem = _pack_members(pcls, pvals)
        for member, v in zip(pmem, pvals):
            key = v.get("id") or v.get("canonical") or ""
            if key in _seen_ids:
                continue
            _seen_ids.add(key)
            body.append(f"            [{cs_string(key)}] = {pcls}.{member},\n")
    body.append("        };\n\n")
    body.append("    /// <summary>Project a runtime dance term (e.g. a custom pack term) to a Dance.</summary>\n")
    body.append("    public static Dance FromTerm(VocabularyTerm t) =>\n")
    body.append('        new(new DanceCode(t.Display?.Code ?? ""), t.Canonical, t.Aliases, "");\n')
    body.append("}\n\n")
    body += _value_types("Dance", "DanceCode", "Dances", "dance")
    body += _view_type("Dance", "Dances", "dance")
    write_cs(tax / "Dance.g.cs", "".join(body))

    emit_vocab_views(dims, packs)
    emit_vocab_enums()
    emit_catalog_attribute_drift(dims, packs)
    return len(dims), len(packs), n_terms


# ── Full runtime baseline (compiled C#, replaces the embedded YAML) ──────────────────────────────
# Emit SocialDanceVocabulary.LoadDefault()'s entire data as compiled C# so the baseline ships as code,
# not as embedded resources read+parsed at runtime (AOT/trim-safe, no reflection, no resource lookup).
# YamlDotNet still ships for user OVERLAYS; only the baseline stops being parsed at load. The on-disk
# YAML stays the source of truth — BaselineVocabularyDriftTests re-parses it and gates this file.

_TERM_SKIP = {"id", "canonical", "aliases", "deprecated", "display"}  # carried as dedicated members, not Attributes


def _cs_scalar(v) -> str:
    """A YAML scalar as the runtime sees it: YamlDotNet's untyped reader yields every scalar as a
    string, so we emit one too (bools lower-cased to match; the loader never sees non-string scalars)."""
    if v is None:
        return "null"
    if isinstance(v, bool):
        return cs_string("true" if v else "false")
    return cs_string(str(v))


def _cs_attr(v) -> str:
    """A C# `object?` expression reproducing one attribute value exactly as the YAML loader yields it:
    scalars -> string, sequences -> object?[], mappings -> string-keyed maps (arbitrary nesting)."""
    if isinstance(v, dict):
        inner = ", ".join(f"[{cs_string(str(k))}] = {_cs_attr(val)}" for k, val in v.items())
        base = "new Dictionary<string, object?>(System.StringComparer.Ordinal)"
        return f"{base} {{ {inner} }}" if inner else base
    if isinstance(v, list):
        inner = ", ".join(_cs_attr(x) for x in v)
        return f"new object?[] {{ {inner} }}" if inner else "new object?[] { }"
    return _cs_scalar(v)


def _attrs_expr(v: dict) -> str:
    """The dimension-specific Attributes map for a term (every per-value key except the dedicated ones)."""
    base = "new Dictionary<string, object?>(System.StringComparer.Ordinal)"
    items = [(k, val) for k, val in v.items() if k not in _TERM_SKIP]
    if not items:
        return base
    inner = ", ".join(f"[{cs_string(str(k))}] = {_cs_attr(val)}" for k, val in items)
    return f"{base} {{ {inner} }}"


def _display_expr(v: dict) -> str:
    """The `TermDisplay` initializer (or `null`); empty fields collapse to null, as the loader does."""
    d = v.get("display")
    if not isinstance(d, dict):
        return "null"

    def fld(key: str) -> str:
        val = d.get(key)
        return cs_string(str(val)) if (val is not None and str(val) != "") else "null"

    args = [fld("code"), fld("emoji"), fld("icon"), fld("color")]
    label = fld("label")
    if label != "null":
        args.append(label)
    return "new TermDisplay(" + ", ".join(args) + ")"


def _baseline_term(v: dict, indent: int) -> str:
    """One `new VocabularyTerm(...)` initializer across three lines (id/canonical/aliases/deprecated,
    then Attributes, then Display)."""
    pad = " " * indent
    aliases = "[" + ", ".join(cs_string(a) for a in (v.get("aliases") or [])) + "]"
    deprecated = "true" if v.get("deprecated") else "false"
    return (f"{pad}new VocabularyTerm({cs_string(v.get('id') or '')}, {cs_string(v.get('canonical') or '')}, "
            f"{aliases}, {deprecated},\n"
            f"{pad}    {_attrs_expr(v)},\n"
            f"{pad}    {_display_expr(v)})")


REVIEW_STATUS_CS = {REVIEW_PENDING: "ReviewStatus.ExpertReviewPending"}


def _review_status_cs(token, source: str) -> str:
    """The C# ReviewStatus member for a `validation` token; an absent token is Unflagged."""
    if token is None:
        return "ReviewStatus.Unflagged"
    if token not in REVIEW_STATUS_CS:
        raise SystemExit(f"gen_vocab_docs: {source} validation {token!r} is not a ReviewStatus — "
                         f"add it to REVIEW_STATUS_CS and the enum")
    return REVIEW_STATUS_CS[token]


def _attribute_review_cs(doc: dict, source: str) -> str:
    """The Dimension constructor's attributeReview argument for a core file's `validation:` map, or ""."""
    review = doc.get("validation") or {}
    if not isinstance(review, dict):
        raise SystemExit(f"gen_vocab_docs: {source} validation must map attribute to status")
    if not review:
        return ""
    entries = ", ".join(f"[{cs_string(key)}] = {_review_status_cs(status, source)}" for key, status in review.items())
    return f",\n    new Dictionary<string, ReviewStatus>(System.StringComparer.Ordinal) {{ {entries} }}"


def _grouping_cs(attribute: str, groups: list, labels: dict, source: str) -> str:
    """A `new TermGrouping(...)` expression: the closed, ordered group keys of one term attribute, each with its label."""
    unlabelled = [g for g in groups if g not in labels]
    if unlabelled:
        raise SystemExit(f"gen_vocab_docs: {source} grouping on '{attribute}' has no label for {unlabelled}")
    entries = ", ".join(f"new({cs_string(str(g))}, {cs_string(labels[g])})" for g in groups)
    return f"new TermGrouping({cs_string(attribute)}, [{entries}])"


def _core_grouping_arg(doc: dict, source: str) -> str:
    """The Dimension constructor's grouping argument for the one axis a core file marks `grouping: true`, or ""."""
    marked = [axis for axis in doc.get("axes") or [] if axis.get("grouping")]
    if len(marked) > 1:
        raise SystemExit(f"gen_vocab_docs: {source} marks {len(marked)} axes as grouping; a dimension has one")
    if not marked:
        return ""
    axis = marked[0]
    return f",\n    grouping: {_grouping_cs(axis['name'], axis.get('values') or [], axis.get('labels') or {}, source)}"


def emit_baseline_vocabulary(manifest: dict) -> int:
    """Emit Generated/BaselineVocabulary.g.cs — the entire runtime vocabulary as compiled C#."""
    dims = manifest.get("core", {}).get("dimensions", [])
    packs = manifest.get("packs", [])
    tax = SRC / "SocialDanceTaggingStandard.Taxonomy" / "Generated"
    n_terms = 0

    body = [header("gen_vocab_docs.py", "data/vocabulary/"),
            "using System.Collections.Generic;\n\n",
            "namespace SocialDanceTaggingStandard.Taxonomy;\n\n",
            "/// <summary>The SDTS baseline vocabulary as compiled C# — the data behind\n",
            '/// <see cref="SocialDanceVocabulary.LoadDefault()"/>, so the baseline ships as code rather than\n',
            "/// embedded YAML (AOT/trim-safe, no resource lookup). Drift-gated against data/vocabulary/ by\n",
            "/// BaselineVocabularyDriftTests.</summary>\n",
            "internal static class BaselineVocabulary\n{\n",
            "    /// <summary>The baseline vocabulary (core music dimensions + dance packs).</summary>\n",
            "    public static SocialDanceVocabulary Build() => new(\n",
            f"        {cs_string(manifest.get('vocabulary_version') or '')},\n",
            "        new Dictionary<DimensionId, Dimension>\n        {\n"]
    for dim in dims:
        record = DIM_TYPES[dim][0]
        body.append(f"            [DimensionIds.{record}] = Build{record}(),\n")
    body.append("        },\n")
    body.append("        new Dictionary<PackId, DancePack>\n        {\n")
    for pack in packs:
        body.append(f"            [PackIds.{pascal_case(pack)}] = Build{pascal_case(pack)}(),\n")
    body.append("        });\n\n")

    for dim in dims:
        record = DIM_TYPES[dim][0]
        doc = load(f"core/{dim}.yaml")
        values = doc.get("values") or []
        n_terms += len(values)
        body.append(f"    private static Dimension Build{record}() => new(DimensionIds.{record}, \"core\",\n    [\n")
        body.append(",\n".join(_baseline_term(v, 8) for v in values))
        source = f"core/{dim}.yaml"
        body.append(f"\n    ]{_attribute_review_cs(doc, source)}{_core_grouping_arg(doc, source)});\n\n")

    for pack in packs:
        meta = load(f"packs/{pack}/pack.yaml")
        values = load(f"packs/{pack}/dances.yaml").get("values") or []
        n_terms += len(values)
        method = pascal_case(pack)
        families = "[" + ", ".join(cs_string(f) for f in (meta.get("families") or [])) + "]"
        body.append(f"    private static DancePack Build{method}() => new(PackIds.{method},\n")
        body.append(f"        {cs_string(meta.get('display_name') or '')}, {cs_string(meta.get('description') or '')},\n")
        body.append(f"        {families},\n")
        body.append('        new Dimension(SocialDanceVocabulary.DancesDimension, "pack",\n        [\n')
        body.append(",\n".join(_baseline_term(v, 12) for v in values))
        grouping = _grouping_cs("family", meta.get("families") or [], meta.get("family_labels") or {},
                                f"packs/{pack}/pack.yaml")
        body.append(f"\n        ],\n        grouping: {grouping}), "
                    f"{_review_status_cs(meta.get('validation'), f'packs/{pack}/pack.yaml')});\n\n")

    body.append("}\n")
    write_cs(tax / "BaselineVocabulary.g.cs", "".join(body))
    return n_terms


# ── Typed per-term attributes (Phase D) ──────────────────────────────────────────────────────────
# Each dimension's attributes become typed members on its catalog record, generated from the SAME YAML
# as the runtime bag and drift-gated against it (CatalogAttributeDrift). WHICH key maps to which
# type/enum is generator config (like DIM_TYPES); the VALUES come wholly from YAML.

# enum name -> ordered [(member, token)]. token is the YAML scalar, stringified; the set is asserted
# against the data at generation time (a new catalog value fails loudly instead of dropping silently).
# Enum'd only where the axis is a CLEAN, closed set in the actual data. Deliberately NOT enums (kept as
# typed string/int): genre `family` (21 inconsistent values — rnb vs r-and-b, electro vs electronic, misc),
# `meter` (carries the meta-value "varies"), `pulse` (data is purely 2/3/4 → int). The data, not the
# aspirational axis declaration, decides — see ENUM key dump in the Phase D work.
ENUM_DEFS = {
    "DanceRelevance": [("High", "high"), ("Medium", "medium"), ("Low", "low"), ("None", "none")],
    "Subdivision": [("Straight", "straight"), ("Triplet", "triplet"), ("BassFeel", "bass-feel"), ("Mixed", "mixed")],
    "Accent": [("OnBeat", "on-beat"), ("OffBeat", "off-beat"), ("Backbeat", "backbeat"), ("Clave", "clave"), ("None", "none")],
    "EnsembleSize": [("Solo", "solo"), ("Duo", "duo"), ("Trio", "trio"), ("Small", "small"), ("Medium", "medium"), ("Large", "large")],
    "InstrumentScope": [("Specific", "specific"), ("Generic", "generic")],
    "ThemeCategory": [("Seasonal", "seasonal"), ("Event", "event"), ("Context", "context")],
    "DanceTradition": [("ArgentineTango", "argentine-tango"), ("BallroomInternationalLatin", "ballroom-international-latin"),
                       ("BallroomInternationalStandard", "ballroom-international-standard"), ("BluesVernacular", "blues-vernacular"),
                       ("CarolinaShag", "carolina-shag"), ("EarlySocial", "early-social"), ("StreetLatin", "street-latin"),
                       ("SwingVernacular", "swing-vernacular"), ("WestCoastSwing", "west-coast-swing")],
}

# ordinal 1..N axis levels (a data/vocabulary/ axis declared with `range: [lo, hi]` instead of `values`):
# a typed int wrapper, its own type so an EnergyLevel is never assignable where a ValenceLevel is expected
# (guideline 1). Range is enforced at generation time — a value outside [lo, hi] fails loudly. The stored
# value is the raw 1..N number; 3×3 / 5×5 grids are non-normative CONSUMER discretisations, not in the type.
# name -> (lo, hi, yaml_axis, blurb).
LEVEL_DEFS = {
    "EnergyLevel": (1, 10, "energy", "arousal / activation (1 = lowest, 10 = highest)"),
    "ValenceLevel": (1, 10, "valence", "musical brightness (1 = darkest, 10 = brightest)"),
}

# dimension -> [(yaml_key, cs_property, kind)]; kind: str|int|double|list|bpm|audio|enum:<Name>|level:<Name>.
ATTR_SCHEMA = {
    "genre": [("parent", "Parent", "str"), ("family", "Family", "str"),
              ("era_start", "EraStart", "int"), ("era_end", "EraEnd", "int"),
              ("dance_relevance", "DanceRelevance", "enum:DanceRelevance"),
              ("typical_rhythmic_feels", "TypicalRhythmicFeels", "list"),
              ("typical_song_forms", "TypicalSongForms", "list"), ("bpm_range", "BpmRange", "bpm"),
              ("source", "Source", "str")],
    "mood": [("energy", "Energy", "level:EnergyLevel"), ("valence", "Valence", "level:ValenceLevel"),
             ("tags", "Tags", "list"), ("source", "Source", "str")],
    "rhythmic_feel": [("cluster", "Cluster", "str"), ("dsl_key", "DslKey", "str"), ("pulse", "Pulse", "int"),
                      ("subdivision", "Subdivision", "enum:Subdivision"), ("accent", "Accent", "enum:Accent"),
                      ("tier", "Tier", "int"), ("bpm_range", "BpmRange", "bpm"), ("audio", "Audio", "audio"),
                      ("source", "Source", "str")],
    "vocality": [("parent", "Parent", "str"), ("examples", "Examples", "list")],
    "instrumentation": [("size", "Size", "enum:EnsembleSize"), ("typical_genres", "TypicalGenres", "list")],
    "instrument": [("tier", "Scope", "enum:InstrumentScope"), ("parent", "Parent", "str"),
                       ("typical_examples", "TypicalExamples", "list"),
                       ("lead_capable", "LeadCapable", "bool")],
    "recording_context": [("musicbrainz_type", "MusicbrainzType", "str"), ("examples", "Examples", "list")],
    "recording_quality_era": [("era_start", "EraStart", "int"), ("era_end", "EraEnd", "int"),
                              ("technology", "Technology", "str"), ("frequency_range", "FrequencyRange", "str"),
                              ("character", "Character", "str")],
    "theme": [("category", "Category", "enum:ThemeCategory")],
    "song_form": [("dsl_key", "DslKey", "str"), ("bars", "Bars", "int"), ("pattern", "Pattern", "str"),
                  ("meter", "Meter", "str"), ("tier", "Tier", "int"),
                  ("typical_genres", "TypicalGenres", "list"), ("typical_share", "TypicalShare", "double")],
    "credit_role": [],  # flat SDTS-authored TIPL role vocabulary — no typed attributes
    "routine": [("typical_dances", "TypicalDances", "list")],  # no tempo: the tagged recording carries its own
    "dances": [("tradition", "Tradition", "enum:DanceTradition"), ("family", "Family", "str"),
               ("parent", "Parent", "str"),
               (("tempo_min", "tempo_max"), "Tempo", "tempo"),  # one tempo band, like genre/feel BpmRange
               ("typical_genres", "TypicalGenres", "list"), ("meter", "Meter", "str"),
               ("disambiguation", "Disambiguation", "str"), ("tempo_note", "TempoNote", "str"),
               ("replaced_by", "ReplacedBy", "str")],
}

_SCALAR_CS = {"str": "string?", "int": "int?", "double": "double?", "bool": "bool?",
              "list": "IReadOnlyList<string>", "bpm": "BpmRange?", "audio": "TermAudio?", "tempo": "BpmRange?"}


def _enum_member(enum: str, token) -> str:
    tok = str(token)
    for member, t in ENUM_DEFS[enum]:
        if t == tok:
            return member
    raise SystemExit(f"gen_vocab_docs: value {token!r} not in enum {enum} — add it to ENUM_DEFS")


def _attr_cs_type(kind: str) -> str:
    if kind.startswith("enum:"):
        return f"{kind[5:]}?"
    if kind.startswith("level:"):
        return f"{kind[6:]}?"
    return _SCALAR_CS[kind]


def _attr_value_expr(kind: str, v) -> str:
    if kind.startswith("enum:"):
        return f"{kind[5:]}.{_enum_member(kind[5:], v)}"
    if kind.startswith("level:"):
        name = kind[6:]
        iv, (lo, hi, *_rest) = int(v), LEVEL_DEFS[name]
        if not lo <= iv <= hi:
            raise SystemExit(f"gen_vocab_docs: {name} value {v!r} out of range {lo}..{hi}")
        return f"new {name}({iv})"
    if kind == "bool":
        return "true" if v else "false"
    if kind == "int":
        return str(int(v))
    if kind == "double":
        s = str(float(v))
        if "e" in s or "E" in s:  # the drift gate compares ToString(Invariant); an exponent would diverge
            raise SystemExit(f"gen_vocab_docs: double {v!r} emits exponent notation {s!r} — add a fixed-format path")
        return s
    if kind == "list":
        return "[" + ", ".join(cs_string(str(x)) for x in v) + "]"
    if kind == "bpm":
        return f"new BpmRange({int(v[0])}, {int(v[1])})"
    if kind == "audio":
        def fld(k):
            val = v.get(k)
            return cs_string(str(val)) if (val is not None and str(val) != "") else "null"
        return f"new TermAudio({fld('clip')}, {fld('youtube')}, {fld('youtube_label')})"
    return cs_string(str(v))


def _attr_initializer(v: dict, schema: list) -> str:
    """The `{ Prop = expr, … }` object-initializer for the typed attributes a value actually carries."""
    parts = []
    for key, prop, kind in schema:
        if kind == "tempo":  # a paired (min, max) -> one BpmRange
            lo, hi = v.get(key[0]), v.get(key[1])
            if lo is not None and hi is not None:
                parts.append(f"{prop} = new BpmRange({int(lo)}, {int(hi)})")
            continue
        val = v.get(key)
        if val is None or (kind == "list" and not val):
            continue
        parts.append(f"{prop} = {_attr_value_expr(kind, val)}")
    return (" { " + ", ".join(parts) + " }") if parts else ""


def _attr_members(schema: list) -> list[str]:
    """The init-only typed attribute members for a dimension's catalog record."""
    out = []
    for key, prop, kind in schema:
        t = _attr_cs_type(kind)
        default = " = [];" if kind == "list" else ""
        key_doc = "/".join(key) if isinstance(key, tuple) else key
        out.append(f"    /// <summary>The <c>{key_doc}</c> attribute (data/vocabulary/), or its default.</summary>\n")
        out.append(f"    public {t} {prop} {{ get; init; }}{default}\n")
    return out


def emit_vocab_enums() -> int:
    """Generate Generated/VocabularyEnums.g.cs: the closed categorical axes as enums, each with a
    ToToken()/Parse() round-trip to its YAML token (the enum MEMBERS come from ENUM_DEFS, validated
    against the data; the per-term value still comes wholly from YAML)."""
    tax = SRC / "SocialDanceTaggingStandard.Taxonomy" / "Generated"
    body = [header("gen_vocab_docs.py", "data/vocabulary/"),
            "namespace SocialDanceTaggingStandard.Taxonomy;\n\n"]
    for enum, members in ENUM_DEFS.items():
        body.append(f"/// <summary>The closed <c>{enum}</c> axis (data/vocabulary/); see {enum}Tokens for the\n")
        body.append("/// YAML token round-trip. Its own type so axis values never mix (CLAUDE.md guideline 1).</summary>\n")
        body.append(f"public enum {enum}\n{{\n")
        body += [f"    /// <summary><c>{tok}</c></summary>\n    {m},\n" for m, tok in members]
        body.append("}\n\n")
        body.append(f"/// <summary>YAML-token round-trip for <see cref=\"{enum}\"/>.</summary>\n")
        body.append(f"public static class {enum}Tokens\n{{\n")
        body.append(f"    /// <summary>The canonical YAML token for an <see cref=\"{enum}\"/> value.</summary>\n")
        body.append(f"    public static string ToToken(this {enum} value) => value switch\n    {{\n")
        body += [f"        {enum}.{m} => {cs_string(tok)},\n" for m, tok in members]
        body.append("        _ => throw new System.ArgumentOutOfRangeException(nameof(value)),\n    };\n\n")
        body.append(f"    /// <summary>Parse a YAML token to its <see cref=\"{enum}\"/>, or null if unknown.</summary>\n")
        body.append(f"    public static {enum}? Parse(string? token) => token switch\n    {{\n")
        body += [f"        {cs_string(tok)} => {enum}.{m},\n" for m, tok in members]
        body.append("        _ => null,\n    };\n}\n\n")
    for name, (lo, hi, axis, blurb) in LEVEL_DEFS.items():
        body.append(f"/// <summary>An ordinal <c>{axis}</c> level {lo}–{hi} (data/vocabulary/) — {blurb}. Its own\n")
        body.append(f"/// type so a {name} is never assignable where another axis level is expected (CLAUDE.md guideline 1).</summary>\n")
        body.append(f"public readonly record struct {name}(int Value) : System.IComparable<{name}>\n{{\n")
        body.append("    /// <summary>Ordinal comparison by <see cref=\"Value\"/>.</summary>\n")
        body.append(f"    public int CompareTo({name} other) => Value.CompareTo(other.Value);\n")
        body.append("    /// <summary>The raw ordinal as a string, e.g. <c>\"8\"</c>.</summary>\n")
        body.append("    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);\n")
        body.append("}\n\n")
    write_cs(tax / "VocabularyEnums.g.cs", "".join(body))
    return len(ENUM_DEFS) + len(LEVEL_DEFS)


def _drift_checks(schema: list) -> list[str]:
    """The per-attribute drift comparisons for one dimension's Check method body."""
    out = []
    for key, prop, kind in schema:
        if kind == "tempo":  # one BpmRange projected back to the two bag scalars
            out.append(f"        foreach (var m in Scalar(t, {cs_string(key[0])}, c.{prop}?.Min.ToString())) yield return m;\n")
            out.append(f"        foreach (var m in Scalar(t, {cs_string(key[1])}, c.{prop}?.Max.ToString())) yield return m;\n")
            continue
        k = cs_string(key)
        if kind.startswith("enum:"):
            out.append(f"        foreach (var m in Scalar(t, {k}, c.{prop}?.ToToken())) yield return m;\n")
        elif kind.startswith("level:"):
            out.append(f"        foreach (var m in Scalar(t, {k}, c.{prop}?.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))) yield return m;\n")
        elif kind == "int":
            out.append(f"        foreach (var m in Scalar(t, {k}, c.{prop}?.ToString())) yield return m;\n")
        elif kind == "double":
            out.append(f"        foreach (var m in Scalar(t, {k}, c.{prop}?.ToString(System.Globalization.CultureInfo.InvariantCulture))) yield return m;\n")
        elif kind == "str":
            out.append(f"        foreach (var m in Scalar(t, {k}, c.{prop})) yield return m;\n")
        elif kind == "bool":  # runtime bag stores the YAML bool lower-cased; match it (bool.ToString() is "True")
            out.append(f"        foreach (var m in Scalar(t, {k}, c.{prop} is null ? null : (c.{prop}.Value ? \"true\" : \"false\"))) yield return m;\n")
        elif kind == "list":
            out.append(f"        foreach (var m in List(t, {k}, c.{prop})) yield return m;\n")
        elif kind == "bpm":
            out.append(f"        foreach (var m in Bpm(t, {k}, c.{prop})) yield return m;\n")
        elif kind == "audio":
            out.append(f"        foreach (var m in Audio(t, {k}, c.{prop})) yield return m;\n")
    known = []  # description stays in the bag, typed as Description; a paired tempo covers both bag keys
    for key, _prop, _kind in schema:
        known.extend(key if isinstance(key, tuple) else [key])
    known.append("description")
    out.append("        foreach (var m in Unexpected(t, new HashSet<string> { "
               + ", ".join(cs_string(k) for k in known) + " })) yield return m;\n")
    return out


def emit_catalog_attribute_drift(dims: list, packs: list) -> None:
    """Generate Generated/CatalogAttributeDrift.g.cs: per-dimension Check(record, runtimeTerm) that
    projects the typed catalog attributes back to their YAML token and yields any mismatch against the
    runtime VocabularyTerm.Attributes bag. The drift-gate test asserts every Check yields nothing."""
    tax = SRC / "SocialDanceTaggingStandard.Taxonomy" / "Generated"
    body = [header("gen_vocab_docs.py", "data/vocabulary/"),
            "using System;\nusing System.Collections;\nusing System.Collections.Generic;\nusing System.Linq;\n\n",
            "namespace SocialDanceTaggingStandard.Taxonomy;\n\n",
            "/// <summary>Drift-gate helper: projects each typed catalog attribute back to its YAML token and\n",
            "/// compares it to the runtime <see cref=\"VocabularyTerm.Attributes\"/> bag. A generator bug in the\n",
            "/// typed catalog surfaces as a non-empty mismatch list. Consumed by the catalog drift-gate test.</summary>\n",
            "public static class CatalogAttributeDrift\n{\n"]
    # shared comparison helpers (generated boilerplate; the runtime bag is string scalars + object?[] + maps)
    body.append("""    private static IEnumerable<string> Scalar(VocabularyTerm t, string key, string? expected)
    {
        // A present-but-null bag value and an absent key both mean "no value" — Convert.ToString(null)
        // would otherwise yield "" and spuriously differ from the catalog's null.
        var actual = t.Attributes.TryGetValue(key, out var v) && v is not null ? Convert.ToString(v) : null;
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
            yield return $"{t.Id}.{key}: catalog='{expected ?? "<null>"}' runtime='{actual ?? "<null>"}'";
    }

    private static IEnumerable<string> List(VocabularyTerm t, string key, IReadOnlyList<string> expected)
    {
        var actual = t.Attributes.TryGetValue(key, out var v) && v is IEnumerable e and not string
            ? e.Cast<object?>().Select(x => Convert.ToString(x) ?? "").ToList()
            : new List<string>();
        if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
            yield return $"{t.Id}.{key}: catalog=[{string.Join(",", expected)}] runtime=[{string.Join(",", actual)}]";
    }

    private static IEnumerable<string> Bpm(VocabularyTerm t, string key, BpmRange? expected)
    {
        var exp = expected is { } b ? $"{b.Min},{b.Max}" : null;
        var act = t.Attributes.TryGetValue(key, out var v) && v is IEnumerable e and not string
            ? string.Join(",", e.Cast<object?>().Select(x => Convert.ToString(x)))
            : null;
        if (!string.Equals(exp, act, StringComparison.Ordinal))
            yield return $"{t.Id}.{key}: catalog='{exp ?? "<null>"}' runtime='{act ?? "<null>"}'";
    }

    private static IEnumerable<string> Audio(VocabularyTerm t, string key, TermAudio? expected)
    {
        var bag = t.Attributes.TryGetValue(key, out var v) && v is IDictionary d ? d : null;
        string? Get(string k)
        {
            if (bag is null) return null;
            foreach (var dk in bag.Keys)
                if (Convert.ToString(dk) == k) return Convert.ToString(bag[dk]);
            return null;
        }
        if (bag is null != (expected is null)
            || (expected is { } a && (Get("clip") != a.Clip || Get("youtube") != a.Youtube || Get("youtube_label") != a.YoutubeLabel)))
            yield return $"{t.Id}.{key}: audio mismatch";
    }

    // Completeness: every runtime bag attribute must be represented by a typed catalog member, so a new
    // YAML attribute can't be silently dropped from the typed surface.
    private static IEnumerable<string> Unexpected(VocabularyTerm t, HashSet<string> known)
    {
        foreach (var key in t.Attributes.Keys)
            if (!known.Contains(key))
                yield return $"{t.Id}.{key}: runtime attribute has no typed catalog member (add it to ATTR_SCHEMA)";
    }

""")
    for dim in dims:
        record = DIM_TYPES[dim][0]
        body.append(f"    /// <summary>Drift-check one {record} catalog term against its runtime term.</summary>\n")
        body.append(f"    public static IEnumerable<string> Check(this {record} c, VocabularyTerm t)\n    {{\n")
        body += _drift_checks(ATTR_SCHEMA.get(dim, []))
        body.append("    }\n\n")
    body.append("    /// <summary>Drift-check one Dance catalog term against its runtime term.</summary>\n")
    body.append("    public static IEnumerable<string> Check(this Dance c, VocabularyTerm t)\n    {\n")
    body += _drift_checks(ATTR_SCHEMA.get("dances", []))
    body.append("    }\n")
    body.append("}\n")
    write_cs(tax / "CatalogAttributeDrift.g.cs", "".join(body))


def main() -> None:
    manifest = load("manifest.yaml")
    if OUT.exists():
        shutil.rmtree(OUT)
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT / "README.md").write_text(index_page(manifest), encoding="utf-8", newline="\n")
    # Dances: a landing page (dances.md, the "Dances" nav section index) + one page per pack under dances/.
    (OUT / "dances.md").write_text(dances_index_page(manifest), encoding="utf-8", newline="\n")
    (OUT / "dances").mkdir(exist_ok=True)
    for pack_name in manifest.get("packs", []):
        (OUT / "dances" / f"{pack_name}.md").write_text(dance_pack_page(pack_name), encoding="utf-8", newline="\n")
    for dim in manifest.get("core", {}).get("dimensions", []):
        (OUT / f"{dim}.md").write_text(dimension_page(dim, load(f"core/{dim}.yaml")), encoding="utf-8", newline="\n")
    nd, npk, nc = emit_vocab_ids(manifest)
    print(f"Wrote typed vocab ids: DimensionIds ({nd}), PackIds ({npk}), term catalogs ({nc} terms)")
    nb = emit_baseline_vocabulary(manifest)
    print(f"Wrote compiled baseline vocabulary: BaselineVocabulary.g.cs ({nb} terms)")
    print(f"Wrote vocabulary browser to {OUT.relative_to(ROOT)} "
          f"({len(manifest.get('packs', []))} packs, "
          f"{len(manifest.get('core', {}).get('dimensions', []))} dimensions)")


if __name__ == "__main__":
    main()
