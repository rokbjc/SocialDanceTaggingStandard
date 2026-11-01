# SDTS baseline vocabulary

The **normative value sets** for SDTS — the canonical dances, genres, moods, feels, … that make the
standard self-contained. `data/fields.yaml` defines the field *schema* and bindings; this directory
defines the *values* those fields may take.

> **Hand-maintained.** These files are the source of truth for SDTS values — edit them directly via
> pull request. After editing, regenerate the derived artifacts (the compiled C# baseline and the docs
> browser) with `python scripts/gen_vocab_docs.py`, and validate with `python scripts/validate_data.py`.

## The fault line: music is shared, dance is partitioned

A value that describes the **music** (genre, mood, feel, meter, …) lives once in `core/` and is shared
by every pack. A value that names a **dance** lives in a per-family **pack**. Swing and blues are *not*
two separate vocabularies — they share all of `core/` and differ only in their dance lists.

```
core/                       shared MUSIC dimensions (one set, every pack reuses)
  genre.yaml  mood.yaml  rhythmic_feel.yaml  vocality.yaml  instrumentation.yaml
  instrument.yaml  recording_context.yaml  recording_quality_era.yaml
  theme.yaml  song_form.yaml  credit_role.yaml  routine.yaml
packs/<scene>/              DANCE dimension, partitioned by dance `family`
  swing/  dances.yaml  pack.yaml
  blues/  dances.yaml  pack.yaml
  …       (one directory per pack listed in manifest.yaml)
manifest.yaml               index: version, core dimensions, packs
```

`routine` is the one deliberate exception to the fault line. A routine names a choreography, which is
dance-side, but the `routine` field states which *recording* that choreography is danced to, and its
values cross-reference dances through `typical_dances` rather than belonging to one pack. It lives in
`core/` because a pack carries a single `dances` dimension; a pack-level home would need the pack
schema, loader and overlay schema extended for four values.

Each term carries a `display:` block — a **bare** `code` (`LH`, not `#LH`), an emoji, a real Iconify
icon (any set — `mdi:`, `game-icons:`, … — verified against the Iconify API), a `color`, and an optional
`label` (a short chip caption distinct from `code`: `Chi-Blues` vs `ChB`). A colour follows the term's
axes (mood by energy×valence, genre by family, recording era by ordinal, else a per-dimension hue; a
dance takes its tradition's hue band but never repeats, see below); colours may repeat across
categories. `instrument` code/emoji come from the reference
implementation's instrument registry.

Everything about a value — its description, `display:` block, demo `audio:`, and a genre's typical
feels and song forms — is authored on the term, in these files; there is no second copy to keep in
sync. An `audio.youtube` link may be a YouTube search where no single recording could be verified.

An attribute whose values a domain expert has not confirmed yet is listed in its core file's top-level
`validation:` map (`typical_song_forms: expert-review-pending`). The docs flag it on the dimension page
and on every entry that carries it, and `scripts/validate_data.py` rejects a flag that would mark nothing.
A pack whose dances await review says so in its `pack.yaml` (`validation: expert-review-pending`); the
docs flag the pack's page and its line in the dances index. .NET exposes both as `ReviewStatus`:
`Dimension.AttributeReview` and `DancePack.ReviewStatus`.

## Packs

A pack is a hand-authored dance set for a scene: `packs/<scene>/dances.yaml` (the dances) plus
`pack.yaml` (its metadata — `display_name`, `description`, `extends: core`, dance `families`). To add a
scene, add a `packs/<scene>/` directory and list it in `manifest.yaml`. Current packs: `swing`,
`blues`, `ballroom`, `street-latin`, `tango`, `west-coast-swing`, `carolina-shag`, `early`.

Every dance family may ship as a pack, because a pack is opt-in: a library never sees a pack's values
unless it enables that pack. What stays out of scope is scene *machinery* — scene-specific fields and
sub-style granularity (tanda/cortina, On1/On2, International-vs-American ballroom, MPM); see
`docs/standard/field-rationale.md`.

## Dance hierarchy, colour, tempo and feel rank

- **`parent` is a strict tree.** A dance has at most one `parent`, the id of a dance in the same
  dimension, and a term is a *group* exactly when another term names it as `parent`. Groups: `swing` (the
  swing-era dances, West Coast Swing and Carolina Shag), `ballroom-standard`, `ballroom-latin`,
  `street-latin`, `argentine-tango-family`; the blues idioms and Slow Drag hang under the dance `blues-dancing`
  (Blues). A group value has no `family`, `tradition` or tempo. `tap-dance` has no parent: it is a
  stage form. A dance that seems to belong to two groups gets two ids instead: `jive` (swing) and
  `jive-ballroom` (Ballroom Latin) are separate dances. `scripts/validate_data.py` rejects an unknown
  parent, a self-parent and a loop.
- **A dance colour is the dance's own.** No two dances share a `display.color`, none sits within 0.04 of
  another in OKLab, and every one keeps 3:1 contrast against `#121212`, so a chip stays readable on the
  dark theme. The colour still follows the tradition's hue band, but no longer repeats inside it.
- **`tempo_min`/`tempo_max` is a typical social-floor band, not a limit** (spec: Tempo bands). The
  ballroom pack records the WDSF/WDC competition tempo, which is narrower than social practice; its
  `tempo_note` says so.
- **`tier` on a rhythmic feel ranks it inside its cluster** (1 = the general feel). The 13 feels without
  one have no general parent in their cluster (the Latin, Brazilian, tango, march and waltz feels), so
  `feel_scenes.yaml` ranks them per dance scene; the validator requires every feel to have a tier or a
  scene.
- **`Fox Trot` (early pack) and `Slow Foxtrot` (ballroom pack) are two dances.** The first is the
  1910s social dance, the second the International Standard dance descended from it.

## Data-model conventions

- **Dances are prose-canonical.** `Lindy Hop` is the canonical value (the same model as genre); the
  bare code `LH` resolves too, via the term's `display.code`.
- **No `#`-prefixed codes in the baseline.** `#LH`, `#BLB`, … are one DJ's personal shorthand, not part
  of the standard — they leaked in because the vocabulary was seeded from a private catalog, and were
  relocated to the personal-overlay layer in 2026-07. A deployment that wants them adds them back with
  `add_aliases` (see `vocabulary.local.example.sdvo.yaml`). Note the two namespaces are **not** parallel:
  `#SLS` was Salsa, while bare `SLS` is St. Louis Shag — never migrate an on-disk value by stripping the
  `#`; resolve it through the vocabulary.
- **No binding metadata here.** MM5/ID3 frame names, delimiters, and maintenance notes are not stored
  in the vocabulary — bindings live in `data/fields.yaml`.

## Consuming it

The `SocialDanceTaggingStandard.Taxonomy` package compiles these files into generated C#
(`BaselineVocabulary.g.cs`); `SocialDanceVocabulary.LoadDefault()` builds the vocabulary with no file I/O
or YAML parsing (AOT/trim-safe). It is still the only package that pulls in YamlDotNet — now only for
optional runtime overlays (`*.sdvo.yaml`) and the internal drift-gate oracle; the core model and
tag bindings stay dependency-light:

```csharp
using SocialDanceTaggingStandard.Taxonomy;

var vocab = SocialDanceVocabulary.LoadDefault();
vocab.Dimension("genre").Resolve("Swing");          // canonical/alias lookup
vocab.Packs["swing"].Dances.Resolve("LH");          // → "Lindy Hop" (bare display code)
vocab.EnabledDances("swing", "blues");              // merged dance set for a swing+blues library
vocab.IsValid("mood", "Happy");                     // membership check (alias-aware)
vocab.Normalize("genre", new[]{"Swing","Big Band"}); // dedup + parent/child hierarchy resolution

var lindy = vocab.Packs["swing"].Dances.Resolve("Lindy Hop");
lindy.Display.Code;   // "LH"    lindy.Display.Emoji;  // "🤸"
lindy.Display.Icon;   // "mdi:dance-ballroom" (Iconify, any set)   lindy.Display.Color;  // "#E0A030"
```

**Rich descriptions are canonical.** Each term's `description` is the authoritative prose for that value —
its meaning *and* how it differs from its siblings (the "Distinguishing trait: … vs X" clause). A consumer
that needs per-value prose (e.g. an LLM-classifier ontology) sources it from the term `description` — via
the `core/*.yaml` / `packs/*/dances.yaml` here, or the typed catalog's `.Description` (`Moods.All`,
`Genres.All`, `Dances.All`) — not from any private catalog. Consumer-specific scaffolding (prompt wording,
model-tuned decision rules) stays on the consumer side; SDTS stays vendor-neutral.

## Customizing it (per-developer)

The baseline is immutable; a deployment can layer its own edits on top — add/override aliases, add or
remove terms, opt out of the legacy `#` codes — without forking the data. Two equivalent ways:

**Data overlay** (`<name>.sdvo.yaml`, language-neutral — see `vocabulary.local.example.sdvo.yaml`, and
`examples/legacy-hash-codes.sdvo.yaml` for the relocated personal `#` codes):
```csharp
var vocab = SocialDanceVocabulary.LoadDefault(new FileInfo("my.sdvo.yaml"));
```

**Builder API** (.NET, runtime):
```csharp
var vocab = SocialDanceVocabulary.LoadDefault()
    .Customize()
        .AddAlias("genre", "Swing", "My Swing")
        .AddAlias("dances", "Lindy Hop", "#LH")   // your own shorthand, on a pack dance term
        .AddTerm("genre", myTerm)
    .Build();
```

Both build a `VocabularyOverlay` applied by `vocab.With(overlay)` → a new vocabulary (a `"dances"` key
applies to every pack). Field→frame **mapping** customization lives in the Sync layer
(`MirrorMap.Compose` / a binding's `additionalFields`).
