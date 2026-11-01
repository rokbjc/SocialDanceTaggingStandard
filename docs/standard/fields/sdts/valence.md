# Valence &nbsp;·&nbsp; `valence`

> Pick by brightness independently of drive — a dark peak, a bright cool-down — and avoid jarring valence jumps between records.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `rating` · single |
| **Values** | numeric range (1-10); human-set, optionally seeded from the track's moods or an audio valence estimate |
| **Tag** | `TXXX:SDTS_VALENCE` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Mood & energy · SDTS-defined |

A standalone 1-10 brightness rating — the valence half of the energy × valence pair, carried as its own field exactly as energy is. Captures how dark or bright the record reads: a driving minor-key stomp can be energy 8 / valence 3, a gentle sunny ballad energy 3 / valence 8. Independent of BPM and of which moods are tagged.

**Values.** discrete integer 1-10 (1 = darkest, 10 = brightest; ~5-6 typical). Part of the 1-10 rating-scale family, and the same substrate as the mood dimension's valence axis. Stored as a plain integer in TXXX:SDTS_VALENCE — NOT a 0-255 byte.

**Examples.** ✓ `3` · `6` · `9`

**Avoid.**
- `0` or `11` — out of range; valence is 1–10
- `bright` — it's a number; the named affect lives in mood
- a low number to mean 'a weak track' — valence is dark↔bright, not bad↔good; quality is `rating`
- `8` copied from the mood's own valence — the field is the TRACK's coordinate, not a mood echo

**Related.** [Energy](energy.md) · [Mood](../id3/mood.md) · [Rhythmic Feel](rhythmic_feel.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
