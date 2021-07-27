using System;

namespace AnyClone.Tests.TestObjects
{
    public class BasicObject : IEquatable<BasicObject>
    {
        private int _privateIntValue;
        private int _randomIdentifier;
        public bool BoolValue { get; set; }
        public byte ByteValue { get; set; }
        public int IntValue { get; set; }
        public long LongValue { get; set; }
        public string StringValue { get; set; }

        public BasicObject()
        {
            _randomIdentifier = new Random((int)DateTime.UtcNow.Ticks).Next(1, 999999);
        }

        public BasicObject(int privateIntValue) : this()
        {
            _privateIntValue = privateIntValue;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BasicObject))
                return false;

            var basicObject = (BasicObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(BasicObject other)
        {
            if (other == null)
                return false;
            return 
                other._privateIntValue == _privateIntValue
                && other._randomIdentifier == _randomIdentifier
                && other.BoolValue == BoolValue
                && other.ByteValue == ByteValue
                && other.IntValue == IntValue
                && other.LongValue == LongValue
                && other.StringValue == StringValue;
        }
    }
}
