# SDTS requirements

Software-requirements specification (SRS) for the **Social-Dance Tagging Standard (SDTS)**, split the
way the project is built — the open **standard** and its **.NET reference library** have different
obligations, so they get different documents:

- [`standard.md`](standard.md) — the format-neutral standard (spec + field registry + vocabulary +
  conformance vectors + cross-format bindings). What it means to *be* SDTS and to *conform* to it.
- [`library.md`](library.md) — the `SocialDanceTaggingStandard.*` .NET packages. What the reference
  implementation must *do*, and the definition-of-done for a v1.0 / public release.

These are the contract behind the project's acceptance tests. They are deliberately scoped: per
RFC 2119, requirement keywords are used *sparingly* — only where genuinely needed for interoperation
or to prevent harm — so the documents are not padded with quality attributes that do not apply to a
standard + library (see each document's **Out of scope** section).

## How to read a requirement

Every requirement has a stable **ID**, a **priority**, a single-sentence **statement** (active voice,
naming the responsible product), and a **verifiable acceptance criterion** with the artifact that
proves it. A requirement you cannot write a test assertion for is a design smell, not a requirement
(W3C QA Framework) — so each row here maps to something runnable or checkable.

| Field | Meaning |
|---|---|
| **ID** | `STD-F-NN` standard functional · `STD-N-NN` standard non-functional · `LIB-F-NN` library functional · `LIB-N-NN` library non-functional. Stable; never renumbered. |
| **Priority** | **MUST** / **MUST NOT** (absolute, gates conformance/release) · **SHOULD** (default, overridable with a stated reason) · **MAY** (optional) — per [RFC 2119](https://datatracker.ietf.org/doc/html/rfc2119). |
| **Acceptance** | A testable assertion + *verified by* the concrete artifact: a conformance vector, an automated test, a CI gate, or a named manual check. |

**Acceptance evidence** the criteria trace to:

- **Conformance** — `conformance/vectors/` (the language-neutral contract) reproduced by the reference implementation.
- **Consumer integration** — the downstream consumer builds against the packages and its tag read/write/sync path passes (`Audio.Tests`).
- **Publication readiness (v1.0)** — the CI gates: `scripts/validate_data.py`, the generated-artifacts in-sync gate (`git diff --exit-code`), a 0-warning `dotnet build`, `dotnet format --verify-no-changes`, the full test suite incl. the codegen drift-gate, and `mkdocs build --strict`.

## NFR taxonomy

Non-functional requirements are organised by the **ISO/IEC 25010:2023** product-quality
characteristics (the canonical model for specifying, measuring, and deriving acceptance criteria):
functional suitability, performance efficiency, compatibility, interaction capability (usability),
reliability, security, maintainability, flexibility (portability), safety. Characteristics that do
not apply to a standard + offline library are listed under **Out of scope** rather than silently
omitted.

## References (research basis)

- [RFC 2119](https://datatracker.ietf.org/doc/html/rfc2119) — requirement-level keywords; use sparingly.
- [W3C QA Framework: Specification Guidelines](https://www.w3.org/TR/qaframe-spec/) and [Test Methodology](https://www.w3.org/TR/test-methodology/) — conformance clauses, test assertions, extensibility, the five parts of a testable requirement.
- [ISO/IEC 25010:2023](https://www.iso.org/standard/78176.html) — product quality model / NFR taxonomy.
- [Semantic Versioning 2.0.0](https://semver.org/) — declare a public API; MAJOR/MINOR/PATCH compatibility contract.
- [ID3v2.4 structure](https://mutagen-specs.readthedocs.io/en/latest/id3/id3v2.4.0-structure.html) — version gating, unknown-frame skip tolerance, reserved identifier namespace.
- Conformance vs interoperability testing — [UNH-IOL](https://www.iol.unh.edu/blog/2021/02/02/interoperability-and-conformance-testing-what-s).
