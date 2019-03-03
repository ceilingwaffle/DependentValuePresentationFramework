using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using CollectionAssert = Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;

namespace RTSP.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
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
            var rootNodesList = RootNodes.ToEnumerable();
            var returnedLeaves = (NodeCollection)obj.Invoke("CollectLeafNodes", rootNodesList);

            CollectionAssert.AreNotEquivalent(returnedLeaves.ToList(), emptyCollection.ToList());
            Assert.AreEqual(returnedLeaves.Count(), expectedLeaves.Count());
            // we don't care about the order the nodes are listed in, only that the nodes exist in both lists.
            CollectionAssert.AreEquivalent(returnedLeaves.ToList(), expectedLeaves.ToList());
        }


        [Test]
        public void TestCollectLeafNodesMethodDoesNotThrowStackOverflowException()
        {
            int iterations = 20000;

            // assert minimum 3 iterations because we need parent, child, grandchild minimum
            Assert.GreaterOrEqual(iterations, 3);

            try
            {
                var dynamicNodes = new List<Node>();

                for (int i = 0; i < iterations; i++)
                {
                    Node n = CreateDynamicNode();
                    dynamicNodes.Add(n);

                    if (i > 0)
                    {
                        var parent = dynamicNodes[i - 1];

                        parent?.AddChildren(n);
                    }
                }

                // assert expected number of dynamic nodes created
                Assert.AreEqual(dynamicNodes.Count, iterations);

                // assert parent, child, grandchild are not null
                Node p = dynamicNodes[0];
                Node c = dynamicNodes[1];
                Node gc = dynamicNodes[2];
                Assert.IsNotNull(p);
                Assert.IsNotNull(c);
                Assert.IsNotNull(gc);

                // assert that the first node is the second node's parent
                Node expectedParent = null;
                c?.Parents?.TryGetValue(p.GetType(), out expectedParent);
                Assert.IsNotNull(expectedParent);
                Assert.AreEqual(p, expectedParent);

                // assert that the grandparent of grandchild is not the parent of grandchild
                Node expectedParentOfGC = null;
                gc?.Parents?.TryGetValue(c.GetType(), out expectedParentOfGC);
                Assert.IsNotNull(expectedParentOfGC);
                Assert.AreNotEqual(p, expectedParentOfGC);

                // stack overflow test on thousands of descendents
                var rootNode = dynamicNodes[0];
                _nodeSupervisor.AddRootNodes(rootNode);

                NodeSupervisor target = _nodeSupervisor;
                PrivateObject obj = new PrivateObject(target);
                NodeCollection RootNodes = new NodeCollection(rootNode);
                IEnumerable<Node> rootNodesList = RootNodes.ToEnumerable();

                var returnedLeaves = (NodeCollection)obj.Invoke("CollectLeafNodes", rootNodesList);
            }
            catch (StackOverflowException e)
            {
                Assert.Fail($"Expected no StackOverflowException, but got: {e.Message}.");
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected no Exception, but got: {e.Message}.");
            }
        }

        /// <summary>
        /// Returns an instance of a "dynamic" Node object (derived from the abstract Node class) with a unique class name.
        /// </summary>
        /// <returns></returns>
        private static Node CreateDynamicNode()
        {
            Type baseType = typeof(Node);

            AssemblyName asmName = new AssemblyName(
                string.Format("{0}_{1}", "tmpAsm", Guid.NewGuid().ToString("N"))
            );

            // create in memory assembly only
            AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);

            ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule("core");

            string proxyTypeName = string.Format("{0}", Guid.NewGuid().ToString("N"));

            TypeBuilder typeBuilder = moduleBuilder.DefineType(proxyTypeName);

            typeBuilder.SetParent(baseType);

            Type proxy = typeBuilder.CreateType();

            Node n = (Node)Activator.CreateInstance(proxy);

            return n;
        }
    }

}
