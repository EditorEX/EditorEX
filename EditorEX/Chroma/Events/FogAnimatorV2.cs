using System;
using Chroma;
using EditorEX.CustomJSONData;
using Heck.Animation;
using UnityEngine;
using Zenject;
using static EditorEX.Chroma.Constants;

namespace EditorEX.Chroma.Events
{
    internal class EditorFogAnimatorV2 : IDisposable
    {
        private readonly BloomFogSO _bloomFog;
        private readonly BloomFogEnvironmentParams _transitionFogParams;
        private readonly BloomFogEnvironmentParams _defaults;

        private EditorFogAnimatorV2(BloomFogSO bloomFog)
        {
            _bloomFog = bloomFog;

            _transitionFogParams = ScriptableObject.CreateInstance<BloomFogEnvironmentParams>();
            _defaults = bloomFog.defaultForParams;
            RestoreDefaults();
            bloomFog.transitionFogParams = _transitionFogParams;
        }

        internal void Apply(Track track)
        {
            _transitionFogParams.attenuation = ChromaFogPreview.Channel(
                track.GetProperty<float>(V2_ATTENUATION),
                _defaults.attenuation
            );
            _transitionFogParams.offset = ChromaFogPreview.Channel(
                track.GetProperty<float>(V2_OFFSET),
                _defaults.offset
            );
            _transitionFogParams.heightFogStartY = ChromaFogPreview.Channel(
                track.GetProperty<float>(V2_HEIGHT_FOG_STARTY),
                _defaults.heightFogStartY
            );
            _transitionFogParams.heightFogHeight = ChromaFogPreview.Channel(
                track.GetProperty<float>(V2_HEIGHT_FOG_HEIGHT),
                _defaults.heightFogHeight
            );
            _bloomFog._transition = 1;
        }

        internal void RestoreDefaults()
        {
            _transitionFogParams.attenuation = _defaults.attenuation;
            _transitionFogParams.offset = _defaults.offset;
            _transitionFogParams.heightFogStartY = _defaults.heightFogStartY;
            _transitionFogParams.heightFogHeight = _defaults.heightFogHeight;
            _bloomFog.transition = 0;
        }

        public void Dispose()
        {
            _bloomFog.transition = 0;
            _bloomFog.transitionFogParams = null;
            UnityEngine.Object.Destroy(_transitionFogParams);
        }
    }
}
