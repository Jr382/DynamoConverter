using Amazon.DynamoDBv2.Model;
using DynamoConverter.Enums;
using DynamoConverter.Models;

namespace DynamoConverter.Serializer
{
    public partial class DynamoSerializer
    {
        private static Dictionary<Type, Func<Field, AttributeValue>> PopulatePrimaryConversions() =>
            new()
            {
                { typeof(string), SerializeCharacterType },
                { typeof(char), SerializeCharacterType },
                { typeof(sbyte), SerializeNumericType },
                { typeof(byte), SerializeNumericType },
                { typeof(short), SerializeNumericType },
                { typeof(ushort), SerializeNumericType },
                { typeof(int), SerializeNumericType },
                { typeof(uint), SerializeNumericType },
                { typeof(long), SerializeNumericType },
                { typeof(ulong), SerializeNumericType },
                { typeof(float), SerializeNumericType },
                { typeof(decimal), SerializeNumericType },
                { typeof(double), SerializeNumericType },
                { typeof(bool), SerializeBooleanType }
            };

        private static Dictionary<Type, Func<Field, AttributeValue>> PopulateCustomConversions() =>
            new()
            {
                { typeof(DateTime), SerializeDateType },
                { typeof(DateTimeOffset), SerializeDateType },
                { typeof(Guid), field => new AttributeValue(((Guid)field.Value).ToString()) }
            };

        private static List<SerializeRule> PopulateCustomRules() => 
            new ()
            {
                GenerateRule(type => type.IsEnum, SerializeEnum)
            };
        

        private static SerializeRule GenerateRule(Func<Type, bool> rule, Func<Field, AttributeValue> conversion)
        {
            return new SerializeRule
            {
                Constraint = rule,
                Conversion = conversion
            };
        }

        private static AttributeValue SerializeEnum(Field field)
        {
            var enumerable = (Enum) field.Value;
            var attribute = new AttributeValue();
            if (field.ReturnType == ReturnType.HashCode) attribute.N = $"{enumerable.GetHashCode()}";
            else attribute.S = enumerable.ToString();
            return attribute;
        }

        private static AttributeValue SerializeDateType(Field field)
        {
            var dateTime = (DateTime)field.Value;
            return new AttributeValue { S = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") };
        }

        private static AttributeValue SerializeNumericType(Field field)
        {
            var number = field.Value;
            return new AttributeValue { N = number.ToString() };
        }

        private static AttributeValue SerializeCharacterType(Field field)
        {
            var str = (string)field.Value;
            return new AttributeValue { S = str };
        }

        private static AttributeValue SerializeBooleanType(Field field)
        {
            var boolean = (bool)field.Value;
            return new AttributeValue { BOOL = boolean };
        }
    }
}