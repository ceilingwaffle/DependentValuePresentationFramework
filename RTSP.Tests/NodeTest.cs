namespace RTSP.Tests
{
    using System;
    using NUnit.Framework;
    using RTSP.Core;

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
            parentNode = new TestParentNode();
            childNode = new TestChildNode();

            parentNode.AddChildren(childNode);
        }

        [TearDown]
        public void CleanUp()
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
            var parentHasChild = childrenOfParents.TryGetValue(typeof(TestChildNode), out var childOfParent);
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
            var childHasParent = parentsOfChild.TryGetValue(typeof(TestParentNode), out var parentOfChild);
            Assert.IsTrue(childHasParent);
            Assert.AreSame(parentOfChild, parentNode);
        }

    }

    class TestParentNode : Node
    {

    }

    class TestChildNode : Node
    {

    }
}
