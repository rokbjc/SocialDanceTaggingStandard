# BPM (raw) &nbsp;·&nbsp; `bpm`

> The primary set-building filter — find tracks inside the dance's tempo window.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `int (BPM, integer)` · single |
| **Values** | derived-from-audio (beat-grid / Essentia) then human-confirmable; numeric range |
| **Tag** | `TBPM` (ID3 standard frame — canonical home) |
| **Status** | active · Tempo & groove · ID3 standard |

The raw machine-measured beats-per-minute of the track, written as an integer to the standard TBPM frame. It is the literal pulse rate from the beat grid, NOT the tempo a dancer perceives — half/double-time tracks carry their raw value here (set it to the danced pulse if your detector mis-locks on a subdivision). Auto-derivable from the SB-Beats cache; once a user confirms it the BPM confirmation gate (Custom10) protects it.

**Values.** positive integer, ~40-330 (covers Slow Drag floor ~40 to St. Louis Shag / Bebop ceiling ~300-320 per ontology tempo_range bands); stored as a whole number

**Examples.** ✓ `120` · `186` · `250`

**Avoid.**
- `120.5` — fractional BPM is non-conformant in `TBPM`; store an integer
- `120; 180` — never multi-value in `TBPM` (it breaks every reader); use bpm + tempo_stability + the beat grid
- `fast` — `TBPM` is a number, not a word

**Related.** [Tempo Stability](../sdts/tempo_stability.md) · [Meter](../sdts/meter.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
