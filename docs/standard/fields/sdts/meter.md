# Meter &nbsp;·&nbsp; `meter`

> Gate 3/4 waltz-family material apart from 4/4.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `enum (2 \| 3 \| 4 \| 2/4 \| 3/4 \| 4/4 \| 6/8 \| 12/8)` · single |
| **Values** | controlled enum — a full time signature OR a bare beats-per-bar grouping; owned by the SDTS baseline vocabulary; derivable from beat grouping then human-confirmed |
| **Tag** | `TXXX:SDTS_METER` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Tempo & groove · SDTS-defined |

The meter of the track — either the full time signature OR just the beats-per-bar grouping (2/3/4) when you do not know or do not care about the denominator. Makes it a hard gate: only a 3-feel (3, 3/4, 3/8) qualifies for Waltz (#WLZ) / Tango-Vals (#TGW); the rest is a 2- or 4-feel. Triplet vs straight (e.g. 12/8 vs 4/4) is rhythmic_feel's job.

**Values.** Either a full time signature (4/4 | 3/4 | 6/8 | 12/8 | 2/4) OR — when the denominator is unknown or irrelevant for dancing — just the beats-per-bar GROUPING (2 | 3 | 4). Both forms are valid and both are accepted; '3' covers the waltz family whether it is notated 3/4 or 3/8. The slash, where present, is literal (NOT a multi-value delimiter). Triplet/compound SUBDIVISION (e.g. 12/8 vs straight 4/4) is captured by rhythmic_feel, not here.

**Examples.** ✓ `4/4` · `3` · `3/4` · `12/8`

**Avoid.**
- `4` — write the full signature `4/4`
- `4/4; 3/4` — single-valued; the '/' is part of the value, not a delimiter
- `common` or `C` — use `4/4`

**Related.** [Song Form](song_form.md) · [Rhythmic Feel](rhythmic_feel.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
