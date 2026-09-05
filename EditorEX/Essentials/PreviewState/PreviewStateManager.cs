using System;
using System.Collections.Generic;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.PreviewState
{
    internal class PreviewStateManager
        : MonoBehaviour,
            IPreviewStateRegistry,
            IInitializable,
            IDisposable
    {
        private PreviewStateScheduler _scheduler = null!;
        private IReadonlyBeatmapState _beatmapState = null!;
        private List<IPreviewStateSource> _sources = null!;
        private bool _ready;
        private float _prevBeat = 9999f;

        [Inject]
        private void Construct(
            EditorGameplayCoreSceneSetupData setupData,
            [InjectOptional] List<IPreviewStateSource> sources,
            SiraLog log
        )
        {
            _beatmapState = setupData.beatmapState;
            _sources = sources ?? new List<IPreviewStateSource>();
            _scheduler = new PreviewStateScheduler(e => log.Error(e));
        }

        public void Add(
            float fromBeat,
            float toBeat,
            IPreviewStateAction action,
            bool alreadyExecuted = false
        )
        {
            _scheduler.Add(fromBeat, toBeat, action, alreadyExecuted);
        }

        public void Refresh()
        {
            if (!_ready)
            {
                return;
            }

            _prevBeat = _beatmapState.beat;
            _scheduler.Apply(_beatmapState.beat);
        }

        public void Initialize()
        {
            foreach (IPreviewStateSource source in _sources)
            {
                source.Build(this);
            }

            _scheduler.Apply(_beatmapState.beat);
            _prevBeat = _beatmapState.beat;
            _ready = true;
        }

        private void Update()
        {
            if (!_ready)
            {
                return;
            }

            // Match object controllers: beatmapState.prevBeat only updates while playing or
            // scrubbing, so use our own prevBeat and always run while the playhead is moving.
            if (!_beatmapState.isPlaying && _prevBeat == _beatmapState.beat)
            {
                return;
            }

            _prevBeat = _beatmapState.beat;
            _scheduler.Apply(_beatmapState.beat);
        }

        public void Dispose()
        {
            _ready = false;
            _scheduler.ReverseAll();
        }
    }
}
