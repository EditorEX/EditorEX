using EditorEX.SDK.ContextMenu.Objects;
using EditorEX.UI.ContextMenu;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using static BeatmapEditor3D.DataModels.BeatmapsCollectionDataModel;

namespace EditorEX.UI.Components.ContextMenu
{
    public class BeatmapListContextMenu : MonoBehaviour, IPointerClickHandler
    {
        private ContextMenuComponent _contextMenu = null!;

        private BeatmapInfoData _beatmapInfoData;

        [Inject]
        private void Construct(ContextMenuComponent contextMenu)
        {
            _contextMenu = contextMenu;
        }

        public void SetData(BeatmapInfoData beatmapInfoData)
        {
            _contextMenu.TryInvalidate(gameObject);
            _beatmapInfoData = beatmapInfoData;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                _contextMenu.ShowContextMenu(
                    new BeatmapListContextMenuObject(_beatmapInfoData),
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
