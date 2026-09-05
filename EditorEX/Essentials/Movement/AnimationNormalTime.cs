namespace EditorEX.Essentials.Movement
{
    internal static class AnimationNormalTime
    {
        internal static bool TryCompute(
            float? noodleTimeProperty,
            float playheadSeconds,
            float noteSeconds,
            float jumpDuration,
            out float normalTime
        )
        {
            if (noodleTimeProperty.HasValue)
            {
                normalTime = noodleTimeProperty.Value;
                return !float.IsNaN(normalTime);
            }

            if (jumpDuration <= 0f || float.IsNaN(jumpDuration))
            {
                normalTime = 0f;
                return false;
            }

            float elapsedTime = playheadSeconds - (noteSeconds - (jumpDuration * 0.5f));
            normalTime = elapsedTime / jumpDuration;
            return !float.IsNaN(normalTime);
        }
    }
}
