using System.Collections.Generic;
using EditorEX.Vivify.Patches;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class CullingLayerScratchTests
    {
        [Fact]
        public void Include_skips_destroyed_and_inactive_objects()
        {
            Assert.False(CullingLayerScratch.Include(alive: false, activeInHierarchy: true));
            Assert.False(CullingLayerScratch.Include(alive: true, activeInHierarchy: false));
            Assert.True(CullingLayerScratch.Include(alive: true, activeInHierarchy: true));
        }

        [Fact]
        public void Capture_moves_each_instance_to_the_culling_layer_once()
        {
            CullingLayerProbe[] source =
            [
                new(1, alive: true, active: true, layer: 8),
                new(1, alive: true, active: true, layer: 8),
                new(2, alive: true, active: false, layer: 8),
                new(3, alive: false, active: true, layer: 8),
                new(4, alive: true, active: true, layer: 11),
            ];
            HashSet<int> seen = new();
            List<(CullingLayerProbe obj, int layer)> cache = new();

            CullingLayerScratch.Capture(source, seen, cache, cullLayer: 22);

            Assert.Equal(2, cache.Count);
            Assert.Equal(1, cache[0].obj.InstanceId);
            Assert.Equal(8, cache[0].layer);
            Assert.Equal(22, cache[0].obj.Layer);
            Assert.Equal(4, cache[1].obj.InstanceId);
            Assert.Equal(11, cache[1].layer);
            Assert.Equal(22, cache[1].obj.Layer);
        }

        [Fact]
        public void Capture_does_not_rewrite_objects_already_on_the_culling_layer()
        {
            CullingLayerProbe probe = new(1, alive: true, active: true, layer: 22);
            HashSet<int> seen = new();
            List<(CullingLayerProbe obj, int layer)> cache = new();

            CullingLayerScratch.Capture([probe], seen, cache, cullLayer: 22);

            Assert.Single(cache);
            Assert.Equal(22, cache[0].layer);
            Assert.Equal(22, probe.Layer);
        }

        [Fact]
        public void Restore_writes_original_layers_back()
        {
            CullingLayerProbe first = new(1, alive: true, active: true, layer: 8);
            CullingLayerProbe second = new(2, alive: true, active: true, layer: 11);
            HashSet<int> seen = new();
            List<(CullingLayerProbe obj, int layer)> cache = new();

            CullingLayerScratch.Capture([first, second], seen, cache, cullLayer: 22);
            CullingLayerScratch.Restore(cache);

            Assert.Empty(cache);
            Assert.Equal(8, first.Layer);
            Assert.Equal(11, second.Layer);
        }
    }
}
