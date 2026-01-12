using System;
using Newtonsoft.Json;

public interface IOptionalValue
{
    public bool HasValue { get; }
    public object Value { get; set; }
}

/// <summary>
/// Wrapper with an additional "HasValue" property to allow for an unset state, 
/// and a set state that includes a possible null or default value.
/// </summary>
/// <typeparam name="T"></typeparam>
public class OptionalValue<T> : IOptionalValue
{
    public bool HasValue => _hasValue;
    private bool _hasValue = false;

    public T Value
    {
        get => _hasValue
            ? _value
            : throw new InvalidOperationException($"OptionalValue has no value.");
        set
        {
            _hasValue = true;
            _value = value;
        }
    }

    object IOptionalValue.Value
    {
        get => Value;
        set => Value = (T)value;
    }

    private T _value;

    public bool TryGetValue(out T value)
    {
        value = _value;
        return _hasValue;
    }

    public void AssignIfSet(ref T target)
    {
        if (_hasValue) target = _value;
    }
}

public sealed class OptionalValueJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
        => typeof(IOptionalValue).IsAssignableFrom(objectType);

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var optional = (IOptionalValue)value;

        if (!optional.HasValue)
        {
            writer.WriteNull();
            return;
        }

        serializer.Serialize(writer, optional.Value);
    }

    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer)
    {
        var result = (IOptionalValue)Activator.CreateInstance(objectType);

        if (reader.TokenType == JsonToken.Null ||
            reader.TokenType == JsonToken.Undefined)
        {
            return result;
        }

        var innerType = objectType.GetGenericArguments()[0];
        var innerValue = serializer.Deserialize(reader, innerType);

        result.Value = innerValue;
        return result;
    }
}
