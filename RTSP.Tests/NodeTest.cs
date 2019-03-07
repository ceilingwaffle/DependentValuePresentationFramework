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
                var node = new ValidOverriddenStatePropertyNameNode();
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
            var node = new ValidOverriddenStatePropertyNameNode();

            try
            {
                var node2 = new DuplicateOverriddenStatePropertyNameNode();
            }
            catch (ArgumentException e)
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
                var node = new EmptyOverriddenStatePropertyNameNode();
            }
            catch (ArgumentException e)
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
                var node = new NullOverriddenStatePropertyNameNode();
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
                var node = new NotOverriddenStatePropertyNameNode();
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

    class ValidOverriddenStatePropertyNameNode : Node
    {
        public override string StatePropertyName => "PropertyName";

        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }

    class DuplicateOverriddenStatePropertyNameNode : Node
    {
        public override string StatePropertyName => "PropertyName";

        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }

    internal class EmptyOverriddenStatePropertyNameNode : Node
    {
        public override string StatePropertyName => "";

        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }

    internal class NullOverriddenStatePropertyNameNode : Node
    {
        public override string StatePropertyName => null;

        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }

    internal class NotOverriddenStatePropertyNameNode : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }

}
