# Musician Credits List &nbsp;·&nbsp; `TMCL`

`ID3 standard frame` · Standard music frames · origin: **ID3 standard**

[← Field catalog](../README.md) · [Spec overview](../../spec.md) · [Registry (fields.yaml)](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Cross-tool interop](../../../sync/tool-comparison.md)

> Record each performer and the instrument they played.

| | |
|---|---|
| **Frame** | `TMCL` |
| **Concept** | Musician Credits List |
| **Origin** | ID3 standard (pass-through; SDTS does not redefine it) |

Performer credits as a flat list of (instrument, person) pairs — who played what. A PAIRED-list frame (ID3v2.4), sibling to TIPL: each entry is an instrument token followed by the performer. The container surfaces it as raw (instrument, name) pairs and constrains nothing; the instrument column MAY be resolved (OPEN) against the SDTS instrument vocabulary (the `instrument` dimension — the same one the instrument_led lead field draws its lead subset from; a known instrument gains a code + description + the specific→family hierarchy, an unknown passes through raw). Credit-only instruments carry lead_capable=false, so they resolve here without polluting the lead field's subset.

**When to use.** Record each performer and the instrument they played. Non-performing production roles (producer, engineer) belong in TIPL. Leave empty when the line-up is unknown.

**Examples (valid).** `Piano / Count Basie` · `Clarinet / Benny Goodman`

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
