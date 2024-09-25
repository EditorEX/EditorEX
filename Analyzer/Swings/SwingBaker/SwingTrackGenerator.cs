using EditorEX.Essentials.Patches;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EditorEX.Analyzer.Swings.SwingBaker
{
    public class SwingTrackGenerator
    {
        private SliceMap _sliceMap;
        private List<HandFrame> _handFrames;
        private BakedSwingTrack _track;

        private LevelUtils _levelUtils;

        private int totalFrames;
        private float secondsPerFrame = 1 / 24f;

        public SwingTrackGenerator(SliceMap sliceMap, bool rightHand, LevelUtils levelUtils)
        {
            _levelUtils = levelUtils;
            _sliceMap = sliceMap;
            _track = new BakedSwingTrack();
            totalFrames = (int)Mathf.Ceil(PopulateBeatmap._beatmapDataModel.bpmData.BeatToSeconds(_sliceMap.Cuts.Last().sliceEndBeat) * 24);

            var handFrames = ProcessHandFrames(rightHand);
            _handFrames = handFrames;
            _track.frames = ProcessFrames(handFrames);
        }

        public BakedSwingTrack GetTrack()
        {
            return _track;
        }

        private float _maxWristOrientDrag = 120.0f;
        private float _maxPalmOrientDrag = 50.0f;
        private float _maxWristPositDrag = 120.0f;
        private float _minWristOrientDrag = 40.0f;
        private float _minPalmOrientDrag = 30.0f;
        private float _minWristPositDrag = 40.0f;

        private float _restingWristOrientation;
        private float _targetWristOrientation;
        private float _targetPalmOrientation;
        private float _targetWristOrientDrag;
        private float _targetPalmOrientDrag;
        private float _targetWristPositDrag;
        private Vector2 _restingWristPosition;
        private Vector2 _targetWristPosition;

        private void SetTargetWristOrientation(float a, HandFrame lastFrame)
        {
            if (lastFrame._wristOrientation - a > 180.0f)
            {
                a -= 360.0f;
            }
            _targetWristOrientation = a;
        }

        private void SetTimeToNextBeat(float t, HandFrame lastFrame)
        {
            if (t < 0.2f)
            {
                _targetWristOrientDrag = _minWristOrientDrag;
                _targetPalmOrientDrag = _minPalmOrientDrag;
                _targetWristPositDrag = _minWristPositDrag;
            }
            else
            {
                _levelUtils.GetWorldXYFromBeatmapCoords((int)_restingWristPosition.x, (int)_restingWristPosition.y);
                SetTargetWristOrientation(_restingWristOrientation, lastFrame);

                float u = Mathf.Clamp((t - 0.2f), 0.0f, 1.0f);

                _targetWristOrientDrag = Mathf.Lerp(_minWristOrientDrag, _maxWristOrientDrag, u) / Time.timeScale;
                _targetPalmOrientDrag = Mathf.Lerp(_minPalmOrientDrag, _maxPalmOrientDrag, u) / Time.timeScale;
                _targetWristPositDrag = Mathf.Lerp(_minWristPositDrag, _maxWristPositDrag, u) / Time.timeScale;
            }
        }

        private List<HandFrame> ProcessHandFrames(bool rightHand)
        {
            int _sliceIndex = 0;
            float _startBeatOffset = 0f;

            _restingWristPosition = _levelUtils.GetWorldXYFromBeatmapCoords(rightHand ? 3 : 0, 1);
            _targetWristPosition = _levelUtils.GetWorldXYFromBeatmapCoords(rightHand ? 3 : 0, 1);

            List<HandFrame> handFrames = new List<HandFrame>(totalFrames);
            HandFrame lastFrame = new HandFrame()
            {
                _wristOrientDrag = _maxWristOrientDrag,
                _palmOrientDrag = _maxPalmOrientDrag,
                _wristPositDrag = _maxWristPositDrag,
                _wristOrientation = -10f,
                _palmOrientation = 0.0f,    
                _wristPosition = _targetWristPosition
            };

            _targetWristOrientDrag = _maxWristOrientDrag;
            _targetPalmOrientDrag = _maxPalmOrientDrag;
            _targetWristPositDrag = _maxWristPositDrag;

            handFrames.Add(lastFrame);
            for (int i = 1; i < totalFrames-1; i++)
            {
                float second = i * secondsPerFrame;
                float _beatTime = PopulateBeatmap._beatmapDataModel.bpmData.SecondsToBeat(second);

                float _timeToReachSabers = -Mathf.Abs((Mathf.Abs(-30f) - 3.0f) / 75f);

                float _beatTimeToReachSabers = PopulateBeatmap._beatmapDataModel.bpmData.SecondsToBeat(_timeToReachSabers);
                float _beatTimeToPrepareSwing = _beatTimeToReachSabers * 0.5f;

                if (_sliceIndex < _sliceMap.GetSliceCount())
                {
                    BeatCutData cutData = _sliceMap.GetBeatCutData(_sliceIndex);
                    if (_beatTime > cutData.sliceStartBeat - _startBeatOffset + _beatTimeToPrepareSwing)
                    {
                        _targetWristPosition = _levelUtils.GetWorldXYFromBeatmapCoords(cutData.startPositioning.x, cutData.startPositioning.y);
                        SetTargetWristOrientation(cutData.startPositioning.angle * -1, lastFrame);
                    }
                    if (_beatTime > cutData.sliceStartBeat - _startBeatOffset + _beatTimeToReachSabers)
                    {
                        _targetPalmOrientation = cutData.sliceParity == Parity.Forehand ? 180.0f : 0.0f;
                    }
                    if (_beatTime > cutData.sliceEndBeat - _startBeatOffset + _beatTimeToReachSabers)
                    {
                        ++_sliceIndex;
                        if (_sliceIndex < _sliceMap.GetSliceCount())
                        {
                            BeatCutData nextCutData = _sliceMap.GetBeatCutData(_sliceIndex);
                            float timeTilNextBeat = PopulateBeatmap._beatmapDataModel.bpmData.BeatToSeconds(nextCutData.sliceStartBeat - cutData.sliceEndBeat);
                            SetTimeToNextBeat(timeTilNextBeat, lastFrame);
                        }
                    }
                }

                var frame = new HandFrame();
                frame._wristOrientDrag = lastFrame._wristOrientDrag - ((lastFrame._wristOrientDrag - _targetWristOrientDrag) / 30f);
                frame._palmOrientDrag = lastFrame._palmOrientDrag - ((lastFrame._palmOrientDrag - _targetPalmOrientDrag) / 5f);
                frame._wristPositDrag = lastFrame._wristPositDrag - ((lastFrame._wristPositDrag - _targetWristPositDrag) / 10f);
                frame._wristOrientation = lastFrame._wristOrientation - ((lastFrame._wristOrientation - (_targetWristOrientation + 5.0f * Mathf.Sin(second))) / lastFrame._wristOrientDrag);
                frame._palmOrientation = lastFrame._palmOrientation - ((lastFrame._palmOrientation - (_targetPalmOrientation + 5.0f * Mathf.Cos(second))) / lastFrame._palmOrientDrag);
                frame._wristPosition = lastFrame._wristPosition - ((lastFrame._wristPosition - _targetWristPosition) / lastFrame._wristPositDrag);
                frame._targetWristPosition = _targetWristPosition;
                frame._timeToReachSabers = _timeToReachSabers;

                lastFrame = frame;

                handFrames.Add(frame);
            }

            return handFrames;
        }

        private struct HandFrame
        {
            public float _wristOrientDrag;
            public float _palmOrientDrag;
            public float _wristPositDrag;
            public float _wristOrientation;
            public float _palmOrientation;
            public Vector2 _wristPosition;
            public Vector2 _targetWristPosition;
            public float _timeToReachSabers;
        }

        private List<Frame> ProcessFrames(List<HandFrame> handFrames)
        {
            //Use temp objects to simulate hierarchy for rotations. TODO: Figure out the math the combine quaternions without this
            GameObject wrist = new GameObject();
            GameObject palm = new GameObject();
            palm.transform.parent = wrist.transform;
            GameObject saber = new GameObject();
            saber.transform.parent = palm.transform;
            saber.transform.eulerAngles = new Vector3(-90f, 0f, 0f);

            List<Frame> frames = new List<Frame>(totalFrames);

            foreach (var handFrame in handFrames)
            {
                Vector3 pos = wrist.transform.position;
                pos.x = handFrame._wristPosition.x;
                pos.y = handFrame._wristPosition.y;
                pos.z = -0f;
                wrist.transform.position = pos;
                wrist.transform.localRotation = Quaternion.AngleAxis(handFrame._wristOrientation, Vector3.forward);
                palm.transform.localRotation = Quaternion.AngleAxis(handFrame._palmOrientation, Vector3.right);
                frames.Add(new Frame() { position = saber.transform.position, rotation = saber.transform.rotation });
            }

            Object.Destroy(wrist);

            return frames;
        }
    }
}
