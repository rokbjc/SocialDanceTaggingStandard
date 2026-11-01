# Genre &nbsp;·&nbsp; `genre`

> Reason about a track's era/character, seed dance + mood expectations, and build style-coherent or deliberately-contrasting blocks.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md) · [Values](../../../model/vocabulary/genre.md)

| | |
|---|---|
| **Type** | `enum[]` · multi · delimiter `;` |
| **Values** | SDTS baseline vocabulary -> Music Genres (canonical names + aliases; per-value family/parent hierarchy + dance_relevance grade) |
| **Tag** | `TCON` (ID3 standard frame — canonical home) |
| **Status** | active · Sound & ensemble · ID3 standard |

The musicological style(s) of the recording — the coarse musical bucket a DJ reasons about before tempo and feel. It captures what the music IS, not whether it is danceable: the same Swing record may suit Lindy, Balboa, or neither depending on the tempo and feel fields, so genre seeds dance and era expectations but never decides them alone.

**Values.** Closed enum, multi-value ('; '-separated), stored as plain canonical text (never ID3v1 numeric codes). Non-canonical synonyms resolve to the canonical vocabulary name via the aliases frontmatter; do not stack a parent and its child in one field (hierarchy dedup -> keep the more specific). Each value carries vocabulary metadata (family/parent, era, dance_relevance grade, bpm_range) used to demote non-floor jazz. Value set: SDTS baseline vocabulary -> Music Genres (not enumerated here).

**Examples.** ✓ `Swing; Big Band` · `Chicago Blues` · `Gypsy Jazz` · `Jump Blues; Boogie Woogie` · `Vocal Jazz`

**Avoid.**
- `Swing/Big Band` — use `; ` not `/` → `Swing; Big Band`
- `Swing; Hot Jazz; NOLA Jazz` — don't stack a parent and its child; keep the most specific
- `13` — never ID3v1 numeric genre codes; store canonical text

**Related.** [Mood](mood.md) · [Rhythmic Feel](../sdts/rhythmic_feel.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
