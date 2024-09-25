using BeatmapEditor3D.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IParityMethod {
    Parity ParityCheck(BeatCutData lastCut, ref BeatCutData currentSwing, List<NoteEditorData> bombs, float playerXOffset, bool rightHand);
    bool UpsideDown { get; }
}

public class DefaultParityCheck : IParityMethod
{
    public bool UpsideDown { get { return _upsideDown; } }
    private bool _upsideDown;

    // Returns true if the inputted note and bomb coordinates cause a reset potentially
    private Dictionary<int, Func<Vector2, int, int, Parity, bool>> _bombDetectionConditions = new()
    {
        { 0, (note, x, y, parity) => ((y >= note.y && y != 0) || (y > note.y && y > 0)) && x == note.x },
        { 1, (note, x, y, parity) => ((y <= note.y && y != 2) || (y < note.y && y < 2)) && x == note.x },
        { 2, (note, x, y, parity) => (parity == Parity.Forehand && (y == note.y || y == note.y - 1) && ((note.x != 3 && x < note.x) || (note.x < 3 && x <= note.x))) ||
            (parity == Parity.Backhand && y == note.y && ((note.x != 0 && x < note.x) || (note.x > 0 && x <= note.x))) },
        { 3, (note, x, y, parity) => (parity == Parity.Forehand && (y == note.y || y == note.y - 1) && ((note.x != 0 && x > note.x) || (note.x > 0 && x >= note.x))) ||
            (parity == Parity.Backhand && y == note.y && ((note.x != 3 && x > note.x) || (note.x < 3 && x >= note.x))) },
        { 4, (note, x, y, parity) => ((y >= note.y && y != 0) || (y > note.y && y > 0)) && x == note.x && x != 3 && parity != Parity.Forehand },
        { 5, (note, x, y, parity) => ((y >= note.y && y != 0) || (y > note.y && y > 0)) && x == note.x && x != 0 && parity != Parity.Forehand },
        { 6, (note, x, y, parity) => ((y <= note.y && y != 2) || (y < note.y && y < 2)) && x == note.x && x != 3 && parity != Parity.Backhand },
        { 7, (note, x, y, parity) => ((y <= note.y && y != 2) || (y < note.y && y < 2)) && x == note.x && x != 0 && parity != Parity.Backhand },
        { 8, (note,x,y, parity) => false }
    };

    public bool BombResetCheck(BeatCutData lastCut, List<NoteEditorData> bombs)
    {
        // Not found yet
        bool bombResetIndicated = false;
        for (int i = 0; i < bombs.Count; i++)
        {
            // Get current bomb
            NoteEditorData bomb = bombs[i];
            NoteEditorData note;

            // If in the center 2 grid spaces, no point trying
            if ((bomb.column == 1 || bomb.column == 2) && bomb.row == 1) continue;

            // Get the last note. In the case of a stack, picks the note that isnt at 2 or 0 as
            // it triggers a reset when it shouldn't.

            note = lastCut.notesInCut.Where(note => note.column == lastCut.endPositioning.x && note.row == lastCut.endPositioning.y).FirstOrDefault();

            // Get the last notes cut direction based on the last swings angle
            var lastNoteCutDir = (lastCut.sliceParity == Parity.Forehand) ?
                SliceMap.ForehandDict.FirstOrDefault(x => x.Value == Math.Round(lastCut.startPositioning.angle / 45.0) * 45).Key :
                SliceMap.BackhandDict.FirstOrDefault(x => x.Value == Math.Round(lastCut.startPositioning.angle / 45.0) * 45).Key;

            // Offset the checking if the entire outerlane bombs indicate moving inwards
            int xOffset = 0;

            bool bombOffsetting = bombs.Any(bomb => bomb.column == note.column && (bomb.row <= note.row && lastCut.sliceParity == Parity.Backhand && lastCut.endPositioning.angle >= 0)) ||
                bombs.Any(bomb => bomb.column == note.column && (bomb.row >= note.row && lastCut.sliceParity == Parity.Forehand && lastCut.endPositioning.angle >= 0));

            if (bombOffsetting && note.column == 0) xOffset = 1;
            if (bombOffsetting && note.column == 3) xOffset = -1;

            // Determine if lastnote and current bomb cause issue
            // If we already found reason to reset, no need to try again
            bombResetIndicated = _bombDetectionConditions[lastNoteCutDir](new Vector2(note.column + xOffset, note.row), bomb.column, bomb.row, lastCut.sliceParity);
            if (bombResetIndicated) return true;
        }
        return false;
    }

    public Parity ParityCheck(BeatCutData lastCut, ref BeatCutData currentSwing, List<NoteEditorData> bombs, float playerXOffset, bool rightHand)
    {
        // AFN: Angle from neutral
        // Assuming a forehand down hit is neutral, and a backhand up hit
        // Rotating the hand inwards goes positive, and outwards negative
        // Using a list of definitions, turn cut direction into an angle, and check
        // if said angle makes sense.

        NoteEditorData nextNote = currentSwing.notesInCut[0];

        float currentAFN = (lastCut.sliceParity != Parity.Forehand) ?
            SliceMap.BackhandDict[(int)lastCut.notesInCut[0].cutDirection] :
            SliceMap.ForehandDict[(int)lastCut.notesInCut[0].cutDirection];

        int orient = (int)nextNote.cutDirection;
        if(orient == 8) orient = (lastCut.sliceParity == Parity.Forehand) ?
                SliceMap.BackhandDict.FirstOrDefault(x => x.Value == Math.Round(lastCut.endPositioning.angle / 45.0) * 45).Key :
                SliceMap.ForehandDict.FirstOrDefault(x => x.Value == Math.Round(lastCut.endPositioning.angle / 45.0) * 45).Key;

        float nextAFN = (lastCut.sliceParity == Parity.Forehand) ?
            SliceMap.BackhandDict[orient] :
            SliceMap.ForehandDict[orient];

        float angleChange = currentAFN - nextAFN;
        _upsideDown = false;

        // Determines if potentially an upside down hit based on note cut direction and last swing angle
        if (lastCut.sliceParity == Parity.Backhand && lastCut.endPositioning.angle > 0 && ((int)nextNote.cutDirection == 0 || (int)nextNote.cutDirection == 8)) {
            _upsideDown = true;
        }
        else if (lastCut.sliceParity == Parity.Forehand && lastCut.endPositioning.angle > 0 && ((int)nextNote.cutDirection == 1 || (int)nextNote.cutDirection == 8)) {
            _upsideDown = true;
        }

        // Check if bombs are in the position to indicate a reset
        bool bombResetIndicated = BombResetCheck(lastCut, bombs);

        // Want to do a seconday check:
        // Checks whether resetting will cause another reset, which helps to catch some edge cases
        // in bomb detection where it triggers for decor bombs.
        bool bombResetParityImplied = false;
        if (bombResetIndicated) {
            if ((int)nextNote.cutDirection == 8 && lastCut.notesInCut.All(x => (int)x.cutDirection == 8)) bombResetParityImplied = true;
            else {
                // In case of dots, calculate using previous swing swing-angle
                int altOrient = (lastCut.sliceParity == Parity.Forehand) ?
                        SliceMap.ForehandDict.FirstOrDefault(x => x.Value == Math.Round(lastCut.endPositioning.angle / 45.0) * 45).Key :
                        SliceMap.BackhandDict.FirstOrDefault(x => x.Value == Math.Round(lastCut.endPositioning.angle / 45.0) * 45).Key;

                if (lastCut.sliceParity == Parity.Forehand)
                {
                    if (Mathf.Abs(SliceMap.ForehandDict[altOrient] + SliceMap.BackhandDict[(int)nextNote.cutDirection]) >= 90) { bombResetParityImplied = true; }
                } else
                {
                    if (Mathf.Abs(SliceMap.BackhandDict[altOrient] + SliceMap.ForehandDict[(int)nextNote.cutDirection]) >= 90) { bombResetParityImplied = true; }
                }
            }
        }

        if (bombResetIndicated && bombResetParityImplied)
        {
            // Set as bomb reset and return same parity as last swing
            currentSwing.resetType = ResetType.Bomb;
            return (lastCut.sliceParity == Parity.Forehand) ? Parity.Forehand : Parity.Backhand;
        }

        // AKA, If a 180 anticlockwise (right) clockwise (left) rotation
        if (lastCut.endPositioning.angle == 180) {
            var altNextAFN = 180 + nextAFN;
            if(altNextAFN >= 0) {
                return (lastCut.sliceParity == Parity.Forehand) ? Parity.Backhand : Parity.Forehand;
            } else {
                currentSwing.resetType = ResetType.Normal;
                return (lastCut.sliceParity == Parity.Forehand) ? Parity.Forehand : Parity.Backhand;
            }
        }

        // If the angle change exceeds 180 even after accounting for bigger rotations then triangle
        if (Mathf.Abs(angleChange) > 180 && !UpsideDown)
        {
            currentSwing.resetType = ResetType.Normal;
            return (lastCut.sliceParity == Parity.Forehand) ? Parity.Forehand : Parity.Backhand;
        }
        else { return (lastCut.sliceParity == Parity.Forehand) ? Parity.Backhand : Parity.Forehand; }
    }
}