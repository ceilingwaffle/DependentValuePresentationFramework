namespace RTSP.Tests
{
    using System;
    using NUnit.Framework;
    using RTSP.Core;
    using RTSP.Tests.Nodes;

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
            Node L0_N0_Leaf = new ExampleNode();

            //Assert
        }


    }

}
