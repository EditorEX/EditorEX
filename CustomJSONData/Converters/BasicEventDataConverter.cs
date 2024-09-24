using BeatmapSaveDataVersion2_6_0AndEarlier;
using Newtonsoft.Json;
using System;

namespace BetterEditor.CustomJSONData.Converters
{
    public class V2BasicEventDataWithoutFloatValueConverter : JsonConverter<BeatmapSaveData.EventData>
    {
        public override BeatmapSaveData.EventData ReadJson(JsonReader reader, Type objectType, BeatmapSaveData.EventData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float time = 0f;
            BeatmapSaveData.BeatmapEventType type = 0;
            int value = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = (string)reader.Value;
                    reader.Read();
                    switch (propertyName)
                    {
                        case "_time":
                            time = (float)(long)reader.Value;
                            break;
                        case "_type":
                            type = (BeatmapSaveData.BeatmapEventType)(int)(long)reader.Value;
                            break;
                        case "_value":
                            value = (int)(long)reader.Value;
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }
            return new BeatmapSaveData.EventData(time, type, value, 0f);
        }

        public override void WriteJson(JsonWriter writer, BeatmapSaveData.EventData value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("_time");
            writer.WriteValue(value.time);
            writer.WritePropertyName("_type");
            writer.WriteValue((int)value.type);
            writer.WritePropertyName("_value");
            writer.WriteValue(value.value);
            writer.WriteEndObject();
        }
    }
}
