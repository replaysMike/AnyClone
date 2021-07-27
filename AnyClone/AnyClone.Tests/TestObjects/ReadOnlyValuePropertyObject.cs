using System;

namespace AnyClone.Tests.TestObjects
{
    public class ReadOnlyValuePropertyObject : IEquatable<ReadOnlyValuePropertyObject>
    {
        public int Property1 { get; }
        public int Property2 { get; private set; }

        public ReadOnlyValuePropertyObject() { }

        public ReadOnlyValuePropertyObject(int initialValue)
        {
            Property1 = initialValue;
            Property2 = initialValue;
        }

        public override bool Equals(object obj)
        {
            var basicObject = (ReadOnlyValueFieldObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(ReadOnlyValuePropertyObject other)
        {
            var e1 = Property1 == other?.Property1;
            var e2 = Property2 == other?.Property2;

            return e1 && e2;
        }
    }
}
