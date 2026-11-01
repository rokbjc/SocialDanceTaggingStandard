# SDTS field rationale — origin, storage, and worth

_Non-normative companion to the spec: **why** each field exists, **where** its value comes from and is stored, and **how worthwhile** it is. The binding definitions live in [`spec.md`](spec.md) / [`../../data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml), and the per-field origin/values are in the generated [field catalog index](fields/README.md). This page adds the judgment (worth/demand) and the storage-migration detail the generator does not emit. Primary user: a solo **swing+blues** DJ._

> **Not normative.** Demand/popularity is time-sensitive — this is a prioritization aid, not a contract. Based on two interop + demand audits (2026-06) across swing/blues/ballroom DJ and dancer communities, music-tagging forums, and the dance domain (federations, teachers, milonga/latin scenes, dance-fitness, MIR groove research).

## Worth: which fields to build, which to reconsider

- **Core (build first):** `bpm`, `energy`, `genre`, `mood`, `dance`, `rating`
- **Recommended (clear value):** `recording_origin`, `theme`, `instrumentation`, `intro_duration`, `outro_duration`, `ending_type`, `playback_start`, `playback_stop`, `recording_quality_era`, `routine`, `vocality`, `confirmation_gates`, `rhythmic_feel`
- **Optional (nice-to-have):** `danceability`, `fade_in_duration`, `fade_out_duration`, `recording_context`, `meter`, `tempo_stability`
- **Marginal (reconsider / defer — AI-slop risk):** `instrument_led`, `lyric_language`, `song_form`

### The judgment behind the tiers

- **The marginal three are the highest AI-slop risk** — plausible-sounding and taxonomically tidy, but thin real demand and high tagging cost. `instrument_led`: no one asks to filter by lead instrument, and it is the hardest to populate. `song_form`: sounds rigorous, but no live DJ workflow consumes it and it is nearly determined by genre. `lyric_language`: near-empty (would read "English" on ~95% of an English-dominant swing repertoire). Keep them analysis-derived or drop until a concrete need appears.
- **`recording_quality_era` was promoted** to *recommended* — the verify pass found it underrated; the practical question ("is the audio good enough to play out?") recurs.
- **`confirmation_gates` is low external demand but high internal fit:** the reference implementation auto-suggests genres/moods/beats via AI+ML that the community openly distrusts, so a verified/unverified gate that protects hand-checked fields earns its place.
- **`blues_idiom_purity` was folded inline onto `dance`** as `:core | :edge | :fusion` (scene-agnostic) rather than shipped as a standalone field — purity is per-(track × dance); the deviation *direction* (jazz/soul) lives in `genre`.
- **`danceability` stays optional:** for a solo swing+blues DJ the human `rating` already encodes "will this work on my floor" better than an algorithmic danceability score.
- **Confirmed OUT (would be AI-slop to add):** musical key / Camelot harmonic mixing (not useful for swing+blues social dance); a crowd-response log and play-history / per-venue memory are **player/session features**, not portable file tags.

## Origin & storage

Every tag has one of three origins: a plain **standard ID3** frame, an **SDTS-defined** field (concept + values authored in SDTS / the source catalog), or **reference-implementation–private** analysis that is not part of the portable standard.

### Standard ID3 — reuse, don't reinvent

`genre` → `TCON`, `mood` → `TMOO` (v2.4-only; a bare `TXXX:MOOD` would shadow it), `bpm` → `TBPM` (integer; set it to the danced pulse if the detector mis-locks on a subdivision), `rating` → `POPM` (owner `no@email`; 0-10 half-stars, 0 = unrated), `lyric_language` → align with `TLAN` (ISO 639-2 — do **not** mint `SDTS_LYRIC_LANGUAGE`). Title / Artist / Album / Comment are written as-is.

`TIT1` (Grouping) is the catalog `workflow_tags` bag — **not a portable SDTS field**. It is being decomposed: intrinsic bits migrate (Medley → `song_form`, Cover/Tribute → `recording_origin`, recording-meta → quality flags) pending catalog ratification; Canonical/Duplicate and personal/triage markers stay non-portable.

### SDTS-defined — reused MediaMonkey slots → portable `TXXX:SDTS_*`

Active fields squat in frozen `COMM:Songs-DB_*` slots today; the portable target is `TXXX:SDTS_*`. **Every value in those slots is SDTS / source-catalog defined** — the slot is just generic storage. Gotchas the generated index does not flag:

- **`dance`** — `SDTS_DANCES` is the **canonical** home (no standard ID3 frame exists for dances), *not* a mirror of a vendor frame. `COMM:Songs-DB_Occasion` is a legacy MediaMonkey mirror only ("Occasion" is a misnomer for dances).
- **`recording_quality_era`** — stored ad-hoc in `TXXX:Quality` (frozen-legacy). It is a recording-**era** enum, **not** a fidelity grade, and conceptually collides with the reference-implementation–private `TXXX:SB-Quality` analyzer JSON (out of scope).
- **`song_form`** — the `Songs-DB_Custom6` allocation is **contested** (an earlier allocation also assigned `Custom6` to the private `SB-Quality` JSON); verify the live allocation before migrating.
- **`confirmation_gates`** — reference-implementation–owned confirmed-state + tamper mechanism, not a catalog taxonomy value, so it has no legacy column to preserve.

#### MediaMonkey reused slots

The reference implementation mirrors some `active` fields into MediaMonkey's frozen `COMM:Songs-DB_*` slots so they surface as sortable MM5 columns. MM5 surfaces each `Songs-DB_CustomN` frame as the **column "Custom Field N+1"** — note the off-by-one. The canonical value lives in `TXXX:SDTS_*` (or the standard frame); the `COMM` slot is a regenerable **mirror** (canonical → mirror only — drop it anytime without losing the canon; e.g. `Lindy Hop` → `#LH` for the MM Occasion column).

| Reused frame (frozen) | MM5 sortable column | SDTS field |
|---|---|---|
| `COMM:Songs-DB_Occasion` | Occasion | `dance` |
| `COMM:Songs-DB_Custom1` | Custom Field 2 | `decade` (field removed — legacy data only; derive era from the year) |
| `COMM:Songs-DB_Custom3` | Custom Field 4 \* | `vocality` |
| `COMM:Songs-DB_Custom4` | Custom Field 5 \* | `rhythmic_feel` |
| `COMM:Songs-DB_Custom5` | Custom Field 6 | `instrumentation` |
| `COMM:Songs-DB_Custom6` | Custom Field 7 | `song_form` ‡ |
| `COMM:Songs-DB_Custom7` | Custom Field 8 | `instrument_led` † |
| `COMM:Songs-DB_Custom8` | Custom Field 9 | `recording_context` |
| `COMM:Songs-DB_Custom9` | Custom Field 10 | `theme` |
| `COMM:Songs-DB_Custom10` | — (no native column) | `confirmation_gates` |
| `TXXX:Quality` | — (not MM5-mapped) | `recording_quality_era` |

\* The `Custom3`/`Custom4` column numbers are inferred from the uniform N+1 rule; they are not separately recorded in the source.
† `instrument_led` previously lived in Grouping/`TIT1` (pre-2026-04-28; migrated out).
‡ `Custom6` is contested (an earlier allocation also assigned it to the private `SB-Quality` JSON) — verify the live allocation before migrating `song_form`.

To mirror an SDTS field into a sortable MM5 column, write the value into the matching `Songs-DB_Custom*` frame (`COMM`, lang `XXX`) in addition to `TXXX:SDTS_*` — see [tool comparison → making SDTS fields visible](../sync/tool-comparison.md#making-sdts-fields-visible-in-each-tool).

### Reference-implementation–private (SB-\*) — not portable

The reference implementation also carries private `SB-*` analysis caches (beat grid, downbeats, stems, chroma, fingerprints, quality, lyrics, choreography, …). They are present on tagged files but are **not** part of the portable standard, and are catalogued in [Appendix B of the frame appendices](spec-frames.md). A few (dance steps, choreography, markers) are candidates to graduate into portable SDTS fields later.

Design convention: timestamped datapoints live in *typed lanes* (the dense machine beat grid, the scalar cue-in/out, the structural sections, and human point annotations each stay separate) rather than one merged tag; a unified timeline is assembled at read-time, not stored merged.

## Dance-domain validation & gaps

The way the **dance world** (not DJ tooling) categorizes music — federations, teachers, milonga/latin scenes, dance-fitness, MIR groove research — maps directly onto SDTS: danceability, energy, rhythmic_feel, song_form/phrasing, dance, mood, tempo_stability, meter, intro/outro + ending_type all find a home. The model holds up against how dancers actually classify music. Two kinds of gap surfaced:

1. **Full-social scene machinery (out of scope by design)** — tango orchestra / tanda / cortina, salsa On1-vs-On2 / clave timing, Latin sub-styles, International-vs-American ballroom, governing-body regulation, MPM as a first-class field. These are scene-specific *fields* and sub-style *granularity* a solo swing+blues DJ does **not** serve; they stay documented-but-out-of-scope. The boundary is fields and granularity, not dances: any dance family may ship as an opt-in **pack** of coarse values — one value per dance, with tempo, typical genres and a description — which a library never sees unless it enables that pack.
2. **MIR groove drivers (genuine in-scope candidates)** — **syncopation**, **low-frequency / bass energy**, **beat strength / pulse salience**. Academic groove research repeatedly finds these are the strongest predictors of the urge to move; today they are only *implicit* in `danceability` and the reference implementation's groove analysis. Worth considering as explicit analysis-derived signals rather than hand tags.

## Scope rule: generalize the shape, don't mint per-scene tags

SDTS carries **no scene-specific tags**. A cross-scene audit (9 partner dances — tango, salsa, bachata, kizomba/zouk, WCS, ballroom, forró, country, merengue) confirmed the recurring distinctive axes are **shared patterns**, not per-scene one-offs. The rule: ship the field SHAPE once, parameterize the value set **per dance** (owned by the SDTS baseline vocabulary); adding a dance = adding value rows, not columns.

- **idiom-purity** (trad ↔ fusion) recurs in 7+ scenes (blues core/fusion ≈ tango trad/nuevo ≈ salsa dura/timba ≈ kizomba semba/urban) → carried **inline per-dance on `dance`** as `:core|edge|fusion`. The deviation *direction* (jazz/soul) lives in `genre`.
- **style/timing variant** (salsa LA/NY/Cuban + on1/on2, bachata dominican/sensual, ballroom Intl/American) — same generalizable shape; **deferred** (no consumer; would also be inline-per-dance).
- **`routine`** generalized swing → "recording bound to a fixed group/called choreography" (country line-anthems, salsa rueda) — a value-table extension, not a new field.
- **Fold-ins** (no new fields): tango orquesta → `instrumentation`; instrumental/estribillo/cantado → `vocality` value-set; era → `recording_quality_era` + the year frame; clave direction → `rhythmic_feel` sub-descriptor; tanda arc → `energy`.
- **Accepted scene-unique residue** (add scene-scoped ONLY when that dance is a real consumer): tango **rhythm-family** (tango/vals/milonga) + **cortina** + **orquesta-as-tanda-key**; ballroom **Paso highlight grid**; country line/partner **use-mode**.
- **Deferred NEW field:** `edit_provenance` (contemporary fan/DJ edits — slowed / half-time / blend), distinct from `recording_origin`; trigger to mint = the first contemporary-music dance scene (WCS) becomes a real consumer.

Governance: every per-dance value enumeration is owned by the **SDTS baseline vocabulary** (`data/vocabulary/`, the taxonomy authority); the reference implementation ships the parameterized shape.

## New candidates worth tracking

Open discovery across DJ/dancer/tagger communities surfaced a few genuinely repeated requests (several from a real West Coast Swing DJ's tracking spreadsheet):

- **familiarity / canon-status** — how well-known the track is to the local scene ("one of the 200 classic Lindy/blues songs" vs an unknown). A scene-scoped ordinal.
- **teachability / dancer-level** — beginner-suitability: predictable phrasing, clear uncomplicated rhythm.
- **energy ARC** — start-energy / end-energy / transition, rather than the single `energy` scalar; a real WCS DJ tracks all three.

Already covered (considered, no new field needed): acousticness → `instrumentation` + `recording_quality_era` + `recording_origin`; vocal-gender / artist-rotation variety → `vocality` + artist metadata.

Out: musical key / Camelot mixing (not useful for swing+blues); crowd-response log and play-history / per-venue memory are player/session features, not portable file tags (they belong in the consumer's session model, not SDTS).

## Sources

Swing DJ Resources, dogpossum, Mr Ben & Nathan Malone (swing/blues DJ writing), SwingDJs.com forum, DJTechTools, Pioneer/SetFlow set-building, Stanford/Richard Powers, Mixed In Key, music4dance, Connie-Does-Data (WCS DJ spreadsheet); WDSF/WDC/NDCA + ISTD + BallroomDJ 4 (ballroom); Tejas Tango / milonga DJ guides; Zumba/Les Mills (dance-fitness); Spotify/Essentia danceability + academic groove research (syncopation, low-frequency energy).
