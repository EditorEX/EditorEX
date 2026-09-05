using System.Collections.Generic;
using EditorEX.Chroma.EnvironmentEnhancement;
using UnityEngine;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class ChromaGeometryPreviewActionTests
    {
        [Fact]
        public void Execute_is_idempotent_until_reversed()
        {
            int spawns = 0;
            int despawns = 0;
            var action = new ChromaGeometryPreviewAction(
                () =>
                {
                    spawns++;
                    return new List<GameObject>();
                },
                _ => despawns++
            );

            action.Execute();
            action.Execute();
            action.Reverse();
            action.Execute();

            Assert.Equal(2, spawns);
            Assert.Equal(1, despawns);
        }

        [Fact]
        public void Reverse_is_safe_before_execute()
        {
            int despawns = 0;
            var action = new ChromaGeometryPreviewAction(
                () => new List<GameObject>(),
                _ => despawns++
            );

            action.Reverse();

            Assert.Equal(0, despawns);
        }
    }
}
