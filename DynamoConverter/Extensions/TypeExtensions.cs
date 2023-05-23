using System.Collections;

namespace DynamoConverter.Extensions;

public static class TypeCodeExtensions
{
    public static bool IsNumericEnumerable(this Type type)
    {
        var isNumericEnumerable = false;
        if (type.IsSequence())
        {
            var innerType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
            var typeCode = Type.GetTypeCode(innerType);
            switch (typeCode)
            {
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    isNumericEnumerable = true;
                    break;
                default:
                    isNumericEnumerable = false;
                    break;
            }
        }

        return isNumericEnumerable;
    }
    
    public static bool IsStringEnumerable(this Type type)
    {
        return IsSequence(type) && Type.GetTypeCode(
                   type.IsArray ? type.GetElementType() 
                   : type.GetGenericArguments()[0]).Equals(TypeCode.String);
    }
    
    public static bool IsSequence(this Type type) => typeof(IEnumerable).IsAssignableFrom(type) && !typeof(IDictionary).IsAssignableFrom(type);

    public static bool IsDictionary(this Type type) => typeof(IDictionary).IsAssignableFrom(type);

    public static bool TryParse(this string source, Type type, out object? casted)
    {
        var successful = true;
        casted = default;
        var typeCode = Type.GetTypeCode(type);
        try
        {
            casted = Convert.ChangeType(source, typeCode);
        }
        catch (InvalidCastException)
        {
            successful = false;
        }
        return successful;
    }

}