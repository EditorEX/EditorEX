using System.IO;
using EditorEX.Util;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class AudioClipLoadTests
    {
        [Fact]
        public void AssignNameOrNull_returns_null_when_clip_is_null()
        {
            Assert.Null(AudioClipLoad.AssignNameOrNull(null, @"C:\maps\song.ogg"));
        }

        [Fact]
        public void ResolveExistingFile_returns_null_for_empty_or_missing_paths()
        {
            Assert.Null(AudioClipLoad.ResolveExistingFile(null));
            Assert.Null(AudioClipLoad.ResolveExistingFile(string.Empty));
            Assert.Null(AudioClipLoad.ResolveExistingFile(@"C:\definitely-not-a-real-audio-file.ogg"));
        }

        [Fact]
        public void ResolveExistingFile_returns_null_for_a_directory()
        {
            string dir = Path.Combine(Path.GetTempPath(), "EditorEX-AudioClipLoadTests");
            Directory.CreateDirectory(dir);

            Assert.Null(AudioClipLoad.ResolveExistingFile(dir));
            Assert.Null(AudioClipLoad.ResolveExistingFile(Path.Combine(dir, string.Empty)));
        }

        [Fact]
        public void ResolveExistingFile_returns_an_existing_file()
        {
            string path = Path.Combine(Path.GetTempPath(), "EditorEX-AudioClipLoadTests-song.ogg");
            File.WriteAllText(path, "not-really-audio");

            try
            {
                Assert.Equal(path, AudioClipLoad.ResolveExistingFile(path));
                Assert.Equal(
                    path,
                    AudioClipLoad.ResolveFromLevel(
                        Path.GetTempPath(),
                        "missing.ogg",
                        songFilePath: path
                    )
                );
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
