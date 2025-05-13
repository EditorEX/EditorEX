using System;
using EditorEX.SDK.ReactiveComponents.Native;
using HMUI;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorLabel : ReactiveComponent, ISkewedComponent, IGraphic, ILeafLayoutItem
    {
        public string Text
        {
            get => _text.text;
            set
            {
                _text.text = value;
                NotifyPropertyChanged();
                RequestLeafRecalculation();
            }
        }

        public bool RichText
        {
            get => _text.richText;
            set
            {
                _text.richText = value;
                NotifyPropertyChanged();
            }
        }

        public float FontSize
        {
            get => _text.fontSize;
            set
            {
                _text.fontSize = value;
                NotifyPropertyChanged();
            }
        }

        public float FontSizeMin
        {
            get => _text.fontSizeMin;
            set
            {
                _text.fontSizeMin = value;
                NotifyPropertyChanged();
            }
        }

        public float FontSizeMax
        {
            get => _text.fontSizeMax;
            set
            {
                _text.fontSizeMax = value;
                NotifyPropertyChanged();
            }
        }

        public bool EnableAutoSizing
        {
            get => _text.enableAutoSizing;
            set
            {
                _text.enableAutoSizing = value;
                NotifyPropertyChanged();
            }
        }

        public FontStyles FontStyle
        {
            get => _text.fontStyle;
            set
            {
                _text.fontStyle = value;
                NotifyPropertyChanged();
            }
        }

        public TMP_FontAsset Font
        {
            get => _text.font;
            set
            {
                _text.font = value;
                NotifyPropertyChanged();
            }
        }

        public Material Material
        {
            get => _text.material;
            set
            {
                _text.material = value;
                NotifyPropertyChanged();
            }
        }

        public bool EnableWrapping
        {
            get => _text.enableWordWrapping;
            set
            {
                _text.enableWordWrapping = value;
                NotifyPropertyChanged();
            }
        }

        public TextOverflowModes Overflow
        {
            get => _text.overflowMode;
            set
            {
                _text.overflowMode = value;
                NotifyPropertyChanged();
            }
        }

        public TextAlignmentOptions Alignment
        {
            get => _text.alignment;
            set
            {
                _text.alignment = value;
                NotifyPropertyChanged();
            }
        }

        public Color Color
        {
            get => _text.color;
            set
            {
                _text.color = value;
                NotifyPropertyChanged();
            }
        }

        public bool RaycastTarget
        {
            get => _text.raycastTarget;
            set
            {
                _text.raycastTarget = value;
                NotifyPropertyChanged();
            }
        }

        float ISkewedComponent.Skew
        {
            get => FontStyle.HasFlag(FontStyles.Italic) ? 1f : 0f;
            set
            {
                if (value > 0f)
                {
                    FontStyle |= FontStyles.Italic;
                }
                else
                {
                    FontStyle &= ~FontStyles.Italic;
                }
            }
        }

        public TextMeshProUGUI TextMesh => _text;

        private TextMeshProUGUI _text = null!;
        private readonly ReactiveContainer _reactiveContainer;

        protected override void Construct(RectTransform rect)
        {
            _text = rect.gameObject.AddComponent<CurvedTextMeshPro>();
            _text.RegisterDirtyLayoutCallback(RequestLeafRecalculation);
        }

        protected override void OnInitialize()
        {
            FontSize = 12f;
            Alignment = TextAlignmentOptions.Center;
            EnableWrapping = false;
        }

        protected override void OnStart()
        {
            RequestLeafRecalculation();
            var container = Content.transform.GetComponentInParent<ReactiveContainerHolder>().ReactiveContainer;
            _text.fontSharedMaterial = container.FontCollector.GetMaterial();
            _text.font = container.FontCollector.GetFontAsset();
            _text.color = container.ColorCollector.GetColor("Button/Text/Normal");
        }

        public event Action<ILeafLayoutItem>? LeafLayoutUpdatedEvent;

        public Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            var measuredWidth = widthMode == MeasureMode.Undefined ? Mathf.Infinity : width;
            var measuredHeight = heightMode == MeasureMode.Undefined ? Mathf.Infinity : height;

            var textSize = _text.GetPreferredValues(measuredWidth, measuredHeight);

            return new()
            {
                x = widthMode == MeasureMode.Exactly ? width : Mathf.Min(textSize.x, measuredWidth),
                y = heightMode == MeasureMode.Exactly ? height : Mathf.Min(textSize.y, measuredHeight)
            };
        }

        private void RequestLeafRecalculation()
        {
            LeafLayoutUpdatedEvent?.Invoke(this);
            ScheduleLayoutRecalculation();
        }
    }
}