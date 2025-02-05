using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.VariableMovement
{
    public class VariableMovementHolder : MonoBehaviour
    {
        [Inject]
        internal EditorNoodleMovementDataProvider.Pool? Pool;
        [Inject]
        public IVariableMovementDataProvider? Original;
    }
}