# Synchronization strategy — using SDTS tags in other software

**Part 2 of the docs.** Part 1 *defines* the standard (see [`../standard/spec.md`](../standard/spec.md), [`../../data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml), [`../standard/field-rationale.md`](../standard/field-rationale.md)). This part is the **strategy for making those tags usable in — and kept in sync with — other software** (MediaMonkey, JRiver, Picard, foobar2000, DJ tools, …). Landscape companion: [`tool-comparison.md`](tool-comparison.md) (tool interop + file-vs-database storage).

## Principles

1. **The file is the single source of truth.** SDTS is file-first: canonical values live in the file (standard frames + `TXXX:SDTS_*`). Never let an app's library DB be the truth — it does not travel with the track.
2. **The portable triangle.** One name, every container (`TXXX:SDTS_X` = Vorbis `SDTS_X` = MP4 `----:com.apple.iTunes:SDTS_X`) — defined in [`../standard/spec.md`](../standard/spec.md#binding-model-two-layers).
3. **Canonical + projections.** The canonical value is the standard frame (where one exists) or `TXXX:SDTS_*`. Everything else (the MM5 `Songs-DB_*` mirror, any DJ-tool projection) is a **derived copy**; the canonical wins on conflict.
4. **Normalize on read, ratchet toward canonical.** On load, migrate stale files and re-normalize external edits back to the canonical shape (the file is a one-way ratchet). The `sdts_version` watermark makes this a one-line decision.

## The layers SDTS writes to a file

| Layer | Frames | Read by | Role |
|---|---|---|---|
| **Standard frames** | `TCON TMOO TBPM POPM TLAN COMM TIT1` | everyone | universal, use as-is |
| **Portable SDTS** | `TXXX:SDTS_*` | JRiver, Picard, foobar2000, Mp3tag, Kid3, MusicBee | the canonical SDTS fields |
| **MM5 mirror** | `COMM:Songs-DB_*` (lang `XXX`) | MediaMonkey columns | optional projection for MM5 sort/filter |
| *(SB-\* private)* | `TXXX:SB-*`, `PRIV` | the reference implementation only | analysis cache — **not synced** |

## Per-tool sync recipe

| Tool | Reads `TXXX:SDTS_*`? | How to make SDTS visible | Notes |
|---|---|---|---|
| **JRiver** | yes | define a Library Field whose Name = the `SDTS_*` description | round-trips natively; no mirror needed |
| **Picard / foobar2000 / Mp3tag / Kid3 / MusicBee** | yes | nothing — they read `TXXX` by description | read **case-insensitively** (Picard mixed-case vs Mp3tag UPPERCASE footgun) |
| **MediaMonkey 5** | as an *Extended Tag* only (not a column) | for a sortable **column**, write the `Songs-DB_Custom*` mirror too | MM writes `COMM` lang `XXX`; force a re-read after write (`PublishTagsRefresh`) |
| **Apple Music / WMP / Plex / Emby** | no (fixed standard-frame readers) | rely on the standard-frame projection (genre / comment / grouping); `SDTS_*` rides in the file but is invisible there | accept invisibility |
| **DJ tools (Serato / Traktor / Rekordbox / Mixxx)** | no | reuse the shared standard frames (`TKEY`, `TBPM`, genre, comment) they already read | never treat their cue/grid DB as truth (it's proprietary + out-of-file) |

## The mirror / dual-write model

Write the **canonical** value once, then keep its projections in sync from a single chokepoint writer:

- **Canonical:** standard frame if one exists (`TCON`/`TMOO`/`TBPM`/`POPM`/`TLAN`), else `TXXX:SDTS_*`.
- **Projection → MM5 column:** mirror to a `Songs-DB_Custom*` slot (only for the fields you actually sort on in MM5).
- **Projection → universal / DJ readers:** the standard frames already cover genre/mood/bpm/rating/comment.
- **Conflict rule:** canonical wins; the mirror is rebuilt from it, never the reverse.

The reference implementation implements this with `MutateAndSave` (race-safe writes), the `Songs-DB_*` setters, `Custom10` gate invalidation, and `PublishTagsRefresh` (MM5 refresh).

## Read-side normalization (the one-way ratchet)

On load:

1. **Version check** — if the file's `sdts_version` < current, run that version's migrations (slash→`; ` delimiter fix, alias resolution, genre-hierarchy dedup, mirror rebuild) and bump `sdts_version`.
2. **External-edit drift** — when MM5 or another app rewrites a field in a diverging shape (e.g. genre with `/`, a stale mirror), re-normalize toward the canonical on the next read/write.
3. **Honour confirmation gates** — never overwrite a user-confirmed field (`Custom10` gate set) with a heuristic; only an explicit "re-analyze" overrides.

## Conflict & round-trip safety

- **Authority:** `SDTS_*` / the standard frame is canonical; the `Songs-DB_*` mirror is derived.
- **Casing:** read `TXXX` descriptions case-insensitively; write the one canonical casing (UPPERCASE `SDTS_*`).
- **Multi-value:** the reference implementation joins with `; `; ID3v2.4's native null-separated list is the cross-tool form — normalize on write.
- **COMM language:** `Songs-DB_*` frames use language `XXX` (MM5 expects it; mismatched language = duplicate frames).
- **Ratings:** `POPM` byte→stars mapping is app-specific → cross-app rating is lossy; accept the half-star approximation.

## What does NOT sync (accept as one-way / lossy)

- **DJ-tool cues / beatgrid / loops** — proprietary, DB-side; the reference implementation's `SB-Beats` will not appear in Serato/Rekordbox, and theirs won't appear in it. No portable bridge exists.
- **Library statistics** (play count, last-played, date-added) — app-DB-side everywhere; the consumer's session log keeps its own as-played/per-venue memory (not a portable tag).
- **Fixed-reader apps** (Apple Music, Plex, Emby, WMP) — will not surface `SDTS_*`; only the standard-frame projection shows.

## In one line

Write canonical to the **file** (standard frames + `TXXX:SDTS_*`); mirror to `Songs-DB_*` only for MM5 columns; lean on standard frames for universal/DJ readers; normalize on read via `sdts_version`; canonical always wins. **The file travels; the database does not.**
