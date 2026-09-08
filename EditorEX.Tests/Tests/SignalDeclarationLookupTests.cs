using System.Collections.Generic;
using EditorEX.SDK.Integration.Patches;
using Zenject;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class SignalDeclarationLookupTests
    {
        private interface ILoadedSignal { }

        private sealed class LoadedSignal : ILoadedSignal { }

        private sealed class UnrelatedSignal { }

        [Fact]
        public void TryGet_returns_exact_declaration_when_the_type_is_declared()
        {
            var map = new Dictionary<BindingId, string>
            {
                [new BindingId(typeof(UnrelatedSignal), null)] = "unrelated",
                [new BindingId(typeof(LoadedSignal), null)] = "exact",
            };

            bool found = SignalDeclarationLookup.TryGet(
                map,
                new BindingId(typeof(LoadedSignal), null),
                out string? value
            );

            Assert.True(found);
            Assert.Equal("exact", value);
        }

        [Fact]
        public void TryGet_matches_a_declared_interface_when_firing_a_concrete_signal()
        {
            var map = new Dictionary<BindingId, string>
            {
                [new BindingId(typeof(UnrelatedSignal), null)] = "unrelated",
                [new BindingId(typeof(ILoadedSignal), null)] = "interface",
            };

            bool found = SignalDeclarationLookup.TryGet(
                map,
                new BindingId(typeof(LoadedSignal), null),
                out string? value
            );

            Assert.True(found);
            Assert.Equal("interface", value);
        }

        [Fact]
        public void TryGet_prefers_an_exact_type_over_an_assignable_interface()
        {
            var map = new Dictionary<BindingId, string>
            {
                [new BindingId(typeof(ILoadedSignal), null)] = "interface",
                [new BindingId(typeof(LoadedSignal), null)] = "exact",
            };

            bool found = SignalDeclarationLookup.TryGet(
                map,
                new BindingId(typeof(LoadedSignal), null),
                out string? value
            );

            Assert.True(found);
            Assert.Equal("exact", value);
        }

        [Fact]
        public void TryGet_does_not_match_when_identifiers_differ()
        {
            var map = new Dictionary<BindingId, string>
            {
                [new BindingId(typeof(ILoadedSignal), "a")] = "interface",
            };

            bool found = SignalDeclarationLookup.TryGet(
                map,
                new BindingId(typeof(LoadedSignal), "b"),
                out string? value
            );

            Assert.False(found);
            Assert.Null(value);
        }

        [Fact]
        public void TryGet_returns_false_for_an_undeclared_unrelated_type()
        {
            var map = new Dictionary<BindingId, string>
            {
                [new BindingId(typeof(ILoadedSignal), null)] = "interface",
            };

            bool found = SignalDeclarationLookup.TryGet(
                map,
                new BindingId(typeof(UnrelatedSignal), null),
                out string? value
            );

            Assert.False(found);
            Assert.Null(value);
        }
    }
}
