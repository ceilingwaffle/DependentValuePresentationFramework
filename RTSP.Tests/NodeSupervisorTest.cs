using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NUnit.Framework;
using RTSP.Core;
using RTSP.Tests.Nodes;
using Assert = NUnit.Framework.Assert;
using CollectionAssert = NUnit.Framework.CollectionAssert;

namespace RTSP.Tests
{
    [TestFixture]
    public class NodeSupervisorTest
    {
        private NodeSupervisor _nodeSupervisor;

        [SetUp]
        protected void SetUp()
        {
            _nodeSupervisor = new NodeSupervisor();
            Helpers.InvokePrivateStaticMethod<Node>("ResetInitializedNodes");
            Helpers.InvokePrivateStaticMethod<Node>("ResetNodeStatePropertyNames");
        }

        [TearDown]
        protected void CleanUp()
        {

        }

        [Test]
        public void TestEnabledNodes()
        {
            var shouldBeEnabled = new ValidEnabledStatePropertyNode();
            var shouldNotBeEnabled = new DisabledStatePropertyNode();

            NodeCollection initializedNodes = Helpers.GetPrivateStaticProperty<NodeCollection, Node>("InitializedNodes");
            NodeCollection enabledNodes = Helpers.InvokePrivateMethod<NodeCollection>(_nodeSupervisor, "_CollectEnabledNodes", initializedNodes);

            CollectionAssert.Contains(enabledNodes, shouldBeEnabled);
            CollectionAssert.DoesNotContain(enabledNodes, shouldNotBeEnabled);
        }

        //[Test]
        //public void TestRootNodesAreRoots()
        //{
        //    // setup
        //    var LMC = new LMC();
        //    var MilkyWay = new MilkyWay();
        //    var Andromeda = new Andromeda();
        //    var SolarSystem = new SolarSystem();
        //    var Sun = new Sun();
        //    var Earth = new Earth();
        //    var Jupiter = new Jupiter();
        //    var Oumuamua = new Oumuamua();
        //    var HalleysComet = new HalleysComet();

        //    MilkyWay.Precedes(SolarSystem);
        //    Andromeda.Precedes(Oumuamua);
        //    SolarSystem.Precedes(Sun, Earth, Jupiter, Oumuamua);
        //    Sun.Precedes(HalleysComet);
        //    Earth.Precedes(HalleysComet);

        //    NodeCollection initializedNodes = Helpers.GetPrivateStaticProperty<NodeCollection, Node>("InitializedNodes");
        //    NodeCollection rootNodes = Helpers.InvokePrivateMethod<NodeCollection>(_nodeSupervisor, "_CollectRootNodes", initializedNodes);

        //    // expected
        //    var expectedRoots = new NodeCollection(LMC, MilkyWay, Andromeda);
        //    var emptyCollection = new NodeCollection();

        //    // assertions
        //    var returnedRoots = rootNodes;

        //    CollectionAssert.AreNotEquivalent(returnedRoots.ToList(), emptyCollection.ToList());
        //    Assert.GreaterOrEqual(returnedRoots.Count(), 1);
        //    Assert.AreEqual(returnedRoots.Count(), expectedRoots.Count());
        //    // we don't care about the order the nodes are listed in, only that the nodes exist in both lists.
        //    CollectionAssert.AreEquivalent(returnedRoots.ToList(), expectedRoots.ToList());
        //}

        //[Test]
        //public void TestLeafNodesAreLeaves()
        //{
        //    // setup
        //    var LMC = new LMC();
        //    var MilkyWay = new MilkyWay();
        //    var Andromeda = new Andromeda();
        //    var SolarSystem = new SolarSystem();
        //    var Sun = new Sun();
        //    var Earth = new Earth();
        //    var Jupiter = new Jupiter();
        //    var Oumuamua = new Oumuamua();
        //    var HalleysComet = new HalleysComet();

        //    MilkyWay.Precedes(SolarSystem);
        //    Andromeda.Precedes(Oumuamua);
        //    SolarSystem.Precedes(Sun, Earth, Jupiter, Oumuamua);
        //    Sun.Precedes(HalleysComet);
        //    Earth.Precedes(HalleysComet);

        //    //_nodeSupervisor.AddRootNodes(LMC, MilkyWay, Andromeda);
        //    NodeCollection initializedNodes = Helpers.GetPrivateStaticProperty<NodeCollection, Node>("InitializedNodes");
        //    NodeCollection rootNodes = Helpers.InvokePrivateMethod<NodeCollection>(_nodeSupervisor, "_CollectRootNodes", initializedNodes);
        //    NodeCollection returnedLeaves = Helpers.InvokePrivateMethod<NodeCollection>(_nodeSupervisor, "_CollectLeafNodes", rootNodes);

        //    // expected
        //    var expectedLeaves = new NodeCollection(LMC, Jupiter, Oumuamua, HalleysComet);
        //    var emptyCollection = new NodeCollection();

        //    // assertions
        //    CollectionAssert.AreNotEquivalent(returnedLeaves.ToList(), emptyCollection.ToList());
        //    Assert.GreaterOrEqual(returnedLeaves.Count(), 1);
        //    Assert.AreEqual(returnedLeaves.Count(), expectedLeaves.Count());
        //    // we don't care about the order the nodes are listed in, only that the nodes exist in both lists.
        //    CollectionAssert.AreEquivalent(returnedLeaves.ToList(), expectedLeaves.ToList());
        //}

        //[Test]
        //public void TestCollectLeafNodesMethodDoesNotThrowStackOverflowException()
        //{
        //    int iterations = 10000;

        //    // assert minimum 3 iterations because we need preceder, follower, "follower of follower" minimum
        //    Assert.GreaterOrEqual(iterations, 3);

        //    try
        //    {
        //        var dynamicNodes = new List<Node>();

        //        for (int i = 0; i < iterations; i++)
        //        {
        //            Node n = CreateDynamicNode();
        //            dynamicNodes.Add(n);

        //            if (i > 0)
        //            {
        //                var preceder = dynamicNodes[i - 1];

        //                preceder?.Precedes(n);
        //            }
        //        }

        //        // assert expected number of dynamic nodes created
        //        Assert.AreEqual(dynamicNodes.Count, iterations);

        //        // assert preceder, follower, "follower of follower" are not null
        //        Node p = dynamicNodes[0];
        //        Node c = dynamicNodes[1];
        //        Node gc = dynamicNodes[2];
        //        Assert.IsNotNull(p);
        //        Assert.IsNotNull(c);
        //        Assert.IsNotNull(gc);

        //        // assert that the first node is the second node's preceder
        //        Node expectedPreceder = null;
        //        c?.Preceders?.TryGetValue(p.GetType(), out expectedPreceder);
        //        Assert.IsNotNull(expectedPreceder);
        //        Assert.AreEqual(p, expectedPreceder);

        //        // assert that the preceder of the follower is not the preceder of "follower of follower" (grandchild)
        //        Node expectedParentOfGC = null;
        //        gc?.Preceders?.TryGetValue(c.GetType(), out expectedParentOfGC);
        //        Assert.IsNotNull(expectedParentOfGC);
        //        Assert.AreNotEqual(p, expectedParentOfGC);

        //        // stack overflow test on thousands of descendents
        //        var rootNode = dynamicNodes[0];
        //        //_nodeSupervisor.AddRootNodes(rootNode);
        //        NodeCollection initializedNodes = Helpers.GetPrivateStaticProperty<NodeCollection, Node>("InitializedNodes");
        //        NodeCollection rootNodes = Helpers.InvokePrivateMethod<NodeCollection>(_nodeSupervisor, "_CollectRootNodes", initializedNodes);
        //        NodeCollection leafNodes = Helpers.InvokePrivateMethod<NodeCollection>(_nodeSupervisor, "_CollectLeafNodes", rootNodes);

        //        NodeCollection returnedLeaves = leafNodes;
        //    }
        //    catch (StackOverflowException e)
        //    {
        //        Assert.Fail($"Expected no StackOverflowException, but got: {e.Message}.");
        //    }
        //    catch (Exception e)
        //    {
        //        Assert.Fail($"Expected no Exception, but got: {e.Message}.");
        //    }
        //}

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

            // define the "DetermineValue" abstract method implementation of class Node
            DefineMethodOnTypeBuilder(
                typeBuilder: typeBuilder,
                methodName: "DetermineValueAsync",
                methodReturnType: typeof(Task<object>),
                methodReturnValue: Task.Run(() => { return new object(); })
            );

            Type proxy = typeBuilder.CreateType();

            Node n = (Node)Activator.CreateInstance(proxy);

            return n;
        }

        /// <summary>
        /// Defines the abstract method implementation of some class on the given TypeBuilder
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="methodName"></param>
        /// <param name="methodReturnType"></param>
        private static void DefineMethodOnTypeBuilder(TypeBuilder typeBuilder, string methodName, Type methodReturnType, object methodReturnValue)
        {
            // get the pointer address of the methodReturnValue object
            // TODO: Bug where when we call DynamicNode.DetermineValueAsync() it throws "Bad class token".
            GCHandle objHandle = GCHandle.Alloc(methodReturnValue, GCHandleType.WeakTrackResurrection);
            int returnValueAddr = GCHandle.ToIntPtr(objHandle).ToInt32();

            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
                MethodAttributes.Public | MethodAttributes.Virtual, methodReturnType, Type.EmptyTypes);

            ILGenerator generator = methodBuilder.GetILGenerator();

            generator.Emit(OpCodes.Ldobj, returnValueAddr);
            generator.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(methodBuilder, typeBuilder.BaseType.GetMethod(methodName));
        }

    }

}
