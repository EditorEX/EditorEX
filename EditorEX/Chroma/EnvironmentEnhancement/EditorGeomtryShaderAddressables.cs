using System;
using UnityEngine;

namespace EditorEX.Chroma.EnvironmentEnhancement;

internal class EditorGeomtryShaderAddressables
{
    internal Shader Waterlit =>
        _waterlit ?? throw new InvalidOperationException("Waterlit shader not loaded");
    internal Shader Glowing =>
        _glowing ?? throw new InvalidOperationException("Glowing shader not loaded");

    private Shader? _waterlit;
    private Shader? _glowing;

    internal void SetWaterlit(Shader shader)
    {
        _waterlit = shader;
    }

    internal void SetGlowing(Shader shader)
    {
        _glowing = shader;
    }
}
