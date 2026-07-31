#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(VisualDocRegistry))]
    class VisualDocRegistryTests
    {
        [Test]
        public void Registry_IsPopulated()
        {
            Assert.That(VisualDocRegistry.pages, Is.Not.Empty);
        }

        [Test]
        public void Pages_HaveUniqueIds()
        {
            var ids = VisualDocRegistry.pages.Select(p => p.id).ToList();
            Assert.That(ids, Is.Unique);
        }

        [Test]
        public void Pages_HaveRequiredContent()
        {
            foreach (var page in VisualDocRegistry.pages)
            {
                Assert.That(page.id, Is.Not.Empty);
                Assert.That(page.displayName, Is.Not.Empty, $"Page '{page.id}' has no display name.");
                Assert.That(page.category, Is.Not.Empty, $"Page '{page.id}' has no category.");
                Assert.That(page.summary, Is.Not.Empty, $"Page '{page.id}' has no summary.");
                Assert.That(page.sections, Is.Not.Empty, $"Page '{page.id}' has no sections.");
            }
        }

        [Test]
        public void Pages_ReferenceExistingTypes()
        {
            var assembly = typeof(VisualDocRegistry).Assembly;
            foreach (var page in VisualDocRegistry.pages)
            {
                Assert.That(assembly.GetType(page.typeFullName), Is.Not.Null,
                    $"Page '{page.id}' references unknown type '{page.typeFullName}'.");
            }
        }

        [Test]
        public void KnownComponentPages_ArePresent()
        {
            var ids = new HashSet<string>(VisualDocRegistry.pages.Select(p => p.id));
            Assert.That(ids, Does.Contain("button"));
            Assert.That(ids, Does.Contain("accordion"));
            Assert.That(ids, Does.Contain("textfield"));
        }
    }
}
#endif
