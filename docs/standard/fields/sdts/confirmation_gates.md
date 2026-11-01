# Confirmation gates (Custom10) &nbsp;·&nbsp; `confirmation_gates`

> Lock human-verified fields so auto-suggestions / re-analysis don't overwrite your confirmed calls.

[← Field catalog](../README.md) · [Spec](../../spec.md) · [Registry](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) · [Interop](../../../sync/tool-comparison.md)

| | |
|---|---|
| **Type** | `object[] (Category:version)` · multi · delimiter `; (semicolon-space)` |
| **Values** | user confirmation actions + 3-state content-CRC tamper detection; open field-id-keyed gate vocabulary with per-gate integer version |
| **Tag** | `TXXX:SDTS_CONFIRMATION_GATES` — same `SDTS_` name across ID3 TXXX · Vorbis · MP4 |
| **Status** | active · Workflow & trust · SDTS-defined |

Per-category ledger of which logical fields the user has hand-verified, forming the trust boundary that protects curated values from automated overwrites. The reference implementation reads it before touching a gated field: heuristic auto-corrections (BPM half-time fix, alias normalization) must refuse the write unless the caller explicitly forces a re-analyze, and a merge/bulk edit that genuinely changes a field clears that field's gate (audio-derived gates are preserved since their inputs aren't merged).

**Values.** semicolon-separated '<field-id>:<version>[:<crc>]' entries — the wire token is the confirmed field's id (e.g. bpm:1, genre:2). Version is a positive integer ≥1 (increments on re-confirmation, bpm:1 → bpm:2). The OPTIONAL 3rd part is a 4-char hex content checksum (compact CRC) for tamper detection. The gate vocabulary is OPEN — any field is gate-able (a core field or a consumer's extension field); a token that is neither a canonical field id nor a profile alias is preserved verbatim and flagged for remediation. 3-state per gate: absent=ungated, version≥1=verified, tamper-flagged when the stored 4-char checksum mismatches the recomputed content checksum.

**Examples.** ✓ `BPM:1:a3f2; Genres:1; Moods:2` · `Beats:1; Structure:1; Segments:1` · `Occasions:1; Comments:1; Lyrics:1`

**Avoid.**
- `BPM` — a gate needs a version: `BPM:1`
- `BPM:1, Moods:2` — wrong delimiter; `; ` → `BPM:1; Moods:2`
- `Tempo:1` — not a canonical gate name

**Notes.** Legacy on-disk mirror `COMM:Songs-DB_Custom10` — kept for backward compatibility (see [Part 2 sync](../../../sync/tool-comparison.md)); NOT the canonical home.

**Related.** [SDTS schema version](sdts_version.md)

---
_Generated from [`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) by `scripts/gen_field_docs.py` — do not hand-edit; edit the registry and re-run._
