using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.SDK.Settings
{
    public class SettingsViewData
    {
        public SettingsViewData(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
