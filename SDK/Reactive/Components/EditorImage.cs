using System;
using System.Linq;
using BeatSaberMarkupLanguage;
using HMUI;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace EditorEX.SDK.Reactive.Components
{
    public class EditorImage : ReactiveComponent, IComponentHolder<EditorImage>, ISkewedComponent, IGraphic, ILeafLayoutItem
    {
        public Sprite? Sprite
        {
            get => _image.sprite;
            set
            {
                _image.sprite = value;
                NotifyPropertyChanged();
            }
        }

        public string Source
        {
            set
            {
                _image.SetImageAsync(value, false);
                NotifyPropertyChanged();
            }
        }

        public Color Color
        {
            get => _image.color;
            set
            {
                _image.color = value;
                NotifyPropertyChanged();
            }
        }

        public ColorSO ColorSO
        {
            get => _image._colorSo;
            set
            {
                _image._colorSo = value;
                NotifyPropertyChanged();
            }
        }

        public bool UseScriptableObjectColors
        {
            get => _image._useScriptableObjectColors;
            set
            {
                _image._useScriptableObjectColors = value;
                NotifyPropertyChanged();
            }
        }

        public Color GradientColor0
        {
            get => _image.color0;
            set
            {
                _image.color0 = value;
                NotifyPropertyChanged();
            }
        }

        public Color GradientColor1
        {
            get => _image.color1;
            set
            {
                _image.color1 = value;
                NotifyPropertyChanged();
            }
        }

        public bool UseGradient
        {
            get => _image.gradient;
            set
            {
                _image.gradient = value;
                NotifyPropertyChanged();
            }
        }

        public ImageView.GradientDirection GradientDirection
        {
            get => _image.GradientDirection;
            set
            {
                _image.GradientDirection = value;
                NotifyPropertyChanged();
            }
        }

        public Material? Material
        {
            get => _image.material;
            set
            {
                _image.material = value;
                NotifyPropertyChanged();
            }
        }

        public bool PreserveAspect
        {
            get => _image.preserveAspect;
            set
            {
                _image.preserveAspect = value;
                NotifyPropertyChanged();
            }
        }

        public UnityEngine.UI.Image.Type ImageType
        {
            get => _image.type;
            set
            {
                _image.type = value;
                NotifyPropertyChanged();
            }
        }

        public UnityEngine.UI.Image.FillMethod FillMethod
        {
            get => _image.fillMethod;
            set
            {
                _image.fillMethod = value;
                NotifyPropertyChanged();
            }
        }

        public float FillAmount
        {
            get => _image.fillAmount;
            set
            {
                _image.fillAmount = value;
                NotifyPropertyChanged();
            }
        }

        public float PixelsPerUnit
        {
            get => _image.pixelsPerUnitMultiplier;
            set
            {
                ImageType = UnityEngine.UI.Image.Type.Sliced;
                _image.pixelsPerUnitMultiplier = value;
                NotifyPropertyChanged();
            }
        }

        public float Skew
        {
            get => _image.Skew;
            set => _image.Skew = value;
        }

        public bool RaycastTarget
        {
            get => _image.raycastTarget;
            set => _image.raycastTarget = value;
        }

        EditorImage IComponentHolder<EditorImage>.Component => this;
        public FixedImageView ImageView => _image;

        private FixedImageView _image = null!;

        protected override void Construct(RectTransform rect)
        {
            _image = rect.gameObject.AddComponent<FixedImageView>();
        }

        public event Action<ILeafLayoutItem>? LeafLayoutUpdatedEvent;

        public Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            var measuredWidth = widthMode == MeasureMode.Undefined ? Mathf.Infinity : width;
            var measuredHeight = heightMode == MeasureMode.Undefined ? Mathf.Infinity : height;

            return new()
            {
                x = widthMode == MeasureMode.Exactly ? width : Mathf.Min(_image.preferredWidth, measuredWidth),
                y = heightMode == MeasureMode.Exactly ? height : Mathf.Min(_image.preferredHeight, measuredHeight)
            };
        }
    }
}