# Lyric Language &nbsp;·&nbsp; `lyric_language`

> Filter by sung language for language-specific rooms or sing-along sets.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `enum[]` · multi · delimiter `;` |
| **Values** | controlled enum — ISO 639-1 two-letter language codes (SDTS controlled vocabulary; new field, proposed). |
| **Tag** | `n/a` (ID3 standard frame — canonical home) |
| **Status** | proposed · Recording character · ID3 standard |

The language(s) of the sung or spoken words, the lyric language a DJ screens for crowd comprehension and sing-along sets. This is the content language of the words themselves, not the language tagged on the recording's metadata. Where Vocality captures the presence and type of voice, this captures what tongue that voice is in.

**Values.** ISO 639-1 codes, e.g. en, fr, de, it, es, sl; plus the sentinel "instrumental" for no-lyric tracks (cross-check against Vocality=Instrumental). Multi-value for bilingual/multilingual tracks. Unset = unknown.

**Examples.** ✓ `en` · `en; fr` · `sl` · `instrumental`

**Avoid.**
- `English` — use the ISO 639-1 code `en`
- `en, fr` — wrong delimiter; `en; fr`
- `en` on an instrumental — use the sentinel `instrumental` (and it must agree with Vocality=Instrumental)

**Related.** [Vocality](../sdts/vocality.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
