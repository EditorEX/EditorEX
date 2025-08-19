using Chroma;
using EditorEX.Chroma.Events;
using EditorEX.CustomJSONData;
using EditorEX.Heck.Deserialize;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.Chroma.Patches.Events
{
    internal class EditorLightPairRotationChromafier : IAffinity
    {
        private readonly EditorDeserializedData _editorDeserializedData;

        private BasicBeatmapEventData? _lastData;

        private EditorLightPairRotationChromafier(
            [InjectOptional(Id = ChromaController.ID)] EditorDeserializedData deserializedData
        )
        {
            _editorDeserializedData = deserializedData;
        }

        // Laser rotation
        [AffinityPrefix]
        [AffinityPatch(
            typeof(LightPairRotationEventEffect),
            nameof(LightPairRotationEventEffect.HandleBeatmapEvent)
        )]
        private void LoadEventData(
            BasicBeatmapEventData basicBeatmapEventData,
            BasicBeatmapEventType ____eventL,
            BasicBeatmapEventType ____eventR
        )
        {
            if (
                basicBeatmapEventData.basicBeatmapEventType == ____eventL
                || basicBeatmapEventData.basicBeatmapEventType == ____eventR
            )
            {
                _lastData = basicBeatmapEventData;
            }
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(LightPairRotationEventEffect),
            nameof(LightPairRotationEventEffect.HandleBeatmapEvent)
        )]
        private void ResetEventData()
        {
            _lastData = null;
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(LightPairRotationEventEffect),
            nameof(LightPairRotationEventEffect.UpdateRotationData)
        )]
        private bool Prefix(
            BasicBeatmapEventType ____eventL,
            float startRotationOffset,
            float direction,
            LightPairRotationEventEffect.RotationData ____rotationDataL,
            LightPairRotationEventEffect.RotationData ____rotationDataR,
            Vector3 ____rotationVector
        )
        {
            if (
                _lastData == null
                || !_editorDeserializedData.Resolve(
                    CustomDataRepository.GetBasicEventConversion(_lastData),
                    out EditorChromaEventData? chromaData
                )
            )
            {
                return true;
            }

            bool isLeftEvent = _lastData.basicBeatmapEventType == ____eventL;

            LightPairRotationEventEffect.RotationData rotationData = isLeftEvent
                ? ____rotationDataL
                : ____rotationDataR;

            bool lockPosition = chromaData.LockPosition;
            float precisionSpeed = chromaData.Speed.GetValueOrDefault(_lastData.value);
            int? dir = chromaData.Direction;

            direction = dir switch
            {
                0 => isLeftEvent ? -1 : 1,
                1 => isLeftEvent ? 1 : -1,
                _ => direction,
            };

            // Actual lasering
            Transform transform = rotationData.transform;
            Quaternion startRotation = rotationData.startRotation;
            float startRotationAngle = rotationData.startRotationAngle;
            switch (_lastData.value)
            {
                case 0:
                    {
                        rotationData.enabled = false;
                        if (!lockPosition)
                        {
                            rotationData.rotationAngle = startRotationAngle;
                            transform.localRotation =
                                startRotation
                                * Quaternion.Euler(____rotationVector * startRotationAngle);
                        }

                        break;
                    }

                case > 0:
                    {
                        rotationData.enabled = true;
                        rotationData.rotationSpeed = precisionSpeed * 20f * direction;
                        if (!lockPosition)
                        {
                            float rotationAngle = startRotationOffset + startRotationAngle;
                            rotationData.rotationAngle = rotationAngle;
                            transform.localRotation =
                                startRotation * Quaternion.Euler(____rotationVector * rotationAngle);
                        }

                        break;
                    }
            }

            return false;
        }
    }
}
