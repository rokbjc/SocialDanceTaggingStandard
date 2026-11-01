using SocialDanceTaggingStandard.Id3;
using SocialDanceTaggingStandard.Sync;

namespace SocialDanceTaggingStandard.MediaMonkey;

/// <summary>
/// Bridges the canonical SDTS fields against MediaMonkey's legacy <c>COMM:Songs-DB_*</c> frames on
/// an MP3 file: <see cref="Import"/> (MM5 → canonical), <see cref="Export"/> (canonical → MM5), and
/// the smart 3-way <see cref="Sync"/>. Change detection uses a per-field baseline stored in
/// <c>TXXX:SDTS_MM5_SYNC</c>; conflicts are handled per <see cref="MirrorSyncOptions"/>.
/// <para>
/// Structural pass-through only — values are moved verbatim until a taxonomy-backed
/// <see cref="IValueTranslator"/> is supplied. Mutations are in memory until <see cref="Flush"/>.
/// </para>
/// </summary>
public sealed class MediaMonkeyMirror : ITagMirror, IDisposable
{
    private readonly Id3File _file;
    private readonly bool _ownsFile;
    private readonly TagMirror _inner;

    /// <summary>Bridges an already-open <see cref="Id3File"/>.</summary>
    /// <param name="file">The open ID3 file.</param>
    /// <param name="options">Sync policy; defaults to <see cref="MirrorSyncOptions.Default"/>.</param>
    /// <param name="translator">Value translator; defaults to identity (pass-through, no taxonomy).</param>
    /// <param name="ownsFile">When true, <see cref="Dispose"/> also disposes <paramref name="file"/>.</param>
    /// <param name="additionalFields">
    /// Optional downstream/private field mappings appended to the standard MediaMonkey map (e.g. a
    /// consumer's own <c>SB-*</c> fields). Merged via <see cref="MirrorMap.Compose"/>, which rejects
    /// any colliding field id / canonical key / mirror key.
    /// </param>
    public MediaMonkeyMirror(
        Id3File file,
        MirrorSyncOptions? options = null,
        IValueTranslator? translator = null,
        bool ownsFile = false,
        IReadOnlyList<FieldMirror>? additionalFields = null)
    {
        _file = file ?? throw new ArgumentNullException(nameof(file));
        _ownsFile = ownsFile;

        var canonical = new Id3UserTextSource(file);
        var mirror = new Id3CommentSource(file, MediaMonkeyFrames.Language);

        IReadOnlyList<FieldMirror> map = additionalFields is { Count: > 0 }
            ? MirrorMap.Compose(MediaMonkeyFrames.Map, additionalFields)
            : MediaMonkeyFrames.Map;

        // The baseline lives in a TXXX frame, so the canonical source doubles as the state store.
        _inner = new TagMirror(
            canonical,
            mirror,
            stateStore: canonical,
            stateKey: MediaMonkeyFrames.SyncStateKey,
            map: map,
            options: options,
            translator: translator);
    }

    /// <summary>Opens <paramref name="file"/> and bridges it; the returned mirror owns and saves the file.</summary>
    /// <param name="file">The MP3 file to open.</param>
    /// <param name="options">Sync policy; defaults to <see cref="MirrorSyncOptions.Default"/>.</param>
    /// <param name="translator">Value translator; defaults to identity (pass-through, no taxonomy).</param>
    /// <param name="additionalFields">Optional downstream/private field mappings (see the constructor).</param>
    public static MediaMonkeyMirror Open(
        FileInfo file,
        MirrorSyncOptions? options = null,
        IValueTranslator? translator = null,
        IReadOnlyList<FieldMirror>? additionalFields = null) =>
        new(new Id3File(file), options, translator, ownsFile: true, additionalFields: additionalFields);

    /// <inheritdoc />
    public SyncReport Import() => _inner.Import();

    /// <inheritdoc />
    public SyncReport Export() => _inner.Export();

    /// <inheritdoc />
    public SyncReport Sync() => _inner.Sync();

    /// <inheritdoc />
    public void WriteCanonical(string field, string? value) => _inner.WriteCanonical(field, value);

    /// <inheritdoc />
    public string? GetCanonical(string field) => _inner.GetCanonical(field);

    /// <summary>Writes the baseline back and saves the file to disk.</summary>
    public void Flush()
    {
        _inner.Flush();
        _file.Save();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsFile)
        {
            _file.Dispose();
        }
    }
}
