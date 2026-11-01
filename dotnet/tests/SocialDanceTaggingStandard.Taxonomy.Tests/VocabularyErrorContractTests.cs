using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>The loader's public failure contract: parse failures arrive as
/// <see cref="SdtsVocabularyException"/> (never a YamlDotNet type a consumer cannot name), file I/O
/// arrives raw, and the Try* overloads report both as absence.</summary>
public sealed class VocabularyErrorContractTests
{
    private const string MalformedYaml = "add_aliases:\n  mood: [Joyful, Sunshiney\n";

    [Fact]
    public void LoadOverlay_MalformedYaml_ThrowsTranslatedException()
    {
        var ex = Assert.Throws<SdtsVocabularyException>(
            () => SocialDanceVocabulary.LoadOverlay(new StringReader(MalformedYaml)));

        Assert.IsAssignableFrom<YamlDotNet.Core.YamlException>(ex.InnerException);
    }

    [Fact]
    public void LoadOverlay_MalformedFile_NamesThePath_ButNotItsContents()
    {
        var file = WriteTemp(MalformedYaml);
        try
        {
            var ex = Assert.Throws<SdtsVocabularyException>(() => SocialDanceVocabulary.LoadOverlay(file));

            Assert.Contains(file.FullName, ex.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("Sunshiney", ex.Message, StringComparison.Ordinal); // user data stays out
        }
        finally
        {
            Delete(file);
        }
    }

    [Fact]
    public void LoadOverlay_MissingFile_LeavesIoErrorRaw()
    {
        // Not translated on purpose: whether an absent profile is fatal is the caller's call.
        var missing = new FileInfo(Path.Combine(Path.GetTempPath(), $"sdts-absent-{Guid.NewGuid():N}.yaml"));

        Assert.Throws<FileNotFoundException>(() => SocialDanceVocabulary.LoadOverlay(missing));
    }

    [Fact]
    public void TryLoadOverlay_AbsentOrMalformed_IsFalse()
    {
        var missing = new FileInfo(Path.Combine(Path.GetTempPath(), $"sdts-absent-{Guid.NewGuid():N}.yaml"));
        Assert.False(SocialDanceVocabulary.TryLoadOverlay(missing, out var none));
        Assert.Null(none);

        var file = WriteTemp(MalformedYaml);
        try
        {
            Assert.False(SocialDanceVocabulary.TryLoadOverlay(file, out var broken));
            Assert.Null(broken);
        }
        finally
        {
            Delete(file);
        }
    }

    [Fact]
    public void TryLoadDefault_UsableOverlay_IsAppliedToTheBaseline()
    {
        var file = WriteTemp("add_aliases:\n  mood:\n    Joyful: [Sunshiney]\n");
        try
        {
            Assert.True(SocialDanceVocabulary.TryLoadDefault(file, out var vocabulary));
            Assert.Equal("Joyful", vocabulary.Dimension(DimensionIds.Mood)!.Resolve("Sunshiney")!.Canonical);
        }
        finally
        {
            Delete(file);
        }
    }

    [Fact]
    public void TryLoadDefault_CollidingOverlay_IsFalse()
    {
        // "Swing" is another genre's canonical, so this alias could never have resolved. Building
        // the dimension rejects it — and an unusable overlay is "no overlay" here, not an exception.
        var file = WriteTemp(
            """
            add_aliases:
              genre:
                Blues: [Swing]
            """);
        try
        {
            Assert.False(SocialDanceVocabulary.TryLoadDefault(file, out var vocabulary));
            Assert.Null(vocabulary);
        }
        finally
        {
            Delete(file);
        }
    }

    [Fact]
    public void TryLoadDefault_AbsentOverlay_IsFalse()
    {
        var missing = new FileInfo(Path.Combine(Path.GetTempPath(), $"sdts-absent-{Guid.NewGuid():N}.yaml"));

        Assert.False(SocialDanceVocabulary.TryLoadDefault(missing, out var vocabulary));
        Assert.Null(vocabulary);
    }

    private static FileInfo WriteTemp(string yaml)
    {
        var file = new FileInfo(Path.Combine(Path.GetTempPath(), $"sdts-overlay-{Guid.NewGuid():N}.yaml"));
        File.WriteAllText(file.FullName, yaml);
        file.Refresh();
        return file;
    }

    private static void Delete(FileInfo file)
    {
        file.Refresh();
        if (file.Exists) file.Delete();
    }
}
