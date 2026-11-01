using SocialDanceTaggingStandard.Id3;
using SocialDanceTaggingStandard.Sync;
using SocialDanceTaggingStandard.TestKit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>An in-memory <see cref="ISdtsContainer"/> test double — standard frames + audio props with
/// no file and no TagLib. Standard frames are plain auto-properties; read-only audio props are seeded via
/// the constructor.</summary>
internal sealed class InMemoryContainer : ISdtsContainer
{
    public InMemoryContainer(TimeSpan duration = default, int audioBitrate = 0, int sampleRate = 0)
    {
        Duration = duration;
        AudioBitrate = audioBitrate;
        SampleRate = sampleRate;
    }

    public ISdtsSource UserText { get; } = new InMemoryTagSource();
    public ISdtsSource Comments { get; } = new InMemoryTagSource();

    public string? Title { get; set; }
    public IReadOnlyList<string> Artists { get; set; } = [];
    public string? Album { get; set; }
    public IReadOnlyList<string> AlbumArtists { get; set; } = [];
    public IReadOnlyList<string> Genres { get; set; } = [];
    public IReadOnlyList<string> Moods { get; set; } = [];
    public int? Year { get; set; }
    public int? Bpm { get; set; }
    public string? Grouping { get; set; }
    public string? Comment { get; set; }
    public int? Rating { get; set; }
    public IReadOnlyList<InvolvedPerson> InvolvedPeople { get; set; } = [];
    public IReadOnlyList<MusicianCredit> MusicianCredits { get; set; } = [];

    public FileInfo? File => null;
    public TimeSpan Duration { get; }
    public int AudioBitrate { get; }
    public int SampleRate { get; }

    public bool Saved { get; private set; }
    public void Save() => Saved = true;
    public void Dispose() { }
}
