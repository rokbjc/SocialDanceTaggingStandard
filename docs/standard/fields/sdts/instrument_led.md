# Instrument-Led &nbsp;·&nbsp; `instrument_led`

> Spot or avoid clustering similar-sounding lead instruments across a block.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md) · [Values](../../../model/vocabulary/instrument.md)

| | |
|---|---|
| **Type** | `enum[]` · multi · delimiter `;` |
| **Values** | SDTS baseline vocabulary -> Instrument-Led (canonical names + aliases) |
| **Tag** | `TXXX:SDTS_INSTRUMENT_LED` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | active · Sound & ensemble · SDTS-defined |

The instrument(s) that carry or feature on the track — the lead voice(s), not the full instrument roster. Name a specific instrument when one dominates, or a generic family when no single instrument leads.

**Values.** Multi-value enum; multiple canonical values joined with '; '. Each value sits on a two-tier axis — specific instruments and generic instrument-family groupings — and must resolve to a canonical vocabulary name (aliases accepted). Stored BARE: any ' Led' suffix is stripped on read (e.g. 'Piano Led' -> 'Piano'). Value set: SDTS baseline vocabulary -> Instrument-Led (not enumerated here).

**Examples.** ✓ `Piano` · `Saxophone` · `Clarinet` · `Reed` · `Trumpet; Trombone`

**Avoid.**
- `Piano Led` — store it BARE: `Piano` (the ' Led' suffix is stripped on read)
- `Saxophone, Trumpet` — wrong delimiter; `Saxophone; Trumpet`
- `Keys` — not a catalog value (use `Piano`, or the family `Novelty`)

**Notes.** Legacy on-disk mirror `COMM:Songs-DB_Custom7` — kept for backward compatibility (see [Part 2 sync](../../../sync/tool-comparison.md)); NOT the canonical home.

**Related.** [Instrumentation (Ensemble)](instrumentation.md) · [Vocality](vocality.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
