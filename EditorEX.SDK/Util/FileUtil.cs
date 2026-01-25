namespace EditorEX.Util
{
    public static class FileUtil
    {
        public static void OpenFileBrowser(string path)
        {
            path = path.Replace("/", "\\").Replace("\\\\", "\\");

            if (!path.StartsWith("\""))
                path = "\"" + path;
            if (!path.EndsWith("\""))
                path += "\"";

            System.Diagnostics.Process.Start("explorer.exe", $"{path}");
        }
    }
}
