namespace DynamoConverter.Annotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class Alias : Attribute
    {
        public readonly string? Name;

        public Alias(string? name)
        {
            Name = name;
        }
    }
}