using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.SDK.Input
{
    public class CustomInputAction : ICustomInputAction
    {
        public CustomInputAction(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
