# Artist Country &nbsp;·&nbsp; `artist_country`

> Pull a country / region set — 'only French', 'US-only' — for geographic theming.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `enum (ISO 3166-1 alpha-2)` · single |
| **Values** | ISO 3166-1 country (external standard, NOT SDTS vocab) — country of the artist's/music's ORIGIN; canonical name + ISO alpha-2/alpha-3 aliases (genre model) |
| **Tag** | `TXXX:SDTS_ARTIST_COUNTRY` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | proposed · Recording character · SDTS-defined |

The country the artist or music ORIGINATES from (France for Django Reinhardt, the United States for Count Basie), so a DJ can build country- or region-themed sets like a French Manouche night or a US-only block. This is about origin, not where a release was issued (release-country) or where the track was recorded (recording-location).

**Values.** Country of the artist's/music's ORIGIN, as a canonical country NAME (genre model; e.g. 'United States', 'France', 'United Kingdom'). Presentation/alias forms: the full name, the ISO 3166-1 alpha-2 ('US') and alpha-3 ('USA') codes, and a flag/locale label — all per-term metadata, rendered by the player; the codes also resolve on input. Value set = ISO 3166-1 (external standard), so NO vocabulary content is needed. Distinct from release-country (where a release was published) and recording-location. Single dominant origin (multi only for genuine multinational collaborations).

**Examples.** ✓ `France` · `United States` · `United Kingdom`

**Avoid.**
- `French` — use the canonical country name `France` (or its ISO alias `FR`)
- `Paris` — that's a recording-location / city, not the artist's country of origin
- the release-country (where it was published) — this field is artist/music ORIGIN, a different concept

**Related.** [Recording Origin](recording_origin.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
