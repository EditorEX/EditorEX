using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EditorEX.UI
{
    public static class BasicUI
    {
        private static SimpleColorSO _bgColors;

        private static void CollectColors()
        {
            var colors = Resources.FindObjectsOfTypeAll<SimpleColorSO>();

            _bgColors = colors.FirstOrDefault(x => x.name == "BeatmapEditor.Navbar.Background.Normal");
        }


        private static SimpleColorSO BGColors { get { if (_bgColors == null) CollectColors(); return _bgColors; } }

        public static ImageView MakeBackground(Transform parent)
        {
            var obj = new GameObject("EditorBackground").AddComponent<ImageView>();
            obj.SetImageAsync("#Background8px");
            obj.type = ImageView.Type.Sliced;
            obj.SetField("_colorSo", BGColors as ColorSO);
            obj.useScriptableObjectColors = true;
            obj.transform.SetParent(parent, false);
            return obj;
        }
    }
}
