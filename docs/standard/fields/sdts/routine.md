# Routine &nbsp;·&nbsp; `routine`

> Pull THE Shim Sham / Big Apple / Tranky Doo track (original), with vetted alternatives and experimental options behind it.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md) · [Values](../../../model/vocabulary/routine.md)

| | |
|---|---|
| **Type** | `enum[]` · multi · delimiter `;` |
| **Values** | SDTS baseline vocabulary -> Routines (canonical names + aliases) |
| **Tag** | `TXXX:SDTS_ROUTINES` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Which dance, and when · SDTS-defined |

Names the choreographed line routines (Shim Sham, Tranky Doo, Big Apple, Jitterbug Stroll) that this exact recording serves, and how well it fits each. Genre, mood, BPM, and even the dance tag only say a track fits the style; this field says THIS specific recording is the one the routine is actually danced to — whether the original accompaniment, an accepted substitute, or an experimental stretch.

**Values.** Multi-value, '; '-delimited list of canonical routine names stored as plain canonical text with spaces; spaceless forms are aliases that resolve to the canonical name ('#'-prefixed forms are personal shorthand for a personal overlay, not baseline aliases). Each name carries an inline fitness tier ':original' / ':alternative' / ':experimental' (original = the canonical recording the routine is danced to; alternative = an accepted substitute; experimental = fits but demands more of the dancers) plus an optional trailing '?' meaning needs validation. One track may be the original for one routine and an alternative/experimental for another; consumers strip ':tier' and a trailing '?' to filter by routine. Pairs with the matching dance tag rather than replacing it. Value set: SDTS baseline vocabulary -> Routines (not enumerated here).

**Examples.** ✓ `Shim Sham:original` · `Shim Sham:alternative; Big Apple:experimental` · `Tranky Doo:original`

**Avoid.**
- `ShimSham:original` — write the canonical name WITH spaces: `Shim Sham:original` (spaceless / #-forms are aliases)
- `Shim Sham:primary` — the tier is one of original|alternative|experimental (the old primary/alternate/caller vocab is gone)
- `Shim Sham` — needs a fitness tier: `Shim Sham:original`
- `#ShimSham` — that's the dance tag in SDTS_DANCES; this field declares THE recording (`Shim Sham:original`)

**Related.** [Dance](dance.md) · [Song Form](song_form.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
