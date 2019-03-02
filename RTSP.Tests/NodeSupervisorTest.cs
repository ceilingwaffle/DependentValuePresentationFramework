using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using CollectionAssert = Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;

namespace RTSP.Tests
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NUnit.Framework;
    using RTSP.Core;
    using RTSP.Tests.Nodes;
    using Assert = NUnit.Framework.Assert;
    using CollectionAssert = NUnit.Framework.CollectionAssert;

    [TestFixture]
    public class NodeSupervisorTest
    {
        private NodeSupervisor _nodeSupervisor;

        [SetUp]
        protected void SetUp()
        {
            _nodeSupervisor = new NodeSupervisor();
        }

        [TearDown]
        protected void CleanUp()
        {

        }

        [Test]
        public void TestLeafNodesAreLeaves()
        {
            // setup
            var LMC = new LMC();
            var MilkyWay = new MilkyWay();
            var Andromeda = new Andromeda();
            var SolarSystem = new SolarSystem();
            var Sun = new Sun();
            var Earth = new Earth();
            var Jupiter = new Jupiter();
            var Oumuamua = new Oumuamua();
            var HalleysComet = new HalleysComet();

            MilkyWay.AddChildren(SolarSystem);
            Andromeda.AddChildren(Oumuamua);
            SolarSystem.AddChildren(Sun, Earth, Jupiter, Oumuamua);
            Sun.AddChildren(HalleysComet);
            Earth.AddChildren(HalleysComet);

            _nodeSupervisor.AddRootNodes(LMC, MilkyWay, Andromeda);

            // expected
            var expectedLeaves = new NodeCollection(LMC, Jupiter, Oumuamua, HalleysComet);
            var emptyCollection = new NodeCollection();

            // assertions
            NodeSupervisor target = _nodeSupervisor;
            PrivateObject obj = new PrivateObject(target);
            var RootNodes = new NodeCollection(LMC, MilkyWay, Andromeda);
            var rootNodesList = RootNodes.ToList().Select((n) => { return n.Value; });
            var returnedLeaves = (NodeCollection)obj.Invoke("CollectLeafNodes", rootNodesList);

            CollectionAssert.AreNotEquivalent(returnedLeaves.ToList(), emptyCollection.ToList());
            Assert.AreEqual(returnedLeaves.Count(), expectedLeaves.Count());
            // we don't care about the order the nodes are listed in, only that the nodes exist in both lists.
            CollectionAssert.AreEquivalent(returnedLeaves.ToList(), expectedLeaves.ToList());
        }


    }

}
