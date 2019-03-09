namespace RTSP.Tests
{
    using System;
    using System.Threading.Tasks;
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
            Helpers.InvokePrivateStaticMethod<Node>("ResetNodeStatePropertyNames");
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

        [Test]
        public void TestStatePropertyName_ValidOverridden()
        {
            try
            {
                var node = new ValidEnabledStatePropertyNode();
            }
            catch (ArgumentException e)
            {
                Assert.Fail($"Expected no ArgumentException but got: {e.Message}.");
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected no Exception but got: {e.Message}.");
            }

            Assert.Pass();
        }

        [Test]
        public void TestStatePropertyName_DuplicateOverridden()
        {
            var node = new ValidEnabledStatePropertyNode();

            try
            {
                var node2 = new DuplicateNameStatePropertyNode();
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected ArgumentException but got Exception: {e.Message}.");
            }

            Assert.Fail($"Expected ArgumentException but got no Exception.");
        }

        [Test]
        public void TestStatePropertyName_EmptyOverridden()
        {
            try
            {
                var node = new EmptyNameStatePropertyNode();
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected ArgumentException but got Exception: {e.Message}.");
            }

            Assert.Fail($"Expected ArgumentException but got no Exception.");
        }

        [Test]
        public void TestStatePropertyName_WhiteSpaceOverridden()
        {
            try
            {
                var node = new WhiteSpaceNameStatePropertyNode();
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected ArgumentException but got Exception: {e.Message}.");
            }

            Assert.Fail($"Expected ArgumentException but got no Exception.");
        }

        [Test]
        public void TestStatePropertyName_NullOverridden()
        {
            try
            {
                var node = new NullNameStatePropertyNode();
            }
            catch (ArgumentException e)
            {
                Assert.Fail($"Expected no ArgumentException but got: {e.Message}.");
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected no Exception but got: {e.Message}.");
            }

            Assert.Pass();
        }

        [Test]
        public void TestStatePropertyName_NotOverridden()
        {
            try
            {
                var node = new DisabledStatePropertyNode();
            }
            catch (ArgumentException e)
            {
                Assert.Fail($"Expected no ArgumentException but got: {e.Message}.");
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected no Exception but got: {e.Message}.");
            }

            Assert.Pass();
        }
    }

    [StateProperty(enabled: true, name: "PropertyName")]
    class ValidEnabledStatePropertyNode : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }

    [StateProperty(enabled: true, name: "PropertyName")]
    class DuplicateNameStatePropertyNode : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }

    [StateProperty(enabled: true, name: "")]
    internal class EmptyNameStatePropertyNode : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }

    [StateProperty(enabled: true, name: "  ")]
    internal class WhiteSpaceNameStatePropertyNode : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }

    [StateProperty(enabled: true, name: null)]
    internal class NullNameStatePropertyNode : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }

    [StateProperty(enabled: false, name: "HopefullyNotEnabled")]
    internal class DisabledStatePropertyNode : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }

}
