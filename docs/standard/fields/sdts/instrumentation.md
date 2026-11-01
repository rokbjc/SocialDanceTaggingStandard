# Instrumentation (Ensemble) &nbsp;·&nbsp; `instrumentation`

> Balance big-band 'meat' against small-combo texture and alternate ensemble size for variety.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md) · [Values](../../../model/vocabulary/instrumentation.md)

| | |
|---|---|
| **Type** | `enum` · single |
| **Values** | SDTS baseline vocabulary -> Instrumentation/Ensemble (canonical ensemble forms; carries a 'size' axis per value) |
| **Tag** | `TXXX:SDTS_INSTRUMENTATION` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | active · Sound & ensemble · SDTS-defined |

The performing-ensemble format of the recording — its size tier (solo through large) and idiom. Distinct from instrument_led (which lead voice carries the track) and vocality (presence and gender of vocals): instrumentation is about the shape and weight of the whole group, not who fronts it or whether anyone sings.

**Values.** Single-value enum: exactly one canonical ensemble form per track (no list, so the field-level delimiter does not apply). Value must resolve to a canonical name from the controlled vocabulary; each value carries a vocabulary-defined 'size' axis (solo through large) so DJs can filter by ensemble size independently of genre. Distinct from instrument_led (lead voice) and vocality (vocal presence/gender). Value set: SDTS baseline vocabulary -> Instrumentation/Ensemble (not enumerated here).

**Examples.** ✓ `Big Band` · `Jazz Combo` · `Solo Piano` · `Vocal + Piano` · `Manouche`

**Avoid.**
- `Saxophone` — that's a lead instrument (instrument_led), not an ensemble; use e.g. `Jazz Combo`
- `Big Band; Orchestra` — single-valued; pick one
- `Quartet` — not a catalog ensemble value

**Notes.** Legacy on-disk mirror `COMM:Songs-DB_Custom5` — kept for backward compatibility (see [Part 2 sync](../../../sync/tool-comparison.md)); NOT the canonical home.

**Related.** [Instrument-Led](instrument_led.md) · [Vocality](vocality.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
