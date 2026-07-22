using EditorEX.SDK.ContextMenu;
using EditorEX.SDK.ContextMenu.Objects;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace EditorEX.UI.Components.ContextMenu
{
    public class SourceListContextMenu : MonoBehaviour, IPointerClickHandler
    {
        private IContextMenuService _contextMenu = null!;

        private string _source;

        [Inject]
        private void Construct(IContextMenuService contextMenu)
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
                _contextMenu.ShowContextMenu(
                    new SourceListContextMenuObject(_source),
                    eventData.position,
                    gameObject
                );
            }
        }

        private void OnDisable()
        {
            _contextMenu.TryInvalidate(gameObject);
        }
    }
}
