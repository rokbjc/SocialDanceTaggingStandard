# Social-Dance Tagging Standard (SDTS) — field catalog

The concrete per-field definition of SDTS. Machine-readable companion: [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml).
Tool interoperability (how these fields compare to other taggers): [`../sync/tool-comparison.md`](../sync/tool-comparison.md).

> **Notational conventions.** The key words **MUST**, **MUST NOT**, **REQUIRED**, **SHOULD**, **SHOULD NOT**,
> and **MAY** are to be interpreted as in [RFC 2119](https://www.rfc-editor.org/rfc/rfc2119) and
> [RFC 8174](https://www.rfc-editor.org/rfc/rfc8174) when, and only when, they appear in capitals.

## Binding model (two layers)

1. **Taxonomy (format-neutral)** — what each field means and its allowed values. The values are hand-maintained in [`data/vocabulary/`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/data/vocabulary), edited directly via PR, and dedicated to the public domain under CC0 (see `LICENSE-SPEC` for provenance).
2. **Serialization bindings** — how each field is stored. The same field name travels across containers via the cross-format triangle: ID3 `TXXX:NAME` = Vorbis `NAME` = MP4 `----:com.apple.iTunes:NAME`.

## Conformance

An implementation **conforms to SDTS** at a given spec version (see [versioning](versioning.md)) when it
satisfies all of the following:

- **Conformance vectors.** A conforming reader/writer MUST reproduce every applicable case in
  [`conformance/vectors/`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/conformance) — `hash.json`
  (StableHash), `sync/reconcile.json` (the 3-way reconcile matrix), and `validation/fields.json` (per-field
  value validation). These vectors are the cross-language contract; an implementation that diverges from them
  is non-conforming.
- **Binding triangle.** A portable field MUST use the same name across containers (the
  [binding model](#binding-model-two-layers) triangle). Where a real standard frame exists (`TCON`, `TMOO`,
  `TBPM`, `POPM`, `TLAN`, `TKEY`, `TSRC`), that frame is canonical and the `SDTS_*` tag MUST be treated as a
  mirror, not an independent source of truth.
- **Multi-value encoding.** A multi-value field MUST be encoded as a single `; ` (semicolon-space) delimited
  string; readers MUST split on `; ` and SHOULD tolerate stray surrounding whitespace.
- **Time unit.** Every time-valued field MUST be an integer number of milliseconds.
- **Frozen frames.** An implementation MUST NOT rename the frozen on-disk frames (`SB-*`, `COMM:Songs-DB_*`)
  on existing files; it reads and writes them under their existing names.
- **Version stamp.** A writer that normalizes a file MUST set `sdts_version` (`TXXX:SDTS_VERSION`) to the SDTS
  **spec version** it normalized under — a SemVer string equal to the `version` in
  [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml). A reader MUST
  treat a file whose `sdts_version` is lower than its own spec version as "needs migration", and SHOULD
  re-normalize before relying on derived fields.
- **Tempo.** A writer MUST NOT place more than one value in `TBPM`; variable tempo is signalled by
  `tempo_stability` and carried in the beat grid — never as a list or range in `TBPM`.
- **Packs.** An implementation MAY support only a subset of dance packs (e.g. swing + blues), but MUST NOT
  emit dance values outside the packs it claims to support.

### Conformance levels

Every field carries a `status` (see [How to read each field](#how-to-read-each-field)). Conformance is defined
over **`active`** fields only:

- A conforming reader/writer MUST read and write every `active` field according to its constraints in
  `validation/fields.json`.
- A writer MUST NOT emit a `proposed` field — it has no on-disk home yet. A reader SHOULD ignore any `SDTS_*`
  field it does not recognise (forward-compatibility) rather than reject the file.
- Supporting a `proposed` field is OPTIONAL and experimental: it does not affect conformance, and the field
  MAY change or be dropped before it is promoted to `active` (see [versioning](versioning.md)).

### What conformance does not require

Conformance is about the SDTS fields and the standard frames — **not** any one implementation's internals. A
conforming implementation is NOT required to produce or understand:

- the reference implementation's private `SB-*` analysis caches (Appendix B) — the **Frozen frames** rule
  above only means you MUST NOT *rename* an `SB-*` or `COMM:Songs-DB_*` frame you find on a file, **not** that
  you must write one;
- the `Sync/` baseline `SyncState` codec, the MediaMonkey `COMM:Songs-DB_*` mirror map, or any .NET API surface
  — these are how the reference library is built, not what the standard mandates.

Conformance is defined per spec version. Adding a field or a vocabulary term is an additive (minor) change
that does not break a conforming implementation; removing or renaming one is breaking — see
[versioning](versioning.md).

## How to read each field

Each field's own document (see the catalog below) carries these facets:

- **Status** — `active` (used today, stored in a frozen on-disk frame) · `proposed` (new SDTS field, not yet written) · `frozen-legacy`.
- **ID3 (now → proposed)** — the current frozen frame, then the portable `TXXX:SDTS_*` target. Frozen frames (`SB-*`, `COMM:Songs-DB_*`) are NOT renamed on existing files; where a real standard frame exists it stays canonical and `SDTS_*` is only a mirror (see [Conformance](#conformance)).
- **Other tools** — how the concept maps across other taggers/players, and whether SDTS aligns with a de-facto name or is SDTS-specific. Full matrix in [`../sync/tool-comparison.md`](../sync/tool-comparison.md).
- **Authority** — the SDTS baseline vocabulary owns values · `sdts` owns schema/binding · `both` where they meet.

A tagged file also carries standard music frames and reference-implementation–private analysis frames — catalogued in **Appendix A** and **Appendix B** below (the private ones are not part of the portable standard).

## Field-id naming

A field id is a wire token — it keys the confirmation-gate ledger and every taxonomy-agnostic field map —
so it is spelled by rule, not by ear:

- **A field id names its dimension, in the SINGULAR.** `mood`, `genre`, `dance`, `theme`, `rhythmic_feel`.
- **Cardinality is declared, never spelled.** Every field record already carries `cardinality: single|multi`;
  putting it in the name too duplicates a fact, and duplicated facts drift. A plural id would also imply the
  cardinality is part of the field's identity, when it is a property that could be widened.
- **The plural is reserved for fields that are lists of composite RECORDS**, not repeated picks from a
  dimension — `confirmation_gates` (field·version·hash triples) and `regions` (typed spans). There is no
  singular dimension behind those, so the plural carries information rather than repeating it.
- **The C# property is plural where it is a collection** (`SdtsRecord.Dances`, `.Moods`, `.Themes`) — the
  id names the concept, the member names the container. That split is declared once in `gen_ids.py`'s
  `ACCESSOR_OVERRIDE`, not spelled per call site.
- **Frames are frozen and out of scope.** `TXXX:SDTS_DANCES` stays plural after `dances` → `dance`: a frame
  description has no cardinality column, so on the wire the plural is the only signal that the value is a
  `; `-joined list. A field id and its frame are separate namespaces and may differ.
- **A field id may differ from the dimension it binds** when the two genuinely disagree: `instrument_led`
  binds the unified `instrument` dimension, and `dance` binds `dances` — plural there because that dimension
  is the cross-pack union of every pack's dance list, and it is a user-facing key in overlay files.

*History:* `dances` → `dance` and `routines` → `routine` were renamed under this rule while the SDTS ID3
write path was still unextracted, so no on-disk gate ledger carried the old tokens. Renaming later would
have required a migration: an unrecognised gate token is preserved verbatim as opaque, so a stale `dances:3`
would survive on disk while silently no longer matching the field.

## Field catalog

Each field has its own document, **split by origin** into `fields/id3/` (real ID3-standard frames — incl.
the **Original-\*** frames `TOPE`/`TOAL`/`TDOR` for covers & reissues — plus the SDTS fields that bind to
one) and `fields/sdts/` (SDTS-defined fields). Generated from
[`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) (valid + invalid examples; counter-examples in
`data/field-invalid-examples.yaml`):

**→ [Field catalog index](fields/README.md)**

Categories: Taxonomy — core (A) · Taxonomy — core (B) · Measured & confirmation ·
Proposed — tempo & structure · Proposed — dance semantics · Proposed — content & provenance.

Time unit: every time-valued field is **integer milliseconds** (matching `TLEN`, the beat grid, and `playback_start`/`playback_stop`); the unit lives in the field's type, not its name.

> The field docs and the catalog index are **generated artifacts** — edit
> [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) and run `scripts/gen_field_docs.py`;
> never hand-edit the files under `fields/`.

## Variable tempo (multiple BPMs)

There is **no portable standard** for multiple / variable BPM: ID3 `TBPM` (and MP4 `tmpo`, Vorbis `BPM`, …) are single-value by definition — "the BPM of the MAIN PART of the audio". ID3's `SYTC` (synchronised tempo codes) and `MLLT` frames *could* carry a tempo map but are **dead letters** — no mainstream tagger/player reads or writes them (TagLib keeps them only as opaque blobs). DJ tools (Rekordbox, Serato, Traktor) capture variable tempo only in **proprietary per-track beatgrids stored outside the file**, and every one of them still surfaces a single headline BPM.

SDTS therefore notates a tempo-changing track as:

1. **`bpm`** = the single dominant / main-section tempo — the value every tool and every sort relies on. Never a list or range inside `TBPM` (e.g. '120; 180' breaks every reader).
2. **`tempo_stability`** = `tempo-change` (or `accelerando` / `drift`) — the explicit flag that the tempo varies. This is the heads-up a DJ needs before cueing it.
3. **Full fidelity** lives in the **beat grid** (`SB-Beats` / `SB-Beatinfos`) — the complete tempo map; the instantaneous BPM at any point is derivable from beat spacing. This is exactly what DJ tools do with their proprietary grids; SDTS already carries its own.
4. *(optional, if you want it queryable)* **`bpm_min` / `bpm_max`** — a coarse range to find or avoid wide-span tracks. Net-new (no standard for it); not yet a field — add only if the query need is real.

Do **not** rely on `SYTC`/`MLLT` for interop (the single-value `TBPM` rule is normative under [Conformance](#conformance)).

## Tempo bands (genre & dance)

Each genre carries a `bpm_range` and each dance a `tempo_min`/`tempo_max` in the baseline vocabulary
(e.g. Lindy Hop 120–240, Balboa 180–300, the Blues genre 50–130). These are **advisory comfort bands** —
the tempo range at which the genre/dance typically works on a social floor — **not** hard limits and
**not** a corpus percentile. A `null` `bpm_range` means the genre is not floor music (tempo is not a
meaningful axis for it).

- A validator **SHOULD** treat a track whose `bpm` falls outside the union of its dances'/genres' bands as
  a **warning** — an outlier worth a human glance (e.g. a half/double-time detection) — and **MUST NOT**
  reject the track on that basis: legitimate tracks (half/double-time, edge tempos, multi-genre) routinely
  sit outside a single band.
- A dance band is a **typical social-floor band**, one per dance. Where a competition body codifies the
  dance (the ballroom pack: WDSF/WDC), the band is that competition tempo, recorded in the dance's
  `tempo_note`, and is narrower than social practice (a social Quickstep at 196 BPM is outside the
  200–208 band and still a Quickstep). A consumer **MUST NOT** use a dance band to reject or remove a
  dance from a track; it may use it to rank or to warn. A second, social band is not defined: no source
  states one, and adding a field later is non-breaking.
- The bands are guidance owned by the SDTS baseline vocabulary; a deployment that wants a tighter or looser band
  applies a [vocabulary customization](../model/vocabulary/README.md), not a spec change. The only hard
  tempo bound is the global `bpm` envelope (`min`/`max`) in `validation/fields.json`.

## Perceptual scales — `rating`, `energy`, `valence`, `danceability`

These four perceptual fields carry a small ordinal scale, but they do **not** share one type: `rating` is the
standard star rating, while `energy`, `valence` and `danceability` are a plain 1-10 integer.

- **`rating`** is the **standard 5-star rating in half-star steps** — 11 levels (`0, 0.5, 1.0, … 5.0`), where
  `0` = unrated. It is carried in the standard `POPM` frame (a single popularity byte on disk under owner
  `no@email`), **not** a `TXXX:SDTS_*` field: rating is `POPM`'s defined purpose, so re-minting a custom rating
  tag would shadow it and break every other tagger that reads `POPM`. Because a rating's value is a star
  count carried in `POPM` (not an SDTS field shape), `rating` is deliberately **not** in
  `data/field-constraints.yaml` or the validation vectors — the POPM-byte ↔ half-star mapping is its contract.
- **`energy` / `valence` / `danceability`** are plain integers **1-10** in `TXXX:SDTS_*` (~5-6 = typical). Do
  **not** copy POPM's 0-255 byte — that is a POPM serialization quirk (lossy, app-specific thresholds,
  over-granular). The free TXXX field stores the plain number (e.g. `7`), and the conformance vectors validate
  it as an integer in range.
- **Orientation** differs by field and is part of each field's contract: `energy` and `danceability` run
  1 = lowest → 10 = highest, while `valence` runs 1 = darkest → 10 = brightest — an *unsigned* axis, not a
  quality score, so a low valence is a dark record and not a bad one.
- **`energy` and `valence` are the two halves of one grid** and share the substrate of the mood vocabulary's
  own `energy`/`valence` axes, so a track's tagged coordinate and its moods' coordinates compare without
  rescaling. They stay **separate fields** rather than one packed value: a DJ tags, searches and sorts on
  drive and brightness independently, and either half is meaningful with the other absent. Discretising the
  pair into a 3×3 or 5×5 grid is a **consumer** presentation choice — the standard stores the raw 1-10.
- **Presentation:** `energy` / `danceability` MAY render as half-stars for a uniform UI (1 = ½★, 2 = 1★, … 10 =
  5★); the *stored* value is the integer (canonical/presentation split). `rating` already is a star value.
  `valence` SHOULD NOT use stars — a star run reads as better-to-worse, which its orientation is not.
- Applies only to NUMERIC perceptual fields. Categorical ordinals stay enums (e.g. `tempo_stability`, and the
  inline `dances` purity / `routines` fitness grades).

## Alignment with public vocabularies

**→ [Vocabulary alignment](spec-alignment.md)** — how SDTS maps to Russell's affect model, the
Spotify/Essentia audio features, Discogs/MusicBrainz genre spellings, MusicBrainz release types, and the
recognised dance families (and where it deliberately diverges).

## Frame appendices

**→ [Frame appendices](spec-frames.md)** — Appendix A (standard music frames passed through unchanged) and
Appendix B (the reference-implementation–private `SB-*` frames a tagged file also carries).
