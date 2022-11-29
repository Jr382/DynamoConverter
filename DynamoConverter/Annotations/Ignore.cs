namespace DynamoConverter.Annotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class Ignore : Attribute
    {
        public readonly bool IgnoreField = true;
    }
}