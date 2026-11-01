using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using SocialDanceTaggingStandard.Taxonomy.Ingest;
using SocialDanceTaggingStandard.Taxonomy.Profile;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// The SDTS baseline vocabulary: the shared music <see cref="Core"/> dimensions plus the
/// dance <see cref="Packs"/>. Hand-maintained in <c>data/vocabulary/</c> and compiled in via the
/// generated <c>BaselineVocabulary</c> (by <c>scripts/gen_vocab_docs.py</c>); load it with <see cref="LoadDefault()"/>.
/// </summary>
/// <remarks>
/// The fault line: a value that describes the MUSIC (genre, mood, feel, …) lives once in
/// <see cref="Core"/> and is shared by every pack; a value that names a DANCE lives in a
/// per-family <see cref="DancePack"/>. See <c>docs/standard/</c>.
/// </remarks>
public sealed class SocialDanceVocabulary
{
    /// <summary>Creates a vocabulary from its core dimensions and dance packs.</summary>
    public SocialDanceVocabulary(
        string version,
        IReadOnlyDictionary<DimensionId, Dimension> core,
        IReadOnlyDictionary<PackId, DancePack> packs)
    {
        Version = version;
        Core = core;
        Packs = packs;
    }

    /// <summary>The combined <c>dances</c> dimension's id (a virtual, cross-pack dimension).</summary>
    public static readonly DimensionId DancesDimension = new("dances");

    /// <summary>The vocabulary version (e.g. <c>"0.1.0-draft"</c>) from the manifest.</summary>
    public string Version { get; }

    /// <summary>Shared music dimensions keyed by id (<c>genre</c>, <c>mood</c>, <c>rhythmic_feel</c>, …).</summary>
    public IReadOnlyDictionary<DimensionId, Dimension> Core { get; }

    /// <summary>Dance packs keyed by id (<c>swing</c>, <c>blues</c>, …).</summary>
    public IReadOnlyDictionary<PackId, DancePack> Packs { get; }

    /// <summary>A core dimension by id, or null if absent.</summary>
    public Dimension? Dimension(DimensionId id)
        => Core.GetValueOrDefault(id);

    /// <summary>True if <paramref name="value"/> is a member of the given core dimension
    /// (alias-aware, ignoring dance grade/<c>?</c> markers). False for an unknown dimension.</summary>
    public bool IsValid(DimensionId dimension, string value)
        => Dimension(dimension)?.Contains(value) ?? false;

    /// <summary>Normalize a multi-value core field (alias→canonical, dedup, hierarchy dedup);
    /// empty for an unknown dimension. See <see cref="Taxonomy.Dimension.Normalize"/>.</summary>
    public IReadOnlyList<VocabularyTerm> Normalize(DimensionId dimension, IEnumerable<string> values)
        => Dimension(dimension)?.Normalize(values) ?? [];

    /// <summary>A combined <c>dances</c> dimension over the named packs, so dance values can be
    /// resolved / validated / normalized the same way as a core dimension —
    /// e.g. <c>Dances(PackIds.Swing, PackIds.Blues).Normalize(track.Dances)</c>.</summary>
    public Dimension Dances(params PackId[] packNames)
        => new Dimension(DancesDimension, "pack", EnabledDances(packNames), grouping: EnabledDanceGrouping(packNames));

    // ── Per-developer customization (overlay) ────────────────────────────────────

    /// <summary>Load the baseline and apply a YAML overlay file (a <c>.sdvo.yaml</c>; see
    /// <see cref="SdvoFormat"/>).</summary>
    /// <exception cref="SdtsVocabularyException">The overlay is not parseable YAML, or one of its
    /// tokens collides with a baseline term (see <see cref="Dimension"/>).</exception>
    public static SocialDanceVocabulary LoadDefault(FileInfo overlayFile)
        => overlayFile is null ? LoadDefault() : LoadDefault().With(LoadOverlay(overlayFile));

    /// <summary>Parse a customization overlay from a YAML file.</summary>
    /// <exception cref="SdtsVocabularyException">The file is not parseable YAML.</exception>
    /// <exception cref="IOException">The file could not be read — deliberately NOT translated;
    /// see <see cref="TryLoadOverlay(FileInfo, out VocabularyOverlay)"/> to treat that as absence.</exception>
    public static VocabularyOverlay LoadOverlay(FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);
        using var reader = file.OpenText();
        return ReadOverlay(reader, file.FullName);
    }

    /// <summary>Parse a customization overlay from a YAML reader.</summary>
    /// <exception cref="SdtsVocabularyException">The document is not parseable YAML.</exception>
    public static VocabularyOverlay LoadOverlay(TextReader reader)
        => ReadOverlay(reader, "the overlay reader");

    /// <summary>Parse an overlay file, reporting an absent, unreadable, or malformed file as
    /// <c>false</c> — for the "apply the personal profile if there is one" call site, which has no
    /// use for the distinction and would otherwise carry a catch.</summary>
    /// <remarks>Unlike <see cref="LoadOverlay(FileInfo)"/> this also absorbs file I/O errors: from
    /// here "no readable overlay" and "no overlay" are the same outcome.</remarks>
    public static bool TryLoadOverlay(FileInfo file, [NotNullWhen(true)] out VocabularyOverlay? overlay)
    {
        overlay = null;
        if (file is null || !file.Exists)
        {
            return false;
        }

        try
        {
            using var reader = file.OpenText();
            overlay = ReadOverlay(reader, file.FullName);
            return true;
        }
        catch (SdtsVocabularyException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>Parse an overlay from a reader, reporting a malformed document as <c>false</c>.</summary>
    public static bool TryLoadOverlay(TextReader reader, [NotNullWhen(true)] out VocabularyOverlay? overlay)
    {
        overlay = null;
        if (reader is null)
        {
            return false;
        }

        try
        {
            overlay = ReadOverlay(reader, "the overlay reader");
            return true;
        }
        catch (SdtsVocabularyException)
        {
            return false;
        }
    }

    /// <summary>Load the baseline and apply <paramref name="overlayFile"/> if it is present and usable;
    /// <c>false</c> leaves the caller to fall back to the bare <see cref="LoadDefault()"/> baseline.
    /// Unusable covers a colliding overlay as well as an unparseable one — from here both are "no
    /// overlay".</summary>
    public static bool TryLoadDefault(FileInfo overlayFile, [NotNullWhen(true)] out SocialDanceVocabulary? vocabulary)
    {
        vocabulary = null;
        if (!TryLoadOverlay(overlayFile, out var overlay))
        {
            return false;
        }
        try
        {
            vocabulary = LoadDefault().With(overlay);
        }
        catch (SdtsVocabularyException)
        {
            return false;
        }
        return true;
    }

    // The one parse boundary. YamlDotNet's exception types are this package's private business — a
    // consumer cannot name them without taking the dependency — so they are translated here. File I/O
    // is NOT caught: the caller opened the stream and owns that failure.
    private static VocabularyOverlay ReadOverlay(TextReader reader, string source)
    {
        ArgumentNullException.ThrowIfNull(reader);
        Dictionary<string, object?> doc;
        try
        {
            doc = AsMap(Yaml.Deserialize<object>(reader));
        }
        catch (YamlException ex)
        {
            throw new SdtsVocabularyException($"Could not parse the vocabulary overlay from {source}.", ex);
        }
        return ParseOverlay(Migrate(doc, source));
    }

    /// <summary>Bring an overlay document forward to <see cref="SdvoFormat.CurrentVersion"/>, or refuse it.
    /// A missing <c>schemaVersion</c> is the unversioned legacy shape written before the format existed;
    /// v0 → v1 added the version field and the modeline, so no key moves and the migration is a relabel.
    /// A version this loader does not know is REJECTED rather than partially read — reading a newer file
    /// with older rules is the failure mode the version exists to prevent.</summary>
    private static Dictionary<string, object?> Migrate(Dictionary<string, object?> doc, string source)
    {
        var raw = Value(doc, "schemaVersion");
        var version = SdvoFormat.LegacyVersion;
        if (raw is not null)
        {
            var token = Convert.ToString(raw, CultureInfo.InvariantCulture);
            if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out version))
                throw new SdtsVocabularyException(
                    $"The vocabulary overlay from {source} has a non-numeric schemaVersion '{token}'.");
        }
        if (version > SdvoFormat.CurrentVersion)
            throw new SdtsVocabularyException(
                $"The vocabulary overlay from {source} is schemaVersion {version}, but this library reads at "
                + $"most {SdvoFormat.CurrentVersion}. Upgrade SocialDanceTaggingStandard.Taxonomy rather than reading it with older rules.");
        if (version < SdvoFormat.LegacyVersion)
            throw new SdtsVocabularyException(
                $"The vocabulary overlay from {source} has a negative schemaVersion {version}.");
        // v0 → v1: shape-identical, so nothing to rewrite. A future vN → vN+1 step goes here.
        return doc;
    }

    /// <summary>Serialize a customization overlay to the <c>.sdvo.yaml</c> shape that
    /// <see cref="LoadOverlay(System.IO.TextReader)"/> reads — the WRITE counterpart, so a personal
    /// profile's custom values and aliases can be persisted and reloaded. Round-trips:
    /// <c>LoadOverlay</c> of the written text reconstructs <paramref name="overlay"/>. To register a
    /// custom value, add it (<see cref="VocabularyOverlay.AddTerm"/>) then save.</summary>
    /// <exception cref="SdtsVocabularyException">The overlay could not be serialized to YAML.</exception>
    public static void SaveOverlay(VocabularyOverlay overlay, TextWriter writer)
        => WriteOverlay(overlay, writer, "the overlay writer");

    /// <summary>Persist a customization overlay to a <c>.sdvo.yaml</c> file — the WRITE
    /// counterpart to <see cref="LoadOverlay(System.IO.FileInfo)"/>.</summary>
    /// <remarks>
    /// Each write goes to its OWN scratch file and is published by an atomic replace, so a crash
    /// mid-write cannot corrupt an existing profile AND two writers of the same overlay cannot splice
    /// each other's bytes. The published document is always exactly one writer's complete output; which
    /// writer wins under contention is last-commit-wins, not defined here.
    /// <para>
    /// A scratch file left behind by a killed process is inert — nothing will ever commit it — but it is
    /// discoverable as <c>&lt;name&gt;.tmp*</c>. A sweep that carries sidecars must MATCH that pattern
    /// rather than appending a literal <c>".tmp"</c>, because the name embeds a per-write counter.
    /// </para>
    /// </remarks>
    /// <exception cref="SdtsVocabularyException">The overlay could not be serialized, or the write could
    /// not be published because another writer held the target for the whole retry budget.</exception>
    public static void SaveOverlay(VocabularyOverlay overlay, FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(overlay);
        ArgumentNullException.ThrowIfNull(file);
        SaveMap(OverlayToMap(overlay), file);
    }

    // ── Personal profile (.sdvo) : the overlay PLUS the read-time rule sections ──────────────────────

    /// <summary>Load a full <see cref="PersonalProfile"/> from a <c>.sdvo.yaml</c> file — the vocabulary
    /// overlay AND the cross-dimension / colour / from-path rule sections. Older overlay-only files load
    /// as a profile whose rule sections are empty; the write counterpart is <see cref="SaveProfile(PersonalProfile, FileInfo)"/>.</summary>
    /// <exception cref="SdtsVocabularyException">The file could not be read or is a newer schemaVersion.</exception>
    public static PersonalProfile LoadProfile(FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);
        using var reader = file.OpenText();
        return ReadProfile(reader, file.FullName);
    }

    /// <summary>Load a full <see cref="PersonalProfile"/> from an already-open reader.</summary>
    public static PersonalProfile LoadProfile(TextReader reader) => ReadProfile(reader, "the profile reader");

    /// <summary>Try to load a profile, treating a missing or unreadable file as absence (returns
    /// <see langword="false"/>); a present-but-malformed or too-new file still throws.</summary>
    public static bool TryLoadProfile(FileInfo file, [NotNullWhen(true)] out PersonalProfile? profile)
    {
        ArgumentNullException.ThrowIfNull(file);
        StreamReader reader;
        try
        {
            reader = file.OpenText();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            profile = null;
            return false;
        }
        using (reader)
        {
            profile = ReadProfile(reader, file.FullName);
            return true;
        }
    }

    /// <summary>Serialize a <see cref="PersonalProfile"/> to the <c>.sdvo.yaml</c> shape
    /// <see cref="LoadProfile(System.IO.TextReader)"/> reads. Round-trips: reloading reconstructs the
    /// overlay and every rule. Gate-token aliases are not persisted (they are a read-time gate concern).</summary>
    /// <exception cref="SdtsVocabularyException">The profile could not be serialized.</exception>
    public static void SaveProfile(PersonalProfile profile, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(profile);
        WriteDocument(ProfileToMap(profile), writer, "the profile writer");
    }

    /// <summary>Persist a <see cref="PersonalProfile"/> to a <c>.sdvo.yaml</c> file (atomic replace, same
    /// scratch discipline as <see cref="SaveOverlay(VocabularyOverlay, FileInfo)"/>).</summary>
    /// <exception cref="SdtsVocabularyException">The profile could not be serialized or published.</exception>
    public static void SaveProfile(PersonalProfile profile, FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(file);
        SaveMap(ProfileToMap(profile), file);
    }

    private static PersonalProfile ReadProfile(TextReader reader, string source)
    {
        ArgumentNullException.ThrowIfNull(reader);
        Dictionary<string, object?> doc;
        try
        {
            doc = AsMap(Yaml.Deserialize<object>(reader));
        }
        catch (YamlException ex)
        {
            throw new SdtsVocabularyException($"Could not parse the vocabulary overlay from {source}.", ex);
        }
        return ParseProfile(Migrate(doc, source));
    }

    private static int _scratchCounter;

    // A concurrent replace of the same target is contention, not failure — retake it briefly before
    // conceding. The budget is deliberately small: a writer that cannot publish in ~45 ms is contending
    // with something structural, and a personal profile save must not hang a UI thread.
    private static void Publish(FileInfo scratch, FileInfo target)
    {
        const int attempts = 3;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                File.Move(scratch.FullName, target.FullName, overwrite: true);
                return;
            }
            catch (Exception ex) when (attempt < attempts && ex is IOException or UnauthorizedAccessException)
            {
                Thread.Sleep(15 * attempt);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new SdtsVocabularyException(
                    $"Could not publish the vocabulary overlay to {target.FullName}: another writer held it "
                    + $"for all {attempts} attempts.", ex);
            }
        }
    }

    private static void TryDeleteScratch(FileInfo scratch)
    {
        try
        {
            scratch.Refresh();
            if (scratch.Exists)
            {
                scratch.Delete();
            }
        }
        catch (IOException)
        {
            // Best effort — an undeletable scratch is inert, and the original failure is what matters.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static void WriteOverlay(VocabularyOverlay overlay, TextWriter writer, string target)
    {
        ArgumentNullException.ThrowIfNull(overlay);
        WriteDocument(OverlayToMap(overlay), writer, target);
    }

    // Serialization and the write are split so only the FORMER is translated: an IOException from the
    // stream is the caller's domain (same rule as the read side in ReadOverlay).
    private static void WriteDocument(Dictionary<string, object?> doc, TextWriter writer, string target)
    {
        ArgumentNullException.ThrowIfNull(writer);
        string text;
        try
        {
            // Line 1 is the editor modeline, so a saved file validates and autocompletes the moment the
            // DJ opens it — the file describes its own contract (persisted-file-formats.md).
            text = $"# yaml-language-server: $schema={SdvoFormat.SchemaId}\n"
                   + YamlOut.Serialize(doc);
        }
        catch (YamlException ex)
        {
            throw new SdtsVocabularyException($"Could not serialize the vocabulary overlay to {target}.", ex);
        }
        writer.Write(text);
    }

    // Atomic publish of a ready-built document map: write to a per-writer scratch file, then replace the
    // target. A crash mid-write cannot corrupt an existing file, and — because the scratch name embeds the
    // process id and a per-write counter — two writers of the same target cannot splice each other's bytes.
    // A scratch left by a killed process is inert (nothing commits it) but discoverable as <name>.tmp*.
    private static void SaveMap(Dictionary<string, object?> doc, FileInfo file)
    {
        file.Directory?.Create();
        var tmp = new FileInfo(
            $"{file.FullName}.tmp{Environment.ProcessId}-{Interlocked.Increment(ref _scratchCounter)}");
        try
        {
            using (var writer = tmp.CreateText())
                WriteDocument(doc, writer, file.FullName);
            Publish(tmp, file);
        }
        catch
        {
            // Nothing else will: the name cannot be reconstructed by a later sweep.
            TryDeleteScratch(tmp);
            throw;
        }
    }

    /// <summary>Return a NEW vocabulary with the overlay applied; the baseline is never mutated.
    /// Core ops apply to the named core dimension; a <c>"dances"</c> key applies to every pack
    /// (alias add/remove and term removal — add a custom pack for new dances).</summary>
    /// <exception cref="SdtsVocabularyException">An added alias or term collides with an existing one
    /// (see <see cref="Dimension"/>), so it could never have resolved.</exception>
    public SocialDanceVocabulary With(VocabularyOverlay overlay)
    {
        ArgumentNullException.ThrowIfNull(overlay);
        var core = new Dictionary<DimensionId, Dimension>();
        foreach (var kv in Core)
            core[kv.Key] = ApplyToDimension(kv.Value, kv.Key, overlay, allowAddTerms: true);
        var packs = new Dictionary<PackId, DancePack>();
        foreach (var kv in Packs)
        {
            var p = kv.Value;
            packs[kv.Key] = new DancePack(p.Name, p.DisplayName, p.Description, p.Families,
                ApplyToDimension(p.Dances, DancesDimension, overlay, allowAddTerms: false), p.ReviewStatus);
        }
        return new SocialDanceVocabulary(Version, core, packs);
    }

    /// <summary>Begin a fluent per-developer customization (see <see cref="VocabularyCustomizer"/>).</summary>
    public VocabularyCustomizer Customize() => new VocabularyCustomizer(this);

    private static VocabularyOverlay ParseOverlay(Dictionary<string, object?> doc)
    {
        var o = new VocabularyOverlay();
        foreach (var dim in AsMap(Value(doc, "add_aliases")))
        {
            foreach (var term in AsMap(dim.Value))
            {
                foreach (var a in AsStringList(term.Value))
                {
                    o.AddAlias(new DimensionId(dim.Key), term.Key, a);
                }
            }
        }
        foreach (var dim in AsMap(Value(doc, "remove_aliases")))
        {
            foreach (var a in AsStringList(dim.Value))
            {
                o.RemoveAlias(new DimensionId(dim.Key), a);
            }
        }
        foreach (var dim in AsMap(Value(doc, "remove_terms")))
        {
            foreach (var k in AsStringList(dim.Value))
            {
                o.RemoveTerm(new DimensionId(dim.Key), k);
            }
        }
        foreach (var dim in AsMap(Value(doc, "add_terms")))
        {
            if (dim.Value is IEnumerable<object> arr)
            {
                foreach (var tm in arr)
                {
                    o.AddTerm(new DimensionId(dim.Key), TermFromMap(AsMap(tm)));
                }
            }
        }
        return o;
    }

    // The inverse of ParseOverlay: project a VocabularyOverlay back to the add_aliases / remove_aliases /
    // add_terms / remove_terms map shape LoadOverlay reads. Empty sections are omitted.
    private static Dictionary<string, object?> OverlayToMap(VocabularyOverlay o)
    {
        // schemaVersion goes in FIRST so it is the first key in the emitted document, right under the
        // modeline — a reader (human or tool) sees the contract before the content.
        var doc = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["schemaVersion"] = SdvoFormat.CurrentVersion,
        };
        if (o.AddAliases.Count > 0)
        {
            var byDim = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var (dim, perTerm) in o.AddAliases)
            {
                var terms = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var (term, aliases) in perTerm)
                    terms[term] = new List<string>(aliases);
                byDim[dim.Value] = terms;
            }
            doc["add_aliases"] = byDim;
        }
        if (o.RemoveAliases.Count > 0)
            doc["remove_aliases"] = DimListMap(o.RemoveAliases);
        if (o.AddTerms.Count > 0)
        {
            var byDim = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var (dim, termList) in o.AddTerms)
            {
                var list = new List<object?>();
                foreach (var t in termList)
                    list.Add(TermToMap(t));
                byDim[dim.Value] = list;
            }
            doc["add_terms"] = byDim;
        }
        if (o.RemoveTerms.Count > 0)
            doc["remove_terms"] = DimListMap(o.RemoveTerms);
        return doc;
    }

    // A profile is its overlay (schemaVersion + the vocabulary sections) plus the read-time rule sections.
    // The rule keys go AFTER the overlay so a diff of an overlay-only file is unchanged when rules are added.
    private static Dictionary<string, object?> ProfileToMap(PersonalProfile profile)
    {
        var doc = OverlayToMap(profile.Overlay);
        if (!string.IsNullOrEmpty(profile.Owner))
        {
            doc["owner"] = profile.Owner;
        }

        if (profile.CrossDimension.Count > 0)
        {
            doc["cross_dimension"] = profile.CrossDimension.Select(CrossToMap).ToList();
        }

        if (profile.Colors.Count > 0)
        {
            doc["colors"] = profile.Colors.Select(ColorToMap).ToList();
        }

        if (profile.FromPath.Count > 0)
        {
            doc["from_path"] = profile.FromPath.Select(PathRuleToMap).ToList();
        }

        return doc;
    }

    private static PersonalProfile ParseProfile(Dictionary<string, object?> doc)
    {
        var overlay = ParseOverlay(doc);
        var colors = ParseList(doc, "colors", ColorFromMap);
        var cross = ParseList(doc, "cross_dimension", CrossFromMap);
        var fromPath = ParseList(doc, "from_path", PathRuleFromMap);
        return new PersonalProfile(overlay, colors, cross, GetStringOrNull(doc, "owner"), fromPath);
    }

    private static List<T> ParseList<T>(Dictionary<string, object?> doc, string key, Func<Dictionary<string, object?>, T> map)
    {
        var list = new List<T>();
        if (Value(doc, key) is IEnumerable<object> items)
        {
            foreach (var raw in items)
            {
                list.Add(map(AsMap(raw)));
            }
        }

        return list;
    }

    private static Dictionary<string, object?> CrossToMap(CrossDimensionRule r)
    {
        // Emit only the non-default flags so the common rule stays a four-key map and diffs stay small.
        var m = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["from_dimension"] = r.FromDimension,
            ["from_value"] = r.FromValue,
            ["to_dimension"] = r.ToDimension,
            ["to_value"] = r.ToValue,
        };
        if (!r.KeepSource)
        {
            m["keep_source"] = false;
        }

        if (r.Migrate)
        {
            m["migrate"] = true;
        }

        if (r.Match != MatchMode.CaseInsensitive)
        {
            m["match"] = r.Match.ToString();
        }

        return m;
    }

    private static CrossDimensionRule CrossFromMap(Dictionary<string, object?> m) => new(
        GetString(m, "from_dimension"),
        GetString(m, "from_value"),
        GetString(m, "to_dimension"),
        GetString(m, "to_value"),
        GetBool(m, "keep_source", fallback: true),
        GetBool(m, "migrate", fallback: false),
        GetEnum(m, "match", MatchMode.CaseInsensitive));

    private static Dictionary<string, object?> ColorToMap(ColorOverride c)
    {
        var m = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["dimension"] = c.Dimension,
            ["value"] = c.Value,
        };
        if (!string.IsNullOrEmpty(c.Color))
        {
            m["color"] = c.Color;
        }

        if (!string.IsNullOrEmpty(c.Label))
        {
            m["label"] = c.Label;
        }

        return m;
    }

    private static ColorOverride ColorFromMap(Dictionary<string, object?> m) =>
        new(GetString(m, "dimension"), GetString(m, "value"), GetStringOrNull(m, "color"), GetStringOrNull(m, "label"));

    private static Dictionary<string, object?> PathRuleToMap(PathRule r)
    {
        var m = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["match"] = r.Match,
            ["mask"] = r.Mask,
        };
        if (r.Codes.Count > 0)
        {
            m["codes"] = r.Codes.Select(c => (object?)new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["token"] = c.Token,
                ["to_dimension"] = c.ToDimension,
                ["to_value"] = c.ToValue,
            }).ToList();
        }

        if (r.Rating is { } rating)
        {
            m["rating"] = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["to"] = rating.To,
                ["scale"] = rating.Scale.ToString(),
                ["max"] = rating.Max,
            };
        }

        if (r.Year is { } year)
        {
            m["year"] = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["to"] = year.To,
                ["pattern"] = year.Pattern,
            };
        }

        if (r.OnUnknownToken != UnknownTokenPolicy.Flag)
        {
            m["on_unknown_token"] = r.OnUnknownToken.ToString();
        }

        if (r.Set.Count > 0)
        {
            var set = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var kv in r.Set)
            {
                set[kv.Key] = kv.Value;
            }

            m["set"] = set;
        }

        return m;
    }

    private static PathRule PathRuleFromMap(Dictionary<string, object?> m)
    {
        var codes = new List<PathCode>();
        if (Value(m, "codes") is IEnumerable<object> rawCodes)
        {
            foreach (var raw in rawCodes)
            {
                var cm = AsMap(raw);
                codes.Add(new PathCode(GetString(cm, "token"), GetString(cm, "to_dimension"), GetString(cm, "to_value")));
            }
        }

        PlusRunRating? rating = null;
        if (Value(m, "rating") is { } rawRating)
        {
            var rm = AsMap(rawRating);
            rating = new PlusRunRating(GetStringOr(rm, "to", "rating"), GetEnum(rm, "scale", RatingScale.Clamp10), GetInt(rm, "max", 11));
        }

        YearExtractor? year = null;
        if (Value(m, "year") is { } rawYear)
        {
            var ym = AsMap(rawYear);
            year = new YearExtractor(GetStringOr(ym, "to", "recording_year"), GetStringOr(ym, "pattern", @"\b(19|20)\d{2}\b"));
        }

        var set = new Dictionary<string, string>(StringComparer.Ordinal);
        if (Value(m, "set") is { } rawSet)
        {
            foreach (var kv in AsMap(rawSet))
            {
                set[kv.Key] = Convert.ToString(kv.Value, CultureInfo.InvariantCulture) ?? string.Empty;
            }
        }

        return new PathRule(
            GetString(m, "match"),
            GetString(m, "mask"),
            codes,
            rating,
            year,
            GetEnum(m, "on_unknown_token", UnknownTokenPolicy.Flag))
        {
            Set = set,
        };
    }

    private static Dictionary<string, object?> DimListMap(Dictionary<DimensionId, List<string>> src)
    {
        var m = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var (dim, list) in src)
            m[dim.Value] = new List<string>(list);
        return m;
    }

    private static Dictionary<string, object?> TermToMap(VocabularyTerm t)
    {
        var m = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["id"] = t.Id,
            ["canonical"] = t.Canonical,
        };
        if (t.Aliases.Count > 0) m["aliases"] = new List<string>(t.Aliases);
        if (t.Deprecated) m["deprecated"] = true;
        foreach (var kv in t.Attributes)  // pass through any authored attributes (empty for a bare custom value)
            m[kv.Key] = kv.Value;
        return m;
    }

    private static Dimension ApplyToDimension(Dimension dim, DimensionId key, VocabularyOverlay overlay, bool allowAddTerms)
    {
        overlay.RemoveTerms.TryGetValue(key, out var removeTerms);
        overlay.RemoveAliases.TryGetValue(key, out var removeAliases);
        overlay.AddAliases.TryGetValue(key, out var addAliases);
        overlay.AddTerms.TryGetValue(key, out var addTerms);
        var hasAddTerms = allowAddTerms && addTerms is { Count: > 0 };
        var aliasOps = removeAliases is { Count: > 0 }
                       || addAliases is { Count: > 0 };
        var hasRemoveTerms = removeTerms is { Count: > 0 };
        if (!aliasOps && !hasAddTerms && !hasRemoveTerms)
            return dim;

        var removeAliasSet = removeAliases is null ? null
            : new HashSet<string>(removeAliases, StringComparer.OrdinalIgnoreCase);
        var removeTermSet = removeTerms is null ? null
            : new HashSet<string>(removeTerms, StringComparer.OrdinalIgnoreCase);

        var terms = new List<VocabularyTerm>();
        foreach (var t in dim.Terms)
        {
            if (removeTermSet != null && (removeTermSet.Contains(t.Id) || removeTermSet.Contains(t.Canonical)))
                continue;
            if (!aliasOps)
            {
                terms.Add(t);
                continue;
            }
            var aliases = new List<string>(t.Aliases);
            if (removeAliasSet != null) aliases.RemoveAll(a => removeAliasSet.Contains(a));
            if (addAliases != null)
            {
                foreach (var pair in addAliases)
                {
                    if (TermMatches(t, pair.Key))
                    {
                        foreach (var a in pair.Value)
                        {
                            if (!aliases.Contains(a, StringComparer.OrdinalIgnoreCase))
                            {
                                aliases.Add(a);
                            }
                        }
                    }
                }
            }
            terms.Add(t.WithAliases(aliases));
        }
        if (hasAddTerms)
            terms.AddRange(addTerms!);
        return new Dimension(dim.Name, dim.Layer, terms, dim.AttributeReview, dim.Grouping);
    }

    private static bool TermMatches(VocabularyTerm t, string key)
        => string.Equals(key, t.Id, StringComparison.OrdinalIgnoreCase)
           || string.Equals(key, t.Canonical, StringComparison.OrdinalIgnoreCase)
           || t.Aliases.Any(a => string.Equals(a, key, StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyList<string> AsStringList(object? value)
        => value is IEnumerable<object> items
            ? items.Select(i => Convert.ToString(i, CultureInfo.InvariantCulture) ?? string.Empty).ToList()
            : Array.Empty<string>();

    /// <summary>
    /// All dance terms contributed by the named packs, deduped by id and in pack/catalog order —
    /// e.g. <c>EnabledDances("swing", "blues")</c> for a swing+blues library.
    /// </summary>
    public IReadOnlyList<VocabularyTerm> EnabledDances(params PackId[] packNames)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var list = new List<VocabularyTerm>();
        foreach (var name in packNames)
        {
            if (!Packs.TryGetValue(name, out var pack)) continue;
            foreach (var term in pack.Dances.Terms)
            {
                if (seen.Add(term.Id)) list.Add(term);
            }
        }
        return list;
    }

    private TermGrouping? EnabledDanceGrouping(PackId[] packNames)
    {
        var groups = new List<TermGroup>();
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in packNames)
        {
            if (!Packs.TryGetValue(name, out var pack) || pack.Dances.Grouping is not { } grouping) continue;
            foreach (var group in grouping.Groups)
            {
                if (keys.Add(group.Key)) groups.Add(group);
            }
        }
        return groups.Count == 0 ? null : new TermGrouping("family", groups);
    }

    // ── Loading ──────────────────────────────────────────────────────────────────

    private static readonly IDeserializer Yaml = new DeserializerBuilder().Build();
    private static readonly ISerializer YamlOut = new SerializerBuilder().Build();

    /// <summary>The baseline vocabulary. Compiled in via the generated <c>BaselineVocabulary</c>
    /// (data/vocabulary/ → C#), so loading touches no embedded resources and parses no YAML — AOT/trim-safe.</summary>
    /// <exception cref="SdtsVocabularyException">The compiled baseline failed to build — an internal
    /// data/codegen defect, surfaced typed so a consumer can degrade instead of catching everything.</exception>
    public static SocialDanceVocabulary LoadDefault()
    {
        try
        {
            return BaselineVocabulary.Build();
        }
        catch (Exception ex)
        {
            // Module boundary: the build is generated code over generated data, so there is no narrower
            // type it is guaranteed to fail as. Translated, never swallowed.
            throw new SdtsVocabularyException("Could not build the compiled SDTS baseline vocabulary.", ex);
        }
    }

    /// <summary>Load the baseline by PARSING the on-disk YAML under <paramref name="directory"/> (the
    /// <c>data/vocabulary/</c> tree). This is the source-of-truth oracle the compiled
    /// <see cref="LoadDefault()"/> is drift-gated against; runtime code uses LoadDefault (no file I/O).</summary>
    internal static SocialDanceVocabulary LoadFromYamlDirectory(DirectoryInfo directory)
    {
        var manifest = ReadFile("manifest.yaml");
        var version = GetString(manifest, "vocabulary_version");

        var core = new Dictionary<DimensionId, Dimension>();
        foreach (var dim in GetStringList(AsMap(Value(manifest, "core")), "dimensions"))
        {
            var id = new DimensionId(dim);
            var doc = ReadFile(Path.Combine("core", $"{dim}.yaml"));
            core[id] = new Dimension(id, "core", ReadTerms(doc), ReadAttributeReview(doc, $"core/{dim}.yaml"), ReadCoreGrouping(doc, $"core/{dim}.yaml"));
        }

        var packs = new Dictionary<PackId, DancePack>();
        foreach (var packName in GetStringList(manifest, "packs"))
        {
            var packMeta = ReadFile(Path.Combine("packs", packName, "pack.yaml"));
            var dances = new Dimension(
                DancesDimension,
                "pack",
                ReadTerms(ReadFile(Path.Combine("packs", packName, "dances.yaml"))),
                grouping: ReadGrouping("family", GetStringList(packMeta, "families"), AsMap(Value(packMeta, "family_labels")), $"packs/{packName}/pack.yaml"));
            packs[new PackId(packName)] = new DancePack(
                new PackId(packName),
                GetString(packMeta, "display_name"),
                GetString(packMeta, "description"),
                GetStringList(packMeta, "families"),
                dances,
                ParseReviewStatus(Value(packMeta, "validation"), $"packs/{packName}/pack.yaml"));
        }

        return new SocialDanceVocabulary(version, core, packs);

        Dictionary<string, object?> ReadFile(string relative)
        {
            var path = Path.Combine(directory.FullName, relative.Replace('/', Path.DirectorySeparatorChar));
            using var reader = new StreamReader(path);
            return AsMap(Yaml.Deserialize<object>(reader));
        }
    }

    private static TermGrouping? ReadCoreGrouping(Dictionary<string, object?> doc, string source)
    {
        if (Value(doc, "axes") is not IEnumerable<object> axes) return null;
        var marked = axes.Select(AsMap).Where(axis => IsTrue(Value(axis, "grouping"))).ToList();
        return marked.Count switch
        {
            0 => null,
            1 => ReadGrouping(GetString(marked[0], "name"), GetStringList(marked[0], "values"), AsMap(Value(marked[0], "labels")), source),
            _ => throw new SdtsVocabularyException($"{source}: more than one axis is marked as grouping."),
        };
    }

    private static TermGrouping? ReadGrouping(string attribute, IReadOnlyList<string> keys, Dictionary<string, object?> labels, string source)
    {
        if (keys.Count == 0) return null;
        var groups = new List<TermGroup>(keys.Count);
        foreach (var key in keys)
        {
            if (!labels.TryGetValue(key, out var label) || label is not string text)
            {
                throw new SdtsVocabularyException($"{source}: grouping on '{attribute}' has no label for '{key}'.");
            }
            groups.Add(new TermGroup(key, text));
        }
        return new TermGrouping(attribute, groups);
    }

    private static Dictionary<string, ReviewStatus> ReadAttributeReview(Dictionary<string, object?> doc, string source)
        => AsMap(Value(doc, "validation"))
            .ToDictionary(kv => kv.Key, kv => ParseReviewStatus(kv.Value, source), StringComparer.Ordinal);

    /// <summary>The one wire seam for a <c>validation</c> status; an absent status is <see cref="ReviewStatus.Unflagged"/>.</summary>
    /// <exception cref="SdtsVocabularyException">The status is not one SDTS defines.</exception>
    private static ReviewStatus ParseReviewStatus(object? token, string source) => token switch
    {
        null => ReviewStatus.Unflagged,
        "expert-review-pending" => ReviewStatus.ExpertReviewPending,
        _ => throw new SdtsVocabularyException($"{source}: unknown validation status '{token}'."),
    };

    private static IReadOnlyList<VocabularyTerm> ReadTerms(Dictionary<string, object?> doc)
    {
        var terms = new List<VocabularyTerm>();
        if (Value(doc, "values") is IEnumerable<object> values)
            foreach (var raw in values)
                terms.Add(TermFromMap(AsMap(raw)));
        return terms;
    }

    private static VocabularyTerm TermFromMap(Dictionary<string, object?> m)
    {
        var attrs = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var kv in m)
        {
            if (kv.Key is "id" or "canonical" or "aliases" or "deprecated" or "display")
                continue;
            attrs[kv.Key] = kv.Value;
        }
        return new VocabularyTerm(
            GetString(m, "id"),
            GetString(m, "canonical"),
            GetStringList(m, "aliases"),
            IsTrue(Value(m, "deprecated")),
            attrs,
            ReadDisplay(Value(m, "display")));
    }

    private static TermDisplay? ReadDisplay(object? raw)
    {
        if (raw is not IDictionary<object, object>)
            return null;
        var d = AsMap(raw);
        return new TermDisplay(
            NullIfEmpty(GetString(d, "code")),
            NullIfEmpty(GetString(d, "emoji")),
            NullIfEmpty(GetString(d, "icon")),
            NullIfEmpty(GetString(d, "color")),
            NullIfEmpty(GetString(d, "label")));
    }

    private static string? NullIfEmpty(string s) => string.IsNullOrEmpty(s) ? null : s;

    // ── YAML projection helpers ──────────────────────────────────────────────────
    // YamlDotNet deserialises an untyped document into Dictionary<object,object>,
    // List<object>, and string scalars; these normalise it to string-keyed maps.

    private static Dictionary<string, object?> AsMap(object? value)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (value is IDictionary<object, object> dict)
        {
            foreach (var kv in dict)
            {
                result[Convert.ToString(kv.Key, CultureInfo.InvariantCulture) ?? string.Empty] = kv.Value;
            }
        }
        return result;
    }

    private static object? Value(IReadOnlyDictionary<string, object?> map, string key)
        => map.GetValueOrDefault(key);

    private static string GetString(IReadOnlyDictionary<string, object?> map, string key)
        => map.TryGetValue(key, out var v) ? Convert.ToString(v, CultureInfo.InvariantCulture) ?? string.Empty : string.Empty;

    private static IReadOnlyList<string> GetStringList(IReadOnlyDictionary<string, object?> map, string key)
    {
        if (map.TryGetValue(key, out var v) && v is IEnumerable<object> items)
            return [.. items.Select(i => Convert.ToString(i, CultureInfo.InvariantCulture) ?? string.Empty)];
        return [];
    }

    private static bool IsTrue(object? v)
        => v is not null && bool.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), out var b) && b;

    // Untyped-scalar readers with an explicit fallback — the YAML graph is string-keyed strings, so a bool
    // arrives as "true"/"false" and an int as "11". A key that is absent, null, or unparseable takes the fallback.
    private static string? GetStringOrNull(IReadOnlyDictionary<string, object?> map, string key)
        => map.TryGetValue(key, out var v) && v is not null ? Convert.ToString(v, CultureInfo.InvariantCulture) : null;

    private static string GetStringOr(IReadOnlyDictionary<string, object?> map, string key, string fallback)
        => GetStringOrNull(map, key) ?? fallback;

    private static bool GetBool(IReadOnlyDictionary<string, object?> map, string key, bool fallback)
        => map.TryGetValue(key, out var v) && v is not null
           && bool.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), out var b) ? b : fallback;

    private static int GetInt(IReadOnlyDictionary<string, object?> map, string key, int fallback)
        => map.TryGetValue(key, out var v) && v is not null
           && int.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)
            ? n : fallback;

    private static TEnum GetEnum<TEnum>(IReadOnlyDictionary<string, object?> map, string key, TEnum fallback) where TEnum : struct
        => map.TryGetValue(key, out var v) && v is not null
           && Enum.TryParse<TEnum>(Convert.ToString(v, CultureInfo.InvariantCulture), ignoreCase: true, out var parsed)
            ? parsed : fallback;
}
