# Instrumentation

The **instrumentation** vocabulary — each value's canonical name (what is stored), the aliases that resolve to it, and the axes that make it queryable.

→ Field reference: [Instrumentation](../../standard/fields/sdts/instrumentation.md) · [all dimensions](README.md) · [dances](dances.md)

Axes — **size** ([solo](#ax-size-solo), [duo](#ax-size-duo), [trio](#ax-size-trio), [small](#ax-size-small), [medium](#ax-size-medium), [large](#ax-size-large)).

Grouped by **size**.

## Solo ensembles

<a id="solo-piano"></a>

### 🎹 Solo Piano `SoP`

size: [solo](#ax-size-solo)

A single pianist covering every role from one instrument: the left hand is the rhythm section (bass plus chords), the right hand carries melody and improvisation, spanning composed ragtime, improvised stride, and driving boogie-woogie. Distinguished by the left-hand style (ragtime oom-pah march, stride's wide bass-to-chord leaps, boogie's relentless eight-to-the-bar ostinato).

<sub>Aliases: Piano Solo</sub>

<a id="solo-blues-performer"></a>

### 🎸 Solo Blues Performer `SoB`

size: [solo](#ax-size-solo)

One person with a guitar and voice (often open-tuning slide/bottleneck plus a moaning, growling vocal) — the most intimate form of blues, with no band and no production. Distinguished by its non-metronomic, breathing 'human' rhythm and slide guitar that imitates the voice; unlike Vocal + Piano, the single performer plays and sings simultaneously rather than being two musicians.

<sub>Aliases: Solo Blues · Solo Blues Singer · Solo Blues Guitar</sub>

<a id="solo"></a>

### 👤 Solo `SOL`

size: [solo](#ax-size-solo)

A single performer of any kind — the generic catch-all when one musician carries the whole track but the instrument is unspecified or not piano/guitar.

**Distinguishing trait:** use this only when the more specific Solo Piano or Solo Blues Performer does not apply; it asserts ensemble size of one without naming the instrument.

<sub>Aliases: Solo Performer</sub>

## Duo ensembles

<a id="vocal-piano"></a>

### 🎤 Vocal + Piano `VoP`

size: [duo](#ax-size-duo)

One singer plus one pianist — the smallest ensemble that still gives the vocal full harmonic and rhythmic support, with the piano covering bass, chords, fills, and solos so the singer has expressive freedom. Distinguished by maximum intimacy and a sparse palette where every word and keystroke is audible; unlike Solo Blues Performer it is two distinct musicians, and unlike Jazz Trio it has no bass or drums.

<sub>Aliases: Vocal and Piano · Voice and Piano · Vocal Piano Duo</sub>

<a id="duo"></a>

### 👥 Duo `Duo`

size: [duo](#ax-size-duo)

Any two-musician pairing — the generic two-piece when the specific instrument combination is unspecified or does not fit Vocal + Piano or Blues Duo.

**Distinguishing trait:** use this only when the more specific duo values do not apply; it asserts an ensemble of two without naming the instruments or idiom.

<sub>Aliases: Duet Ensemble</sub>

<a id="blues-duo"></a>

### 🎶 Blues Duo `BlD`

size: [duo](#ax-size-duo)

Two musicians playing blues, typically a guitarist and a second guitar, harmonica, or voice in the country-blues tradition.

**Distinguishing trait:** larger than the lone Solo Blues Performer (two distinct players) but smaller than the rhythm-section Blues Band, with an acoustic, intimate feel and no drums.

<sub>Aliases: Blues Duet</sub>

## Trio ensembles

<a id="jazz-trio"></a>

### 🎼 Jazz Trio `JzT`

size: [trio](#ax-size-trio)

Piano plus double bass plus drums — the most intimate jazz ensemble that still carries a rhythm section, serving either as backing for a vocalist or as a standalone group. Distinguished by the 'air' and space that a big band cannot offer, supporting a soloist or singer without competing with the voice.

<sub>Aliases: Piano Trio · Trio</sub>

<a id="rockabilly-trio"></a>

### 🤠 Rockabilly Trio `RbT`

size: [trio](#ax-size-trio)

Guitar plus slap bass plus drums (often drumless) — the minimal lineup that defined the Sun Records rockabilly sound, raw and garage-like with slapback echo making a small group sound big. Distinguished by the slap bass that is simultaneously bass and percussion, and frequently absent or minimal drums.

## Small ensembles

<a id="jazz-combo"></a>

### 🎷 Jazz Combo `JzC`

size: [small](#ax-size-small)

A small jazz group from trio to octet, most commonly a quintet (trumpet, sax, piano, bass, drums), using the same lineup across hot jazz, bebop, and hard bop where the difference is playing style rather than instrumentation. Distinguished by improvisational freedom and intimate interplay, more open and unpredictable than scored big band arrangements.

<sub>Aliases: Jazz Group · Small Jazz Combo · Jazz Quartet · Jazz Quintet · Jazz Sextet · Modern Swing Combo · Small Swing Combo</sub>

<a id="blues-band"></a>

### ⚡ Blues Band `BlB`

size: [small](#ax-size-small)

Electric guitar plus amplified harmonica plus rhythm section — the Chicago Blues sound born when Delta players electrified to cut through loud urban bars. Distinguished by a steady, danceable shuffle groove (drums and bass keep the tempo in the pocket) that retains blues rawness, a compromise between authenticity and danceability that solo Delta blues lacks.

<sub>Aliases: Electric Blues Band · Chicago Blues Band</sub>

<a id="manouche"></a>

### 🌹 Manouche `Man`

size: [small](#ax-size-small)

Acoustic guitars plus violin plus double bass with no drums and no horns — the all-string Hot Club format Django Reinhardt created, where rhythm comes entirely from 'la pompe' rhythm guitar and walking bass. Distinguished by being a unique all-acoustic lineup found in no other jazz genre, with the percussive 'la pompe' standing in for drums.

<sub>Aliases: Manouche Ensemble · Gypsy Jazz Ensemble · Manouche Group · Gypsy Jazz</sub>

<a id="string-band"></a>

### 🪕 String Band `StB`

size: [small](#ax-size-small)

Fiddle plus guitar plus banjo plus double bass — the original all-acoustic country lineup from rural American home music-making, rooted in Irish/Scottish (fiddle) and African (banjo) traditions. Distinguished by a warm, organic, homemade folk sound where fiddle and banjo create the recognizable country character.

<sub>Aliases: Country String Band · Hillbilly Band</sub>

<a id="new-orleans-small-group"></a>

### 🎺 New Orleans Small Group `NOG`

size: [small](#ax-size-small)

A small hot-jazz combo (front-line trumpet, clarinet, trombone over a rhythm section) playing the New Orleans/Dixieland repertoire in a club setting rather than the street — the seated, recording-band equivalent of the brass band (New Orleans Rhythm Kings, early Armstrong sides).

**Distinguishing trait:** smaller and tighter than the marching New Orleans Brass Band, with a fixed seated rhythm section rather than portable tuba-and-banjo, while keeping the same collective front-line improvisation.

<sub>Aliases: NOLA Small Group · New Orleans Combo · NOLA Combo · Rhythm Kings</sub>

<a id="rockabilly-combo"></a>

### 🕺 Rockabilly Combo `RbC`

size: [small](#ax-size-small)

A 4-6 piece rockabilly band that expands the classic trio with extra instruments such as a separate lead guitar, sax, piano, and a full drum kit. Distinguished from the Rockabilly Trio by being more layered and fuller rather than the raw, minimal three-piece Sun Records sound.

<a id="jump-blues-combo"></a>

### 💥 Jump Blues Combo `JBC`

size: [small](#ax-size-small)

A 5-7 piece sax-led R&B/jump format of the 1940s-50s (Louis Jordan's Tympany Five archetype) with tight rhythm, lead sax, vocal, and horn riffs, bridging big band swing and early R&B/rock'n'roll. Distinguished by sax-led dominance in the jump-blues idiom; unlike the later R&B Combo it sits in the 1940s-55 era with heavier sax focus.

<a id="r-b-combo"></a>

### 🎵 R&B Combo `RnB`

size: [small](#ax-size-small)

A 4-6 piece pre-soul R&B band of the 1950s-65 era with a mixed lead instrument (guitar, piano, or sax) and vocal-led repertoire, bridging the sax-led Jump Blues Combo and the horn-driven Soul Combo. Distinguished by its mixed lead and R&B repertoire of that era; unlike Soul Combo it predates the horn-section-dominant gospel-vocal soul sound.

<sub>Aliases: R&B Combo with Piano</sub>

<a id="soul-combo"></a>

### ❤️ Soul Combo `SoC`

size: [small](#ax-size-small)

A 4-7 piece soul band of the 1965-and-later era (Stax/Atlantic/Motown lineage) with a tight rhythm section, horn section, and gospel-derived expressive vocal. Distinguished from the R&B Combo by a more prominent horn section and gospel-rooted vocal expressiveness.

<sub>Aliases: Retro Soul Combo · Motown Combo</sub>

<a id="rock-combo"></a>

### 🤘 Rock Combo `RkC`

size: [small](#ax-size-small)

The post-1965 standard rock band format invented in the Beatles era: lead vocal plus two guitars plus bass plus drums. Distinguished from the Rockabilly Combo by being modern (post-1965) without slap bass and twang guitar; rarely used in a swing context.

<a id="vocal-acapella"></a>

### 🎙️ Vocal Acapella `Aca`

size: [small](#ax-size-small)

A vocal-only ensemble of 3-6 voices with no instrumental accompaniment (occasionally percussion or hand-claps), where the voices are the entire instrumentation in lead-plus-harmony arrangements. Distinguished from a combo-with-vocals by having only voices, and from Vocal + Piano by being three or more voices with no piano.

<sub>Aliases: Vocal Acapella Group · Vocal Quartet · Acapella · Vocal Ensemble (Acapella)</sub>

<a id="electronic-swing"></a>

### 🎛️ Electronic Swing `ElS`

size: [small](#ax-size-small)

A DAW-produced format (one or two producers) combining programmed beats, synthesized bass, and sampled vintage big band brass and vocals — not an acoustic ensemble but a production. Distinguished from every acoustic instrumentation category by being studio production-based rather than a live ensemble.

<sub>Aliases: Electronic with Swing Samples · Electroswing Production · Swing Samples</sub>

## Medium ensembles

<a id="new-orleans-brass-band"></a>

### 🎺 New Orleans Brass Band `NOB`

size: [medium](#ax-size-medium)

A street wind ensemble — the roots of jazz — where three front-line horns (cornet/trumpet, clarinet, trombone) collectively improvise over a portable rhythm section of tuba, banjo, and marching drums. Distinguished by three-voice collective polyphony ('organized chaos') in which no one solos in the modern sense, and a recognizable syncopated second-line rhythm.

<sub>Aliases: NOLA Brass Band · Brass Band</sub>

<a id="dixieland"></a>

### 🎺 Dixieland `Dix`

size: [medium](#ax-size-medium)

A traditional-jazz band built on collective front-line polyphony (cornet/trumpet, clarinet, trombone) over a banjo/piano/bass/drums rhythm section, playing the 1920s trad and revival repertoire.

**Distinguishing trait:** closely overlaps the New Orleans Small Group but names the revival/trad-jazz idiom and lineup broadly; unlike a Jazz Combo it favors collective ensemble polyphony over sequential individual solos.

<sub>Aliases: Dixieland Ensemble · Dixieland Band · Dixieland Combo</sub>

<a id="soul-blues-band"></a>

### 🔥 Soul Blues Band `SBB`

size: [medium](#ax-size-medium)

Front-and-center vocal plus horn section plus rhythm section — a more produced sound than the Blues Band, pairing a gospel-trained voice with a polished rhythmic engine and 'glamorous' horns (Memphis Stax/Hi style). Distinguished by the vocal as protagonist (not the guitar) and the polish of horns and organ, making it the most accessible blues for a wide audience.

<sub>Aliases: Soul Blues Combo · R&B Band</sub>

<a id="western-swing-band"></a>

### 🐎 Western Swing Band `WSB`

size: [medium](#ax-size-medium)

Country instruments (fiddle, steel guitar) playing swing jazz — a unique fusion found in no other genre, with the lineup expanding from 5 to 18+ players and jazz elements (sax, trumpet) added on top. Distinguished by jazz sophistication and improvisation in a country form, with steel guitar tone and fiddle jazz solos that no jazz big band has, setting it apart from pure country.

## Large ensembles

<a id="big-band"></a>

### 🎺 Big Band `BB`

size: [large](#ax-size-large)

A large ensemble organized into sections (saxes, trumpets, trombones, rhythm) that trade and answer each other — the sound that defined the swing era and created the music for Lindy Hop, built on Fletcher Henderson's arranging template. Distinguished by a 'wall of sound' and huge dynamic range (whisper to full-ensemble shout chorus) that no smaller group can reproduce.

<sub>Aliases: Big Band Ensemble · Swing Big Band · Modern Big Band · Big Band Revival · Latin Jazz Big Band</sub>

<a id="orchestra"></a>

### 🎻 Orchestra `Orc`

size: [large](#ax-size-large)

A large ensemble built around a string section, often with woodwinds, brass, and a rhythm section behind a singer — the lush studio backing of vocal-jazz, easy-listening, and pop-standards sessions (Sinatra-with-strings, Nelson Riddle arrangements).

**Distinguishing trait:** led by bowed strings rather than the sax/trumpet/trombone sections that define a Big Band, producing a sweeping, smooth backdrop rather than a swinging, sectional shout.

<sub>Aliases: String Orchestra · Symphony Orchestra</sub>

## Axis reference

**Size**

- <a id="ax-size-solo"></a>`solo` — A single performer — one voice or one instrument carrying the whole track.
- <a id="ax-size-duo"></a>`duo` — Two performers together, such as a singer with one accompanist or a pairing of two instruments.
- <a id="ax-size-trio"></a>`trio` — Three performers — a compact group like a piano-bass-drums rhythm section or a vocal-plus-two combo.
- <a id="ax-size-small"></a>`small` — A small combo of roughly four to six players, fuller than a trio but well short of a full band.
- <a id="ax-size-medium"></a>`medium` — A mid-sized band of about seven to twelve players, with distinct sections but lighter than a full orchestra.
- <a id="ax-size-large"></a>`large` — A large ensemble — a full big band, orchestra, or comparable group with multiple full sections and a heavy sound.

_Generated from [`data/vocabulary/`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/data/vocabulary) by `scripts/gen_vocab_docs.py` — do not hand-edit; edit `data/vocabulary/`, then re-run this._
