namespace SocialDanceTaggingStandard.Id3;

/// <summary>
/// The file-backed SDTS gateway: an <see cref="SdtsRecord"/> whose backing store is a media
/// <see cref="Container"/> (standard frames + audio + <see cref="Save"/>). The typed SDTS_* accessors
/// come from the base; genre/mood/bpm/rating and persistence come from the container. Non-sealed so a
/// consumer can derive (e.g. a downstream <c>SgdtsFile : SdtsFile</c>).
/// </summary>
public partial class SdtsFile : SdtsRecord, IDisposable
{
    /// <summary>The media container (standard frames + audio + Save). Never null.</summary>
    public ISdtsContainer Container { get; }

    /// <summary>Creates a file-backed gateway over <paramref name="container"/>; the SDTS_* field source
    /// is the container's TXXX region (<see cref="ISdtsContainer.UserText"/>).</summary>
    public SdtsFile(ISdtsContainer container)
        : base((container ?? throw new ArgumentNullException(nameof(container))).UserText)
        => Container = container;

    /// <summary>Opens <paramref name="file"/> as an ID3 file and wires the gateway over its TXXX region.</summary>
    public static SdtsFile OpenId3(FileInfo file) => new(new Id3File(file));

    /// <summary>Opens <paramref name="file"/>, applies <paramref name="mutate"/>, saves and disposes.
    /// Wrap this in a re-read/retry loop for race-safe persistence.</summary>
    public static void Edit(FileInfo file, Action<SdtsFile> mutate)
    {
        using var sdts = OpenId3(file);
        mutate(sdts);
        sdts.Save();
    }

    /// <summary>Persists via <see cref="Container"/>.</summary>
    public void Save() => Container.Save();

    /// <inheritdoc />
    public void Dispose()
    {
        Container.Dispose();
        GC.SuppressFinalize(this);
    }
}
