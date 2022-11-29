using System.Collections;
using Amazon.DynamoDBv2.Model;
using DynamoConverter.Annotations;
using DynamoConverter.Extensions;
using System.Reflection;

namespace DynamoConverter.Deserializer
{
    public partial class DynamoDeserializer
    {
        private readonly Dictionary<Type, Func<AttributeValue, object>> _customConversions;
        private readonly Dictionary<Type, Func<AttributeValue, object>> _primaryConversions;

        public DynamoDeserializer(
            Dictionary<Type, Func<AttributeValue, object>> primaryConversions,
            Dictionary<Type, Func<AttributeValue, object>> customConversions)
        {
            _customConversions = customConversions;
            _primaryConversions = primaryConversions;
        }

        public DynamoDeserializer()
            : this(PopulatePrimaryConversions(), PopulateCustomConversions())
        {
        }

        private object DeserializeAttribute(Type type, AttributeValue attribute)
        {
            object deserialized;
            if (_primaryConversions.ContainsKey(type)) deserialized = _primaryConversions[type](attribute);
            else if (_customConversions.ContainsKey(type)) deserialized = _customConversions[type](attribute);
            else if (type.IsClass) deserialized = DeserializeObjectAttribute(type, attribute);
            else throw new InvalidCastException("Not found valid deserialization for the current attribute");
            
            return deserialized;
        }

        private static object CastString(Type type, string value)
        {
            object deserialized;
            if (type.IsEnum) deserialized = Enum.Parse(type, value);
            else if (value.TryParse(type, out var casted)) deserialized = casted!;
            else throw new InvalidCastException("Not found valid deserialization for the current attribute");

            return deserialized;
        }

        private object Deserialize(IDictionary<string, AttributeValue> item, Type type)
        {
            var instance = Activator.CreateInstance(type)!;
            foreach (var attribute in type.GetAttributes())
            {
                var key = attribute.GetCustomAttribute<Alias>()?.Name ?? attribute.Name;
                if (item.ContainsKey(key)) SetAttributeValue(instance, attribute, item[key]);
            }

            return instance;
        }

        public T Deserialize<T>(Dictionary<string, AttributeValue> item)
        {
            var instance = Activator.CreateInstance<T>()!;
            foreach (var attribute in typeof(T).GetAttributes())
            {
                var key = attribute.GetCustomAttribute<Alias>()?.Name ?? attribute.Name;
                if (item.ContainsKey(key)) SetAttributeValue(instance, attribute, item[key]);
            }

            return instance;
        }

        private void SetAttributeValue(object instance, MemberInfo attribute, AttributeValue value)
        {
            switch (attribute.MemberType)
            {
                case MemberTypes.Field:
                    var fieldInfo = (FieldInfo)attribute;
                    var fieldType = Nullable.GetUnderlyingType(fieldInfo.FieldType) ?? fieldInfo.FieldType;
                    var deserialized = DeserializeAttribute(fieldType, value);
                    fieldInfo.SetValue(instance, deserialized);
                    break;
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo)attribute;
                    var propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                    propertyInfo.SetValue(instance, DeserializeAttribute(propertyType, value));
                    break;
            }
        }

        private object DeserializeObjectAttribute(Type type, AttributeValue attribute)
        {
            object deserialized;
            if (_customConversions.ContainsKey(type))
                deserialized = _customConversions[type](attribute);
            else if (type.IsNumericEnumerable())
                deserialized = DeserializeEnumerable(attribute.NS, type);
            else if (type.IsStringEnumerable())
                deserialized = DeserializeEnumerable(attribute.SS, type);
            else if (type.IsSequence())
                deserialized = DeserializeEnumerable(attribute.L, type);
            else if (type.IsDictionary())
                deserialized = DeserializeDictionary(attribute.M, type);
            else if (type.IsClass)
                deserialized = Deserialize(attribute.M, type);
            else throw new InvalidCastException("Not found valid deserialization for the current attribute");

            return deserialized;
        }

        private IDictionary DeserializeDictionary(Dictionary<string, AttributeValue> dictionary,
            Type type)
        {
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];
            var deserialized = (IDictionary) Activator.CreateInstance(type)!;
            foreach (var (key, attribute) in dictionary)
                deserialized.Add(CastString(keyType, key), DeserializeAttribute(valueType, attribute));
            

            return deserialized;
        }

        private IEnumerable DeserializeEnumerable(IEnumerable<AttributeValue> list, Type type)
        {
            var innerType = type.IsArray ? type.GetElementType()! : type.GetGenericArguments()[0];
            var values = CreateArray(innerType, list.Select(item => DeserializeAttribute(innerType, item)));
            var deserialized = type.IsArray ? values : (IEnumerable)Activator.CreateInstance(type, values)!;

            return deserialized;
        }
        
        private IEnumerable DeserializeEnumerable(IEnumerable<string> list, Type type)
        {
            var innerType = type.IsArray ? type.GetElementType()! : type.GetGenericArguments()[0];
            var values = CreateArray(innerType, list.Select(item => CastString(innerType, item)));
            var deserialized = type.IsArray ? values : (IEnumerable)Activator.CreateInstance(type, values)!;

            return deserialized;
        }

        private IEnumerable CreateArray(Type type, IEnumerable<object> items)
        {
            var enumerable = items as object[] ?? items.ToArray();
            var array = Array.CreateInstance(type, enumerable.Length);
            var index = 0;
            foreach (var item in enumerable)
            {
                array.SetValue(item, index);
                index++;
            }

            return array;
        }

        public void AddConversion(Type type, Func<AttributeValue, object> conversion)
        {
            if (_customConversions.ContainsKey(type)) _customConversions[type] = conversion;
            else _customConversions.Add(type, conversion);
        }

        public void AddConversions(List<Tuple<Type, Func<AttributeValue, object>>> conversions)
        {
            foreach (var conversion in conversions)
            {
                AddConversion(conversion.Item1, conversion.Item2);
            }
        }
    }
}