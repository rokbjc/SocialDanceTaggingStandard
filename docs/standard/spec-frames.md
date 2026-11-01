# Frame appendices

The frames a tagged file carries beyond the portable SDTS fields. ([Back to the spec](spec.md).)

## Appendix A — standard music frames

SDTS does not redefine universal music metadata. The standard music frames a conforming writer emits
(`TIT2`/`TPE1`/`TALB`/`COMM`/`TIT1`) and the **Original-\*** frames (`TOPE`/`TOAL`/`TDOR`, for covers &
reissues) are documented — with when/how to use plus valid/invalid examples — in the **`id3/`** folder of
the field catalog: [fields/README.md](fields/README.md). `TIT1` (Grouping) is the catalog `workflow_tags`
bag and stays a frozen pass-through (not a portable SDTS field — see [field rationale](field-rationale.md)).

Other standard library frames (`TLEN`, `TRCK`, `TPOS`, `TDRC`, `TPE2`, `APIC`) are read and passed through unchanged.

## Appendix B — reference-implementation–private frames (not part of SDTS)

These are present on tagged files but are **private analysis / cache data**, not portable SDTS fields. They
are the reference implementation's private cache frames, use the frozen `SB-` prefix, and are listed only so
implementers know what else is in the file. A few (dance steps, choreography, warnings) are candidates to
graduate into portable SDTS fields later.

| Frame | Purpose |
|---|---|
| `TXXX:SB-Beats` | Beat positions (ms array) — the beat grid. |
| `TXXX:SB-Beatinfos` | Per-beat annotations (BeatInfoDto; tuple-row JSON). |
| `TXXX:SB-BeatLocks` | Beat indices the user manually verified (anchors for shift / redistribute). |
| `TXXX:SB-Downbeats` | Downbeat positions (ms array). |
| `TXXX:SB-SongForm` | Structural form (AABA / 12-bar Blues / bar count) — this is the storage for the portable song_form field. |
| `TXXX:SB-Verified` | Per-category verification booleans (JSON object). |
| `TXXX:SB-Stems` | Demucs per-section / per-bar RMS profile + fingerprint (tuple-row JSON v1). |
| `TXXX:SB-WordByWordLyrics` | Whisper word-by-word timings + language + segment boundaries (JSON v1). |
| `TXXX:SB-Quality` | Audio-quality analysis JSON (score / cutoff / DR14 / source). Distinct from the recording_quality_era enum. |
| `TXXX:SB-Migrations` | Applied one-shot migration tokens ('; '-joined) so a track is not re-migrated each load. |
| `TXXX:SB-DanceSteps` | Choreography step list (tuple-row JSON v1). Candidate to graduate into a portable SDTS field. |
| `TXXX:SB-Choreography` | Track-level choreography metadata: name / era / formation (JSON). Candidate to graduate. |
| `TXXX:SB-EssentiaResult` | Cached Essentia high-level analysis (genres / moods / danceability / tempo / instruments) + schema version. |
| `TXXX:SB-Chroma` | Cached per-beat 12-bin chroma vectors, pinned to the beat-grid fingerprint. |
| `TXXX:SB-Warnings` | User-flagged unusual moments (odd ending / break / genre shift; tuple-row JSON). Candidate to graduate. |
| `TXXX:SB-Chromaprint` | fpcalc acoustic fingerprint (base64url) for duplicate detection / AcoustID. |
| `TXXX:SB-ChromaprintDuration` | fpcalc-measured duration (s, float), paired with the fingerprint for AcoustID. |
| `TXXX:SB-Instruments` | Hand-set instrument overrides (JSON object). |
| `PRIV:Moodbar Cache` | Binary moodbar (PNG / WAV) cache. |
| `TXXX:SB-BaseStructureSize, TXXX:SB-BarSize` | Legacy — superseded by SB-SongForm; kept read-only for migration. |
