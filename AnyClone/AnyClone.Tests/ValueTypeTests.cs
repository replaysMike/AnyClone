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

        [Test]
        public void Should_Clone_ArrayOfInts()
        {
            var original = new int[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_2dMultidimensionalArrayOfInts()
        {
            var original = new int[4, 2] {
                { 1, 2 },
                { 3, 4 },
                { 5, 6 },
                { 7, 8 }
            };

            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_3dMultidimensionalArrayOfInts()
        {
            var original = new int[2, 3, 3] {
                // row 1
                { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } },
                // row 2
                { { 10, 11, 12 }, { 13, 14, 15 }, { 16, 17, 18 } }
            };

            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_4dMultidimensionalArrayOfInts()
        {
            var original = new int[2, 2, 2, 2] {
                // row 1
                { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } },
                // row 2
                { { { 9, 10 }, { 11, 12 } }, { { 13, 14 }, { 15, 16 } } },
            };

            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }

        [Test]
        public void Should_Clone_JaggedArrayOfInts()
        {
            var original = new int[][] {
                new int[] { 1, 2 },
                new int[] { 3, 4 },
                new int[] { 5, 6 },
                new int[] { 7, 8 }
            };

            var cloned = original.Clone();

            Assert.AreEqual(original, cloned);
        }
    }
}
