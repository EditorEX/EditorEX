using NoodleExtensions.Managers;

namespace EditorEX.NoodleExtensions.Events
{
    internal static class AssignPlayerToTrackOwnership
    {
        public static bool Conflicts(PlayerObject left, PlayerObject right) => left == right;
    }
}
