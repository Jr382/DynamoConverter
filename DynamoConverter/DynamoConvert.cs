using Amazon.DynamoDBv2.Model;
using DynamoConverter.Deserializer;
using DynamoConverter.Serializer;
using System.Collections;

namespace DynamoConverter
{
    public static class DynamoConvert
    {
        private static readonly DynamoSerializer Serializer = new ();
        private static readonly DynamoDeserializer Deserializer = new ();

        public static Dictionary<string, AttributeValue>? SerializeObject(object? value, string prefix = "")
        {
            return value != null ? (Dictionary<string, AttributeValue>) Serializer.Serialize(value, prefix) : null;
        }

        public static List<AttributeValue>? SerializeObject(IEnumerable? enumerable)
        {
            return enumerable != null ? (List<AttributeValue>) Serializer.Serialize(enumerable) : null;
        }

        public static Dictionary<string, AttributeValue>? SerializeObject(IDictionary? dictionary)
        {
            return dictionary != null ? (Dictionary<string, AttributeValue>) Serializer.Serialize(dictionary) : null;
        }

        public static T? DeserializeObject<T>(Dictionary<string, AttributeValue>? value) where T : class
        {
            return value != null ? Deserializer.Deserialize<T>(value) : default;
        }

        public static IEnumerable<T> DeserializeObject<T>(IEnumerable<Dictionary<string, AttributeValue>?> values) where T : class
        {
            return values
                .Where(value => value != null)
                .Select(value => Deserializer.Deserialize<T>(value!));
        }

        public static void AddDeserialization(Type type, Func<AttributeValue, object> conversion) =>
            Deserializer.AddConversion(type, conversion);
        
        public static void AddDeserializations(
            List<Tuple<Type, Func<AttributeValue, object>>> conversions)
        {
            Deserializer.AddConversions(conversions);
        }
    }
}