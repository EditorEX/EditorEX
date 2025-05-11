using BeatmapEditor3D.InputSystem;
using BeatmapEditor3D.Views;
using EditorEX.SDK.Base;
using EditorEX.SDK.Factories;
using EditorEX.Util;
using HarmonyLib;
using HMUI;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.UI.Patches
{
    internal class BetterKeybindViewingPatches : IAffinity
    {
        private static BetterKeybindViewingPatches? _instance;
        private static Dictionary<InputActionBinding, KeyBindingView> _keyBindingViews = new Dictionary<InputActionBinding, KeyBindingView>();
        private static readonly MethodInfo _addDictionary = AccessTools.Method(_keyBindingViews.GetType(), "Add");
        private static readonly FieldInfo _keyBindingViewField = AccessTools.Field(typeof(BetterKeybindViewingPatches), "_keyBindingViews");
        private static readonly MethodInfo _redirect = AccessTools.Method(typeof(BetterKeybindViewingPatches), "Redirect");
        private static readonly MethodInfo _redirectContentSize = AccessTools.Method(typeof(BetterKeybindViewingPatches), "RedirectContentSize");
        private static readonly MethodInfo _createBindingGroupUI = AccessTools.Method(typeof(KeybindsView), "CreateBindingGroupUI");

        private SiraLog _siraLog;
        private ButtonFactory _buttonFactory;
        private ScrollViewFactory _scrollViewFactory;
        private StringInputFactory _stringInputFactory;
        private ImageFactory _imageFactory;
        private ScrollView? _scrollView;
        private RectTransform? _buttonParent;
        private Transform? _container;
        private List<Transform> _groupTabs = new List<Transform>();
        private ReversibleDictionary<BindingGroup, GameObject> _groupButtons = new ReversibleDictionary<BindingGroup, GameObject>();
        private int _selectedGroupIndex = 0;

        private BetterKeybindViewingPatches(
            SiraLog siraLog,
            ButtonFactory buttonFactory,
            ScrollViewFactory scrollViewFactory,
            StringInputFactory stringInputFactory,
            ImageFactory imageFactory)
        {
            _instance = this;
            _siraLog = siraLog;
            _buttonFactory = buttonFactory;
            _scrollViewFactory = scrollViewFactory;
            _stringInputFactory = stringInputFactory;
            _imageFactory = imageFactory;
        }

        Transform NewGroupTab(KeybindsView keybindsView, BindingGroup bindingGroup)
        {
            if (_buttonParent == null || _container == null)
            {
                _siraLog.Error("BetterKeybindViewingPatches: ButtonParent or Container is null.");
                return keybindsView._contentTransform;
            }
            _scrollView = _scrollViewFactory.Create(_container, new LayoutData());
            _groupTabs.Add(_scrollView.transform);
            if (_groupTabs.Count != 1)
            {
                _scrollView.gameObject.SetActive(false);
            }
            int index = _groupTabs.Count - 1;
            var button = _buttonFactory.Create(_buttonParent, bindingGroup.type.DisplayName(), () =>
            {
                _selectedGroupIndex = index;
                for (int i = 0; i < _groupTabs.Count; i++)
                {
                    _groupTabs[i].gameObject.SetActive(i == index);
                }
            });
            _groupButtons.Add(bindingGroup, button.gameObject);
            //_imageFactory.Create(button.transform, "EditorEX.UI.Resources.circle.png", new LayoutData(new Vector2(15f, 15f), new Vector2(-15f, -20.26f)));
            keybindsView._contentTransform = _scrollView.contentTransform;
            _scrollView.contentTransform.GetComponent<VerticalLayoutGroup>().spacing = 50f;
            return _scrollView.contentTransform;
        }

        private static Transform Redirect(KeybindsView keybindsView, BindingGroup bindingGroup)
        {
            return _instance?.NewGroupTab(keybindsView, bindingGroup) ?? keybindsView._contentTransform;
        }

        private void RefreshContentSize()
        {
            _groupTabs.Last().gameObject.GetComponent<ScrollView>().UpdateContentSize();
        }

        private static void RedirectContentSize()
        {
            _instance?.RefreshContentSize();
        }

        [AffinityPatch(typeof(KeybindsView), nameof(KeybindsView.InitIfNeeded))]
        [AffinityPrefix]
        private void UIPatch(KeybindsView __instance)
        {
            if (__instance._initialized)
            {
                return;
            }
            _container = __instance.transform.GetChild(0);
            Object.Destroy(_container.GetChild(0).gameObject);

            _buttonParent = _scrollViewFactory.Create(_container, new LayoutData(new Vector2(-900f, 0f), new Vector2(1000f, 0f))).contentTransform;

            var verticalLayoutGroup = _buttonParent.GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childForceExpandWidth = false;
            verticalLayoutGroup.childAlignment = TextAnchor.MiddleRight;
            verticalLayoutGroup.spacing = 10f;
        }

        [AffinityPatch(typeof(KeybindsView), nameof(KeybindsView.InitIfNeeded))]
        [AffinityPostfix]
        private void UIPatchPost(KeybindsView __instance)
        {
            _scrollView?.UpdateContentSize();

            var searchInput = _stringInputFactory.Create(__instance.transform, "Search", 300f, x =>
            {
                var results = SearchKeybindings(x);
                UpdateUIForSearchResults(results);
            });
            searchInput.transform.parent.localPosition = new Vector3(-890f, 495f, 0f);
        }

        [AffinityPatch(typeof(KeybindsView), nameof(KeybindsView.CreateBindingGroupUI))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerGroup(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .Advance(2)
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld))
                .RemoveInstructions(2)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _redirect))
                .InstructionEnumeration();

            return result;
        }

        [AffinityPatch(typeof(KeybindsView), nameof(KeybindsView.CreateBindingUI))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerBinding(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .End()
                .Insert(
                    new CodeInstruction(OpCodes.Ldsfld, _keyBindingViewField),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Callvirt, _addDictionary))
                .InstructionEnumeration();

            return result;
        }

        [AffinityPatch(typeof(KeybindsView), nameof(KeybindsView.InitIfNeeded))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerGroupRefresh(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .End()
                .Advance(-3)
                .RemoveInstructions(3)
                .MatchEndBackwards(new CodeMatch(OpCodes.Call, _createBindingGroupUI))
                .Advance(1)
                .Insert(new CodeInstruction(OpCodes.Call, _redirectContentSize))
                .InstructionEnumeration();

            return result;
        }

        private void UpdateUIForSearchResults(SearchResult? result)
        {
            if (result == null)
            {
                foreach (var group in _groupButtons)
                {
                    group.Value.SetActive(true);
                }
                foreach (var keybindingView in _keyBindingViews)
                {
                    keybindingView.Value.gameObject.SetActive(true);
                }
                return;
            }

            foreach (var group in _groupButtons)
            {
                group.Value.SetActive(result.Value.groupResults.Any(x => x.bindingGroup == group.Key));
            }

            for (int i = 0; i < _groupButtons.Count; i++)
            {
                var kvp = _groupButtons.ElementAt(i);
                if (_selectedGroupIndex == i && !result.Value.groupResults.Any(x => x.bindingGroup == kvp.Key))
                {
                    _groupButtons.FirstOrDefault(x => result.Value.groupResults.Any(y => y.bindingGroup == x.Key)).Value?.GetComponent<Button>().onClick.Invoke();
                    break;
                }
            }

            foreach (var keybindingView in _keyBindingViews)
            {
                var binding = keybindingView.Key;
                var groupResult = result.Value.groupResults.FirstOrDefault(x => x.bindingSorted.Contains(binding));
                if (groupResult.bindingGroup == null)
                {
                    keybindingView.Value.gameObject.SetActive(false);
                }
                else
                {
                    keybindingView.Value.gameObject.SetActive(true);
                }
            }
        }

        private SearchResult? SearchKeybindings(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return null;
            }

            var keybindings = KeyBindings.GetDefault();
            var searchResult = new SearchResult();
            foreach (var bindingGroup in new List<BindingGroup>([keybindings.activatorsBindingGroup]).Concat(keybindings.extendedBindingGroups))
            {
                Dictionary<InputActionBinding, int> matchingBindings = new Dictionary<InputActionBinding, int>();
                foreach (var binding in bindingGroup.bindings)
                {
                    var text = binding.inputAction.DisplayName();
                    if (text != null)
                    {
                        bool matched = FuzzyMatcher.FuzzyMatch(text, searchText, out var score);
                        if (matched)
                        {
                            matchingBindings.Add(binding, score);
                        }
                    }
                }
                if (matchingBindings.Count == 0)
                {
                    continue;
                }
                var groupResult = new GroupResult
                {
                    bindingGroup = bindingGroup,
                    bindingSorted = matchingBindings.OrderByDescending(x => x.Value).Select(x => x.Key).ToArray()
                };
                searchResult.groupResults.Add(groupResult);
            }
            _scrollView?.UpdateContentSize();
            return searchResult;
        }


        private struct SearchResult
        {
            public SearchResult()
            {
            }

            public List<GroupResult> groupResults = new List<GroupResult>();
        }

        private struct GroupResult
        {
            public BindingGroup bindingGroup;
            public InputActionBinding[] bindingSorted;
        }
    }
}
