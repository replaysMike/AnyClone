using System;

namespace AnyClone.Tests.TestObjects
{
    public class ReadOnlyValueFieldObject : IEquatable<ReadOnlyValueFieldObject>
    {
        private readonly int _constField;

        public ReadOnlyValueFieldObject() { }

        public ReadOnlyValueFieldObject(int initialValue)
        {
            _constField = initialValue;
        }

        public override bool Equals(object obj)
        {
            var basicObject = (ReadOnlyValueFieldObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(ReadOnlyValueFieldObject other)
        {
            var e1 = _constField == other?._constField;

            return e1;
        }
    }
}
