using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>Concurrent <see cref="SocialDanceVocabulary.SaveOverlay"/> writers. Two player instances can
/// share one workspace, so this is reachable in production, not only under a test suite: a shared scratch
/// file let one writer publish another's half-written bytes with no error on either side.</summary>
public sealed class OverlayConcurrentWriteTests : IDisposable
{
    private readonly DirectoryInfo _dir = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), $"sdts-overlay-race-{Guid.NewGuid():N}"));

    public void Dispose()
    {
        try
        {
            _dir.Delete(recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private static VocabularyOverlay OverlayFor(int writer) =>
        new VocabularyOverlay().AddAlias(DimensionIds.Mood, "Joyful", $"Writer{writer}");

    [Fact]
    public void ConcurrentWriters_PublishOneCompleteDocument_NeverASplice()
    {
        var file = new FileInfo(Path.Combine(_dir.FullName, $"race{SdvoFormat.Extension}"));
        const int writers = 4;
        const int rounds = 3;

        for (var round = 0; round < rounds; round++)
        {
            var failures = new System.Collections.Concurrent.ConcurrentBag<Exception>();
            Parallel.For(0, writers, i =>
            {
                try
                {
                    SocialDanceVocabulary.SaveOverlay(OverlayFor(i), file);
                }
                // Only the contention failures are collected and asserted on. Anything else is a bug in
                // the writer, not a race, and should surface as a test error rather than a count.
                catch (Exception ex) when (ex is SdtsVocabularyException or IOException or UnauthorizedAccessException)
                {
                    failures.Add(ex);
                }
            });

            Assert.True(failures.IsEmpty, $"round {round}: {failures.Count} writer(s) threw: "
                                          + string.Join(" | ", failures.Select(e => e.Message)));

            // The survivor must be exactly ONE writer's complete document — not a truncation, and not
            // two writers' bytes spliced together.
            var reloaded = SocialDanceVocabulary.LoadOverlay(file);
            var aliases = reloaded.AddAliases[DimensionIds.Mood]["Joyful"];
            Assert.Single(aliases);
            Assert.Matches(@"^Writer[0-3]$", aliases[0]);
        }
    }

    [Fact]
    public void ConcurrentWriters_LeaveNoScratchFilesBehind()
    {
        var file = new FileInfo(Path.Combine(_dir.FullName, $"leftovers{SdvoFormat.Extension}"));

        Parallel.For(0, 4, i => SocialDanceVocabulary.SaveOverlay(OverlayFor(i), file));

        // Scratch names embed a per-write counter, so a sweep must DISCOVER them by pattern rather than
        // appending a literal ".tmp" — assert the successful path leaves none to discover.
        Assert.Empty(_dir.GetFiles($"{file.Name}.tmp*"));
        Assert.Single(_dir.GetFiles($"*{SdvoFormat.Extension}"));
    }

    [Fact]
    public void ScratchFileIsRemoved_WhenTheWriteFails()
    {
        // A directory where the target name is taken by a DIRECTORY: serialization succeeds, the publish
        // cannot. The scratch must not survive - nothing else knows its name.
        var file = new FileInfo(Path.Combine(_dir.FullName, $"blocked{SdvoFormat.Extension}"));
        Directory.CreateDirectory(file.FullName);

        Assert.ThrowsAny<Exception>(() => SocialDanceVocabulary.SaveOverlay(OverlayFor(0), file));

        Assert.Empty(_dir.GetFiles($"{file.Name}.tmp*"));
    }
}
