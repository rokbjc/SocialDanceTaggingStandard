# Dance &nbsp;·&nbsp; `dance`

> Filter the library into per-dance pools and build for the room in front of you — 'give me Lindy at 180', 'a blues set'.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md) · [Values](../../../model/vocabulary/dances.md)

| | |
|---|---|
| **Type** | `enum[]` · multi · delimiter `;` |
| **Values** | SDTS baseline vocabulary -> Dance Styles (canonical names + aliases) |
| **Tag** | `TXXX:SDTS_DANCES` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | active · Which dance, and when · SDTS-defined |

Which social dances a track serves on the floor — the danceability axis, distinct from the musical 'genre' it belongs to and from the listening-situation 'theme'. A track may serve several dances at once, and order carries meaning: the first entry is the primary/headline dance and the entry count is the track's versatility, which is why there is no separate primary_dance or dance_versatility field. (The legacy MediaMonkey 'Occasion' column is a misnomer for this field, not a separate concept.)

**Values.** Multi-value list of canonical dance NAMES (plain canonical text, '; '-delimited), same model as genre: short forms, bare display codes and alternate spellings are ALIASES that resolve to the canonical name. Legacy '#'-prefixed codes are one DJ's shorthand and resolve only through a personal overlay (e.g. data/vocabulary/examples/legacy-hash-codes.sdvo.yaml), never through the baseline. ORDER IS SIGNIFICANT — the first entry is the primary/headline dance and the entry count is the track's dance-versatility, so there is no separate primary_dance or dance_versatility field. PER-NAME NOTATION: each name may carry an optional idiom-purity grade ':core'/':edge'/':fusion' on one generic scale (canonical -> boundary/crossover -> modern/contested; deviation direction lives in 'genre') plus an optional trailing '?' (= needs validation, pairs with confirmation_gates); consumers strip the ':grade' and trailing '?' to filter by dance, absent purity = unspecified/core. Display forms — a bare short code, an emoji, an Iconify icon and a suggested hex color — are SDTS-authored per-term metadata in each term's display block, not a standard tag mechanism; the legacy fitness suffix ('!'/'!!') and '!CC' AI marker are NOT part of the standard. Dances form a strict tree through the single-valued `parent` attribute: a group value (Swing, Ballroom Standard, Ballroom Latin, Street Latin, Argentine Tango Family; Blues for its idioms) names a set of dances, has no steps of its own and is taggable; normalisation keeps the most specific value when a track carries a group and one of its dances. A dance's tempo band is an advisory typical band for a social floor, never a hard limit (spec: Tempo bands). Value set: SDTS baseline vocabulary -> Dance Styles (not enumerated here).

**Examples.** ✓ `Lindy Hop` · `Lindy Hop; Balboa` · `Blues; Slow Drag` · `Blues:fusion` · `Lindy Hop:core?` · `East Coast Swing:edge`

**Avoid.**
- `#LH` as the STORED value — the canonical form is the name `Lindy Hop`; `#LH` is an alias (genre model)
- `Lindy Hop, Balboa` — wrong delimiter; multi-value uses `; ` → `Lindy Hop; Balboa`
- a name that is neither a catalog Dance Style canonical name nor an alias of one
- `Blues:traditional` — the purity grade is `core`/`edge`/`fusion`, not free-text
- migrating a `#`-code by just dropping the `#` — the two namespaces are NOT parallel: `#SLS` is an alias of **Salsa**, while bare `SLS` is the display code of **St. Louis Shag**. Resolve through the vocabulary; never rewrite the token by hand
- purity stored in a separate field — it rides INLINE on the dance name (`Blues:fusion`), because purity is per-(track×dance)

**Notes.** Legacy on-disk mirror `COMM:Songs-DB_Occasion` — kept for backward compatibility (see [Part 2 sync](../../../sync/tool-comparison.md)); NOT the canonical home.

**Related.** [Theme (Listening / Event Context)](theme.md) · [Routine](routine.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
