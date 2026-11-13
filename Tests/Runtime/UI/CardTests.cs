using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Card))]
    class CardTests : VisualElementTests<Card>
    {
        protected override string mainUssClassName => Card.ussClassName;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => BuildCard(false, false));
                yield return new Story("Interactive", InteractiveHovered);
                yield return new Story("Selected", (ctx) => BuildCard(true, true));
            }
        }

        /// <summary>
        /// An interactive card under the pointer.
        /// </summary>
        /// <remarks>
        /// At rest <c>interactive</c> only changes the cursor, so an unhovered interactive card is
        /// pixel-identical to <c>Default</c>. Everything it paints lives under <c>.is-hovered</c>
        /// and <c>.is-active</c>, which <c>Pressable</c> maintains from pointer events; the class
        /// is raised directly here, after attach so nothing clears it.
        /// </remarks>
        static IEnumerator InteractiveHovered(StoryContext ctx)
        {
            var card = BuildCard(true, false);
            ctx.container.Add(card);
            yield return null;

            s_RestingBackgroundColor = card.resolvedStyle.backgroundColor;
            card.AddToClassList(Styles.hoveredUssClassName);

            // The hover tint arrives over a 200ms background-color transition, so the frame after
            // the class is added still shows the resting colour.
            yield return WaitForTransition(
                () => card.resolvedStyle.backgroundColor,
                s_RestingBackgroundColor);
        }

        /// <summary>
        /// What the same card resolved to before it was hovered.
        /// </summary>
        /// <remarks>
        /// Recorded from the attached card rather than built fresh for the comparison: an
        /// unattached card has no stylesheets, so its resolved colour is a default that differs
        /// from both the resting and the hovered one -- an assertion against it would hold even
        /// with the hover rule deleted.
        /// </remarks>
        static Color s_RestingBackgroundColor;

        [UnityTest]
        public IEnumerator InteractiveStory_CapturesAVisibleState()
        {
            yield return RunStory("Interactive", ctx =>
            {
                var card = ctx.container.Q<Card>();
                Assert.IsNotNull(card);
                Assert.IsTrue(card.interactive);
                Assert.IsTrue(card.ClassListContains(Styles.hoveredUssClassName),
                    "At rest `interactive` only changes the cursor, so without a hover state this "
                    + "story is pixel-identical to Default.");
                Assert.AreNotEqual(s_RestingBackgroundColor, card.resolvedStyle.backgroundColor,
                    "The hover tint must actually resolve, and the transition must have finished "
                    + "before the frame is captured.");
            });
        }

        static Card BuildCard(bool interactive, bool selected)
        {
            var card = new Card { interactive = interactive, selected = selected, style = { width = 220 } };
            card.Add(new CardMedia { style = { height = 100 } });
            var header = new CardHeader();
            header.Add(new CardTitle { text = "Third Person Template" });
            header.Add(new CardDescription { text = "A ready-made character controller." });
            card.Add(header);
            var content = new CardContent();
            content.Add(new Text("URP  •  2.4 GB"));
            card.Add(content);
            var footer = new CardFooter();
            footer.Add(new Unity.AppUI.UI.Button { title = "Open" });
            card.Add(footer);
            return card;
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Card />",
            @"<appui:Card interactive=""true"" />",
            @"<appui:Card interactive=""true"" selected=""true"" elevation=""2"" />",
            @"<appui:Card>
                 <appui:CardMedia />
                 <appui:CardHeader>
                   <appui:CardTitle text=""Title"" />
                   <appui:CardDescription text=""Description"" />
                 </appui:CardHeader>
                 <appui:CardContent><appui:Text text=""Body"" /></appui:CardContent>
                 <appui:CardFooter><appui:Button title=""Open"" /></appui:CardFooter>
               </appui:Card>",
        };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Card.uss";

        static string ReadUss()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            return File.ReadAllText(fullPath);
        }

        static string RuleBody(string uss, string selector)
        {
            var rule = Regex.Match(uss, Regex.Escape(selector) + @"\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, $"Card.uss has no `{selector}` rule.");
            return rule.Groups["body"].Value;
        }

        // ---------------------------------------------------------------- state

        [Test]
        public void Default_IsNeitherInteractiveNorSelected()
        {
            var card = new Card();
            Assert.IsFalse(card.interactive);
            Assert.IsFalse(card.selected);
            Assert.IsFalse(card.ClassListContains(Card.interactiveUssClassName));
            Assert.IsFalse(card.ClassListContains(Styles.selectedUssClassName));
            Assert.AreEqual(0f, card.elevation);
        }

        [Test]
        public void Interactive_TogglesTheUssClassAndFocusability()
        {
            var card = new Card();
            Assert.IsFalse(card.focusable);

            card.interactive = true;
            Assert.IsTrue(card.ClassListContains(Card.interactiveUssClassName));
            Assert.IsTrue(card.focusable);
            Assert.AreEqual(0, card.tabIndex);

            card.interactive = false;
            Assert.IsFalse(card.ClassListContains(Card.interactiveUssClassName),
                "The interactive USS class was not removed, so stale state accumulates.");
            Assert.IsFalse(card.focusable);
        }

        [Test]
        public void Selected_TogglesTheSharedSelectedUssClass()
        {
            var card = new Card { selected = true };
            Assert.IsTrue(card.ClassListContains(Styles.selectedUssClassName));
            card.selected = false;
            Assert.IsFalse(card.ClassListContains(Styles.selectedUssClassName),
                "The selected USS class was not removed.");
        }

        [Test]
        public void Elevation_SwapsTheElevationUssClass()
        {
            var card = new Card { elevation = 2 };
            Assert.IsTrue(card.ClassListContains(Styles.elevationUssClassName + "2"));
            card.elevation = 6;
            var elevationClasses = card.GetClasses()
                .Where(c => c.StartsWith(Styles.elevationUssClassName)).ToList();
            CollectionAssert.AreEqual(new[] { Styles.elevationUssClassName + "6" }, elevationClasses,
                "The previous elevation class was not removed.");
        }

        [Test]
        public void Interactive_ClickFiresTheClickedEvent()
        {
            var card = new Card { interactive = true };
            var clicked = 0;
            card.clicked += () => clicked++;
            card.clickable.InvokePressed(null);
            Assert.AreEqual(1, clicked);
        }

        // ------------------------------------------------------------ sub-parts

        [Test]
        public void SubElements_CarryTheirBemUssClasses()
        {
            Assert.AreEqual("appui-card__media", CardMedia.ussClassName);
            Assert.AreEqual("appui-card__header", CardHeader.ussClassName);
            Assert.AreEqual("appui-card__title", CardTitle.ussClassName);
            Assert.AreEqual("appui-card__description", CardDescription.ussClassName);
            Assert.AreEqual("appui-card__content", CardContent.ussClassName);
            Assert.AreEqual("appui-card__footer", CardFooter.ussClassName);

            Assert.IsTrue(new CardMedia().ClassListContains(CardMedia.ussClassName));
            Assert.IsTrue(new CardHeader().ClassListContains(CardHeader.ussClassName));
            Assert.IsTrue(new CardTitle().ClassListContains(CardTitle.ussClassName));
            Assert.IsTrue(new CardDescription().ClassListContains(CardDescription.ussClassName));
            Assert.IsTrue(new CardContent().ClassListContains(CardContent.ussClassName));
            Assert.IsTrue(new CardFooter().ClassListContains(CardFooter.ussClassName));
        }

        [Test]
        public void CardTitleAndDescription_ExposeText()
        {
            Assert.AreEqual("Hello", new CardTitle { text = "Hello" }.text);
            Assert.AreEqual("World", new CardDescription { text = "World" }.text);
        }

        [UnityTest]
        public IEnumerator CardHeader_ZeroesTheGapOnItsFirstChildOnly()
        {
            var header = new CardHeader();
            var title = new CardTitle { text = "Title" };
            var description = new CardDescription { text = "Description" };
            header.Add(title);
            header.Add(description);
            yield return Mount(header);

            Assert.IsTrue(title.ClassListContains(Styles.firstChildUssClassName),
                "The first child of a CardHeader must carry the first-child marker so it gets no leading gap.");
            Assert.IsFalse(description.ClassListContains(Styles.firstChildUssClassName));
            Assert.Greater(description.resolvedStyle.marginTop, 0f,
                "Subsequent CardHeader children must be separated by the header gap.");
            Assert.AreEqual(0f, title.resolvedStyle.marginTop, 0.01f);
        }

        // ------------------------------------------------------------ USS tokens

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Card_BindsItsSurfaceForegroundAndRadiusToTokens()
        {
            var body = RuleBody(ReadUss(), ".appui-card");
            StringAssert.Contains("background-color: var(--appui-alias-surface-raised)", body);
            StringAssert.Contains("color: var(--appui-alias-text-primary)", body);
            StringAssert.Contains("border-radius: var(--appui-border-radius-lg)", body);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void InteractiveHover_BindsToTheControlHoverToken()
        {
            var uss = ReadUss();
            var body = RuleBody(uss, ".appui-card--interactive.is-hovered");
            StringAssert.Contains("background-color: var(--appui-alias-control-background-color-hover)", body);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Selected_BindsToTheAccentActionToken()
        {
            var body = RuleBody(ReadUss(), ".appui-card.is-selected");
            StringAssert.Contains(
                "border-color: var(--appui-alias-actions-accent-background-color-default)", body);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void TitleAndDescription_BindToTheTypeAndTextTokens()
        {
            var uss = ReadUss();
            var title = RuleBody(uss, ".appui-card__title");
            StringAssert.Contains("font-size: var(--appui-font-sizes-200)", title);
            StringAssert.Contains("color: var(--appui-alias-text-primary)", title);

            var description = RuleBody(uss, ".appui-card__description");
            StringAssert.Contains("font-size: var(--appui-font-sizes-100)", description);
            StringAssert.Contains("color: var(--appui-alias-text-secondary)", description);
        }

        /// <summary>
        /// Tokens are the contract: no literal colours and no magic pixel values anywhere in
        /// Card.uss. Only the border-width / radius tokens may carry a unit, and they carry it
        /// through a var(), so a bare `NNpx` or `#hex` here is always a bug.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Card_UsesNoLiteralColourAndNoMagicPixelValue()
        {
            var uss = StripComments(ReadUss());
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "Card.uss contains a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "Card.uss contains a literal rgb()/rgba() colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @":\s*-?\d+(\.\d+)?px"), "Card.uss contains a magic pixel value.");
        }

        static string StripComments(string uss) => Regex.Replace(uss, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

        // ----------------------------------------------------------- layout/density

        [UnityTest]
        public IEnumerator CardHeaderPadding_ShrinksUnderCompactDensity()
        {
            var normalHeader = new CardHeader();
            normalHeader.Add(new CardTitle { text = "Title" });
            yield return Mount(normalHeader, "normal");
            var normalPadding = normalHeader.resolvedStyle.paddingTop;

            var compactHeader = new CardHeader();
            compactHeader.Add(new CardTitle { text = "Title" });
            yield return Mount(compactHeader, "compact");
            var compactPadding = compactHeader.resolvedStyle.paddingTop;

            Assert.Greater(normalPadding, 0f, "The CardHeader padding rule is missing.");
            Assert.Less(compactPadding, normalPadding,
                $"The CardHeader padding did not shrink under compact density " +
                $"(normal {normalPadding}, compact {compactPadding}). It is probably a hardcoded pixel value.");
        }

        IEnumerator Mount(VisualElement content, string density = "normal")
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "dark", density = density, lang = "en" };
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(content);

            for (var i = 0; i < 4; i++)
                yield return null;
        }
    }
}
