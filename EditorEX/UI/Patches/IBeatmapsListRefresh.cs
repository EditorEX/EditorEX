namespace EditorEX.UI.Patches
{
    /// <summary>
    /// Seam for triggering a beatmaps-list source/cell refresh without depending on the concrete
    /// UI patch that owns the segmented source control (e.g. from context menu actions that
    /// add/rename/delete sources).
    /// </summary>
    internal interface IBeatmapsListRefresh
    {
        void ReloadCells();
    }
}
