# Involved People List &nbsp;·&nbsp; `TIPL`

`ID3 standard frame` · Standard music frames · origin: **ID3 standard**

[← Field catalog](../README.md) · [Spec overview](../../spec.md) · [Registry (fields.yaml)](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Cross-tool interop](../../../sync/tool-comparison.md)

> Record who did WHAT behind the recording (producer, engineer, arranger).

| | |
|---|---|
| **Frame** | `TIPL` |
| **Concept** | Involved People List |
| **Origin** | ID3 standard (pass-through; SDTS does not redefine it) |

Production / technical credits as a flat list of (function, person) pairs — producer, engineer, arranger, and the like. A PAIRED-list frame (ID3v2.4): each entry is a role token followed by the person who filled it, not a single text value. The container surfaces it as raw (role, name) pairs and imposes no role vocabulary — typed resolution is a semantic-layer concern. SDTS defines an OPEN baseline `credit_role` vocabulary for that resolution: a known role gains a code + description, an unknown role passes through carrying its raw string.

**When to use.** Record who did WHAT behind the recording (producer, engineer, arranger). Performers on an instrument belong in TMCL, not here. Leave empty when no production credits are known — do NOT mirror the artist (TPE1).

**Examples (valid).** `producer / John Hammond` · `arranger / Fletcher Henderson`

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
