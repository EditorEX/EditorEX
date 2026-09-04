using System;
using System.Collections.Generic;
using System.Linq;
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

    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        JsonObjectContract contract = base.CreateObjectContract(objectType);
        if (!ShouldUseVanillaSerializableFields(objectType))
        {
            return contract;
        }

        // Vanilla save types are [Serializable] so Newtonsoft writes fields (_time, b, x, …).
        // CustomJSONData's *SaveData subclasses are not [Serializable] and would otherwise
        // emit public property names (beat, lineIndex) instead of the file format.
        contract.MemberSerialization = MemberSerialization.Fields;
        contract.Properties.Clear();
        foreach (JsonProperty property in CreateProperties(objectType, MemberSerialization.Fields))
        {
            contract.Properties.Add(property);
        }

        return contract;
    }

    protected override IList<JsonProperty> CreateProperties(
        Type type,
        MemberSerialization memberSerialization
    )
    {
        IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
        if (
            memberSerialization != MemberSerialization.Fields
            || !typeof(ICustomData).IsAssignableFrom(type)
        )
        {
            return properties;
        }

        PropertyInfo? customData = type.GetProperty(
            "customData",
            BindingFlags.Instance | BindingFlags.Public
        );
        if (customData == null)
        {
            return properties;
        }

        List<JsonProperty> result = properties
            .Where(p => !IsCustomDataBackingField(p.UnderlyingName))
            .ToList();
        JsonProperty customDataProperty = CreateProperty(customData, MemberSerialization.OptOut);
        customDataProperty.Readable = true;
        customDataProperty.Writable = true;
        result.Add(customDataProperty);
        return result;
    }

    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization
    )
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        if (IsCustomDataBackingField(member.Name))
        {
            property.Ignored = true;
            return property;
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

    private static bool IsCustomDataBackingField(string? name)
    {
        return name != null
            && name.IndexOf("customData", StringComparison.Ordinal) >= 0
            && name.IndexOf("k__BackingField", StringComparison.Ordinal) >= 0;
    }
}
