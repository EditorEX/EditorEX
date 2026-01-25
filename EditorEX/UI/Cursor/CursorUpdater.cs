using System;
using System.Runtime.InteropServices;
using EditorEX.UI.Patches;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.UI.Cursor
{
    internal class CursorUpdater : ITickable
    {
        public enum WindowsCursor
        {
            StandardArrowAndSmallHourglass = 32650,
            StandardArrow = 32512,
            Crosshair = 32515,
            Hand = 32649,
            ArrowAndQuestionMark = 32651,
            IBeam = 32513,

            //Icon = 32641, // Obsolete for applications marked version 4.0 or later.
            SlashedCircle = 32648,

            //Size = 32640,  // Obsolete for applications marked version 4.0 or later. Use FourPointedArrowPointingNorthSouthEastAndWest
            FourPointedArrowPointingNorthSouthEastAndWest = 32646,
            DoublePointedArrowPointingNortheastAndSouthwest = 32643,
            DoublePointedArrowPointingNorthAndSouth = 32645,
            DoublePointedArrowPointingNorthwestAndSoutheast = 32642,
            DoublePointedArrowPointingWestAndEast = 32644,
            VerticalArrow = 32516,
            Hourglass = 32514,
        }

        [DllImport("user32.dll", EntryPoint = "SetCursor")]
        public static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll", EntryPoint = "LoadCursor")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        private static void ChangeCursor(WindowsCursor cursor)
        {
            SetCursor(LoadCursor(IntPtr.Zero, (int)cursor));
        }

        public void Tick()
        {
            bool any = false;
            foreach (var selectable in Selectable.allSelectablesArray)
            {
                any |= selectable.IsHighlighted();
            }
            foreach (var selectable in SelectableCellsCursorPatches.SelectableCells)
            {
                any |= selectable.highlighted;
            }
            ChangeCursor(any ? WindowsCursor.Hand : WindowsCursor.StandardArrow);
        }
    }
}
