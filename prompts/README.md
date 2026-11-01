# prompts/

Portable, versioned LLM prompt templates that help a DJ author an SDTS Personal Profile from their
existing data. They are **authoring aids**, run by the DJ in any LLM — the SDTS library makes no
network/LLM calls and only **validates** the result (`schemas/profile.schema.json` + the import-time
invariants in `docs/standard/profile.md` §7). Keep the SDTS dimension list in each prompt in sync with
`data/vocabulary/` (it may be generator-injected).

- [`generate-path-profile.md`](generate-path-profile.md) — turn a filename/folder tagging convention into
  a `mapping.from_path` rule set.
