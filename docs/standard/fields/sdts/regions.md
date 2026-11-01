# Regions &nbsp;·&nbsp; `regions`

> Trim talky intros, mark silence gaps, flag music-vs-speech boundaries — durable across machines.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `region-list` · multi · delimiter `;` |
| **Values** | human-marked typed regions (start/end ms + kind); durable in-tag, not session-scoped |
| **Tag** | `TXXX:SDTS_REGIONS` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Structure & cueing · SDTS-defined |

Manually-marked typed regions on a track — leading/trailing silence, spoken/speech passages, and music-boundary marks — persisted durably in-tag so they survive across sessions and machines. Each region is a [start_ms, end_ms) span with a kind (silence/speech/music-boundary). Distinct from playback_start/stop (single cue points) and intro/outro_duration (scalar lengths): regions is the general list of typed spans.

**Values.** An ordered '; '-joined list of typed regions, each 'start_ms:end_ms:kind' (two integers + a kind token). kind is a closed set: silence | speech | music-boundary. No JSON (per the SDTS no-JSON-in-tags rule). Per-region source/confidence are a reserved forward-compatible extension (trailing ':'-delimited fields).

**Examples.** ✓ `0:3200:silence; 3200:8500:speech` · `185000:190000:music-boundary`

**Avoid.**
- `0:3200` — every region needs a kind: `0:3200:silence`
- `0:3200:noise` — kind is a closed set: silence | speech | music-boundary
- `0.5:3200:silence` — the bounds are integer milliseconds, not seconds or fractions
- `3200:0:silence` — end must come after start (the per-value validator can't catch this; a consumer must)
- `{"start":0,"end":3200}` — no JSON in tags; regions is a `; `-joined list of `start:end:kind`
- a single cue point squeezed in as a zero-length region — that's `playback_start` / `playback_stop`

**Related.** [Playback Start (cue-in)](playback_start.md) · [Playback Stop (cue-out)](playback_stop.md) · [Intro Duration](intro_duration.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
