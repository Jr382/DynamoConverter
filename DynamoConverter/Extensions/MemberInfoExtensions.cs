using DynamoConverter.Annotations;
using System.Reflection;
using DynamoConverter.Models;

namespace DynamoConverter.Extensions
{
    public static class MemberInfoExtensions
    {
        public static bool TryParse(this MemberInfo info, object? item, out Field castedField)
        {
            var successful = false;
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    var fieldInfo = (FieldInfo) info;
                    var fieldValue = item != null ? fieldInfo.GetValue(item) : null;
                    castedField = fieldValue != null ? new Field(info, fieldInfo.FieldType, fieldValue) : null!;
                    successful = fieldValue != null;
                    break;
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo) info;
                    var propertyValue = item != null ? propertyInfo.GetValue(item) : null;
                    castedField = propertyValue != null ? new Field(info, propertyInfo.PropertyType, propertyValue) : null!;
                    successful = propertyValue != null;
                    break;
                default:
                    castedField = default!;
                    break;
            }

            return successful;
        }

        public static List<MemberInfo> GetAttributes(this Type type)
        {
            var attributes = new List<MemberInfo>();
            attributes.AddRange(type.GetProperties());
            attributes.AddRange(type.GetFields());
            return attributes;
        }
    }
}