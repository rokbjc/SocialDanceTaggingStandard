# Playback Start (cue-in) &nbsp;·&nbsp; `playback_start`

> Skip a long talky / applause / rubato intro so the track drops straight into the groove.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `int (ms)` · single |
| **Values** | milliseconds from the file start (absent or 0 = play from the beginning); optionally a beat index via the SB-Beats grid |
| **Tag** | `TXXX:SDTS_PLAYBACK_START` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Structure & cueing · SDTS-defined |

The point (ms from the start of the file) where playback should begin — the cue-in. Trims dead air, a long count-in, or a rubato/spoken intro so the danceable part starts immediately. Absent or 0 means start at the beginning. Distinct from intro_duration, which only *describes* the intro length; playback_start is the actual instruction to skip it. (JRiver expresses the pair as one string field [Playback Range] = "cue-in - cue-out"; SDTS uses two scalars for easier validation.)

**Values.** non-negative integer milliseconds; must be < playback_stop and < track length. Absent/0 = start at 0:00.

**Examples.** ✓ `8000` · `0` · `12500`

**Avoid.**
- `8.0` or `8s` — integer milliseconds only (`8000`)
- a value ≥ playback_stop or ≥ track length — the cue-in must come before both
- a negative value — must be ≥ 0 (absent/0 = start at the beginning)

**Related.** [Playback Stop (cue-out)](playback_stop.md) · [Fade-in](fade_in_duration.md) · [Intro Duration](intro_duration.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
