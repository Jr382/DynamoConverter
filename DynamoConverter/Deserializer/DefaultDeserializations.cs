using Amazon.DynamoDBv2.Model;

namespace DynamoConverter.Deserializer;

public partial class DynamoDeserializer
{
    private static Dictionary<Type, Func<AttributeValue, object>> PopulatePrimaryConversions() =>
        new()
        {
            { typeof(string), attribute =>  attribute.S },
            { typeof(char), attribute => char.Parse(attribute.S) },
            { typeof(sbyte), attribute => sbyte.Parse(attribute.N) },
            { typeof(byte), attribute => byte.Parse(attribute.N) },
            { typeof(short), attribute => short.Parse(attribute.N) },
            { typeof(ushort), attribute => ushort.Parse(attribute.N) },
            { typeof(int), attribute => int.Parse(attribute.N) },
            { typeof(uint), attribute => uint.Parse(attribute.N) },
            { typeof(long), attribute => long.Parse(attribute.N) },
            { typeof(ulong), attribute => ulong.Parse(attribute.N) },
            { typeof(float), attribute => float.Parse(attribute.N) },
            { typeof(decimal), attribute => decimal.Parse(attribute.N) },
            { typeof(double), attribute => double.Parse(attribute.N) },
            { typeof(bool), attribute => attribute.BOOL }
        };
    
    private static Dictionary<Type, Func<AttributeValue, object>> PopulateCustomConversions() =>
        new()
        {
            { typeof(Guid), attribute => new Guid(attribute.S) },
            { typeof(DateTime), attribute => DateTime.Parse(attribute.S) },
            { typeof(DateTimeOffset), attribute => DateTimeOffset.Parse(attribute.S) }
        };
}