# Album / Labels &nbsp;·&nbsp; `TALB`

`ID3 standard frame` · Standard music frames · origin: **ID3 standard**

[← Field catalog](../README.md) · [Spec overview](../../spec.md) · [Registry (fields.yaml)](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Cross-tool interop](../../../sync/tool-comparison.md)

> Carry the label(s) / release grouping.

| | |
|---|---|
| **Frame** | `TALB` |
| **Concept** | Album / Labels |
| **Origin** | ID3 standard (pass-through; SDTS does not redefine it) |

Album title. The reference implementation overloads it as a '; '-joined list of record labels / release groupings rather than a single album name.

**When to use.** Carry the label(s) / release grouping. The track's ORIGINAL album (for a reissue or cover) goes in TOAL, not here.

**Examples (valid).** `Decca` · `Columbia; OKeh`

**Invalid (don't do this).**
- the ORIGINAL album of a reissue — that goes in TOAL, not TALB

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
