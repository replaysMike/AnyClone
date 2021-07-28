namespace AnyClone.Tests.TestObjects
{
    public class DifferentBasicObject : BasicObject
    {
        private readonly int _differentIntValue;
        public int DifferentIntValue => _differentIntValue;

        public DifferentBasicObject(int value) : base(value)
        {
            _differentIntValue = value;
        }
    }
}
