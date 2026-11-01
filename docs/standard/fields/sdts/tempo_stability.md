# Tempo Stability &nbsp;·&nbsp; `tempo_stability`

> Avoid rubato / tempo-change tracks when you need a steady, beatmatchable floor.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `enum` · single |
| **Values** | derived-from-audio (variance of SB-Beats inter-beat intervals) then human-confirmed |
| **Tag** | `TXXX:SDTS_TEMPO_STABILITY` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Tempo & groove · SDTS-defined |

A one-word summary of how steady the pulse is across the track, derived from the variance of the SB-Beats inter-beat intervals. 'stable' tracks are safe for back-to-back mixing; 'rubato'/'accelerando'/'tempo-change' warn the DJ of a moving target that may strand dancers or break a transition.

**Values.** stable | drift | rubato | accelerando | tempo-change (closed enum). stable = tight grid; drift = slow human wander; rubato = expressive free time; accelerando = sustained speed-up; tempo-change = one or more abrupt section tempo shifts.

**Examples.** ✓ `stable` · `rubato` · `accelerando`

**Avoid.**
- `fast` — that's a speed (a 'Tempo' false friend), not stability; use `stable`/`drift`/…
- `stable; drift` — single-valued
- `steady` — use the canonical `stable`

**Related.** [BPM (raw)](../id3/bpm.md) · [Song Form](song_form.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
