using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BeatmapEditor3D.InputSystem;
using BeatmapEditor3D.InputSystem.Utilities;
using BeatmapEditor3D.Views;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Native;
using EditorEX.SDK.ReactiveComponents.Table;
using EditorEX.Util;
using HarmonyLib;
using Reactive;
using Reactive.Components.Basic;
using Reactive.Yoga;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.UI.Patches
{
    internal class BetterKeybindViewingPatches : IAffinity
    {
        private SiraLog _siraLog = null!;
        private ReactiveContainer _reactiveContainer = null!;

        private Dictionary<InputActionBinding, KeyBindingView> _keyBindingViews =
            new Dictionary<InputActionBinding, KeyBindingView>();
        private Layout? _buttonContent;
        private Layout? _bindingsContainer;
        private ReversibleDictionary<BindingGroup, GameObject> _groupButtons =
            new ReversibleDictionary<BindingGroup, GameObject>();
        private ObservableValue<int> _selectedGroupIndex = ValueUtils.Remember(0);

        private BetterKeybindViewingPatches(SiraLog siraLog, ReactiveContainer reactiveContainer)
        {
            _siraLog = siraLog;
            _reactiveContainer = reactiveContainer;
        }

        private static StringBuilder _keybindStringBuilder = new();

        private (string, string) GetKeybindString(
            InputActionBinding inputActionBinding,
            InputKey activatorKeyBind
        )
        {
            _keybindStringBuilder.Clear();
            if (activatorKeyBind != InputKey.none)
            {
                _keybindStringBuilder.Append(activatorKeyBind.DisplayName() + " + ");
            }
            for (int i = 0; i < inputActionBinding.keysCombination.Count; i++)
            {
                _keybindStringBuilder.Append(inputActionBinding.keysCombination[i].DisplayName());
                if (i < inputActionBinding.keysCombination.Count - 1)
                {
                    _keybindStringBuilder.Append(" + ");
                }
            }
            string text = inputActionBinding.inputAction.DisplayName();
            if (inputActionBinding.strictCombination)
            {
                text += "*";
            }
            return (text, _keybindStringBuilder.ToString());
        }

        private void CreateBindingUI(
            Layout group,
            InputActionBinding inputActionBinding,
            InputKey activatorKeyBind
        )
        {
            var (text, keybindString) = GetKeybindString(inputActionBinding, activatorKeyBind);
            group.Children.Add(
                new EditorLabel
                {
                    Text = keybindString,
                    FontSize = 17f,
                    Alignment = TMPro.TextAlignmentOptions.Right,
                }
                    .InEditorNamedRail(text, 17f)
                    .AsFlexItem()
            );
        }

        private void CreateBindingGroupUI(
            BindingGroup bindingGroup,
            Dictionary<InputAction, InputKey> commandToKeybind,
            int index
        )
        {
            _bindingsContainer!.Children.Add(
                new ScrollArea
                {
                    ScrollContent = new LayoutChildren
                    {
                        new EditorHeaderLabel
                        {
                            Text = bindingGroup.type.DisplayName(),
                        }.AsFlexItem(margin: new YogaFrame(0f, 10f, 0f, 0f)),
                    }
                    .AsLayout()
                    .Export(out var groupLayout)
                    .AsFlexItem(size: "auto")
                    .AsFlexGroup(FlexDirection.Column, gap: 20f),
                }
                .AsFlexItem(size: new YogaVector("auto", 100.pct()))
                .EnabledWithObservable(_selectedGroupIndex, index)
            );

            foreach (var inputActionBinding in bindingGroup.bindings)
            {
                CreateBindingUI(
                    groupLayout,
                    inputActionBinding,
                    commandToKeybind[bindingGroup.activator]
                );
            }

            _buttonContent!.Children.Add(
                new EditorLabelButton
                {
                    Text = bindingGroup.type.DisplayName(),
                    FontSize = 18f,
                    OnClick = () =>
                    {
                        _selectedGroupIndex.Value = index;
                    },
                }.AsFlexItem(size: new YogaVector { x = "auto", y = 40f })
            );
        }

        [AffinityPatch(typeof(KeybindsView), nameof(KeybindsView.InitIfNeeded))]
        [AffinityPrefix]
        private bool InitIfNeeded(KeybindsView __instance)
        {
            Object.Destroy(__instance.transform.GetChild(0).gameObject);

            new LayoutChildren
            {
                new LayoutChildren
                {
                    new EditorStringInput { }
                        .InEditorNamedRail("Search", 18f)
                        .AsFlexItem(),
                }
                .AsLayout()
                .AsFlexItem(flex: 3, size: new YogaVector("auto", 100.pct()))
                .AsFlexGroup(FlexDirection.Column, gap: 40f)
                .Bind(ref _bindingsContainer),

                new LayoutChildren
                {
                    new ScrollArea()
                    {
                        ScrollContent = new Layout { }
                            .Bind(ref _buttonContent)
                            .AsFlexItem()
                            .AsFlexGroup(FlexDirection.Column, gap: 10f),
                    }
                    .Export(out var _buttonScrollArea)
                    .AsFlexItem(size: 100f.pct()),

                    new EditorScrollbar()
                        .AsFlexItem(
                            size: new() { x = 7f, y = 100.pct() },
                            position: new() { right = 2f }
                        )
                        .With(x => _buttonScrollArea!.Scrollbar = x),
                }
                .AsLayout()
                .AsFlexItem(flex: 1)
                .AsFlexGroup(FlexDirection.Row),
            }
            .AsLayout()
            .WithReactiveContainer(_reactiveContainer)
            .AsFlexItem(size: new YogaVector(1700f, 1000))
            .AsFlexGroup(FlexDirection.Row, gap: 600f)
            .Use(__instance.transform);

            var keybindings = KeyBindings.GetDefault();
            Dictionary<InputAction, InputKey> dictionary = new Dictionary<InputAction, InputKey>();
            dictionary[InputAction.None] = InputKey.none;
            foreach (
                InputActionBinding inputActionBinding in keybindings.activatorsBindingGroup.bindings
            )
            {
                dictionary[inputActionBinding.inputAction] = inputActionBinding.keysCombination[0];
            }

            CreateBindingGroupUI(keybindings.activatorsBindingGroup, dictionary, 0);
            for (int i = 0; i < keybindings.extendedBindingGroups.Length; i++)
            {
                var bindingGroup = keybindings.extendedBindingGroups[i];
                CreateBindingGroupUI(bindingGroup, dictionary, i + 1);
            }
            return false;
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
                group.Value.SetActive(
                    result.Value.groupResults.Any(x => x.bindingGroup == group.Key)
                );
            }

            for (int i = 0; i < _groupButtons.Count; i++)
            {
                var kvp = _groupButtons.ElementAt(i);
                if (
                    _selectedGroupIndex == i
                    && !result.Value.groupResults.Any(x => x.bindingGroup == kvp.Key)
                )
                {
                    _groupButtons
                        .FirstOrDefault(x =>
                            result.Value.groupResults.Any(y => y.bindingGroup == x.Key)
                        )
                        .Value?.GetComponent<Button>()
                        .onClick.Invoke();
                    break;
                }
            }

            foreach (var keybindingView in _keyBindingViews)
            {
                var binding = keybindingView.Key;
                var groupResult = result.Value.groupResults.FirstOrDefault(x =>
                    x.bindingSorted.Contains(binding)
                );
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
            foreach (
                var bindingGroup in new List<BindingGroup>(
                    [keybindings.activatorsBindingGroup]
                ).Concat(keybindings.extendedBindingGroups)
            )
            {
                Dictionary<InputActionBinding, int> matchingBindings = [];
                
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
                    bindingSorted = matchingBindings
                        .OrderByDescending(x => x.Value)
                        .Select(x => x.Key)
                        .ToArray(),
                };
                searchResult.groupResults.Add(groupResult);
            }
            return searchResult;
        }

        private struct SearchResult
        {
            public SearchResult() { }

            public List<GroupResult> groupResults = new List<GroupResult>();
        }

        private struct GroupResult
        {
            public BindingGroup bindingGroup;
            public InputActionBinding[] bindingSorted;
        }
    }
}
