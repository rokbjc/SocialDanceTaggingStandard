# Governance

How decisions get made in SocialDanceTaggingStandard. This is deliberately lightweight: the project is small
and currently has **one maintainer**. This document describes how it works today and how that scales — so the
process is legible even while it is small.

## Roles

- **Maintainer** — currently [@rokbjc](https://github.com/rokbjc). Reviews and merges changes, cuts releases,
  and has the final say on the normative standard. Additional maintainers may be added (see
  [Becoming a maintainer](#becoming-a-maintainer)).
- **Contributors** — anyone who opens an issue or PR. See [`CONTRIBUTING.md`](CONTRIBUTING.md).

## How decisions are made

Day-to-day changes run on **lazy consensus**: a proposal (issue or PR) that draws no unresolved objection
within a reasonable window is accepted. Where there is disagreement, discussion happens in the open on the
issue/PR; if consensus is not reached, the maintainer decides and records the reason. Bigger or breaking
changes (see below) SHOULD start as an issue before a PR, so the design is discussed before code is written.

## Two things this project governs

The [taxonomy packaging model](data/vocabulary/README.md) splits authority, and governance follows that split:

1. **The schema and binding rules** — `data/fields.yaml`, `docs/standard/`, `schemas/`, `conformance/`. This is
   the standard proper; the maintainer owns it.
2. **The vocabulary values** — which dances, genres, moods, … exist. These are **hand-maintained** in
   `data/vocabulary/` (the normative value sets); add or change a value by editing those files directly via
   pull request — see **Track B** in [`CONTRIBUTING.md`](CONTRIBUTING.md) and
   [`data/vocabulary/README.md`](data/vocabulary/README.md).

## Field lifecycle (`proposed` → `active` → `deprecated`)

Every field in `data/fields.yaml` carries a `status`. Promotion is a governance decision, not an accident of
implementation:

- **`proposed`** — designed and documented, but **not yet written to disk**. A conforming writer MUST NOT emit
  a `proposed` field; a reader SHOULD tolerate one it does not recognise. Proposed fields may change or be
  dropped without a major bump.
- **`active`** — promoted once (a) its shape is stable, (b) it has conformance vectors under
  `conformance/vectors/`, and (c) there is a real consumer writing it. Active fields are covered by the
  compatibility rules in [versioning](docs/standard/versioning.md).
- **`deprecated`** — kept resolving for at least one major cycle before removal (mark, don't delete). See
  [versioning](docs/standard/versioning.md).

## Breaking changes, versioning, and releases

What counts as additive vs breaking, the deprecate-don't-delete rule, and how releases are cut and tagged are
all defined in **[`docs/standard/versioning.md`](docs/standard/versioning.md)** — the single source of truth
for version policy. The spec version (in `data/fields.yaml`) and the reference-implementation package versions
move independently.

## Changing this document

Governance changes go through the same PR + lazy-consensus process, with the maintainer as the final reviewer.

## Becoming a maintainer

Sustained, high-quality contribution (reviews, well-scoped PRs, help triaging issues) is the path. The
maintainer may invite an active contributor to co-maintain; when that happens, this document and
[`.github/CODEOWNERS`](.github/CODEOWNERS) are updated to name them and to distribute review by area.
