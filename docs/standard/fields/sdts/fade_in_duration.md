# Fade-in &nbsp;·&nbsp; `fade_in_duration`

> Ease a track in from its cue-in instead of a hard entry.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `int (ms)` · single |
| **Values** | milliseconds — fade-in ramp length starting at playback_start (0 or absent = hard/instant entry) |
| **Tag** | `TXXX:SDTS_FADE_IN_DURATION` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Structure & cueing · SDTS-defined |

Length of a fade-in ramp applied from playback_start (the cue-in). 0 or absent = hard entry (no fade). Lets a track ease in from its chosen cue point instead of jumping in. The fade CURVE is not stored — the player picks it (default equal-power); add a fade_curve field only if some track needs a non-default shape.

**Values.** non-negative integer milliseconds; 0 = no fade (hard in). Typically 0-4000.

**Examples.** ✓ `0` · `1500` · `2000`

**Avoid.**
- `2s` or `2.0` — integer milliseconds (`2000`)
- a curve name like `equal-power` — the curve isn't stored; this is only a duration
- a negative value — 0 = hard in (no fade)

**Related.** [Fade-out](fade_out_duration.md) · [Playback Start (cue-in)](playback_start.md) · [Intro Duration](intro_duration.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
