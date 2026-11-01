# Danceability &nbsp;·&nbsp; `danceability`

> Separate genuine floor tracks from listening-only music.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `rating` · single |
| **Values** | numeric range 1–10; human-set, optionally seeded from beat-clarity in SB-Beats analysis then human-confirmed |
| **Tag** | `TXXX:SDTS_DANCEABILITY` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Mood & energy · SDTS-defined |

A 1–10 beat-clarity / floor-readiness score answering "is this danceable, or is it a listening track?" — the single dimension DJ guides rank above genre and BPM. It decouples "good music" from "good FLOOR music": a beloved torch ballad can be a 2, an average jump tune a 5. Auto-derivable as a first pass from beat-grid pulse strength in the SB-Beats cache, then human-confirmed (gate-able).

**Values.** discrete integer 1-10 (1 = barely danceable / listening-only, 10 = irresistible floor track). Part of the 1-10 rating-scale family. Plain integer in TXXX:SDTS_DANCEABILITY.

**Examples.** ✓ `10` · `5` · `2`

**Avoid.**
- `0` or `11` — out of range; 1–10
- `200` — a plain integer, not a POPM byte
- `danceable` — it's a number, not a word

**Related.** [Energy](energy.md) · [Rating](../id3/rating.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
