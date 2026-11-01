# Title &nbsp;·&nbsp; `TIT2`

`ID3 standard frame` · Standard music frames · origin: **ID3 standard**

[← Field catalog](../README.md) · [Spec overview](../../spec.md) · [Registry (fields.yaml)](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Cross-tool interop](../../../sync/tool-comparison.md)

> Always; every track has a title.

| | |
|---|---|
| **Frame** | `TIT2` |
| **Concept** | Title |
| **Origin** | ID3 standard (pass-through; SDTS does not redefine it) |

The title of THIS recording, written as-is. For a cover or medley it is this version's own title. ID3 has no original-title frame; the work this recording performs is named by the SDTS `original_title` field, alongside TOPE (original performer), TOAL (original album) and TDOR (original date).

**When to use.** Always; every track has a title. Keep the artist out of it (use TPE1) and don't bake '(Live)'/'(Cover)' decorations in — those are recording_context / object-state.

**Examples (valid).** `Sing Sing Sing` · `Medley: Jumpin' at the Woodside / One O'Clock Jump`

**Invalid (don't do this).**
- `` (empty) — every track needs a title
- `Sing Sing Sing — Benny Goodman` — keep the artist out of the title (it goes in TPE1)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
