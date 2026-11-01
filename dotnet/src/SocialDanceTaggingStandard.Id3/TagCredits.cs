namespace SocialDanceTaggingStandard.Id3;

/// <summary>One TIPL (Involved People List) entry: a production/technical function and the person who
/// performed it (e.g. <c>("producer", "John Hammond")</c>). Its own type, distinct from
/// <see cref="MusicianCredit"/>, because the two frames draw their first column from different codebooks
/// (a role vocabulary vs an instrument vocabulary). The container is vocab-agnostic — <see cref="Role"/>
/// and <see cref="Name"/> are raw strings; typed resolution is a semantic-layer concern.</summary>
/// <param name="Role">The production/technical function (raw).</param>
/// <param name="Name">The person's name as stored (raw; may itself be a comma-list per the ID3 spec).</param>
public readonly record struct InvolvedPerson(string Role, string Name);

/// <summary>One TMCL (Musician Credits List) entry: an instrument and the person who played it (e.g.
/// <c>("Piano", "Count Basie")</c>). See <see cref="InvolvedPerson"/> for why the two are distinct
/// types.</summary>
/// <param name="Instrument">The instrument (raw).</param>
/// <param name="Name">The performer's name as stored (raw; may be a comma-list per the ID3 spec).</param>
public readonly record struct MusicianCredit(string Instrument, string Name);
