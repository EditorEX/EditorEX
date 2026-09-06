using System;
using System.Collections.Generic;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Events;
using Heck.Animation;
using UnityEngine;
using Vivify;

namespace EditorEX.Vivify.Events
{
    internal sealed class VivifySetGlobalPropertyPreviewAction : IPreviewStateAction
    {
        private readonly List<MaterialProperty> _properties;
        private readonly EditorSetGlobalProperty _setGlobalProperty;
        private readonly float _fromBeat;
        private readonly float _durationBeats;
        private readonly Functions _easing;
        private readonly List<Action> _restore = [];
        private bool _active;
        private bool _written;

        public VivifySetGlobalPropertyPreviewAction(
            List<MaterialProperty> properties,
            EditorSetGlobalProperty setGlobalProperty,
            float fromBeat,
            float durationBeats,
            Functions easing
        )
        {
            _properties = properties;
            _setGlobalProperty = setGlobalProperty;
            _fromBeat = fromBeat;
            _durationBeats = durationBeats;
            _easing = easing;
        }

        public void Execute()
        {
            if (_active)
            {
                return;
            }

            CaptureOriginals();
            _active = true;
        }

        public void Reverse()
        {
            if (!_active)
            {
                return;
            }

            for (int i = 0; i < _restore.Count; i++)
            {
                _restore[i]();
            }

            _written = false;
            _active = false;
        }

        public void Tick(float beat)
        {
            if (!_active || !VivifyPreviewOwnership.NeedsMaterialWrite(_durationBeats, _written))
            {
                return;
            }

            float progress = HeckTrackPreviewSampler.EasedProgress(
                beat,
                _fromBeat,
                _durationBeats,
                repeat: 0,
                _easing,
                out _
            );
            _setGlobalProperty.ApplyAtProgress(_properties, progress);
            _written = true;
        }

        private void CaptureOriginals()
        {
            if (_restore.Count > 0)
            {
                return;
            }

            foreach (MaterialProperty property in _properties)
            {
                switch (property.Id)
                {
                    case int id:
                        Remember(id, property.Type);
                        break;
                    case string name
                        when property.Type == global::Vivify.MaterialPropertyType.Keyword:
                        bool enabled = Shader.IsKeywordEnabled(name);
                        _restore.Add(() => EditorSetGlobalProperty.SetGlobalKeyword(name, enabled));
                        break;
                }
            }
        }

        private void Remember(int id, global::Vivify.MaterialPropertyType type)
        {
            switch (type)
            {
                case global::Vivify.MaterialPropertyType.Float:
                    float value = Shader.GetGlobalFloat(id);
                    _restore.Add(() => Shader.SetGlobalFloat(id, value));
                    break;
                case global::Vivify.MaterialPropertyType.Color:
                    Color color = Shader.GetGlobalColor(id);
                    _restore.Add(() => Shader.SetGlobalColor(id, color));
                    break;
                case global::Vivify.MaterialPropertyType.Vector:
                    Vector4 vector = Shader.GetGlobalVector(id);
                    _restore.Add(() => Shader.SetGlobalVector(id, vector));
                    break;
                case global::Vivify.MaterialPropertyType.Texture:
                    Texture? texture = Shader.GetGlobalTexture(id);
                    _restore.Add(() => Shader.SetGlobalTexture(id, texture));
                    break;
            }
        }
    }
}
