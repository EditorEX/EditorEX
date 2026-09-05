using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.MapData.Objects;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class CustomDataUtilTests
    {
        [Fact]
        public void Filter_strips_NE_scratch_keys_and_keeps_owned()
        {
            var json = new CustomData
            {
                ["NE_flipLineIndex"] = 1f,
                ["coordinates"] = new[] { 0f, 1f },
            };

            CustomData filtered = CustomDataUtil.Filter(json);

            Assert.False(filtered.ContainsKey("NE_flipLineIndex"));
            Assert.True(filtered.ContainsKey("coordinates"));
        }

        [Fact]
        public void SaveCustom_is_false_when_object_has_no_bag()
        {
            var repo = new CustomDataRepository();
            NoteEditorData note = ColorNote();

            Assert.False(CustomDataUtil.SaveCustom(note, repo, out CustomData custom));
            Assert.Null(custom);
        }

        [Fact]
        public void SaveCustom_is_false_when_only_scratch_keys_remain()
        {
            var repo = new CustomDataRepository();
            NoteEditorData note = ColorNote();
            repo.AddCustomData(note, new CustomData { ["NE_flipYSide"] = 1f });

            Assert.False(CustomDataUtil.SaveCustom(note, repo, out _));
        }

        [Fact]
        public void SaveCustom_is_true_when_a_real_key_survives_filter()
        {
            var repo = new CustomDataRepository();
            NoteEditorData note = ColorNote();
            repo.AddCustomData(
                note,
                new CustomData { ["NE_flipYSide"] = 1f, ["track"] = "lane" }
            );

            Assert.True(CustomDataUtil.SaveCustom(note, repo, out CustomData custom));
            Assert.Equal("lane", custom["track"]);
            Assert.False(custom.ContainsKey("NE_flipYSide"));
        }

        private static NoteEditorData ColorNote()
        {
            return NoteEditorData.CreateNew(
                4f,
                1,
                0,
                0,
                ColorType.ColorA,
                NoteType.Note,
                NoteCutDirection.Up,
                0
            );
        }
    }
}
