using System.Reflection;

//https://stackoverflow.com/a/14210097
namespace EditorEX.CustomJSONData.Util
{
    public static class BackingFieldUtil
    {
        private static string GetBackingFieldName(string propertyName)
        {
            return string.Format("<{0}>k__BackingField", propertyName);
        }

        public static FieldInfo GetBackingField<T>(string propertyName)
        {
            return typeof(T).GetField(GetBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}
