namespace RTSP.Tests
{
    using System;
    using NUnit.Framework;
    using RTSP.Core;
    using RTSP.Tests.Nodes;

    [TestFixture]
    public class NodeTest
    {
        /// <summary>
        /// Initializes Node test objects
        /// </summary>
        /// 
        [SetUp]
        protected void SetUp()
        {
            Helpers.InvokePrivateStaticMethod<Node>("ResetInitializedNodes");
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
            // setup
            var milkyWay = new MilkyWay();
            var solarSystem = new SolarSystem();
            milkyWay.AddChildren(solarSystem);
            var milkyWayChildren = milkyWay.Children;
            var milkyWayHasChild = milkyWayChildren.TryGetValue(typeof(SolarSystem), out var milkyWayChild);

            // assertions
            Assert.IsTrue(milkyWayHasChild);
            Assert.IsNotNull(milkyWayChildren);
            Assert.AreSame(milkyWayChild, solarSystem);
        }

        /// <summary>
        /// Tests child has parent
        /// </summary>
        [Test]
        public void TestParentOfChild()
        {
            // setup
            var milkyWay = new MilkyWay();
            var solarSystem = new SolarSystem();
            milkyWay.AddChildren(solarSystem);
            var solarSystemParents = solarSystem.Parents;
            var solarSystemHasParent = solarSystemParents.TryGetValue(typeof(MilkyWay), out var solarSystemParent);
            
            // assertions
            Assert.IsNotNull(solarSystemParents);
            Assert.IsTrue(solarSystemHasParent);
            Assert.AreSame(solarSystemParent, milkyWay);
        }

        [Test]
        public void TestHasChildrenMethod()
        {
            var milkyWay = new MilkyWay();

            Assert.IsFalse(milkyWay.HasChildren());

            var childNode = new SolarSystem();
            milkyWay.AddChildren(childNode);

            Assert.IsTrue(milkyWay.HasChildren());
        }

        [Test]
        public void TestExceptionThrownWhenInitializingTwoOfSameNodeType()
        {
            var milkyWay1 = new MilkyWay();

            try
            {
                var milkyWay2 = new MilkyWay();
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Expected ArgumentException (initializing the same node types should not be allowed.");
        }
    }


}
