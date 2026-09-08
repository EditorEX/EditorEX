using System;
using EditorEX.UI.Patches;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class VariableNjsSupportTests
    {
        [Theory]
        [InlineData("2.6.0")]
        [InlineData("3.3.0")]
        [InlineData("3.2.0")]
        public void IsAvailable_is_false_for_v2_and_v3(string version)
        {
            Assert.False(VariableNjsSupport.IsAvailable(new Version(version)));
        }

        [Theory]
        [InlineData("4.0.0")]
        [InlineData("4.1.0")]
        public void IsAvailable_is_true_for_v4(string version)
        {
            Assert.True(VariableNjsSupport.IsAvailable(new Version(version)));
        }

        [Fact]
        public void IsAvailable_is_false_when_version_is_null()
        {
            Assert.False(VariableNjsSupport.IsAvailable(null));
        }
    }
}
