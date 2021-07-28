using System;

namespace AnyClone.Tests.TestObjects
{
    public class ReadOnlyReferenceFieldObject : IEquatable<ReadOnlyReferenceFieldObject>
    {
        private readonly BasicObject _constField;

        public ReadOnlyReferenceFieldObject() { }

        public ReadOnlyReferenceFieldObject(BasicObject initialValue)
        {
            _constField = initialValue;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj)
        {
            var basicObject = (ReadOnlyReferenceFieldObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(ReadOnlyReferenceFieldObject other)
        {
            var e1 = _constField.Equals(other?._constField);

            return e1;
        }
    }
}
