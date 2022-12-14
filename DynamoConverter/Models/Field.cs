using System.Reflection;
using DynamoConverter.Annotations;
using DynamoConverter.Enums;

namespace DynamoConverter.Models
{
    public class Field
    {
        public Type Type;
        public object Value;
        public string Name;
        public ReturnType? ReturnType;
        public bool IgnoreField;
        public string? Alias;

        public Field(MemberInfo memberInfo, Type type, object value)
        {
            Name = memberInfo.Name;
            ReturnType = memberInfo.GetCustomAttribute<SerializeBy>()?.ReturnType;
            IgnoreField = memberInfo.GetCustomAttribute<Ignore>()?.IgnoreField ?? false;
            Alias = memberInfo.GetCustomAttribute<Alias>()?.Name;
            Type = type;
            Value = value;
        }

        public Field()
        { }
    }
}