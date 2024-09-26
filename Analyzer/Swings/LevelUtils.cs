using EditorEX;
using EditorEX.Essentials.Movement.Data;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;
using Zenject;

public class LevelUtils
{
    private EditorBasicBeatmapObjectSpawnMovementData _spawnMovementData;

    [Inject]
    private void Construct(EditorBasicBeatmapObjectSpawnMovementData spawnMovementData)
    {
        _spawnMovementData = spawnMovementData;
    }

    public static Vector2 GetCellSize()
    {
        return Vector2.one;
    }

    public Vector2 GetWorldXYFromBeatmapCoords(int x, int y)
    {
        var _gravity = _spawnMovementData.NoteJumpGravityForLineLayer((NoteLineLayer)y, NoteLineLayer.Base);
        var _startVerticalVelocity = _gravity * _spawnMovementData._jumpDuration * 0.5f;
        var yPos = _startVerticalVelocity * 0.75f - _gravity * 0.75f * 0.75f * 0.5f;

        float num = (float)(-(float)(4 - 1)) * 0.5f;
        num = (num + (float)x) * 0.8f;

        return new Vector2(num, yPos);
    }

}
