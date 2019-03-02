namespace RTSP.Tests
{
    using System;
    using NUnit.Framework;
    using RTSP.Core;
    using RTSP.Tests.Nodes;

    [TestFixture]
    public class NodeCollectionTest
    {
        private NodeCollection _nodeCollection;


        /// <summary>
        /// Initializes Node test objects
        /// </summary>
        /// 
        [SetUp]
        protected void SetUp()
        {
            _nodeCollection = new NodeCollection();
        }

        [TearDown]
        protected void CleanUp()
        {

        }

        [Test]
        public void TestCount()
        {
            // test 0 nodes added
            Assert.AreEqual(_nodeCollection.Count(), 0);

            // test 1 nodes added
            var node1 = new ExampleNode();
            _nodeCollection.Add(node1);
            Assert.AreEqual(_nodeCollection.Count(), 1);

            // test 2 nodes added
            var node2 = new TestParentNode();
            _nodeCollection.Add(node2);
            Assert.AreEqual(_nodeCollection.Count(), 2);
        }

        [Test]
        public void TestNodeOfSameTypeNotAddedTwice()
        {
            var node1 = new ExampleNode();
            Assert.AreEqual(_nodeCollection.Count(), 0);
            _nodeCollection.Add(node1);
            Assert.AreEqual(_nodeCollection.Count(), 1);
            _nodeCollection.Add(node1);
            Assert.AreEqual(_nodeCollection.Count(), 1);
        }
    }

}
