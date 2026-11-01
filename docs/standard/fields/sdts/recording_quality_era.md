# Recording Quality Era &nbsp;·&nbsp; `recording_quality_era`

> Rank duplicate versions and judge whether a lo-fi acoustic-era recording suits the room and PA.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md) · [Values](../../../model/vocabulary/recording_quality_era.md)

| | |
|---|---|
| **Type** | `enum` · single |
| **Values** | SDTS baseline vocabulary -> Recording Quality (canonical names; each value carries era_start/era_end + technology + frequency_range + character) |
| **Tag** | `TXXX:SDTS_RECORDING_QUALITY_ERA` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | active · Recording character · SDTS-defined |

The recording-technology window a track was captured in, which sets its inherent fidelity ceiling — frequency range, mono vs stereo, surface noise. It describes the source's sonic character, not how good a particular copy is; a clean reissue master of an old acoustic cut still carries acoustic-era character.

**Values.** Single enum value (cardinality 1, no delimiter). The value is a canonical recording-technology era name; each vocabulary value carries era_start/era_end, technology, frequency_range, and character attributes. Captures source-fidelity character, independent of release year (a modern reissue keeps its original-era character) and of recording_context. Value set: SDTS baseline vocabulary -> Recording Quality (not enumerated here).

**Examples.** ✓ `Acoustic Era` · `Electrical Mono` · `Hi-Fi Mono` · `Stereo Era` · `Digital Era`

**Avoid.**
- `Excellent` — that's a fidelity GRADE (a false friend), not a recording era
- `Hi-Fi Mono; Stereo Era` — single-valued; one era
- `Digital` — use the canonical `Digital Era`

**Notes.** Legacy on-disk mirror `TXXX:Quality` — kept for backward compatibility (see [Part 2 sync](../../../sync/tool-comparison.md)); NOT the canonical home.

**Related.** [Recording Origin](recording_origin.md) · [Recording Context](recording_context.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
