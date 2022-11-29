using Amazon.DynamoDBv2.Model;

namespace DynamoConverter.Models
{
    public class SerializeRule
    {
        public Func<Type, bool> Constraint;
        public Func<Field, AttributeValue> Conversion;
    }
}