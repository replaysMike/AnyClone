using AnyClone.Extensions;
using AnyClone.Tests.TestObjects;
using NUnit.Framework;

namespace AnyClone.Tests
{
    [TestFixture]
    public class EnumTypeTests
    {
        [Test]
        public void Should_Clone_TestEnum()
        {
            const TestEnum original = TestEnum.Test2;
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }
    }
}
