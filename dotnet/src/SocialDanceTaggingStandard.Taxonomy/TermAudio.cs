namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// Audio illustration for a term (used by rhythmic feels): a synthesized rhythm <see cref="Clip"/>
/// under <c>data/vocabulary/audio/</c> and a real-recording <see cref="Youtube"/> example with a
/// human <see cref="YoutubeLabel"/>. Any field may be null.
/// </summary>
public sealed record TermAudio(string? Clip, string? Youtube, string? YoutubeLabel);
