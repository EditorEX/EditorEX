using System.Collections.Generic;

namespace EditorEX.Tests.BeatSaver
{
    public static class MapCatalog
    {
        // Curated human-mapped Standard ExpertPlus. Hash is pinned so a later
        // BeatSaver re-upload cannot silently change the fixture.
        public static readonly MapFixture V3VanillaExpertPlus = new(
            "f1745bc63ab0befbc86fc06881e334a92eecde33",
            "Standard",
            "ExpertPlus",
            3
        );

        public static readonly MapFixture V3NoodleChromaExpertPlus = new(
            "1fa93811ee42d918715ebc24d550ef60213542db",
            "Lawless",
            "ExpertPlus",
            3
        );

        public static readonly MapFixture V2NoodleChromaExpertPlus = new(
            "b0ea07691e483e7f5b2d9a2daf6774b512a68855",
            "Standard",
            "ExpertPlus",
            2
        );

        public static readonly MapFixture V4VanillaExpertPlus = new(
            "741833d14d890f22e734f457bc60bd4b1c99de22",
            "Standard",
            "ExpertPlus",
            4
        );

        public static IEnumerable<object[]> AllTheoryData
        {
            get
            {
                foreach (MapFixture fixture in All)
                {
                    yield return new object[] { fixture };
                }
            }
        }

        public static IEnumerable<MapFixture> All
        {
            get
            {
                yield return V3VanillaExpertPlus;
                yield return V3NoodleChromaExpertPlus;
                yield return V2NoodleChromaExpertPlus;
                yield return V4VanillaExpertPlus;
            }
        }
    }
}
