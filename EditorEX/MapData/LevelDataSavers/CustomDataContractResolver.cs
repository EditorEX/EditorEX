using System.Collections;
using System.Reflection;
using CustomJSONData.CustomBeatmap;
using EditorEX.MapData.Contexts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EditorEX.MapData.LevelDataSavers;

public class CustomDataContractResolver : DefaultContractResolver
{
    public static readonly CustomDataContractResolver Instance = new()
    {
        IgnoreSerializableAttribute = false,
    };

    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization
    )
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        if (property.PropertyType == typeof(CustomData))
        {
            Plugin.Logger.Info(property.PropertyName ?? "son");
            if (property.PropertyName == "customData" && MapContext.Version.Major < 3)
            {
                property.PropertyName = "_customData";
            }

            property.ShouldSerialize = instance =>
            {
                var value = property.ValueProvider?.GetValue(instance) as CustomData;
                return value?.Count > 0;
            };
        }

        return property;
    }
}
