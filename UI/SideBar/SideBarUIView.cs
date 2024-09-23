using BeatmapEditor3D.Views;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace BetterEditor.UI.SideBar
{
	internal class SideBarUI
	{
		private Transform root;
		private static List<SideBarButton> buttons = new List<SideBarButton>();

		[Inject]
		private SideBarUI()
		{
			root = new GameObject("SideBarRoot").transform;
			root.SetParent(GameObject.Find("ScreenContainer").transform);
			root.transform.localPosition = new Vector3(910f, 0f, 0f);
			var bg = BasicUI.MakeBackground(root);
			bg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

			bg.gameObject.AddComponent<VerticalLayoutGroup>();
			bg.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			foreach (var button in buttons)
			{
				MakeButton(bg.transform, button);
			}
		}

		private void MakeButton(Transform bg, SideBarButton button)
		{
			var obj = new GameObject("EditorBackground").AddComponent<ClickableImage>();
			obj.rectTransform.sizeDelta = new Vector2(40f, 40f);
			obj.sprite = button.sprite;
			obj.preserveAspect = true;
			obj.OnClickEvent += button.onClick;

			obj.transform.SetParent(bg, false);
		}

		public static SideBarButton RegisterButton(Sprite sprite, string name, Action<PointerEventData> onClick)
		{
			var button = new SideBarButton(sprite, name, onClick);
			buttons.Add(button);
			return button;
		}

		public class SideBarButton
		{
			public Sprite sprite;
			public string name;
			public Action<PointerEventData> onClick;

			public SideBarButton(Sprite _sprite, string _name, Action<PointerEventData> _onClick)
			{
				sprite = _sprite;
				name = _name;
				onClick = _onClick;
			}
		}
	}
}
