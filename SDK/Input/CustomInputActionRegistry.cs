using System.Collections.Generic;
using System.Linq;
using SiraUtil.Logging;
using Zenject;

namespace EditorEX.SDK.Input
{
    public class CustomInputActionRegistry
    {
        private readonly List<ICustomInputGroup> _groups = new List<ICustomInputGroup>();

        [Inject]
        private void Construct(List<ICustomInputGroup> groups, SiraLog siraLog)
        {
            siraLog.Info(
                $"EditorEX Keybind Registration has Concluded! Registering {groups.Sum(a => a.GetKeybindings().Length)} Keybind(s) across {groups.Count} Group(s)."
            );
            int groupIndex = 1000;
            int actionIndex = 1000;
            foreach (var action in groups)
            {
                action.AssignGroupIndex(groupIndex);
                siraLog.Debug(
                    $"Group {action.ID} has been assigned an index of {action.GroupIndex}."
                );
                foreach (var keybinding in action.GetKeybindings())
                {
                    siraLog.Debug(
                        $"Keybinding {keybinding.Name} has been registered in group {action.ID}."
                    );
                    keybinding.AssignActionIndex(actionIndex);
                    actionIndex++;
                }
                groupIndex++;
            }
            _groups.AddRange(groups);
        }

        public List<ICustomInputGroup> GetGroups()
        {
            return _groups;
        }
    }
}
