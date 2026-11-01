# Recording Context &nbsp;·&nbsp; `recording_context`

> Avoid dropping a talky live/applause track where you need a tight transition; choose studio vs live deliberately.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md) · [Values](../../../model/vocabulary/recording_context.md)

| | |
|---|---|
| **Type** | `enum` · single |
| **Values** | SDTS baseline vocabulary -> Recording Context (canonical names) |
| **Tag** | `TXXX:SDTS_RECORDING_CONTEXT` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | active · Recording character · SDTS-defined |

How the recording was captured — its performance setting and provenance (studio session, live concert, radio/broadcast, film soundtrack, bootleg, jam, transcription disc). Distinct from recording_quality_era, which is the technology/fidelity window, and from TDRC, which is the precise release year.

**Values.** Single enum value (cardinality: single) drawn from the vocabulary's recording-context vocabulary; resolve to the canonical name. Captures how the recording was captured (performance setting + provenance), kept separate from the technology window (recording_quality_era) and the precise release year (TDRC). Value set: SDTS baseline vocabulary -> Recording Context (not enumerated here).

**Examples.** ✓ `Studio` · `Live` · `Radio Broadcast` · `Jam Session`

**Avoid.**
- `Studio; Live` — single-valued; pick the dominant context
- `Concert` — not a catalog value (use `Live`)
- `Remastered` — that's a recording-meta flag, not a capture context

**Notes.** Legacy on-disk mirror `COMM:Songs-DB_Custom8` — kept for backward compatibility (see [Part 2 sync](../../../sync/tool-comparison.md)); NOT the canonical home.

**Related.** [Recording Quality Era](recording_quality_era.md) · [Recording Origin](recording_origin.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
