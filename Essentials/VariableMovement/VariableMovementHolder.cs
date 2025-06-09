using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.VariableMovement
{
    public class VariableMovementHolder : MonoBehaviour
    {
        [Inject]
        internal EditorNoodleMovementDataProvider.Pool? Pool = null!;
        [Inject]
        public IVariableMovementDataProvider? Original = null!;
    }
}