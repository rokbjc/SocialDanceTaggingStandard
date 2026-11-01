using SocialDanceTaggingStandard.Sync;

namespace SocialDanceTaggingStandard.TestKit;

/// <summary>An in-memory <see cref="ISdtsSource"/> test double — no file involved.</summary>
public sealed class InMemoryTagSource : ISdtsSource
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

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
