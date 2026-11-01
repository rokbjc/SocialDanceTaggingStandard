# Versioning, deprecation & releases

SDTS keeps **two independent version streams** — don't conflate them.

## Spec version vs. implementation version

- **Spec version** — the version of the standard itself (the field schema + vocabulary + binding rules).
  The authoritative source is the `version` key in
  [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml). It is the value a
  tagged file records in its `sdts_version` field, and every conformance vector's `suite_version` MUST equal it
  (enforced by `scripts/validate_data.py`). Current spec version: **`0.1.0-draft`**.
- **Implementation / package versions** — each reference implementation versions independently: the .NET packages
  (`SocialDanceTaggingStandard*`, sharing one `<Version>`). One library MAY
  ship many releases against a single spec version (e.g. a .NET `0.4.x` implementing spec `0.1.0`). The
  `sdts_version` on a file always names the **spec** version it was normalized under — never a package version.

Both streams follow [SemVer](https://semver.org/). **Pre-1.0:** while a stream is `0.x`, a minor release MAY
contain breaking changes (SemVer §4) — pin an exact version if you need stability.

## What counts as a breaking change

**Schema** ([`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml)):

| Bump | Change |
|------|--------|
| patch | clarify a description, fix a typo, add an example |
| minor (additive) | add a field, add an optional value, widen a value set, add an alias |
| major (breaking) | remove/rename a field, narrow a value set, change a field's type/delimiter/binding, remove a canonical term |

**Vocabulary** ([`data/vocabulary/`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/data/vocabulary)):

| Bump | Change |
|------|--------|
| minor (additive) | add a term or an alias |
| major (breaking) | remove/rename a canonical term, or change what an existing term means |

## Deprecation — mark, don't delete

Removing a field or term breaks files already tagged with it, so SDTS **deprecates rather than deletes**:

- A field carries `status: deprecated` (+ optional `replaced_by: <field-id>`) in `fields.yaml`; a vocabulary term
  carries `deprecated: true` (surfaced as `VocabularyTerm.Deprecated` in the .NET loader).
- A deprecated field/term keeps resolving for **at least one major cycle** before it may be removed, so existing
  files migrate without data loss.
- Every deprecation is announced in the CHANGELOG under a `Deprecated` heading.

## Releases

- The [CHANGELOG](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/CHANGELOG.md) follows
  [Keep a Changelog](https://keepachangelog.com/): unreleased work accrues under `[Unreleased]`; each release is a
  dated, versioned section.
- A release is cut by moving `[Unreleased]` into a dated `[x.y.z]` section and tagging the commit with an
  **annotated git tag** (`vX.Y.Z` for a package release). The spec version in `data/fields.yaml` is bumped only
  when the standard itself changes.
- No release is tagged yet — the project is pre-publication at spec `0.1.0-draft`.

## Conformance

An implementation **conforms to SDTS** at a given spec version when it passes that version's conformance vectors
([`conformance/`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/conformance)) and honors the normative rules
in the standard. See the **Conformance** requirements in [the SDTS spec](spec.md).
