#!/usr/bin/env python3
"""Validate the SDTS data files against their JSON Schemas + a few cross-checks.

Also gates data/vocabulary/, which has no schema: every canonical, alias, and display code must
resolve to exactly one term in its dimension (see resolution_token_errors), and every typical_genres,
typical_song_forms, and typical_rhythmic_feels entry must name a canonical of the dimension it points at
(see typical_reference_errors), and every review flag must be one the docs can render (see
validation_marker_errors), and every parent must name a term of its dimension (see parent_errors), and
every dance colour must be its own and readable on the dark theme (see dance_colour_errors), and every
rhythmic feel must be ranked by a tier or a scene (see feel_rank_errors).

Runs locally and in CI. Usage: python scripts/validate_data.py
Requires: pyyaml, jsonschema  (pip install pyyaml jsonschema)
"""
import json
import math
import re
import sys
from pathlib import Path
from typing import NamedTuple

import yaml
from jsonschema import Draft202012Validator

ROOT = Path(__file__).resolve().parent.parent
VOCAB = ROOT / "data" / "vocabulary"


def load_yaml(path):
    with open(path, encoding="utf-8") as fh:
        return yaml.safe_load(fh)


def load_json(path):
    with open(path, encoding="utf-8") as fh:
        return json.load(fh)


class Claim(NamedTuple):
    """One token a term authors for itself, and where it was authored."""
    source: str
    term_id: str
    kind: str
    token: str


def load_vocabulary_terms():
    """Returns ([(source, dimension id, term)], {source: its top-level validation value}, errors) over every
    baseline dimension and pack.yaml.

    Pack dances all report the dimension id `dances`: SocialDanceVocabulary.Dances() merges the
    enabled packs into one dimension, so a token two packs both claim collides there too.
    """
    manifest = load_yaml(VOCAB / "manifest.yaml")
    sources = [(f"core/{dim}.yaml", dim) for dim in manifest.get("core", {}).get("dimensions", [])]
    sources += [(f"packs/{pack}/dances.yaml", "dances") for pack in manifest.get("packs", [])]

    terms, markers, errors = [], {}, []
    for rel, dimension in sources:
        path = VOCAB / rel
        if not path.exists():
            errors.append(f"vocabulary/manifest.yaml names '{rel}', which does not exist")
            continue
        doc = load_yaml(path) or {}
        terms += [(rel, dimension, term) for term in doc.get("values") or []]
        if "validation" in doc:
            markers[rel] = doc["validation"]
    for pack in manifest.get("packs", []):
        rel = f"packs/{pack}/pack.yaml"
        if (VOCAB / rel).exists() and "validation" in (meta := load_yaml(VOCAB / rel) or {}):
            markers[rel] = meta["validation"]
    return terms, markers, errors


def resolution_token_errors(terms):
    """Report every token a dimension does not map to exactly one term.

    Dimension (dotnet/src/SocialDanceTaggingStandard.Taxonomy/Dimension.cs) resolves canonicals, aliases, and display
    codes through ONE case-insensitive lookup, and rejects a token two terms both claim. This holds
    the same line at the source and additionally rejects what the runtime cannot see: a term
    claiming one token twice, which resolves fine and is dead data. The single allowed repeat is a
    display code equal to its own term's canonical (`Pop` / `POP`) — the term needs both, and the
    lookup keeps it reachable either way.
    """
    claims = {}
    for source, dimension, term in terms:
        authored = [(term.get("canonical"), "canonical")]
        authored += [(alias, "alias") for alias in term.get("aliases") or []]
        authored += [((term.get("display") or {}).get("code"), "display code")]
        for token, kind in authored:
            if not token:
                continue
            claim = Claim(source, term.get("id"), kind, str(token).strip())
            claims.setdefault((dimension, claim.token.casefold()), []).append(claim)

    errors = []
    for (dimension, _), claimants in claims.items():
        if len(claimants) < 2:
            continue
        where = "; ".join(f"{c.source}:{c.term_id} {c.kind} '{c.token}'" for c in claimants)
        if len({c.term_id for c in claimants}) > 1:
            errors.append(f"{dimension}: '{claimants[0].token}' is claimed by {len(claimants)} terms "
                          f"({where}) — resolution is case-insensitive, so only one of them is reachable")
        elif sorted(c.kind for c in claimants) != ["canonical", "display code"]:
            errors.append(f"{dimension}: term '{claimants[0].term_id}' claims '{claimants[0].token}' "
                          f"more than once ({where}) — only a display code may repeat its own canonical")
    return errors


TYPICAL_REFERENCES = {
    "typical_genres": "genre",
    "typical_song_forms": "song_form",
    "typical_rhythmic_feels": "rhythmic_feel",
}


def typical_reference_errors(terms: list[tuple[str, str, dict]]) -> list[str]:
    """Report every typical_* entry that is not exactly a canonical of its target dimension, or that repeats one.

    A reference names the canonical, never an alias, display code, or dsl_key: those can be renamed or moved
    to another term without touching the terms that point at them, so a reference through one breaks
    silently. gen_vocab_docs.py also deep-links each reference to the anchor it derives from the canonical.
    """
    errors = []
    for key, dimension in TYPICAL_REFERENCES.items():
        targets = [term for _, term_dimension, term in terms if term_dimension == dimension]
        canonicals = {term.get("canonical") for term in targets}
        canonical_for = {str(token).casefold(): term.get("canonical")
                         for term in targets
                         for token in [*(term.get("aliases") or []), (term.get("display") or {}).get("code"),
                                       term.get("dsl_key")]
                         if token}
        canonical_for |= {str(canonical).casefold(): canonical for canonical in canonicals}

        for source, _, term in terms:
            where = f"{source}: term '{term.get('id')}' {key}"
            references = term.get(key) or []
            if not isinstance(references, list):
                errors.append(f"{where} must be a list, not {type(references).__name__}")
                continue
            for reference in references:
                if reference in canonicals:
                    continue
                canonical = canonical_for.get(str(reference).casefold())
                fix = f"use '{canonical}'" if canonical else f"it names no {dimension} in core/{dimension}.yaml"
                errors.append(f"{where} '{reference}' is not a {dimension} canonical — {fix}")
            for repeated in sorted({str(r) for r in references if references.count(r) > 1}):
                errors.append(f"{where} lists '{repeated}' more than once")
    return errors


REVIEW_STATUSES = ("expert-review-pending",)


def validation_marker_errors(terms: list[tuple[str, str, dict]], markers: dict[str, object]) -> list[str]:
    """Report every `validation:` entry that gen_vocab_docs.py would not render as a review flag.

    The docs flag a core attribute by its key and a whole pack by its pack.yaml status, so a key no term
    carries, an unknown status, or a flag in a pack's dances.yaml would mark nothing and hide that the
    data is unreviewed.
    """
    errors = []
    for source, marks in markers.items():
        if source.endswith("/pack.yaml"):
            if marks not in REVIEW_STATUSES:
                errors.append(f"{source}: validation '{marks}' — expected one of {', '.join(REVIEW_STATUSES)}")
            continue
        if not source.startswith("core/"):
            errors.append(f"{source}: validation is rendered for core dimensions only — "
                          f"a pack's review status belongs in its pack.yaml")
            continue
        if not isinstance(marks, dict):
            errors.append(f"{source}: validation must map attribute to status, not {type(marks).__name__}")
            continue
        carried = {key for term_source, _, term in terms if term_source == source for key in term}
        for attribute, status in marks.items():
            if attribute not in carried:
                errors.append(f"{source}: validation names '{attribute}', which no term in the file carries")
            if status not in REVIEW_STATUSES:
                errors.append(f"{source}: validation '{attribute}' has status '{status}' — "
                              f"expected one of {', '.join(REVIEW_STATUSES)}")
    return errors


def parent_errors(terms: list[tuple[str, str, dict]]) -> list[str]:
    """Report every `parent` that does not point at another term of the same dimension, or that closes a loop.

    The runtime tolerates an unknown parent (an enabled pack may lack the group), so a dangling id is
    silent there; the baseline carries every pack, so here it is a typo. The hierarchy is a strict tree:
    one parent per term.
    """
    ids = {(dimension, term.get("id")) for _, dimension, term in terms}
    parent_of = {(dimension, term.get("id")): term["parent"]
                 for _, dimension, term in terms if isinstance(term.get("parent"), str)}
    errors = []
    for source, dimension, term in terms:
        parent = term.get("parent")
        if not parent:
            continue
        where = f"{source}: term '{term.get('id')}' parent '{parent}'"
        if not isinstance(parent, str):
            errors.append(f"{where} must be a single term id, not {type(parent).__name__}")
        elif parent == term.get("id"):
            errors.append(f"{where} points at the term itself")
        elif (dimension, parent) not in ids:
            errors.append(f"{where} names no {dimension} term")
        else:
            seen, step = {term.get("id")}, parent
            while (dimension, step) in parent_of:
                if step in seen:
                    errors.append(f"{where} closes a loop through '{step}'")
                    break
                seen.add(step)
                step = parent_of[(dimension, step)]
    return errors


DARK_BACKGROUND = "#121212"
MIN_GRAPHICAL_CONTRAST = 3.0
MIN_COLOUR_DISTANCE = 0.04


def _linear(channel: int) -> float:
    c = channel / 255
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def _rgb(colour: str) -> tuple[int, int, int]:
    return int(colour[1:3], 16), int(colour[3:5], 16), int(colour[5:7], 16)


def _luminance(colour: str) -> float:
    r, g, b = (_linear(c) for c in _rgb(colour))
    return 0.2126 * r + 0.7152 * g + 0.0722 * b


def _oklab(colour: str) -> tuple[float, float, float]:
    r, g, b = (_linear(c) for c in _rgb(colour))
    l, m, s = (x ** (1 / 3) for x in (0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b,
                                      0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b,
                                      0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b))
    return (0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s,
            1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s,
            0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s)


def dance_colour_errors(terms: list[tuple[str, str, dict]]) -> list[str]:
    """Report dance colours that are malformed, too dark for the dark theme, or too close to another dance's.

    A dance chip is told apart from its neighbours by colour alone in a filter list, so the colour is the
    dance's own: no two dances (groups included) share one or sit within MIN_COLOUR_DISTANCE of each other
    in OKLab, and every colour keeps MIN_GRAPHICAL_CONTRAST against the dark background (WCAG 1.4.11).
    """
    errors = []
    coloured = []
    background = _luminance(DARK_BACKGROUND) + 0.05
    for source, dimension, term in terms:
        if dimension != "dances":
            continue
        colour = (term.get("display") or {}).get("color")
        where = f"{source}: dance '{term.get('id')}'"
        if not (isinstance(colour, str) and re.fullmatch(r"#[0-9A-Fa-f]{6}", colour)):
            errors.append(f"{where} needs display.color as '#RRGGBB', not {colour!r}")
            continue
        colour = colour.upper()
        contrast = (_luminance(colour) + 0.05) / background
        if contrast < MIN_GRAPHICAL_CONTRAST:
            errors.append(f"{where} display.color {colour} has contrast {contrast:.2f}:1 against "
                          f"{DARK_BACKGROUND}, below {MIN_GRAPHICAL_CONTRAST}:1")
        coloured.append((term.get("id"), colour))
    for i, (first_id, first) in enumerate(coloured):
        for second_id, second in coloured[i + 1:]:
            distance = math.dist(_oklab(first), _oklab(second))
            if distance < MIN_COLOUR_DISTANCE:
                errors.append(f"dances '{first_id}' ({first}) and '{second_id}' ({second}) are "
                              f"{distance:.3f} apart in OKLab, below {MIN_COLOUR_DISTANCE}")
    return errors


def feel_rank_errors(terms: list[tuple[str, str, dict]], feel_scenes: dict) -> list[str]:
    """Report every rhythmic feel that is ranked neither by a term `tier` nor by a scene entry.

    A feel's `tier` ranks it inside its cluster (1 = the general feel); the feels of a dance scene with no
    general parent in the cluster are ranked per scene in feel_scenes.yaml instead. A feel with neither
    would sort by catalog order, and a scene entry for a feel that does not exist ranks nothing.
    """
    feels = {term.get("id"): term for _, dimension, term in terms if dimension == "rhythmic_feel"}
    errors = [f"feel_scenes.yaml: '{feel}' is not a rhythmic_feel term"
              for feel in feel_scenes if feel not in feels]
    for feel_id, term in feels.items():
        if "tier" not in term and not (feel_scenes.get(feel_id) or {}).get("scenes"):
            errors.append(f"core/rhythmic_feel.yaml: feel '{feel_id}' has no tier and no scene in "
                          f"feel_scenes.yaml, so nothing ranks it")
    return errors


def _grouping_declaration_errors(source: str, attribute: str, keys: list, labels: dict, terms: list[dict],
                                 exempt: set[str]) -> list[str]:
    """Report a grouping whose keys, labels and term filings disagree.

    The group keys are closed: a label for a key that is not declared, a declared key with no label, a
    key nothing is filed under, and a term filed under an undeclared key are each an error.
    """
    errors = []
    if len(keys) != len(set(keys)):
        errors.append(f"{source}: grouping on '{attribute}' repeats a key")
    for key in keys:
        if key not in labels:
            errors.append(f"{source}: grouping on '{attribute}' has no label for '{key}'")
    for label_key in labels:
        if label_key not in keys:
            errors.append(f"{source}: grouping on '{attribute}' labels '{label_key}', which is not a declared key")
    used = set()
    for term in terms:
        value = term.get(attribute)
        if value is None:
            if term.get("id") not in exempt:
                errors.append(f"{source}: term '{term.get('id')}' has no '{attribute}', so no group holds it")
        elif value not in keys:
            errors.append(f"{source}: term '{term.get('id')}' is filed under '{value}', which is not declared")
        else:
            used.add(value)
    for key in keys:
        if key not in used:
            errors.append(f"{source}: group '{key}' of '{attribute}' holds no term")
    return errors


def grouping_errors(manifest: dict) -> list[str]:
    """Check every closed grouping: the core axes marked `grouping: true` and each pack's `families`.

    A pack's group terms (a dance with no `family`, standing for the family itself) are exempt from
    the filing rule; they are the terms other dances name as `parent`.
    """
    errors = []
    for dimension in (manifest.get("core") or {}).get("dimensions") or []:
        source = f"core/{dimension}.yaml"
        doc = load_yaml(VOCAB / "core" / f"{dimension}.yaml")
        marked = [axis for axis in doc.get("axes") or [] if axis.get("grouping")]
        if len(marked) > 1:
            errors.append(f"{source}: more than one axis is marked grouping")
        for axis in marked:
            errors += _grouping_declaration_errors(
                source, axis["name"], axis.get("values") or [], axis.get("labels") or {}, doc.get("values") or [], set())
    for pack in manifest.get("packs") or []:
        source = f"packs/{pack}/pack.yaml"
        meta = load_yaml(VOCAB / "packs" / pack / "pack.yaml")
        dances = (load_yaml(VOCAB / "packs" / pack / "dances.yaml").get("values")) or []
        group_terms = {term.get("parent") for term in dances if term.get("parent")} - {None}
        errors += _grouping_declaration_errors(
            source, "family", meta.get("families") or [], meta.get("family_labels") or {}, dances,
            {term.get("id") for term in dances if "family" not in term and term.get("id") in group_terms})
    return errors


def main():
    errors = []

    vocabulary_terms, vocabulary_markers, vocabulary_errors = load_vocabulary_terms()
    errors += vocabulary_errors
    errors += resolution_token_errors(vocabulary_terms)
    errors += typical_reference_errors(vocabulary_terms)
    errors += validation_marker_errors(vocabulary_terms, vocabulary_markers)
    errors += parent_errors(vocabulary_terms)
    errors += dance_colour_errors(vocabulary_terms)
    errors += grouping_errors(load_yaml(VOCAB / "manifest.yaml"))
    errors += feel_rank_errors(vocabulary_terms, load_yaml(VOCAB / "feel_scenes.yaml").get("feels") or {})

    fields_doc = load_yaml(ROOT / "data" / "fields.yaml")
    invalid_doc = load_yaml(ROOT / "data" / "field-invalid-examples.yaml") or {}
    constraints_doc = load_yaml(ROOT / "data" / "field-constraints.yaml") or {}

    targets = [
        (fields_doc, "fields.schema.json"),
        (invalid_doc, "field-invalid-examples.schema.json"),
        (constraints_doc, "field-constraints.schema.json"),
    ]
    for data, schema_name in targets:
        schema = load_json(ROOT / "schemas" / schema_name)
        validator = Draft202012Validator(schema)
        for err in sorted(validator.iter_errors(data), key=str):
            loc = "/".join(str(p) for p in err.absolute_path) or "(root)"
            errors.append(f"{schema_name} @ {loc}: {err.message}")

    # Every *.sdvo.yaml in the repo is an INSTANCE of the overlay schema — the shipped example and the
    # legacy-code fragment are what a DJ copies, so a broken one ships the mistake. Also assert the
    # yaml-language-server modeline on line 1 actually names the schema's $id (the reachability check
    # persisted-file-formats.md asks for: modeline -> shipped schema -> version).
    overlay_schema = load_json(ROOT / "schemas" / "sdvo.schema.json")
    overlay_validator = Draft202012Validator(overlay_schema)
    overlays = sorted(ROOT.glob("**/*.sdvo.yaml"))
    if not overlays:
        errors.append("no *.sdvo.yaml instances found — the shipped example should be one")
    for path in overlays:
        rel = path.relative_to(ROOT).as_posix()
        first = path.read_text(encoding="utf-8").splitlines()[0] if path.read_text(encoding="utf-8") else ""
        if not first.startswith("# yaml-language-server: $schema="):
            errors.append(f"{rel}: line 1 must be the yaml-language-server modeline")
        elif first.split("$schema=", 1)[1].strip() != overlay_schema["$id"]:
            errors.append(f"{rel}: modeline does not resolve to the schema $id {overlay_schema['$id']}")
        doc = load_yaml(path)
        for err in sorted(overlay_validator.iter_errors(doc), key=str):
            loc = "/".join(str(p) for p in err.absolute_path) or "(root)"
            errors.append(f"sdvo.schema.json @ {rel}:{loc}: {err.message}")

    # Cross-checks the schema alone can't express.
    ids = [f["id"] for f in fields_doc.get("fields", [])]
    seen = set()
    for fid in ids:
        if fid in seen:
            errors.append(f"fields.yaml: duplicate field id '{fid}'")
        seen.add(fid)

    # Invalid-examples may be keyed by a field id OR by a standard-frame slug
    # (the generator documents standard frames too — frame_slug = first token, lowercased).
    field_ids = set(ids)
    allowed_keys = set(field_ids)
    for sf in fields_doc.get("standard_frames", []):
        frame = str(sf.get("frame", ""))
        if frame:
            allowed_keys.add(frame.split()[0].lower().strip("()"))
    for key in invalid_doc:
        if key not in allowed_keys:
            errors.append(
                f"field-invalid-examples.yaml: '{key}' is not a field id or standard-frame slug")

    for key in constraints_doc:
        if key not in field_ids:
            errors.append(f"field-constraints.yaml: '{key}' is not a field id in fields.yaml")

    # The authoritative SDTS spec version lives in fields.yaml `version`; every conformance vector's
    # suite_version MUST equal it (single source of truth — see docs/standard/versioning.md).
    spec_version = fields_doc.get("version")
    for rel in ("vectors/hash.json", "vectors/sync/reconcile.json", "vectors/validation/fields.json"):
        vpath = ROOT / "conformance" / rel
        if vpath.exists():
            sv = load_json(vpath).get("suite_version")
            if sv != spec_version:
                errors.append(
                    f"conformance/{rel}: suite_version '{sv}' != spec version '{spec_version}' (data/fields.yaml)")

    if errors:
        print("DATA VALIDATION FAILED:")
        for e in errors:
            print("  -", e)
        return 1

    print(f"OK - {len(field_ids)} fields valid; "
          f"{len(invalid_doc)} invalid-example sets all resolve to real fields; "
          f"{len(vocabulary_terms)} vocabulary terms claim no token twice "
          f"and name typical genres, song forms, and feels by canonical.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
