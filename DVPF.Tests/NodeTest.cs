namespace DVPF.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using DVPF.Core;
    using DVPF.Tests.Nodes;
    using DVPF.Tests.NodeTestClasses;

    using NUnit.Framework;

    /// <summary>
    /// Tests for class <see cref="Node"/>
    /// </summary>
    [TestFixture]
    public class NodeTest
    {
        // TODO: Write test for ensuring disabled nodes do not call their CalculateValue() methods

        /// <summary>
        /// The set up.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
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
        /// Tests preceder has follower.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
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
        /// Tests follower has preceder.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
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

        /// <summary>
        /// Test for method <see cref="Node.HasFollowers"/>
        /// </summary>
        [Test]
        public void TestHasFollowersMethod()
        {
            var milkyWay = new MilkyWay();

            Assert.IsFalse(milkyWay.HasFollowers());

            var solarSystem = new SolarSystem();
            milkyWay.Precedes(solarSystem);

            Assert.IsTrue(milkyWay.HasFollowers());
        }

        /// <summary>
        /// Test for method <see cref="Node.HasPreceders"/>
        /// </summary>
        [Test]
        public void TestHasPrecedersMethod()
        {
            var solarSystem = new SolarSystem();

            Assert.IsFalse(solarSystem.HasPreceders());

            var milkyWay = new MilkyWay();
            solarSystem.Follows(milkyWay);

            Assert.IsTrue(solarSystem.HasPreceders());
        }

        /// <summary>
        /// Asserts that an exception is thrown when initializing two nodes of same type.
        /// </summary>
        [Test]
        public void TestExceptionThrownWhenInitializingTwoNodesOfSameType()
        {
            var dummy1 = new MilkyWay();

            try
            {
                var dummy2 = new MilkyWay();
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Expected ArgumentException (initializing the same node types should not be allowed.");
        }

        /// <summary>
        /// Asserts that an exception is thrown when making Node A follow Node B, then making Node B precede Node A.
        /// </summary>
        [Test]
        public void TestExceptionThrownWhenCallingRedundantPrecedeAfterFollow()
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

        /// <summary>
        /// Asserts that an exception is thrown when making Node A precede Node B, then making Node B follow Node A.
        /// </summary>
        [Test]
        public void TestExceptionThrownWhenCallingRedundantFollowAfterPrecede()
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

        /// <summary>
        /// Asserts that an exception is thrown when a node attempts to follow itself.
        /// </summary>
        [Test]
        public void TestExceptionThrownWhenNodeFollowsItself()
        {
            var solarSystem = new SolarSystem();

            try
            {
                solarSystem.Follows(solarSystem);
            }
            catch (Exception)
            {
                Assert.Pass();
            }

            Assert.Fail("Excepted Exception (node cannot follow itself).");
        }

        /// <summary>
        /// Asserts that an exception is thrown when a node attempts to follow a null object.
        /// </summary>
        [Test]
        public void TestExceptionThrownWhenNodeFollowsNullNode()
        {
            var solarSystem = new SolarSystem();

            try
            {
                solarSystem.Follows(null);
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Excepted ArgumentException (node cannot follow a null node).");
        }

        /// <summary>
        /// Asserts that an exception is thrown when a node attempts to precede itself.
        /// </summary>
        [Test]
        public void TestExceptionThrownWhenNodePrecedesItself()
        {
            var solarSystem = new SolarSystem();

            try
            {
                solarSystem.Precedes(solarSystem);
            }
            catch (Exception)
            {
                Assert.Pass();
            }

            Assert.Fail("Excepted Exception (node cannot precede itself).");
        }

        /// <summary>
        /// Asserts that an exception is thrown when a node attempts to precede a null object.
        /// </summary>
        [Test]
        public void TestExceptionThrownWhenNodePrecedesNullNode()
        {
            var solarSystem = new SolarSystem();

            try
            {
                solarSystem.Precedes(null);
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Excepted ArgumentException (node cannot precede a null node).");
        }
    }
}
