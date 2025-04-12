using EditorEX.Essentials.Features.ViewMode;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace EditorEX.Essentials.Visuals
{
    public class VisualsTypeProvider : ITypeProvider
    {
        private readonly SiraLog _siraLog;
        private ActiveViewMode _activeViewMode;
        private List<ValueTuple<string[], Type>> _providers;

        [Inject]
        private VisualsTypeProvider(
            SiraLog _siraLog,
            ActiveViewMode activeViewMode,
            [Inject(Id = "Visuals")] List<ValueTuple<string[], Type>> providers)
        {
            _activeViewMode = activeViewMode;
            _providers = providers;
        }

        public Type GetProvidedType(Type[]? availableTypes)
        {
            var viewingMode = _activeViewMode.Mode.ID;

            // Provider name to use, fallback if none exist.
            var provider = _providers.Any(x => x.Item1.Contains(viewingMode)) ? viewingMode : "normal";

            var pickedProvider = _providers.FirstOrDefault(x => x.Item1.Contains(provider) && availableTypes.Contains(x.Item2)).Item2;

            if (pickedProvider == null)
            {
                _siraLog.Error($"Something has gone horribly wrong! No Visuals Provider could be found for the present conditions. Viewing Mode {viewingMode} types: {string.Join(",", availableTypes.Select(x => x.Name))}");
            }

            return pickedProvider;
        }
    }
}
