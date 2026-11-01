# Rhythmic Feel &nbsp;·&nbsp; `rhythmic_feel`

> Hold or deliberately switch the groove (shuffle vs straight vs slow-drag) — the main blues flow lever beyond tempo.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md) · [Values](../../../model/vocabulary/rhythmic_feel.md)

| | |
|---|---|
| **Type** | `enum` · single |
| **Values** | SDTS baseline vocabulary -> Rhythmic Feels (canonical names + aliases) |
| **Tag** | `TXXX:SDTS_RHYTHMIC_FEEL` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | active · Tempo & groove · SDTS-defined |

The dominant rhythmic groove a dancer locks into — the pre-tempo axis (swung vs straight vs shuffle vs slow-drag) that separates two records at the same BPM and genre into different dances and floor energies. It is finer-grained than meter: meter only gates the beats-per-bar grouping, while feel captures the subdivision and accent character within that grouping.

**Values.** Single value (cardinality single) — one canonical Rhythmic Feel name (or alias resolving to one) from the controlled enum; resolve aliases to the canonical name before writing. Each value carries structured axes — pulse (2|3|4|mixed), subdivision (straight|triplet|bass-feel|mixed), accent (on-beat|off-beat|backbeat|clave|none). (`tier` ranks a feel inside its cluster, 1 = the general feel; it is absent for the 13 Latin, Brazilian, tango, march and waltz feels that have no general parent in their cluster, and those are ranked per dance scene through feel_scenes.yaml instead, so a single global rank never has to be swing-centric.) Beware name collisions: the 'Stride' feel vs the blues 'Stride' idiom, and the 'Slow Drag' feel vs the '#SD' Slow Drag occasion. Value set: SDTS baseline vocabulary -> Rhythmic Feels (not enumerated here).

**Examples.** ✓ `Jazz Swing 8ths` · `Shuffle` · `Slow Drag` · `Stride` · `Manouche Swing`

**Avoid.**
- `Swing` — too vague; use a catalog feel like `Jazz Swing 8ths`
- `Jazz Swing 8ths; Shuffle` — single-valued; one primary feel
- `Stride` meant as the blues idiom — beware the name collision; here it is the rhythmic feel

**Notes.** Legacy on-disk mirror `COMM:Songs-DB_Custom4` — kept for backward compatibility (see [Part 2 sync](../../../sync/tool-comparison.md)); NOT the canonical home.

**Related.** [Genre](../id3/genre.md) · [Song Form](song_form.md) · [Tempo Stability](tempo_stability.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
