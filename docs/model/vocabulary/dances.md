# Dances

Which social dances each track serves — the danceability axis. Tempo bands are the DJ's first filter; a track fits a dance only inside its band **and** with the right feel.

→ Field reference: [Dance](../../standard/fields/sdts/dance.md) · the [music & dance model](../index.md)

## How the dance vocabulary is organised

Dances are partitioned into **packs** — one per dance scene (Swing, Blues, …) — so a library only loads the floors it actually plays. Within a pack, dances are grouped into **families** of close relatives (e.g. the swing-partner family holds Lindy Hop, Balboa, the Shags). A dance carries a **tempo band** (its danceable BPM range) and its **typical genres** — the two filters a DJ reaches for first: a track fits a dance only inside its band *and* with the right feel and genre.

## Packs

- **[Swing](dances/swing.md)** — Swing-era partner, solo-rhythmic, and boogie/rock'n'roll dances.
- **[Blues](dances/blues.md)** — Blues partner dances and the blues idioms (jookin' and ballroomin').
- **[Tango (Argentine)](dances/tango.md)** [⚠](dances/tango.md#expert-review-pending "Expert review pending") — Argentine tango social dances — tango de salón, vals, milonga, canyengue.
- **[Ballroom (International)](dances/ballroom.md)** [⚠](dances/ballroom.md#expert-review-pending "Expert review pending") — International DanceSport ballroom dances — Standard (Waltz, Tango, Viennese, Foxtrot, Quickstep) and Latin (Cha-Cha, Samba, Rumba, Paso Doble, Jive), per WDSF/WDC.
- **[Street Latin](dances/street-latin.md)** [⚠](dances/street-latin.md#expert-review-pending "Expert review pending") — Social / street Latin dances — Salsa, Bachata, Merengue, Kizomba, Zouk, Son Cubano. Distinct from the ballroom pack's codified International Latin.
- **[West Coast Swing](dances/west-coast-swing.md)** [⚠](dances/west-coast-swing.md#expert-review-pending "Expert review pending") — West Coast Swing — the slotted, swing-descended partner dance of its own contemporary-music scene.
- **[Carolina Shag](dances/carolina-shag.md)** [⚠](dances/carolina-shag.md#expert-review-pending "Expert review pending") — Carolina Shag — the slotted beach-music partner dance of the Carolina coast.
- **[Early Social Dances](dances/early.md)** [⚠](dances/early.md#expert-review-pending "Expert review pending") — The pre-swing social couple dances of the 1900s-1920s that DJs still play for vintage and historical dance floors — One-Step, Polka and the social Fox Trot.

Deferred dance families (not yet built): Niche.

_Generated from [`data/vocabulary/`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/data/vocabulary) by `scripts/gen_vocab_docs.py` — do not hand-edit; edit `data/vocabulary/`, then re-run this._
