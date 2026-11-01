using SocialDanceTaggingStandard.Samples;

// Runnable companion to docs/standard/usage-scenarios.md. Each scenario is a self-contained
// class; the in-memory demos run as-is, the file-based variants run only if you pass an .mp3 path:
//
//   dotnet run --project dotnet/samples/SocialDanceTaggingStandard.Samples [path\to\track.mp3]
//
// Nothing writes to a file unless you pass one.

string? mp3Arg = args.FirstOrDefault(a => a.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase));
FileInfo? mp3 = mp3Arg is null ? null : new FileInfo(mp3Arg);

static void Header(string title)
{
    Console.WriteLine();
    Console.WriteLine($"=== {title} ===");
}

Header("01  Normalize a crate to SDTS (3-way reconcile + taxonomy canonicalization)");
NormalizeToSdts.Run();

Header("02  First-sync ImportOnly migration off MediaMonkey");
MigrateImportOnly.Run();

Header("03  Set builder: per-dance tempo pools + energy arc");
SetBuilder.Run();

Header("04  Cross-app conflict: ReportOnly surfaces it, PreferCanonical lands it");
CrossAppConflict.Run();

Header("05  Conformance: stable-hash + validator parity (re-implementer)");
Conformance.Run();

Header("06  Pre-gig lint: FieldValidator + alias canonicalization");
PreGigLint.Run();

Header("07  Portable cue points & playback automation");
CuePoints.Run();

Header("08  Audio enrichment + the confirmation-gate trust boundary");
GateTrustBoundary.Run();

Header("09  Cross-source duplicate merge guarded by the gate ledger");
MergeGuardedByGates.Run();

Header("10  How a consumer app consumes SDTS (integration view; wires the shipped translator)");
ConsumerIntegration.Run();

// (Scenario 11 — adding a dance — is a CLI/data flow, no .NET sample; see usage-scenarios.md.)
Header("12  Customize the vocabulary for your deployment (overlay API; baseline immutable)");
CustomizeVocabulary.Run();

if (mp3 is not null)
{
    Header($"file-based variants over {mp3.Name}");
    NormalizeToSdts.RunWithFile(mp3);
    MigrateImportOnly.RunWithFile(mp3);
    CuePoints.RunWithFile(mp3);
    ConsumerIntegration.RunWithFile(mp3);
}
else
{
    Console.WriteLine();
    Console.WriteLine("(pass an .mp3 path to also run the Id3File / MediaMonkeyMirror file-based variants)");
}
