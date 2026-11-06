using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hp2BaseMod;

public sealed class GameStatePayloadConverter : JsonConverter<IGameStatePayload>
{
    private const string IDENTIFIER = "GameStateId";

    public override void WriteJson(
        JsonWriter writer,
        IGameStatePayload value,
        JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var valueType = value.GetType();

        if (!ModInterface.GameData.GameStates.TryGetId(valueType, out var id))
        {
            throw new JsonSerializationException(
                $"Unsupported payload type: {valueType.Name}");
        }

        var payloadSerializer = CreatePayloadSerializer(serializer);

        var obj = JObject.FromObject(value, payloadSerializer);
        obj[IDENTIFIER] = id.ToString();
        obj.WriteTo(writer);
    }

    public override IGameStatePayload ReadJson(
        JsonReader reader,
        Type objectType,
        IGameStatePayload existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var obj = JObject.Load(reader);
        var idStr = (string)obj[IDENTIFIER];

        if (!RelativeId.TryParse(idStr, out var id) ||
            !ModInterface.GameData.GameStates.TryGetPayloadType(id, out var valueType))
        {
            ModInterface.Log.Warning(
                $"Failed to find payload type for id {idStr}. Returning a null payload.");

            return null;
        }

        // Deserialize the concrete type without invoking this converter again.
        var payloadSerializer = CreatePayloadSerializer(serializer);

        return (IGameStatePayload)obj.ToObject(valueType, payloadSerializer);
    }

    private static JsonSerializer CreatePayloadSerializer(JsonSerializer source)
    {
        var serializer = new JsonSerializer
        {
            Culture = source.Culture,
            ContractResolver = source.ContractResolver,
            Context = source.Context,
            ConstructorHandling = source.ConstructorHandling,
            DateFormatHandling = source.DateFormatHandling,
            DateFormatString = source.DateFormatString,
            DateParseHandling = source.DateParseHandling,
            DateTimeZoneHandling = source.DateTimeZoneHandling,
            DefaultValueHandling = source.DefaultValueHandling,
            FloatFormatHandling = source.FloatFormatHandling,
            FloatParseHandling = source.FloatParseHandling,
            Formatting = source.Formatting,
            MaxDepth = source.MaxDepth,
            MetadataPropertyHandling = source.MetadataPropertyHandling,
            MissingMemberHandling = source.MissingMemberHandling,
            NullValueHandling = source.NullValueHandling,
            ObjectCreationHandling = source.ObjectCreationHandling,
            PreserveReferencesHandling = source.PreserveReferencesHandling,
            ReferenceLoopHandling = source.ReferenceLoopHandling,
            StringEscapeHandling = source.StringEscapeHandling,
            TypeNameAssemblyFormatHandling = source.TypeNameAssemblyFormatHandling,
            TypeNameHandling = source.TypeNameHandling,
            CheckAdditionalContent = source.CheckAdditionalContent
        };

        foreach (var converter in source.Converters)
        {
            if (converter is not GameStatePayloadConverter)
                serializer.Converters.Add(converter);
        }

        return serializer;
    }
}