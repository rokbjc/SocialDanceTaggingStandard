# Original Release Date &nbsp;·&nbsp; `TDOR`

`ID3 standard frame` · Standard music frames · origin: **ID3 standard**

[← Field catalog](../README.md) · [Spec overview](../../spec.md) · [Registry (fields.yaml)](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Cross-tool interop](../../../sync/tool-comparison.md)

> Set on reissues / remasters / covers so the original date survives.

| | |
|---|---|
| **Frame** | `TDOR` |
| **Concept** | Original Release Date |
| **Origin** | ID3 standard (pass-through; SDTS does not redefine it) |

The ORIGINAL release date of the work (ID3v2.4; v2.3 used TORY, year-only). Distinct from TDRC, which is THIS release's date.

**When to use.** Set on reissues / remasters / covers so the original date survives. Feeds the 'is this period-original?' judgement (see recording_origin). A full date or a bare year both work.

**Examples (valid).** `1938` · `1938-08-16`

**Invalid (don't do this).**
- this reissue's date — that's TDRC; TDOR is the ORIGINAL date
- a decade like `30s` — TDOR is a date/year; SDTS has no decade field (derive the era from the year)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
