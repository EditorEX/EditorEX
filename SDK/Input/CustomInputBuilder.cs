using System;
using System.Collections.Generic;
using BeatmapEditor3D.InputSystem;
using EditorEX.SDK.Input;
using Zenject;

namespace BetterEditor.SDK.Input
{
    public static class CustomInputBuilder
    {
        public static GroupBuilding StartGroup(string modName, string groupName)
        {
            CheckGroupInputs(modName, groupName);

            return new GroupBuilding(modName, groupName);
        }

        private static void CheckGroupInputs(string modName, string groupName)
        {
            if (string.IsNullOrWhiteSpace(modName))
                throw new ArgumentException("Mod name cannot be null or empty.", nameof(modName));

            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentException("Group name cannot be null or empty.", nameof(groupName));
        }

        public class GroupBuilding 
        {
            public string ModName { get; }
            public string GroupName { get; }
            public string GroupID { get; }
            private List<CustomInputAction> actions = new List<CustomInputAction>();

            internal GroupBuilding(string modName, string groupName)
            {
                ModName = modName;
                GroupName = groupName;
                GroupID = (modName + "_" + groupName).ToLowerInvariant().Replace(" ", "_");
            }

            public GroupBuilding AddKeybinding(string keybindingName, InputKey[] defaultKeys)
            {
                CheckKeybindingInputs(keybindingName, defaultKeys);

                actions.Add(MakeKeybinding(keybindingName, defaultKeys));
                return this;
            }

            public GroupBuilding AddKeybinding(string keybindingName, InputKey[] defaultKeys, ref CustomInputAction keybindingReference)
            {
                CheckKeybindingInputs(keybindingName, defaultKeys);

                keybindingReference = MakeKeybinding(keybindingName, defaultKeys);
                actions.Add(keybindingReference);
                return this;
            }
            
            public CustomInputGroup Build()
            {
                if (actions.Count == 0)
                    throw new InvalidOperationException("Cannot build group with no keybindings.");

                var group = new CustomInputGroup(GroupID, GroupName, actions.ToArray());
                return group;
            }
            
            public CustomInputGroup Build(ref CustomInputGroup referenceGroup, DiContainer? toBind = null)
            {
                referenceGroup = Build();
                if (toBind != null)
                {
                    toBind.Bind<ICustomInputGroup>().FromInstance(referenceGroup);
                }
                return referenceGroup;
            }

            public CustomInputAction[] GetKeybindings()
            {
                return actions.ToArray();
            }

            private void CheckKeybindingInputs(string keybindingName, InputKey[] defaultKeys)
            {
                if (string.IsNullOrWhiteSpace(keybindingName))
                    throw new ArgumentException("Keybinding name cannot be null or empty.", keybindingName);

                if (defaultKeys == null || defaultKeys.Length == 0)
                    throw new ArgumentException("Keys cannot be null or empty.");

                if (actions.Exists(a => a.Name == keybindingName))
                    throw new ArgumentException($"Keybinding '{keybindingName}' already exists.");
            }

            private CustomInputAction MakeKeybinding(string keybindingName, InputKey[] defaultKeys)
            {
                return new CustomInputAction(keybindingName, defaultKeys);
            }
        }
    }
}