using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Navigation;
using UnityEngine;

namespace Unity.AppUI.Tests.Navigation
{
    [TestFixture]
    [TestOf(typeof(NavDestination))]
    class NavDestinationTests
    {
        NavDestination m_Destination;

        [SetUp]
        public void SetUp()
        {
            m_Destination = ScriptableObject.CreateInstance<NavDestination>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Destination);
        }

        [Test]
        public void MergeArguments_ReplacesDefaultWithProvidedArgumentOfSameName()
        {
            m_Destination.arguments = new List<Argument>
            {
                new Argument("id", "1"),
                new Argument("mode", "view"),
            };

            var merged = m_Destination.MergeArguments(new Argument("id", "2"));

            Assert.AreEqual(2, merged.Length);
            Assert.AreEqual(new Argument("id", "2"), merged[0]);
            Assert.AreEqual(new Argument("mode", "view"), merged[1]);
        }

        [Test]
        public void MergeArguments_AppendsProvidedArgumentWithNewName()
        {
            m_Destination.arguments = new List<Argument> { new Argument("mode", "view") };

            var merged = m_Destination.MergeArguments(new Argument("id", "2"));

            Assert.AreEqual(2, merged.Length);
            Assert.AreEqual(new Argument("mode", "view"), merged[0]);
            Assert.AreEqual(new Argument("id", "2"), merged[1]);
        }
    }
}
