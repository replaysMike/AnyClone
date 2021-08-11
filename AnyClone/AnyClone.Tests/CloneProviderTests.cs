using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AnyClone;
using AnyClone.Tests.TestObjects;
using NUnit.Framework;

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
                IntArray = new [] { 1, 2, 3, 4 },
                DoubleArray = new [] { 1.0, 2.0, 3.0, 4.0, 5.0 },
            };
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_2dMultidimensionalArrayObject()
        {
            var original = new MultiDimensional2dArrayObject
            {
                Int2DArray = new [,] {
                    { 1, 2 },
                    { 3, 4 },
                    { 5, 6 },
                    { 7, 8 }
                }
            };
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_3dMultidimensionalArrayObject()
        {
            var original = new MultiDimensional3dArrayObject
            {
                Int3DArray = new [,,] {
                    // row 1
                    { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } },
                    // row 2
                    { { 10, 11, 12 }, { 13, 14, 15 }, { 16, 17, 18 } }
                }
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
                IntArray = new [] { 1, 2, 3, 4 },
                DoubleArray = new [] { 1.0, 2.0, 3.0, 4.0, 5.0 },
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
        public void Should_Clone_ObjectWithReadOnlyReferenceField()
        {
            var original = new ReadOnlyReferenceFieldObject(new BasicObject(24) {
                IntValue = 100,
                BoolValue = true,
                ByteValue = 240,
                LongValue = 1000,
                StringValue = "test"
            });
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_ObjectWithReadOnlyValueField()
        {
            var original = new ReadOnlyValueFieldObject(100);
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_ObjectWithReadOnlyValueProperty()
        {
            var original = new ReadOnlyValuePropertyObject(100);
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void ModifiedClone_ComplexObject_ShouldNotBeEqual()
        {
            var original = new ComplexObject(100);
            var cloned = original.Clone();
            cloned.ListOfStrings.Add("new string");

            Assert.AreNotEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_CustomCollectionObject()
        {
            var original = new CustomCollectionObject<BasicObject>(100, "test")
            {
                new BasicObject() { IntValue = 1, BoolValue = true, ByteValue = 10, LongValue = 100, StringValue = "Test 1" },
                new BasicObject() { IntValue = 2, BoolValue = false, ByteValue = 20, LongValue = 200, StringValue = "Test 2" },
                new BasicObject() { IntValue = 3, BoolValue = true, ByteValue = 30, LongValue = 300, StringValue = "Test 3" }
            };
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_ToDifferentType()
        {
            var original = new BasicObject
            {
                BoolValue = true,
                ByteValue = 0x10,
                IntValue = 100,
                LongValue = 10000,
                StringValue = "Test String"
            };
            var cloned = original.CloneTo<DifferentBasicObject>();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_ToDifferentTypeInstance()
        {
            var original = new BasicObject(101)
            {
                BoolValue = true,
                ByteValue = 0x10,
                IntValue = 100,
                LongValue = 10000,
                StringValue = "Test String"
            };
            var cloned = original.CloneTo(new DifferentBasicObject(99));
            Assert.AreEqual(original, cloned);
            Assert.AreEqual(99, cloned.DifferentIntValue);
        }

        [Test]
        public void Should_Clone_ToVeryDifferentType()
        {
            var original = new BasicObject
            {
                BoolValue = true,
                ByteValue = 0x10,
                IntValue = 100,
                LongValue = 10000,
                StringValue = "Test String"
            };
            var cloned = original.CloneTo<VeryDifferentBasicObject>();

            Assert.AreEqual(original.BoolValue, cloned.BoolValue);
            Assert.AreEqual(original.ByteValue, cloned.ByteValue);
            Assert.AreEqual(original.StringValue, cloned.StringValue);
            Assert.AreEqual(0, cloned.UniqueIntProperty);
        }

        [Test]
        public void Should_Clone_AttributesWithParams()
        {
            var original = new BasicObjectWithParamAttributes();
            var cloned = original.Clone();

            Assert.IsNotNull(cloned);
        }

        [Test]
        public void Should_Clone_ReadOnlyCollection()
        {
            var original = new List<int> { 1, 2, 3, 4, 5 }.AsReadOnly();
            var cloned = original.Clone();

            Assert.IsNotNull(cloned);
        }

#if NET45_OR_GREATER || NETSTANDARD1_0_OR_GREATER

        [Test]
        public void Should_Clone_ReadOnlyDictionary()
        {
            var values = new Dictionary<int, bool> { { 1, true }, { 2, false }, { 3, true }, { 4, false }, { 5, true } };
            var original = new ReadOnlyDictionary<int, bool>(values);
            var cloned = original.Clone();

            Assert.IsNotNull(cloned);
        }

#endif

#if NET45_OR_GREATER || NETSTANDARD1_0_OR_GREATER

        [Test]
        public void Should_Clone_BuyObject()
        {
            // this test represents cloning of a very complex class
            var original = new BuyObject();
            var cloned = original.Clone();
            Func<BuyObject, int> func = i => 100;
            Expression<Func<BuyObject, int>> newUnits = i => func(i);
            cloned.Units = newUnits;

            Assert.IsNotNull(cloned);
            Assert.AreEqual(newUnits, cloned.Units);
            Assert.AreNotEqual(newUnits, original.Units);
        }

#endif

        [Test]
        public void Should_Clone_Logger()
        {
            var original = NLog.LogLevel.Trace;
            var cloned = original.Clone(CloneConfiguration.SkipReadOnlyMembers());
            Assert.IsNotNull(cloned);
        }
    }
}
