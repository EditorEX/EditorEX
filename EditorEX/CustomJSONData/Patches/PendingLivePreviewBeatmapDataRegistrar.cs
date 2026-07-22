namespace EditorEX.CustomJSONData.Patches
{
    /// <summary>
    /// Eagerly flushes <see cref="LivePreviewBeatmapDataBuffer"/> into
    /// <see cref="ICustomDataRepository"/> once the models installer has bound the repo.
    /// Safe if this runs after <see cref="PreviewCustomBeatmap"/>; if it runs first, the
    /// buffer stays pending and is picked up on the next <c>GetBeatmapData</c>.
    /// </summary>
    internal class PendingLivePreviewBeatmapDataRegistrar
    {
        private PendingLivePreviewBeatmapDataRegistrar(ICustomDataRepository customDataRepository)
        {
            var pending = LivePreviewBeatmapDataBuffer.Consume();
            if (pending != null)
            {
                customDataRepository.SetBeatmapData(pending);
            }
        }
    }
}
