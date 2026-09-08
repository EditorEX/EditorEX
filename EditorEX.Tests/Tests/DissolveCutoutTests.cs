using EditorEX.Essentials.Visuals;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class DissolveCutoutTests
    {
        [Fact]
        public void TryGetChanged_skips_when_dissolve_is_null()
        {
            float last = 0.25f;

            bool changed = DissolveCutout.TryGetChanged(null, ref last, out float cutout);

            Assert.False(changed);
            Assert.Equal(0.25f, last);
            Assert.Equal(0.25f, cutout);
        }

        [Fact]
        public void TryGetChanged_converts_dissolve_to_cutout()
        {
            float last = float.NaN;

            bool changed = DissolveCutout.TryGetChanged(0.25f, ref last, out float cutout);

            Assert.True(changed);
            Assert.Equal(0.75f, cutout);
            Assert.Equal(0.75f, last);
        }

        [Fact]
        public void TryGetChanged_skips_when_cutout_is_unchanged()
        {
            float last = 0.5f;

            bool changed = DissolveCutout.TryGetChanged(0.5f, ref last, out float cutout);

            Assert.False(changed);
            Assert.Equal(0.5f, cutout);
            Assert.Equal(0.5f, last);
        }
    }
}
