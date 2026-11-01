"""The field <-> vocabulary-dimension correspondence, shared by every generator.

A field draws its values from the same-named vocabulary dimension, EXCEPT the fields in
BACKING_DIMENSION_OVERRIDE, which bind a differently-named one. A field whose backing id is not a real
dimension (a number, a cue, an intrinsic enum like meter) has no vocabulary backing at all. The runtime
reads the same mapping from the generated FieldCatalog, so code and docs cannot disagree about it.
"""
from __future__ import annotations

import functools
import pathlib

import yaml

ROOT = pathlib.Path(__file__).resolve().parent.parent
MANIFEST_YAML = ROOT / "data" / "vocabulary" / "manifest.yaml"
FIELDS_YAML = ROOT / "data" / "fields.yaml"

# `dance` binds `dances`, the cross-pack union; `instrument_led` binds the unified `instrument` dimension.
BACKING_DIMENSION_OVERRIDE = {"dance": "dances", "instrument_led": "instrument"}


@functools.cache
def vocabulary_dimensions() -> frozenset[str]:
    manifest = yaml.safe_load(MANIFEST_YAML.read_text(encoding="utf-8"))
    return frozenset(manifest["core"]["dimensions"]) | {"dances"}


@functools.cache
def _field_ids() -> tuple[str, ...]:
    return tuple(f["id"] for f in yaml.safe_load(FIELDS_YAML.read_text(encoding="utf-8"))["fields"])


def backing_dimension(field_id: str) -> str | None:
    """The vocabulary dimension a field draws its values from, or None for a non-vocabulary field."""
    backing = BACKING_DIMENSION_OVERRIDE.get(field_id, field_id)
    return backing if backing in vocabulary_dimensions() else None


def backing_field(dimension: str) -> str | None:
    """The SDTS field a dimension supplies values to, or None — `credit_role` feeds the standard TIPL frame,
    not an SDTS field."""
    return next((f for f in _field_ids() if backing_dimension(f) == dimension), None)
