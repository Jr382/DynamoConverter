using Amazon.DynamoDBv2.Model;
using DynamoConverter.Extensions;
using System.Collections;
using System.Diagnostics;
using DynamoConverter.Models;

namespace DynamoConverter.Serializer
{
    public partial class DynamoSerializer
    {
        private static Dictionary<Type, Func<Field, AttributeValue>> _primaryConversions = null!;
        private static List<SerializeRule> _customConversionsRules = null!;
        private static Dictionary<Type, Func<Field, AttributeValue>> _customConversions = null!;

        private DynamoSerializer(
            Dictionary<Type, Func<Field, AttributeValue>> customConversions,
            Dictionary<Type, Func<Field, AttributeValue>> primaryConversions,
            List<SerializeRule> customConversionsRules)
        {
            _customConversions = customConversions;
            _primaryConversions = primaryConversions;
            _customConversionsRules = customConversionsRules;
        }

        public DynamoSerializer() :
            this(PopulateCustomConversions(), PopulatePrimaryConversions(), PopulateCustomRules())
        {
        }

        public object Serialize(object item, string prefix = "")
        {
            var type = item.GetType();
            object casted = null!;
            if (type.IsSequence()) casted = SerializeEnumerable((IEnumerable)item);
            else if (type.IsDictionary()) casted = SerializeDictionary((IDictionary)item, prefix);
            else if (type.IsClass) casted = SerializeObject(item, prefix);
            return casted;
        }
        
        private Dictionary<string, AttributeValue> SerializeObject(object item, string prefix = "")
        {
            var serializeObject = new Dictionary<string, AttributeValue>();
            foreach (var attribute in item.GetType().GetAttributes())
            {
                if (!attribute.TryParse(item, out var castedField) || castedField.IgnoreField) continue;
                var fieldName = castedField.Alias ?? castedField.Name;
                serializeObject.Add(prefix + fieldName, SerializeField(castedField));
            }

            return serializeObject;
        }

        private AttributeValue SerializeField(Field field)
        {
            AttributeValue serialized;
            if (field.Type.IsPrimitive || field.Type == typeof(string)) serialized = _primaryConversions[field.Type](field);
            else if (_customConversions.TryGetValue(field.Type, out var conversion)) serialized = conversion(field);
            else serialized = SerializeByCustomRule(field);

            return serialized;
        }

        private AttributeValue SerializeByCustomRule(Field field)
        {
            var serialized = new AttributeValue();
            var serializeRules = _customConversionsRules.Where(rule => rule.Constraint(field.Type)).ToList();
            Debug.Assert(serializeRules.Count <= 1, "Found multiples serializations for the current object");
            if (serializeRules.Any()) serialized = serializeRules.First().Conversion(field);
            else if (field.Type.IsNumericEnumerable()) serialized.NS = SerializeStringEnumerable((IEnumerable)field.Value);
            else if (field.Type.IsStringEnumerable()) serialized.SS = SerializeStringEnumerable((IEnumerable)field.Value);
            else if (field.Type.IsSequence()) serialized.L = SerializeEnumerable((IEnumerable)field.Value);
            else if (field.Type.IsDictionary()) serialized.M = SerializeDictionary((IDictionary)field.Value);
            else if (field.Type.IsClass) serialized.M = SerializeObject(field.Value);
            else throw new InvalidCastException("Not found valid serialization for the current object");

            return serialized;
        }
        
        private static List<string> SerializeStringEnumerable(IEnumerable list)
        {
            return list.Cast<object>()
                .Where(item => item != null)
                .Select(item => item.ToString())
                .ToList()!;
        }
        
        private List<AttributeValue> SerializeEnumerable(IEnumerable enumerable)
        {
            var serializeEnumerable = new List<AttributeValue>();
            foreach (var item in enumerable)
            {
                var field = new Field { Type = item.GetType(), Value = item };
                serializeEnumerable.Add(SerializeField(field));
            }

            return serializeEnumerable;
        }

        private Dictionary<string, AttributeValue> SerializeDictionary(IDictionary dictionary, string prefix = "")
        {
            var serializeDictionary = new Dictionary<string, AttributeValue>();
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Value == null) continue;
                var field = new Field { Type = entry.Value.GetType(), Value = entry.Value };
                serializeDictionary.Add($"{prefix}{entry.Key}", SerializeField(field));
            }

            return serializeDictionary;
        }

        public static void AddSerialization(Type type, Func<Field, AttributeValue> conversion)
        {
            if (_customConversions.ContainsKey(type)) _customConversions[type] = conversion;
            else _customConversions.Add(type, conversion);
        }

        public static void AddSerialization(Func<Type, bool> rule, Func<Field, AttributeValue> conversion)
        {
            _customConversionsRules.Add(GenerateRule(rule, conversion));
        }
    }
}