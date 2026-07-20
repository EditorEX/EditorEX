using System.Collections.Generic;
using System.Linq;

namespace EditorEX.Essentials.Features.ViewMode
{
    public static class ViewModeRepository
    {
        private static readonly LinkedList<ViewMode> ViewModes = new();

        public static bool RegisterViewMode(ViewMode viewMode)
        {
            if (ViewModes.Any(x => x.ID == viewMode.ID))
            {
                return false;
            }

            ViewModes.AddLast(viewMode);
            return true;
        }

        public static ViewMode? ViewMode(string id)
        {
            return ViewModes.FirstOrDefault(x => x.ID == id);
        }

        public static ViewMode GetNextViewMode(ViewMode currentViewMode)
        {
            return ViewModes.Find(currentViewMode)?.Next?.Value ?? ViewModes.First.Value;
        }

        public static ViewMode GetPreviousViewMode(ViewMode currentViewMode)
        {
            return ViewModes.Find(currentViewMode)?.Previous?.Value ?? ViewModes.Last.Value;
        }
    }
}
