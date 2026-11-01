using SocialDanceTaggingStandard.Internal;

namespace SocialDanceTaggingStandard.Sync;

/// <summary>
/// The reusable, container-neutral <see cref="ITagMirror"/> implementation. Given a canonical
/// source, a mirror source, a place to keep the baseline, and a field map, it provides import,
/// export, and the 3-way <see cref="MirrorSyncEngine"/> sync. A concrete binding (e.g. the
/// MediaMonkey bridge) supplies the wiring and owns persistence.
/// </summary>
public sealed class TagMirror : ITagMirror
{
    private readonly ISdtsSource _canonical;
    private readonly ISdtsSource _mirror;
    private readonly ISdtsSource _stateStore;
    private readonly string _stateKey;
    private readonly IReadOnlyList<FieldMirror> _map;
    private readonly MirrorSyncOptions _options;
    private readonly IValueTranslator _translator;
    private readonly MirrorSyncEngine _engine;
    private readonly SyncState _state;

    /// <summary>Wires a mirror over the three sources and a field map.</summary>
    /// <param name="canonical">Canonical SDTS source (e.g. the TXXX:SDTS_* frames).</param>
    /// <param name="mirror">External mirror source (e.g. the COMM:Songs-DB_* frames).</param>
    /// <param name="stateStore">Where the baseline is read from / written to (often the same file).</param>
    /// <param name="stateKey">Key under which the baseline is stored in <paramref name="stateStore"/>.</param>
    /// <param name="map">The structural field correspondences.</param>
    /// <param name="options">Sync policy; defaults to <see cref="MirrorSyncOptions.Default"/>.</param>
    /// <param name="translator">Value translator; defaults to identity (pass-through).</param>
    public TagMirror(
        ISdtsSource canonical,
        ISdtsSource mirror,
        ISdtsSource stateStore,
        string stateKey,
        IReadOnlyList<FieldMirror> map,
        MirrorSyncOptions? options = null,
        IValueTranslator? translator = null)
    {
        _canonical = canonical ?? throw new ArgumentNullException(nameof(canonical));
        _mirror = mirror ?? throw new ArgumentNullException(nameof(mirror));
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _stateKey = stateKey ?? throw new ArgumentNullException(nameof(stateKey));
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _options = options ?? MirrorSyncOptions.Default;
        _translator = translator ?? IdentityValueTranslator.Instance;
        _engine = new MirrorSyncEngine(_translator);
        _state = SyncState.Parse(_stateStore.Get(new TagKey(_stateKey)));
    }

    /// <inheritdoc />
    public SyncReport Sync()
    {
        SyncReport report = _engine.Reconcile(_canonical, _mirror, _state, _map, _options);
        SaveState();
        return report;
    }

    /// <inheritdoc />
    public SyncReport Import()
    {
        var results = new List<FieldSyncResult>();
        foreach (FieldMirror m in _map)
        {
            string? rawMirror = _mirror.Get(m.MirrorKey);
            string? asCanonical = _translator.MirrorToCanonical(m.Field, rawMirror);

            if (_options.Direction is MirrorDirection.None or MirrorDirection.ExportOnly)
            {
                results.Add(new FieldSyncResult(
                    m.Field, FieldSyncOutcome.Skipped, _canonical.Get(m.CanonicalKey), rawMirror, false));
                continue;
            }

            _canonical.Set(m.CanonicalKey, asCanonical);

            // Realign secondary mirror keys to the primary so duplicates never drift.
            string? toMirror = _translator.CanonicalToMirror(m.Field, asCanonical);
            for (int i = 1; i < m.MirrorKeys.Count; i++)
            {
                _mirror.Set(m.MirrorKeys[i], toMirror);
            }

            _state.Set(m.Field, StableHash.Of(asCanonical));
            results.Add(new FieldSyncResult(m.Field, FieldSyncOutcome.Imported, asCanonical, rawMirror, true));
        }

        SaveState();
        return new SyncReport(results);
    }

    /// <inheritdoc />
    public SyncReport Export()
    {
        var results = new List<FieldSyncResult>();
        foreach (FieldMirror m in _map)
        {
            string? canonicalValue = _canonical.Get(m.CanonicalKey);

            if (_options.Direction is MirrorDirection.None or MirrorDirection.ImportOnly)
            {
                results.Add(new FieldSyncResult(
                    m.Field, FieldSyncOutcome.Skipped, canonicalValue, _mirror.Get(m.MirrorKey), false));
                continue;
            }

            string? toMirror = _translator.CanonicalToMirror(m.Field, canonicalValue);
            foreach (var key in m.MirrorKeys)
            {
                _mirror.Set(key, toMirror);
            }
            _state.Set(m.Field, StableHash.Of(canonicalValue));
            results.Add(new FieldSyncResult(m.Field, FieldSyncOutcome.Exported, canonicalValue, toMirror, true));
        }

        SaveState();
        return new SyncReport(results);
    }

    /// <inheritdoc />
    public void WriteCanonical(string field, string? value)
    {
        FieldMirror? m = _map.FirstOrDefault(x => x.Field.Value == field) ?? throw new ArgumentException(
                $"Field '{field}' is not mapped by this mirror; write it directly on the canonical source.",
                nameof(field));

        _canonical.Set(m.CanonicalKey, value);

        bool exporting = _options.Direction is MirrorDirection.Bidirectional or MirrorDirection.ExportOnly;
        if (_options.WriteThrough && exporting)
        {
            string? toMirror = _translator.CanonicalToMirror(new FieldId(field), value);
            foreach (var key in m.MirrorKeys)
            {
                _mirror.Set(key, toMirror);
            }
            _state.Set(new FieldId(field), StableHash.Of(value));
            SaveState();
        }
    }

    /// <inheritdoc />
    public string? GetCanonical(string field)
    {
        FieldMirror? m = _map.FirstOrDefault(x => x.Field.Value == field) ?? throw new ArgumentException(
                $"Field '{field}' is not mapped by this mirror; read it directly on the canonical source.",
                nameof(field));

        return _canonical.Get(m.CanonicalKey);
    }

    /// <inheritdoc />
    public void Flush() => SaveState();

    private void SaveState() => _stateStore.Set(new TagKey(_stateKey), _state.Encode());
}
