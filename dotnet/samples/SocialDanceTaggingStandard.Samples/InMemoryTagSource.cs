using SocialDanceTaggingStandard;
using SocialDanceTaggingStandard.Sync;

namespace SocialDanceTaggingStandard.Samples;

/// <summary>
/// A dictionary-backed <see cref="ISdtsSource"/> so the sync / taxonomy / validation samples run
/// without touching a real MP3. The file-based scenarios use <c>Id3File</c> /
/// <c>MediaMonkeyMirror.Open</c> instead — see each scenario's <c>RunWithFile</c>.
/// Same null/empty == absent semantics as the real ID3 sources.
/// </summary>
internal sealed class InMemoryTagSource : ISdtsSource
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

    public InMemoryTagSource()
    {
    }

    public InMemoryTagSource(IDictionary<string, string?> seed)
    {
        foreach (KeyValuePair<string, string?> kv in seed)
        {
            Set(new TagKey(kv.Key), kv.Value);
        }
    }

    public string? Get(TagKey key) => _values.TryGetValue(key.Value, out string? v) ? v : null;

    public void Set(TagKey key, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            _values.Remove(key.Value);
        }
        else
        {
            _values[key.Value] = value!;
        }
    }
}
