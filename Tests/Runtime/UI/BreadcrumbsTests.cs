using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Breadcrumbs))]
    class BreadcrumbsTests : VisualElementTests<Breadcrumbs>
    {
        protected override string mainUssClassName => Breadcrumbs.ussClassName;

        // Handed over to BreadcrumbsSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        // An empty Breadcrumbs is an empty row. The trail is the component.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => BuildTrail());
            }
        }

        static Breadcrumbs BuildTrail()
        {
            var breadcrumbs = new Breadcrumbs();
            breadcrumbs.Add(new BreadcrumbItem { text = "Home" });
            breadcrumbs.Add(new BreadcrumbSeparator());
            breadcrumbs.Add(new BreadcrumbItem { text = "Library" });
            breadcrumbs.Add(new BreadcrumbSeparator());
            breadcrumbs.Add(new BreadcrumbItem { text = "Assets" });
            breadcrumbs.Add(new BreadcrumbSeparator());
            breadcrumbs.Add(new BreadcrumbItem { text = "MyAsset", isCurrent = true });
            return breadcrumbs;
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Breadcrumbs>
                <appui:BreadcrumbItem text=""Home"" />
                <appui:BreadcrumbSeparator />
                <appui:BreadcrumbItem text=""Library"" />
                <appui:BreadcrumbSeparator />
                <appui:BreadcrumbItem text=""Assets"" />
                <appui:BreadcrumbSeparator />
                <appui:BreadcrumbItem text=""MyAsset"" is-current=""true"" />
            </appui:Breadcrumbs>"
        };
    }

    [TestFixture]
    [TestOf(typeof(BreadcrumbItem))]
    class BreadcrumbItemTests : VisualElementTests<BreadcrumbItem>
    {
        // Handed over to BreadcrumbItemSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;


        // A textless BreadcrumbItem lays out to nothing.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new BreadcrumbItem { text = "Library" });
                yield return new Story("Current", (ctx) => new BreadcrumbItem { text = "MyAsset", isCurrent = true });
            }
        }

        protected override string mainUssClassName => BreadcrumbItem.ussClassName;

        [Test]
        public void Size_DefaultsToSm()
        {
            var item = new BreadcrumbItem { text = "Home" };
            Assert.AreEqual(LinkSize.Sm, item.size);
            Assert.IsTrue(item.ClassListContains(Link.GetSizeUssClassName(LinkSize.Sm)));
        }

        [Test]
        public void Size_ExplicitValueReplacesTheDefault()
        {
            var item = new BreadcrumbItem { text = "Home", size = LinkSize.Lg };
            Assert.AreEqual(LinkSize.Lg, item.size);
            Assert.IsTrue(item.ClassListContains(Link.GetSizeUssClassName(LinkSize.Lg)));
            Assert.IsFalse(item.ClassListContains(Link.GetSizeUssClassName(LinkSize.Sm)));
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Stylesheet_LeavesTheSizeToTheLinkSizeClasses()
        {
            // A font-size on a two-class breadcrumb rule would outrank every .appui-link--size-*
            // rule and make BreadcrumbItem.size do nothing.
            var uss = UssSource.Read("Breadcrumbs.uss");
            foreach (var selector in new[] { ".appui-breadcrumb-item", ".appui-breadcrumb-item.appui-link" })
            {
                var block = UssSource.RuleBlock(uss, selector);
                if (block != null)
                    StringAssert.DoesNotContain("font-size", block,
                        $"{selector} sets a font-size, which overrides BreadcrumbItem.size.");
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:BreadcrumbItem text=""Home"" />",
            @"<appui:BreadcrumbItem text=""Home"" is-current=""true"" />"
        };
    }

    [TestFixture]
    [TestOf(typeof(BreadcrumbSeparator))]
    class BreadcrumbSeparatorTests : VisualElementTests<BreadcrumbSeparator>
    {
        // Handed over to BreadcrumbSeparatorSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override string mainUssClassName => BreadcrumbSeparator.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:BreadcrumbSeparator />",
            @"<appui:BreadcrumbSeparator text="">"" />"
        };
        /// <summary>
        /// A cramped bar has to ellipsize the crumb labels rather than squash the separators to nothing,
        /// which is what a separator with no shrink guard does.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_SeparatorDoesNotShrink()
        {
            var uss = System.IO.File.ReadAllText(System.IO.Path.GetFullPath(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Breadcrumbs.uss"));
            var rule = System.Text.RegularExpressions.Regex.Match(uss,
                @"\.appui-breadcrumb-separator\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, "Breadcrumbs.uss has no separator rule.");
            StringAssert.Contains("flex-shrink: 0", rule.Groups["body"].Value);
        }

    }
}
