using System;
using System.Collections.Generic;
using EditorEX.Essentials.PreviewState;
using UnityEngine;

namespace EditorEX.Chroma.EnvironmentEnhancement
{
    internal sealed class ChromaGeometryPreviewAction : IPreviewStateAction
    {
        private readonly Func<IReadOnlyList<GameObject>> _spawn;
        private readonly Action<IReadOnlyList<GameObject>> _despawn;
        private IReadOnlyList<GameObject>? _spawned;

        public ChromaGeometryPreviewAction(
            Func<IReadOnlyList<GameObject>> spawn,
            Action<IReadOnlyList<GameObject>> despawn
        )
        {
            _spawn = spawn;
            _despawn = despawn;
        }

        public void Execute()
        {
            if (_spawned != null)
            {
                return;
            }

            _spawned = _spawn();
        }

        public void Reverse()
        {
            if (_spawned == null)
            {
                return;
            }

            _despawn(_spawned);
            _spawned = null;
        }

        public void Tick(float beat) { }
    }
}
