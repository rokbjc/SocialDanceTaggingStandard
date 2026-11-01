# Requirements — the SDTS standard

SRS for the **Social-Dance Tagging Standard** as a format-neutral standard: the field catalog
([`docs/standard/spec.md`](../docs/standard/spec.md) + [`data/fields.yaml`](../data/fields.yaml)), the
baseline vocabulary ([`data/vocabulary/`](../data/vocabulary/)), the cross-format bindings, and the
conformance vectors ([`conformance/`](../conformance/)). Requirement keywords are **MUST / SHOULD /
MAY** per RFC 2119. See [`README.md`](README.md) for the ID/priority/acceptance conventions.

This document specifies the *standard*; the .NET implementation's obligations are in
[`library.md`](library.md). Anything that is implementation behavior (API shape, performance, packaging)
belongs there, not here.

## 1. Scope

SDTS standardises **per-track tags for social/partner-dance DJ libraries** (swing and blues first,
extensible to the wider social-dance world): which fields exist, their controlled values, how they map
onto real tag containers, and what it means to read/write them conformantly. It is **format-neutral**
(ID3 first, Vorbis/MP4 via the same field names) and **language-neutral** (no implementation language is
assumed).

## 2. Conformance clause

*What may conform, and how* (W3C QA Framework). Three conformance targets are defined:

- a **Reader** — software that reads SDTS tags from a file into the model;
- a **Writer** — software that writes SDTS tags to a file;
- an **Implementation** — software that is both.

A product **conforms to SDTS `<version>`** when it satisfies every **MUST**/**MUST NOT** requirement
below that applies to its target and reproduces every applicable case in `conformance/vectors/`.
**SHOULD** requirements are conformance recommendations (deviation permitted only with a stated reason);
**MAY** requirements are optional. Conformance is defined **per spec version**. An implementation **MAY**
support only a subset of dance packs and still conform, provided it does not redefine any field or value.

Conformance (against this spec) is distinct from interoperability (working alongside other real
taggers); both are required — STD-N-02 covers the latter.

## 3. Functional requirements

| ID | Pri | Requirement | Acceptance criterion (verified by) |
|---|---|---|---|
| STD-F-01 | MUST | Every SDTS field is defined in the registry with a stable lower-snake `id`, a `data_type`, `cardinality`, a value source, and its bindings. | `validate_data.py` passes `data/fields.yaml` against `schemas/fields.schema.json`; `spec.md` has one section per field. *(CI data job)* |
| STD-F-02 | MUST | Each taxonomy dimension (genre, mood, rhythmic_feel, dances, …) defines a controlled value set of canonical names, each with its aliases and (where authored) a short code. | `data/vocabulary/` validates and loads; every alias/code resolves to exactly one canonical term. *(vocab loader + tests)* |
| STD-F-03 | MUST | A portable field uses the **same name across containers** — ID3 `TXXX:SDTS_X` = Vorbis `SDTS_X` = MP4 `----:com.apple.iTunes:SDTS_X` (the binding triangle). | `spec.md` §Conformance "Binding triangle"; `fields.yaml` carries `vorbis`/`mp4`/`id3_proposed` per field. *(conformance vector)* |
| STD-F-04 | MUST | Where a real standard frame exists for a concept (TCON, TMOO, TBPM, POPM, TLAN, TKEY, TSRC), that frame is canonical and the `SDTS_*` tag is treated as a mirror, not the home. | `spec.md` §Conformance "Binding triangle"; `fields.yaml` `id3_proposed` notes the standard frame stays canonical. |
| STD-F-05 | MUST | A multi-value field is encoded as a single `"; "` (semicolon-space) delimited string; a reader splits on `"; "` and SHOULD tolerate surrounding whitespace. | `spec.md` §Conformance "Multi-value encoding"; round-trip conformance vector. |
| STD-F-06 | MUST | Every time-valued field is an integer number of milliseconds. | `spec.md` §Conformance "Time unit"; conformance vector. |
| STD-F-07 | MUST | A writer that normalises a file sets `sdts_version`; a reader treats a file whose `sdts_version` is lower than its own as "needs migration". | `spec.md` §Conformance "Version stamp". |
| STD-F-08 | MUST NOT | An implementation does not rename the frozen on-disk frames (`SB-*` private caches, `COMM:Songs-DB_*` MediaMonkey columns). | `spec.md` §Conformance "Frozen frames"; library `.Id3`/`.MediaMonkey` keep the literal descriptions. |
| STD-F-09 | MUST | A conforming reader **skips/ignores** SDTS fields and refinements it does not recognise rather than failing (forward-compatible unknown-field tolerance; cf. ID3 unknown-frame skip, Dublin Core "dumb-down"). | Conformance vector containing an unknown `SDTS_*` field; reader returns the known fields without error. |
| STD-F-10 | MUST | The standard defines how a downstream consumer adds **private** fields without colliding — it owns the `SDTS_*` description namespace and the mapped `Songs-DB_*` frames; private tags use the consumer's own prefix via the portable `TXXX:NAME` / `----:com.apple.iTunes:NAME` convention. | `spec.md` extensibility clause; library composition test (`MirrorMap.Compose` rejects a colliding key). |
| STD-F-11 | SHOULD | The dance vocabulary is partitioned into **packs** by family (swing, blues; ballroom/tango/street-latin/west-coast-swing/carolina-shag/early authored, `niche` deferred); an implementation may ship a subset. | `data/vocabulary/manifest.yaml` lists packs; each pack self-contained. |
| STD-F-12 | MUST | The standard ships **machine-readable conformance vectors** that a conforming reader/writer reproduces. | `conformance/vectors/` exist and the reference implementation reproduces them. *(CI: vectors in-sync + tests)* |
| STD-F-13 | SHOULD | Per-term display metadata (code/emoji/icon/color) and authored cross-references (e.g. a genre's typical rhythmic-feels / song-forms) are SDTS-authored overlays, not catalog values, and flagged when expert-review-pending. | Overlays live on the term in `data/vocabulary/core/*.yaml`; the file's top-level `validation:` map flags a pending attribute, the generated docs show the flag on the dimension page and on each flagged entry, and `scripts/validate_data.py` rejects a flag that marks nothing. |
| STD-F-14 | MAY | An OPTIONAL Personal Profile MAY declare path-glob-scoped `from_path` rules that parse a track's filename/path into field values via a mask plus a token dictionary and typed extractors (rating, year). An unrecognised tail token MUST be reported, not assigned or dropped. | `schemas/profile.schema.json` (`mapping.from_path`) + the path-ingest conformance vector (`conformance/vectors/profile/path-ingest.json`). |

## 4. Non-functional requirements (ISO/IEC 25010)

| ID | Pri | Characteristic | Requirement | Acceptance criterion (verified by) |
|---|---|---|---|---|
| STD-N-01 | MUST | Functional suitability | The spec, the field registry, and the vocabulary are mutually consistent — every documented field/value traces to the data source, with no drift. | Docs are generated from `data/` (`gen_field_docs.py`, `gen_vocab_docs.py`) and the CI in-sync gate (`git diff --exit-code`) fails on drift. |
| STD-N-02 | MUST | Compatibility — interoperability | SDTS-tagged files remain readable by mainstream taggers: standard concepts use their real frames (STD-F-04) and an SDTS writer leaves frames it does not own untouched. | Interop check: write SDTS tags, re-open in a third-party tagger (e.g. MediaMonkey/TagLib), confirm standard frames + foreign frames intact. *(manual interop)* |
| STD-N-03 | MUST | Compatibility — co-existence | The `SDTS_*` description namespace and the frozen `Songs-DB_*`/`SB-*` frames are partitioned so SDTS and private/consumer tags never collide. | No `SDTS_*` id duplicates a standard frame or a reserved private prefix; `MirrorMap.Validate` rejects collisions. |
| STD-N-04 | MUST | Maintainability | The standard has a **single source of truth** — `data/fields.yaml` (schema) + `data/vocabulary/` (values) — from which all human docs and machine artifacts are generated; no hand-maintained mirror. | Every `docs/standard/fields/*` and `docs/model/vocabulary/*` page carries a "generated — do not edit" banner; CI regenerates and diffs. |
| STD-N-05 | MUST | Flexibility — portability | The spec is format- and language-neutral: it does not assume ID3, .NET, or any one tool; the binding triangle makes Vorbis/MP4 bindings additive. | `spec.md` defines fields independently of container; bindings are a per-field table, not prose assumptions. |
| STD-N-06 | MUST | Reliability | Canonical values round-trip losslessly (write→read returns the same normalized values), and the conformance vectors are deterministic (a fixed input yields a fixed expected output). | Round-trip + hash conformance vectors are byte-stable across runs. |
| STD-N-07 | SHOULD | Interaction capability (usability) | The spec is readable by both audiences — DJs/dancers (the value model) and implementers (the binding/conformance) — with normative and informative content clearly separated (RFC 2119 marked up). | `spec.md` separates the Conformance clause (normative, RFC-2119) from explanatory prose; the docs site builds clean. |
| STD-N-08 | MUST | Maintainability — versioning | The standard is versioned; **additive changes** (a new field or vocabulary term) are a minor (backward-compatible) change, and **conformance is defined per version**. Frozen frame identifiers are never renamed across versions. | `docs/standard/versioning.md` + `CHANGELOG.md`; `version`/`vocabulary_version` stamped in the data. |
| STD-N-09 | MUST | Security — legal/provenance | The spec, schemas, vocabulary, and conformance vectors are openly licensed (CC0) with documented provenance, asserting no third-party rights it cannot grant. | `LICENSE-SPEC` (CC0) + `NOTICE.md` provenance statement; `spec.md` provenance line. |

## 5. Out of scope (deliberately not required)

To keep the standard honest and the document un-padded (RFC 2119: keywords only where needed for
interop or harm), these common quality attributes are **explicitly excluded** — they do not apply to a
format-neutral data standard:

- **Runtime performance / latency / throughput SLAs** — a standard has no runtime; performance is the
  implementation's concern ([`library.md`](library.md)).
- **Authentication / authorization, multi-tenancy, high-availability/uptime, horizontal scaling** — there
  is no service, no users, no deployment.
- **Audio analysis algorithms, beat/key detection, playback, DJ cue points / beat grids** — out of
  portable scope (tool/DB territory; the private `SB-*` caches live in the consumer, not the standard).
- **DRM / encryption of tags** — SDTS describes open, human-curated metadata.
- **A wire/transport protocol** — SDTS is at-rest file tags, not a network format.

## 6. Traceability

| Requirement | Primary evidence |
|---|---|
| STD-F-01..02, STD-N-01/04 | `validate_data.py` + the CI generated-in-sync gate (docs, vectors, ids) |
| STD-F-03..09, STD-F-12 | `conformance/vectors/` reproduced by the reference implementation's test suite |
| STD-F-08/10, STD-N-03 | `MirrorMap.Validate` / `.MediaMonkey` frozen-frame tests; spec extensibility clause |
| STD-N-02 | Manual third-party-tagger interop check (release gate) |
| STD-N-08 | `docs/standard/versioning.md`, `CHANGELOG.md`, stamped data versions |
| STD-N-09 | `LICENSE-SPEC`, `NOTICE.md` |
