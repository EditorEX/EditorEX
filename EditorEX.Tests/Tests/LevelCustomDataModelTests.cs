using CustomJSONData.CustomBeatmap;
using EditorEX.CustomDataModels;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class LevelCustomDataModelTests
    {
        [Fact]
        public void GetOrCreateBeatmapCustomData_creates_an_entry_for_a_new_difficulty_filename()
        {
            var model = new LevelCustomDataModel();

            CustomData created = model.GetOrCreateBeatmapCustomData("ExpertPlus.beatmap.dat");

            Assert.NotNull(created);
            Assert.Same(
                created,
                model.BeatmapCustomDatasByFilename!["ExpertPlus.beatmap.dat"]
            );
        }

        [Fact]
        public void GetOrCreateBeatmapCustomData_returns_the_existing_bag()
        {
            var existing = new CustomData();
            var model = new LevelCustomDataModel
            {
                BeatmapCustomDatasByFilename = new() { ["ExpertPlusStandard.dat"] = existing },
            };

            Assert.Same(existing, model.GetOrCreateBeatmapCustomData("ExpertPlusStandard.dat"));
        }
    }
}
