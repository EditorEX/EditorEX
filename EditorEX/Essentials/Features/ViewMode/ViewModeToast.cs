using System.Collections;
using System.Collections.Generic;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Attachable;
using EditorEX.SDK.ReactiveComponents.Native;
using Reactive;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.Essentials.Features.ViewMode
{
    internal class ViewModeToast : ReactiveComponent
    {
        private const string InactiveTextColor = "Button/Text/Normal";
        private const string ActiveTextColor = "Button/Text/Highlighted";
        private const string PanelColor = "VerticalList/Background/Pressed";
        private const string ArrowColor = "Button/Text/Highlighted";

        private const float ArrowLerpSeconds = 0.18f;
        private const float FadeSeconds = 0.12f;
        private const float LabelGap = 28f;
        private const float PanelPadY = 9f;
        private const float PanelPadX = 20f;
        private const float SlotExtraPad = 16f;
        private const float ArrowDisplayWidth = 22f;
        private const float ArrowDisplayHeight = 15f;
        private const float ArrowGapAbovePanel = 6f;
        private const float FontSize = 18f;

        private AnimatedState<float> _arrowX = null!;
        private AnimatedState<float> _alpha = null!;
        private State<int> _selectedIndex = null!;

        private readonly List<EditorLabel> _labels = new();
        private readonly List<float> _labelCentersX = new();
        private EditorImage? _arrow;
        private EditorBackground _panel = null!;
        private CanvasGroup _canvasGroup = null!;
        private bool _visible;
        private bool _slotsLocked;

        public bool IsVisible => _visible;

        public void ShowOrUpdate(ViewMode mode, ViewMode? previousMode)
        {
            var modes = ViewModeRepository.GetAll();
            var index = IndexOf(modes, mode);
            if (index < 0)
            {
                return;
            }

            var previousIndex = previousMode != null ? IndexOf(modes, previousMode) : index;
            if (previousIndex < 0)
            {
                previousIndex = index;
            }

            EnsureSlotsLocked();

            _selectedIndex.Value = index;
            ApplyLabelStyles(index);
            RefreshLabelCenters();

            var targetX = GetCachedLabelCenterX(index);

            if (!_visible)
            {
                _arrowX.SetValueImmediate(GetCachedLabelCenterX(previousIndex));
                _alpha.SetValueImmediate(0f);
                _visible = true;
                Content.SetActive(true);
            }

            _arrowX.TargetValue = targetX;
            _alpha.TargetValue = 1f;
        }

        public void BeginHide()
        {
            if (!_visible)
            {
                return;
            }

            _alpha.OnFinish = _ =>
            {
                _visible = false;
                if (!IsDestroyed)
                {
                    Content.SetActive(false);
                }
                _alpha.OnFinish = null;
            };
            _alpha.TargetValue = 0f;
        }

        protected override GameObject Construct()
        {
            _selectedIndex = Remember(0);
            _arrowX = RememberAnimated(
                0f,
                new AnimationDuration(ArrowLerpSeconds, Reactive.DurationUnit.Seconds)
            );
            _alpha = RememberAnimated(
                0f,
                new AnimationDuration(FadeSeconds, Reactive.DurationUnit.Seconds)
            );

            var modes = ViewModeRepository.GetAll();
            _labels.Clear();

            var labelChildren = new LayoutChildren();
            foreach (var mode in modes)
            {
                var label = new EditorLabel
                {
                    Text = mode.DisplayName,
                    FontSize = FontSize,
                    Alignment = TextAlignmentOptions.Center,
                    RaycastTarget = false,
                    EnableWrapping = false,
                }.AsFlexItem();
                _labels.Add(label);
                labelChildren.Add(label);
            }

            // Arrow is created in OnInitialize outside Yoga so layout cannot fight its position.
            var panel = labelChildren
                .As<EditorBackground>(x =>
                {
                    x.Source = "#Background8px";
                    x.ImageType = Image.Type.Sliced;
                    x.RaycastTarget = false;
                })
                .With(x => x.WrappedImage.Attach<ColorSOAttachable>(PanelColor))
                .AsFlexGroup(
                    direction: FlexDirection.Row,
                    justifyContent: Justify.Center,
                    alignItems: Align.Center,
                    padding: new YogaFrame(PanelPadY, PanelPadX),
                    gap: LabelGap,
                    constrainHorizontal: false,
                    constrainVertical: false
                )
                .AsFlexItem(size: "max-content")
                .Bind(ref _panel);

            return new Layout { Children = { panel } }
                .AsFlexGroup(
                    direction: FlexDirection.Column,
                    alignItems: Align.Center,
                    constrainHorizontal: false,
                    constrainVertical: false
                )
                .AsFlexItem(size: "max-content")
                .Use();
        }

        protected override void OnInitialize()
        {
            _canvasGroup =
                Content.GetComponent<CanvasGroup>() ?? Content.AddComponent<CanvasGroup>();
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            _canvasGroup.alpha = 0f;

            _arrow = new EditorImage { Sprite = CreateDownArrowSprite(), RaycastTarget = false };
            _arrow.Use(ContentTransform);
            _arrow.Attach<ColorSOAttachable>(ArrowColor);

            var arrowRect = _arrow.ContentTransform;
            arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
            arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.sizeDelta = new Vector2(ArrowDisplayWidth, ArrowDisplayHeight);
            arrowRect.localScale = Vector3.one;
            arrowRect.SetAsLastSibling();

            Content.SetActive(false);
            ApplyLabelStyles(0);
        }

        protected override void OnStart()
        {
            base.OnStart();
            StartCoroutine(LockSlotsNextFrame());
        }

        protected override void OnUpdate()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _alpha.Value;
            }

            UpdateArrowPosition();
        }

        private void UpdateArrowPosition()
        {
            if (_arrow == null || !_arrow.IsInitialized || _panel == null || !_panel.IsInitialized)
            {
                return;
            }

            var panel = _panel.ContentTransform;
            // World-space top-center of the panel, then into toast local space.
            var panelTopLocal = ContentTransform.InverseTransformPoint(
                panel.TransformPoint(new Vector3(panel.rect.center.x, panel.rect.yMax, 0f))
            );

            var arrowRect = _arrow.ContentTransform;
            arrowRect.sizeDelta = new Vector2(ArrowDisplayWidth, ArrowDisplayHeight);
            // Tip sits just above the panel; center is half the arrow height above the tip.
            arrowRect.localPosition = new Vector3(
                _arrowX.Value,
                panelTopLocal.y + ArrowDisplayHeight * 0.5f + ArrowGapAbovePanel,
                0f
            );
        }

        private IEnumerator LockSlotsNextFrame()
        {
            yield return null;
            EnsureSlotsLocked();
        }

        private void EnsureSlotsLocked()
        {
            if (_slotsLocked || _labels.Count == 0)
            {
                return;
            }

            foreach (var label in _labels)
            {
                if (!label.IsInitialized || label.TextMesh.font == null)
                {
                    return;
                }
            }

            // Preferred sizes can be wrong while inactive — measure with content enabled.
            var wasActive = Content.activeSelf;
            Content.SetActive(true);

            float totalWidth = PanelPadX * 2f;
            float maxHeight = 0f;

            for (var i = 0; i < _labels.Count; i++)
            {
                var label = _labels[i];
                label.FontStyle = FontStyles.Bold;
                label.TextMesh.ForceMeshUpdate();
                var preferred = label.TextMesh.GetPreferredValues(label.Text);
                var slotW = Mathf.Ceil(preferred.x) + SlotExtraPad;
                var slotH = Mathf.Ceil(preferred.y) + 4f;
                maxHeight = Mathf.Max(maxHeight, slotH);

                if (label.LayoutModifier is YogaModifier modifier)
                {
                    var size = new YogaVector(slotW, slotH);
                    modifier.Size = size;
                    modifier.MinSize = size;
                    modifier.MaxSize = size;
                }

                totalWidth += slotW;
                if (i < _labels.Count - 1)
                {
                    totalWidth += LabelGap;
                }
            }

            var panelHeight = maxHeight + PanelPadY * 2f;
            if (_panel.LayoutModifier is YogaModifier panelMod)
            {
                var panelSize = new YogaVector(totalWidth, panelHeight);
                panelMod.MinSize = panelSize;
                panelMod.Size = panelSize;
            }

            ApplyLabelStyles(_selectedIndex.Value);
            ScheduleLayoutRecalculation();
            Content.SetActive(wasActive);

            _slotsLocked = true;
            StartCoroutine(CacheCentersAfterLayout());
        }

        private IEnumerator CacheCentersAfterLayout()
        {
            var wasActive = Content.activeSelf;
            Content.SetActive(true);
            yield return null;
            RefreshLabelCenters();
            Content.SetActive(wasActive);
        }

        private void RefreshLabelCenters()
        {
            _labelCentersX.Clear();
            for (var i = 0; i < _labels.Count; i++)
            {
                _labelCentersX.Add(MeasureLabelCenterX(i));
            }
        }

        private void ApplyLabelStyles(int activeIndex)
        {
            var colors = Content
                .GetComponentInParent<ReactiveContainerHolder>()
                ?.ReactiveContainer.ColorCollector;
            if (colors == null)
            {
                return;
            }

            for (var i = 0; i < _labels.Count; i++)
            {
                var active = i == activeIndex;
                _labels[i].ColorSO = colors.GetColor(active ? ActiveTextColor : InactiveTextColor);
                _labels[i].FontStyle = active ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        private float GetCachedLabelCenterX(int index)
        {
            if (index >= 0 && index < _labelCentersX.Count)
            {
                return _labelCentersX[index];
            }

            return MeasureLabelCenterX(index);
        }

        private float MeasureLabelCenterX(int index)
        {
            if (index < 0 || index >= _labels.Count || !_labels[index].IsInitialized)
            {
                return 0f;
            }

            var label = _labels[index].ContentTransform;
            var local = ContentTransform.InverseTransformPoint(
                label.TransformPoint(label.rect.center)
            );
            return local.x;
        }

        private static int IndexOf(IReadOnlyList<ViewMode> modes, ViewMode mode)
        {
            for (var i = 0; i < modes.Count; i++)
            {
                if (modes[i].ID == mode.ID)
                {
                    return i;
                }
            }

            return -1;
        }

        private static Sprite CreateDownArrowSprite()
        {
            const int w = 96;
            const int h = 68;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0,
            };

            var cx = (w - 1) * 0.5f;
            for (var y = 0; y < h; y++)
            {
                var t = h == 1 ? 0f : (float)y / (h - 1);
                var halfWidth = Mathf.Lerp(0.5f, (w - 1) * 0.5f, t);
                for (var x = 0; x < w; x++)
                {
                    var dist = Mathf.Abs(x - cx) - halfWidth;
                    var alpha = 1f - Mathf.Clamp01(dist + 0.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
