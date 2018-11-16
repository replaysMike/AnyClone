using System;

namespace AnyClone.Tests.TestObjects
{
    public class BasicObject : IEquatable<BasicObject>
    {
        public bool BoolValue { get; set; }
        public byte ByteValue { get; set; }
        public int IntValue { get; set; }
        public long LongValue { get; set; }
        public string StringValue { get; set; }

        public override bool Equals(object obj)
        {
            var basicObject = (BasicObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(BasicObject other)
        {
            return other.BoolValue == BoolValue
                && other.ByteValue == ByteValue
                && other.IntValue == IntValue
                && other.LongValue == LongValue
                && other.StringValue == StringValue
                ;
        }
    }
}
