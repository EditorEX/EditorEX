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

        private Layout? _buttonContent;
        private Layout? _bindingsContainer;
        private ReversibleDictionary<BindingGroup, EditorLabelButton> _groupButtons = new();
        private Dictionary<InputActionBinding, EditorNamedRail> _keyBindingViews = new();
        private State<int> _selectedGroupIndex = StateUtils.Remember(0);

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
            var namedRail = new EditorLabel
            {
                Text = keybindString,
                FontSize = 17f,
                Alignment = TMPro.TextAlignmentOptions.Right,
            }
                .InEditorNamedRail(text, 17f)
                .AsFlexItem();

            _keyBindingViews[inputActionBinding] = namedRail;
            group.Children.Add(namedRail);
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
                        new EditorHeaderLabel { Text = bindingGroup.type.DisplayName() }.AsFlexItem(
                            margin: new YogaFrame(0f, 10f, 0f, 0f)
                        ),
                    }
                        .AsLayout()
                        .Export(out var groupLayout)
                        .AsFlexItem(size: "auto")
                        .AsFlexGroup(FlexDirection.Column, gap: 20f),
                }
                    .WithRectExpand()
                    .AsFlexItem(size: new YogaVector("auto", 100.pct))
                    .EnabledWithState(_selectedGroupIndex, index)
            );

            foreach (var inputActionBinding in bindingGroup.bindings)
            {
                CreateBindingUI(
                    groupLayout,
                    inputActionBinding,
                    commandToKeybind[bindingGroup.activator]
                );
            }

            var button = new EditorLabelButton
            {
                Text = bindingGroup.type.DisplayName(),
                FontSize = 18f,
                OnClick = () =>
                {
                    _selectedGroupIndex.Value = index;
                },
            }.AsFlexItem(size: new YogaVector { x = "auto", y = 40f });

            _groupButtons[bindingGroup] = button;

            _buttonContent!.Children.Add(button);
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
                        .WithListener(
                            x => x.Text,
                            s => UpdateUIForSearchResults(SearchKeybindings(s))
                        )
                        .InEditorNamedRail("Search", 18f)
                        .AsFlexItem(),
                }
                    .AsLayout()
                    .AsFlexItem(flex: 3, size: new YogaVector("auto", 100.pct))
                    .AsFlexGroup(FlexDirection.Column, gap: 40f)
                    .Bind(ref _bindingsContainer),
                new LayoutChildren
                {
                    new ScrollArea()
                    {
                        ScrollSize = -500f,
                        ScrollContent = new Layout { }
                            .Bind(ref _buttonContent)
                            .AsFlexItem()
                            .AsFlexGroup(FlexDirection.Column, gap: 10f, constrainVertical: false),
                    }
                        .WithRectExpand()
                        .Export(out var _buttonScrollArea),
                    new EditorScrollbar()
                        .AsFlexItem(
                            size: new() { x = 7f, y = 100.pct },
                            position: new() { right = -20f }
                        )
                        .With(x => _buttonScrollArea!.Scrollbar = x),
                }
                    .AsLayout()
                    .AsFlexItem(flex: 1)
                    .AsFlexGroup(FlexDirection.Row, gap: 10f),
            }
                .AsLayout()
                .WithReactiveContainer(_reactiveContainer)
                .AsFlexItem(size: new YogaVector(1700f, 1000))
                .AsFlexGroup(FlexDirection.Row, gap: 600f)
                .Use(__instance.transform);

            var keybindings = KeyBindings.GetDefault();
            var dictionary = new Dictionary<InputAction, InputKey>();
            dictionary[InputAction.None] = InputKey.none;
            foreach (var inputActionBinding in keybindings.activatorsBindingGroup.bindings)
            {
                dictionary[inputActionBinding.inputAction] = inputActionBinding.keysCombination[0];
            }

            CreateBindingGroupUI(keybindings.activatorsBindingGroup, dictionary, 0);
            for (var i = 0; i < keybindings.extendedBindingGroups.Length; i++)
            {
                var bindingGroup = keybindings.extendedBindingGroups[i];
                CreateBindingGroupUI(bindingGroup, dictionary, i + 1);
            }

            _buttonScrollArea.ScrollToStart(true);
            _siraLog.Info($"KeybindsView initialized.");
            _siraLog.Info(_buttonScrollArea.ContentTransform.rect.size.y);

            return false;
        }

        private void UpdateUIForSearchResults(SearchResult? result)
        {
            if (result == null)
            {
                foreach (var group in _groupButtons)
                {
                    group.Value.Enabled = true;
                }

                foreach (var keybindingView in _keyBindingViews)
                {
                    keybindingView.Value.Enabled = true;
                }
                return;
            }

            foreach (var group in _groupButtons)
            {
                group.Value.Enabled = result.Value.groupResults.Any(x =>
                    x.bindingGroup == group.Key
                );
            }

            if (
                _groupButtons
                    .Where(
                        (kvp, i) =>
                            _selectedGroupIndex == i
                            && result.Value.groupResults.All(x => x.bindingGroup != kvp.Key)
                    )
                    .Any()
            )
            {
                _groupButtons
                    .FirstOrDefault(x =>
                        result.Value.groupResults.Any(y => y.bindingGroup == x.Key)
                    )
                    .Value?.Invoke();
            }
            foreach (var keybindingView in _keyBindingViews)
            {
                var binding = keybindingView.Key;
                var groupResult = result.Value.groupResults.FirstOrDefault(x =>
                    x.bindingSorted.Contains(binding)
                );
                keybindingView.Value.Enabled = groupResult.bindingGroup != null;
            }
        }

        private SearchResult? SearchKeybindings(string searchText)
        {
            _siraLog.Info(searchText);
            if (string.IsNullOrEmpty(searchText))
            {
                return null;
            }

            var keybindings = KeyBindings.GetDefault();
            var searchResult = new SearchResult();
            foreach (
                var bindingGroup in new List<BindingGroup>([
                    keybindings.activatorsBindingGroup,
                ]).Concat(keybindings.extendedBindingGroups)
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

            public List<GroupResult> groupResults = [];
        }

        private struct GroupResult
        {
            public BindingGroup bindingGroup;
            public InputActionBinding[] bindingSorted;
        }
    }
}
