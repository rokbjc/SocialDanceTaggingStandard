# SocialDanceTaggingStandard — .NET reference implementation

.NET packages (not yet on NuGet; build from source) for the [**Social-Dance Tagging Standard (SDTS)**](https://github.com/rokbjc/SocialDanceTaggingStandard) —
a portable tag standard for social/partner-dance DJ music libraries (swing and blues first, with room for the
wider social-dance world).

Full documentation: **https://rokbjc.github.io/SocialDanceTaggingStandard/**

## Packages

| Package | Role |
|---|---|
| `SocialDanceTaggingStandard` | Format-neutral SDTS model + the `Sync/` 3-way reconcile engine. No taxonomy values, no container deps. |
| `SocialDanceTaggingStandard.Taxonomy` | The normative baseline vocabulary (value sets) + loader + alias/value resolution. Load via `SocialDanceVocabulary.LoadDefault()`. |
| `SocialDanceTaggingStandard.Id3` | ID3v2.4 binding (TagLibSharp): low-level `ISdtsSource` over `TXXX` + `COMM` frames. |
| `SocialDanceTaggingStandard.MediaMonkey` | MediaMonkey 5 bridge: import / export / 3-way sync vs legacy `COMM:Songs-DB_*` frames. |

All target `net8.0`.

## Quick start

```bash
dotnet add package SocialDanceTaggingStandard.Taxonomy
```

```csharp
using SocialDanceTaggingStandard.Taxonomy;

var vocab  = SocialDanceVocabulary.LoadDefault();   // compiled-in baseline vocabulary — no catalog or network
var dances = vocab.Dances("swing");                 // the swing pack's dance dimension
var lindy  = dances.Resolve("#LH");                 // resolve an alias to its canonical term
Console.WriteLine(lindy?.Canonical);                // → "Lindy Hop"
```

For tag read/write and sync against real files, see the
[usage scenarios](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/docs/standard/usage-scenarios.md)
and the runnable `dotnet/samples/SocialDanceTaggingStandard.Samples` project.

## License

Dual-licensed: library code is [**Apache-2.0**](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/LICENSE);
the specification, value registry, schemas, and conformance vectors are public domain under
[**CC0-1.0**](https://github.com/rokbjc/SocialDanceTaggingStandard/blob/HEAD/LICENSE-SPEC).
