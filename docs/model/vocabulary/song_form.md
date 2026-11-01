# Song Form

The **song form** vocabulary — each value's canonical name (what is stored), the aliases that resolve to it, and the axes that make it queryable.

→ Field reference: [Song Form](../../standard/fields/sdts/song_form.md) · [all dimensions](README.md) · [dances](dances.md)

Axes — **bars** (8, 12, 16, 24, 28, 32, 48, 64); **tier** ([1](#ax-tier-1), [2](#ax-tier-2), [3](#ax-tier-3)); **meter** ([4/4](#ax-meter-4-4), [2/4](#ax-meter-2-4), [12/8](#ax-meter-12-8)).

Grouped by **tier**.

## Tier 1 — foundational

<a id="12-bar-blues"></a>

### 🎺 12-bar Blues `B12`

bars: 12 · pattern: A4+A4+B4 · meter: [4/4](#ax-meter-4-4) · tier: [1](#ax-tier-1)

A 12-bar blues chorus built on a I-IV-V progression, split into three 4-bar phrases following an A-A'-B (statement, varied repeat, answer) pattern that repeats throughout the song. By far the most common form in blues, jump blues, boogie woogie and R&B; distinguished from other blues forms purely by its 12-bar length (vs 8, 16 or 24).

<sub>Aliases: Blues12 · 12 Bar Blues · Blues 12 · Twelve-bar Blues</sub>

<a id="bar-form"></a>

### 📜 Bar Form `AAB`

pattern: Verse + Verse + Chorus (at the section level) · meter: [4/4](#ax-meter-4-4) · tier: [1](#ax-tier-1)

A section-level AAB structure (Stollen-Stollen-Abgesang): two similar verses followed by a contrasting closing chorus, the whole pattern often repeating across the song. Unlike 12-bar Blues, the AAB here operates at the section level (verse + verse + chorus, 16+ bars each) rather than the bar level. Its roots are medieval German Meistersang, and it shapes many Lutheran hymns and the Prize Song in Wagner's Die Meistersinger.

<sub>Aliases: Stollen-Stollen-Abgesang · VVC · Verse-Verse-Chorus · AAB Section Form</sub>

<a id="aaba"></a>

### 🎼 AABA `AABA`

bars: 32 · pattern: A8+A8+B8+A8 · meter: [4/4](#ax-meter-4-4) · tier: [1](#ax-tier-1)

The classic Tin Pan Alley / Great American Songbook form: 32 bars in four 8-bar sections (A-A-B-A), where the B 'bridge' contrasts harmonically and melodically and the final A recapitulates the opening. The defining trait is that the fourth section returns to the first; if it does not, it is ABAC instead.

<sub>Aliases: 32-bar AABA · 32 Bar AABA · Standard AABA</sub>

<a id="abac"></a>

### 🎶 ABAC `ABAC`

bars: 32 · pattern: A8+B8+A8+C8 · meter: [4/4](#ax-meter-4-4) · tier: [1](#ax-tier-1)

A 32-bar form in four 8-bar sections (A-B-A-C) where, unlike AABA, the final section (C) introduces new material rather than recapitulating the opening A. The primary distinguishing cue: after the second A, the melody does NOT return but resolves with something new. It is common among standards written for Hollywood films.

<sub>Aliases: 32-bar ABAC · 32 Bar ABAC</sub>

<a id="double-16"></a>

### 🪞 Double 16 `D16`

bars: 32 · pattern: A16+B16 · meter: [4/4](#ax-meter-4-4) · tier: [1](#ax-tier-1)

A 32-bar form built as two equal 16-bar sections (A16+B16) with no internal 8-bar subdivisions; the melodic phrase runs a full 16 bars without repeating, giving a 'long-flow' feel. It is the most ambiguous 32-bar form, since many standards (e.g. 'All of Me') can equally be heard as AABA or ABAC at the 8-bar level.

<sub>Aliases: 32-bar Double16 · 32 Bar Double16 · AB Double16 · Double16</sub>

<a id="ragtime"></a>

### 🎹 Ragtime `RAG`

bars: 64 · pattern: AABBACCDD or AABBCCDD · meter: [2/4](#ax-meter-2-4) · tier: [1](#ax-tier-1)

A multi-strain march-derived form with several distinct 16-bar themes ('strains'), typically AABBACCDD or AABBCCDD, often with an intro and transition bridges and a 'trio' strain modulating to the subdominant. Distinguished from cyclic forms by the fact that the melody keeps moving to brand-new strains instead of returning (and from a medley, since it is one composition).

<sub>Aliases: Multi-strain Ragtime · Multi-strain · Ragtime Multi-strain</sub>

## Tier 2 — extended & variant

<a id="8-bar-blues"></a>

### 🎸 8-bar Blues `B8`

bars: 8 · pattern: I-V-IV-I · meter: [4/4](#ax-meter-4-4) · tier: [2](#ax-tier-2)

A compressed 8-bar blues that keeps the I-IV-V harmonic basis of 12-bar blues in just 8 bars, common in early country/Delta blues. Distinguished from 12-bar Blues purely by its shorter 8-bar chorus that comes back around noticeably faster.

<sub>Aliases: Blues8 · 8 Bar Blues · Blues 8</sub>

<a id="16-bar-blues"></a>

### 🎵 16-bar Blues `B16`

bars: 16 · pattern: AAAB or extended 12-bar · meter: [4/4](#ax-meter-4-4) · tier: [2](#ax-tier-2)

An extended major-key blues of 16 bars, usually heard as a 12-bar progression with an added 4-bar section or as an AAAB structure (three repeats plus an answer). Distinguished from 12-bar Blues by its longer chorus, and from 16-bar AB by its blues (I-IV-V) harmony rather than a folk A8+B8.

<sub>Aliases: Blues16 · 16 Bar Blues · Blues 16 · Major Blues 16</sub>

<a id="16-bar-minor-blues"></a>

### 🌑 16-bar Minor Blues `B16m`

bars: 16 · pattern: i-iv-V7 · meter: [4/4](#ax-meter-4-4) · tier: [2](#ax-tier-2)

A 16-bar form in a minor key using an i-iv-V7 cycle, most iconic in gypsy jazz / manouche (Django Reinhardt's 'Minor Swing' is the prototype), often in four 4-bar sections. The minor key (i-iv-V7) is the key distinguisher from the major-key 16-bar Blues (I-IV-V).

<sub>Aliases: Blues16Minor · Minor Blues 16 · 16-bar Minor</sub>

<a id="ab"></a>

### 🔁 AB `AB`

bars: 16 · pattern: A8+B8 · meter: [4/4](#ax-meter-4-4) · tier: [2](#ax-tier-2)

A short 16-bar form of two contrasting 8-bar sections (A8+B8) with no recapitulation, common in folk, hymns and children's songs but rare in jazz. Critically, it must NOT be confused with 32-bar Double16: this is the short A8+B8 form, not the long A16+B16.

<sub>Aliases: 16-bar AB · 16 Bar AB · AB 16</sub>

<a id="aabb"></a>

### 🥁 AABB `AABB`

bars: 32 · pattern: A8+A8+B8+B8 · meter: [4/4](#ax-meter-4-4) · tier: [2](#ax-tier-2)

A 32-bar double-strain form (A8+A8+B8+B8): two themes, each repeated, with no bridge or recapitulation, rooted in marching-band, folk, Celtic and western-swing traditions. Distinguished from AABA by the absence of a contrasting bridge/return — the last section is the repeated B, not a return to A.

<sub>Aliases: 32-bar AABB · 32 Bar AABB</sub>

<a id="rhythm-changes"></a>

### 🌀 Rhythm Changes `RC`

bars: 32 · pattern: AABA with Gershwin harmony · meter: [4/4](#ax-meter-4-4) · tier: [2](#ax-tier-2)

A variant of 32-bar AABA fixed to the harmony of Gershwin's 'I Got Rhythm' (I-vi-ii-V turnarounds in the A sections, a cycle-of-fifths bridge), used by the bebop era as the basis for countless contrafacts. Structurally identical to AABA, it is identified by that specific harmony rather than by its shape; when unsure, tag plain AABA.

<sub>Aliases: Rhythm Changes 32 · I Got Rhythm</sub>

<a id="aaba-with-verse"></a>

### 🎤 AABA with Verse `AABAv`

bars: 48 · pattern: 16 verse + 32 AABA chorus · meter: [4/4](#ax-meter-4-4) · tier: [2](#ax-tier-2)

An older Tin Pan Alley convention: a 16-bar narrative 'introductory verse' (often rubato, played only once) preceding a standard 32-bar AABA refrain, for roughly 48 bars total. Distinguished from plain AABA by that one-time sectional verse — not the repeating verse of modern pop, and not a mere band intro.

<sub>Aliases: AABAWithVerse · AABA + Verse · AABA w/ Verse</sub>

<a id="blues-with-verse"></a>

### 🗣️ Blues with Verse `BvV`

bars: 28 · pattern: 16 verse + 12 AAB blues chorus · meter: [4/4](#ax-meter-4-4) · tier: [2](#ax-tier-2)

A compound race-records form: a 16-bar introductory verse played once, followed by a repeating 12-bar AAB blues chorus (roughly 28 bars across the opening). It is the blues analogue of AABA-with-Verse, distinguished from plain 12-bar Blues by the one-time verse and from AABA-with-Verse by its 12-bar blues refrain.

<sub>Aliases: BluesWithVerse · Blues + Verse · Verse Blues</sub>

## Tier 3 — rare / edge-case

<a id="24-bar-blues"></a>

### 📏 24-bar Blues `B24`

bars: 24 · pattern: extended 12-bar · meter: [4/4](#ax-meter-4-4) · tier: [3](#ax-tier-3)

A rare, largely theoretical extended blues of 24 bars, understood either as a doubled/expanded 12-bar cycle (bebop-style, with secondary dominants) or as two 12-bar choruses joined into one macro-chorus. Distinguished from 12-bar Blues by a chorus cycle that returns roughly twice as slowly; nearly absent from real-world repertoire.

<sub>Aliases: Blues24 · 24 Bar Blues · Blues 24</sub>

<a id="abcd"></a>

### 🔤 ABCD `ABCD`

bars: 32 · pattern: A8+B8+C8+D8 · meter: [4/4](#ax-meter-4-4) · tier: [3](#ax-tier-3)

A through-composed 32-bar form of four distinct, non-repeating 8-bar sections (A-B-C-D), often modulating without returning to the home key. Distinguished from AABA/ABAC by the fact that all four sections sound different with no recapitulation; genuinely rare, as most apparent ABCD songs are really ABAC.

<sub>Aliases: 32-bar ABCD · Through-composed 32</sub>

<a id="irregular"></a>

### 🌊 Irregular `IRR`

bars: 0 · pattern: varies · meter: varies · tier: [3](#ax-tier-3)

A sentinel catch-all for songs that fit no standard pattern — odd chorus lengths (28, 36, 40 bars), through-composed structures, or odd meters like 5/4, 7/4 and 9/8 (e.g. 'Take Five', 'Blue Rondo a la Turk'). Use only after ruling out miscounting, extended variants of standard forms, compound forms and medleys; bars=0 means 'variable', not literally zero.

<sub>Aliases: Non-standard · Custom Form · Through-composed</sub>

## Axis reference

**Meter**

- <a id="ax-meter-2-4"></a>`2/4` — Two beats per bar, each a quarter note — a tight, two-count grouping with a strong downbeat (polka, habanera, some tango).
- <a id="ax-meter-3-4"></a>`3/4` — Three beats per bar, each a quarter note — the waltz grouping, with a clear strong-weak-weak lilt.
- <a id="ax-meter-4-4"></a>`4/4` — Four beats per bar, each a quarter note — the most common social-dance meter, counted in even fours.
- <a id="ax-meter-6-8"></a>`6/8` — Six eighth-notes per bar grouped into two big beats of three, giving a rolling compound-duple lilt.
- <a id="ax-meter-12-8"></a>`12/8` — Twelve eighth-notes per bar grouped into four big beats of three — four main beats over an underlying triplet pulse.

**Tier**

- <a id="ax-tier-1"></a>`1` — Foundational and common — the everyday core values most tracks and most DJs will use.
- <a id="ax-tier-2"></a>`2` — Extended and secondary — values that appear regularly but are less central than the foundational set.
- <a id="ax-tier-3"></a>`3` — Niche and rare — specialist values that turn up only in uncommon tracks or edge cases.

_Generated from [`data/vocabulary/`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/data/vocabulary) by `scripts/gen_vocab_docs.py` — do not hand-edit; edit `data/vocabulary/`, then re-run this._
