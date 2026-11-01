# Original Title &nbsp;·&nbsp; `original_title`

> Group performances of one work — pull every version of 'Stormy Monday', or avoid programming the same work twice in a set.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `string` · single |
| **Values** | free text — the title of the WORK this recording is a performance of; human-set, or imported from a MediaMonkey library's Original Title column |
| **Tag** | `TXXX:SDTS_ORIGINAL_TITLE` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Recording character · SDTS-defined |

The title of the WORK this recording performs, when it differs from this recording's own title (TIT2). It is the missing fourth member of the original-work cluster: TOPE carries the original performer, TOAL the original album, TDOR the original date — and until now nothing carried the original TITLE, a gap TIT2's own documentation names. Absent on an original-era recording, exactly like TOPE.

**Values.** Free text: the original work's title as commonly known, written like a title (no '(cover)' decorations, no artist). Absent means this recording IS the original, or the work title equals TIT2 — a consumer needing a work key falls back to TIT2.

**Examples.** ✓ `Stormy Monday` · `Sing Sing Sing`

**Avoid.**
- the same string as TIT2 — leave it EMPTY when this recording IS the work; a consumer falls back to TIT2
- `Stormy Monday (T-Bone Walker)` — the performer goes in TOPE, not in the title
- `Call It Stormy Monday (cover)` — no decorations; the work's title only
- the original ALBUM — that is TOAL, a different frame and a different fact
- filling it on an original-era recording — like TOPE, it stays empty unless this performs someone else's work

**Related.** [Recording Origin](recording_origin.md) · [Recording Context](recording_context.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
