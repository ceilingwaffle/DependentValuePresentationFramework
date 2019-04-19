namespace DVPF.Tests
{
    using System;
    using DVPF.Core;
    using NUnit.Framework;
    using Tests.Nodes;

    /// <summary>
    /// Tests for <see cref="NodeCollection"/>
    /// </summary>
    [TestFixture]
    internal class NodeCollectionTest
    {
        /// <summary>
        /// The node collection.
        /// </summary>
        private NodeCollection nodeCollection;

        /// <summary>
        /// Initializes Node test objects
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.nodeCollection = new NodeCollection();
            NodeSupervisor.ResetInitializedNodes();
            Node.ResetNodeStatePropertyNames();
        }

        /// <summary>
        /// The clean up.
        /// </summary>
        [TearDown]
        public void CleanUp()
        {
        }

        /// <summary>
        /// Assert <see cref="NodeCollection.Count"/> correctly reflects the total number of <see cref="Node"/>s added.
        /// </summary>
        [Test]
        public void TestCount()
        {
            // test 0 nodes added
            Assert.AreEqual(this.nodeCollection.Count(), 0);

            // test 1 nodes added
            var lmc = new LMC();
            this.nodeCollection.Add(lmc);
            Assert.AreEqual(this.nodeCollection.Count(), 1);

            // test 2 nodes added
            var milkyWay = new MilkyWay();
            this.nodeCollection.Add(milkyWay);
            Assert.AreEqual(this.nodeCollection.Count(), 2);

            // adding a follower should not increase collection count
            var solarSystem = new SolarSystem();
            milkyWay.Precedes(solarSystem);
            Assert.AreEqual(this.nodeCollection.Count(), 2);
        }

        /// <summary>
        /// Assert same <see cref="Node"/> not added twice.
        /// </summary>
        [Test]
        public void TestExceptionThrownWhenSameNodeAddedTwice()
        {
            var milkyWay = new MilkyWay();
            Assert.AreEqual(this.nodeCollection.Count(), 0);

            this.nodeCollection.Add(milkyWay);
            Assert.AreEqual(this.nodeCollection.Count(), 1);

            try
            {
                this.nodeCollection.Add(milkyWay);
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Expected ArgumentException (attempted to add same node twice).");
        }

        /// <summary>
        /// Assert <see cref="Node"/> of same type not added twice.
        /// </summary>
        [Test]
        public void TestExceptionThrownWhenNodeOfSameTypeAddedTwice()
        {
            var milkyWay1 = new MilkyWay();
            Assert.AreEqual(this.nodeCollection.Count(), 0);

            this.nodeCollection.Add(milkyWay1);
            Assert.AreEqual(this.nodeCollection.Count(), 1);

            try
            {
                var milkyWay2 = new MilkyWay();
                this.nodeCollection.Add(milkyWay2);
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Expected ArgumentException (attempted to add node of same type twice).");
        }

        /// <summary>
        /// Assertions for method <see cref="NodeCollection.Exists"/>
        /// </summary>
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
