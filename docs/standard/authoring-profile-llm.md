# Author a profile from your filenames with an LLM

Many DJs already encode dance, rating, year and more **in their file and folder names**. The optional
[Personal Profile](profile.md)'s `from_path` rules let SDTS *read* that convention as a read-time
projection (the file is never rewritten). Hand-authoring the token dictionary is the tedious part — an
LLM can draft it from a sample of your names.

This is an **authoring aid, not a library feature**: the SDTS library makes no network/LLM calls. You run
an LLM in your own tool; the library only **validates** the result and reports what it could not map.

## The loop

```
your filename/path sample ─▶ [LLM + prompt template] ─▶ draft from_path YAML
                                          │
              ProfileValidator + schemas/profile.schema.json  (validate)
                                          │
              the projection's unmapped-token report  (you resolve, re-project)
```

The LLM's first pass is a **proposal**, never trusted output. Two things close the loop and keep it
honest: the validator rejects codes that don't resolve to a real dimension value, and the parser's
**flag-don't-guess** rule surfaces any trailing token it did not recognise instead of guessing a meaning.

## How to use it

1. List a representative sample of your relative paths (one library "scheme" at a time).
2. Paste them into any LLM together with the prompt template:
   [`prompts/generate-path-profile.md`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/prompts/generate-path-profile.md).
3. Validate the draft against [`schemas/profile.schema.json`](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/schemas/profile.schema.json)
   and the import-time invariants (profile §7).
4. Resolve the `# UNMAPPED:` tokens by hand — those are the ones only you can disambiguate (a personal
   `6K` = "six-count / East Coast Swing", a `hum` modifier, …). Add them to `tail.codes` and re-project.

## Worked example

Given names like `Bob Crosby - Ja-Da BAL LIN ++++++.mp3` under `swing/!root/`, the LLM proposes a rule
whose `tail.codes` maps `LIN → dances: Lindy Hop`, `BAL → dances: Balboa`, reads `++++++` as a rating and a
leading 4-digit run as the year. Projecting `1953 Bob Crosby - Ja-Da BAL LIN ++++++.mp3` then yields
`artist: Bob Crosby · title: Ja-Da · dances: Balboa, Lindy Hop · rating: 6 · recording_year: 1953` — with
nothing written to the file. See [`mapping.from_path`](profile.md#6-profile-yaml-shape-with-the-real-examples).
