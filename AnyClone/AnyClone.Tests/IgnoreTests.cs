using AnyClone.Tests.TestObjects;
using NUnit.Framework;
using TypeSupport.Extensions;

namespace AnyClone.Tests
{
    [TestFixture]
    public class IgnoreTests
    {
        [Test]
        public void ShouldNot_Ignore_ByAttribute()
        {
            var test = new BasicObjectWithIgnore { BoolValue = true, ByteValue = 1, IntValue = 100, LongValue = 1000, StringValue = "A test string" };
            var clonedObject = test.Clone<BasicObjectWithIgnore>(CloneOptions.DisableIgnoreAttributes);
            Assert.AreEqual(test, clonedObject);
            Assert.IsNotNull(clonedObject.StringValue);
        }

        [Test]
        public void Should_Ignore_ByAttribute()
        {
            var test = new BasicObjectWithIgnore { BoolValue = true, ByteValue = 1, IntValue = 100, LongValue = 1000, StringValue = "A test string" };
            var clonedObject = test.Clone<BasicObjectWithIgnore>();
            Assert.AreEqual(test, clonedObject);
            Assert.IsNull(clonedObject.StringValue);
        }

        [Test]
        public void Should_Ignore_ByJsonAttribute()
        {
            var test = new BasicObjectWithJsonIgnore { BoolValue = true, ByteValue = 1, IntValue = 100, LongValue = 1000, StringValue = "A test string" };
            var clonedObject = test.Clone<BasicObjectWithJsonIgnore>();
            Assert.AreEqual(test, clonedObject);
            Assert.IsNull(clonedObject.StringValue);
        }

        [Test]
        public void Should_Ignore_ByPropertyName()
        {
            var test = new BasicObject { BoolValue = true, ByteValue = 1, IntValue = 100, LongValue = 1000, StringValue = "A test string" };
            var clonedObject = test.Clone<BasicObject>("StringValue");
            Assert.AreEqual(test.BoolValue, clonedObject.BoolValue);
            Assert.AreEqual(test.ByteValue, clonedObject.ByteValue);
            Assert.AreEqual(test.IntValue, clonedObject.IntValue);
            Assert.AreEqual(test.LongValue, clonedObject.LongValue);
            Assert.IsNull(clonedObject.StringValue);
        }

        [Test]
        public void Should_Ignore_ByPropertyPath()
        {
            var test = new BasicObject { BoolValue = true, ByteValue = 1, IntValue = 100, LongValue = 1000, StringValue = "A test string" };
            var clonedObject = test.Clone<BasicObject>(".StringValue");
            Assert.AreEqual(test.BoolValue, clonedObject.BoolValue);
            Assert.AreEqual(test.ByteValue, clonedObject.ByteValue);
            Assert.AreEqual(test.IntValue, clonedObject.IntValue);
            Assert.AreEqual(test.LongValue, clonedObject.LongValue);
            Assert.IsNull(clonedObject.StringValue);
        }

        [Test]
        public void Should_Ignore_ByPropertyExpression()
        {
            var test = new BasicObject { BoolValue = true, ByteValue = 1, IntValue = 100, LongValue = 1000, StringValue = "A test string" };
            var clonedObject = test.Clone<BasicObject>(x => x.StringValue);
            Assert.AreEqual(test.BoolValue, clonedObject.BoolValue);
            Assert.AreEqual(test.ByteValue, clonedObject.ByteValue);
            Assert.AreEqual(test.IntValue, clonedObject.IntValue);
            Assert.AreEqual(test.LongValue, clonedObject.LongValue);
            Assert.IsNull(clonedObject.StringValue);
        }

        [Test]
        public void Should_Ignore_ByAutoFieldName()
        {
            var test = new BasicObject { BoolValue = true, ByteValue = 1, IntValue = 100, LongValue = 1000, StringValue = "A test string" };
            var clonedObject = test.Clone<BasicObject>("<StringValue>k__BackingField");
            Assert.AreEqual(test.BoolValue, clonedObject.BoolValue);
            Assert.AreEqual(test.ByteValue, clonedObject.ByteValue);
            Assert.AreEqual(test.IntValue, clonedObject.IntValue);
            Assert.AreEqual(test.LongValue, clonedObject.LongValue);
            Assert.IsNull(clonedObject.StringValue);
        }

        [Test]
        public void Should_Ignore_ByAutoFieldPath()
        {
            var test = new BasicObject { BoolValue = true, ByteValue = 1, IntValue = 100, LongValue = 1000, StringValue = "A test string" };
            var clonedObject = test.Clone<BasicObject>(".<StringValue>k__BackingField");
            Assert.AreEqual(test.BoolValue, clonedObject.BoolValue);
            Assert.AreEqual(test.ByteValue, clonedObject.ByteValue);
            Assert.AreEqual(test.IntValue, clonedObject.IntValue);
            Assert.AreEqual(test.LongValue, clonedObject.LongValue);
            Assert.IsNull(clonedObject.StringValue);
        }

        [Test]
        public void Should_Ignore_ByFieldName()
        {
            var test = new BasicObject(10) { BoolValue = true, ByteValue = 1, IntValue = 100, LongValue = 1000, StringValue = "A test string" };
            var clonedObject = test.Clone<BasicObject>("_privateIntValue");
            Assert.AreNotEqual(test.GetFieldValue<int>("_privateIntValue"), clonedObject.GetFieldValue<int>("_privateIntValue"));
            Assert.AreEqual(test.BoolValue, clonedObject.BoolValue);
            Assert.AreEqual(test.ByteValue, clonedObject.ByteValue);
            Assert.AreEqual(test.IntValue, clonedObject.IntValue);
            Assert.AreEqual(test.LongValue, clonedObject.LongValue);
            Assert.AreEqual(test.StringValue, clonedObject.StringValue);
        }

        [Test]
        public void Should_Ignore_ByFieldPath()
        {
            var test = new BasicObject(10) { BoolValue = true, ByteValue = 1, IntValue = 100, LongValue = 1000, StringValue = "A test string" };
            var clonedObject = test.Clone<BasicObject>("._privateIntValue");
            Assert.AreNotEqual(test.GetFieldValue<int>("_privateIntValue"), clonedObject.GetFieldValue<int>("_privateIntValue"));
            Assert.AreEqual(test.BoolValue, clonedObject.BoolValue);
            Assert.AreEqual(test.ByteValue, clonedObject.ByteValue);
            Assert.AreEqual(test.IntValue, clonedObject.IntValue);
            Assert.AreEqual(test.LongValue, clonedObject.LongValue);
            Assert.AreEqual(test.StringValue, clonedObject.StringValue);
        }
    }
}
