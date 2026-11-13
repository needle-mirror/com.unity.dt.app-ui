using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Text))]
    class TextTests : VisualElementTests<Text>
    {
        protected override string mainUssClassName => Text.ussClassName;

        // Handed over to TextSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Text("The quick brown fox"));
                yield return new Story("Sizes", (ctx) =>
                {
                    var root = new VisualElement();
                    foreach (TextSize size in System.Enum.GetValues(typeof(TextSize)))
                        root.Add(new Text(size.ToString()) { size = size });
                    return root;
                });
                yield return new Story("Variants", (ctx) =>
                {
                    var root = new VisualElement();
                    foreach (TextVariant variant in System.Enum.GetValues(typeof(TextVariant)))
                        root.Add(new Text(variant.ToString()) { variant = variant });
                    return root;
                });
                yield return new Story("Disabled", (ctx) =>
                {
                    // Disabled coverage from the UUM-108227 audit.
                    var text = new Text { text = "Text" };
                    text.SetEnabled(false);
                    return text;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Text />",
            @"<appui:Text text=""Body"" size=""M"" />",
            @"<appui:Text text=""Caption"" size=""XS"" variant=""Muted"" />",
            @"<appui:Text text=""Lead"" size=""XXXL"" variant=""Secondary"" />",
            @"<appui:Text text=""Text"" enabled=""false"" />",
        };

        /// <summary>The registry's `textVariants` size vocabulary, mirrored 1:1.</summary>
        static readonly TextSize[] k_AllSizes =
        {
            TextSize.XS, TextSize.S, TextSize.M,
            TextSize.L, TextSize.XL, TextSize.XXL, TextSize.XXXL,
        };

        static readonly TextVariant[] k_AllVariants =
        {
            TextVariant.Default, TextVariant.Secondary, TextVariant.Muted,
        };

        /// <summary>
        /// Normal-density px each size resolves to. Mirrors the registry's
        /// `TEXT_TIERS` (xs←75, sm←100, base←200) and its continuation.
        /// </summary>
        static readonly Dictionary<TextSize, float> k_NormalPx = new()
        {
            { TextSize.XS, 11f },
            { TextSize.S, 12f },
            { TextSize.M, 14f },
            { TextSize.L, 18f },
            { TextSize.XL, 19f },
            { TextSize.XXL, 25f },
            { TextSize.XXXL, 32f },
        };

        /// <summary>Same steps under `.appui--compact` — proof the USS binds to the token vars.</summary>
        static readonly Dictionary<TextSize, float> k_CompactPx = new()
        {
            { TextSize.XS, 10f },
            { TextSize.S, 12f },
            { TextSize.M, 12f },
            { TextSize.L, 15f },
            { TextSize.XL, 17f },
            { TextSize.XXL, 21f },
            { TextSize.XXXL, 27f },
        };

        static IEnumerable<string> SizeClasses(VisualElement el) =>
            el.GetClasses().Where(c => c.StartsWith(Text.sizeUssClassName));

        static IEnumerable<string> VariantClasses(VisualElement el) =>
            k_AllVariants.Select(Text.GetVariantUssClassName).Where(el.ClassListContains);

        [Test]
        public void Size_ShouldDefaultToBase()
        {
            var text = new Text("hello");
            Assert.AreEqual(TextSize.M, text.size);
            Assert.IsTrue(text.ClassListContains(Text.GetSizeUssClassName(TextSize.M)));
        }

        [Test]
        public void Variant_ShouldDefaultToDefault()
        {
            var text = new Text("hello");
            Assert.AreEqual(TextVariant.Default, text.variant);
            Assert.IsTrue(text.ClassListContains(Text.GetVariantUssClassName(TextVariant.Default)));
        }

        [Test]
        public void SizeUssClassNames_ShouldFollowTheMemberNames()
        {
            CollectionAssert.AreEqual(k_AllSizes, System.Enum.GetValues(typeof(TextSize)));
            var expected = new[]
            {
                "appui-text--size-xs", "appui-text--size-s", "appui-text--size-m",
                "appui-text--size-l", "appui-text--size-xl", "appui-text--size-xxl",
                "appui-text--size-xxxl",
            };
            CollectionAssert.AreEqual(expected, k_AllSizes.Select(Text.GetSizeUssClassName));
        }

        [Test]
        public void VariantUssClassNames_ShouldMirrorTheRegistryVocabulary()
        {
            CollectionAssert.AreEqual(k_AllVariants, System.Enum.GetValues(typeof(TextVariant)));
            var expected = new[]
            {
                "appui-text--default", "appui-text--secondary", "appui-text--muted",
            };
            CollectionAssert.AreEqual(expected, k_AllVariants.Select(Text.GetVariantUssClassName));
        }

        [Test]
        public void Size_ShouldApplyExactlyOneSizeClass([ValueSource(nameof(k_AllSizes))] TextSize size)
        {
            var text = new Text("hello") { size = size };
            Assert.AreEqual(size, text.size);
            CollectionAssert.AreEqual(
                new[] { Text.GetSizeUssClassName(size) },
                SizeClasses(text),
                "Exactly one size class must be present; the previous one must be removed.");
        }

        [Test]
        public void Variant_ShouldApplyExactlyOneVariantClass([ValueSource(nameof(k_AllVariants))] TextVariant variant)
        {
            var text = new Text("hello") { variant = variant };
            Assert.AreEqual(variant, text.variant);
            CollectionAssert.AreEqual(
                new[] { Text.GetVariantUssClassName(variant) },
                VariantClasses(text),
                "Exactly one variant class must be present; the previous one must be removed.");
        }

        [Test]
        public void Size_ShouldNotAccumulateStaleClasses()
        {
            var text = new Text("hello");
            foreach (var size in k_AllSizes)
                text.size = size;
            CollectionAssert.AreEqual(new[] { Text.GetSizeUssClassName(TextSize.XXXL) }, SizeClasses(text));
        }

        [Test]
        public void Variant_ShouldNotAccumulateStaleClasses()
        {
            var text = new Text("hello");
            foreach (var variant in k_AllVariants)
                text.variant = variant;
            CollectionAssert.AreEqual(new[] { Text.GetVariantUssClassName(TextVariant.Muted) }, VariantClasses(text));
        }

        [UnityTest]
        public IEnumerator FontSize_ShouldResolveFromTheFontSizeTokens()
        {
            yield return AssertResolvedFontSizes("normal", k_NormalPx);
        }

        [UnityTest]
        public IEnumerator FontSize_ShouldRescaleWithDensity()
        {
            yield return AssertResolvedFontSizes("compact", k_CompactPx);
        }

        [UnityTest]
        public IEnumerator Variant_ShouldResolveDistinctForegroundTokens()
        {
            var panel = BuildPanel("normal");
            var elements = k_AllVariants.ToDictionary(v => v, v => new Text("hello") { variant = v });
            foreach (var el in elements.Values)
                panel.Add(el);
            yield return null;
            yield return null;
            yield return null;

            var colors = elements.ToDictionary(kv => kv.Key, kv => kv.Value.resolvedStyle.color);
            foreach (var kv in colors)
                Assert.AreNotEqual(default(UnityEngine.Color), kv.Value, $"{kv.Key} resolved to no color at all.");

            Assert.AreNotEqual(colors[TextVariant.Default], colors[TextVariant.Secondary],
                "secondary must not resolve to the same token as default.");
            Assert.AreNotEqual(colors[TextVariant.Default], colors[TextVariant.Muted],
                "muted must not resolve to the same token as default.");
            Assert.AreNotEqual(colors[TextVariant.Secondary], colors[TextVariant.Muted],
                "secondary and muted must not resolve to the same token.");
        }

        Panel BuildPanel(string density)
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(
                UnityEngine.Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(
                UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                    "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "dark", density = density, lang = "en" };
            m_TestUI.rootVisualElement.Add(panel);
            return panel;
        }

        IEnumerator AssertResolvedFontSizes(string density, Dictionary<TextSize, float> expected)
        {
            var panel = BuildPanel(density);
            var elements = k_AllSizes.ToDictionary(s => s, s => new Text(s.ToString()) { size = s });
            foreach (var el in elements.Values)
                panel.Add(el);
            yield return null;
            yield return null;
            yield return null;

            foreach (var size in k_AllSizes)
            {
                Assert.AreEqual(expected[size], elements[size].resolvedStyle.fontSize, 0.01f,
                    $"{density}/{size} must resolve from --appui-font-sizes-*, not a hardcoded px.");
            }
        }
    }
}
