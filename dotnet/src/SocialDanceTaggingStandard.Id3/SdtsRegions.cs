using System.Globalization;

namespace SocialDanceTaggingStandard.Id3;

/// <summary>A typed on-disk region kind for the <c>SDTS_REGIONS</c> frame — a closed set. Wire tokens:
/// <c>silence</c> / <c>speech</c> / <c>music-boundary</c>.</summary>
public enum RegionKind
{
    /// <summary>Leading/trailing/interior silence (a gap with no audible content).</summary>
    Silence,
    /// <summary>Spoken/speech passage (talk, intro chatter, announcement) rather than music.</summary>
    Speech,
    /// <summary>A music-boundary mark (e.g. where the danceable music proper starts/ends).</summary>
    MusicBoundary,
}

/// <summary>One typed region on a track: the half-open span <c>[<see cref="StartMs"/>, <see cref="EndMs"/>)</c>
/// in milliseconds with a <see cref="Kind"/>. Value semantics, so a write→read cycle round-trips by
/// equality.</summary>
public readonly record struct Region(int StartMs, int EndMs, RegionKind Kind);

public partial class SdtsRecord
{
    /// <summary>Manually-marked typed regions (silence / speech / music-boundary), persisted durably in the
    /// <c>SDTS_REGIONS</c> frame so they survive across sessions and machines. Absent frame → empty list.
    /// Wire form: a "; "-joined list of <c>start_ms:end_ms:kind</c> tuples (no JSON, per the SDTS
    /// no-JSON-in-tags rule); malformed or unknown-kind tuples are skipped on read.</summary>
    public IReadOnlyList<Region> Regions
    {
        get
        {
            var list = new List<Region>();
            foreach (var token in GetMulti(SdtsFrames.Regions))
            {
                if (TryParseRegion(token, out var region))
                {
                    list.Add(region);
                }
            }

            return list;
        }
        set
        {
            var parts = new List<string>();
            foreach (var r in value)
            {
                parts.Add($"{r.StartMs.ToString(CultureInfo.InvariantCulture)}:"
                          + $"{r.EndMs.ToString(CultureInfo.InvariantCulture)}:{RegionKindToken(r.Kind)}");
            }

            SetMulti(SdtsFrames.Regions, parts);
        }
    }

    private static bool TryParseRegion(string token, out Region region)
    {
        region = default;
        var parts = token.Split(':');  // start:end:kind (extra trailing fields reserved — ignored for now)
        if (parts.Length < 3
            || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var start)
            || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var end)
            || ParseRegionKind(parts[2]) is not { } kind)
        {
            return false;
        }

        region = new Region(start, end, kind);
        return true;
    }

    /// <summary>The canonical wire token for a <see cref="RegionKind"/>.</summary>
    public static string RegionKindToken(RegionKind kind) => kind switch
    {
        RegionKind.Silence => "silence",
        RegionKind.Speech => "speech",
        RegionKind.MusicBoundary => "music-boundary",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    /// <summary>Parse a wire token to its <see cref="RegionKind"/>, or null if unknown.</summary>
    public static RegionKind? ParseRegionKind(string? token) => token switch
    {
        "silence" => RegionKind.Silence,
        "speech" => RegionKind.Speech,
        "music-boundary" => RegionKind.MusicBoundary,
        _ => null,
    };
}
