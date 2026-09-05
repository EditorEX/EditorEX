using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Scripts.SerializedData;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.MapData.Objects;
using Xunit;
using V2Custom = CustomJSONData.CustomBeatmap.Version2_6_0AndEarlierCustomBeatmapSaveData;
using V3Custom = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;

namespace EditorEX.Tests.Tests
{
    public class ObstacleCodecTests
    {
        [Fact]
        public void LoadV3_crouch_wall_uses_v3_encoding_not_v4_height_minus_two()
        {
            // Official v3 crouch: y=2, h=3. Editor cells are row = y/2, height = (h+1)/2.
            // Beat Saber 1.40's CreateObstacleEditorData_v3 wrongly applies v4's h-2 (row=2, height=1).
            V3Custom.ObstacleSaveData data = new(0f, 1, 2, 1f, 1, 3, new CustomData());

            ObstacleEditorData editor = ObstacleCodec.LoadV3(data, EmptyV3Rotation());

            Assert.Equal(1, editor.row);
            Assert.Equal(2, editor.height);
        }

        [Theory]
        [InlineData(0, 1, 0, 1)]
        [InlineData(0, 5, 0, 3)]
        [InlineData(0, 9, 0, 5)]
        [InlineData(4, 3, 2, 2)]
        public void LoadV3_maps_file_layer_and_height_to_editor_cells(
            int fileY,
            int fileH,
            int expectedRow,
            int expectedHeight
        )
        {
            V3Custom.ObstacleSaveData data = new(0f, 0, fileY, 1f, 1, fileH, new CustomData());

            ObstacleEditorData editor = ObstacleCodec.LoadV3(data, EmptyV3Rotation());

            Assert.Equal(expectedRow, editor.row);
            Assert.Equal(expectedHeight, editor.height);
        }

        [Fact]
        public void LoadV2_top_wall_matches_v3_crouch_editor_cells()
        {
            V2Custom.ObstacleSaveData data = new(
                0f,
                1,
                ObstacleType.Top,
                1f,
                1,
                new CustomData()
            );

            ObstacleEditorData editor = ObstacleCodec.LoadV2(
                data,
                new BeatmapEditorRotationProcessor_v2(
                    System.Array.Empty<EventData>()
                )
            );

            Assert.Equal(1, editor.row);
            Assert.Equal(2, editor.height);
        }

        [Fact]
        public void SaveV2_crouch_editor_cells_write_top_type()
        {
            ObstacleEditorData editor = ObstacleEditorData.CreateNew(0f, 1, 1, 0, 1f, 1, 2);

            BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData saved =
                ObstacleCodec.SaveV2(editor, new CustomDataRepository());

            Assert.Equal(ObstacleType.Top, saved.type);
        }

        [Theory]
        [InlineData(3, 2, 5)]
        [InlineData(2, 2, 3)]
        [InlineData(3, 3, 5)]
        [InlineData(2, 3, 3)]
        [InlineData(3, 4, 5)]
        [InlineData(2, 4, 4)]
        public void GameplayHeight_converts_editor_cells_by_map_version(
            int editorHeight,
            int versionMajor,
            int expected
        )
        {
            Assert.Equal(expected, ObstacleCodec.GameplayHeight(editorHeight, versionMajor));
        }

        [Fact]
        public void LoadV4_uses_height_minus_two()
        {
            var data = new BeatmapSaveDataVersion4.Obstacle
            {
                x = 1,
                y = 1,
                d = 2f,
                w = 2,
                h = 4,
            };

            ObstacleEditorData editor = ObstacleCodec.LoadV4(8f, 15, data);

            Assert.Equal(8f, editor.beat);
            Assert.Equal(15, editor.rotation);
            Assert.Equal(1, editor.column);
            Assert.Equal(1, editor.row);
            Assert.Equal(2, editor.height);
            Assert.Equal(2, editor.width);
            Assert.Equal(2f, editor.duration);
        }

        [Fact]
        public void SaveV4Data_adds_two_to_editor_height()
        {
            ObstacleEditorData editor = ObstacleEditorData.CreateNew(0f, 2, 1, 0, 1.5f, 3, 2);

            BeatmapSaveDataVersion4.Obstacle saved = ObstacleCodec.SaveV4Data(editor);

            Assert.Equal(2, saved.x);
            Assert.Equal(1, saved.y);
            Assert.Equal(1.5f, saved.d);
            Assert.Equal(3, saved.w);
            Assert.Equal(4, saved.h);
        }

        [Theory]
        [InlineData(0, 1, false)]
        [InlineData(1, 0, false)]
        [InlineData(1, 1, true)]
        public void CanSaveV4_requires_positive_width_height_and_duration(
            int width,
            int height,
            bool expected
        )
        {
            ObstacleEditorData editor = ObstacleEditorData.CreateNew(0f, 0, 0, 0, 1f, width, height);
            Assert.Equal(expected, ObstacleCodec.CanSaveV4(editor));
        }

        [Theory]
        [InlineData(0, 3, 0)]
        [InlineData(1, 3, 2)]
        [InlineData(1, 4, 1)]
        public void GameplayLayer_converts_editor_row_by_map_version(
            int editorRow,
            int versionMajor,
            int expected
        )
        {
            Assert.Equal(expected, ObstacleCodec.GameplayLayer(editorRow, versionMajor));
        }

        private static BeatmapEditorRotationProcessor_v3 EmptyV3Rotation()
        {
            return new BeatmapEditorRotationProcessor_v3(new List<RotationEventData>());
        }
    }
}
