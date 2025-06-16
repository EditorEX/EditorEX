using System;
using System.Linq;
using EditorEX.HierarchyTraverser.Modifiers;
using UnityEngine;

namespace EditorEX.HierarchyTraverser
{
    public class TraverserNode<T> : ITraversable
        where T : TraverserNode<T>
    {
        private GameObject _gameObject;

        public TraverserNode(GameObject gameObject)
        {
            _gameObject = gameObject;
            Fill();
        }

        private T Fill()
        {
            var properties = typeof(T)
                .GetProperties(
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
                )
                .Where(p => p.PropertyType.IsSubclassOf(typeof(TraverserNode<T>)));

            foreach (var property in properties)
            {
                var childGameObject = _gameObject.transform.Find(property.Name)?.gameObject;
                if (childGameObject != null)
                {
                    var childNode = (T)
                        Activator.CreateInstance(property.PropertyType, [childGameObject]);
                    property.SetValue(this, childNode.Fill());
                }
            }
            return (T)this;
        }

        public R GetComponent<R>()
            where R : Component
        {
            return _gameObject.GetComponent<R>();
        }

        public GameObject Get()
        {
            return _gameObject;
        }

        public Transform GetTransform()
        {
            return _gameObject.transform;
        }

        public RectTransform GetRectTransform()
        {
            return _gameObject.GetComponent<RectTransform>();
        }

        public T ApplyModifiers(IModifier[] modifiers)
        {
            foreach (var modifier in modifiers)
            {
                modifier.Apply(this);
            }
            return (T)this;
        }
    }
}
