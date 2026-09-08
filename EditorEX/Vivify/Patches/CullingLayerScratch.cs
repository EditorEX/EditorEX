using System;
using System.Collections.Generic;
using UnityEngine;
using Vivify.Controllers;

namespace EditorEX.Vivify.Patches
{
    internal sealed class CullingLayerProbe
    {
        internal CullingLayerProbe(int instanceId, bool alive, bool active, int layer)
        {
            InstanceId = instanceId;
            Alive = alive;
            ActiveInHierarchy = active;
            Layer = layer;
        }

        internal int InstanceId { get; }

        internal bool Alive { get; }

        internal bool ActiveInHierarchy { get; }

        internal int Layer { get; set; }
    }

    internal static class CullingLayerScratch
    {
        internal static bool Include(bool alive, bool activeInHierarchy)
        {
            return alive && activeInHierarchy;
        }

        internal static void Capture(
            IReadOnlyList<CullingLayerProbe> source,
            HashSet<int> seen,
            List<(CullingLayerProbe obj, int layer)> cache,
            int cullLayer
        )
        {
            seen.Clear();
            cache.Clear();
            int count = source.Count;
            for (int i = 0; i < count; i++)
            {
                CullingLayerProbe obj = source[i];
                if (!Include(obj.Alive, obj.ActiveInHierarchy) || !seen.Add(obj.InstanceId))
                {
                    continue;
                }

                int layer = obj.Layer;
                cache.Add((obj, layer));
                if (layer != cullLayer)
                {
                    obj.Layer = cullLayer;
                }
            }
        }

        internal static void Restore(List<(CullingLayerProbe obj, int layer)> cache)
        {
            int count = cache.Count;
            for (int i = 0; i < count; i++)
            {
                (CullingLayerProbe obj, int layer) = cache[i];
                obj.Layer = layer;
            }

            cache.Clear();
        }

        internal static void Capture(
            GameObject[] source,
            HashSet<int> seen,
            List<(GameObject obj, int layer)> cache,
            int cullLayer
        )
        {
            seen.Clear();
            cache.Clear();
            int length = source.Length;
            for (int i = 0; i < length; i++)
            {
                GameObject obj = source[i];
                if ((object)obj == null || !obj)
                {
                    continue;
                }

                if (!Include(true, obj.activeInHierarchy) || !seen.Add(obj.GetInstanceID()))
                {
                    continue;
                }

                int layer = obj.layer;
                cache.Add((obj, layer));
                if (layer != cullLayer)
                {
                    obj.layer = cullLayer;
                }
            }
        }

        internal static void Restore(List<(GameObject obj, int layer)> cache)
        {
            int count = cache.Count;
            for (int i = 0; i < count; i++)
            {
                (GameObject obj, int layer) = cache[i];
                if ((object)obj == null || !obj)
                {
                    continue;
                }

                obj.layer = layer;
            }

            cache.Clear();
        }

        internal static GameObject[] UniqueActive(HashSet<RendererController> maskRenderers)
        {
            HashSet<int> seen = new();
            List<GameObject> dest = new();
            foreach (RendererController controller in maskRenderers)
            {
                if ((object)controller == null || !controller)
                {
                    continue;
                }

                Renderer[] renderers = controller.ChildRenderers;
                int length = renderers.Length;
                for (int i = 0; i < length; i++)
                {
                    Renderer renderer = renderers[i];
                    if ((object)renderer == null || !renderer)
                    {
                        continue;
                    }

                    GameObject gameObject = renderer.gameObject;
                    if (
                        !Include(true, gameObject.activeInHierarchy)
                        || !seen.Add(gameObject.GetInstanceID())
                    )
                    {
                        continue;
                    }

                    dest.Add(gameObject);
                }
            }

            return dest.Count == 0 ? Array.Empty<GameObject>() : dest.ToArray();
        }
    }
}
