# SDTS Personal Profile + ingest mapping (optional, non-normative layer)

> **Optional, non-normative.** A per-DJ overlay + ingest mapping on top of the immutable baseline. The
> baseline vocabulary, the [stable hash](stable-hash.md), and the [sync semantics](../sync/sync-strategy.md)
> are unchanged — nothing here bumps `sdts_version`. The schema (`schemas/profile.schema.json`) and the
> resolution conformance vectors (`conformance/vectors/profile/resolution.json`) ship in-repo.

## 1. Why

Different DJs tag with completely different, idiosyncratic schemes. Forcing the standard's vocabulary on
them up front is the wrong on-ramp. The goal: **a DJ keeps working in their own scheme, sees it presented
as SDTS, and migrates to the canonical vocabulary at their own pace.** Meet the DJ where they are.

Two concrete examples (the design is checked against these):

- The DJ uses `#LH` as a tag → it should *be understood as* `dances: Lindy Hop`.
- The DJ files Slovenian artists under `genre: "Slovenian"` → that's really `artist_country: SI`.

## 2. Two concepts, one file

A **Personal Profile** is a single per-DJ YAML artifact with three sections:

1. **Vocabulary overlay** — add / remove / rename / re-grade terms and aliases over the immutable baseline.
2. **Display overrides** — a **color** + optional short-label per value per dimension (UI only).
3. **Mapping rules** — project the DJ's *existing* idiosyncratic tags onto SDTS. v1 power = **alias**
   (same dimension) + **cross-dimension remap** (a value in dim X means a value in dim Y). No
   derivation/split in v1.

## 3. Architecture split

- **SDTS defines the format and owns the library mechanism.** A conforming library imports a profile,
  validates it, builds a *customized* vocabulary (baseline untouched), and exposes a resolver
  (alias + cross-dimension projection). SDTS ships the **schema** (`schemas/profile.schema.json`) and the
  importer/resolver semantics.
- **The consumer app implements the use.** It holds the actual profile data, wires the resolver
  in place of the identity translator, renders colors/labels, and triggers the migrate action.
- **The SDTS baseline vocabulary stays the taxonomy authority.** A *suggested* color on a STANDARD term is a
  baseline-vocabulary attribute; the profile only *overrides* it and sets colors for the DJ's own custom terms.
  SDTS resolves no taxonomy values unilaterally. Color-distinctness is scoped to the **dance category**: distinct within one
  category (a DJ sees ~one category per night), may repeat across categories (salsa vs swing rarely share a
  night). Non-dance dimensions shown all-at-once (e.g. moods) keep distinctness global within the dimension.

## 4. The hard invariant: hash-safety

`StableHash` is FNV-1a over the **raw canonical value bytes** and is normative + cross-language. The whole
design is built so a profile **can never alter the canonical string that reaches the hash**:

- **Custom terms** carry a `user-`-namespaced **id** (collision-free identity), but the **canonical** is a
  plain string. Only the canonical is hashed; the `user-` prefix is never folded in. (Same split the
  baseline already uses: id `lindy-hop` vs canonical `"Lindy Hop"`.)
- **Rename** changes only the display label and auto-aliases the old word; the stored/hashed canonical of a
  standard term never moves. (A DJ who wants their own canonical string adds a *custom term* instead.)
- **Color / short-label** are presentation-only; they are unreachable from the write/hash path by
  construction (the dimension's value lookup never reads display).
- **Cross-dimension remap** is resolved and discarded *before* any value reaches the sync layer; a migrate
  writes a plain canonical (`"SI"`, `"Lindy Hop"`) — byte-identical to what a native-canonical DJ types.

Consequence: two DJs with two different profiles still share one immutable baseline and **one interoperable
hash space**. A migrated file 3-way-syncs identically on a machine that never saw the profile.

## 5. Mapping: read-time projection vs opt-in migrate

The mapping engine lives **above** the sync layer and has two modes:

- **Read-time projection (default).** When the editor/player presents a track, the mapper *projects* the
  DJ's existing tags as SDTS for display: `genre:"Slovenian"` shows as `artist_country: SI`; `#LH` shows
  as `dances: Lindy Hop`. **It writes nothing and never constructs a sync/baseline object** — it is a pure
  function of (vocabulary, rules, input strings). The file on disk is unchanged. This is "work in your own
  scheme, see it as SDTS."
- **Migrate (explicit, opt-in).** When the DJ is ready, a migrate action computes a plan of canonical
  writes and applies them through the *normal* canonical write path (so they hash stably) — rewriting
  `#LH → Lindy Hop`, moving `Slovenian` from genre to `artist_country`. Per-rule `migrate: true/false`
  controls which rules participate; `on_source: keep|drop` controls whether a remapped source token stays.

Migration is the bridge: as the DJ rewrites tags to canonical, rules become no-ops and retire; a custom
term that proves broadly useful graduates into the SDTS baseline vocabulary (hand-maintained in
`data/vocabulary/`, edited directly via PR). The profile never forces anything.

## 6. Profile YAML (shape, with the real examples)

```yaml
profile_version: 1
sdts_baseline: "0.1.0-draft"   # advisory: spec version authored against; never normative
owner: "dj-handle"             # free-form label for whose profile this is

# (a) vocabulary overlay — same model as the baseline overlay
vocabulary:
  add_terms:
    genre:
      - id: user-balkan-brass        # custom ids MUST be user-namespaced (identity only, never hashed)
        canonical: Balkan Brass      # THIS string is what StableHash sees
        aliases: [Brass BG, Guča]
        origin: user                 # marks a profile-local custom term (never a baseline term)
        display: { code: BBR, emoji: "🎺", color: "#C0392B" }
  remove_terms: { genre: [reggaeton] }
  add_aliases:  { genre: { Swing: [My Swing] }, dances: { Lindy Hop: [Jitterbug] } }
  remove_aliases: { dances: ["#BLB"] }
  rename:  { dances: [ { term: Balboa, label: "Bal" } ] }     # DISPLAY ONLY; canonical "Balboa" unchanged + auto-aliased
  re_grade: { genre: [ { term: Vocal Jazz, attribute: dance_relevance, value: low } ] }

# (b) display overrides — color + label per dimension+value (UI only, never hashed)
display:
  genre: { Chicago Blues: { color: "#1F3A93", label: "Chi-Blues" } }
  mood:  { Melancholic:   { color: "#5D6D7E" } }

# (c) mapping — project existing idiosyncratic tags onto SDTS
mapping:
  alias:                              # same dimension; legacy token -> canonical
    - { from_dimension: dances, from_value: "#LH", to_value: "Lindy Hop", match: ci, migrate: true }
  cross_dimension:                    # value in dim X means a value in dim Y
    - { from_dimension: genre, from_value: "Slovenian",
        to_dimension: artist_country, to_value: "SI", on_source: keep, migrate: false }
  from_path:                          # read-time projection of filename/path onto SDTS
    - match: "swing/!root/*.mp3"      # path glob, relative to the library root
      mask: "%artist% - %title% <tail>"
      tail:
        rating: { kind: plus-run, to: rating, scale: clamp10 }     # +++ -> rating
        year:   { kind: regex, to: recording_year, pattern: '(19|20)\d{2}' }
        codes:                        # token -> (dimension, canonical value); also the recogniser
          Lin:   { dances: "Lindy Hop" }
          BAL:   { dances: "Balboa" }
      on_unknown_token: flag          # unrecognised tail tokens are audited, never guessed
```

Deliberate shape choices (not defaults):

- **`rename` is a display override**, never a new canonical — protects the hash (see §4).
- **same-dimension remaps fold into `add_aliases`** (one resolution mechanism; `cross_dimension` with
  `to_dimension == from_dimension` is a schema error).
- **`on_source` defaults to `keep`** (non-destructive); **`migrate` defaults to `false`** (read-time only).
- **`from_path` is read-time projection.** The `codes` dictionary doubles as the recogniser — a trailing
  token is "tail" only if it appears there; an unrecognised trailing token stays in the title and is
  surfaced by a library token audit (flag-don't-guess). Rules are scoped by a path glob, so one library
  can mix schemes. Migrate (writing the projected values to disk) is a separate, opt-in concern (see §5).

## 7. Identity governance

The profile validator (run at import, before the resolver is built) enforces:

- Custom term ids MUST match `^(x|user)-[a-z0-9][a-z0-9-]*$` (so they can never collide with baseline ids).
- A custom canonical colliding (case-insensitively) with a baseline canonical/alias in the same dimension
  is an error (prevents silent shadowing) — re-grade an existing term instead of adding a twin.
- A cross-dimension remap whose target `(dim, canonical)` doesn't resolve is reported and dropped (never
  emit a non-vocabulary canonical).
- Alias-uniqueness within a dimension is a runtime invariant (an alias resolving to two canonicals is an
  import error) — it can't be expressed in JSON Schema, so a conformance vector pins it.

## 8. Conformance & versioning

- **Optional + non-normative.** A profile never changes `sdts_version`; the baseline is immutable; an
  implementation does not prove conformance by reproducing any profile.
- **The one normative obligation is resolution semantics**, pinned by
  `conformance/vectors/profile/resolution.json`: alias resolution (exact + ci), cross-dimension projection
  (`keep`/`drop`), the hash-safety of rename / custom-term canonical (no namespace in the hash), migrate
  producing the canonical baseline hash, and the import-time invariants (alias-uniqueness, same-dimension
  rejection). Display (color/label) is never hashed and never conformance-tested.
- **The profile schema** (`profile_version`) versions independently of the spec and package streams.

## 9. Future (designed-for, not specified now)

- **Filename / path as a tag SOURCE.** The `mapping` block is a typed-rule list; a future
  `from_source: filename|path` rule kind feeds the same mapper with a `sourceDimension` of `"path"` —
  additive, no restructuring. Pattern syntax to adopt later: Mp3tag's `%field%` + `\`
  folder-delimiter convention (the DJ-world default).
- **Derivation / split** (one tag → multiple SDTS values, conditional) — out of v1; the `mapping` container
  is a discriminated list so a v2 `derive:` block is additive.
