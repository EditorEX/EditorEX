using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Movement.Types;
using EditorEX.Essentials.ViewMode;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace EditorEX.Essentials.Movement
{
    public class MovementTypeProvider
    {
        private ActiveViewMode _activeViewMode;
        private List<ValueTuple<string[], Type>> _providers;

        [Inject]
        private MovementTypeProvider(ActiveViewMode activeViewMode, [Inject(Id = "Movement")] List<ValueTuple<string[], Type>> providers)
        {
            _activeViewMode = activeViewMode;
            _providers = providers; //providers.Select(x => (x.ViewModes, x.Type)).ToList();
        }

        public Type GetNoteMovement(Type[] availableTypes)
        {
            var viewingMode = _activeViewMode.Mode;

            // Provider name to use, fallback if none exist.
            var provider = _providers.Any(x => x.Item1.Contains(viewingMode)) ? viewingMode : "Normal";

            var pickedProvider = _providers.FirstOrDefault(x => x.Item1.Contains(provider) && availableTypes.Contains(x.Item2)).Item2;

            if (pickedProvider == null)
            {
                Plugin.Log.Error($"Something has gone horribly wrong! No NoteMovement provider could be found for the present conditions. Viewing Mode {viewingMode}");
            }

            return pickedProvider;
        }
    }
}
