# Recording Origin &nbsp;·&nbsp; `recording_origin`

> Choose period-original vs revival/cover vs reissue — Herrang wants vintage, a modern crowd may prefer a clean revival.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `enum` · single |
| **Values** | controlled enum (SDTS controlled vocabulary; new field — not yet in the baseline vocabulary; proposed). |
| **Tag** | `TXXX:SDTS_RECORDING_ORIGIN` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Recording character · SDTS-defined |

Where a recording sits on the authenticity/provenance axis relative to the music's own era — an original-era cut, a modern band playing the idiom period-correctly, or a re-release of an original. Deliberately orthogonal to the three sibling axes it is easily confused with: release year (when the music was written/popularized), recording quality / quality-era (how it sounds, e.g. Acoustic vs Digital Era), and recording context (the capture setting, Studio/Live/Broadcast). A 2015 digital-era faithful revival of a 1936 Lindy tune scores on all three independently, which is why provenance needs its own field.

**Values.** 3-value enum: period-original | faithful-revival | reissue (proposed). period-original = recorded in the music's own era; faithful-revival = a modern band playing the idiom period-correctly (e.g. NOLA Jazz Revival, Modern Swing Revival); reissue = a re-release/remaster of an original-era recording. Unset = unknown.

**Examples.** ✓ `period-original` · `faithful-revival` · `reissue`

**Avoid.**
- `original` — use the canonical `period-original`
- `period-original; reissue` — single-valued
- `Digital Era` — that's recording_quality_era, a different axis

**Related.** [Recording Quality Era](recording_quality_era.md) · [Recording Context](recording_context.md) · [Artist Country](artist_country.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
