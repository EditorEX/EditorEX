using EditorEX.SDK.ContextMenu.Objects;
using EditorEX.UI.ContextMenu;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace EditorEX.UI.Components.ContextMenu
{
    public class SourceListContextMenu : MonoBehaviour, IPointerClickHandler
    {
        private ContextMenuComponent _contextMenu = null!;

        private string _source;

        [Inject]
        private void Construct(
            ContextMenuComponent contextMenu)
        {
            _contextMenu = contextMenu;
        }

        public void SetData(string source)
        {
            _contextMenu.TryInvalidate(gameObject);
            _source = source;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                _contextMenu.ShowContextMenu(new SourceListContextMenuObject(_source), eventData.position, gameObject);
            }
        }

        private void OnDisable()
        {
            _contextMenu.TryInvalidate(gameObject);
        }
    }
}
