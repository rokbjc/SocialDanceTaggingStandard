# Comment &nbsp;·&nbsp; `COMM (no desc, any language)`

`ID3 standard frame` · Standard music frames · origin: **ID3 standard**

[← Field catalog](../README.md) · [Spec overview](../../spec.md) · [Registry (fields.yaml)](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Cross-tool interop](../../../sync/tool-comparison.md)

> Free-text notes only.

| | |
|---|---|
| **Frame** | `COMM (no desc, any language)` |
| **Concept** | Comment |
| **Origin** | ID3 standard (pass-through; SDTS does not redefine it) |

The canonical user comment: the COMM frame with an EMPTY description, in any language (the reference implementation reads any and writes 'XXX'). It is the user's free text with no label, prefix or schema, so it stays readable in every tool that shows a comment. A COMM frame WITH a description is never the canonical comment (MediaMonkey's Songs-DB_* columns are such frames). Historically it also carried (i)/(vm)-style abbreviation tags, now migrated to structured fields.

**When to use.** Free-text notes only. A tool that ADDS a note appends it after '; ' and never rewrites, reorders or de-duplicates the existing text (reference implementation: AppendCanonicalComment). A reader may split on '; ' to show separate notes, but treats the text as opaque prose when writing it back, since a note may itself contain '; '. Structured data (vocality, energy, dance tags, warning severity or position) goes to its own field or a tool-private extension frame, never into the comment.

**Examples (valid).** `Great clarinet solo at 1:12` · `From the 1938 Carnegie Hall concert`

**Invalid (don't do this).**
- structured data (vocality / energy / dance tags) — those have dedicated fields now

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
