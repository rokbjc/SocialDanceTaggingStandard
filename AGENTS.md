# SocialDanceTaggingStandard

The Social-Dance Tagging Standard (SDTS): a tag standard for social/partner-dance DJ music libraries
(swing and blues first) and its .NET reference implementation. Spec and data are CC0 (`LICENSE-SPEC`),
code is Apache-2.0 (`LICENSE`).

## Commands

```bash
cd dotnet && dotnet build SocialDanceTaggingStandard.slnx -c Release   # release gate: 0 warnings
cd dotnet && dotnet test SocialDanceTaggingStandard.slnx
dotnet format dotnet/SocialDanceTaggingStandard.slnx --verify-no-changes
python scripts/validate_data.py                  # data/ against schemas/ + vocabulary cross-checks
python scripts/gen_field_docs.py                 # data/fields.yaml -> docs/standard/fields/
python scripts/gen_vocab_docs.py                 # data/vocabulary/ -> docs/model/vocabulary/ + BaselineVocabulary.g.cs
python scripts/gen_conformance_vectors.py        # data/field-constraints.yaml -> conformance/vectors/validation/
python scripts/gen_ids.py                        # data/fields.yaml -> dotnet/src/*/Generated/*.g.cs
python scripts/gen_overlay_schema.py             # schemas/sdvo.schema.json.in -> schemas/sdvo.schema.json + SdvoFormat.g.cs
python -m mkdocs build --strict                  # needs requirements-docs.txt
dotnet run --project dotnet/samples/SocialDanceTaggingStandard.Samples
```

## Generated output

After an edit under `data/` or to `schemas/sdvo.schema.json.in`, run all five generators and commit their
output. `.github/workflows/ci.yml` regenerates and fails on `git diff`. Never hand-edit generated output:
`docs/standard/fields/`, `docs/model/vocabulary/`, `conformance/vectors/validation/`,
`schemas/sdvo.schema.json`, `dotnet/src/*/Generated/`.

A value with a YAML source is generated, never hand-written: extend `scripts/gen_ids.py` or
`scripts/gen_*_docs.py`. Hand-write only what has no YAML source (example:
`MediaMonkeyFrames.MusicMatchSituation`).

## Map

| Path | Content |
|---|---|
| `data/fields.yaml` | field registry: schema, bindings, field `status`, spec `version` |
| `data/vocabulary/` | hand-maintained values: `core/` = music dimensions, `packs/<name>/` = dances of one family, `manifest.yaml` = index. Rules: `data/vocabulary/README.md` |
| `docs/standard/spec.md` | the spec; normative rules are in §Conformance |
| `requirements/` | `standard.md` (STD-*), `library.md` (LIB-* and the v1.0 checklist) |
| `conformance/vectors/` | language-neutral vectors that the .NET tests reproduce |
| `schemas/` | JSON Schemas for the data, overlay and profile files |
| `scripts/` | Python generators and `validate_data.py` |
| `dotnet/src/SocialDanceTaggingStandard` | `SocialDanceTaggingStandard`: format-neutral model, `Sync/` mirror engine, `Validation/`, `Gates/`. No container dependency, no YAML dependency |
| `dotnet/src/SocialDanceTaggingStandard.Taxonomy` | `SocialDanceTaggingStandard.Taxonomy`: compiled-in baseline vocabulary, alias and value resolution, overlays and profiles, path ingest. The only package that references YamlDotNet |
| `dotnet/src/SocialDanceTaggingStandard.Id3` | `SocialDanceTaggingStandard.Id3`: ID3v2.4 binding over TagLibSharp. `SdtsFile` is the consumer gateway and exposes no TagLib type |
| `dotnet/src/SocialDanceTaggingStandard.MediaMonkey` | `SocialDanceTaggingStandard.MediaMonkey`: mirror of SDTS fields to the `COMM:Songs-DB_*` columns |
| `dotnet/tests` | xUnit test projects and `SocialDanceTaggingStandard.TestKit` |
| `dotnet/samples` | `SocialDanceTaggingStandard.Samples` (usage scenarios), `SocialDanceTaggingStandard.ConsumerProbe` (compiles only if a consumer needs no TagLib type) |

A new dance pack is a directory `data/vocabulary/packs/<name>/` listed in `manifest.yaml`.

## Frozen identities

- Package ids and namespaces: `SocialDanceTaggingStandard`, `SocialDanceTaggingStandard.Taxonomy`, `SocialDanceTaggingStandard.Id3`,
  `SocialDanceTaggingStandard.MediaMonkey`. Folder and `.csproj` names match the package ids.
- The standard owns the name space `TXXX:SDTS_*`. One name serves three containers: ID3 `TXXX:SDTS_X`,
  Vorbis `SDTS_X`, MP4 `----:com.apple.iTunes:SDTS_X`.
- A concept with a real standard frame (TCON, TMOO, TBPM, POPM) is stored in that frame; `data/fields.yaml`
  defines no `SDTS_*` copy for it.
- Legacy on-disk frames are never renamed: `COMM:Songs-DB_*` and `TXXX:Quality`
  (`dotnet/src/SocialDanceTaggingStandard.MediaMonkey/Generated/MmColumns.g.cs`), `MusicMatch_Situation` and the sync-state key
  `SDTS_MM5_SYNC` (`dotnet/src/SocialDanceTaggingStandard.MediaMonkey/MediaMonkeyFrames.cs`), `SB-*` (private analysis caches of
  consumers, listed in `data/fields.yaml` under `reference_impl_private`; this repository does not write them).
- The multi-value delimiter is `"; "`. A time value is an integer number of milliseconds.
- A writer must not emit a field whose `status` is `proposed` (`docs/standard/spec.md` §Conformance levels).
- Two independent version streams (`docs/standard/versioning.md`): the spec version is `version` in
  `data/fields.yaml` (also the `sdts_version` stamp and every vector's `suite_version`); the package
  version is `<Version>` in `dotnet/Directory.Build.props`.
- Requirement ids (`STD-F-NN`, `STD-N-NN`, `LIB-F-NN`, `LIB-N-NN`) are never renumbered.
- A requirement added to `requirements/*.md` gets a ledger row in `docs/status/standard.yaml` or
  `docs/status/library.yaml` in the same change.

## Terms

| Term | Meaning |
|---|---|
| binding | mapping of the format-neutral taxonomy to one tag container (ID3 today) |
| canonical, mirror | in `Sync/`: the authoritative storage location of a field, and a second location kept in sync with it (for example a MediaMonkey column) |
| pack | the dance values of one dance family; an implementation may enable a subset |
| baseline | the vocabulary compiled into `SocialDanceTaggingStandard.Taxonomy` |
| overlay | one user's changes to the baseline, stored in a `.sdvo.yaml` file |
| profile | an overlay plus read-time rules (`docs/standard/profile.md`) |

## Code rules

- Each kind of id has its own type (`FieldId`, `TagKey`, `Id3FrameId`, `Id3UserTextDescription`,
  `Id3CommentDescription`, `DimensionId`, `PackId`); a `FieldId` is not assignable where an `Id3FrameId` is
  expected. A bare literal such as `"COMM"` or `"Songs-DB_Custom7"` at a call site is a defect.
- `dotnet/src/SocialDanceTaggingStandard.Id3` contains no MediaMonkey identifier in executable code; those live in
  `dotnet/src/SocialDanceTaggingStandard.MediaMonkey` (LIB-F-10).
- A breaking API change in the four `dotnet/src` projects breaks downstream consumers that build them from
  source. Concurrent builds of one checkout share `bin/` and `obj/` unless they pass
  `-p:SdtsArtifactsPath=<dir>` (`dotnet/Directory.Build.props`).
- The root `.editorconfig` is the shared editor configuration; `dotnet/.editorconfig` adds analyzer
  severities for `dotnet/`.
- Code, comments and commit messages are in English. Comments are the exception: write one only where the
  code cannot say it.
- Defensive input handling and explicit error handling are required; a build produces 0 warnings.

## Deviations from common defaults

- The packages target `net8.0`: they are public libraries and `net8.0` reaches the most consumers.
  `LangVersion` stays `14.0`.
- The tests use plain xUnit `Assert` and no assertion library, to keep the repository free of an extra test
  dependency.
