# Generate an SDTS `from_path` profile from your filenames

An authoring aid: paste a sample of your file/folder names into any LLM together with the prompt below.
It returns a draft `mapping.from_path` rule set. **It is a proposal, not trusted output** — validate it
with the SDTS library (`schemas/profile.schema.json` + the import-time invariants), then resolve any
`# UNMAPPED:` tokens by hand. The library itself makes no LLM/network calls; this runs in your tool.

The loop: **LLM draft → validate → unmapped-token report → fix → re-project**. v1 targets the
`mapping.from_path` block only (the hardest part to hand-author — the token dictionary).

---

```
You are helping a social-dance DJ turn their filename/folder tagging convention into an
SDTS Personal Profile `from_path` rule set.

INPUTS
- SDTS dimensions to map values onto: dances, genre, mood, rhythmic_feel, vocality,
  instrumentation, instrument_led, recording_context, recording_quality_era, theme
  (plus standard targets: rating [1-10], recording_year).
- Profile schema: schemas/profile.schema.json — emit schema-valid YAML.
- A sample of the DJ's relative paths/filenames (pasted below).

TASK
1. Infer the stable skeleton mask (e.g. "%artist% - %title% <tail>") and the separator.
2. Identify the trailing token vocabulary (dance codes, modifiers) and the rating/year encodings.
3. Emit a `mapping.from_path` rule set: path-glob `match`, `mask`, and a `tail.codes` dictionary
   mapping each recognized token to (dimension, canonical SDTS value); a plus-run for +-ratings,
   a regex for years.
4. For any token whose meaning is unclear, DO NOT guess — list it under `# UNMAPPED:` for the DJ.

OUTPUT: valid profile YAML (the mapping.from_path block) + the UNMAPPED list. Nothing else.

SAMPLE PATHS:
<paste a folder listing here>
```

---

The `UNMAPPED` tokens are exactly the ones only you can disambiguate (e.g. a personal `6K` = "six-count /
East Coast Swing", or a `hum` modifier). Add them to `tail.codes`, re-validate, and re-project.
