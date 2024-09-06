using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;

/// <summary>
/// Json conversion helper to create concrete AnchorPayload classes
/// </summary>
public class AnchorPayloadConverter : CustomCreationConverter<AnchorPayload>
{
    PayloadType _currentObjectType;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jobj = JObject.ReadFrom(reader);
        _currentObjectType = jobj["payloadType"].ToObject<PayloadType>();
        return base.ReadJson(jobj.CreateReader(), objectType, existingValue, serializer);
    }

    public override AnchorPayload Create(Type objectType)
    {
        switch (_currentObjectType)
        {
            case PayloadType.Note:
                return new SpatialNotePayload();
            case PayloadType.HazardZone:
                return new HazardZonePayload();
            default:
                throw new NotImplementedException();
        }
    }
}