using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EditorEX.Util
{
    internal class InputUtils
    {
        public static bool IsInputFieldActive()
        {
            GameObject selectedObj = EventSystem.current.currentSelectedGameObject;

            if (selectedObj != null)
            {
                TMP_InputField tmpInputField = selectedObj.GetComponent<TMP_InputField>();
                if (tmpInputField != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
