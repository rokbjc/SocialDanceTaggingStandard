# Vocabulary alignment

How SDTS lines up with public vocabularies and models. ([Back to the spec](spec.md).)

SDTS follows a two-speed rule: **the storage mechanism is standards-aligned; the value vocabulary is
deliberately domain-specific** — because no public standard classifies social-dance music at this
granularity, which is the reason SDTS exists.

- **Standard frames stay canonical.** Where a real frame exists, SDTS uses it and treats `SDTS_*` as a
  mirror only: genre→`TCON`, mood→`TMOO`, bpm→`TBPM`, rating→`POPM`, lyric_language→`TLAN`,
  key→`TKEY`, isrc→`TSRC`. New fields ride the one cross-tool extension point every serious tagger
  supports — the ID3 `TXXX` frame — via the portable triangle (defined in [`spec.md`](spec.md#binding-model-two-layers)).
- **Mood = the standard affect model.** The Energy × Valence grid is the valence–arousal plane of
  **Russell's circumplex model of affect** (1980); **Thayer's** energy×stress model (1989) is the same
  plane relabeled. SDTS stores a 1–10 coordinate on each axis and leaves discretisation (3×3, 5×5, …)
  to the consumer.
- **Energy / valence / danceability = published audio features.** They are the human-confirmed 1-10
  analogues (the [1-10 perceptual scale](spec.md#perceptual-scales-rating-energy-valence-danceability)) of the
  Spotify / Echo Nest audio features of the same names — `valence` is a first-class SDTS field, not only
  the mood vocabulary's own axis.
  Spotify deprecated the audio-features endpoint for new apps on **2024-11-27**, which is the practical
  reason SDTS stores them as first-class tags rather than depending on a computed source (open
  alternative: Essentia / AcousticBrainz). `energy` also aligns to the Mixed In Key `ENERGYLEVEL` 1-10.
- **Genre spellings track public vocabularies.** No single accepted genre taxonomy exists (Discogs ~15
  genres + ~500 styles · MusicBrainz ~2,300 · Spotify ~6,000). SDTS canonical names follow the Discogs
  Style / MusicBrainz genre spelling where one exists (Gypsy Jazz, Jump Blues, Chicago Blues, Boogie
  Woogie, …) so files round-trip; other spellings are carried as aliases. The closed, swing/blues-scoped
  enum with `dance_relevance` grading is the deliberate divergence.
- **Recording context ↔ MusicBrainz types.** `recording_context` values cross-reference MusicBrainz
  primary/secondary release types (`musicbrainz_type` in the vocabulary: Live, Soundtrack, Broadcast,
  Bootleg, …); SDTS keeps it as one flat per-track enum rather than a release-level multi-type model.
- **Dances grouped by recognised families.** Grouping dances into families is a recognised public
  concept — **WDSF/WDC** International Standard & Latin, **USA Dance** American Smooth & Rhythm, and the
  swing/blues community's vernacular families. SDTS records this as a cross-scene `tradition` axis on
  each dance (`swing-vernacular`, `blues-vernacular`, `argentine-tango`, `ballroom-international-standard`,
  `ballroom-international-latin`, …) so a reader from any scene recognises their own world and name
  collisions (Tango, Waltz, Jive) disambiguate by tradition. But there is **no public standard for
  tagging which dance a track serves** (closest: BallroomDJ 4's single `Dance` field) — so `SDTS_DANCES`
  is the canonical home, and the ordered-multi-value + `:core/:edge/:fusion` purity grade are SDTS-original.
- **Genuinely novel (no standard).** `rhythmic_feel` and `song_form` have no tagging standard in any
  container or database (only computed adjacents: Essentia rhythm, segment analysis). Divergence here is
  the point, not an oversight.
