using EditorEX.SDK.AddressableHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace EditorEX.SDK.Input
{
    public class CustomInputActionRegistry
    {
        [Inject]
        private void Construct(
            List<ICustomInputAction> actions)
        {

        }
    }
}
