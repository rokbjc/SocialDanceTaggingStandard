# Usage scenarios

End-to-end scenarios for the SocialDanceTaggingStandard (SDTS) reference library, each grounded in the **actual
public API** and the **real baseline vocabulary** — not invented surface. Every code block here has a
runnable counterpart in
[`dotnet/samples/SocialDanceTaggingStandard.Samples`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/dotnet/samples/SocialDanceTaggingStandard.Samples)
(`dotnet run --project dotnet/samples/SocialDanceTaggingStandard.Samples`).

!!! warning "What the library does today (read this first)"
    These scenarios are honest about the seams that are **not wired yet** — that boundary is the most
    useful thing to know before you build on SDTS.

    **Implemented and working:** the value-agnostic 3-way **sync engine** (`TagMirror` /
    `MirrorSyncEngine`), the **whole-record ID3 gateway** (`SdtsFile` — typed per-field read/write of a
    complete SDTS record, resolved via `SdtsFile.OpenId3`), the low-level **ID3 sources**
    (`Id3UserTextSource` over `TXXX`, `Id3CommentSource` over `COMM`), the **MediaMonkey bridge**
    (`MediaMonkeyMirror`), **value-level translation**
    (`TaxonomyValueTranslator` resolves `#LH` ⇄ `Lindy Hop` inside sync — opt-in; the default is the
    no-op `IdentityValueTranslator`), the **taxonomy** (`SocialDanceVocabulary` — alias→canonical
    resolution, validation, normalization), the per-value **`FieldValidator`**, and the normative
    **`StableHash`**.

    **Not wired yet:** the four recurring seams (the 7-field MediaMonkey map,
    `proposed` fields not yet on disk, shape-only validation, no gate enforcer) are summarized in the
    [seam map](#recurring-gaps-the-seam-map) at the end — and each scenario flags the ones it hits in
    its **Gaps** note.

The scenarios:

| # | Scenario | Primary packages |
|---|----------|------------------|
| [1](#1-normalize-a-crate-to-sdts) | Batch "Normalize to SDTS" over a crate | MediaMonkey · Taxonomy |
| [2](#2-first-sync-migration-off-mediamonkey) | First-sync migration off MediaMonkey | MediaMonkey |
| [3](#3-build-per-dance-pools-an-energy-arc) | Per-dance tempo pools + energy arc | Id3 · Taxonomy |
| [4](#4-cross-app-conflict-resolution) | Cross-app conflict (foobar vs MediaMonkey) | Sync · MediaMonkey |
| [5](#5-re-implement-sdts-in-another-language) | Re-implement SDTS in another language | conformance vectors (CC0) |
| [6](#6-pre-gig-tag-audit) | Pre-gig tag audit / lint | Validation · Taxonomy |
| [7](#7-portable-cue-points) | Portable cue points & playback automation | Id3 |
| [8](#8-the-confirmation-gate-trust-boundary) | Audio enrichment + gate trust boundary | Sync (StableHash) |
| [9](#9-cross-source-duplicate-merge) | Cross-source duplicate merge | Taxonomy (StableHash) |
| [10](#10-how-a-consumer-app-consumes-sdts) | How a consumer app consumes SDTS (integration view) | Sync · Taxonomy · MediaMonkey |
| [11](#11-add-a-new-dance-to-the-vocabulary) | Add a new dance (vocabulary extension) | direct vocabulary edit · scripts |
| [12](#12-customize-the-vocabulary-for-your-deployment) | Customize the vocabulary for your deployment | Taxonomy (overlay API) |

---

## 1. Normalize a crate to SDTS

**Who / trigger.** An owner app's batch *"Normalize to SDTS"* command runs when a DJ right-clicks a folder
of ~400 curated swing/blues MP3s. The dance/feel/ensemble data lives only in MediaMonkey's legacy
`COMM:Songs-DB_*` columns; a prior pass hand-confirmed BPM (a half-time fix locking Ella's "Rug Cutter"
at 250) and genre. The command must lift the legacy columns into portable `TXXX:SDTS_*`, keep MediaMonkey
in step, and **not** clobber the confirmed BPM/genre.

**Fields.** `dance`, `rhythmic_feel`, `instrumentation`, `song_form`, `instrument_led`, `theme`,
`vocality` (the 7 mirrored fields) — `bpm`/`genre` are handled outside the mirror.

```csharp
var options = MirrorSyncOptions.Default with
{
    Direction    = MirrorDirection.Bidirectional,
    OnConflict   = ConflictResolution.ReportOnly,   // never guess a real two-sided conflict
    WriteThrough = true,
};

using MediaMonkeyMirror mirror = MediaMonkeyMirror.Open(path, options);
SyncReport report = mirror.Sync();                  // reconciles the 7 mapped fields
if (report.HasUnresolvedConflicts) { /* park in a human queue */ }

// The sync seam is value-agnostic. Either canonicalize the imported dances yourself (here) or pass
// TaxonomyValueTranslator to resolve '#LH' <-> 'Lindy Hop' inside sync (scenario 10):
var dances = SocialDanceVocabulary.LoadDefault().Dances("swing", "blues");
// "#LH; Bal"  ->  "Lindy Hop; Balboa"
mirror.Flush();   // persists the TXXX:SDTS_MM5_SYNC baseline + saves the file
```

**Outcome.** Each clean file carries portable `TXXX:SDTS_*` frames mirroring (and now leading) the legacy
`Songs-DB_*` columns; the still-installed MediaMonkey keeps reading them because `WriteThrough` kept them
in step. Confirmed BPM/genre survive untouched (outside the map). Conflicts are parked, not guessed.

**Gaps.** `bpm`/`genre` are not in the map → `mirror.WriteCanonical("bpm", …)` throws; gate enforcement is
the owner app's policy code, not the library's; private `SB-*` caches don't fit `additionalFields` (a
`FieldMirror` needs a `COMM` mirror key) — write them straight onto the shared `Id3File` via
`Id3UserTextSource`.

---

## 2. First-sync migration off MediaMonkey

**Who / trigger.** A DJ with a 12,000-track MediaMonkey library (dances in the *Occasion* column for
~8 years) whose MediaMonkey install starts crashing. She wants a one-time, read-only pass that lifts the
legacy `COMM:Songs-DB_*` values into portable `TXXX:SDTS_*` — without MediaMonkey running, without touching
the originals.

```csharp
var options = new MirrorSyncOptions { Direction = MirrorDirection.ImportOnly };
using MediaMonkeyMirror mirror = MediaMonkeyMirror.Open(path, options);

SyncReport report = mirror.Import();   // one-way mirror -> canonical, verbatim
foreach (FieldSyncResult f in report.Fields)
    Console.WriteLine($"{f.Field}: {f.Outcome} '{f.CanonicalValue}'");
//  dance Imported 'Lindy Hop; Balboa' | rhythmic_feel Imported 'Jazz Swing 8ths' | song_form Imported 'AABA'

mirror.Flush();   // writes SDTS_* + the SDTS_MM5_SYNC baseline; Dispose() alone would NOT save
```

**Outcome.** The file ends up with **both** the frozen `COMM:Songs-DB_*` frames and the new portable
`TXXX:SDTS_*` set, pinned to ID3v2.4 so the `"; "`-joined multi-value dances frame round-trips.

**Gaps.** Ratings **do not** migrate through this bridge — `POPM` is already a portable standard frame and
is not in the map. `Import()` never produces a conflict (use `Sync()` for that). The map's outcome for a
field absent on both sides is unspecified — treat `report.Changed` as a spot-check signal. No batch driver
or threading: you enumerate the 12,000 files yourself.

---

## 3. Build per-dance pools & an energy arc

**Who / trigger.** A DJ's set planner. An hour before a blues social, the class teacher asks for
*"slow-drag basics, instrumental only, nothing over 90."* The planner carves an instrumental Slow-Drag
pool in the 60–100 BPM band and sequences the social as opener → peak → cooldown.

**Fields.** `dance`, `rhythmic_feel`, `vocality`, `bpm`, `energy` (proposed), `mood`.

```csharp
var vocab    = SocialDanceVocabulary.LoadDefault();
var danceDim = vocab.Dances("swing", "blues");
var feelDim  = vocab.Dimension("rhythmic_feel")!;
var slowDrag = feelDim.Resolve("Slow Drag");

using var file = SdtsFile.OpenId3(path);

var dances = danceDim.Normalize(file.Dances);
bool blues = dances.Any(d => d.Id is "blues-dancing" or "slow-drag");        // "#BLS; #SD" resolves here

int bpm = file.Bpm ?? 0;   // bpm is the standard TBPM frame — the gateway surfaces it as file.Bpm
if (blues && /* vocality is instrumental && feel is slow-drag && */ bpm is >= 60 and <= 100) { /* class pool */ }
```

**Why this is SDTS-specific.** `dance` answers *which* social dances a track serves (genre cannot);
`rhythmic_feel` separates two 120-BPM records into different dances; the per-dance tempo bands are real
term attributes (`Blues` 50–120, `Slow Drag` 40–70). The opener→peak→cooldown arc is what the
proposed 1–10 `energy` field is for.

**Gaps.** **BPM now reads through the gateway** — `file.Bpm` surfaces the standard `TBPM` frame, so no
consumer has to reach around to `file.Tag.BeatsPerMinute` (TagLib#) any more; the whole record is resolved
via `SdtsFile.OpenId3`. `energy` is **proposed** (unwritten on most files) → the arc falls back to the mood term's energy axis.
Reading `SDTS_*` assumes files are already migrated (scenario 2); otherwise the data is still in the
`COMM` mirror.

---

## 4. Cross-app conflict resolution

**Who / trigger.** A DJ tags in MediaMonkey but plays out of foobar2000. The same track's `dance` got
edited in **both** since the last sync: foobar wrote `TXXX:SDTS_DANCES = "Lindy Hop; Balboa"`, MediaMonkey
wrote `COMM:Songs-DB_Occasion = "Lindy Hop; Collegiate Shag"`, and the baseline recorded them agreeing on
`"Lindy Hop"`. Both sides moved → a true 3-way conflict. A separate one-sided `rhythmic_feel` edit is not.

```csharp
var engine = new MirrorSyncEngine();

// 1) Dry run with the defaults (Bidirectional, ReportOnly): surface, do not clobber.
SyncReport report = engine.Reconcile(canonical, mirror, state, MediaMonkeyFrames.Map, MirrorSyncOptions.Default);
//   dance         Conflict resolved=False  c='Lindy Hop; Balboa' m='Lindy Hop; Collegiate Shag'
//   rhythmic_feel Imported resolved=True                          (one-sided edit imports even under ReportOnly)

// 2) DJ judges the foobar edit authoritative: canonical wins.
engine.Reconcile(canonical, mirror, state, MediaMonkeyFrames.Map,
    MirrorSyncOptions.Default with { OnConflict = ConflictResolution.PreferCanonical });
//   -> Songs-DB_Occasion overwritten with 'Lindy Hop; Balboa', baseline advances, HasUnresolvedConflicts=false
```

**Outcome.** `ReportOnly` leaves the conflicting field (and its baseline) untouched so it re-surfaces every
sync until a human decides; the one-sided edit still imports in the same run. `PreferCanonical` lands the
DJ's call. After `Flush()`, MediaMonkey and foobar read identical dances.

**Gaps.** `mood`/`genre`/`rating` aren't in the map (out of this mirror's scope). Identity translation only
— a `#LH`-coded mirror value would compare byte-for-byte as different from `Lindy Hop` and surface a
spurious conflict. There is no separate read-only conflict-inspection API — the report comes from a real
`Sync()` (which also performs the one-sided imports/exports).

---

## 5. Re-implement SDTS in another language

**Who / trigger.** A developer builds `socialdancetags-rs` for a WASM tagger. The browser writes the
`dance` baseline hash; that night the same file is 3-way-synced by the .NET `MediaMonkeyMirror`. If the
Rust FNV-1a doesn't produce the **identical** 8-hex baseline, the .NET engine sees a phantom divergence on
an untouched field.

```rust
const OFFSET_BASIS: u32 = 2166136261;
const PRIME: u32 = 16777619;

fn stable_hash(value: Option<&str>) -> String {        // MUST match .NET StableHash.Of + Python
    match value {
        None | Some("") => "00000000".to_string(),
        Some(v) => {
            let mut h = OFFSET_BASIS;
            for b in v.as_bytes() { h ^= *b as u32; h = h.wrapping_mul(PRIME); }   // UTF-8 bytes, not chars
            format!("{:08x}", h)
        }
    }
}
// conformance/vectors/hash.json (CC0):
//   "Lindy Hop" -> 41379818 | "Lindy Hop; Balboa" -> 9e33e3a8 | "Café" -> cfc290a9 | "120" -> 733864ae
```

The normative byte-level definition is [StableHash](stable-hash.md); the .NET side of the same proof —
`StableHash.Of` against the vectors, plus `FieldValidator` against the `dance`/`bpm` constraints — is the
runnable
[`Scenario05_Conformance`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/dotnet/samples/SocialDanceTaggingStandard.Samples/Scenario05_Conformance.cs).

**Why this is SDTS-specific.** The sync baseline *is* a `StableHash` and the engine compares only by that
hash, so a one-bit divergence across ports doesn't corrupt data — it silently mis-fires the DJ's
MediaMonkey↔SDTS sync. The test values carry SDTS's own notation (the `:core`/`:edge`/`:fusion` purity
grade and trailing `?` needs-validation marker on the `dance` field). The **CC0** license on the
spec + vectors is what makes an independent port legally possible (library code stays Apache-2.0).

**Gaps.** Catalog membership is unportable today — it lives only in the YamlDotNet-embedded
`SocialDanceTaggingStandard.Taxonomy` (no Python/Rust port), so a port can prove **shape**, not membership. There is no
conformance vector for `Dimension.Resolve`/`Normalize` (alias collapse, hierarchy dedup) — only
`hash.json`, `validation/fields.json`, and `sync/reconcile.json` exist. A `' 5 '` (whitespace-padded int)
diverges between Python `int()` and .NET `int.TryParse`, uncovered by any vector.

---

## 6. Pre-gig tag audit

**Who / trigger.** A DJ who maintains a shared library batch-tags ~300 MP3s the night before a social. Last
month the set-builder silently dropped a Balboa block because one track had `bpm "400"` and another
`dance "Blues:traditional"`. She wants a one-shot lint that reports malformed values **and** dance
shorthand to canonicalize.

```csharp
var bpm    = new IntConstraint { Min = 1, Max = 330 };
var dances = new EnumConstraint { Cardinality = "multi",
                                  Grades = ["core", "edge", "fusion"], AllowValidationMarker = true };

// Pass 1 — shape (machine-readable codes: not_integer / out_of_range / not_in_enum / bad_grade / no_match)
FieldValidationResult r = FieldValidator.Validate("400", bpm);   // r.IsValid == false, r.Error?.ToWireCode() == "out_of_range"
FieldValidator.Validate("Lindy Hop; Blues:traditional", dances); // -> bad_grade

// Pass 2 — vocabulary/alias (FieldValidator does NOT check membership for open enums)
var dim = SocialDanceVocabulary.LoadDefault().Dances("swing", "blues");
dim.Resolve("#LH")?.Canonical;       // "Lindy Hop"  (alias -> canonical)
dim.Resolve("Lindyhop");             // null         (no fuzzy match -> needs_manual_fix)
```

**Why two passes.** `FieldValidator` is per-value **shape** only; it intentionally skips membership
for open enum fields, so canonicalizing DJ shorthand (`#LH` → `Lindy Hop`, `Bal` → `Balboa`,
`#BLS` → `Blues`) requires the separate taxonomy package and its alias-aware `Dimension.Resolve`.

**Gaps.** No `FieldConstraints` loader/registry — you hand-build constraints (or deserialize the conformance
vectors). No whole-record validator. `FieldConstraints.Delimiter` is dead config (`"; "` is hardcoded).
`meter`/`energy`/`tempo_stability` are proposed.

---

## 7. Portable cue points

**Who / trigger.** A social DJ wants tracks to drop straight into the groove (skip a long spoken/rubato
intro) and to cut an applause tail for tight transitions. JRiver (`[Playback Range]`) and iTunes
(Start/Stop Time) do this but keep it in **their** database, so it is lost the moment the file moves apps.

**Fields (all proposed).** `playback_start`, `playback_stop`, `intro_duration`, `outro_duration`,
`ending_type`, `fade_in_duration`.

```csharp
SdtsFile.Edit(path, file =>
{
    file.PlaybackStart = TimeSpan.FromSeconds(8);            // skip an 8 s spoken intro
    file.PlaybackStop  = TimeSpan.FromMilliseconds(195000); // hard cut at 3:15
    file.EndingType    = EndingType.Definitive;
});
```

**Outcome.** The cue-in/cue-out and ending type travel *with the file* as plain `TXXX` scalars — readable by
any SDTS-aware player on any machine, unlike the DB-only equivalents in JRiver/iTunes. The `SdtsFile` gateway
surfaces them as typed `TimeSpan?` / `EndingType?` accessors — no raw `SDTS_*` descriptions in consumer code.

**Gaps.** These fields are `proposed` in
[`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml) — the descriptions
above are their planned home; no player writes/reads them yet.

---

## 8. The confirmation-gate trust boundary

**Who / trigger.** The reference implementation's audio pipeline derives measured fields from the beat grid
(`tempo_stability`, `intro_duration`, …) and the DJ hand-confirms BPM (the "Rug Cutter" half-time fix at
250). Every later re-analysis pass must **refuse** to overwrite a field the DJ confirmed — otherwise the
confirmed half-time BPM gets re-doubled on the next run.

```csharp
// The gate ledger records what the DJ verified, with an optional 4-hex content CRC:
string crc = StableHash.Of("250").Substring(0, 4);          // a consumer convention for the compact CRC
txxx.Set("SDTS_CONFIRMATION_GATES", $"BPM:1:{crc}; Beats:1");

// A re-analysis mis-locks on the half-time subdivision and proposes 125:
var gates = ParseGates(txxx.Get("SDTS_CONFIRMATION_GATES"));
if (gates.ContainsKey("BPM")) { /* REFUSE the overwrite — keep the confirmed 250 */ }
```

**Why this is SDTS-specific.** No mainstream tagger has a per-field "verified" ledger. The gate string
round-trips with an optional CRC (`BPM:1:a3f2`) for tamper detection — the trust boundary that makes
automated enrichment safe on a hand-curated library.

**Gaps.** **There is no gate enforcer in any SDTS package.** SDTS round-trips the gate string, and
`FieldValidator` can shape-check it (`composite` pattern), but *honoring* the gate — and computing the CRC
— is 100% consumer policy. (`StableHash` is the natural primitive; the 4-hex CRC convention shown here is
the consumer's choice.)

---

## 9. Cross-source duplicate merge

**Who / trigger.** The same track arrives from two sources — a 320 kbps vinyl rip and a 128 kbps YouTube
Music rip. The reference implementation picks a primary, fills the primary's **empty** fields from the secondary, and never
overwrites a present or gated field; the gate ledger flags which copy's confirmed values to keep.

```csharp
// Pick primary by bitrate / curated source (a reference-implementation heuristic, not an SDTS API),
// then fill only the primary's empty fields:
foreach (var (key, value) in secondary.Tags)
{
    if (key == "SDTS_CONFIRMATION_GATES") continue;              // gate ledgers are not merged
    if (string.IsNullOrEmpty(primary.Tags.GetValueOrDefault(key)))
        primary.Tags[key] = value;                             // e.g. SDTS_THEME 'Late Night' fills in
}
// then canonicalize the merged dances through the taxonomy, as in any normalize pass.
```

**Outcome.** The primary keeps its present values and gains the secondary's missing ones; the merged
`dance` is canonicalized the same way as a normalize pass.

**What SDTS actually provides here.** The gate ledger + `StableHash` (for the content CRC) and the taxonomy
canonicalization. The pick-primary heuristic and the merge policy are the consumer's — SDTS does not ship a
merger.

---

## 10. How a consumer app consumes SDTS

**Who / trigger.** A consumer app depends on every SDTS package and keeps its own audio
analysis (`SB-*`) in a private audio-tools library. SDTS ships dependency-light and value-agnostic on purpose, so
the integration work is **filling the seams the library leaves open** — on every track through the consumer's
pipeline. Scenarios 1–9 are tasks; this is the composition view.

**The three seams the consumer fills.**

- **A taxonomy-backed `IValueTranslator`.** The library now ships `TaxonomyValueTranslator` (backed by
  `SocialDanceVocabulary`); wire it into a binding's `translator` and `#LH` ⇄ `Lindy Hop` is resolved
  *inside* sync — closing the manual canonicalize step from scenario 1. (`IdentityValueTranslator` stays
  the pass-through default.)
- **Private `SB-*` caches on the same file** (the beat grid, and any JSON blob — e.g. a consumer's
  choreography `SB-Choreography` / `SB-DanceSteps`) — read/written through the same `Id3UserTextSource`;
  the engine never touches them because they are not in the field map.
- **Extra private mirrored fields** via `additionalFields` (`MirrorMap.Compose`), plus the gate-enforcement
  policy (read the ledger, refuse the overwrite).

```csharp
// The taxonomy translator ships in …Taxonomy. Wire it (+ a private mirrored field) into the bridge;
// the private SB-* caches ride the same file, untouched by sync (they are not in the field map).
var vocab = SocialDanceVocabulary.LoadDefault();
var extra = new[] { new FieldMirror("sb_energy", "SB-Energy", "Songs-DB_Custom2") };
using var mm = MediaMonkeyMirror.Open(path, MirrorSyncOptions.Default,
    translator: new TaxonomyValueTranslator(vocab, "; ", "swing", "blues"),
    additionalFields: extra);
mm.Sync();    // now '#LH; Bal' in the MM Occasion column imports as 'Lindy Hop; Balboa'
mm.Flush();
```

**Outcome.** One engine, now value-aware: the legacy `#LH; Bal` imports as `Lindy Hop; Balboa` automatically;
the private `sb_energy` reconciles and `SB-Beats` sits untouched, exactly as mapped; gated fields are
protected by the consumer's policy. SDTS supplies the mechanism (and now the translator); the consumer wires
it and adds its own private storage (`SB-*`), extra mappings (`additionalFields`), and gate policy.

**Gaps.** Gate enforcement is consumer policy (no enforcer ships); `SB-*` stay private (never portable).
The whole-record read/write itself is now covered by the `SdtsFile` gateway.

---

## 11. Add a new dance to the vocabulary

**Who / trigger.** A contributor wants to add a dance to the vocabulary as a small, reviewable PR.

**The model.** `data/vocabulary/` is the **hand-maintained** source of truth for values — you edit it
directly. A dance lives in its scene's pack, so add a term under `values:` in
`data/vocabulary/packs/<pack>/dances.yaml`, matching the shape of the surrounding terms. (Shared music
dimensions like genre/mood live in `data/vocabulary/core/<dim>.yaml` instead.)

```yaml
# data/vocabulary/packs/swing/dances.yaml — add an entry under `values:`
- id: collegiate-shag
  canonical: Collegiate Shag
  aliases: ['#CSH']
  tradition: swing-vernacular
  family: swing-partner
  tempo_min: 180
  tempo_max: 250
  source: "scene / authority reference that supports this term"
```

```bash
python scripts/gen_vocab_docs.py    # compile the C# baseline + regenerate docs/model/vocabulary/ (browser lists it)
python scripts/validate_data.py     # data validity + cross-checks — CI gate
cd dotnet && dotnet test            # the drift gate confirms the compiled baseline matches the YAML
# LoadDefault().Dances("swing","blues").Resolve("Collegiate Shag") now resolves (canonical + #CSH)
```

**What validates / generates.** `gen_vocab_docs.py` compiles the term into `BaselineVocabulary.g.cs` + the
typed dimension classes and regenerates the docs browser; `validate_data.py` runs the schema + cross-checks;
the .NET drift gate asserts the compiled baseline matches the YAML. `gen_field_docs.py` is **not** needed — a
value doesn't touch `data/fields.yaml`.

**Distinctiveness.** A dance lives in its scene's pack (`packs/<pack>/dances.yaml`), not in shared `core/`;
canonical is the prose form with the `#`-code demoted to an alias.

**Gaps.** A brand-new *dance* cannot be added via the runtime overlay (scenario 12) — that path edits
aliases/terms of existing dimensions; new dances are added here, or a whole new scene as a hand-authored
`packs/<name>/` directory.

---

## 12. Customize the vocabulary for your deployment

**Who / trigger.** A studio deploys SDTS but its DJs never use the legacy `#LH` codes and want a house alias for a
genre — without forking the baseline data.

**Two layers, routed deliberately.** *Value* customization (aliases, terms, the `#`-code opt-out) is an
**immutable baseline + overlay** in the Taxonomy package; *field→frame mapping* customization (private fields,
which frame a field uses) stays in the Sync layer (`additionalFields` / `MirrorMap.Compose`, scenario 10). The
overlay never mutates the baseline — `With()` returns a NEW vocabulary; a `"dances"` key fans out to every pack.

```csharp
// Fluent builder (runtime):
var vocab = SocialDanceVocabulary.LoadDefault()
    .Customize()
        .RemoveAlias("dances", "#LH")            // opt out of the legacy '#' convention (codes ship bare: LH)
        .AddAlias("genre", "Swing", "My Swing")  // house shorthand
        .AddTerm("genre", new VocabularyTerm("balkan-brass", "Balkan Brass",
                 new[] { "Brass BG" }, false, new Dictionary<string, object?>()))
    .Build();                                    // -> a NEW vocabulary; baseline untouched

// Or a language-neutral YAML overlay (copy data/vocabulary/vocabulary.local.example.sdvo.yaml -> my.sdvo.yaml):
var v2 = SocialDanceVocabulary.LoadDefault(new FileInfo("my.sdvo.yaml"));
```

The YAML keys mirror the builder: `add_aliases` / `remove_aliases` / `add_terms` / `remove_terms`, each keyed by
dimension (a core dimension, or `"dances"` for every pack).

**Other customization axes:** value translation → wire the shipped `TaxonomyValueTranslator` (scenario 10) or
implement `IValueTranslator`. Display (code/icon/color) → edit the term's `display:` block in
`data/vocabulary/` + re-run `gen_vocab_docs.py` (generate-time only). Custom packs/scenes → add a
`packs/<name>/` directory. Custom validation → author `FieldConstraints` (scenario 6) + regenerate vectors with
`gen_conformance_vectors.py`. New container → implement `ISdtsSource` (no Vorbis/MP4 binding ships yet).

**Distinctiveness.** Value/vocabulary edits are an immutable-baseline overlay (builder OR YAML, both → `With` → a
new vocabulary, never a fork), while field→frame mapping is `additionalFields`/`MirrorMap.Compose`. The headline
move — opting out of the `#`-codes per-deployment — needs no data fork because codes ship bare and `#LH` is only
an alias.

**Gaps.** `AddTerm` is **core-dimensions only** — a new *dance* is a data activity (scenario 11 / authored pack),
not a runtime term. Display customization is generate-time only (no runtime override). There is no
`FieldConstraints` loader (hand-built in code). No Vorbis/MP4 binding ships (the whole-record ID3 gateway does).

---

## Recurring gaps (the seam map)

The same "not wired yet" facts surface across the scenarios. They are the most useful thing to know before
building on SDTS — and they point at what is next:

| Gap | Consequence | Where it bites |
|-----|-------------|----------------|
| MediaMonkey map = 7 fields | `bpm`/`genre`/`mood`/`rating` are in `TBPM`/`TCON`/`TMOO`/`POPM`; `WriteCanonical` throws for them | 1, 2, 3, 4 |
| 15/30 fields are `proposed` | `energy`/`meter`/`playback_*`/`tempo_stability` aren't written to disk yet | 3, 7, 8 |
| `FieldValidator` = per-value shape only | Catalog membership is the taxonomy package's job, not the validator's | 1, 5, 6 |
| No gate enforcer | Honoring `confirmation_gates` (and the CRC) is consumer policy | 1, 8, 9 |

---

## Implementations and writing your own

SDTS is a **vendor-neutral standard** — any tool can read and write the tags; the rest of the docs stay tool-neutral.

**Reference implementation.** The `SocialDanceTaggingStandard.*` packages in this repository, together with a DJ library manager for swing/blues, exercise the whole standard: portable `TXXX:SDTS_*` tag I/O with the legacy `COMM:Songs-DB_*` mirror kept in step (the `MediaMonkeyMirror` bridge); private `SB-*` audio-analysis caches that are **not** part of the portable standard (catalogued in [field rationale](field-rationale.md)); the confirmation-gate trust boundary; and the hand-maintained baseline vocabulary (`data/vocabulary/`) as the source of truth for values. Scenario 10 above is the composition view of how a consumer wires the packages together.

**Writing your own.** SDTS needs nothing from the reference implementation. Build a conformant implementation in any language from the [spec](spec.md), the [field reference](fields/README.md), and the public-domain (CC0) [conformance vectors](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/conformance). Scenario 5 above is the re-implementer walkthrough (StableHash + validator parity).

## Running the samples

```bash
# all in-memory scenarios (no files touched):
dotnet run --project dotnet/samples/SocialDanceTaggingStandard.Samples

# additionally run the Id3File / MediaMonkeyMirror file-based variants over a real MP3:
dotnet run --project dotnet/samples/SocialDanceTaggingStandard.Samples -- "path/to/track.mp3"
```

Each runtime scenario is a self-contained class
([`Scenario01_…`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/dotnet/samples/SocialDanceTaggingStandard.Samples)
through `Scenario12_…`); the in-memory demos run as-is and only the file-based variants need an MP3. Scenario 11
(adding a dance) is a CLI/data flow — its steps are the shell commands above, not a .NET sample.
