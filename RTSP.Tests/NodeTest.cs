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
        // TODO: Write test for ensuring disabled nodes do not call their CalculateValue() methods

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
        /// Tests preceder has follower
        /// </summary>
        [Test]
        public void TestFollowerOfPreceder()
        {
            // setup
            var milkyWay = new MilkyWay();
            var solarSystem = new SolarSystem();
            milkyWay.Precedes(solarSystem);
            var milkyWayFollowers = milkyWay.Followers;
            var milkyWayHasFollower = milkyWayFollowers.TryGetValue(typeof(SolarSystem), out var milkyWayFollower);

            // assertions
            Assert.IsTrue(milkyWayHasFollower);
            Assert.IsNotNull(milkyWayFollowers);
            Assert.AreSame(milkyWayFollower, solarSystem);
        }

        /// <summary>
        /// Tests follower has preceder
        /// </summary>
        [Test]
        public void TestPrecederOfFollower()
        {
            // setup
            var milkyWay = new MilkyWay();
            var solarSystem = new SolarSystem();
            milkyWay.Precedes(solarSystem);
            var solarSystemPreceders = solarSystem.Preceders;
            var solarSystemHasPreceder = solarSystemPreceders.TryGetValue(typeof(MilkyWay), out var solarSystemPreceder);
            
            // assertions
            Assert.IsNotNull(solarSystemPreceders);
            Assert.IsTrue(solarSystemHasPreceder);
            Assert.AreSame(solarSystemPreceder, milkyWay);
        }

        [Test]
        public void TestHasFollowersMethod()
        {
            var milkyWay = new MilkyWay();

            Assert.IsFalse(milkyWay.HasFollowers());

            var solarSystem = new SolarSystem();
            milkyWay.Precedes(solarSystem);

            Assert.IsTrue(milkyWay.HasFollowers());
        }

        [Test]
        public void TestHasPrecedersMethod()
        {
            var solarSystem = new SolarSystem();

            Assert.IsFalse(solarSystem.HasPreceders());

            var milkyWay = new MilkyWay();
            solarSystem.Follows(milkyWay);

            Assert.IsTrue(solarSystem.HasPreceders());
        }

        [Test]
        public void TestExceptionThrown_InitializingTwoNodesOfSameType()
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
        public void TestExceptionThrown_CallingRedundantPrecedeAfterFollow()
        {
            var milkyWay = new MilkyWay();
            var solarSystem = new SolarSystem();

            solarSystem.Follows(milkyWay);

            try
            {
                milkyWay.Precedes(solarSystem);
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Expected ArgumentException (attempted to assign node relationship twice " +
                        "-- redundant call to b.Precedes(a) when already called a.Follows(b).");

        }

        [Test]
        public void TestExceptionThrown_CallingRedundantFollowAfterPrecede()
        {
            var milkyWay = new MilkyWay();
            var solarSystem = new SolarSystem();

            milkyWay.Precedes(solarSystem);

            try
            {
                solarSystem.Follows(milkyWay);
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Expected ArgumentException (attempted to assign node relationship twice " +
                        "-- redundant call to a.Follows(b) when already called b.Precedes(a)).");

        }

        [Test]
        public void TestExceptionThrown_NodeFollowsItself()
        {
            var solarSystem = new SolarSystem();

            try
            {
                solarSystem.Follows(solarSystem);
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Excepted ArgumentException (node cannot follow itself).");
        }

        [Test]
        public void TestExceptionThrown_NodePrecedesItself()
        {
            var solarSystem = new SolarSystem();

            try
            {
                solarSystem.Precedes(solarSystem);
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Excepted ArgumentException (node cannot precede itself).");
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
    internal class EmptyStringNameStatePropertyNode : Node
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

    internal class AttributeNotDefinedStatePropertyNode : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }

}
