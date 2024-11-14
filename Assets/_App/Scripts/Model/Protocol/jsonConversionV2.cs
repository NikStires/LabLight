using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class JsonConversionV2
{
    public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        NullValueHandling = NullValueHandling.Ignore,
        Converters = { new DictionaryConverter() } // Custom converter for dictionaries
    };

    public static ProtocolDefinition DeserializeProtocol(string json)
    {
        try
        {
            var protocol = JsonConvert.DeserializeObject<ProtocolDefinition>(json, SerializerSettings);
            return protocol;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error deserializing protocol: " + ex.Message);
            throw;
        }
    }

    public static string SerializeProtocol(ProtocolDefinition protocol)
    {
        try
        {
            var json = JsonConvert.SerializeObject(protocol, Formatting.Indented, SerializerSettings);
            return json;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error serializing protocol: " + ex.Message);
            throw;
        }
    }
}

public class DictionaryConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(Dictionary<string, object>));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var dict = new Dictionary<string, object>();
        serializer.Populate(reader, dict);
        return dict;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
