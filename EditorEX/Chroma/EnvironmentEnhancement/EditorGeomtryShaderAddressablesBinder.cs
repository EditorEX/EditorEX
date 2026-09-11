using EditorEX.SDK.AddressableHelpers;
using UnityEngine;

namespace EditorEX.Chroma.EnvironmentEnhancement;

internal class EditorGeomtryShaderAddressablesBinder
{
    private EditorGeomtryShaderAddressablesBinder(
        EditorGeomtryShaderAddressables shaders,
        AddressableSignalBus addressableSignalBus
    )
    {
        addressableSignalBus.Subscribe<Shader>(
            "waterlit",
            null,
            signal => shaders.SetWaterlit(signal.Object)
        );
        addressableSignalBus.Subscribe<Shader>(
            "glowing",
            null,
            signal => shaders.SetGlowing(signal.Object)
        );
    }
}
