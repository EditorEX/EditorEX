using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace BetterEditor.Util
{
    public static class ShaderKeywordExtensions
    {
        public static void EnableGameArc(this LocalKeyword[] keywords, Material material)
        {
            List<LocalKeyword> list = keywords.ToList();
            list.RemoveAll(x => x.name == "BEATMAP_EDITOR_ONLY");
            list.Add(new LocalKeyword(material.shader, "PRECISE_FOG"));
            list.Add(new LocalKeyword(material.shader, "_FOGTYPE_ALPHA"));
            material.enabledKeywords = list.ToArray();
        }

        public static void DisableGameArc(this LocalKeyword[] keywords, Material material)
        {
            List<LocalKeyword> list = keywords.ToList();
            list.RemoveAll(x => x.name == "PRECISE_FOG" || x.name == "_FOGTYPE_ALPHA");
            list.Add(new LocalKeyword(material.shader, "BEATMAP_EDITOR_ONLY"));
            material.enabledKeywords = list.ToArray();
        }
    }
}