namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// The public failure contract of the taxonomy loader: a vocabulary YAML document (overlay or
/// baseline) could not be parsed, serialized, or built. Consumers catch THIS instead of the
/// YamlDotNet types they would otherwise have to reference — the loader translates its internal
/// parser exceptions at the module boundary.
/// </summary>
/// <remarks>
/// What it does NOT wrap: file I/O. <see cref="IOException"/>,
/// <see cref="UnauthorizedAccessException"/> and <see cref="FileNotFoundException"/> from opening or
/// replacing a file stay raw, because whether a missing or unreadable profile is fatal is the
/// caller's decision, not the loader's. A caller that wants neither exception uses the
/// <c>TryLoad*</c> overloads, which report both as <c>false</c>.
/// <para>
/// The message names the failing artifact (file path or dimension) but never quotes the document's
/// contents — a personal overlay is user data.
/// </para>
/// </remarks>
public sealed class SdtsVocabularyException : Exception
{
    /// <summary>Creates the exception with no message (prefer an overload that names the artifact).</summary>
    public SdtsVocabularyException()
    {
    }

    /// <summary>Creates the exception with a message naming the failing artifact.</summary>
    public SdtsVocabularyException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception wrapping the parser/build failure that caused it.</summary>
    public SdtsVocabularyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
