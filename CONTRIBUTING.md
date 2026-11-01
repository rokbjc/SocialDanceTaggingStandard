# Contributing to SocialDanceTaggingStandard

Thanks for helping build an open, portable tag standard for social-dance DJ libraries.

This repo holds three kinds of thing, with **different contribution rules**:

| You want to… | Track | See |
|---|---|---|
| Fix a typo, a field doc, the spec, the sync notes, code | **A — minimal fix** | [below](#track-a--minimal-fix) |
| Add or change a **dance / genre / mood / value** | **B — taxonomy** | [below](#track-b--taxonomy) |
| Add or change **library code** (.NET, future Python/TS) | **A** + tests | [below](#code-changes) |

> **The golden rule: never hand-edit generated files.** Several files are *generated* from a
> source-of-truth and will be overwritten. Edit the **source**, then regenerate.

## Source vs. derived

| Edit this (source) | NOT this (generated from it) | Regenerate with |
|---|---|---|
| `data/fields.yaml`, `data/field-invalid-examples.yaml` | `docs/standard/fields/**` | `python scripts/gen_field_docs.py` |
| `data/vocabulary/**` (see Track B) | `docs/model/vocabulary/**`, the compiled C# baseline | `python scripts/gen_vocab_docs.py` |
| `docs/standard/spec.md`, `docs/sync/**`, `README.md` | — (edit directly) | — |

Every generated file carries an `AUTO-GENERATED — DO NOT EDIT` header. If you see one, find its source above.

## Track A — minimal fix

1. Edit the **source** file (see the table).
2. If you touched `data/fields.yaml` or `data/field-invalid-examples.yaml`, run
   `python scripts/gen_field_docs.py` and commit the regenerated `docs/standard/fields/**` too.
3. Run the local checks (below) and open a PR.

## Track B — taxonomy

Taxonomy **values** (which dances, genres, moods, … exist) are **hand-maintained** in
[`data/vocabulary/`](data/vocabulary/) — the normative value sets. Add or change a value by editing those
files directly:

1. Edit the relevant file: `data/vocabulary/core/<dimension>.yaml` for a shared music dimension
   (genre, mood, feel, …), or `data/vocabulary/packs/<pack>/dances.yaml` for a dance. Match the shape of
   the surrounding terms — `id`, `canonical`, `aliases`, the dimension-specific attributes — and include a
   **`source:` citation**. See [`data/vocabulary/README.md`](data/vocabulary/README.md) for the model.
2. Regenerate the derived artifacts and validate, and commit the regenerated `docs/model/vocabulary/**`
   and `dotnet/src/SocialDanceTaggingStandard.Taxonomy/Generated/**`:
   `python scripts/gen_vocab_docs.py && python scripts/validate_data.py`.
3. Open a PR (or open a **new-taxonomy-term** issue first to discuss). If you can't run the generators,
   fill in the issue and a maintainer will add the term.

> **Before removing or renaming anything:** what counts as a breaking vs. additive change, and the
> deprecate-don't-delete rule, are defined in [Versioning & releases](docs/standard/versioning.md).

## Code changes

- .NET lives under `dotnet/`. Add or update tests in `dotnet/tests/SocialDanceTaggingStandard.Tests`.
- Cross-language behaviour (validation rules, the sync engine, `StableHash`) is pinned by the
  language-neutral vectors under `conformance/` — if you change behaviour, update the vectors so every
  language stays in lock-step.

## Local checks (what CI runs)

```bash
# 1. data is valid + docs are in sync with the registry
python -m pip install pyyaml jsonschema
python scripts/validate_data.py
python scripts/gen_field_docs.py && git diff --exit-code docs/standard/fields data

# 2. .NET builds + tests pass (uses the compiled-in baseline vocabulary)
cd dotnet && dotnet build SocialDanceTaggingStandard.slnx && dotnet test
```

## Branches and pull requests

- **`dev`** is the default branch and the integration branch: open pull requests against it.
- **`main`** holds released versions only and is updated by the maintainer when a release is cut (see
  [Versioning & releases](docs/standard/versioning.md)). Do not open pull requests against it.
- Name your branch `feature/<short-description>`, `fix/…`, `docs/…`, `refactor/…` or `chore/…`: lowercase,
  kebab-case.
- Pre-releases are tags such as `v0.1.0-beta.1` on `dev`, not a separate branch.

## Licensing of contributions

This repo is **dual-licensed** (see [`README.md`](README.md#license)):

- Everything under `docs/`, `data/`, `conformance/`, and `schemas/` (the **specification + value registry +
  schemas + test vectors**) is dedicated to the public domain under **CC0-1.0** ([`LICENSE-SPEC`](LICENSE-SPEC)).
- Code under `dotnet/`, `scripts/` (and future `js/`) is **Apache-2.0** ([`LICENSE`](LICENSE)).

By contributing you agree your contribution is provided under the license of the path you change, and
you certify the [Developer Certificate of Origin](https://developercertificate.org/) — commit with
`git commit -s` (a `Signed-off-by:` line).
