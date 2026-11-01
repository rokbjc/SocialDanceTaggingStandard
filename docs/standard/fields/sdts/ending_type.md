# Ending Type &nbsp;·&nbsp; `ending_type`

> Know the ending (definitive / fade / cold-stop) for tight transitions.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `enum` · single |
| **Values** | enum (controlled vocab) — how the track ends; derived from segments then human-confirmed |
| **Tag** | `TXXX:SDTS_ENDING_TYPE` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Structure & cueing · SDTS-defined |

How the track ends — a definitive resolved ending, a fade-out, or a cold stop. The genuinely distinct part of the old intro_outro composite: lets the editor flag fade/cold-stop endings that hurt social-dance flow and the player time transitions.

**Values.** One of: definitive (clear resolved ending) | fade (volume fade-out) | cold-stop (abrupt cut). Unset = unknown.

**Examples.** ✓ `definitive` · `fade` · `cold-stop`

**Avoid.**
- `fade-out` — the value is `fade` (one of definitive|fade|cold-stop)
- `big-finish` — use `definitive`

**Related.** [Outro Duration](outro_duration.md) · [Fade-out](fade_out_duration.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
