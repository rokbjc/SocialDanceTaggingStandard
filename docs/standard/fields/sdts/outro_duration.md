# Outro Duration &nbsp;·&nbsp; `outro_duration`

> Spot tracks with a long fade / applause tail.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `int (ms)` · single |
| **Values** | milliseconds — length of the outro/tail after the last groove (derived from SB-Beats/segments, human-confirmable) |
| **Tag** | `TXXX:SDTS_OUTRO_DURATION` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Structure & cueing · SDTS-defined |

How long the outro/tail runs after the last danceable groove, in ms. DESCRIPTIVE — not a trim instruction (that is playback_stop) — so the editor/player can flag a long fade or applause tail. Pairs with ending_type. Auto-derivable from the beat grid and segment boundaries.

**Values.** non-negative integer milliseconds — how long the outro/tail runs. 0 = ends on the groove. The player MAY flag a long fade / applause tail above a threshold.

**Examples.** ✓ `0` · `3000` · `11000`

**Avoid.**
- `3s` — integer milliseconds (e.g. `3000`)
- using it to TRIM — that's `playback_stop`; `outro_duration` only DESCRIBES the length

**Related.** [Intro Duration](intro_duration.md) · [Ending Type](ending_type.md) · [Playback Stop (cue-out)](playback_stop.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
