using EditorEX.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Collectors
{
    public class ColorCollector : IInitializable
    {
        internal Dictionary<string, SimpleColorSO> _colors = new();

        public void Initialize()
        {
            var colors = Resources.FindObjectsOfTypeAll<SimpleColorSO>();
            foreach (var color in colors)
            {
                if (color.name.StartsWith("BeatmapEditor"))
                {
                    string remappedName = color.name.Substring(14).Replace(".", "/");
                    _colors[remappedName] = color;
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var color in _colors)
            {
                var unityColor = color.Value.color;
                var r = (int)(unityColor.r * 255);
                var g = (int)(unityColor.g * 255);
                var b = (int)(unityColor.b * 255);
                var a = (int)(unityColor.a * 255);
                var (invR, invG, invB) = ColorUtil.GetAdjustedInverseColor(r, g, b);
                stringBuilder.AppendLine($"<span style=\"font-weight: bold; color:rgba({r}, {g}, {b}, {a}); background-color:rgba({invR}, {invG}, {invB});\">{color.Key}</span>");
            }

            File.WriteAllText("test.txt", stringBuilder.ToString());
        }

        public SimpleColorSO GetColor(string name)
        {
            if (!_colors.ContainsKey(name))
            {
                throw new ArgumentException($"Color {name} does not exist! Did you mispell something?");
            }
            return _colors[name];
        }
    }
}