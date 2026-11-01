#!/usr/bin/env python3
"""Generate the vocabulary-overlay JSON Schema + its C# format constants.

Source of truth = schemas/sdvo.schema.json.in (the hand-edited contract) + data/vocabulary/manifest.yaml
(the dimension list). Emits, checked in:

  schemas/sdvo.schema.json                                the shipped draft 2020-12 contract
  dotnet/src/SocialDanceTaggingStandard.Taxonomy/Generated/SdvoFormat.g.cs      extension, schema id, current schemaVersion

Why generate instead of hand-freezing the dimension enum: a hand-written enum drifts the moment a core
dimension is added (credit_role and instrument both arrived recently), and omitting the enum entirely
would leave an unknown dimension key as what it is today - a silent no-op that changes nothing and
reports nothing. Generating gives both the check and editor autocomplete on dimension names.
"""
from __future__ import annotations

import json
import pathlib

import yaml

from _csharp_emit import cs_string, header, write_cs

ROOT = pathlib.Path(__file__).resolve().parent.parent
TEMPLATE = ROOT / "schemas" / "sdvo.schema.json.in"
MANIFEST = ROOT / "data" / "vocabulary" / "manifest.yaml"
SCHEMA_OUT = ROOT / "schemas" / "sdvo.schema.json"
CS_OUT = ROOT / "dotnet" / "src" / "SocialDanceTaggingStandard.Taxonomy" / "Generated" / "SdvoFormat.g.cs"

# The compound kind extension (persisted-file-formats.md: "<kind>.<fmt>", one extension <-> one schema).
EXTENSION = ".sdvo.yaml"
SCHEMA_FILE = "sdvo.schema.json"
SCHEMA_VERSION = 2
# The virtual cross-pack dimension: valid everywhere a dimension key is, EXCEPT add_terms.
DANCES_DIMENSION = "dances"


def _dimensions() -> tuple[list[str], list[str]]:
    manifest = yaml.safe_load(MANIFEST.read_text(encoding="utf-8"))
    core = list(manifest["core"]["dimensions"])
    return core + [DANCES_DIMENSION], core


def emit_schema(all_dims: list[str], core_dims: list[str]) -> None:
    text = TEMPLATE.read_text(encoding="utf-8")
    text = text.replace("{{DIMENSIONS}}", json.dumps(all_dims))
    text = text.replace("{{CORE_DIMENSIONS}}", json.dumps(core_dims))
    # Round-trip through json so the emitted file is byte-stable regardless of template formatting.
    doc = json.loads(text)
    SCHEMA_OUT.write_text(json.dumps(doc, indent=2, ensure_ascii=False) + "\n", encoding="utf-8", newline="\n")


def emit_cs(all_dims: list[str]) -> None:
    body = [
        header("gen_overlay_schema.py", "schemas/sdvo.schema.json.in + data/vocabulary/manifest.yaml"),
        "namespace SocialDanceTaggingStandard.Taxonomy;\n\n",
        "/// <summary>The vocabulary-overlay file format: its compound extension, its shipped JSON Schema,\n",
        "/// and the schema version this loader writes. A file naming the format is spelled through these\n",
        "/// constants, never as a literal (CLAUDE.md guideline 1).</summary>\n",
        "public static class SdvoFormat\n{\n",
        '    /// <summary>The canonical compound extension — one extension, one schema.</summary>\n',
        f"    public const string Extension = {cs_string(EXTENSION)};\n\n",
        '    /// <summary>The shipped schema\'s file name, next to the library under <c>schemas/</c>.</summary>\n',
        f"    public const string SchemaFileName = {cs_string(SCHEMA_FILE)};\n\n",
        '    /// <summary>The schema <c>$id</c> — the identity a file\'s modeline resolves to.</summary>\n',
        f"    public const string SchemaId = {cs_string(json.loads(SCHEMA_OUT.read_text(encoding='utf-8'))['$id'])};\n\n",
        "    /// <summary>The <c>schemaVersion</c> this loader writes. An overlay with no version is legacy\n",
        "    /// v0 and is migrated forward; a higher version is rejected rather than partially read.</summary>\n",
        f"    public const int CurrentVersion = {SCHEMA_VERSION};\n\n",
        "    /// <summary>The version of an overlay written before the format was versioned.</summary>\n",
        "    public const int LegacyVersion = 0;\n\n",
        "    /// <summary>Every dimension key an overlay may use (core dimensions plus the virtual\n",
        "    /// <c>dances</c> dimension), in manifest order.</summary>\n",
        "    public static readonly IReadOnlyList<string> DimensionNames =\n    [\n",
    ]
    body += [f"        {cs_string(d)},\n" for d in all_dims]
    body.append("    ];\n}\n")
    write_cs(CS_OUT, "".join(body))


def main() -> None:
    all_dims, core_dims = _dimensions()
    emit_schema(all_dims, core_dims)
    emit_cs(all_dims)
    print(f"Wrote {SCHEMA_OUT.relative_to(ROOT)} ({len(all_dims)} dimensions) + "
          f"{CS_OUT.relative_to(ROOT)} (v{SCHEMA_VERSION}, {EXTENSION})")


if __name__ == "__main__":
    main()
