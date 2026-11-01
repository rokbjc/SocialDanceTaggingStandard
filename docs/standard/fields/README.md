# Field reference

What a track can be tagged with, **grouped by the question each field answers** on the floor. Each row links to a one-page detail (meaning, values, examples, binding). The actual value sets live in the [vocabulary](../../model/vocabulary/README.md); this is about the fields.

Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) — the single source of truth. Conceptual spec: [`../spec.md`](../spec.md). Cross-tool interop: [`../../sync/tool-comparison.md`](../../sync/tool-comparison.md).

<a id="which-dance-and-when"></a>

## Which dance, and when

| Field | Tag | What it does |
|---|---|---|
| [Dance](sdts/dance.md) `dance` | `TXXX:SDTS_DANCES` | Filter the library into per-dance pools and build for the room in front of you — 'give me Lindy at 180', 'a blues set'. |
| [Theme (Listening / Event Context)](sdts/theme.md) `theme` | `TXXX:SDTS_THEME` | Pull a holiday / Valentine / Halloween / event set on demand. |
| [Routine](sdts/routine.md) `routine` | `TXXX:SDTS_ROUTINES` | Pull THE Shim Sham / Big Apple / Tranky Doo track (original), with vetted alternatives and experimental options behind it. · _proposed_ |

<a id="tempo-and-groove"></a>

## Tempo & groove

| Field | Tag | What it does |
|---|---|---|
| [BPM (raw)](id3/bpm.md) `bpm` | `TBPM` | The primary set-building filter — find tracks inside the dance's tempo window. |
| [Rhythmic Feel](sdts/rhythmic_feel.md) `rhythmic_feel` | `TXXX:SDTS_RHYTHMIC_FEEL` | Hold or deliberately switch the groove (shuffle vs straight vs slow-drag) — the main blues flow lever beyond tempo. |
| [Meter](sdts/meter.md) `meter` | `TXXX:SDTS_METER` | Gate 3/4 waltz-family material apart from 4/4. · _proposed_ |
| [Tempo Stability](sdts/tempo_stability.md) `tempo_stability` | `TXXX:SDTS_TEMPO_STABILITY` | Avoid rubato / tempo-change tracks when you need a steady, beatmatchable floor. · _proposed_ |

<a id="mood-and-energy"></a>

## Mood & energy

| Field | Tag | What it does |
|---|---|---|
| [Mood](id3/mood.md) `mood` | `TMOO` | Steer the room's affect — pull a lift, a cool-down, or a peak by feel, and avoid jarring valence jumps. |
| [Energy](sdts/energy.md) `energy` | `TXXX:SDTS_ENERGY` | Build the night's energy arc and contrast — pull an opener, a builder, a peak, a cool-down. · _proposed_ |
| [Valence](sdts/valence.md) `valence` | `TXXX:SDTS_VALENCE` | Pick by brightness independently of drive — a dark peak, a bright cool-down — and avoid jarring valence jumps between records. · _proposed_ |
| [Danceability](sdts/danceability.md) `danceability` | `TXXX:SDTS_DANCEABILITY` | Separate genuine floor tracks from listening-only music. · _proposed_ |

<a id="sound-and-ensemble"></a>

## Sound & ensemble

| Field | Tag | What it does |
|---|---|---|
| [Genre](id3/genre.md) `genre` | `TCON` | Reason about a track's era/character, seed dance + mood expectations, and build style-coherent or deliberately-contrasting blocks. |
| [Vocality](sdts/vocality.md) `vocality` | `TXXX:SDTS_VOCALITY` | Alternate instrumental vs vocal and male vs female leads for variety; pull instrumentals for classes/jams or vocals to draw the crowd. |
| [Instrumentation (Ensemble)](sdts/instrumentation.md) `instrumentation` | `TXXX:SDTS_INSTRUMENTATION` | Balance big-band 'meat' against small-combo texture and alternate ensemble size for variety. |
| [Instrument-Led](sdts/instrument_led.md) `instrument_led` | `TXXX:SDTS_INSTRUMENT_LED` | Spot or avoid clustering similar-sounding lead instruments across a block. |

<a id="recording-character"></a>

## Recording character

| Field | Tag | What it does |
|---|---|---|
| [Recording Context](sdts/recording_context.md) `recording_context` | `TXXX:SDTS_RECORDING_CONTEXT` | Avoid dropping a talky live/applause track where you need a tight transition; choose studio vs live deliberately. |
| [Recording Quality Era](sdts/recording_quality_era.md) `recording_quality_era` | `TXXX:SDTS_RECORDING_QUALITY_ERA` | Rank duplicate versions and judge whether a lo-fi acoustic-era recording suits the room and PA. |
| [Recording Origin](sdts/recording_origin.md) `recording_origin` | `TXXX:SDTS_RECORDING_ORIGIN` | Choose period-original vs revival/cover vs reissue — Herrang wants vintage, a modern crowd may prefer a clean revival. · _proposed_ |
| [Original Title](sdts/original_title.md) `original_title` | `TXXX:SDTS_ORIGINAL_TITLE` | Group performances of one work — pull every version of 'Stormy Monday', or avoid programming the same work twice in a set. · _proposed_ |
| [Lyric Language](id3/lyric_language.md) `lyric_language` | `n/a` | Filter by sung language for language-specific rooms or sing-along sets. · _proposed_ |
| [Artist Country](sdts/artist_country.md) `artist_country` | `TXXX:SDTS_ARTIST_COUNTRY` | Pull a country / region set — 'only French', 'US-only' — for geographic theming. · _proposed_ |

<a id="structure-and-cueing"></a>

## Structure & cueing

| Field | Tag | What it does |
|---|---|---|
| [Song Form](sdts/song_form.md) `song_form` | `TXXX:SDTS_SONG_FORM` | Know the harmonic form (AABA / 12-bar) for routine matching and musicality cueing. |
| [Intro Duration](sdts/intro_duration.md) `intro_duration` | `TXXX:SDTS_INTRO_DURATION` | Spot tracks with an uncomfortably long intro before the groove. · _proposed_ |
| [Outro Duration](sdts/outro_duration.md) `outro_duration` | `TXXX:SDTS_OUTRO_DURATION` | Spot tracks with a long fade / applause tail. · _proposed_ |
| [Ending Type](sdts/ending_type.md) `ending_type` | `TXXX:SDTS_ENDING_TYPE` | Know the ending (definitive / fade / cold-stop) for tight transitions. · _proposed_ |
| [Playback Start (cue-in)](sdts/playback_start.md) `playback_start` | `TXXX:SDTS_PLAYBACK_START` | Skip a long talky / applause / rubato intro so the track drops straight into the groove. · _proposed_ |
| [Playback Stop (cue-out)](sdts/playback_stop.md) `playback_stop` | `TXXX:SDTS_PLAYBACK_STOP` | Cut a fade-out or applause tail so the next track comes in tight. · _proposed_ |
| [Fade-in](sdts/fade_in_duration.md) `fade_in_duration` | `TXXX:SDTS_FADE_IN_DURATION` | Ease a track in from its cue-in instead of a hard entry. · _proposed_ |
| [Fade-out](sdts/fade_out_duration.md) `fade_out_duration` | `TXXX:SDTS_FADE_OUT_DURATION` | Tame a fade-ending or applause-tailed record with a clean fade. · _proposed_ |
| [Regions](sdts/regions.md) `regions` | `TXXX:SDTS_REGIONS` | Trim talky intros, mark silence gaps, flag music-vs-speech boundaries — durable across machines. · _proposed_ |

<a id="workflow-and-trust"></a>

## Workflow & trust

| Field | Tag | What it does |
|---|---|---|
| [Rating](id3/rating.md) `rating` | `POPM with email/owner 'no@email'` | Your fastest trust signal mid-set — 'which of my 150-BPM tracks is actually a killer'. |
| [Confirmation gates (Custom10)](sdts/confirmation_gates.md) `confirmation_gates` | `TXXX:SDTS_CONFIRMATION_GATES` | Lock human-verified fields so auto-suggestions / re-analysis don't overwrite your confirmed calls. |
| [SDTS schema version](sdts/sdts_version.md) `sdts_version` | `TXXX:SDTS_VERSION` | Skip files already at the current standard and migrate only older ones — fast, idempotent normalization. |

## Identity, origin & credit frames (ID3 pass-through)

Universal frames SDTS does **not** redefine — title, artist, album, comment, grouping, the **Original-\*** frames (`TOPE`/`TOAL`/`TDOR`) for covers & reissues, and the paired credit-list frames (`TIPL`/`TMCL`).

| Frame | Concept | Doc |
|---|---|---|
| `TIT2` | Title | [id3/tit2.md](id3/tit2.md) |
| `TPE1` | Artist | [id3/tpe1.md](id3/tpe1.md) |
| `TALB` | Album / Labels | [id3/talb.md](id3/talb.md) |
| `COMM (no desc, any language)` | Comment | [id3/comm.md](id3/comm.md) |
| `TIT1` | Grouping | [id3/tit1.md](id3/tit1.md) |
| `TOPE` | Original Artist | [id3/tope.md](id3/tope.md) |
| `TOAL` | Original Album | [id3/toal.md](id3/toal.md) |
| `TDOR` | Original Release Date | [id3/tdor.md](id3/tdor.md) |
| `TIPL` | Involved People List | [id3/tipl.md](id3/tipl.md) |
| `TMCL` | Musician Credits List | [id3/tmcl.md](id3/tmcl.md) |
