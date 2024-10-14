using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Collectors
{
    public class TransitionCollector : IInitializable
    {
        internal Dictionary<string, BaseTransitionSO> _transitions = new();

        public void Initialize()
        {
            var transitions = Resources.FindObjectsOfTypeAll<BaseTransitionSO>();
            foreach (var transition in transitions)
            {
                if (transition.name.StartsWith("BeatmapEditor"))
                {
                    string remappedName = transition.name.Substring(14).Replace(".", "/");
                    _transitions[remappedName] = transition;
                    Debug.Log($"Found transition: {remappedName}");
                }
            }
        }

        public T GetTransition<T>(string name) where T: BaseTransitionSO
        {
            if (!_transitions.ContainsKey(name))
            {
                throw new ArgumentException($"Transition {name} does not exist! Did you mispell something?");
            }
            return (T)_transitions[name];
        }
    }
}