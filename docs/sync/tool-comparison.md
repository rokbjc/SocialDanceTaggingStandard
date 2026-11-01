# SDTS custom fields vs other tools (interoperability)

How SDTS stores its custom fields, how that compares to the wider tool ecosystem, and **where** each app keeps each field — in the file's tags (portable) or in the app's own database. The per-field *format* mapping (ID3 / Vorbis / MP4) is in [`../standard/spec.md`](../standard/spec.md) and [`../../data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml); each field there also has an **Other tools** note. This page has two parts: a **tool-axis** survey (what each tool calls a custom field) and a **storage-axis** matrix (file tag vs database).

## The core finding

Every serious tagger supports exactly one cross-tool extension point for arbitrary fields: the ID3 **`TXXX`** frame (and its Vorbis / MP4 equivalents). SDTS uses it — new fields are `TXXX:SDTS_<NAME>`.

- **MediaMonkey 5** is the only mainstream tool locked to a *private* scheme (`COMM:Songs-DB_Custom1..10`, lang `XXX`) for its sortable columns — so SDTS keeps a `Songs-DB_*` mirror only for fields you sort on in MM5.
- **JRiver**, **foobar2000**, **MusicBee**, **Picard**, **Mp3tag**, **Kid3**, **TagScanner**, **Yate** all read/write arbitrary `TXXX:NAME` and round-trip it.
- **Apple Music/iTunes, Windows Media Player, Winamp, Plex, Emby** do *not* expose arbitrary `TXXX` — they read a fixed standard-frame set. So SDTS fields survive in files but are invisible there.
- **DJ tools** (Serato, Traktor, Rekordbox, djay) keep working data in private `GEOB`/`PRIV`/DB; SDTS never touches those and reuses only the shared frames (`TKEY`, `TBPM`).

### Casing footgun

ID3 `TXXX` descriptions are **case-sensitive**, yet tools disagree on casing: **Picard** preserves mixed case (`TXXX:Artists`), while **Mp3tag / foobar2000 / MusicBee** write all-caps (`TXXX:ARTISTS`) — the same field then appears as two frames. SDTS fixes one canonical form, **UPPERCASE `SDTS_<UPPER_SNAKE>`**, and consumers should read `TXXX` descriptions case-insensitively.

## Part 1 — what each tool stores, and whether it reads `TXXX`

### Tag editors

| Tool | Stores a custom field as | Reads arbitrary `TXXX`? | Notable native extra fields |
|---|---|---|---|
| MusicBrainz Picard | Custom tags are saved to / reloaded from ID3 TXXX (User Defined Text Information) frames as TXXX:<name>. | yes | Mood -> writes ID3v2.4 TMOO (and TXXX:MOOD on v2.3); |
| Mp3tag | Any field not in its built-in mapping is written to ID3v2 as TXXX:<fieldname> (same for v2.3 and v2.4). | yes | MOOD -> TMOO (v2.4) / TXXX:MOOD (v2.3); |
| Kid3 | Add a frame -> choose TXXX (User-defined text) and set its description, e.g. | yes | Exposes raw ID3 frames directly, so standard frames are used as-is (POPM, TMOO, TIT1, etc.) rather than re-badged. |
| MusicBee | Define New Tags lets you create custom tags persisted to the file (or DB-only if you choose). | yes | Mood -> TMOO (v2.4) / TXXX:MOOD (v2.3); |
| beets | Arbitrary 'flexible attributes' can be set on any field name, but by DEFAULT they live only in the beets SQLite database and are NOT written to the fi… | partial | Built on mediafile/mutagen; |
| TagScanner | Supports user-defined fields; | yes | Standard frames used for known fields (rating/POPM, etc.); |
| Yate Music Tagger (macOS) | Offers 100 fully integrated Custom fields. | yes | Explicitly designed to map to 'any extensible frame used by other applications'. |
| dBpoweramp (ID Tag / CD Ripper) | Unknown/extra tags are written to ID3 as TXXX/[tagname] (its TXXX notation uses '/'). | yes | Rating -> POPM; |
| EasyTAG | No general GUI for arbitrary user-named TXXX fields. | partial | None proprietary — sticks to standard frames for its fixed field set. |
| Quod Libet / Ex Falso | Arbitrary tag keys are supported; | partial | Migrates foobar2000-style TXXX:replaygain_* to proper RVA2 on save; |

### Players, library managers & media servers

| Tool | Stores a custom field as | Reads arbitrary `TXXX`? | Notable native extra fields |
|---|---|---|---|
| MediaMonkey 5 | Does NOT use TXXX for its built-in classification/custom fields. | partial | Classification block: Mood (standard TMOO), Tempo (COMM:Songs-DB_Tempo), Occasion (COMM:Songs-DB_Occasion), Quality (COMM:Songs-DB… |
| JRiver Media Center | Uses real ID3v2 TXXX frames. | yes | Large predefined field set (Keywords, Mood, Genre, Album Artist (auto), Date, plus user-creatable fields of type List/Date/Path). |
| foobar2000 | Every non-native field name is stored in a TXXX frame with the field name as the TXXX description (exception: names starting with 'WWW' go to WXXX URL… | yes | No proprietary classification set; |
| MusicBee | User-defined Custom tags (up to 16) are written to ID3v2 TXXX with the tag identifier as the description (e.g. | yes | Tags(2) tab exposes Quality, Tempo, Mood, Occasion (each with editable value lists) plus up to 16 Custom1-16 fields. |
| Apple Music / iTunes | For MP3/ID3 it does not expose a generic custom-field-to-TXXX writer in the UI; | no | Invents many non-standard frames: TCMP (compilation flag, 1/0), GRP1 (Grouping, since iTunes 12.5; |
| Windows Media Player | Maps its WM/* attribute model to ID3v2 frames when writing MP3 (e.g. | no | WM/Mood -> TMOO (stored in the file, per Microsoft docs). |
| Winamp | Built-in ID3 editor handles the standard frame set; | no | Standard fields plus library/media-library ratings (Winamp ML rating, largely DB-side). |
| AIMP | AIMP Advanced Tag Editor works with ID3v1/v2, APE and Vorbis. | unknown | Standard tag fields; |
| Roon | READ-ONLY with respect to files: Roon imports tags from files but, by design, never writes its own metadata (ratings, tags, edits) back to ID3/files —… | partial | Roon has its own 'Tags' and 'Focus' features and rich editorial metadata, but these are DB-only. |
| Plex | Reads embedded ID3 tags during scan (depends on the music agent / 'Personal Media' settings). | no | Recognizes core embedded fields: Track/Album/Artist/Album Artist, Genre (TCON), Year, Mood (TMOO) and Style in some agent configs,… |
| Jellyfin | Core Jellyfin reads only a fixed set of standard frames via its TagLib-based extractor (Artist, Album, Album Artist, Genre, Year, Title, Composer, BPM… | partial | Core: standard frames only; |
| Emby | Like Jellyfin (shared MediaBrowser/TagLib lineage), Emby reads a fixed set of standard music ID3 frames during scan and stores library metadata in its… | no | Standard music fields (Artist/Album/Album Artist/Genre/Year/Composer/etc.); |
| Navidrome | Reads custom ID3 tags only when you declare them. | partial | Built-in multi-frame mappings, e.g. |

### DJ software

| Tool | Stores a custom field as | Reads arbitrary `TXXX`? | Notable native extra fields |
|---|---|---|---|
| Serato DJ (Pro/Lite) | Proprietary GEOB (General Encapsulated Object) frames on ID3v2.4. | no | Hotcues + loops + colors live inside 'Serato Markers2' (per-cue RGB triplet). |
| Traktor (Pro 2/3/4) | Single proprietary binary blob in an ID3v2 PRIV frame with owner identifier 'TRAKTOR4' (literal; | no | Energy/rating/comment2 (COM2) all live inside the TRAKTOR4 PRIV blob. |
| Rekordbox (6/7) | Almost nothing is written to the file. | no | My Tag (multi-category user labels), Color, Rating, cues/loops/beatgrid — all database-only, none in ID3. |
| VirtualDJ | Hybrid: on FIRST analysis writes BPM, key, color, cue points (HotCues) and comment both to the file (its own ID3 frames) and to database.xml; | partial | Color, POIs/cue points, User1/User2 (='Field 1'/'Field 2'), Gain, Play Count, First Seen, Last Play — all primarily database.xml; |
| Mixxx | By default writes NOTHING to file tags — beatgrid, hotcues, loops, waveform, play count, crates/playlists all in its internal SQLite database. | partial | Hotcues, color, rating, beatgrid — database-only. |
| Mixed In Key | Writes to STANDARD/widely-read frames so every DJ app can read it: Key to TKEY (Camelot notation e.g. | partial | Energy (1-10) via TXXX:EnergyLevel / ENERGYLEVEL (de-facto cross-tool standard). |
| Lexicon DJ | Database-first (its own library DB) but with a field-mapping engine that can READ/WRITE a CURATED set of ID3 frames — NOT arbitrary TXXX. | no | Custom Tags (organizational), Energy, Danceability, Popularity — database-only unless mapped. |
| beaTunes | Writes ID3v2 standard frames plus its own TXXX custom frames. | yes | Color = TXXX:beaTunes_COLOR (RGB hex). |
| djay (Pro) — Algoriddim | Database/container-first: beatgrids, cue points, loops, waveforms, analysis stored in djay's local app container (or iCloud when syncing). | no | Cues/loops/beatgrid/energy/analysis — all container/iCloud-only, none in ID3. |

## Part 2 — where each tool keeps it: file tag vs database

The storage location is the whole point: a field stored only in a DB is invisible the moment the file moves machines or apps. The tables above say *what* each tool calls a field; these say *where* it lives.

**Legend:** 📄 file tag (portable) · 🗄️ internal DB / library only · 🔀 both (mirrored) · ➖ not supported · ❔ unknown.

### Tag editors

| Field | **Reference impl (SDTS)** | MusicBrainz Picard | Mp3tag | Kid3 | MusicBee | beets |
|---|---|---|---|---|---|---|
| **Title / Artist / Album** | 📄 | 📄 | 📄 | 📄 | 🔀 | 🔀 |
| **Genre** | 📄 | 📄 | 📄 | 📄 | 🔀 | 🔀 |
| **Mood** | 📄 | 📄 | 📄 | 📄 | 🔀 | 🗄️ |
| **BPM** | 📄 | 📄 | 📄 | 📄 | 🔀 | 🔀 |
| **Musical key** | ➖ | 📄 | 📄 | 📄 | 🔀 | 🔀 |
| **Rating** | 📄 | 📄 | 📄 | 📄 | 🔀 | 🗄️ |
| **Play count** | ➖ | ➖ | ➖ | ➖ | 🗄️ | 🗄️ |
| **Last played / date added** | ➖ | ➖ | ➖ | ➖ | 🗄️ | 🗄️ |
| **Comment** | 📄 | 📄 | 📄 | 📄 | 🔀 | 🔀 |
| **Grouping** | 📄 | 📄 | 📄 | 📄 | 🔀 | 🔀 |
| **Energy** | 📄 | ➖ | 📄 | 📄 | 🔀 | 🗄️ |
| **Dance / occasion** | 📄 | ➖ | 📄 | 📄 | 🔀 | 🗄️ |
| **Color / label** | ➖ | ➖ | ➖ | ➖ | 🗄️ | 🗄️ |
| **Cue points / hotcues** | ➖ | ➖ | ➖ | ➖ | ➖ | ➖ |
| **Beatgrid** | 📄 | ➖ | ➖ | ➖ | ➖ | ➖ |
| **Loops** | ➖ | ➖ | ➖ | ➖ | ➖ | ➖ |
| **My-tags / custom tags** | 📄 | 📄 | 📄 | 📄 | 🔀 | 🗄️ |
| **Playback start/stop** | 📄 | ➖ | ➖ | ➖ | 🗄️ | ➖ |
| **Waveform / analysis** | 📄 | ➖ | ➖ | ➖ | 🗄️ | 🗄️ |
| **ReplayGain / gain** | ➖ | 📄 | 📄 | 📄 | 🔀 | 🔀 |

### Players, library managers & media servers

| Field | **Reference impl (SDTS)** | MediaMonkey 5 | JRiver Media Center | foobar2000 | Apple Music / iTunes | Roon | Plex |
|---|---|---|---|---|---|---|---|
| **Title / Artist / Album** | 📄 | 🔀 | 🔀 | 📄 | 🔀 | 🗄️ | 🗄️ |
| **Genre** | 📄 | 🔀 | 🔀 | 📄 | 🔀 | 🗄️ | 🗄️ |
| **Mood** | 📄 | 🔀 | 🔀 | 📄 | 🗄️ | 🗄️ | ➖ |
| **BPM** | 📄 | 🔀 | 🔀 | 📄 | 🔀 | ➖ | ➖ |
| **Musical key** | ➖ | 🔀 | 🔀 | 📄 | ➖ | ➖ | ➖ |
| **Rating** | 📄 | 🔀 | 🔀 | 🗄️ | 🗄️ | 🗄️ | 🗄️ |
| **Play count** | ➖ | 🗄️ | 🗄️ | 🗄️ | 🗄️ | 🗄️ | 🗄️ |
| **Last played / date added** | ➖ | 🗄️ | 🗄️ | 🗄️ | 🗄️ | 🗄️ | 🗄️ |
| **Comment** | 📄 | 🔀 | 🔀 | 📄 | 🔀 | 🗄️ | ➖ |
| **Grouping** | 📄 | 🔀 | 🔀 | 📄 | 🔀 | 🗄️ | ➖ |
| **Energy** | 📄 | ➖ | ➖ | ➖ | ➖ | ➖ | ➖ |
| **Dance / occasion** | 📄 | 🔀 | ➖ | ➖ | ➖ | ➖ | ➖ |
| **Color / label** | ➖ | ➖ | ➖ | ➖ | ➖ | 🗄️ | 🗄️ |
| **Cue points / hotcues** | ➖ | ➖ | 🗄️ | ➖ | ➖ | ➖ | ➖ |
| **Beatgrid** | 📄 | ➖ | ➖ | ➖ | ➖ | ➖ | ➖ |
| **Loops** | ➖ | ➖ | 🗄️ | ➖ | ➖ | ➖ | ➖ |
| **My-tags / custom tags** | 📄 | 🗄️ | 🔀 | 📄 | 🗄️ | 🗄️ | 🗄️ |
| **Playback start/stop** | 📄 | ➖ | 🗄️ | ➖ | 🗄️ | ➖ | 🗄️ |
| **Waveform / analysis** | 📄 | 🗄️ | 🗄️ | 🗄️ | 🗄️ | 🗄️ | 🗄️ |
| **ReplayGain / gain** | ➖ | 🔀 | 🔀 | 📄 | 🗄️ | 🗄️ | 🗄️ |

### DJ software

| Field | **Reference impl (SDTS)** | Serato DJ | Traktor | Rekordbox | VirtualDJ | Mixxx | Mixed In Key |
|---|---|---|---|---|---|---|---|
| **Title / Artist / Album** | 📄 | 📄 | 🔀 | 🔀 | 🔀 | 🗄️ | 📄 |
| **Genre** | 📄 | 📄 | 🔀 | 🔀 | 🔀 | 🗄️ | 📄 |
| **Mood** | 📄 | ➖ | ➖ | ➖ | ➖ | ➖ | ➖ |
| **BPM** | 📄 | 🔀 | 🔀 | 🗄️ | 🔀 | 🗄️ | 📄 |
| **Musical key** | ➖ | 📄 | 🔀 | 🔀 | 🔀 | 🗄️ | 📄 |
| **Rating** | 📄 | 🗄️ | 🔀 | 🗄️ | 🗄️ | 🗄️ | ➖ |
| **Play count** | ➖ | 🗄️ | 🗄️ | 🗄️ | 🗄️ | 🗄️ | ➖ |
| **Last played / date added** | ➖ | 🗄️ | 🗄️ | 🗄️ | 🗄️ | 🗄️ | ➖ |
| **Comment** | 📄 | 📄 | 🔀 | 🔀 | 🔀 | 🗄️ | 📄 |
| **Grouping** | 📄 | 📄 | 🔀 | 🔀 | 🔀 | 🗄️ | 📄 |
| **Energy** | 📄 | ➖ | 🗄️ | ➖ | ➖ | ➖ | 📄 |
| **Dance / occasion** | 📄 | ➖ | ➖ | ➖ | ➖ | ➖ | ➖ |
| **Color / label** | ➖ | 🔀 | 🗄️ | 🗄️ | 🗄️ | 🗄️ | ➖ |
| **Cue points / hotcues** | ➖ | 🔀 | 🔀 | 🗄️ | 🔀 | 🗄️ | 📄 |
| **Beatgrid** | 📄 | 🔀 | 🔀 | 🗄️ | 🔀 | 🗄️ | ➖ |
| **Loops** | ➖ | 🔀 | 🔀 | 🗄️ | 🔀 | 🗄️ | ➖ |
| **My-tags / custom tags** | 📄 | ➖ | ➖ | 🗄️ | 🗄️ | ➖ | ➖ |
| **Playback start/stop** | 📄 | 📄 | 🔀 | 🗄️ | 🗄️ | 🗄️ | ➖ |
| **Waveform / analysis** | 📄 | 🔀 | 🗄️ | 🗄️ | 🗄️ | 🗄️ | ➖ |
| **ReplayGain / gain** | ➖ | 🔀 | 🔀 | 🗄️ | 🗄️ | 🗄️ | ➖ |

> 🔀 in the DJ table means: **Serato** writes cues/grid to the file `GEOB` blob; **Traktor** to a file `PRIV` ('TRAKTOR4') frame + `collection.nml`. **Rekordbox** and **Mixxx** are 🗄️ **DB-only by default** (Mixxx file-write is opt-in). **Mixed In Key** writes key + energy to 📄 the file. All still surface one headline BPM/key.

### What the storage picture shows

- **Reference impl (SDTS)** is the file-first contrast (first column in every table): 📄 for everything it manages — including the **beatgrid** (`SB-Beats`) and the **waveform / analysis cache** (`SB-Chroma`/`SB-Stems`/Essentia, moodbar `PRIV`) that every DJ tool keeps in a 🗄️ DB or proprietary blob. It deliberately does NOT do library stats (play count / last-played → ➖; the consumer's session log handles as-played, not as a portable tag), DJ hotcues / loops (➖), or musical key (➖, out of scope). Some SDTS fields are still *proposed* (not yet written) — see the spec status; the storage MODEL is 📄 throughout.
- **Tag editors (Picard, Mp3tag, Kid3)** are pure 📄 — everything they touch is in the file (or ➖ unsupported). No database, so nothing is ever locked away; this is the portable baseline SDTS aligns with.
- **Library players (MediaMonkey, JRiver, foobar2000, Apple Music, Roon, Plex)** keep a lot in 🗄️ the library: **play count, last-played, date-added** are DB-side almost everywhere; **ratings** are often 🔀 or 🗄️. The famous traps: **Apple Music Start/Stop Time** and **JRiver Playback Range & Bookmark** are 🗄️ DB-only — they do NOT travel with the file (exactly why SDTS makes cue-in/out a portable 📄 field). **Roon/Plex** are essentially all 🗄️ (library-first; Roon never writes files). **MediaMonkey is the exception that writes its classification (Mood/Occasion/Custom) to 📄 the file** via `COMM:Songs-DB_*`.
- **DJ software** is where 🗄️ dominates for performance data (cue points, hotcues, beatgrid, loops, color, my-tags) — see the file-vs-DB splits in the table footnote above.
- **The portable spine** — title/artist/album, genre, BPM, key, comment, ReplayGain — is 📄 nearly everywhere. Divergence is concentrated in **DJ-performance data and library statistics**. SDTS's contribution is taking the **dance-DJ semantic fields** (dance/occasion, felt-tempo, energy, set-role, cue-in/out, …) that others either lack or lock in a 🗄️ DB and making them portable 📄 — so the DJ's knowledge travels with the track.

## The one portable rule

A field named `X` is the *same* field in every container via the cross-format triangle (`TXXX:X` = Vorbis `X` = MP4 `----:com.apple.iTunes:X`) — defined once in [`../standard/spec.md`](../standard/spec.md#binding-model-two-layers). SDTS fixes one canonical `SDTS_<UPPER_SNAKE>` and uses it everywhere.

## What each tool calls the cross-tool concepts

For concepts that several tools share, here is the de-facto name and the SDTS choice. (SDTS fields with no row here are SDTS-specific — see each field's **Other tools** note in `spec.md`.)

| Concept | SDTS field | De-facto across tools | SDTS choice |
|---|---|---|---|
| mood | `mood` | Strong de-facto standard EXISTS: TMOO (ID3v2.4) is used by Picard, Mp3tag, Kid3, MusicBee, MediaMonkey 5, JRiver and beaTunes. | TMOO (keep standard) |
| energy / intensity | `energy` | NO native energy frame exists in any tagger. | TXXX:SDTS_ENERGY (structured, not COMM) |
| musical key | `none (TKEY)` | Universal de-facto standard: TKEY (ID3) / INITIAL_KEY (Vorbis) / WM/InitialKey (WMA). | TKEY (align; no SDTS field yet) |
| grouping / bucket | `none (TIT1)` | Contested standard due to the 2016 iTunes 12.5 split: legacy/universal tools use TIT1 for grouping; | TIT1 in the reference implementation (not an SDTS field) |
| situation / occasion / 'my tags' | `theme` | No clean cross-tool *standard*, but a real de-facto *concept*: MusicMatch 'Situation', MediaMonkey & MusicBee 'Occasion'. | TXXX:SDTS_THEME (matches that de-facto concept). NB: the reference implementation repurposed the `COMM:Songs-DB_Occasion` frame for `dances`, so `theme` is stored in `Songs-DB_Custom9`. `dances` (which partner dance) is SDTS-specific — no cross-tool row. |
| comment | `comment-ish` | Universal: COMM (ID3) / ©cmt (MP4). | COMM (not an SDTS field) |
| rating | `rating` | De-facto standard frame is POPM, but it is a MESSY standard: POPM is keyed per-application by an e-mail identifier (traktor@native-instruments.de, Med… | POPM no@email |
| compilation | `none (TCMP)` | Universal de-facto standard: TCMP (the non-spec but ubiquitous 'iTunes compilation' flag, 1/0) / MP4 cpil. | TCMP if needed (not an SDTS field) |
| danceability | `danceability` | NO native tag field in ANY tagger. | TXXX:SDTS_DANCEABILITY |
| language | `lyric_language` | De-facto standard frame is TLAN (ISO 639-2 codes). | TLAN — recommend over SDTS_ |
| BPM | `bpm` | Fully universal de-facto standard: TBPM (ID3, integer per spec) / tmpo (MP4) / WM/BeatsPerMinute. | TBPM (keep standard) |
| ISRC | `isrc` | Fully universal de-facto standard: TSRC (ID3) / ISRC (Vorbis/APE) / ©isr (MP4). | TSRC (passthrough) |

## Collisions & reserved namespaces — never use these as an SDTS `TXXX` name

- **Shadows a real frame:** `MOOD` (→TMOO), `GROUPING`/`CONTENTGROUP` (→GRP1/TIT1, ambiguous since iTunes 12.5), `COMPILATION` (→TCMP), `INVOLVEDPEOPLE` (→TIPL/IPLS), `KEY`/`INITIALKEY` (→TKEY), `ISRC` (→TSRC), `LANGUAGE` (→TLAN), `BARCODE`, `CATALOGNUMBER`, `replaygain_*`.
- **Established TXXX conventions to reuse, not collide with:** `TXXX:ITUNESADVISORY` (Apple content advisory), `TXXX:MOOD` (foobar2000 v2.3 mood).
- **Vendor namespaces:** `MusicBrainz *`, `Acoustid *` (Picard); `GEOB:"Serato *"` (Serato); `PRIV` owner `TRAKTOR4` + `POPM` id `traktor@native-instruments.de` (Traktor); the `JR/` prefix (JRiver); `beaTunes_*` (beaTunes); MediaMonkey `COMM:Songs-DB_*` / `COMM:MusicMatch_*`.
- The `SDTS_` prefix sidesteps all of them — that is its job.

## Making SDTS fields visible in each tool

- **MediaMonkey 5:** add an Extended Tag (reads `TXXX:SDTS_*`) for visibility; mirror to a free `Songs-DB_Custom` slot for a real sortable column (MM5 writes `COMM` lang `XXX` — keep that).
- **JRiver / MusicBee:** define a custom Library/user field named `SDTS_DANCES` (etc.) and enable save-to-file → it round-trips `TXXX:SDTS_DANCES`.
- **Picard / foobar2000 / Mp3tag / Kid3 / TagScanner / Yate:** nothing to configure — `TXXX:SDTS_*` appears; read case-insensitively.
- **beets:** register a `MediaField` with `MP3DescStorageStyle('SDTS_*')`, then `beet write` (flexattrs are DB-only by default).
- **Apple Music / WMP / Plex / Emby / Winamp:** will not surface `SDTS_*` (fixed standard-frame readers) — the data still rides in the file.
- **FLAC / MP4:** automatic via the portable triangle.

The reference implementation also mirrors some `active` SDTS fields into MediaMonkey's frozen `COMM:Songs-DB_*` slots so they surface as sortable MM5 columns. That reused-slot ↔ column map (and the canonical-vs-mirror rule for `dances`) is reference-implementation storage detail — see [field rationale → MediaMonkey reused slots](../standard/field-rationale.md#mediamonkey-reused-slots).

## Sources

Cross-tool interop research + the storage-location pass (adversarially verified, 2026-06): id3.org, picard-docs.musicbrainz.org, docs.mp3tag.de, Kid3 handbook, getmusicbee wiki, beets/mediafile docs, hydrogenaudio (foobar2000 + ReplayGain), MediaMonkey wiki, wiki.jriver.com, Apple/iTunes scripting refs, Roon/Plex/Jellyfin/Navidrome docs, Xiph Vorbis spec, mutagen (MP4), Holzhaus/serato-tags, Traktor4 reverse-engineering, Deep Symmetry / pyrekordbox (Rekordbox ANLZ), Mixxx wiki, mixedinkey.com, beaTunes blog.
