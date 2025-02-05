using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Movement.Types;
using EditorEX.Essentials.ViewMode;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using EditorEX.MapData.Contexts;
using Zenject;

namespace EditorEX.Essentials.Movement
{
    public class VariableMovementTypeProvider : ITypeProvider
    {
        private readonly SiraLog _siraLog;
        private readonly List<ValueTuple<string, Type>> _providers;

        [Inject]
        private VariableMovementTypeProvider(
            SiraLog siraLog,
            [Inject(Id = "VariableMovement")] List<ValueTuple<string, Type>> providers)
        {
            _siraLog = siraLog;
            _providers = providers;
        }

        public Type GetProvidedType(Type[]? availableTypes)
        {
            var mode = MapContext.Version.Major > 3 ? "Variable" : "Noodle";

            var pickedProvider = _providers.FirstOrDefault(x => x.Item1 == mode).Item2;

            if (pickedProvider == null)
            {
                _siraLog.Error($"Something has gone horribly wrong! No Variable Movement Provider could be found for the present conditions. Mode {mode}");
            }

            return pickedProvider;
        }
    }
}
