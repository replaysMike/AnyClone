using System;

namespace AnyClone.Tests.TestObjects
{
    public class VeryDifferentBasicObject : IEquatable<VeryDifferentBasicObject>
    {
        public bool BoolValue { get; set; }
        public byte ByteValue { get; set; }
        public string StringValue { get; set; }
        public int UniqueIntProperty { get; set; }

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(VeryDifferentBasicObject))
                return false;

            var basicObject = (VeryDifferentBasicObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(VeryDifferentBasicObject other)
        {
            if (other == null)
                return false;
            return 
                other.BoolValue == BoolValue
                && other.ByteValue == ByteValue
                && other.StringValue == StringValue
                && other.UniqueIntProperty == UniqueIntProperty;
        }
    }
}
