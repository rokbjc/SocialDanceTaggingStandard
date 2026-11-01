# SDTS documentation

The **Social-Dance Tagging Standard** describes a music track the way a social-dance DJ thinks about
it — which dances it serves, its tempo and feel, mood, structure, and recording character — in a
portable tag that travels with the file. Swing and blues first, with room for the wider social-dance
world.

Two doors, depending on why you're here.

## Understand the music

For DJs, dancers, and anyone curious how the music is described.

- **[The music & dance model](model/index.md)** — the handful of axes a track is described by, in plain language.
- **[Vocabulary](model/vocabulary/README.md)** — the actual values: every [dance](model/vocabulary/dances.md) with its tempo band, the [mood grid](model/vocabulary/mood.md), the genres and feels.
- **[Field reference](standard/fields/README.md)** — one page per field, grouped by the question it answers.

## Build with it

For developers integrating, syncing, or re-implementing the standard.

- **[Usage scenarios](standard/usage-scenarios.md)** — twelve end-to-end uses (migration, sync, set-building, validation, extending & customizing) with runnable code.
- **[Sync strategy](sync/sync-strategy.md)** — file as source of truth, canonical + projections, conflict resolution; companion: [tool comparison](sync/tool-comparison.md) (interop + file-vs-database storage).
- **[StableHash](standard/stable-hash.md)** — the normative cross-language sync baseline.
- **[Field rationale](standard/field-rationale.md)** — non-normative: where each tag comes from, how it's stored, and how worthwhile it is (origin + demand × worth).
- **[Implementations & writing your own](standard/usage-scenarios.md#implementations-and-writing-your-own)** — SDTS is vendor-neutral; the reference implementation, and how to build a conformant one.

## The definition

- **[Spec](standard/spec.md)** — the formal standard: binding model, variable-tempo notation, the rating-scale family.
- **[`data/fields.yaml`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/data/fields.yaml)** — the machine-readable field registry (the source of truth the field reference is generated from).
- **[`data/vocabulary/`](https://github.com/rokbjc/SocialDanceTaggingStandard/tree/HEAD/data/vocabulary)** — the normative value sets (the vocabulary browser is generated from these).
