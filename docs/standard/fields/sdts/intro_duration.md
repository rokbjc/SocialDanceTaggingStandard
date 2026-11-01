# Intro Duration &nbsp;·&nbsp; `intro_duration`

> Spot tracks with an uncomfortably long intro before the groove.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `int (ms)` · single |
| **Values** | milliseconds — length of the intro before the groove lands (derived from SB-Beats/segments, human-confirmable) |
| **Tag** | `TXXX:SDTS_INTRO_DURATION` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Structure & cueing · SDTS-defined |

How long the intro runs before the danceable groove lands, in ms. DESCRIPTIVE — not a trim instruction (that is playback_start) — so the editor/player can flag a long talky/rubato/count-in intro. Auto-derivable from the beat grid (time to the first strong downbeat).

**Values.** non-negative integer milliseconds — how long until the danceable groove starts. 0 = drops straight in. The player MAY flag an 'uncomfortably long' intro above a threshold.

**Examples.** ✓ `0` · `4200` · `12000`

**Avoid.**
- `8s` / `4.2` — integer milliseconds only (e.g. `8000`)
- using it to TRIM the intro — that's `playback_start`; `intro_duration` only DESCRIBES the length

**Related.** [Outro Duration](outro_duration.md) · [Ending Type](ending_type.md) · [Playback Start (cue-in)](playback_start.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
