namespace EditorEX.Tests.BeatSaver
{
    public sealed class MapFixture
    {
        public MapFixture(
            string hash,
            string characteristic,
            string difficulty,
            int expectedMajorVersion
        )
        {
            Hash = hash;
            Characteristic = characteristic;
            Difficulty = difficulty;
            ExpectedMajorVersion = expectedMajorVersion;
        }

        public string Hash { get; }

        public string Characteristic { get; }

        public string Difficulty { get; }

        public int ExpectedMajorVersion { get; }

        public override string ToString()
        {
            return $"{Hash} {Characteristic} {Difficulty} v{ExpectedMajorVersion}";
        }
    }
}
