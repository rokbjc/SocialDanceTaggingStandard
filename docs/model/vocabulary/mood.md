# Mood

The **mood** vocabulary — each value's canonical name (what is stored), the aliases that resolve to it, and the axes that make it queryable.

→ Field reference: [Mood](../../standard/fields/id3/mood.md) · [all dimensions](README.md) · [dances](dances.md)

Axes — **energy** (1–10, 1 = lowest arousal, 10 = highest); **valence** (1–10, 1 = darkest, 10 = brightest).

Each value carries a 1–10 **energy × valence** coordinate on the [Russell circumplex](https://en.wikipedia.org/wiki/Emotion_classification#Circumplex_model) plane; this grid folds them to the 3×3 view — a 5×5 or finer view buckets the same 1–10, since discretisation is the consumer's choice, not the standard's:

| energy ↓ / valence → | Bright | Neutral | Dark |
|---|---|---|---|
| High | Joyful, Playful, Hot | Confident, Virtuosic, Wild | Intense, Driving, Angry |
| Medium | Warm, Elegant, Hopeful | Cool, Sophisticated, Soulful, Groovy | Bluesy |
| Low | Smooth, Tender, Calm | Dreamy | Sultry, Melancholic, Haunting, Bittersweet |

<a id="joyful"></a>

### 😄 Joyful `JOY`

energy: 8 · valence: 10

Pure, unfiltered, celebratory happiness that lifts you up without complexity or irony — bright major-key high energy.

**Distinguishing trait:** it gladdens rather than amuses (vs Playful, which has humor) and conveys joy rather than power (vs Confident).

<sub>Aliases: Exuberant · Celebratory · Jubilant · Happy · Cheerful</sub>

<a id="playful"></a>

### 😜 Playful `PLY`

energy: 7 · valence: 9

Lighthearted, humorous music with a 'wink' that makes you smile or laugh, via witty lyrics, performance asides, or surprising musical turns.

**Distinguishing trait:** it has an explicit humor/comedy element (it makes you laugh), unlike Joyful's straight gladness.

<sub>Aliases: Fun · Humorous · Witty · Cheeky · Quirky · Sassy</sub>

<a id="hot"></a>

### 🔥 Hot `HOT`

energy: 8 · valence: 7

Sexy, high-energy, forward-leaning performance with an erotic charge — full-blooded, brassy, 'I want you' momentum at medium-to-fast tempo.

**Distinguishing trait:** seductive forward intent rather than slow smoky sensuality (vs Sultry) or chaotic energy (vs Wild).

<sub>Aliases: Fiery · Scorching</sub>

<a id="confident"></a>

### 😎 Confident `CNF`

energy: 7 · valence: 6

Cool, assured swagger and controlled power that makes you feel unbeatable — strutting tempo, in-the-pocket rhythm, authoritative delivery (Sinatra stepping onstage).

**Distinguishing trait:** relaxed self-assured strength ('I know I'll win'), not joy (Joyful) and not the strained, fighting power of Intense.

<sub>Aliases: Swagger · Empowering · Bold</sub>

<a id="virtuosic"></a>

### ✨ Virtuosic `VRT`

energy: 9 · valence: 6

Technical brilliance on display — speed, harmonic complexity, and athletic musicianship (bebop runs, Art Tatum, Buddy Rich solos) that demands attention rather than serving the dance floor.

**Distinguishing trait:** melodic/technical acrobatics rather than rhythmic repetition (vs Driving).

<sub>Aliases: Technical · Dazzling · Show-off</sub>

<a id="wild"></a>

### 🤪 Wild `WLD`

energy: 10 · valence: 5

Untamed, raw, chaotic energy where unpredictability is the feature itself — manic, off-the-rails, full of shouts and sudden swells.

**Distinguishing trait:** chaos and loss of control, versus the locked-in predictable motor of Driving or the controlled brilliance of Virtuosic.

<sub>Aliases: Crazy · Unbridled · Mad · Frantic · Shouting</sub>

<a id="intense"></a>

### ⚡ Intense `INT`

energy: 9 · valence: 3

Dramatic, fierce, high-tension music that grabs you by the throat — dark energy that builds and explodes through big dynamic contrasts (minor-key swing, Ellington drama).

**Distinguishing trait:** it builds and erupts via quiet-to-loud contrast, unlike Driving's even, contrast-free force.

<sub>Aliases: Dramatic · Fierce · Powerful · Edgy</sub>

<a id="driving"></a>

### 🚂 Driving `DRV`

energy: 8 · valence: 4

Unstoppable, relentless forward force from a repetitive motor-like rhythm — hypnotic, earthy, in-the-pocket (boogie-woogie ostinato, one-chord Howlin' Wolf riffs).

**Distinguishing trait:** even, repetitive momentum without dynamic contrast, unlike Intense which builds and explodes.

<sub>Aliases: Relentless · Raw</sub>

<a id="warm"></a>

### 🤗 Warm `WRM`

energy: 5 · valence: 8

A warm, kind, full-bodied feeling like a hug — bright but unenergetic, often crooner vocals (Nat King Cole, Tony Bennett), suited to intimate and cozy moments.

**Distinguishing trait:** robust emotional warmth, more full-bodied than fragile Tender and more feeling-focused than polished Smooth.

<sub>Aliases: Cozy · Intimate · Heartfelt</sub>

<a id="elegant"></a>

### 🎩 Elegant `ELG`

energy: 5 · valence: 7

Graceful, refined, sweet, formal-ballroom-appropriate music — sweet bands and society orchestras for hotel ballrooms and white-tie events; polished and dignified, not hot or wild.

**Distinguishing trait:** social/aesthetic refinement for the dance floor, versus Sophisticated's harmonic/melodic complexity for the listener.

<sub>Aliases: Graceful · Polite</sub>

<a id="cool"></a>

### 🧊 Cool `COO`

energy: 5 · valence: 5

Restrained, understated, cerebral jazz feeling — the opposite of 'hot': intellectual, lyrical, neither happy nor sad (Miles Davis 'Birth of the Cool', Lester Young, Chet Baker).

**Distinguishing trait:** emotional detachment ('reserved', not 'sad'), distinct from Melancholic's bitterness.

<sub>Aliases: Laid-back · Restrained · Detached · Relaxed · Chill</sub>

<a id="sophisticated"></a>

### 🍸 Sophisticated `SOP`

energy: 5 · valence: 6

Harmonically rich, culturally refined, grown-up feeling — Ellington-level composition, Sinatra's Capitol period, cabaret elegance for cocktail-hour and black-tie listening.

**Distinguishing trait:** fully expressed yet refined harmonic/melodic complexity, versus Cool's restraint and detachment.

<sub>Aliases: Refined · Urbane · Lyrical · Izbran</sub>

<a id="soulful"></a>

### 🙌 Soulful `SOU`

energy: 6 · valence: 6

Gospel/church-influenced emotional expression — melisma, vocal cries, call-and-response, building intensity (Ray Charles, Aretha Franklin, Otis Redding).

**Distinguishing trait:** gospel uplift and church-style power, versus Bluesy's secular sorrow and blues-scale roots.

<sub>Aliases: Church-influenced · Gospel-tinged · Feeling-heavy · Expressive</sub>

<a id="groovy"></a>

### 🕺 Groovy `GRV`

energy: 6 · valence: 6

A body-rhythm, locked-in 'in the pocket' feel that immediately makes you sway — the rhythm section is glued together with a strong backbeat.

**Distinguishing trait:** body engagement rather than forward speed (vs Driving's tempo-motor) and active involvement rather than restraint (vs Cool).

<sub>Aliases: In the Pocket · Locked-in</sub>

<a id="bluesy"></a>

### 🎸 Bluesy `BLZ`

energy: 5 · valence: 3

A blues-harmonic, mournful, soulful feeling marked by specific blues markers — blue notes, blues scales, 12-bar feel, microtonal bending (Muddy Waters, BB King).

**Distinguishing trait:** it is harmonic-technical (blues scales and bending) rather than purely emotional like Melancholic.

<sub>Aliases: Blues-tinged · Mournful · Blues-like · Earthy</sub>

<a id="smooth"></a>

### 🪶 Smooth `SMO`

energy: 4 · valence: 7

Elegant, sophisticated, 'cool' polish — like a midnight cocktail in a jazz club; effortlessly, confidently relaxed beauty.

**Distinguishing trait:** it impresses rather than moves you (vs Tender), is low-energy passive elegance (vs high-energy active Confident), and is present rather than floating (vs Dreamy).

<sub>Aliases: Polished · Mellow</sub>

<a id="tender"></a>

### 💗 Tender `TND`

energy: 3 · valence: 8

Romantic, gentle, loving music that touches the heart — warm, direct, sincere emotion without irony or distance (an honest love song).

**Distinguishing trait:** it moves you and sings directly 'to you' here and now, versus Smooth (impresses) and Dreamy (floats elsewhere).

<sub>Aliases: Romantic · Gentle · Sentimental</sub>

<a id="dreamy"></a>

### 💫 Dreamy `DRM`

energy: 3 · valence: 6

Floating, starry-eyed music that carries you elsewhere — ambivalent, open 'floating' harmony evoking moon, stars, and memory; beautiful yet out of reach.

**Distinguishing trait:** nostalgic beauty without pain ('postcard, not funeral'), versus Melancholic's sorrow, and absent/floating versus Tender's direct presence.

<sub>Aliases: Starry · Wistful · Ethereal · Hypnotic</sub>

<a id="sultry"></a>

### 💋 Sultry `SLT`

energy: 3 · valence: 4

Sensual, smoky, seductive music — darkness that attracts rather than repels, like a dim club of cigarette smoke and whiskey; erotic tension that is dark but not sad.

**Distinguishing trait:** dark attraction rather than dark pain (vs Melancholic), and physical 'in your ear' closeness versus Smooth's cool distance.

<sub>Aliases: Sensual · Smoky · Seductive · Sexy</sub>

<a id="melancholic"></a>

### 😢 Melancholic `MEL`

energy: 1 · valence: 1

Sad, lonely, nostalgic music — pain expressed through beauty, conveying loss, longing, and isolation; the darkest mood, with unresolved minor-key tension.

**Distinguishing trait:** it saddens rather than attracts (vs Sultry) and carries real pain (vs Dreamy's painless nostalgia).

<sub>Aliases: Melancholy · Sad · Lonely · Brooding</sub>

<a id="haunting"></a>

### 👻 Haunting `HNT`

energy: 2 · valence: 2

Eerie, otherworldly music with lingering, unsettling emotional weight — drones, modal/suspended harmony, reverb and isolation that leave residue after the song ends (Robert Johnson, Skip James).

**Distinguishing trait:** psychological/unsettling resonance rather than the plain emotional sadness of Melancholic.

<sub>Aliases: Eerie · Ghostly · Chilling</sub>

<a id="angry"></a>

### 😠 Angry `ANG`

energy: 9 · valence: 2

Hostile, aggressive, confrontational music — clenched, combative energy aimed outward.

**Distinguishing trait:** outward aggression and hostility, unlike Intense which builds and erupts through quiet-to-loud drama, and unlike Driving's even, contrast-free motor.

<sub>Aliases: Aggressive · Furious · Hostile</sub>

<a id="bittersweet"></a>

### 🥲 Bittersweet `BSW`

energy: 3 · valence: 4

Happiness and sadness at once — fond but aching, sweet but tinged with loss.

**Distinguishing trait:** the blend of sweet and sad in one feel, unlike Melancholic's pure sorrow and Tender's pure warmth.

<a id="calm"></a>

### 😌 Calm `CLM`

energy: 2 · valence: 7

Peaceful, serene, settled music that puts the room at ease — unhurried and restful.

**Distinguishing trait:** plain serenity, unlike Smooth's polished cocktail elegance and Cool's detached, cerebral restraint.

<sub>Aliases: Peaceful · Serene · Restful</sub>

<a id="hopeful"></a>

### 🌅 Hopeful `HOP`

energy: 6 · valence: 8

Optimistic, uplifting music that feels like things are looking up — a warm forward lift toward something better.

**Distinguishing trait:** rising optimism and anticipation, unlike Joyful's in-the-moment, arc-free happiness.

<sub>Aliases: Optimistic · Uplifting</sub>

## Axis reference

**Energy**

- <a id="ax-energy-high"></a>`high` — 7–10 on the 1–10 arousal scale — hard-driving, up-front intensity that pushes the floor.
- <a id="ax-energy-medium"></a>`medium` — 5–6 on the 1–10 arousal scale — moderate drive, neither pushing nor holding back.
- <a id="ax-energy-low"></a>`low` — 1–4 on the 1–10 arousal scale — relaxed, low intensity that settles or stills the room.

**Valence**

- <a id="ax-valence-bright"></a>`bright` — 7–10 on the 1–10 brightness scale — warm, upbeat feeling; major-leaning harmony and open timbre that read as happy, sunny, or uplifting.
- <a id="ax-valence-dark"></a>`dark` — 1–4 on the 1–10 brightness scale — moody, heavy feeling; minor-leaning harmony and shadowed timbre that read as sad, tense, or somber.
- <a id="ax-valence-neutral"></a>`neutral` — 5–6 on the 1–10 brightness scale — emotionally even or ambiguous, neither clearly bright nor dark.

_Generated from [`data/vocabulary/`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/data/vocabulary) by `scripts/gen_vocab_docs.py` — do not hand-edit; edit `data/vocabulary/`, then re-run this._
