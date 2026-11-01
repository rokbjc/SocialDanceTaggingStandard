# Theme

The **theme** vocabulary — each value's canonical name (what is stored), the aliases that resolve to it, and the axes that make it queryable.

→ Field reference: [Theme](../../standard/fields/sdts/theme.md) · [all dimensions](README.md) · [dances](dances.md)

Axes — **category** (seasonal, event, context).

| Value | Code | Aliases | Description |
|---|---|---|---|
| **Christmas** | `XMS` | `Xmas` · `Holiday-Christmas` | December holiday season — Christmas songs. |
| **Summer** | `SUM` |  | Summer vibes — beach, warm weather, summer events. |
| **Winter** | `WIN` |  | Winter mood (excluding Christmas — see the Christmas value). |
| **Spring** | `SPR` |  | Spring vibes. |
| **Autumn** | `AUT` | `Fall` | Autumn vibes. |
| **Holiday** | `HOL` | `Vacation` | Generic holiday season — not Christmas-specific. |
| **Halloween** | `HAL` | `Spooky` | Halloween season — spooky/haunting repertoire (October). Analogous to Christmas. |
| **Wedding** | `WED` | `Wedding Party` | Wedding event repertoire. |
| **Party** | `PTY` |  | Generic party / upbeat event context. |
| **Love** | `LOV` | `Romance` · `Romantic Theme` · `Romantic Songs` · `Love Song` | Love-themed / romantic songs. Distinct from Mood 'Tender' (energy=low, valence=bright); Theme=Love is about the lyric topic. NB: 'Romantic' in the Mood field is an alias for Tender; in Theme it is an alias for Love. |
| **Heartbreak** | `HRT` | `Breakup` · `Lost Love` · `Torch` | Heartbreak / breakup / lost-love lyric topic. The complement of the Love theme (happy love) — Heartbreak is sad/lost love. Typical of blues sets. Distinct from Mood 'Melancholic' (emotional fingerprint) — Heartbreak is about lyric content. |
| **Chillout** | `CHL` | `Chill-out` · `Lounge Mood` | Relaxation listening context (background, cocktail hour, lounge). Distinct from Mood 'Cool' (restrained jazz feel) — the Chillout theme is about the atmospheric setting, not the mood. |
| **Driving** | `DRV` | `Road Trip` · `In-Car` · `Travel` | In-car / road-trip listening context. Distinct from Mood 'Driving' (high/dark relentless motor) — Theme=Driving is about the listening situation (driving in a car); Mood=Driving is about the rhythmic feel. |
| **Workout** | `WKO` | `Gym` · `Exercise` · `Fitness` | Gym / exercise / running listening context. |
| **Cocktail** | `CKT` | `Cocktail Hour` · `Lounge` · `Aperitif` | Cocktail-hour ambient — pre-event background. |
| **Late Night** | `LTE` | `Late-Night` · `After Hours` | Late-night listening — intimate, low-energy, smoky. |

_Generated from [`data/vocabulary/`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/data/vocabulary) by `scripts/gen_vocab_docs.py` — do not hand-edit; edit `data/vocabulary/`, then re-run this._
