# Vocality

The **vocality** vocabulary — each value's canonical name (what is stored), the aliases that resolve to it, and the axes that make it queryable.

→ Field reference: [Vocality](../../standard/fields/sdts/vocality.md) · [all dimensions](README.md) · [dances](dances.md)

Codes read as `V` (vocal) + an optional **type** (`X` featured · `S` scat · `W` spoken · `G` group · `D` duet; omit for a plain lead) + an optional **gender suffix** (`M` male · `F` female). So `VM` = male lead, `VX` = featured (any gender), `VXM` = featured male, `VSF` = scat female; `V` alone is a generic vocal and `INS` is instrumental. Gendered variants are well-formed even where no separate value exists yet.

| Value | Code | Aliases | Description |
|---|---|---|---|
| **Instrumental** | `INS` | `No Vocal` | No vocals. Pure instrumental — no one sings through the track. |
| **Male** | `VM` | `Vocal Male` · `Vocal Male Lead` · `Vocal Male (lead)` · `Male Vocal` · `Male Lead` · `Vocal M` | Male vocal-led — a male singer is the main voice through most of the track. |
| **Female** | `VF` | `Vocal Female` · `Vocal Female Lead` · `Vocal Female (lead)` · `Female Vocal` · `Female Lead` · `Vocal F` | Female vocal-led — a female singer is the lead. |
| **Group** | `VG` | `Vocal Group` · `Vocal Ensemble` · `Vocal Ensemble (Male)` · `Vocal Ensemble (Female)` · `Vocal Trio` · `Vocal Trio (Female)` · `Vocal Trio (Male)` · `Vocal Quartet` | Several singers at once (doo-wop, harmony trio/quartet, gospel choir). |
| **Duet** | `VD` | `Vocal Duet` | Two singers — M+F, M+M, or F+F — both equal vocal leads. |
| **Feature** | `VX` | `Vocal Feature` · `Vocal Male (featured)` · `Vocal Female (featured)` · `Vocal (featured)` · `Vocal Featured` · `Featured Vocal` · `Vocal Cameo` | A largely instrumental track with a vocal part (verse/chorus) but mostly instrumental. |
| **Scat** | `VS` | `Scat Led` · `Scat Vocal` · `Vocal Scat` · `Vocal Male (featured as Scat)` · `Vocal Female (featured as Scat)` | The singer uses scat instead of lyrics through the main part of the track. |
| **Spoken** | `VW` | `Spoken Word` · `Talking` · `Vocal Spoken` | The performer speaks the lyrics rather than singing them. |
| **Vocal** | `V` |  | Generic vocal — use when the specific form (Male/Female/Group/Feature/Scat/Spoken) is unknown or unspecified, or when vocality is mixed/uncertain. |

_Generated from [`data/vocabulary/`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/data/vocabulary) by `scripts/gen_vocab_docs.py` — do not hand-edit; edit `data/vocabulary/`, then re-run this._
