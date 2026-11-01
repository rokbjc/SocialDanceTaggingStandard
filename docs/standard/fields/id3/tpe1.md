# Artist &nbsp;·&nbsp; `TPE1`

`ID3 standard frame` · Standard music frames · origin: **ID3 standard**

[← Field catalog](../README.md) · [Spec overview](../../spec.md) · [Registry (fields.yaml)](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Cross-tool interop](../../../sync/tool-comparison.md)

> Always.

| | |
|---|---|
| **Frame** | `TPE1` |
| **Concept** | Artist |
| **Origin** | ID3 standard (pass-through; SDTS does not redefine it) |

Lead performer(s) of THIS recording. For a cover or tribute this is the performer of this version; the ORIGINAL artist goes in TOPE.

**When to use.** Always. Multiple performers join with '; '. The composer is not the artist (that's TCOM).

**Examples (valid).** `Benny Goodman` · `Ella Fitzgerald; Louis Armstrong`

**Invalid (don't do this).**
- `Various` on a single-artist track
- the composer's name — that's TCOM, not the performer

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
