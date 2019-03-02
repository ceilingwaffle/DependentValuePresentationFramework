namespace RTSP.Tests
{
    using System;
    using NUnit.Framework;
    using RTSP.Core;
    using RTSP.Tests.Nodes;

    [TestFixture]
    public class NodeTest
    {
        private Node parentNode;
        private Node childNode;

        /// <summary>
        /// Initializes Node test objects
        /// </summary>
        /// 
        [SetUp]
        protected void SetUp()
        {
            parentNode = new MilkyWay();
            childNode = new SolarSystem();

            parentNode.AddChildren(childNode);
        }

        [TearDown]
        protected void CleanUp()
        {

        }

        /// <summary>
        /// Tests parent has child
        /// </summary>
        [Test]
        public void TestChildOfParent()
        {
            var childrenOfParents = parentNode.Children;
            Assert.IsNotNull(childrenOfParents);
            var parentHasChild = childrenOfParents.TryGetValue(typeof(SolarSystem), out var childOfParent);
            Assert.IsTrue(parentHasChild);
            Assert.AreSame(childOfParent, childNode);
        }

        /// <summary>
        /// Tests child has parent
        /// </summary>
        [Test]
        public void TestParentOfChild()
        {
            var parentsOfChild = childNode.Parents;
            Assert.IsNotNull(parentsOfChild);
            var childHasParent = parentsOfChild.TryGetValue(typeof(MilkyWay), out var parentOfChild);
            Assert.IsTrue(childHasParent);
            Assert.AreSame(parentOfChild, parentNode);
        }

        [Test]
        public void TestHasChildrenMethod()
        {
            var parentNode = new MilkyWay();

            Assert.IsFalse(parentNode.HasChildren());

            var childNode = new SolarSystem();
            parentNode.AddChildren(childNode);

            Assert.IsTrue(parentNode.HasChildren());
        }

    }


}
