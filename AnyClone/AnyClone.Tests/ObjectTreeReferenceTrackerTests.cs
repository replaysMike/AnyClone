using System;
using System.Collections.Generic;
using System.Text;
using AnyClone.Tests.TestObjects;
using NUnit.Framework;

namespace AnyClone.Tests
{
    [TestFixture]
    public class ObjectTreeReferenceTrackerTests
    {
        [Test]
        public void Should_ReferenceAddresses_BeSame()
        {
            var testObject = new BasicObject();
            var testObject2 = testObject;
            var tracker = new ObjectTreeReferenceTracker(ReferenceTrackingType.Reference);
            tracker.Add(testObject);
            Assert.AreEqual(1, tracker.Count);
            tracker.Add(testObject2);
            Assert.AreEqual(1, tracker.Count);
        }

        [Test]
        public void Should_Hashcodes_BeSame()
        {
            var testObject = new HashcodeGeneratedObject(1);
            var testObject2 = new HashcodeGeneratedObject(1);
            var tracker = new ObjectTreeReferenceTracker(ReferenceTrackingType.Hashcode, true);
            tracker.Add(testObject);
            Assert.AreEqual(1, tracker.Count);
            tracker.Add(testObject2);
            Assert.AreEqual(1, tracker.Count);
        }
    }
}
