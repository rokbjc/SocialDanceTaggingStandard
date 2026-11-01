# SDTS conformance vectors

Language-neutral fixtures that pin **cross-language** behaviour. Every implementation (today .NET; any
future port) runs the **same** vectors, so a port can't silently diverge. The vectors are
public-domain (CC0, like the rest of the spec).

## Files

| File | What it pins | Consumed by |
|---|---|---|
| `vectors/hash.json` | `StableHash` (the sync baseline) — see [`../docs/standard/stable-hash.md`](../docs/standard/stable-hash.md) | `StableHashTests` (.NET) |
| `vectors/sync/reconcile.json` | the mirror 3-way reconcile + import/export, across direction/conflict/write-through options | `ConformanceVectorsTests` (.NET) |
| `vectors/validation/fields.json` | per-field value validation (int range, enum membership, inline grade, regex) — see [`../docs/standard/field-rationale.md`](../docs/standard/field-rationale.md) | `ValidationConformanceTests` (.NET) |

## hash.json

```jsonc
{ "cases": [ { "input": <string|null>, "hash": "<8 lowercase hex>" } ] }
```
Assert `StableHash(input) == hash`.

## sync/reconcile.json

A single mapped field (`field`, `canonical_key`, `mirror_key` at the top). Each case:

```jsonc
{
  "name": "...", "op": "sync" | "import" | "export",
  "canonical_in": <string|null>,        // initial canonical value (null = absent)
  "mirror_in":    <string|null>,        // initial mirror value
  "baseline_value": <string|null>,      // value both sides had at last sync; runner seeds
                                        //   state = StableHash(baseline_value). null = never synced.
  "options": { "direction": "Bidirectional|ImportOnly|ExportOnly|None",
               "on_conflict": "ReportOnly|PreferCanonical|PreferMirror",
               "write_through": <bool> },
  "expect": { "outcome": "Unchanged|Imported|Exported|Conflict|Skipped",
              "canonical_out": <string|null>, "mirror_out": <string|null>, "resolved": <bool> }
}
```

Runner: build in-memory canonical/mirror sources, seed the baseline, run `op` with `options`, then assert
the field's outcome + both post-values + `resolved`.

## Adding a language

Implement `StableHash` + the reconcile/import/export engine, then load these JSON files in your test
runner and assert. Do **not** hand-maintain per-language copies — there is one set of vectors.
