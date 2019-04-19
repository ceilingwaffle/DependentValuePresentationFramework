namespace DVPF.Tests
{
    using System;

    using DVPF.Core;
    using DVPF.Tests.NodeTestClasses;

    using NUnit.Framework;

    /// <summary>
    /// The state property attribute test.
    /// </summary>
    [TestFixture]
    internal class StatePropertyAttributeTest
    {
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
        /// Asserts that no exceptions thrown for <see cref="Node"/> with <see cref="StatePropertyAttribute"/> 
        /// <para />(See: <seealso cref="ValidEnabledStatePropertyNode"/>)
        /// </summary>
        [Test]
        public void TestStatePropertyNameValidEnabled()
        {
            try
            {
                var dummy = new ValidEnabledStatePropertyNode();
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

        /// <summary>
        /// Asserts that <see cref="ArgumentException"/> is thrown when declaring two <see cref="Node"/>s with the same <see cref="StatePropertyAttribute.Name"/>
        /// <para />(See: <seealso cref="DuplicateNameStatePropertyNode"/>)
        /// </summary>
        [Test]
        public void TestStatePropertyNameDeclaredWithTheSameNameOnAnotherNode()
        {
            var dummy1 = new ValidEnabledStatePropertyNode();

            try
            {
                var dummy2 = new DuplicateNameStatePropertyNode();
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected ArgumentException but got Exception: {e.Message}.");
            }

            Assert.Fail("Expected ArgumentException but got no Exception.");
        }

        /// <summary>
        /// Asserts that <see cref="ArgumentException"/> is thrown when attempting to declare a <see cref="Node"/> with an empty string for <see cref="StatePropertyAttribute.Name"/>
        /// </summary>
        [Test]
        public void TestStatePropertyNameWithEmptyName()
        {
            try
            {
                var dummy = new EmptyStringNameStatePropertyNode();
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected ArgumentException but got Exception: {e.Message}.");
            }

            Assert.Fail("Expected ArgumentException but got no Exception.");
        }

        /// <summary>
        /// Asserts that <see cref="ArgumentException"/> is thrown when attempting to declare a <see cref="Node"/> containing only "white space" for <see cref="StatePropertyAttribute.Name"/>
        /// </summary>
        [Test]
        public void TestStatePropertyNameContainingOnlyWhiteSpace()
        {
            try
            {
                var dummy = new WhiteSpaceNameStatePropertyNode();
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected ArgumentException but got Exception: {e.Message}.");
            }

            Assert.Fail("Expected ArgumentException but got no Exception.");
        }

        /// <summary>
        /// Asserts that <see cref="ArgumentException"/> is thrown when attempting to declare a <see cref="Node"/> with null for <see cref="StatePropertyAttribute.Name"/>
        /// </summary>
        [Test]
        public void TestStatePropertyNameWithNullName()
        {
            try
            {
                var dummy = new NullNameStatePropertyNode();
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected no Exception but got: {e.Message}.");
            }

            Assert.Fail("Expected no ArgumentException but got no Exception.");
        }

        /// <summary>
        /// Asserts that no exception is thrown when attempting to declare a <see cref="Node"/> with <see cref="StatePropertyAttribute.Enabled"/> set to false
        /// </summary>
        [Test]
        public void TestStatePropertyAttributeNotEnabled()
        {
            try
            {
                var dummy = new DisabledStatePropertyNode();
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

        /// <summary>
        /// Asserts that no exception is thrown when attempting to initialize a <see cref="Node"/> with no declared <see cref="StatePropertyAttribute"/>
        /// </summary>
        [Test]
        public void TestNodeWhenStatePropertyAttributeIsNotDeclared()
        {
            try
            {
                var dummy = new AttributeNotDefinedStatePropertyNode();
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
}
