using System;
using DVPF.Tests.Nodes;

namespace DVPF.Tests
{
    using NUnit.Framework;
    using DVPF.Core;
    using Tests.Nodes;

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
            Helpers.InvokePrivateStaticMethod<Node>("ResetInitializedNodes");
            Helpers.InvokePrivateStaticMethod<Node>("ResetNodeStatePropertyNames");
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
            var lmc = new LMC();
            _nodeCollection.Add(lmc);
            Assert.AreEqual(_nodeCollection.Count(), 1);

            // test 2 nodes added
            var milkyWay = new MilkyWay();
            _nodeCollection.Add(milkyWay);
            Assert.AreEqual(_nodeCollection.Count(), 2);

            // adding a follower should not increase collection count
            var solarSystem = new SolarSystem();
            milkyWay.Precedes(solarSystem);
            Assert.AreEqual(_nodeCollection.Count(), 2);
        }

        [Test]
        public void TestExceptionThrownWhenSameNodeAddedTwice()
        {
            var milkyWay = new MilkyWay();
            Assert.AreEqual(_nodeCollection.Count(), 0);

            _nodeCollection.Add(milkyWay);
            Assert.AreEqual(_nodeCollection.Count(), 1);

            try
            {
                _nodeCollection.Add(milkyWay);
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Expected ArgumentException (attempted to add same node twice).");
        }

        [Test]
        public void TestExceptionThrownWhenNodeOfSameTypeAddedTwice()
        {
            var milkyWay1 = new MilkyWay();
            Assert.AreEqual(_nodeCollection.Count(), 0);

            _nodeCollection.Add(milkyWay1);
            Assert.AreEqual(_nodeCollection.Count(), 1);

            try
            {
                var milkyWay2 = new MilkyWay();
                _nodeCollection.Add(milkyWay2);
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Expected ArgumentException (attempted to add node of same type twice).");
        }

        [Test]
        public void TestExists()
        {
            // setup
            var coll = new NodeCollection();
            var milkyWay = new MilkyWay();

            // assert node does not exist
            Assert.IsFalse(coll.Exists(milkyWay));

            // assert node exists
            coll.Add(milkyWay);
            Assert.IsTrue(coll.Exists(milkyWay));
        }
    }

}
