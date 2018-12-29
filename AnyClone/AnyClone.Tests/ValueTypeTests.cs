using NUnit.Framework;

namespace AnyClone.Tests
{
    [TestFixture]
    public class ValueTypeTests
    {
        [Test]
        public void Should_Clone_Bool()
        {
            var original = true;
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_Short()
        {
            var original = (short)3;
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_Integer()
        {
            var original = 3;
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_Double()
        {
            var original = 3.1415;
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_Decimal()
        {
            var original = 3.1415M;
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }
    }
}
