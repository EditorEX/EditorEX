using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vivify.ObjectPrefab.Hijackers;

namespace EditorEX.Vivify.ObjectPrefab;

public class EditorNoteMpbHijacker : IHijacker<GameObject>
{
    private readonly Transform _child;
    private readonly MaterialPropertyBlockController _materialPropertyBlockController;
    private readonly Renderer?[] _originalRenderers;
    private Renderer[]? _cachedRenderers;

    internal EditorNoteMpbHijacker(Component component)
    {
        _originalRenderers = component.GetComponentsInChildren<Renderer>();
        _child = component.transform.parent;

        _materialPropertyBlockController =
            component.GetComponent<MaterialPropertyBlockController>();
    }

    public void Activate(List<GameObject> gameObjects, bool hideOriginal)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            gameObject.transform.SetParent(_child, false);
        }

        _cachedRenderers = _materialPropertyBlockController._renderers;
        IEnumerable<Renderer> newRenderers = gameObjects.SelectMany(n =>
            n.GetComponentsInChildren<Renderer>(true)
        );

        if (hideOriginal)
        {
            foreach (Renderer? renderer in _originalRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }

            _materialPropertyBlockController._renderers = newRenderers.ToArray();
        }
        else
        {
            _materialPropertyBlockController._renderers = _cachedRenderers
                .Concat(newRenderers)
                .ToArray();
        }

        _materialPropertyBlockController.ApplyChanges();
    }

    public void Deactivate()
    {
        if (_cachedRenderers != null)
        {
            _materialPropertyBlockController._renderers = _cachedRenderers;
            _cachedRenderers = null;
        }

        foreach (Renderer? renderer in _originalRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }
}
