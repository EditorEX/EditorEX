using System.IO;
using System.Threading;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using BeatSaberMarkupLanguage;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using Reactive;
using Reactive.Components;
using SFB;
using UnityEngine;
using Zenject;

namespace EditorEX.UI.Patches.EditBeatmapLevel
{
    /// <summary>
    /// Picks, loads, and validates the level's cover image for
    /// <see cref="EditBeatmapLevelViewController"/>, mirroring the checks performed by
    /// <see cref="CoverImageInputView"/>.
    /// </summary>
    internal class EditBeatmapLevelCoverController
    {
        private const int CoverMinResolution = 256;

        private readonly LazyInject<BeatmapProjectManager> _beatmapProjectManager;

        private EditorClickableImage? _coverImage;

        public EditBeatmapLevelCoverController(
            LazyInject<BeatmapProjectManager> beatmapProjectManager
        )
        {
            _beatmapProjectManager = beatmapProjectManager;
        }

        public EditorClickableImage CreateCoverImageComponent(
            EditBeatmapLevelViewController controller
        )
        {
            return new EditorClickableImage
            {
                PreserveAspect = true,
                OnClick = () => PickCoverImage(controller),
            }.Bind(ref _coverImage);
        }

        private void PickCoverImage(EditBeatmapLevelViewController controller)
        {
            var folder = _beatmapProjectManager.Value._workingBeatmapProject ?? string.Empty;
            var picked = NativeFileDialogs.OpenFileDialog(
                "Select Cover Image",
                new ExtensionFilter[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") },
                folder
            );
            if (string.IsNullOrEmpty(picked))
            {
                return;
            }

            LoadCoverImage(picked, triggerUpdate: true, controller);
        }

        /// <summary>
        /// Loads a cover the same way <see cref="CoverImageInputView"/> does:
        /// <see cref="MediaAsyncLoader.LoadSpriteAsync"/>, square + min-resolution checks,
        /// then optionally fires <see cref="BeatmapDataModelSignals.UpdateBeatmapCoverImageSignal"/>.
        /// </summary>
        public async void LoadCoverImage(
            string? path,
            bool triggerUpdate,
            EditBeatmapLevelViewController? controller = null
        )
        {
            if (_coverImage == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                _coverImage.Sprite = null;
                return;
            }

            var sprite = await MediaAsyncLoader.LoadSpriteAsync(path, CancellationToken.None);
            if (_coverImage == null)
            {
                return;
            }

            if (sprite == null)
            {
                return;
            }

            var tex = sprite.texture;
            var notSquare = tex.width != tex.height;
            var tooSmall = tex.width < CoverMinResolution || tex.height < CoverMinResolution;
            if (notSquare || tooSmall)
            {
                return;
            }

            _coverImage.Sprite = sprite;
            _coverImage.Color = Color.white;

            if (triggerUpdate && controller != null)
            {
                controller._signalBus.Fire(
                    new BeatmapDataModelSignals.UpdateBeatmapCoverImageSignal(path)
                );
            }
        }
    }
}
