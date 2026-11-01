using SocialDanceTaggingStandard.Sync;
using TagLib;
using Id3v2Tag = TagLib.Id3v2.Tag;

namespace SocialDanceTaggingStandard.Id3;

/// <summary>
/// Owns a TagLib# file and its ID3v2 tag — the ID3 implementation of <see cref="ISdtsContainer"/>.
/// Writes ID3v2.4 on save (the SDTS on-disk convention) by setting the version on this file's own tag; the
/// process-wide TagLib defaults (<c>DefaultVersion</c>, <c>ForceDefaultVersion</c>) are never touched, so a
/// host that also writes through TagLib keeps its own version. Mutations are in memory until <see cref="Save"/>.
/// </summary>
public sealed class Id3File : ISdtsContainer
{
    private const byte Id3v24 = 4;

    private readonly TagLib.File _file;
    private readonly FileInfo _fileInfo;
    private Id3UserTextSource? _userText;
    private Id3CommentSource? _comments;

    /// <summary>Opens <paramref name="file"/> for tag read/write and ensures an ID3v2 tag exists.</summary>
    public Id3File(FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        _fileInfo = file;
        _file = TagLib.File.Create(file.FullName);
        Tag = (Id3v2Tag)_file.GetTag(TagTypes.Id3v2, create: true);
    }

    /// <summary>The mutable ID3v2 tag — an advanced escape hatch. The portable, TagLib-free surface is
    /// this container's members (and the <c>SdtsFile</c> gateway); reach for <c>Tag</c> only for frames
    /// SDTS does not model. Requires the consumer to reference TagLibSharp.</summary>
    public Id3v2Tag Tag { get; }

    /// <inheritdoc />
    public ISdtsSource UserText => _userText ??= new Id3UserTextSource(Tag);
    /// <inheritdoc />
    public ISdtsSource Comments => _comments ??= new Id3CommentSource(Tag);

    /// <inheritdoc />
    public string? Title { get => Tag.Title; set => Tag.Title = value; }
    /// <inheritdoc />
    public IReadOnlyList<string> Artists { get => Tag.Performers; set => Tag.Performers = [.. value]; }
    /// <inheritdoc />
    public string? Album { get => Tag.Album; set => Tag.Album = value; }
    /// <inheritdoc />
    public IReadOnlyList<string> AlbumArtists { get => Tag.AlbumArtists; set => Tag.AlbumArtists = [.. value]; }
    /// <inheritdoc />
    public IReadOnlyList<string> Genres { get => [.. Tag.GetGenres()]; set => Tag.SetGenres([.. value]); }
    /// <inheritdoc />
    public IReadOnlyList<string> Moods { get => [.. Tag.GetMoods()]; set => Tag.SetMoods([.. value]); }
    /// <inheritdoc />
    public int? Year { get => Tag.Year == 0 ? null : (int)Tag.Year; set => Tag.Year = value is > 0 ? (uint)value.Value : 0u; }
    /// <inheritdoc />
    public int? Bpm { get => Tag.GetBpm(); set => Tag.SetBpm(value); }
    /// <inheritdoc />
    public string? Grouping { get => Tag.Grouping; set => Tag.Grouping = value; }
    /// <inheritdoc />
    public string? Comment { get => Tag.GetCanonicalComment(); set => Tag.SetCanonicalComment(value ?? ""); }
    /// <inheritdoc />
    public int? Rating { get => Tag.GetRating(); set => Tag.SetRating(value); }

    /// <inheritdoc />
    public IReadOnlyList<InvolvedPerson> InvolvedPeople { get => Tag.GetInvolvedPeople(); set => Tag.SetInvolvedPeople(value); }
    /// <inheritdoc />
    public IReadOnlyList<MusicianCredit> MusicianCredits { get => Tag.GetMusicianCredits(); set => Tag.SetMusicianCredits(value); }

    /// <inheritdoc />
    public FileInfo File => _fileInfo;
    /// <inheritdoc />
    public TimeSpan Duration => _file.Properties?.Duration ?? TimeSpan.Zero;
    /// <inheritdoc />
    public int AudioBitrate => _file.Properties?.AudioBitrate ?? 0;
    /// <inheritdoc />
    public int SampleRate => _file.Properties?.AudioSampleRate ?? 0;

    /// <inheritdoc />
    public void Save()
    {
        Tag.Version = Id3v24;
        _file.Save();
    }
    /// <inheritdoc />
    public void Dispose() => _file.Dispose();
}
