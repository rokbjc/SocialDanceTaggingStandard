namespace SocialDanceTaggingStandard.TestKit;

/// <summary>
/// Locates repo-relative test resources (the language-neutral <c>conformance/</c> vectors) from a test
/// bin directory — the single home for what used to be copy-pasted into each conformance test.
/// </summary>
public static class ConformancePaths
{
    /// <summary>The repository root: the nearest ancestor of the test bin dir that contains <c>conformance/</c>.</summary>
    public static string Root { get; } = FindRoot();

    /// <summary>Absolute path to <c>conformance/vectors</c> under the repo root.</summary>
    public static string VectorsDir { get; } = Path.Combine(Root, "conformance", "vectors");

    /// <summary>Resolve a repo-relative path, e.g. <c>Resolve("conformance/vectors/hash.json")</c>.</summary>
    public static string Resolve(string relative) =>
        Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar));

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(System.AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "conformance")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new DirectoryNotFoundException("conformance/ not found above " + System.AppContext.BaseDirectory);
    }
}
