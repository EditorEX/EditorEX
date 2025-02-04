using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.Essentials.ViewMode
{
    public static class ViewModeRepository
    {
        private static List<ViewMode> viewModes = new();

        public static bool RegisterViewMode(ViewMode viewMode)
        {
            if (viewModes.Any(x => x.ID == viewMode.ID))
            {
                return false;
            }

            viewModes.Add(viewMode);
            return true;
        }

        public static List<ViewMode> GetViewModes()
        {
            return viewModes;
        }
    }
}
