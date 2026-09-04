using System;
using System.Collections.Generic;
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

    protected override List<MemberInfo> GetSerializableMembers(Type objectType)
    {
        if (!ShouldUseVanillaSerializableFields(objectType))
        {
            return base.GetSerializableMembers(objectType);
        }

        // GetSerializableMembers() ignores CreateProperties' MemberSerialization argument and
        // keys off [Serializable] on the runtime type. CustomJSONData *SaveData subclasses are
        // not [Serializable], so OptOut would emit beat/lineIndex. Collect the same instance
        // fields vanilla uses, plus the customData property once.
        List<MemberInfo> members = new();
        Type? current = objectType;
        while (current != null && current != typeof(object))
        {
            foreach (
                FieldInfo field in current.GetFields(
                    BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic
                        | BindingFlags.DeclaredOnly
                )
            )
            {
                if (IsCustomDataBackingField(field.Name))
                {
                    continue;
                }

                members.Add(field);
            }

            current = current.BaseType;
        }

        PropertyInfo? customData = objectType.GetProperty(
            "customData",
            BindingFlags.Instance | BindingFlags.Public
        );
        if (customData != null)
        {
            members.Add(customData);
        }

        return members;
    }

    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization
    )
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        // NoteSaveData is not [Serializable], so CreateProperty runs as OptOut and marks
        // private vanilla fields (_time, b, …) unreadable. Keep those fields writable.
        if (member is FieldInfo field && IsVanillaBeatmapSaveField(field))
        {
            property.Ignored = false;
            property.Readable = true;
            property.Writable = !field.IsInitOnly && !field.IsLiteral;
        }

        if (property.PropertyType == typeof(CustomData))
        {
            if (
                property.PropertyName == "customData"
                && MapContext.Version != null
                && MapContext.Version.Major < 3
            )
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

    private static bool ShouldUseVanillaSerializableFields(Type objectType)
    {
        if (!typeof(ICustomData).IsAssignableFrom(objectType))
        {
            return false;
        }

        Type? current = objectType.BaseType;
        while (current != null && current != typeof(object))
        {
            if (current.IsDefined(typeof(SerializableAttribute), inherit: false))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static bool IsVanillaBeatmapSaveField(FieldInfo field)
    {
        if (IsCustomDataBackingField(field.Name))
        {
            return false;
        }

        string? ns = field.DeclaringType?.Namespace;
        return ns == "BeatmapSaveDataVersion2_6_0AndEarlier"
            || ns == "BeatmapSaveDataVersion3"
            || ns == "BeatmapSaveDataVersion4"
            || ns == "BeatmapSaveDataCommon";
    }

    private static bool IsCustomDataBackingField(string name)
    {
        return name.IndexOf("customData", StringComparison.Ordinal) >= 0
            && name.IndexOf("k__BackingField", StringComparison.Ordinal) >= 0;
    }
}
