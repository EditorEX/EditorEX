using TMPro;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorHeaderLabel : EditorLabel
    {
        public override string Text
        {
            get => _textString;
            set
            {
                _textString = value;
                _text.text = $"<uppercase>{value}</uppercase>";
                RequestLeafRecalculationOnDirty();
            }
        }
        public override FontStyles FontStyle
        {
            get => _text.fontStyle;
            set
            {
                _text.fontStyle = value.HasFlag(FontStyles.Bold) ? value : value | FontStyles.Bold;
            }
        }

        private string _textString = string.Empty;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Alignment = TextAlignmentOptions.Left;
            FontSize = 20f;
        }
    }
}
