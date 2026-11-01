#!/usr/bin/env python3
"""Generate the per-field SDTS docs + grouped index from data/fields.yaml.

Single source of truth = data/fields.yaml (field registry) + data/standard-frames
(inside the same file) + data/field-invalid-examples.yaml (doc-only counter-examples).
The markdown under docs/standard/fields/ is a DERIVED ARTIFACT — do not hand-edit;
edit the data and re-run:

    python scripts/gen_field_docs.py

Layout: docs are split by ORIGIN into two folders the user asked for —
    docs/standard/fields/id3/    ID3-standard frames + the SDTS fields that bind to one
    docs/standard/fields/sdts/   SDTS-defined fields (TXXX:SDTS_* / reused Songs-DB_* slots)
plus docs/standard/fields/README.md (grouped catalog index).

Rationale: keeps human docs in sync with the registry with zero drift
(the repo rule is "as few redundancies as possible").
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

from _field_dimensions import backing_dimension

ROOT = pathlib.Path(__file__).resolve().parent.parent
FIELDS_YAML = ROOT / "data" / "fields.yaml"
INVALID_YAML = ROOT / "data" / "field-invalid-examples.yaml"
OUT_DIR = ROOT / "docs" / "standard" / "fields"

# Derived fields the user marked "won't be saved" — listed in the index, no detail doc.
SKIP_DOCS = set()

# The SDTS fields whose canonical home IS a real standard ID3 frame (→ id3/ folder).
# Everything else in `fields` is SDTS-defined (TXXX:SDTS_* / reused slots) → sdts/ folder.
ID3_STANDARD_FIELDS = {"genre", "mood", "bpm", "rating", "lyric_language"}

CATEGORIES = [
    ("taxonomy-core-a", "Taxonomy — core (A)"),
    ("taxonomy-core-b", "Taxonomy — core (B)"),
    ("measured-confirmation", "Measured & confirmation"),
    ("proposed-tempo-structure", "Proposed — tempo & structure"),
    ("proposed-dance-semantic", "Proposed — dance semantics"),
    ("proposed-content-provenance", "Proposed — content & provenance"),
]
CAT_DISPLAY = dict(CATEGORIES)

# Reader-facing grouping: by the musical/dance QUESTION a field answers, not by storage origin.
# This is the index's spine — a DJ/dancer scans by question; the frame is a per-field detail.
QUESTION_GROUPS = [
    ("Which dance, and when", ["dance", "theme", "routine"]),
    ("Tempo & groove", ["bpm", "rhythmic_feel", "meter", "tempo_stability"]),
    ("Mood & energy", ["mood", "energy", "valence", "danceability"]),
    ("Sound & ensemble", ["genre", "vocality", "instrumentation", "instrument_led"]),
    ("Recording character",
     ["recording_context", "recording_quality_era", "recording_origin", "original_title", "lyric_language",
      "artist_country"]),
    ("Structure & cueing",
     ["song_form", "intro_duration", "outro_duration", "ending_type",
      "playback_start", "playback_stop", "fade_in_duration", "fade_out_duration", "regions"]),
    ("Workflow & trust", ["rating", "confirmation_gates", "sdts_version"]),
]
QGROUP_OF = {fid: label for label, ids in QUESTION_GROUPS for fid in ids}

RELATED = {
    "dance": ["theme", "routine"],
    "genre": ["mood", "rhythmic_feel"],
    "mood": ["energy", "valence", "genre"],
    "vocality": ["lyric_language", "instrument_led"],
    "rhythmic_feel": ["genre", "song_form", "tempo_stability"],
    "instrumentation": ["instrument_led", "vocality"],
    "instrument_led": ["instrumentation", "vocality"],
    "recording_context": ["recording_quality_era", "recording_origin"],
    "theme": ["dance", "mood"],
    "recording_quality_era": ["recording_origin", "recording_context"],
    "song_form": ["meter", "rhythmic_feel"],
    "bpm": ["tempo_stability", "meter"],
    "rating": ["danceability", "energy"],
    "confirmation_gates": ["sdts_version"],
    "sdts_version": ["confirmation_gates"],
    "energy": ["valence", "danceability", "mood"],
    "valence": ["energy", "mood", "rhythmic_feel"],
    "tempo_stability": ["bpm", "song_form"],
    "meter": ["song_form", "rhythmic_feel"],
    "intro_duration": ["outro_duration", "ending_type", "playback_start"],
    "outro_duration": ["intro_duration", "ending_type", "playback_stop"],
    "ending_type": ["outro_duration", "fade_out_duration"],
    "playback_start": ["playback_stop", "fade_in_duration", "intro_duration"],
    "playback_stop": ["playback_start", "fade_out_duration", "outro_duration"],
    "fade_in_duration": ["fade_out_duration", "playback_start", "intro_duration"],
    "fade_out_duration": ["fade_in_duration", "playback_stop", "ending_type"],
    "regions": ["playback_start", "playback_stop", "intro_duration"],
    "routine": ["dance", "song_form"],
    "danceability": ["energy", "rating"],
    "lyric_language": ["vocality"],
    "recording_origin": ["recording_quality_era", "recording_context", "artist_country"],
    "original_title": ["recording_origin", "recording_context"],
    "artist_country": ["recording_origin"],
}

# Relative path from a doc in docs/standard/fields/<folder>/ back to the repo.
NAV = {
    "readme": "../README.md",
    "spec": "../../spec.md",
    "registry": "https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml",
    "sync": "../../../sync/tool-comparison.md",
}

def vocab_ref(fid: str):
    """The vocabulary page listing a field's values (its backing dimension's page), or None."""
    dimension = backing_dimension(fid)
    return f"../../../model/vocabulary/{dimension}.md" if dimension else None


def cell(v) -> str:
    return str(v or "").replace("|", "\\|").strip()


def slug(label: str) -> str:
    """Stable anchor that resolves identically on GitHub and the mkdocs site (`&` -> `and`)."""
    s = re.sub(r"[^\w\s-]", "", label.lower().replace("&", "and"))
    return re.sub(r"[-\s]+", "-", s).strip("-")


def folder_for(fid: str) -> str:
    return "id3" if fid in ID3_STANDARD_FIELDS else "sdts"


def clean_frame(s) -> str:
    s = str(s or "")
    for sep in (" (", " —", " →", " --"):
        i = s.find(sep)
        if i > 0:
            s = s[:i]
    return s.strip()


def first_sentence(s) -> str:
    s = str(s or "").strip()
    i = s.find(". ")
    return s[: i + 1] if i > 0 else s


def canonical_tag(f: dict) -> str:
    if f["id"] in ID3_STANDARD_FIELDS:
        return clean_frame(f.get("id3_current", ""))
    return clean_frame(f.get("id3_proposed", ""))


def frame_slug(frame: str) -> str:
    return frame.split()[0].lower().strip("()") if frame else "frame"


def invalid_block(items: list) -> list:
    if not items:
        return []
    return ["", "**Invalid (don't do this).**"] + [f"- {it}" for it in items]


def field_doc(f: dict, by_id: dict, invalid: dict) -> str:
    fid = f["id"]
    name = f.get("display_name", fid)
    cat = QGROUP_OF.get(fid, CAT_DISPLAY.get(f.get("group", ""), f.get("group", "")))
    is_std = fid in ID3_STANDARD_FIELDS
    origin = "ID3 standard" if is_std else "SDTS-defined"
    my_folder = folder_for(fid)

    if is_std:
        tag = f"`{clean_frame(f.get('id3_current',''))}` (ID3 standard frame — canonical home)"
    else:
        tag = (f"`{clean_frame(f.get('id3_proposed',''))}` — same `SDTS_` name "
               "across ID3 TXXX · Vorbis · MP4")

    legacy = f.get("id3_current", "")
    legacy_note = ""
    if not is_std and legacy and not legacy.strip().lower().startswith("n/a"):
        legacy_note = (f"Legacy on-disk mirror `{clean_frame(legacy)}` — kept for backward "
                       f"compatibility (see [Part 2 sync]({NAV['sync']})); NOT the canonical home.")

    rel = []
    for r in RELATED.get(fid, []):
        if r in SKIP_DOCS or r not in by_id:
            continue
        rf = folder_for(r)
        path = f"{r}.md" if rf == my_folder else f"../{rf}/{r}.md"
        rel.append(f"[{by_id[r].get('display_name', r)}]({path})")
    rel_md = " · ".join(rel)

    valid = " · ".join(f"`{e}`" for e in f.get("examples", []))
    card = cell(f.get("cardinality", ""))
    delim = cell(f.get("delimiter", ""))
    type_cell = f"`{cell(f.get('type', f.get('data_type','')))}` · {card}"
    if card == "multi" and delim and delim not in ("—", "-"):
        type_cell += f" · delimiter `{delim}`"

    lines = [
        f"# {name} &nbsp;·&nbsp; `{fid}`",
        "",
        f"> {f.get('dj_use','').strip()}",
        "",
        (f"[← Field catalog]({NAV['readme']}) · [Spec]({NAV['spec']}) · "
         f"[Registry]({NAV['registry']}) · [Interop]({NAV['sync']})"
         + (f" · [Values]({vocab_ref(fid)})" if vocab_ref(fid) else "")),
        "",
        "| | |",
        "|---|---|",
        f"| **Type** | {type_cell} |",
        f"| **Values** | {cell(f.get('value_source',''))} |",
        f"| **Tag** | {tag} |",
        f"| **Status** | {cell(f.get('status',''))} · {cat} · {origin} |",
        "",
        f"{f.get('description','').strip()}",
        "",
        f"**Values.** {f.get('allowed_values','').strip()}",
        "",
        ("**Examples.** ✓ " + valid) if valid else "**Examples.** —",
    ]
    invs = invalid.get(fid, [])
    if invs:
        lines += ["", "**Avoid.**"] + [f"- {it}" for it in invs]
    if legacy_note:
        lines += ["", f"**Notes.** {legacy_note}"]
    if rel_md:
        lines += ["", f"**Related.** {rel_md}"]
    lines += [
        "",
        "---",
        f"_Generated from [`data/fields.yaml`]({NAV['registry']}) by "
        "`scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._",
        "",
    ]
    return "\n".join(lines)


def frame_doc(sf: dict, invalid: dict) -> str:
    frame = sf["frame"]
    name = sf.get("field", frame)
    examples = " · ".join(f"`{e}`" for e in sf.get("examples", []))
    lines = [
        f"# {name} &nbsp;·&nbsp; `{frame}`",
        "",
        "`ID3 standard frame` · Standard music frames · origin: **ID3 standard**",
        "",
        f"[← Field catalog]({NAV['readme']}) · [Spec overview]({NAV['spec']}) · "
        f"[Registry (fields.yaml)]({NAV['registry']}) · [Cross-tool interop]({NAV['sync']})",
        "",
        f"> {sf.get('when_to_use','').strip().split('. ')[0]}.",
        "",
        "| | |",
        "|---|---|",
        f"| **Frame** | `{cell(frame)}` |",
        f"| **Concept** | {cell(name)} |",
        "| **Origin** | ID3 standard (pass-through; SDTS does not redefine it) |",
        "",
        f"{sf.get('description','').strip()}",
        "",
        f"**When to use.** {sf.get('when_to_use','').strip()}",
        "",
        f"**Examples (valid).** {examples}",
    ]
    lines += invalid_block(invalid.get(frame_slug(frame), []) or invalid.get(frame, []))
    lines += [
        "",
        "---",
        f"_Generated from [`data/fields.yaml`]({NAV['registry']}) by "
        "`scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._",
        "",
    ]
    return "\n".join(lines)


def index_doc(fields: list, std_frames: list, by_id: dict) -> str:
    out = [
        "# Field reference",
        "",
        "What a track can be tagged with, **grouped by the question each field answers** on the floor. "
        "Each row links to a one-page detail (meaning, values, examples, binding). The actual value "
        "sets live in the [vocabulary](../../model/vocabulary/README.md); this is about the fields.",
        "",
        "Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) — the single source of truth. "
        "Conceptual spec: [`../spec.md`](../spec.md). Cross-tool interop: "
        "[`../../sync/tool-comparison.md`](../../sync/tool-comparison.md).",
        "",
    ]

    def field_row(fid: str) -> str:
        f = by_id[fid]
        name = f.get("display_name", fid)
        summary = cell(first_sentence(f.get("dj_use", "") or f.get("description", "")))
        status = cell(f.get("status", ""))
        tag = "_derived — not stored_" if fid in SKIP_DOCS else f"`{cell(canonical_tag(f))}`"
        link = name if fid in SKIP_DOCS else f"[{name}]({folder_for(fid)}/{fid}.md)"
        flag = " · _proposed_" if status == "proposed" else ""
        return f"| {link} `{fid}` | {tag} | {summary}{flag} |"

    covered = set()
    for label, ids in QUESTION_GROUPS:
        present = [fid for fid in ids if fid in by_id]
        if not present:
            continue
        out += [f'<a id="{slug(label)}"></a>', "", f"## {label}", "",
                "| Field | Tag | What it does |", "|---|---|---|"]
        out += [field_row(fid) for fid in present]
        out.append("")
        covered.update(present)

    leftover = [f["id"] for f in fields if f["id"] not in covered]
    if leftover:
        out += ["## More", "", "| Field | Tag | What it does |", "|---|---|---|"]
        out += [field_row(fid) for fid in leftover]
        out.append("")
        print(f"WARN: fields not in a QUESTION_GROUP (shown under 'More'): {', '.join(leftover)}")

    out += [
        "## Identity, origin & credit frames (ID3 pass-through)",
        "",
        "Universal frames SDTS does **not** redefine — title, artist, album, comment, grouping, the "
        "**Original-\\*** frames (`TOPE`/`TOAL`/`TDOR`) for covers & reissues, and the paired credit-list "
        "frames (`TIPL`/`TMCL`).",
        "",
        "| Frame | Concept | Doc |",
        "|---|---|---|",
    ]
    for sf in std_frames:
        out.append(f"| `{cell(sf['frame'])}` | {cell(sf.get('field',''))} | [id3/{frame_slug(sf['frame'])}.md](id3/{frame_slug(sf['frame'])}.md) |")
    out.append("")
    return "\n".join(out)


def main() -> None:
    data = yaml.safe_load(FIELDS_YAML.read_text(encoding="utf-8"))
    fields = data["fields"]
    std_frames = data.get("standard_frames", [])
    by_id = {f["id"]: f for f in fields}
    invalid = {}
    if INVALID_YAML.exists():
        invalid = yaml.safe_load(INVALID_YAML.read_text(encoding="utf-8")) or {}

    # Clean regenerate (everything under fields/ is generated).
    if OUT_DIR.exists():
        shutil.rmtree(OUT_DIR)
    (OUT_DIR / "id3").mkdir(parents=True, exist_ok=True)
    (OUT_DIR / "sdts").mkdir(parents=True, exist_ok=True)

    written = 0
    for f in fields:
        if f["id"] in SKIP_DOCS:
            continue
        (OUT_DIR / folder_for(f["id"]) / f"{f['id']}.md").write_text(field_doc(f, by_id, invalid), encoding="utf-8", newline="\n")
        written += 1
    for sf in std_frames:
        (OUT_DIR / "id3" / f"{frame_slug(sf['frame'])}.md").write_text(frame_doc(sf, invalid), encoding="utf-8", newline="\n")

    (OUT_DIR / "README.md").write_text(index_doc(fields, std_frames, by_id), encoding="utf-8", newline="\n")
    print(f"Wrote {written} field docs + {len(std_frames)} standard-frame docs + README.md")
    print(f"  id3/: {len(list((OUT_DIR/'id3').glob('*.md')))}  sdts/: {len(list((OUT_DIR/'sdts').glob('*.md')))}")
    print(f"Skipped (derived, no doc): {', '.join(sorted(SKIP_DOCS))}")
    missing = [f['id'] for f in fields if f['id'] not in SKIP_DOCS and f['id'] not in invalid]
    if missing:
        print(f"WARN: no invalid-examples for: {', '.join(missing)}")


if __name__ == "__main__":
    main()
