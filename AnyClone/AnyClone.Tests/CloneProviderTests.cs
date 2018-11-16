using AnyClone.Tests.TestObjects;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace AnyClone.Tests
{
    [TestFixture]
    public class CloneProviderTests
    {
        [Test]
        public void Should_Clone_BasicObject()
        {
            var original = new BasicObject
            {
                BoolValue = true,
                ByteValue = 0x10,
                IntValue = 100,
                LongValue = 10000,
                StringValue = "Test String"
            };
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void ModifiedClone_Basic_ShouldNotBeEqual()
        {
            var original = new BasicObject
            {
                BoolValue = true,
                ByteValue = 0x10,
                IntValue = 100,
                LongValue = 10000,
                StringValue = "Test String"
            };
            var cloned = original.Clone();
            cloned.StringValue = "A different string";

            Assert.AreNotEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_ArrayObject()
        {
            var original = new ArrayObject
            {
                ByteArray = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                IntArray = new int[] { 1, 2, 3, 4 },
                DoubleArray = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 },
            };
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void ModifiedClone_Array_ShouldNotBeEqual()
        {
            var original = new ArrayObject
            {
                ByteArray = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                IntArray = new int[] { 1, 2, 3, 4 },
                DoubleArray = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 },
            };
            var cloned = original.Clone();
            cloned.ByteArray[2] = 0x10;

            Assert.AreNotEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_CollectionObject()
        {
            var original = new CollectionObject
            {
                IntCollection = new List<int> { 1, 2, 3, 4 },
                ObjectCollection = new List<BasicObject>
                {
                    new BasicObject
                    {
                        BoolValue = true,
                        ByteValue = 0x10,
                    },
                    new BasicObject
                    {
                        BoolValue = false,
                        ByteValue = 0x20,
                    },
                }
            };
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void ModifiedClone_Collection_ShouldNotBeEqual()
        {
            var original = new CollectionObject
            {
                IntCollection = new List<int> { 1, 2, 3, 4 },
                ObjectCollection = new List<BasicObject>
                {
                    new BasicObject
                    {
                        BoolValue = true,
                        ByteValue = 0x10,
                    },
                    new BasicObject
                    {
                        BoolValue = false,
                        ByteValue = 0x20,
                    },
                }
            };
            var cloned = original.Clone();
            cloned.ObjectCollection.Skip(1).First().BoolValue = true;

            Assert.AreNotEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_DictionaryObject()
        {
            var original = new DictionaryObject
            {
                Collection = {
                    { 1, new BasicObject() { IntValue = 1, LongValue = 10 } },
                    { 2, new BasicObject() { IntValue = 2, LongValue = 20} },
                    { 3, new BasicObject() { IntValue = 3, LongValue = 30} },
                }
            };
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void ModifiedClone_Dictionary_ShouldNotBeEqual()
        {
            var original = new DictionaryObject
            {
                Collection = {
                    { 1, new BasicObject() { IntValue = 1, LongValue = 10 } },
                    { 2, new BasicObject() { IntValue = 2, LongValue = 20} },
                    { 3, new BasicObject() { IntValue = 3, LongValue = 30} },
                }
            };
            var cloned = original.Clone();
            cloned.Collection[2].LongValue = 200;

            Assert.AreNotEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_InterfacesObject()
        {
            var original = new InterfaceObject()
            {
                BoolValue = true,
                IntValue = 10,
                DictionaryValue = new Dictionary<int, BasicObject>
                {
                    { 1, new BasicObject() { IntValue = 1, LongValue = 10 } },
                    { 2, new BasicObject() { IntValue = 2, LongValue = 20 } },
                    { 3, new BasicObject() { IntValue = 3, LongValue = 30 } },
                },
            };
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void ModifiedClone_InterfaceObject_ShouldNotBeEqual()
        {
            var original = new InterfaceObject()
            {
                BoolValue = true,
                IntValue = 10,
                DictionaryValue = new Dictionary<int, BasicObject>
                {
                    { 1, new BasicObject() { IntValue = 1, LongValue = 10 } },
                    { 2, new BasicObject() { IntValue = 2, LongValue = 20 } },
                    { 3, new BasicObject() { IntValue = 3, LongValue = 30 } },
                },
            };
            var cloned = original.Clone();
            cloned.DictionaryValue[2].StringValue = "Test string";

            Assert.AreNotEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_ComplexObject()
        {
            var original = new ComplexObject(100);
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void ModifiedClone_ComplexObject_ShouldNotBeEqual()
        {
            var original = new ComplexObject(100);
            var cloned = original.Clone();
            cloned.listOfStrings.Add("new string");

            Assert.AreNotEqual(original, cloned);
        }
    }
}
