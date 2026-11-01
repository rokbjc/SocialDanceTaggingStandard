#!/usr/bin/env python3
"""Generate the validation conformance vectors from fields.yaml + field-constraints.yaml.

Emits conformance/vectors/validation/fields.json: per field, the machine constraints + a list of cases
(valid cases from the registry `examples`; invalid cases from each constraint's `bad_examples`). Consumed
by both the Python and .NET validators. Generated artifact — do not hand-edit; re-run after editing
data/field-constraints.yaml. Usage: python scripts/gen_conformance_vectors.py
"""
import copy
import json
import pathlib

import yaml

ROOT = pathlib.Path(__file__).resolve().parent.parent


def load_yaml(path):
    with open(path, encoding="utf-8") as fh:
        return yaml.safe_load(fh)


def main():
    fields_doc = load_yaml(ROOT / "data" / "fields.yaml")
    fields = {f["id"]: f for f in fields_doc["fields"]}
    constraints = load_yaml(ROOT / "data" / "field-constraints.yaml")

    # suite_version tracks the authoritative SDTS spec version (data/fields.yaml `version`),
    # so the conformance suite can never drift from the spec it tests.
    out = {"suite_version": fields_doc.get("version", "0.0.0"), "doc": "../../README.md", "fields": {}}
    for fid, raw in constraints.items():
        c = copy.deepcopy(raw)
        bad = c.pop("bad_examples", [])
        cases = [{"value": str(ex), "valid": True, "error": None}
                 for ex in fields.get(fid, {}).get("examples", [])]
        for b in bad:
            cases.append({"value": b["value"], "valid": False, "error": b["error"]})
        out["fields"][fid] = {"constraints": c, "cases": cases}

    dest = ROOT / "conformance" / "vectors" / "validation" / "fields.json"
    dest.parent.mkdir(parents=True, exist_ok=True)
    dest.write_text(json.dumps(out, indent=2, ensure_ascii=False) + "\n", encoding="utf-8", newline="\n")
    print(f"Wrote {dest.relative_to(ROOT)} - {len(out['fields'])} fields")


if __name__ == "__main__":
    main()
