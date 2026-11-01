# Vocality &nbsp;·&nbsp; `vocality`

> Alternate instrumental vs vocal and male vs female leads for variety; pull instrumentals for classes/jams or vocals to draw the crowd.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md) · [Values](../../../model/vocabulary/vocality.md)

| | |
|---|---|
| **Type** | `enum` · single |
| **Values** | SDTS baseline vocabulary -> Vocality (canonical names + aliases) |
| **Tag** | `TXXX:SDTS_VOCALITY` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | active · Sound & ensemble · SDTS-defined |

Captures whether anyone sings and in what role — instrumental versus a lead singer, an ensemble vocal, an occasional vocal feature over an otherwise instrumental track, or non-lyric voice like scat or spoken word — including the singer's gender where it applies. It describes the presence and character of the voice, not the words sung (lyric language is a separate field).

**Values.** Single canonical value per track (enum, cardinality single). Must resolve to a canonical Vocality name; one designated generic value serves as the fallback for mixed/uncertain tracks. Legacy free-text comment-tag forms (e.g. 'Vocal Male' / 'VM' / '(vm)') resolve to canonical values via aliases. Value set: SDTS baseline vocabulary -> Vocality (not enumerated here).

**Examples.** ✓ `Instrumental` · `Female` · `Male` · `Group` · `Feature`

**Avoid.**
- `Instrumental; Male` — single-valued; pick the one dominant vocality
- `(vm)` — legacy comment form; it resolves to `Male`, don't store the raw token
- `Vocals` — not canonical; use `Vocal` as the generic fallback

**Notes.** Legacy on-disk mirror `COMM:Songs-DB_Custom3` — kept for backward compatibility (see [Part 2 sync](../../../sync/tool-comparison.md)); NOT the canonical home.

**Related.** [Lyric Language](../id3/lyric_language.md) · [Instrument-Led](instrument_led.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
