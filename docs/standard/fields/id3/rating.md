# Rating &nbsp;·&nbsp; `rating`

> Your fastest trust signal mid-set — 'which of my 150-BPM tracks is actually a killer'.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `rating` · single |
| **Values** | user judgement; half-star scale stored as a POPM popularity byte |
| **Tag** | `POPM with email/owner 'no@email'` (ID3 standard frame — canonical home) |
| **Status** | active · Workflow & trust · ID3 standard |

The DJ's quality/usability rating of the track on a 0-5 scale in half-star increments. Internally carried as half-stars; on disk it is a single POPM popularity byte under owner 'no@email' so MediaMonkey and JRiver read the same stars. It is a workflow judgement (is this track worth playing), distinct from danceability or energy.

**Values.** 0-5 stars in half-star steps internally (0, 0.5 … 5.0 → 11 levels); on disk a single POPM popularimeter byte 0-255 (the canonical MM-compatible mapping, byte families per half-star — see POPM byte 32 note)

**Examples.** ✓ `5 (=POPM 255)` · `3.5` · `0 (unrated)`

**Avoid.**
- `6` stars — the max is 5 (10 half-stars)
- `★★★` — store the `POPM` byte, not glyphs
- minting `TXXX:SDTS_RATING` — use the standard `POPM` frame; don't shadow it

**Related.** [Danceability](../sdts/danceability.md) · [Energy](../sdts/energy.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
