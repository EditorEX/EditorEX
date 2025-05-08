using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Reactive.Components.Native
{
    public class ReactiveContainerHolder : MonoBehaviour
    {
        public ReactiveContainer ReactiveContainer { get; set; } = null!;
    }
}