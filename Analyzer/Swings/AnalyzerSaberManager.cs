using BeatmapEditor3D.Commands;
using BeatmapEditor3D.DataModels;
using EditorEX.Analyzer.Swings.SwingBaker;
using EditorEX.Essentials.Patches;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace EditorEX.Analyzer.Swings
{
    public class AnalyzerSaberManager : MonoBehaviour, IInitializable, ITickable
    {
        private SaberManager _saberManager;
        private IReadonlyBeatmapState _state;

        private LevelUtils _levelUtils;

        private Saber _leftSaber;
        private Saber _rightSaber;

        private BakedSwingTrack _swingTrackLeft;
        private BakedSwingTrack _swingTrackRight;

        private SwingTrackGenerator _swingTrackGeneratorLeft;
        private SwingTrackGenerator _swingTrackGeneratorRight;

        [Inject]
        private void Construct(
            SaberManager saberManager,
            IReadonlyBeatmapState state,
            LevelUtils levelUtils)
        {
            _saberManager = saberManager;
            _leftSaber = _saberManager.leftSaber;
            _rightSaber = _saberManager.rightSaber;
            _state = state;
            _levelUtils = levelUtils;
        }

        public void Initialize()
        {
            Resources.FindObjectsOfTypeAll<CuttingManager>().FirstOrDefault().enabled = false;

            var notes = PopulateBeatmap._beatmapObjectsDataModel.allBeatmapObjects.OfType<NoteEditorData>().ToList();
            var obstacles = PopulateBeatmap._beatmapObjectsDataModel.allBeatmapObjects.OfType<ObstacleEditorData>().ToList();

            var sliceMapRight = new SliceMap(notes, obstacles, true);
            var sliceMapLeft = new SliceMap(notes, obstacles, false);
            
            _swingTrackGeneratorLeft = new SwingTrackGenerator(sliceMapLeft, false, _levelUtils);
            _swingTrackGeneratorRight = new SwingTrackGenerator(sliceMapRight, true, _levelUtils);
        }

        float _prevBeat = 999f;

        public void Tick()
        {
            _saberManager.transform.parent.gameObject.SetActive(true);
            if ((_swingTrackLeft == null || _swingTrackRight == null))
            {
                _swingTrackLeft = _swingTrackGeneratorLeft.GetTrack();
                _swingTrackRight = _swingTrackGeneratorRight.GetTrack();
                if ((_swingTrackLeft == null || _swingTrackRight == null))
                {
                    return;
                }
            }

            float _beatTime = _state.beat;

            if (!_state.isPlaying && _beatTime == _prevBeat)
            {
                _prevBeat = _beatTime;
                return;
            }

            _prevBeat = _beatTime;

            int leftFrame = (int)(PopulateBeatmap._audioDataModel.bpmData.BeatToSeconds(_beatTime) * 24f);
            int rightFrame = (int)(PopulateBeatmap._audioDataModel.bpmData.BeatToSeconds(_beatTime) * 24f);

            if (_swingTrackLeft.frames.Count > leftFrame)
            {
                var frame = _swingTrackLeft.frames[leftFrame];
                _leftSaber.transform.position = frame.position;
                _leftSaber.transform.rotation = frame.rotation;
            }

            if (_swingTrackRight.frames.Count > rightFrame)
            {
                var frame = _swingTrackRight.frames[rightFrame];
                _rightSaber.transform.position = frame.position;
                _rightSaber.transform.rotation = frame.rotation;
            }
        }
    }
}
