using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Movement.Types;
using EditorEX.Essentials.ViewMode;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace EditorEX.Essentials.Movement
{
    public class MovementTypeProvider : ITypeProvider
    {
        private readonly SiraLog _siraLog;
        private readonly ActiveViewMode _activeViewMode;
        private readonly List<ValueTuple<string[], Type>> _providers;

        [Inject]
        private MovementTypeProvider(
            SiraLog siraLog,
            ActiveViewMode activeViewMode,
            [Inject(Id = "Movement")] List<ValueTuple<string[], Type>> providers)
        {
            _siraLog = siraLog;
            _activeViewMode = activeViewMode;
            _providers = providers;
        }

        public Type GetProvidedType(Type[] availableTypes, bool REDACTED)
        {
            var viewingMode = _activeViewMode.Mode.ID;

            // Provider name to use, fallback if none exist.
            var provider = _providers.Any(x => x.Item1.Contains(viewingMode)) ? viewingMode : "normal";

            var pickedProvider = _providers.FirstOrDefault(x => x.Item1.Contains(provider) && availableTypes.Contains(x.Item2)).Item2;

            if (pickedProvider == null)
            {
                _siraLog.Error($"Something has gone horribly wrong! No Movement Provider could be found for the present conditions. Viewing Mode {viewingMode}");
            }

            return pickedProvider;
        }
    }
}
