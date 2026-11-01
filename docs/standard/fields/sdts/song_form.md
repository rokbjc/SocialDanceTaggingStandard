# Song Form &nbsp;·&nbsp; `song_form`

> Know the harmonic form (AABA / 12-bar) for routine matching and musicality cueing.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md) · [Values](../../../model/vocabulary/song_form.md)

| | |
|---|---|
| **Type** | `enum` · single |
| **Values** | SDTS baseline vocabulary -> Song Forms (canonical names; each value carries bars/pattern/meter/tier) |
| **Tag** | `TXXX:SDTS_SONG_FORM` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | active · Structure & cueing · SDTS-defined |

The harmonic/structural form of a single chorus (12-bar blues, 32-bar AABA, multi-strain ragtime, etc.). This is the repeating chord-and-section template only — it does not capture where individual breaks or phrases land in this particular recording, nor any positional beat-by-beat annotation; those positional details are a separate, private concern.

**Values.** Single harmonic-form value (cardinality single; the '—' delimiter is irrelevant here as no multi-value lists are stored). Must be a canonical form name resolved against the vocabulary, which carries each value's bar count, section pattern, and implied meter; an 'Irregular' sentinel flags irregular phrasing. Harmonic form only — macro phrasing/breaks and section-by-section beat annotations live in the private SB-SongForm/SB-Beatinfos caches, not here. Value set: SDTS baseline vocabulary -> Song Forms (not enumerated here).

**Examples.** ✓ `12-bar Blues` · `AABA` · `ABAC` · `Ragtime` · `Rhythm Changes`

**Avoid.**
- `12-bar` — use the canonical `12-bar Blues`
- `AABA; 12-bar Blues` — single-valued; one harmonic form
- `Verse-Chorus` — not a catalog form value

**Notes.** Legacy on-disk mirror `COMM:Songs-DB_Custom6` — kept for backward compatibility (see [Part 2 sync](../../../sync/tool-comparison.md)); NOT the canonical home.

**Related.** [Meter](meter.md) · [Rhythmic Feel](rhythmic_feel.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
