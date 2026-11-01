# Grouping &nbsp;·&nbsp; `TIT1`

`ID3 standard frame` · Standard music frames · origin: **ID3 standard**

[← Field catalog](../README.md) · [Spec overview](../../spec.md) · [Registry (fields.yaml)](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Cross-tool interop](../../../sync/tool-comparison.md)

> Frozen pass-through only.

| | |
|---|---|
| **Frame** | `TIT1` |
| **Concept** | Grouping |
| **Origin** | ID3 standard (pass-through; SDTS does not redefine it) |

Reused for the legacy `workflow_tags` bag — multi-value markers across analytic / personal / recording-meta / classifier-meta / object-state.

**When to use.** Frozen pass-through only. NOT a portable SDTS field — being decomposed (Medley→song_form, Cover/Tribute→recording_origin, recording-meta→quality flags; Canonical/Duplicate + personal/triage markers stay non-portable), pending vocabulary-maintainer ratification.

**Examples (valid).** `Canonical` · `Cover` · `Medley`

**Invalid (don't do this).**
- treating it as a portable SDTS field — it's a frozen pass-through
- inventing new tokens — the vocabulary is catalog-owned (`workflow_tags`)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
