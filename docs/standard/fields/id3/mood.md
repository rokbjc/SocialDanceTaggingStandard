# Mood &nbsp;·&nbsp; `mood`

> Steer the room's affect — pull a lift, a cool-down, or a peak by feel, and avoid jarring valence jumps.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md) · [Values](../../../model/vocabulary/mood.md)

| | |
|---|---|
| **Type** | `enum[]` · multi · delimiter `;` |
| **Values** | SDTS baseline vocabulary -> Moods (canonical names on the Energy x Valence grid) |
| **Tag** | `TMOO` (ID3 standard frame — canonical home) |
| **Status** | active · Mood & energy · ID3 standard |

The track's emotional feel, placed on the vocabulary's Energy × Valence mood model. Unlike the standalone energy field (raw floor-drive) or theme (listening/event situation), mood is the valence-aware affect descriptor — what the record sounds like emotionally, not just how hard it pushes the room.

**Values.** Closed enum, multi-value; values joined with '; '. Stored as canonical text; each value carries an Energy axis and a Valence axis, each a 1-10 ordinal on the vocabulary's energy×valence mood model (the consumer discretises to a 3×3 / 5×5 grid). Multi-value captures tracks that shift or sit between cells. Value set: SDTS baseline vocabulary -> Moods (not enumerated here).

**Examples.** ✓ `Joyful` · `Bluesy; Soulful` · `Melancholic` · `Hot; Wild` · `Tender`

**Avoid.**
- `Happy` — not a catalog mood; the nearest is `Joyful`
- `Joyful/Hot` — use `; ` → `Joyful; Hot`
- writing it to `TXXX:MOOD` instead of `TMOO` — that shadows the real standard frame

**Related.** [Energy](../sdts/energy.md) · [Valence](../sdts/valence.md) · [Genre](genre.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
