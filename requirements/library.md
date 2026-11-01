# Requirements — the .NET reference library

SRS for the **`SocialDanceTaggingStandard.*`** packages — the reference implementation of the SDTS
standard. Requirement keywords are **MUST / SHOULD / MAY** per RFC 2119. See [`README.md`](README.md)
for conventions and [`standard.md`](standard.md) for the standard these implement.

## 1. Scope

Four packages, layered so value data and container deps do not bleed across the standard surface:

- **`SocialDanceTaggingStandard`** (core) — the format-neutral model + the `Sync/` mirror engine. No taxonomy values, no container deps.
- **`SocialDanceTaggingStandard.Taxonomy`** — the baseline vocabulary (compiled C#) + loader + alias/value resolution. The only package with a YAML dependency (now only for runtime overlays + the drift-gate).
- **`SocialDanceTaggingStandard.Id3`** — the portable ID3v2.4 binding (TagLibSharp): standard frames + the `TXXX:SDTS_*` surface.
- **`SocialDanceTaggingStandard.MediaMonkey`** — the MM5 bridge: import/export/3-way sync against the legacy `COMM:Songs-DB_*` columns.

The primary consumer is **the reference-implementation DJ app**; "fit for the consumer" is a first-class acceptance target.

## 2. Functional requirements

| ID | Pri | Requirement | Acceptance criterion (verified by) |
|---|---|---|---|
| LIB-F-01 | MUST | The library loads the baseline vocabulary from compiled-in generated C# via `SocialDanceVocabulary.LoadDefault()` with **no network and no external-data access** at runtime. | A test loads the vocabulary offline and finds the expected dimensions/packs. *(Taxonomy tests)* |
| LIB-F-02 | MUST | A dimension resolves an alias or display code to its canonical term, validates membership, and normalises a multi-value list (alias→canonical, dedup, parent/child hierarchy collapse). | `Dimension.Resolve/Contains/Normalize` tests, incl. dance grade/`?` marker stripping. |
| LIB-F-03 | MUST | Every kind of identifier (field id, frame id, frame description, language, vocabulary code per dimension) is a distinct strong type, not a bare string; the literal values are defined once and generated from the YAML. | Build compiles with the typed ids; `git grep` finds no magic-string id literals at call sites in `.Id3`/`.MediaMonkey`. *(code-review gate)* |
| LIB-F-04 | MUST | The `.Id3` binding reads and writes the portable surface — genres (TCON), moods (TMOO), the canonical user comment, and `TXXX:SDTS_*` descriptions — via typed accessors. | `Id3TaxonomyExtensions` read/write round-trip tests. |
| LIB-F-05 | MUST | The `.MediaMonkey` bridge imports/exports/3-way-syncs the SDTS fields against the legacy `COMM:Songs-DB_*` columns, preserving values MediaMonkey wrote. | `.MediaMonkey` accessor + mirror tests; the consumer's `Audio.Tests` round-trip. |
| LIB-F-06 | MUST | The Sync engine performs a value-agnostic 3-way reconcile with per-field baseline change-detection and configurable conflict resolution (`ReportOnly` / `PreferCanonical` / `PreferMirror`) and direction/write-through policy. | `TagMirror` / `MirrorSyncEngine` tests covering each conflict mode + baseline change-detection. |
| LIB-F-07 | MUST | A downstream consumer can add its **own private fields** (extra `FieldMirror` rows via `MirrorMap.Compose`) and supply a custom `IValueTranslator`, and the engine never disturbs frames it was not mapped to. | `MirrorMap.Compose` rejects colliding id/key; an unmapped-frame-untouched test; the consumer's `SB-*` private tags survive a sync. |
| LIB-F-08 | MUST | The typed id/value members and the per-dimension term catalogs are **generated from the YAML** sources (`data/fields.yaml`, `data/vocabulary/`); no hand-maintained mirror of that data exists. | `scripts/gen_ids.py` + `gen_vocab_docs.py` regenerate byte-identically; the CI in-sync gate diffs `dotnet/src/*/Generated`. |
| LIB-F-09 | MUST | The reference library reproduces the standard's conformance vectors. | The test suite consumes `conformance/vectors/` and asserts the expected output. |
| LIB-F-10 | MUST NOT | The portable `.Id3` package contains no MediaMonkey/MM5-specific identifiers in executable code (those live in `.MediaMonkey`). | `git grep` of `.Id3` finds `Songs-DB_`/`MusicMatch_`/MM5 frames only in doc-comments. *(code-review gate)* |
| LIB-F-11 | SHOULD | The library projects SDTS field values from a track's filename/path per a profile's `from_path` rules as a **read-only, pure** function (no I/O beyond the input string), reusing the `IngestMapper` map stage, and returns the unmapped-token report. | `PathParser`/`PathIngest` unit tests + the path-ingest conformance vector. |
| LIB-F-12 | MUST | The `.Id3` binding exposes a **whole-record gateway** (`SdtsFile`) that reads/writes a complete SDTS record — SDTS `TXXX` fields + standard frames + audio properties + arbitrary TXXX — in one open→save cycle via typed per-field accessors, **without a consumer naming any `TagLib` type**. | `SdtsFileTests` round-trip; a consumer referencing only `SocialDanceTaggingStandard.Id3` (`SocialDanceTaggingStandard.ConsumerProbe`) compiles naming no `TagLib` type. |

## 3. Non-functional requirements (ISO/IEC 25010)

| ID | Pri | Characteristic | Requirement | Acceptance criterion (verified by) |
|---|---|---|---|---|
| LIB-N-01 | MUST | Functional suitability | The library implements the standard exactly; the generated compile-time catalogs agree with the runtime-loaded vocabulary value-for-value. | The drift-gate test (`GeneratedVocabularyCatalogTests`) + conformance vectors pass. |
| LIB-N-02 | MUST | Compatibility | The packages target `net8.0` (the previous LTS, so apps on .NET 8/9/10 can consume them), depend only on TagLibSharp (`.Id3`) and YamlDotNet (`.Taxonomy`), and the standard's portable surface (core) pulls in neither a container nor YAML dependency. | `dotnet build` succeeds; core `.csproj` has no TagLibSharp/YamlDotNet reference. |
| LIB-N-03 | MUST | Compatibility — consumer | The consumer builds against the packages and its tag read/write/sync path passes end-to-end. | The consumer solution builds; its `Audio.Tests` green. *(release gate)* |
| LIB-N-04 | SHOULD | Performance efficiency | `LoadDefault()` returns in well under a second from compiled-in C# (no I/O or YAML parsing beyond the assembly), and per-file tag operations are linear in the file's frame count. | A timing test asserts `LoadDefault()` under a generous bound; no network/file access during load. |
| LIB-N-05 | MUST | Reliability | Tag read→write→read round-trips losslessly and a sync never silently drops a mapped value; behavior is deterministic. | Round-trip + sync no-data-loss tests; deterministic codegen (re-run produces no diff). |
| LIB-N-06 | MUST | Maintainability | The build is **0 warnings**, `dotnet format` clean, fully tested, and CI-gated (validate-data, generated-in-sync, build, format, test). | The CI dotnet + data jobs pass; `dotnet build` reports 0 warnings. |
| LIB-N-07 | SHOULD | Flexibility — portability | The code is OS-neutral (no platform-specific APIs beyond the cross-platform TagLibSharp); generated files and sources use LF; the `.editorconfig` mirrors the ecosystem root. | CI runs on Linux + Windows; `.gitattributes` enforces LF. |
| LIB-N-08 | MUST | Security | No secrets are committed; data is parsed, never executed; dependency licenses are compatible and documented (TagLibSharp LGPL-2.1 via dynamic NuGet linkage, YamlDotNet MIT). | `NOTICE.md` records deps + licenses; no `eval`/dynamic code from vocabulary data; secret-scan clean. |
| LIB-N-09 | MUST | Maintainability — versioning | The library declares a public API and follows SemVer: an additive API change is MINOR, a breaking change is MAJOR; breaking changes are not made within a major version. | `PackageId`/version set; a public-API diff review classifies each release; (recommended) a public-API analyzer/baseline. |
| LIB-N-10 | SHOULD | Maintainability — packaging | Each package ships as a NuGet package with description, license, and the standard metadata, restorable without private feeds. | `dotnet pack` produces packages with `Description`/license; restore works from public sources. |
| LIB-N-11 | MUST | Security — authoring | Profile authoring (incl. LLM-assisted generation) is external to the library; the library MUST NOT make network/LLM calls — it only validates a profile against the schema + invariants and reports unmapped tokens. | No network/HTTP/LLM client in any package; dependency audit + `dotnet build`. |

## 4. Out of scope (deliberately not required)

These do not apply to an offline reference library and are excluded so the document is not padded:

- **Audio analysis / beat-grid / key detection / playback** — the private `SB-*` analysis caches stay in
  the consumer; the library moves tags, it does not analyse audio.
- **UI / rendering** — the docs site is separate; the library is headless.
- **Server, multi-tenancy, authentication/authorization, high-availability/uptime SLAs, horizontal
  scaling, concurrency at scale** — there is no service; the library is a single-process dependency.
- **Telemetry / analytics** — none collected.

## 5. Acceptance / Definition of Done (v1.0 / public release)

The release gate is the conjunction of these (all MUST hold; each is one of the requirements above):

- [ ] `dotnet build dotnet/SocialDanceTaggingStandard.slnx -c Release` — **0 warnings, 0 errors** (LIB-N-06)
- [ ] `dotnet test` — all green, including the codegen **drift-gate** (LIB-N-01, LIB-F-09)
- [ ] `dotnet format --verify-no-changes` — clean (LIB-N-06)
- [ ] `validate_data.py` + the generated-artifacts in-sync gate (`git diff --exit-code`) — clean (LIB-F-08)
- [ ] `mkdocs build --strict` — clean (docs/usability)
- [ ] The **consumer solution** builds + its `Audio.Tests` green — consumer proven (LIB-N-03)
- [ ] `.Id3` is **MM5-free** in executable code (LIB-F-10)
- [ ] A consumer referencing only `SocialDanceTaggingStandard.Id3` reads/writes a whole record and names no `TagLib` type — proven by `SocialDanceTaggingStandard.ConsumerProbe` (LIB-F-12)
- [ ] `NOTICE.md` / `LICENSE` / `LICENSE-SPEC` present and accurate (LIB-N-08, STD-N-09)
- [ ] Public API reviewed; version set per SemVer (LIB-N-09)

## 6. Traceability

| Requirement | Primary evidence |
|---|---|
| LIB-F-01..09, LIB-N-01/05 | `dotnet/tests/SocialDanceTaggingStandard.Tests` (vocab, sync, drift-gate, conformance) |
| LIB-F-04..07, LIB-N-03 | The consumer's `Audio.Tests` (round-trip / sync) |
| LIB-F-03/08/10, LIB-N-06 | CI dotnet + data jobs; `git grep` / code-review gate |
| LIB-N-02/07/10 | `*.csproj` (TFMs, deps, package metadata); CI matrix (Linux+Windows) |
| LIB-N-08, LIB-N-09 | `NOTICE.md`; public-API review at release |
