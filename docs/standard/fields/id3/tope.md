# Original Artist &nbsp;·&nbsp; `TOPE`

`ID3 standard frame` · Standard music frames · origin: **ID3 standard**

[← Field catalog](../README.md) · [Spec overview](../../spec.md) · [Registry (fields.yaml)](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Cross-tool interop](../../../sync/tool-comparison.md)

> Fill on a Cover (a distinct interpretation) or a Tribute (an imitation aiming to sound like the original).

| | |
|---|---|
| **Frame** | `TOPE` |
| **Concept** | Original Artist |
| **Origin** | ID3 standard (pass-through; SDTS does not redefine it) |

The ORIGINAL performer(s) of the work, used when this recording is a cover or a tribute. Pairs with the Cover/Tribute object-state: TPE1 = this version's performer, TOPE = who originally did it.

**When to use.** Fill on a Cover (a distinct interpretation) or a Tribute (an imitation aiming to sound like the original). Leave EMPTY on an original-era recording — don't mirror TPE1. It is the original PERFORMER, not the composer (TCOM).

**Examples (valid).** `Louis Prima` · `Benny Goodman Orchestra`

**Invalid (don't do this).**
- mirroring TPE1 on an original-era recording — leave TOPE empty unless it's a cover/tribute
- the composer — TOPE is the original PERFORMER (composer is TCOM)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
