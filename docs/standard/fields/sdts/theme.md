# Theme (Listening / Event Context) &nbsp;·&nbsp; `theme`

> Pull a holiday / Valentine / Halloween / event set on demand.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md) · [Values](../../../model/vocabulary/theme.md)

| | |
|---|---|
| **Type** | `enum[]` · multi · delimiter `;` |
| **Values** | SDTS baseline vocabulary -> Themes (canonical bare-word values) |
| **Tag** | `TXXX:SDTS_THEME` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | active · Which dance, and when · SDTS-defined |

The listening, event, or seasonal occasion a track suits (Christmas, Cocktail, Heartbreak, Late Night) — the situation a DJ programs for, beyond the track's musical identity. Distinct from Mood, which is the emotional feel: Theme=Love is the lyric topic while Mood=Tender is the feel; Theme=Driving is in-car listening while Mood=Driving is rhythmic motor. Also distinct from the dance/occasion side — listening-context themes are not dance-floor markers.

**Values.** Multi-value enum, '; '-separated; resolved to canonical theme names. Bare words only (seasonal / listening / event context) — #-prefixed dance-floor markers are NOT themes and belong to the dance/occasion side. Distinct from Mood (TMOO emotional fingerprint): theme is the listening/event situation, not the feel. Value set: SDTS baseline vocabulary -> Themes (not enumerated here).

**Examples.** ✓ `Christmas` · `Cocktail; Late Night` · `Wedding` · `Heartbreak`

**Avoid.**
- `#JAM` — dance-floor `#`-markers moved off `theme` to the dance/occasion side
- `Xmas` — use the canonical `Christmas`
- `Tender` — that's a Mood, not a Theme

**Notes.** Legacy on-disk mirror `COMM:Songs-DB_Custom9` — kept for backward compatibility (see [Part 2 sync](../../../sync/tool-comparison.md)); NOT the canonical home.

**Related.** [Dance](dance.md) · [Mood](../id3/mood.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
