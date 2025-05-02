using System;
using System.ComponentModel;
using System.Globalization;

namespace Hp2BaseMod;

[Serializable]
[TypeConverter(typeof(Converter))]
public struct RelativeId
{
    /// <summary>
    /// The id of the source that defined this.
    /// </summary>
    public int SourceId;

    /// <summary>
    /// The id defined by the source.
    /// </summary>
    public int LocalId;

    public RelativeId(int sourceId, int localId)
    {
        SourceId = sourceId;
        LocalId = localId;
    }

    internal RelativeId(Definition def)
    {
        SourceId = -1;
        LocalId = def?.id ?? -1;
    }

    public static RelativeId Default => new RelativeId(-1, -1);
    public static RelativeId Zero => new RelativeId(-1, 0);

    public static bool operator !=(RelativeId x, RelativeId y)
    {
        return !(x == y);
    }

    public static bool operator ==(RelativeId x, RelativeId y)
    {
        return x.SourceId == y.SourceId
               && x.LocalId == y.LocalId;
    }

    public override int GetHashCode()
    {
        int hashCode = -21478398;
        hashCode = hashCode * -1521134295 + SourceId.GetHashCode();
        hashCode = hashCode * -1521134295 + LocalId.GetHashCode();
        return hashCode;
    }

    public override bool Equals(object obj)
    {
        if (obj is RelativeId relativeId)
        {
            return relativeId == this;
        }

        return false;
    }

    public override string ToString() => $"{SourceId}.{LocalId}";

    public static bool TryParse(string str, out RelativeId relativeId)
    {
        if (!string.IsNullOrWhiteSpace(str))
        {
            var split = str.Split('.');

            if (split.Length == 2
                && int.TryParse(split[0], out var sourceId)
                && int.TryParse(split[1], out var localId))
            {
                relativeId = new RelativeId(sourceId, localId);
                return true;
            }
        }

        relativeId = Default;
        return false;
    }

    public class Converter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str && RelativeId.TryParse(str, out var relativeId))
            {
                return relativeId;
            }

            return base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string)
                && value is RelativeId relativeId)
            {
                return relativeId.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
