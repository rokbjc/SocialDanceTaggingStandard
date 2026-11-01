using SocialDanceTaggingStandard.Id3;
using TagLib;
using Xunit;
using Id3v2Tag = TagLib.Id3v2.Tag;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>An <see cref="Id3File"/> writes ID3v2.4 for its own tag and leaves the process-wide TagLib
/// version defaults to the host application.</summary>
public sealed class Id3FileVersionTests : IDisposable
{
    private const int MajorVersionOffset = 3;
    private const byte Id3v23 = 3;
    private const byte Id3v24 = 4;
    private const int FrameLength = 417;
    private const int FrameCount = 8;

    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("sdts-id3-version-");

    public void Dispose() => _directory.Delete(recursive: true);

    [Fact]
    public void Opening_AndDisposing_LeavesTheTagLibVersionDefaultsUnchanged()
    {
        FileInfo mp3 = CreateMp3WithTag(Id3v23);
        byte defaultVersion = Id3v2Tag.DefaultVersion;
        bool forced = Id3v2Tag.ForceDefaultVersion;

        using (new Id3File(mp3))
        {
        }

        Assert.Equal(defaultVersion, Id3v2Tag.DefaultVersion);
        Assert.Equal(forced, Id3v2Tag.ForceDefaultVersion);
    }

    [Fact]
    public void Save_WritesId3v24_OverAnId3v23Tag()
    {
        FileInfo mp3 = CreateMp3WithTag(Id3v23);
        Assert.Equal(Id3v23, ReadMajorVersion(mp3));

        using (var file = new Id3File(mp3))
        {
            file.Title = "Ja-Da";
            file.Save();
        }

        Assert.Equal(Id3v24, ReadMajorVersion(mp3));
    }

    [Fact]
    public void Save_DoesNotChangeTheVersionOfAnUnrelatedTagLibWrite()
    {
        FileInfo ours = CreateMp3WithTag(Id3v23);
        FileInfo theirs = CreateMp3WithTag(Id3v23);

        using (var file = new Id3File(ours))
        {
            file.Title = "Ja-Da";
            file.Save();
        }

        using (var other = TagLib.File.Create(theirs.FullName))
        {
            other.Tag.Title = "Chee Chee";
            other.Save();
        }

        Assert.Equal(Id3v24, ReadMajorVersion(ours));
        Assert.Equal(Id3v23, ReadMajorVersion(theirs));
    }

    private FileInfo CreateMp3WithTag(byte version)
    {
        var mp3 = new FileInfo(Path.Combine(_directory.FullName, $"{Guid.NewGuid():N}.mp3"));
        var audio = new byte[FrameLength * FrameCount];
        for (int frame = 0; frame < FrameCount; frame++)
        {
            int start = frame * FrameLength;
            audio[start] = 0xFF;
            audio[start + 1] = 0xFB;
            audio[start + 2] = 0x90;
        }

        System.IO.File.WriteAllBytes(mp3.FullName, audio);

        using var created = TagLib.File.Create(mp3.FullName);
        var tag = (Id3v2Tag)created.GetTag(TagTypes.Id3v2, create: true);
        tag.Version = version;
        tag.Title = "seed";
        created.Save();

        Assert.Equal(version, ReadMajorVersion(mp3));
        return mp3;
    }

    private static byte ReadMajorVersion(FileInfo mp3)
    {
        using FileStream stream = mp3.OpenRead();
        var header = new byte[MajorVersionOffset + 1];
        stream.ReadExactly(header);
        Assert.Equal("ID3", System.Text.Encoding.ASCII.GetString(header, 0, MajorVersionOffset));
        return header[MajorVersionOffset];
    }
}
