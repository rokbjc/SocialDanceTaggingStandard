# Original Album &nbsp;·&nbsp; `TOAL`

`ID3 standard frame` · Standard music frames · origin: **ID3 standard**

[← Field catalog](../README.md) · [Spec overview](../../spec.md) · [Registry (fields.yaml)](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Cross-tool interop](../../../sync/tool-comparison.md)

> Optional.

| | |
|---|---|
| **Frame** | `TOAL` |
| **Concept** | Original Album |
| **Origin** | ID3 standard (pass-through; SDTS does not redefine it) |

The album / show / film the work ORIGINALLY appeared on, when this release differs (reissue, compilation, cover).

**When to use.** Optional. Set when the original release differs from this one's TALB — e.g. a 2015 compilation track originally issued on a 1938 single. NOTE: MediaMonkey borrows this frame for the original TITLE (its OrigSongTitle column), which is NOT the ID3 meaning and NOT how SDTS reads it — the original title has its own field (`original_title`). SDTS cannot tell the two meanings apart from the frame alone, so treating a library's TOAL as an original title is an INGEST-PROFILE declaration about that known library, never a per-file inference.

**Examples (valid).** `The Famous 1938 Carnegie Hall Jazz Concert`

**Invalid (don't do this).**
- this reissue's album — that's TALB; TOAL is the ORIGINAL album

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
