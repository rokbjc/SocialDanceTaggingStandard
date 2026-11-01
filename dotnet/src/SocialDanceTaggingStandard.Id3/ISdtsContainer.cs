using SocialDanceTaggingStandard.Sync;

namespace SocialDanceTaggingStandard.Id3;

/// <summary>
/// A media-tag container: the standard frames + audio properties + save, plus access to its raw string
/// regions (TXXX / COMM). The storage axis of the SDTS gateway — ID3 is the first implementation
/// (<see cref="Id3File"/>). Standard frames are the portable subset SDTS needs; ID3-idiosyncratic bits
/// beyond this (raw <c>Tag</c>) stay on the concrete implementation.
/// </summary>
public interface ISdtsContainer : IDisposable
{
    /// <summary>The TXXX-by-description region (home of <c>SDTS_*</c> and private <c>SG_*</c> keys).</summary>
    ISdtsSource UserText { get; }

    /// <summary>The COMM-by-description region (the legacy MediaMonkey mirror keys).</summary>
    ISdtsSource Comments { get; }

    /// <summary>Title (TIT2).</summary>
    string? Title { get; set; }
    /// <summary>Performing artist(s) (TPE1).</summary>
    IReadOnlyList<string> Artists { get; set; }
    /// <summary>Album (TALB).</summary>
    string? Album { get; set; }
    /// <summary>Album artist(s) (TPE2).</summary>
    IReadOnlyList<string> AlbumArtists { get; set; }
    /// <summary>Genre values (TCON, plain text — never ID3v1 numeric codes).</summary>
    IReadOnlyList<string> Genres { get; set; }
    /// <summary>Mood values (TMOO).</summary>
    IReadOnlyList<string> Moods { get; set; }
    /// <summary>Release year (TDRC); null when unset (0).</summary>
    int? Year { get; set; }
    /// <summary>Beats per minute (TBPM); null when unset (0).</summary>
    int? Bpm { get; set; }
    /// <summary>Content grouping (TIT1).</summary>
    string? Grouping { get; set; }
    /// <summary>The canonical user comment (empty-description COMM slot).</summary>
    string? Comment { get; set; }
    /// <summary>Popularity rating as the raw 0–255 POPM byte (owner <c>no@email</c>); null when unset.
    /// The 0–5 half-star mapping is a higher-layer concern and is not applied here.</summary>
    int? Rating { get; set; }

    /// <summary>Involved-people credits (TIPL): production/technical roles → person.</summary>
    IReadOnlyList<InvolvedPerson> InvolvedPeople { get; set; }
    /// <summary>Musician credits (TMCL): instrument → person.</summary>
    IReadOnlyList<MusicianCredit> MusicianCredits { get; set; }

    /// <summary>The file's filesystem identity, or null when the container is not file-backed.</summary>
    FileInfo? File { get; }
    /// <summary>Track duration (<see cref="TimeSpan.Zero"/> if unknown).</summary>
    TimeSpan Duration { get; }
    /// <summary>Audio bitrate in kbps (0 if unknown).</summary>
    int AudioBitrate { get; }
    /// <summary>Audio sample rate in Hz (0 if unknown).</summary>
    int SampleRate { get; }

    /// <summary>Writes pending changes back to the backing store.</summary>
    void Save();
}
