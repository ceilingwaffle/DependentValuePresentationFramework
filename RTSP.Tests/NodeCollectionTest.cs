namespace RTSP.Tests
{
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

            // adding child should not increase collection count
            var solarSystem = new SolarSystem();
            milkyWay.AddChildren(solarSystem);
            Assert.AreEqual(_nodeCollection.Count(), 2);
        }

        [Test]
        public void TestNodeOfSameTypeNotAddedTwice()
        {
            var milkyWay = new MilkyWay();
            Assert.AreEqual(_nodeCollection.Count(), 0);
            _nodeCollection.Add(milkyWay);
            Assert.AreEqual(_nodeCollection.Count(), 1);
            _nodeCollection.Add(milkyWay);
            Assert.AreEqual(_nodeCollection.Count(), 1);
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
