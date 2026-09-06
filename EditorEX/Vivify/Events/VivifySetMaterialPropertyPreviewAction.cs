using System;
using System.Collections.Generic;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Events;
using Heck.Animation;
using UnityEngine;
using Vivify;

namespace EditorEX.Vivify.Events
{
    internal sealed class VivifySetMaterialPropertyPreviewAction : IPreviewStateAction
    {
        private readonly Material _material;
        private readonly List<MaterialProperty> _properties;
        private readonly EditorSetMaterialProperty _setMaterialProperty;
        private readonly float _fromBeat;
        private readonly float _durationBeats;
        private readonly Functions _easing;
        private readonly List<Action> _restore = [];
        private bool _active;
        private bool _written;

        public VivifySetMaterialPropertyPreviewAction(
            Material material,
            List<MaterialProperty> properties,
            EditorSetMaterialProperty setMaterialProperty,
            float fromBeat,
            float durationBeats,
            Functions easing
        )
        {
            _material = material;
            _properties = properties;
            _setMaterialProperty = setMaterialProperty;
            _fromBeat = fromBeat;
            _durationBeats = durationBeats;
            _easing = easing;
        }

        public void Execute()
        {
            if (_active || !VivifyPreviewOwnership.CanWriteMaterial(_material))
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

            if (VivifyPreviewOwnership.CanWriteMaterial(_material))
            {
                for (int i = 0; i < _restore.Count; i++)
                {
                    _restore[i]();
                }
            }

            _written = false;
            _active = false;
        }

        public void Tick(float beat)
        {
            if (!_active || !VivifyPreviewOwnership.CanWriteMaterial(_material))
            {
                return;
            }

            if (!VivifyPreviewOwnership.NeedsMaterialWrite(_durationBeats, _written))
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
            _setMaterialProperty.ApplyAtProgress(_material, _properties, progress);
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
                        bool enabled = _material.IsKeywordEnabled(name);
                        _restore.Add(() => SetKeyword(name, enabled));
                        break;
                }
            }
        }

        private void Remember(int id, global::Vivify.MaterialPropertyType type)
        {
            switch (type)
            {
                case global::Vivify.MaterialPropertyType.Float:
                    float value = _material.GetFloat(id);
                    _restore.Add(() => _material.SetFloat(id, value));
                    break;
                case global::Vivify.MaterialPropertyType.Color:
                    Color color = _material.GetColor(id);
                    _restore.Add(() => _material.SetColor(id, color));
                    break;
                case global::Vivify.MaterialPropertyType.Vector:
                    Vector4 vector = _material.GetVector(id);
                    _restore.Add(() => _material.SetVector(id, vector));
                    break;
                case global::Vivify.MaterialPropertyType.Texture:
                    Texture? texture = _material.GetTexture(id);
                    _restore.Add(() => _material.SetTexture(id, texture));
                    break;
            }
        }

        private void SetKeyword(string name, bool enabled)
        {
            if (!VivifyPreviewOwnership.CanWriteMaterial(_material) || string.IsNullOrEmpty(name))
            {
                return;
            }

            if (enabled)
            {
                _material.EnableKeyword(name);
            }
            else
            {
                _material.DisableKeyword(name);
            }
        }
    }
}
