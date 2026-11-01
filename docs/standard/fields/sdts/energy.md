# Energy &nbsp;·&nbsp; `energy`

> Build the night's energy arc and contrast — pull an opener, a builder, a peak, a cool-down.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `rating` · single |
| **Values** | numeric range (1-10); human-set, optionally seeded from audio energy estimate |
| **Tag** | `TXXX:SDTS_ENERGY` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Mood & energy · SDTS-defined |

A standalone 1-10 floor-drive rating, independent of both BPM and the mood's valence. Captures how hard a track pushes the room: a slow grinding blues can be energy 8 at 70 BPM; a bright mid-tempo novelty can be energy 3. Decouples the 'how much does this lift the floor' lever from the energy×valence mood grid, which is too coarse (only high/medium/low and always tied to brightness).

**Values.** discrete integer 1-10 (1 = lowest floor-drive, 10 = peak; ~5-6 typical). Part of the 1-10 rating-scale family. Stored as a plain integer in TXXX:SDTS_ENERGY — NOT a 0-255 byte.

**Examples.** ✓ `3` · `5` · `8`

**Avoid.**
- `0` or `11` — out of range; energy is 1–10
- `200` — don't copy POPM's 0–255 byte; store the plain integer
- `high` — it's a number; the qualitative axis lives in mood

**Related.** [Valence](valence.md) · [Danceability](danceability.md) · [Mood](../id3/mood.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
