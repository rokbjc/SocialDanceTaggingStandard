# Changelog

All notable changes are documented here. Format follows
[Keep a Changelog](https://keepachangelog.com/); the project uses [SemVer](https://semver.org/)
(pre-1.0: the API is unstable and may change in minor releases).

## [Unreleased]

## [0.1.0-beta.1] - 2026-11-01

### Added
- **Dance `parent` and group values.** Every dance may name one `parent` (a single id, a strict tree; absent is
  the same as null). `Swing`, `Ballroom Standard`, `Ballroom Latin`, `Street Latin` and `Argentine Tango Family`
  are values a DJ can tag when a track suits the whole group; `Dimension.Normalize` drops such a group when a
  dance below it is also present. `Dimension.ChildrenOf(term)` lists the direct children.
- **Pack `early`** — One-Step, Polka and Fox Trot (alias Foxtrot, Social Foxtrot), the pre-swing social couple
  dances. `PackIds.Early`. Fox Trot is the social dance; the ballroom Slow Foxtrot stays in `ballroom`.
- **Closed groupings.** `Dimension.Grouping` (`TermGrouping`, `TermGroup`) names the term attribute that files a
  dimension's terms and lists its groups with a display label each: genre `family`, rhythmic_feel `cluster`,
  instrumentation `size`, theme `category`, and dances `family` (merged over the enabled packs). Mood,
  recording_context, recording_quality_era, credit_role, routine and song_form are flat by design; instrument
  and vocality are hierarchy-only (`parent`). `validate_data.py` gates every grouping (labels, filings, empty groups).
- **Dance colours are the dance's own**, unique across all packs and checked for 3:1 contrast against a dark
  background and a minimum OKLab distance by `validate_data.py`.
- **Rhythmic feels rank inside their cluster** by `tier`, or per scene in `feel_scenes.yaml`; a feel with neither
  fails validation.
- **Canonical-comment convention + `AppendCanonicalComment`.** The empty-description `COMM` frame (any
  language) is the user's free text: no label, prefix or schema. A tool that adds a note appends it after
  `"; "` and never rewrites, reorders or de-duplicates the existing text; structured data goes to its own
  field or a tool-private frame. `Id3TaxonomyExtensions.AppendCanonicalComment` implements the write side and
  skips a blank note or one already present, so a writer that runs twice does not stack it.
- **Blues idioms in the `blues` pack** (family `blues-idiom`): Struttin', Savoy Walk, Texas Shuffle, Chicago
  Triple, Piedmont Triple and Funky Butt, each described from blues-dance sources. None carries a tempo:
  no source gives an idiom a BPM range. Grind and Fishtail are **not** values — sources treat them as
  movement vocabulary inside jookin', with no music profile a DJ would program for.
- **Genre `West Coast Blues`** — 1940s Los Angeles / Bay Area blues by Texas and Oklahoma musicians (T-Bone
  Walker, Lowell Fulson, Charles Brown); the music Texas Shuffle is danced to.
- **Packs `west-coast-swing` and `carolina-shag`** — each its own opt-in pack because each is a separate
  scene: West Coast Swing 80–128 BPM, Carolina Shag 100–132 BPM (sources in each `tempo_note`). New ids:
  `PackIds.WestCoastSwing`, `PackIds.CarolinaShag`, `DanceTradition.WestCoastSwing` / `.CarolinaShag`.
- **Core dimension `routine`** (`DimensionIds.Routine`, typed `Routines`) — Shim Sham, Big Apple, Tranky Doo,
  Jitterbug Stroll, each pointing at its dances through `typical_dances`; it backs the `routine` field. No
  tempo: no source gives routine BPM, and the tagged recording carries its own. A routine names a
  choreography, so this is a deliberate exception to the music/dance fault line (`data/vocabulary/README.md`).
- **Descriptions for the 21 ballroom, street-latin and tango dances**, which had none.
- **Personal profiles persist in `.sdvo` (schemaVersion 2).** `SocialDanceVocabulary.SaveProfile` /
  `LoadProfile` round-trip a full `PersonalProfile` — the vocabulary overlay plus the read-time rule
  sections `cross_dimension`, `colors` and `from_path` — beside the overlay-only `SaveOverlay` /
  `LoadOverlay`. A v0/v1 file loads as a profile with empty rule sections; gate-token aliases are not persisted.
- **`FieldCatalog` — runtime field metadata.** `gen_ids.py` emits `FieldId → FieldMetadata` (display name,
  description, `dj_use`, group, layer, backing dimension) from `data/fields.yaml`, so a consumer can show
  what SDTS says a field is without parsing YAML. `TaxonomyValueTranslator` reads the field→dimension
  mapping from it instead of a hand-written ternary.
- **`SdtsStandard.Version` + `IsCurrent`.** The standard-version constant (from `data/fields.yaml`
  `version`) that every consumer stamps into `sdts_version`, with an exact-match compare-on-load contract.
- **`original_title` field** (`TXXX:SDTS_ORIGINAL_TITLE`) — completes the original-work cluster next to
  `TOPE` and `TDOR`; `TOAL` stays the original *album*.
- **Mirror-sync policies.** `MirrorSyncOptions.AbsentMirror` (`Cleared` default | `Unknown`: an absent
  mirror frame never imports as a deletion) and `SecondaryMirror` (`Realign` default | `Leave`: only the
  primary mirror key is written, so a legacy secondary is neither flattened nor minted).
- **`SdtsArtifactsPath` build property.** A consumer that builds SDTS from source passes
  `-p:SdtsArtifactsPath=<dir>` to move every SDTS output (bin and obj) under `<dir>`, so parallel builds
  sharing one checkout do not race. Unset, the build is unchanged.
- **Aliases for four library values:** mood `Cheerful` → Joyful and `Sassy` → Playful, rhythmic feel
  `Swinging` → Jazz Swing 8ths, genre `Funky` → Funk.
- **The `SdtsFile` typed record gateway** — whole-record ID3 read/write off a file in one open→save cycle
  (SDTS `TXXX` fields + standard TCON/TMOO/TBPM/POPM frames + audio + arbitrary TXXX) via typed per-field
  accessors, a **TagLib-free consumer surface** (a consumer referencing only `SocialDanceTaggingStandard.Id3` names no
  `TagLib` type). Plus the storage-axis `ISdtsContainer` abstraction (`Id3File` is the first
  implementation) over the existing `FieldId`-keyed confirmation-gate ledger.
- **Public-project scaffolding — governance, CI, validation, conformance, Python.**
  - **Governance:** `CONTRIBUTING.md` (two tracks — minimal-fix vs. taxonomy — + a source-vs-derived
    table), `.github/` issue + PR templates, and **file-level dual licensing**
    (`LICENSE-SPEC` = CC0-1.0 for `docs/` + `data/` + `conformance/`; Apache-2.0 for code).
  - **CI** (`.github/workflows/ci.yml`): a data+docs-in-sync gate (`validate_data.py` then
    `gen_field_docs.py` + `git diff --exit-code`) and .NET build+test (ubuntu + windows).
  - **Validation harness:** JSON Schemas for `data/fields.yaml` + `data/field-invalid-examples.yaml` and
    `scripts/validate_data.py` (schema + cross-checks; no external data needed).
  - **Cross-language conformance + validator:** `conformance/vectors/` (StableHash, the 3-way reconcile
    matrix, AND per-field value validation) — the language-neutral contract any future port must satisfy,
    consumed by the .NET suite. Adds a
    cross-language **validator** (`FieldValidator` / `validate`) driven by a machine-checkable
    `data/field-constraints.yaml` (int range, enum membership, inline grade, regex) →
    `scripts/gen_conformance_vectors.py`. `StableHash` promoted to public with a normative spec
    (`docs/standard/stable-hash.md`).
  - **Docs site:** `mkdocs.yml` (Material) + `.github/workflows/docs.yml` (GitHub Pages). The doc
    generator now emits absolute GitHub URLs for out-of-`docs/` links, so the rendered site and GitHub
    both resolve.
  - **Hand-maintained baseline vocabulary:** `data/vocabulary/` is the normative source of truth for
    taxonomy values — edited directly via PR and compiled into the C# baseline by `scripts/gen_vocab_docs.py`.
    The private DJ Catalog value-source, the `scripts/import_taxonomy.py` importer, the
    `data/vocabulary/proposals/` overlay (+ its schema), and per-term `origin` provenance were dropped: a
    single hand-owned source replaces the import-and-overlay model. Contribute terms by editing
    `data/vocabulary/` directly (see `CONTRIBUTING.md` Track B).
- **.NET sync libraries — `Sync` core + ID3 sources + MediaMonkey bridge.** A format-neutral,
  value-agnostic **3-way reconcile engine** (`SocialDanceTaggingStandard`, `…Sync` namespace:
  `MirrorSyncEngine` / `TagMirror`) with baseline change-detection (compact `field=hash` `SyncState`),
  conflict resolution (`ReportOnly` default / `PreferCanonical` / `PreferMirror`), direction + write-through
  policies (`MirrorDirection`, `WriteThrough`), and an `IValueTranslator` seam defaulting to identity
  (no taxonomy yet). Plus ID3 `ISdtsSource`s over TXXX + COMM frames (`…Id3`, TagLibSharp) and
  `SocialDanceTaggingStandard.MediaMonkey` — import / export / sync of the canonical SDTS fields against
  MediaMonkey's legacy `COMM:Songs-DB_*` frames (structural mapping only). Builds on net8.0;
  engine **unit-tested** (xUnit — 45 tests in `dotnet/tests/SocialDanceTaggingStandard.Tests` covering the
  3-way matrix, direction + conflict + write-through policies, the `SyncState` codec, the MM5 frame map, and
  map composition). **Downstream-extensible:** a consumer (the reference implementation) builds its own lib on top and
  adds PRIVATE tags via composable `FieldMirror` maps (`MirrorMap.Compose`, collision-checked) +
  `MediaMonkeyMirror(additionalFields:)` + a custom `IValueTranslator`; the standard owns `SDTS_*`, private
  tags use a downstream prefix (`SB-*`), and the engine never touches frames it isn't mapped to.
- **`artist_country` field** (proposed) — country of the artist's/music's ORIGIN, for country/region-themed
  sets ("only French", "US-only"). Genre model: canonical country name + ISO 3166-1 alpha-2 alias; value
  set is ISO (external standard, no catalog content). `TXXX:SDTS_ARTIST_COUNTRY`. Distinct from
  release-country / recording-location.
- **Cross-scene scope rule (scene-agnostic design).** New section in `field-demand-analysis.md`: SDTS
  carries no scene-specific tags — generalize the field *shape*, parameterize values per-dance
  (catalog-owned), don't mint per-scene fields. Backed by an adversarially-verified 9-scene audit (tango,
  salsa, bachata, kizomba, WCS, ballroom, forró, country, merengue): idiom-purity / style-variant /
  routine-song recur across scenes; `edit_provenance` deferred (trigger: a contemporary-music scene);
  tango (rhythm-family / cortina / orquesta-key) and ballroom (Paso grid) residue stays scene-scoped.
  Catalog ratification handoff: `socialdancetags-2026-06-22-cross-scene-axes`.
- **Field catalog split by origin + Original-\* frames + invalid examples.** The per-field docs now live in
  `docs/standard/fields/id3/` (real ID3-standard frames + the SDTS fields that bind to one) and
  `docs/standard/fields/sdts/` (SDTS-defined). Added documented **Original-\*** standard frames — `TOPE`
  (Original Artist), `TOAL` (Original Album), `TDOR` (Original Release Date) — with when/how-to-use for
  covers / tributes / reissues. Every doc now shows **valid and invalid** examples; the counter-examples
  live in `data/field-invalid-examples.yaml` and are merged by the generator.
- **Per-field catalog generated from the registry.** Each SDTS field now has its own document under
  `docs/standard/fields/<id>.md`, grouped by category with an index (`docs/standard/fields/README.md`)
  and cross-links. They are **generated** from `data/fields.yaml` by `scripts/gen_field_docs.py` — the
  registry stays the single source of truth (no hand-maintained duplication). `spec.md` is slimmed to the
  conceptual overview (binding model, variable-tempo, rating-scale family, appendices) + a pointer to the
  catalog.
- **Docs organised in two parts** as **physical folders** (index: `docs/README.md`): **Part 1 —
  the standards** in `docs/standard/` (spec.md, field-provenance.md, field-demand-analysis.md = what
  SDTS/SG define; the machine-readable `data/fields.yaml` stays at the repo root) and **Part 2 —
  synchronization with other software** in `docs/sync/` (sync-strategy.md + tool-comparison.md +
  cross-app-field-matrix.md). All inter-doc links and the root README / CLAUDE.md pointers updated.
- `docs/sync-strategy.md` — the strategy for making SDTS tags usable in / synced with other apps:
  file as source of truth, canonical + projections (Songs-DB_* mirror for MM5 columns, standard frames
  for universal/DJ readers), per-tool sync recipe, read-side normalization via `sdts_version`,
  conflict resolution, and what deliberately does not sync (DJ cues/grid, library stats).
- **Review status in the .NET API.** `DancePack.ReviewStatus` and `Dimension.AttributeReview` expose the
  expert-review flags as the new `ReviewStatus` enum (`Unflagged`, `ExpertReviewPending`), so an app can
  mark unconfirmed packs and attributes the way the docs do. A vocabulary rebuilt by an overlay or a
  profile keeps them, and the drift gate compares them. **Breaking:** the `DancePack` constructor takes
  the status as a new last parameter.
- **Review flags for unconfirmed vocabulary attributes.** A core dimension file's top-level `validation:`
  map lists attributes a domain expert has not confirmed; `core/genre.yaml` lists `typical_rhythmic_feels`
  and `typical_song_forms`. The genre page opens with an "Expert review pending" note and puts a ⚠ link
  on every flagged line, so a reader who deep-links to one genre still sees it (STD-F-13).
  `scripts/validate_data.py` rejects a flag naming an attribute no term carries, an unknown status, or a
  flag in a pack's `dances.yaml`.
  - A pack marked `validation: expert-review-pending` in its `pack.yaml` (ballroom, carolina-shag,
    street-latin, tango, west-coast-swing) gets the same note on its page and a ⚠ link in the dances
    index; `validate_data.py` checks that status too.

### Changed
- **`tempo` is a social band**, not a competition tempo (spec §Fields, `dance`).
- **Genre `family` values reconciled** to one declared list of 22.
- `StableHash` lives in `SocialDanceTaggingStandard.Internal`; tests and samples import it.
- **Blues band widened 50–120 → 50–160 BPM.** The old ceiling excluded the fast blues the scene dances
  (120–160+, the consumer's dance-taxonomy research); recorded in the term's `tempo_note`.
- **Dance scope settled.** Any dance family may ship as an opt-in pack of coarse values (one value per
  dance, tempo, typical genres, description); scene *machinery* — scene-specific fields and sub-style
  granularity — stays out (`docs/standard/field-rationale.md`). Modern Jive stays documentation-only until a
  consumer DJs it.
- **The `#`-markers `#JAM`, `#BD`, `#BirthdayJam`, `#HDC`, `#C`, `#Z` stay out of the portable standard.**
  They are programming and status markers (jam song, birthday dance, Herräng suitability, challenging,
  popular/familiar) — the excluded `status` namespace, like `#FC`/`#CP`/`#I`/`#NI` — not dances or themes.
  An intrinsic property they imply already has a field: `#HDC`'s period authenticity is `recording_origin`
  + `recording_quality_era` + `genre`; `#C` is `danceability` / `tempo_stability` / `meter`.
- **Legacy catalog pairs and name collisions checked.** `#CSH`/`#CS`, `#STLS`/`#STL`, `#SD`/`#SDG` each survive
  as one value (the `#`-form only in the legacy-hash overlay; the bare stubs exist nowhere); `#OS`/`#1S` is
  One Step, in the deferred `early` family. No status marker is a value in any dimension. "Stride" lives in
  genre (Stride Piano) and rhythmic feel, one per dimension. Struttin' takes no "Strut" alias (the New
  Orleans Strut is a separate idiom) and Chicago Triple no "Chicago Shuffle" alias (a rhythmic-feel canonical).
- **`dance` / `routine` field docs corrected:** `#`-prefixed codes resolve only through a personal overlay,
  never through the baseline (true since the `#`-code relocation, stale in the field text until now).
- **`.sdvo` schemaVersion 1 → 2 (breaking for older readers).** A reader that knows only v1 refuses a v2
  file instead of silently dropping its rule sections. The shipped example overlays are v2.
- **`#`-prefixed dance codes removed from the baseline (breaking).** `#LH`, `#BLB`, … were one DJ's
  personal shorthand. All 17 moved to `data/vocabulary/examples/legacy-hash-codes.sdvo.yaml`, which a
  library holding such values loads as an overlay. The MediaMonkey dance mirror writes the bare display
  code (`LH`, `BAL`, `SLS`) and falls back to the canonical name when an overlay rebound that code.
- **`Id3CommentSource` matches `COMM` frames across languages.** It prefers the configured language
  (`XXX`), reads the sole other-language frame when that one is absent, and on write reuses it in place
  with its language normalised instead of minting a duplicate. Two candidates stay unresolved.
- **Fixed: concurrent `SaveOverlay` writers could publish each other's half-written bytes.** Each write
  uses its own scratch file (`<target>.tmp<pid>-<n>`), retries the commit briefly under contention, and
  removes its scratch on failure.
- **Vocabulary token collisions now fail instead of silently resolving to the wrong term (breaking).**
  `Dimension` resolves canonicals, aliases, and display codes through one case-insensitive lookup and
  used to swallow every duplicate — `R&B Combo`'s code `RBC` collided with `Rockabilly Combo`'s `RbC`,
  leaving `R&B Combo` unreachable by its own code with no error anywhere.
  - `R&B Combo`'s display code is now **`RnB`**. `RBC` still resolves to `Rockabilly Combo`, which held
    that token before, so no stored value changes meaning.
  - The `Dimension` constructor throws `SdtsVocabularyException` when two terms claim the same token
    ignoring case, or when one id is defined twice. The single collision with a decidable winner stays
    legal: a display code YIELDS to another term's explicit canonical/alias — that is how a personal
    overlay rebinds a code it does not use (a DJ binding `SLS` to Salsa instead of St. Louis Shag).
    `LoadDefault(FileInfo)`, `With`, and `Customize().Build()` throw — they are programmatic calls, and
    an edit that could never resolve is a bug in the caller. The two configuration-driven paths stay
    total: `TryLoadDefault` reports a colliding overlay as `false` like an unparseable one, and
    `WithProfile` drops just the colliding edit, reports it as an error in the profile's
    `ValidationReport`, and applies the rest — the same treatment a dangling cross-dimension rule
    already got. `Dimension` remains the only judge of what collides; the profile path asks it rather
    than restating the rule.
  - Aliases that only repeated their own term's display code were dropped (`Arr`, `Eng`, `Mix`, `Perc`,
    `Son`) — the bare code already resolves. The genre alias `LoFi` went too (`Lofi` already claims that
    token case-insensitively, so it was dead data).
  - `scripts/validate_data.py` now gates `data/vocabulary/`, which has no schema. It is stricter than the
    runtime: in the shipped baseline no token may repeat at all, the rebinding included, except a display
    code equal to its own canonical (`Pop` / `POP`).
- **`typical_genres` names genre canonicals only.** Every entry on dances, instrumentations and song
  forms now equals a `core/genre.yaml` canonical, and `scripts/validate_data.py` fails on an alias, an
  unknown genre, or a repeat, naming file, term and value.
  - 17 references through an alias moved to the canonical (`Stride` → `Stride Piano`, `NOLA Jazz` →
    `New Orleans Jazz`, `Rock'n'Roll` → `Early Rock 'n' Roll`, …), so renaming an alias cannot break them.
  - Of 16 references that resolved to nothing, those naming a real genre were mapped: `Early Rock n Roll`
    → `Early Rock 'n' Roll`; `Tin Pan Alley`, `Movie Standards`, `Jazz Standards` → `Adult Standards`;
    `Race Records` and `Vintage Blues` on Blues with Verse → `Classic Vaudeville Blues`; `Vintage Blues`
    on 8-bar Blues → `Country Blues` + `Delta Blues`; `Folk-influenced` → `Folk`; `Atypical Jazz` →
    `Jazz`. Free descriptors were dropped (`Hymn`, `Wagner Opera`, `Medieval German`, `Minor Blues`,
    `Vintage`, `Marching`, `Modern`); Bar Form's and ABAC's descriptions now carry the origin they implied.
- **Genre `typical_song_forms` names song-form canonicals only.** 66 of 159 entries used a `dsl_key`
  (`Blues12` → `12-bar Blues`, `Blues8` → `8-bar Blues`, `AABAWithVerse` → `AABA with Verse`,
  `BluesWithVerse`, `Blues16Minor`, `Double16`) or the case variant `Bar form` → `Bar Form`; they now
  equal the `core/song_form.yaml` canonical. `Genre.TypicalSongForms` returns the canonical, and the genre page's
  song-form links no longer point at anchors that do not exist.
  - `scripts/validate_data.py` holds `typical_song_forms` and `typical_rhythmic_feels` to the same rule as
    `typical_genres` (one check, `typical_reference_errors`): an alias, display code, `dsl_key`, case
    variant, unknown value or repeat fails, naming file, term and value and suggesting the canonical.
- **`data/vocabulary/` has one copy of each value.** The standalone `associations.yaml`, `audio.yaml`,
  `display.yaml` and `translations.en.yaml` are gone: their content is already on each term in `core/`
  and the packs, and nothing read them. They had drifted — no `west-coast-blues` in two of them, six
  themes removed from `core/` in June still listed, `instrument_led` for the renamed `instrument`
  dimension. Edit the term itself. The expert-review-pending marker for typical feels and song forms
  moved into `core/genre.yaml`, and the colour and `audio.youtube` authoring notes to
  `data/vocabulary/README.md`.
- **`ITagSource` renamed to `ISdtsSource`** (keyed by `TagKey`, with `Remove`/`Contains`); `genre`/`mood`
  now use the standard `TCON`/`TMOO` frames — the redundant `TXXX:SDTS_GENRE`/`SDTS_MOOD` mirrors were
  dropped (routing rule 1: a standard concept with a real ID3 frame uses the frame).
- **Validation results are strongly typed (breaking).** `FieldValidator.Validate` now returns
  `FieldValidationResult` (a `ValidationError?` enum, wire code via `ToWireCode()`) instead of a
  `(bool, string?)` tuple; `ProfileValidator.Validate` now returns a shared `ValidationReport`
  (severity-tagged `ValidationMessage` list) and `ProfileValidationReport` is removed. New vocabulary
  lives in `SocialDanceTaggingStandard.Validation`. Wire codes and validation behaviour are unchanged.
- **Renamed field `routine_song` → `routines`** (display name "Routine Song" → "Routines"; frame
  `TXXX:SDTS_ROUTINE_SONG` → `TXXX:SDTS_ROUTINES`, with the matching Vorbis `SDTS_ROUTINES` / MP4
  `----:com.apple.iTunes:SDTS_ROUTINES`). The old singular `routine_song` read like a boolean ("is this a
  routine song?"); the field is a plural `enum[]` naming WHICH choreographed routine(s) a track is the song
  for, parallel to `dances`. Normative field-id change — also updates the conformance vector
  `conformance/vectors/validation/fields.json`. No semantics changed.
- **Field docs reformatted + de-duplicated; standard stripped of catalog vocab + vendor impl.** The generated
  per-field docs now lead with a one-line use, a compact facts table (Type · Values · Tag · Status), then
  **Values** (the contract) · **Examples ✓ / Avoid** · **Notes** · **Related** — dropping the duplicate
  "DJ use", the "Other tools" wall, and the Vorbis/MP4 sprawl. Per the taxonomy-authority split,
  `allowed_values` no longer **enumerates** catalog vocabularies (genres / dances / forms / moods / feels /
  instruments / themes / routines) — it states the value SHAPE + a "Value set: DJ Catalog → X" pointer; the
  concrete lists live in the DJ Catalog, not the standard. The canonical serialization shown is the `SDTS_*`
  tag (or the real standard frame TCON/TMOO/TBPM/POPM); legacy vendor frames (`COMM:Songs-DB_*`) are demoted
  to a one-line **Notes** → Part 2 sync. 16 bloated descriptions trimmed to concise definitions (e.g. `dances`
  1488 → 498 chars). No field semantics changed — registry contract + serialization are unchanged.
- **Removed `set_role`.** Not a real social-dance-DJ need: the night-arc concept exists in club DJing as
  an ENERGY scalar (Mixed In Key), but swing/blues DJs program reactively, and `set_role` heavily overlapped
  `energy` (peak/warm-up) + `rating` (opener-vs-best). The community-painpoint research never surfaced
  set-position tagging. The one distinct bit (first-dance/last-dance special slots) is better served as a
  personal playlist/filter than a per-track tag (same derive/filter-don't-store logic as `decade`).
- **`routine_song` adopts the dances model.** Values are now canonical routine names WITH spaces (`Shim
  Sham`, `Big Apple`, `Tranky Doo`, `Jitterbug Stroll`; spaceless / `#`-forms are aliases) carrying an
  inline FITNESS tier — `:original` (THE canonical recording) / `:alternative` (vetted substitute) /
  `:experimental` (fits but demands much more from dancers) — plus the optional trailing `?`. Replaces the
  old `{routine_id}:{role}` with role ∈ primary/alternate/caller; the `caller` axis is **dropped** (it
  graded a called/jam VERSION, not fitness). No more `object[]` framing — it's a plain `; `-delimited
  `enum[]`, structurally identical to `dances`.
- **Removed `phrase_structure`.** It was a JSON-in-a-tag composite (`{phrase_bars, regularity, breaks}`)
  and only `proposed` (never active). All three parts are already covered: `phrase_bars` → `song_form`
  (bar count is form metadata), `regularity` → `song_form`'s `Irregular` value, break positions → the
  private SB-Beats/SB-Beatinfos caches (positional, not a portable tag). The Tranky Doo case (12-bar song,
  32-bar routine) is `routine_song`, not a music-property field. `song_form` stays.
- **`dances` canonical home clarified: `SDTS_DANCES` is authoritative; `Songs-DB_Occasion` is a mirror.**
  Unlike genre/mood/bpm/rating (which keep their real standard frame — TCON/TMOO/TBPM/POPM — as canonical),
  dances has NO standard ID3 frame, so the custom `TXXX:SDTS_DANCES` IS its canonical home, not a mirror of a
  vendor frame. The legacy MediaMonkey `COMM:Songs-DB_Occasion` is demoted to a Part-2 **sync mirror**
  (canonical-name ↔ `#`-code projection) kept only for MediaMonkey-column compatibility. This removes the
  'Occasion' misnomer from the standard itself — Part 1 now only ever names `SDTS_DANCES`.
- **Removed `primary_dance` and `dance_versatility`.** Both are now read straight off the `dances` list:
  it is ORDERED, so the FIRST dance is the primary/headline dance, and the COUNT of dances is the track's
  versatility. One ordered multi-value field carries which-dances + primary + breadth — no separate fields,
  and nothing is listed-but-undocumented anymore (`SKIP_DOCS` is empty).
- **Dropped `content_advisory`; split `intro_outro`; `meter` accepts both forms.** `content_advisory`
  removed (re-addable later if a real need appears — aligns with `ITUNESADVISORY` then; non-destructive).
  `intro_outro` (a JSON composite) split into three plain fields — `intro_duration` (ms), `outro_duration`
  (ms), `ending_type` (definitive/fade/cold-stop) — no JSON in a tag, each independently queryable. `meter`
  now accepts EITHER a full time signature (4/4, 3/4, …) OR a bare beats-per-bar grouping (2/3/4): '3'
  covers the waltz family whether 3/4 or 3/8; subdivision (12/8 vs 4/4) lives in `rhythmic_feel`. Also: the
  rating-family half-star presentation is now documented (1-10 stored, ½★…5★ shown).
- **Removed `floor_profile`, `movement_space`, `felt_bpm`, `felt_tempo_relation`.** `floor_profile` /
  `movement_space` (spot vs traveling) are dance-derivable (`dances` + tempo + energy), not per-track.
  `felt_bpm` (+ companion `felt_tempo_relation`) modelled half/double-time, better handled by setting `bpm`
  to the danced pulse; the per-dancer "dances slower because it's too fast" case is out of scope.
  (`TXXX:SDTS_FLOOR_PROFILE` / `…_MOVEMENT_SPACE` / `…_FELT_BPM` / `…_FELT_TEMPO_RELATION` gone.)
- **Removed `decade` and `bpm_unit`.** `decade` is derivable from the year frame (`TDRC`) — a smart-playlist
  filter, not a stored tag; `bpm_unit` (beats- vs bars-per-minute) is ballroom-only and `bpm` is always
  beats-per-minute for swing/blues. Both dropped (`TXXX:SDTS_DECADE` / `TXXX:SDTS_BPM_UNIT` gone); cross-field
  prose referencing them updated. The frozen `COMM:Songs-DB_Custom1` (legacy decade data) is unaffected.
- **`dances` follows the genre model: canonical names + aliases.** The stored form in `SDTS_DANCES` is the
  canonical dance NAME (`Lindy Hop`), like genre stores `Swing; Big Band`; the legacy `#`-codes (`#LH`, …)
  become **aliases** that resolve to the canonical name. The per-name `:core|edge|fusion` purity grade +
  trailing `?` (needs-validation) are retained. Display forms (abbreviation / colour / flag) are catalog
  per-term metadata (like genre's family/era/bpm_range), so **no separate presentation mechanism** is
  needed. Frozen `COMM:Songs-DB_Occasion` keeps the `#`-codes on existing files; `SDTS_DANCES` uses names.
- **Dropped `blues_idiom_purity`; idiom-purity is now per-dance, inline on `dances`.** Purity is
  per-(track×dance), so it can't be a track-level scalar; it now rides on each dance tag as an optional
  `:core`/`:edge`/`:fusion` grade (e.g. `Blues:fusion`, `Blues:edge?`) — a 2nd per-dance axis, with the deviation *direction*
  (jazz/soul…) carried by genre. The legacy fitness suffix (`!`/`!!`) and the `!CC` AI marker are
  **dropped from the standard** (per-dance danceability → `danceability`/`tempo_stability`); a trailing
  `?` flags needs-validation. Scene-agnostic by design (blues
  core/fusion ≈ tango trad/nuevo ≈ salsa dura/timba), validated across 9 partner-dance scenes. The
  tag-syntax extension + per-dance value sets are pending DJ Catalog ratification (cross-scene-axes
  handoff); the `TXXX:SDTS_BLUES_PURITY` field is removed.
- **`Grouping` (`TIT1`) decomposed, not carried as a portable field.** Corrected the spec/provenance:
  `TIT1` is the catalog `workflow_tags` bag (6 categories: analytic/personal/recording-meta/
  classifier-meta/object-state), not the "object-state + `#HDC`" the old Appendix A claimed (`#HDC` is a
  dance-context Occasion tag). Per the decompose decision, `TIT1` stays a frozen pass-through frame and is
  **not** a portable SDTS field: intrinsic bits migrate (Medley→`song_form`, Cover/Tribute→`recording_origin`,
  recording-meta→a quality-flags field) and the library/personal/triage markers (Canonical/Duplicate, `#?`,
  `#FC`, `#CP`, `#I`, `#NI`) stay out of portable scope. Value-level migration is pending DJ Catalog
  ratification — handoff `socialdancetags-2026-06-22-grouping-decomposition`.
- **`theme` no longer holds dance-floor `#`-tags.** `theme` is now seasonal/event/listening occasions only
  (Christmas, Cocktail, Heartbreak …); the `#`-prefixed markers (`#JAM`, `#BD`, `#BirthdayJam`, `#HDC`,
  `#C`, `#Z`) leave `theme`. Their disposition is settled above: they stay out of the portable standard.
- **Time fields drop the unit from their name.** `fade_in_ms`/`fade_out_ms` → `fade_in_duration`/`fade_out_duration`
  (`TXXX:SDTS_FADE_IN_DURATION` / `…_OUT_DURATION`); the `intro_outro` keys `intro_ms`/`outro_ms` →
  `intro_duration`/`outro_duration`. The unit (integer ms — matching `TLEN`, the beat grid, and
  `playback_start`/`playback_stop`) lives in the type, not the name.
- **Spec de-coupled from MediaMonkey.** Part 1 (`docs/standard/spec.md`, `field-provenance.md`, and the
  `data/fields.yaml` registry) no longer names MediaMonkey's private tags/columns. The frozen
  `COMM:Songs-DB_*` frame names stay (documented as opaque on-disk storage, per the provenance
  reframing), but the MM5 column mapping (`Custom Field N+1`), the "MM5 mirror" language, and the
  MediaMonkey `Quality`/`Tempo`/`Occasion` false-friends moved to Part 2 — a new **reused-slot ↔ MM5
  column map** in `docs/sync/tool-comparison.md`. Neutral standard-frame interop (MediaMonkey listed
  among the tools that read `TMOO`/`POPM`/`TLAN`) is kept — those are not MediaMonkey-private tags.
- **Named `rating` type:** `rating`, `energy`, `danceability` now carry the data type **`rating`** —
  a discrete 1-10 integer scale (1 lowest … 10 highest; `rating` adds 0 = unrated via POPM). `energy`
  and `danceability` moved 1-5 → 1-10. The TXXX fields store a plain integer, **not** POPM's 0-255 byte.
  Categorical ordinals (set_role, blues_idiom_purity, movement_space, floor_profile) stay enums.
- **Provenance reframed (`field-provenance.md`):** the `Songs-DB_Custom*` / `Songs-DB_Occasion` frames
  are now documented as **reused storage slots, not MediaMonkey features** — every value in those
  fields is **defined by the reference implementation / the DJ catalog**. The doc's buckets are now Standard ID3 (5+3),
  **reference-implementation-defined** (34: 11 in reused slots + 23 net-new `TXXX:SDTS_*`), and consumer-private (18);
  the misleading "recycled from MediaMonkey" framing is gone.
- Defined the `dances` **per-dance fitness suffix** as a single axis (kept by user decision):
  bare = fits; `!` = strongly suited; `!!` = a stretch (fits only with imagination); `?` = not yet
  classifiable; `!CC` = AI-suggested (separate marker). Marks are **non-monotonic** (`!` stronger
  than bare, `!!` weaker than bare). Consumers must strip the suffix to filter by dance. **Supersedes**
  the older catalog definition (`?`=low-confidence, `!!`=exemplar) — the DJ Catalog must be updated to match.
- Renamed the dance field `occasion` → **`dances`** (it holds dances, not occasions — "Occasion" was
  a misnomer inherited from the repurposed MM frame). The listening-occasion / situation concept is
  the separate `theme` field. On-disk frame stays the frozen `COMM:Songs-DB_Occasion`; portable name
  is now `TXXX:SDTS_DANCES`.

### Added
- `docs/field-provenance.md` — classifies every tag by origin: **standard ID3** (5+3, use as-is),
  **recycled** (13, mostly MediaMonkey `Songs-DB_*` frames carrying catalog-defined values; + MusicMatch,
  Apple ITUNESADVISORY, MusicBrainz/Discogs concept-level), **new in SDTS** (21, the dance-DJ +
  measured/derived fields with fresh `TXXX:SDTS_*` names — the standard's real contribution), and
  **consumer-private** (18, `SB-*` analysis caches, not portable). Columns: type + note. Includes the
  time-stamped-markers convention (typed lanes; warnings + notes merge into one `SB-Markers` lane).
- `docs/cross-app-field-matrix.md` — emoji-coded matrix of **where** each app stores each field:
  📄 file tag · 🗄️ internal DB / library · 🔀 both · ➖ unsupported. **The reference implementation (SDTS)** is the
  reference first column in every table — file-first (📄) throughout, including the beatgrid + analysis
  cache that DJ tools keep DB-side. Surfaces what does NOT travel with the file (Apple Music Start/Stop
  & JRiver Playback Range/Bookmark DB-only; Rekordbox/Mixxx cues+grid DB-only; Roon/Plex library-first).
- **`sdts_version`** field (`TXXX:SDTS_VERSION`) — the SDTS schema version a file was last normalized
  under. On load, compare to the current version: older → migrate + re-normalize, equal → skip. Makes
  normalization a cheap, idempotent one-line check (distinct from `SB-Migrations` one-shot tokens).
- **Variable tempo (multiple BPMs)** convention in `spec.md`: `bpm` = single dominant/main-section
  tempo, `tempo_stability=tempo-change` flags variability, the beat grid (`SB-Beats`) carries full
  fidelity, optional `bpm_min`/`bpm_max` for queryable range. Research confirmed there is no portable
  multi-BPM standard (TBPM/tmpo/Vorbis BPM are single-value; ID3 SYTC/MLLT are dead; DJ-tool beatgrids
  are proprietary + out-of-file and still surface one headline BPM). Don't overload TBPM; don't rely on SYTC.
- Per-field precise **data type** (`type`) in `spec.md` / `fields.yaml` — a language-neutral signature
  with units, ranges and shapes (e.g. `int (ms)`, `int (1-5)`, `enum[]`, `object ({routine_id, role})`),
  refining the coarse `data_type` + `cardinality`.
- Per-field **DJ use** note (`dj_use`) in `spec.md` / `fields.yaml` — what each tag lets a dance DJ
  actually do (e.g. occasion → "build for the room in front of you"; felt_bpm → "sort by the tempo
  dancers actually move to").
- `docs/field-demand-analysis.md` — demand × worth tiering (core 7 / recommended 16 / optional 9 /
  marginal 6) from a DJ + dancer community and dance-domain audit. Flags the AI-slop-risk **marginal**
  fields (instrument_led, song_form, lyric_language, bpm_unit, phrase_structure, movement_space),
  upgrades `content_advisory` + `recording_quality_era` to recommended, validates the SDTS dimensions
  against how the dance world categorizes music, and surfaces candidate new fields (familiarity /
  canon-status, teachability / dancer-level, energy-arc) plus MIR groove signals (syncopation,
  low-frequency energy). Confirms musical-key/Camelot should stay out.
- Full cross-tool audit: **every** portable field now has a researched `other_tools` note (no field
  left on the generic "SDTS-specific" placeholder). Covers vocality / instrumentation / instrument-led /
  recording context-quality-origin and the remaining fields (decade, theme, song_form, rhythmic_feel,
  set_role, tempo_stability, phrase_structure, meter, bpm_unit, intro_outro, routine_song,
  blues_idiom_purity, movement_space, floor_profile, primary_dance, dance_versatility,
  felt_tempo_relation, confirmation_gates). Notable findings: `recording_quality_era` must NOT be
  conflated with MediaMonkey/MusicBee "Quality" (a fidelity grade); `theme` aligns with the MusicMatch
  "Situation"/MM "Occasion" concept (SG stores it in Custom9 since Songs-DB_Occasion is repurposed for
  dance); `intro_outro` has positional prior art (ID3 ETCO, BWF cart); several have relational prior
  art in MusicBrainz/Discogs (performer/instrument/ensemble credits) rather than a flat field.
- Complete tag inventory: `spec.md` / `fields.yaml` now document **every** frame on a reference-implementation-tagged
  file — the portable fields plus **Appendix A** (standard music frames: TIT2/TPE1/TALB/COMM/TIT1, with
  SG's overloads) and **Appendix B** (20 consumer-private `SB-*` / `PRIV:Moodbar Cache` analysis
  frames, marked non-portable — they live in the reference implementation's private audio-tools library). Replaces the single
  audio-caches placeholder note.
- `playback_start` / `playback_stop` (cue-in / cue-out, milliseconds) — portable per-track playback
  range. JRiver `[Playback Range]` and iTunes Start/Stop Time offer this but keep it library-DB-only
  (it does not travel with the file) and there is no ID3 standard for it; SDTS carries it in
  `TXXX:SDTS_PLAYBACK_START` / `TXXX:SDTS_PLAYBACK_STOP`. (Nearest ID3 standard is the ETCO event-marker
  frame — descriptive, rarely populated — noted for future interop.)
- `fade_in_ms` / `fade_out_ms` (milliseconds) — fade ramps at the cue-in / cue-out, as separate
  scalars (cleaner than a `200000+2000` composite that reads as arithmetic). `0` = hard in / cold
  stop. Net-new: no container stores fade duration anywhere — not ID3, not the BWF `cart` chunk, not
  DJ tools (they fade at runtime). Curve is not stored; the player picks it (default equal-power).
  (38 portable fields total.)
- Initial scaffold under the standard `dotnet/` layout: `SocialDanceTaggingStandard` (format-neutral
  SDTS model) and `SocialDanceTaggingStandard.Id3` (ID3v2.4 binding via TagLibSharp).
- Apache-2.0 license, ecosystem `.editorconfig` mirror, `Directory.Build.props`, `global.json`.
- Field catalog: `docs/spec.md` (prose, one section per field) + `data/fields.yaml` (machine-readable
  registry) — 35 fields described across taxonomy, measured, confirmation, and proposed groups.
- `docs/tool-comparison.md` — interoperability of the custom fields across **32 tools** (taggers:
  Picard, Mp3tag, Kid3, MusicBee, beets, TagScanner, Yate, dBpoweramp, EasyTAG, Quod Libet;
  players/servers: MediaMonkey, JRiver, foobar2000, Apple Music, WMP, Winamp, AIMP, Roon, Plex,
  Jellyfin, Emby, Navidrome; DJ: Serato, Traktor, Rekordbox, VirtualDJ, Mixxx, Mixed In Key, Lexicon,
  beaTunes, djay) + a concept×tool matrix, the casing footgun, collisions and reserved namespaces.
- Per-field **Other tools** note in `spec.md` / `fields.yaml` (`other_tools`): each field's cross-tool
  equivalent, or "SDTS-specific". Surfaced alignment recommendations: `lyric_language`→`TLAN`,
  `content_advisory`→`TXXX:ITUNESADVISORY`, `energy`↔`ENERGYLEVEL` read alias.

### Removed
- **Instrument `covers`** on the generic instrument terms; `parent` is the one hierarchy.
