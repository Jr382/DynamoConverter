using DynamoConverter.Enums;

namespace DynamoConverter.Annotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializeBy : Attribute
    {
        public readonly ReturnType ReturnType;
        
        public SerializeBy(string returnType)
        {
            if (Enum.TryParse<ReturnType>(returnType, true, out var castedReturnType))
            {
                ReturnType = castedReturnType;
            }
            else throw new ArgumentException("Invalid return type");
        }
        
        public SerializeBy(ReturnType returnType)
        {
            ReturnType = returnType;
        }
    }
}