# Playback Stop (cue-out) &nbsp;·&nbsp; `playback_stop`

> Cut a fade-out or applause tail so the next track comes in tight.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `int (ms)` · single |
| **Values** | milliseconds from the file start (absent = play to the end); optionally a beat index via the SB-Beats grid |
| **Tag** | `TXXX:SDTS_PLAYBACK_STOP` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Structure & cueing · SDTS-defined |

The point (ms) where playback should stop — the cue-out. Cuts a long fade-out, an applause tail, or hidden-track silence so the next track comes in cleanly. Absent means play to the end. Pairs with playback_start as the portable equivalent of JRiver [Playback Range] / iTunes Stop Time.

**Values.** positive integer milliseconds; must be > playback_start and <= track length. Absent = play to the end.

**Examples.** ✓ `195000` · `201500`

**Avoid.**
- `195` — that's seconds; use ms (`195000`)
- a value ≤ playback_start — the cue-out must come after the cue-in
- using it as a resume/bookmark — it's a fixed cue-out, not a moving position

**Related.** [Playback Start (cue-in)](playback_start.md) · [Fade-out](fade_out_duration.md) · [Outro Duration](outro_duration.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
