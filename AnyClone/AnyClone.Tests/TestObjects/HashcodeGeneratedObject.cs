using System;
using System.Collections.Generic;
using System.Text;

namespace AnyClone.Tests.TestObjects
{
    public class HashcodeGeneratedObject
    {
        public int Value { get; set; }
        public HashcodeGeneratedObject(int value)
        {
            Value = value;
        }

        public override int GetHashCode() => Value;
    }
}
