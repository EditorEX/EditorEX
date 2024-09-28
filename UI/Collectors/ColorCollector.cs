using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace EditorEX.UI.Collectors
{
    public class ColorCollector : IInitializable
    {
        private Dictionary<string, SimpleColorSO> _colors = new();

        public void Initialize()
        {
            var colors = Resources.FindObjectsOfTypeAll<SimpleColorSO>();
            foreach (var color in colors)
            {
                if (color.name.StartsWith("BeatmapEditor"))
                {
                    string remappedName = color.name.Substring(14).Replace(".", "/");
                    Plugin.Log.Info(remappedName);
                    _colors[remappedName] = color;
                }
            }
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