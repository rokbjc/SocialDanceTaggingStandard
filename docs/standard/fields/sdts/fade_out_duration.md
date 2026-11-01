# Fade-out &nbsp;·&nbsp; `fade_out_duration`

> Tame a fade-ending or applause-tailed record with a clean fade.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `int (ms)` · single |
| **Values** | milliseconds — fade-out ramp length ENDING at playback_stop (0 or absent = cold stop) |
| **Tag** | `TXXX:SDTS_FADE_OUT_DURATION` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Structure & cueing · SDTS-defined |

Length of a fade-out ramp that ENDS at playback_stop (the cue-out). 0 or absent = cold stop. Your example — end at 200 s with a 2 s fade — is playback_stop=200000, fade_out_duration=2000 (clean, no '+'-as-arithmetic ambiguity). Curve not stored; player default equal-power.

**Values.** non-negative integer milliseconds; 0 = cold stop (no fade). Typically 0-8000.

**Examples.** ✓ `0` · `2000` · `4000`

**Avoid.**
- `200000+2000` — don't encode a composite; set playback_stop=200000 and fade_out_duration=2000 separately
- `4s` — integer ms (`4000`)
- a negative value — 0 = cold stop

**Related.** [Fade-in](fade_in_duration.md) · [Playback Stop (cue-out)](playback_stop.md) · [Ending Type](ending_type.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
