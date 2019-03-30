using System;
using NUnit.Framework;
using DVPF.Core;

namespace DVPF.Tests
{
    [TestFixture]
    class StatePropertyAttributeTest
    {
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

        [Test]
        public void TestStatePropertyName_ValidEnabled()
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
        public void TestStatePropertyName_SameNameAsAnother()
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
        public void TestStatePropertyName_EmptyStringName()
        {
            try
            {
                var node = new EmptyStringNameStatePropertyNode();
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
        public void TestStatePropertyName_WhiteSpaceName()
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
        public void TestStatePropertyName_NullName()
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
        public void TestStatePropertyName_NotEnabled()
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

        [Test]
        public void TestStatePropertyName_AttributeNotDefined()
        {
            try
            {
                var node = new AttributeNotDefinedStatePropertyNode();
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
