using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ImageLightbox))]
    class ImageLightboxTests : VisualElementTests<ImageLightbox>
    {
        protected override string mainUssClassName => ImageLightbox.ussClassName;

        // Handed over to ImageLightboxSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/ImageLightbox.uss";

        readonly List<Texture2D> m_Textures = new List<Texture2D>();

        [TearDown]
        public void DisposeTextures()
        {
            foreach (var texture in m_Textures)
                Object.DestroyImmediate(texture);
            m_Textures.Clear();
        }

        Background Image(int width, int height)
        {
            var texture = new Texture2D(width, height);
            m_Textures.Add(texture);
            return Background.FromTexture2D(texture);
        }

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new ImageLightbox
                {
                    src = Image(64, 32),
                    caption = "cliff-face-01.png",
                    alt = "Cliff face, tiling",
                });
                yield return new Story("NoImage", (ctx) => new ImageLightbox
                {
                    caption = "cliff-face-01.png",
                    fallback = new Text { text = "Preview unavailable" },
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ImageLightbox />",
            @"<appui:ImageLightbox caption=""cliff-face-01.png"" alt=""Cliff face"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var lightbox = new ImageLightbox();
            Assert.IsNull(lightbox.caption);
            Assert.IsNull(lightbox.alt);
            Assert.IsFalse(lightbox.hasImage);
            Assert.AreEqual(0.9f, ImageLightbox.panelWidthFraction, "The registry's 90vw.");
            Assert.AreEqual(0.9f, ImageLightbox.panelHeightFraction, "The registry's 90vh.");
            Assert.AreEqual(0.7f, ImageLightbox.imageHeightFraction, "The registry's max-h-[70vh].");
        }

        [Test]
        public void Fallback_ShowsOnlyWhileThereIsNoImage()
        {
            var lightbox = new ImageLightbox { fallback = new Text { text = "Preview unavailable" } };
            var fallback = lightbox.Q(className: ImageLightbox.fallbackUssClassName);
            var image = lightbox.Q(className: ImageLightbox.imageUssClassName);

            Assert.AreEqual(DisplayStyle.Flex, fallback.resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.None, image.resolvedStyle.display);

            lightbox.src = Image(64, 32);
            Assert.AreEqual(DisplayStyle.None, fallback.resolvedStyle.display,
                "With an image to show, the stand-in must go.");
            Assert.AreEqual(DisplayStyle.Flex, image.resolvedStyle.display);
        }

        [Test]
        public void VectorImage_CountsAsAnImage_AndDoesNotFallBack()
        {
            var lightbox = new ImageLightbox { fallback = new Text { text = "Preview unavailable" } };
            var fallback = lightbox.Q(className: ImageLightbox.fallbackUssClassName);
            var image = lightbox.Q(className: ImageLightbox.imageUssClassName);

            var vector = ScriptableObject.CreateInstance<VectorImage>();
            try
            {
                lightbox.src = Background.FromVectorImage(vector);

                // A vector image has no natural pixel size, which is a sizing question — it must not
                // decide whether an image is there at all.
                Assert.IsTrue(lightbox.hasImage);
                Assert.AreEqual(DisplayStyle.Flex, image.resolvedStyle.display);
                Assert.AreEqual(DisplayStyle.None, fallback.resolvedStyle.display,
                    "A perfectly good vector image was hidden behind the fallback.");
            }
            finally
            {
                Object.DestroyImmediate(vector);
            }
        }

        [UnityTest]
        public IEnumerator LargeImage_IsShrunkToFitThePanel_KeepingItsAspectRatio()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            yield return null;

            // Far larger than any test resolution, and deliberately not square.
            var lightbox = new ImageLightbox { src = Image(4000, 2000), caption = "huge.png" };
            panel.Add(lightbox);
            yield return null;
            yield return null;

            var image = lightbox.Q(className: ImageLightbox.imageUssClassName);
            var available = panel.layout.size;

            Assert.Less(image.resolvedStyle.width, available.x * ImageLightbox.panelWidthFraction + 1f,
                "The image must not spill past the width the registry allows it.");
            Assert.Less(image.resolvedStyle.height, available.y * ImageLightbox.imageHeightFraction + 1f,
                "The image must not spill past the height the registry allows it.");
            Assert.AreEqual(2f, image.resolvedStyle.width / image.resolvedStyle.height, 0.05f,
                "Shrinking must be uniform: a 2:1 source stays 2:1.");
        }

        [UnityTest]
        public IEnumerator SmallImage_IsNeverUpscaled()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            yield return null;

            var lightbox = new ImageLightbox { src = Image(24, 16), caption = "tiny.png" };
            panel.Add(lightbox);
            yield return null;
            yield return null;

            var image = lightbox.Q(className: ImageLightbox.imageUssClassName);
            Assert.AreEqual(24f, image.resolvedStyle.width, 0.5f,
                "An asset whose only file is small must stay crisp at its own resolution.");
            Assert.AreEqual(16f, image.resolvedStyle.height, 0.5f);
        }

        [UnityTest]
        public IEnumerator Viewer_IsCappedToTheAllowedShareOfThePanel()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            yield return null;

            var lightbox = new ImageLightbox { src = Image(4000, 4000), caption = "huge.png" };
            panel.Add(lightbox);
            yield return null;
            yield return null;

            var available = panel.layout.size;
            Assert.AreEqual(available.x * ImageLightbox.panelWidthFraction,
                lightbox.resolvedStyle.maxWidth.value, 1f,
                "UI Toolkit has no viewport unit, so the viewer measures the panel and applies the " +
                "registry's fraction itself.");
            Assert.AreEqual(available.y * ImageLightbox.panelHeightFraction,
                lightbox.resolvedStyle.maxHeight.value, 1f);
        }

        static ImageLightbox NavLightbox(int index, int count) => new ImageLightbox
        {
            caption = "Oak",
            navigationCount = count,
            navigationIndex = index,
        };

        static ActionButton Previous(ImageLightbox l) =>
            l.Q<ActionButton>(className: ImageLightbox.previousUssClassName);

        static ActionButton Next(ImageLightbox l) =>
            l.Q<ActionButton>(className: ImageLightbox.nextUssClassName);

        static VisualElement NavBar(ImageLightbox l) => // the step buttons show and hide together
            Previous(l);

        static string Position(ImageLightbox l) =>
            l.Q<LocalizedTextElement>(className: ImageLightbox.positionUssClassName).text;

        [Test]
        public void Navigation_IsHiddenWithoutASet()
        {
            var single = new ImageLightbox { caption = "Oak" };
            Assert.AreEqual(0, single.navigationCount);
            Assert.AreEqual(DisplayStyle.None, NavBar(single).resolvedStyle.display,
                "Without a set the viewer shows a single image, with no step controls.");
            Assert.AreEqual(DisplayStyle.None,
                single.Q(className: ImageLightbox.positionUssClassName).resolvedStyle.display);

            single.navigationCount = 1;
            Assert.AreEqual(DisplayStyle.None, NavBar(single).resolvedStyle.display,
                "A set of one has nowhere to step to.");

            single.navigationCount = 5;
            Assert.AreEqual(DisplayStyle.Flex, NavBar(single).resolvedStyle.display);
        }

        [Test]
        public void Navigation_ShowsPositionAndDisablesTheEnds()
        {
            var lightbox = NavLightbox(0, 5);
            Assert.AreEqual("1 / 5", Position(lightbox), "The caption leads with its position.");
            Assert.IsFalse(Previous(lightbox).enabledSelf, "Nothing before the first image.");
            Assert.IsTrue(Next(lightbox).enabledSelf);

            lightbox.navigationIndex = 4;
            Assert.AreEqual("5 / 5", Position(lightbox));
            Assert.IsTrue(Previous(lightbox).enabledSelf);
            Assert.IsFalse(Next(lightbox).enabledSelf, "Nothing after the last image.");
        }

        [Test]
        public void Navigation_UsesExistingIconsAndAccessibleNames()
        {
            var lightbox = NavLightbox(1, 3);
            Assert.AreEqual("caret-left", Previous(lightbox).icon);
            Assert.AreEqual("caret-right", Next(lightbox).icon);
            Assert.AreEqual("Previous image", Previous(lightbox).tooltip);
            Assert.AreEqual("Next image", Next(lightbox).tooltip);
            lightbox.previousLabel = "Back";
            lightbox.nextLabel = "Forward";
            Assert.AreEqual("Back", Previous(lightbox).tooltip);
            Assert.AreEqual("Forward", Next(lightbox).tooltip);
        }

        [Test]
        public void Step_DoesNotWrapPastEitherEnd()
        {
            var lightbox = NavLightbox(0, 3);
            var raised = new List<int>();
            lightbox.navigationIndexChanged += raised.Add;

            lightbox.Step(-1);
            Assert.AreEqual(0, lightbox.navigationIndex, "Before the first image, a step does nothing.");
            lightbox.Step(1);
            lightbox.Step(1);
            lightbox.Step(1);
            Assert.AreEqual(2, lightbox.navigationIndex, "Past the last image, a step does nothing.");
            CollectionAssert.AreEqual(new[] { 1, 2 }, raised, "Only real steps are reported.");
        }

        [UnityTest]
        public IEnumerator ArrowKeys_Step()
        {
            var lightbox = NavLightbox(1, 3);
            m_TestUI.rootVisualElement.Add(lightbox);
            yield return null;

            Key(lightbox, KeyCode.RightArrow);
            yield return null;
            Assert.AreEqual(2, lightbox.navigationIndex);
            Key(lightbox, KeyCode.LeftArrow);
            yield return null;
            Key(lightbox, KeyCode.LeftArrow);
            yield return null;
            Assert.AreEqual(0, lightbox.navigationIndex);
        }

        [UnityTest]
        public IEnumerator GamepadMove_Steps_WithoutAKeyEvent()
        {
            var lightbox = NavLightbox(1, 3);
            m_TestUI.rootVisualElement.Add(lightbox);
            yield return null;

            Move(lightbox, NavigationMoveEvent.Direction.Right);
            yield return null;
            Assert.AreEqual(2, lightbox.navigationIndex, "A D-pad sends only a navigation move.");
            Move(lightbox, NavigationMoveEvent.Direction.Left);
            yield return null;
            Assert.AreEqual(1, lightbox.navigationIndex);
        }

        [UnityTest]
        public IEnumerator ArrowKey_AndItsNavigationMove_StepOnce()
        {
            var lightbox = NavLightbox(0, 3);
            m_TestUI.rootVisualElement.Add(lightbox);
            yield return null;

            // An arrow key arrives as both events.
            Key(lightbox, KeyCode.RightArrow);
            Move(lightbox, NavigationMoveEvent.Direction.Right);
            yield return null;
            Assert.AreEqual(1, lightbox.navigationIndex, "One key press is one step.");
            KeyUp(lightbox, KeyCode.RightArrow);
            yield return null;

            // In either order.
            Move(lightbox, NavigationMoveEvent.Direction.Right);
            Key(lightbox, KeyCode.RightArrow);
            yield return null;
            Assert.AreEqual(2, lightbox.navigationIndex, "One key press is one step, whichever event leads.");
        }

        [UnityTest]
        public IEnumerator HeldArrowKey_StepsOnKeyRepeatsOnly()
        {
            var lightbox = NavLightbox(0, 5);
            m_TestUI.rootVisualElement.Add(lightbox);
            yield return null;

            Key(lightbox, KeyCode.RightArrow);
            yield return null;
            // The navigation repeats run on their own schedule, in other frames.
            Move(lightbox, NavigationMoveEvent.Direction.Right);
            yield return null;
            Key(lightbox, KeyCode.RightArrow);
            yield return null;
            Move(lightbox, NavigationMoveEvent.Direction.Right);
            yield return null;
            Assert.AreEqual(2, lightbox.navigationIndex, "Only the key repeats step while the key is held.");

            KeyUp(lightbox, KeyCode.RightArrow);
            yield return null;
            yield return new WaitForSecondsRealtime(0.1f);
            Move(lightbox, NavigationMoveEvent.Direction.Right);
            yield return null;
            Assert.AreEqual(3, lightbox.navigationIndex, "Released, a gamepad move steps again.");
        }

        [UnityTest]
        public IEnumerator SuccessivePresses_InOneFrame_EachStep()
        {
            var lightbox = NavLightbox(0, 5);
            m_TestUI.rootVisualElement.Add(lightbox);
            yield return null;

            // An Edit Mode window does not advance the player frame count between presses.
            Key(lightbox, KeyCode.RightArrow);
            KeyUp(lightbox, KeyCode.RightArrow);
            Key(lightbox, KeyCode.RightArrow);
            KeyUp(lightbox, KeyCode.RightArrow);
            yield return null;
            Assert.AreEqual(2, lightbox.navigationIndex);
        }

        [UnityTest]
        public IEnumerator OneNavigationMove_PairsWithOneKeyPressOnly()
        {
            var lightbox = NavLightbox(0, 5);
            m_TestUI.rootVisualElement.Add(lightbox);
            yield return null;

            Move(lightbox, NavigationMoveEvent.Direction.Right);
            Key(lightbox, KeyCode.RightArrow);
            KeyUp(lightbox, KeyCode.RightArrow);
            Key(lightbox, KeyCode.RightArrow);
            yield return null;
            Assert.AreEqual(2, lightbox.navigationIndex, "A second, distinct press must still step.");
        }

        [UnityTest]
        public IEnumerator LosingApplicationFocus_ReleasesTheHeldArrow()
        {
            var lightbox = NavLightbox(0, 5);
            m_TestUI.rootVisualElement.Add(lightbox);
            yield return null;

            Key(lightbox, KeyCode.RightArrow);
            yield return null;
            // Released in another application: no KeyUp ever arrives.
            lightbox.OnApplicationFocusChanged(false);
            yield return new WaitForSecondsRealtime(0.1f);
            Move(lightbox, NavigationMoveEvent.Direction.Right);
            yield return null;
            Assert.AreEqual(2, lightbox.navigationIndex, "A gamepad move must not stay blocked.");
        }

        static void KeyUp(VisualElement target, KeyCode code)
        {
            using var evt = KeyUpEvent.GetPooled('\0', code, EventModifiers.None);
            evt.target = target;
            target.SendEvent(evt);
        }

        [UnityTest]
        public IEnumerator Show_FocusesTheViewer_SoArrowKeysStepStraightAway()
        {
            var lightbox = NavLightbox(0, 3);
            var modal = lightbox.Show(m_TestUI.rootVisualElement);
            yield return new WaitForSeconds(1f);
            try
            {
                Assert.AreEqual(lightbox, lightbox.focusController?.focusedElement,
                    "The modal focuses its own wrapper, which the arrow keys never reach the viewer from.");
            }
            finally
            {
                modal.Dismiss();
            }
        }

        static void Move(VisualElement target, NavigationMoveEvent.Direction direction)
        {
            using var evt = NavigationMoveEvent.GetPooled(direction);
            evt.target = target;
            target.SendEvent(evt);
        }

        [UnityTest]
        public IEnumerator ArrowKeys_DoNothingWithoutASet()
        {
            var lightbox = NavLightbox(0, 0);
            var raised = 0;
            lightbox.navigationIndexChanged += _ => raised++;
            m_TestUI.rootVisualElement.Add(lightbox);
            yield return null;

            Key(lightbox, KeyCode.RightArrow);
            yield return null;
            Assert.AreEqual(0, raised);
        }

        [UnityTest]
        public IEnumerator Clicking_StepButtons_Steps()
        {
            var lightbox = NavLightbox(0, 3);
            m_TestUI.rootVisualElement.Add(lightbox);
            yield return null;

            Next(lightbox).clickable.InvokePressed(null);
            Assert.AreEqual(1, lightbox.navigationIndex);
            Previous(lightbox).clickable.InvokePressed(null);
            Assert.AreEqual(0, lightbox.navigationIndex);
        }

        [UnityTest]
        public IEnumerator ReachingAnEnd_MovesFocusToTheOtherStepButton()
        {
            var lightbox = NavLightbox(1, 3);
            m_TestUI.rootVisualElement.Add(lightbox);
            yield return null;

            var next = Next(lightbox);
            next.Focus();
            yield return null;
            Assert.AreEqual(next, lightbox.focusController.focusedElement);

            next.clickable.InvokePressed(null);
            yield return null;
            Assert.IsFalse(next.enabledSelf);
            Assert.AreEqual(Previous(lightbox), lightbox.focusController.focusedElement,
                "A disabled button drops focus, so it moves to the other step button.");

            Previous(lightbox).clickable.InvokePressed(null);
            Previous(lightbox).clickable.InvokePressed(null);
            yield return null;
            Assert.AreEqual(Next(lightbox), lightbox.focusController.focusedElement);
        }

        [UnityTest]
        public IEnumerator ALaterCountChange_LeavesFocusAlone()
        {
            var lightbox = NavLightbox(0, 3);
            var elsewhere = new Unity.AppUI.UI.Button { focusable = true };
            m_TestUI.rootVisualElement.Add(lightbox);
            m_TestUI.rootVisualElement.Add(elsewhere);
            yield return null;

            var next = Next(lightbox);
            next.Focus();
            next.clickable.InvokePressed(null);
            yield return null;
            Assert.AreEqual(1, lightbox.navigationIndex);

            elsewhere.Focus();
            yield return null;
            // The set shrinks for reasons of its own: Next disables, but the step is long over.
            lightbox.navigationCount = 2;
            yield return null;
            Assert.IsFalse(next.enabledSelf);
            Assert.AreEqual(elsewhere, lightbox.focusController.focusedElement);
        }

        [UnityTest]
        public IEnumerator TallImage_ScrollsAtFullWidth_InsteadOfShrinking()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            yield return null;

            var available = panel.layout.size;
            var width = Mathf.Max(8, Mathf.FloorToInt(available.x * 0.25f));
            var height = Mathf.CeilToInt(available.y * 3f);
            var lightbox = new ImageLightbox { src = Image(width, height), caption = "sheet.png" };
            panel.Add(lightbox);
            yield return null;
            yield return null;

            Assert.IsTrue(lightbox.isTall);
            var image = lightbox.Q(className: ImageLightbox.imageUssClassName);
            var scroll = lightbox.Q<ScrollView>(className: ImageLightbox.scrollUssClassName);
            Assert.AreEqual(width, image.resolvedStyle.width, 0.5f, "Shown at its own width, not shrunk.");
            Assert.AreEqual(height, image.resolvedStyle.height, 0.5f);
            Assert.Less(scroll.layout.height, available.y * ImageLightbox.imageHeightFraction + 1f,
                "The scroller is capped to the image share of the panel.");
            Assert.Greater(image.layout.height, scroll.layout.height, "So the picture scrolls.");
        }

        [UnityTest]
        public IEnumerator SlightlyTallImage_KeepsFitting()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            yield return null;

            // 1.2 is under the registry's 1.25 threshold.
            var lightbox = new ImageLightbox { src = Image(1000, 1200), caption = "portrait.png" };
            panel.Add(lightbox);
            yield return null;
            yield return null;

            Assert.IsFalse(lightbox.isTall);
            var image = lightbox.Q(className: ImageLightbox.imageUssClassName);
            Assert.Less(image.resolvedStyle.height, panel.layout.height * ImageLightbox.imageHeightFraction + 1f);
        }

        static void Key(VisualElement target, KeyCode code)
        {
            using var evt = KeyDownEvent.GetPooled('\0', code, EventModifiers.None);
            evt.target = target;
            target.SendEvent(evt);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsOnTokensAndCarriesNoLiterals()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = Regex.Replace(File.ReadAllText(fullPath), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            var root = RuleBody(uss, @"\.appui-imagelightbox");
            StringAssert.Contains("var(--appui-alias-surface-raised)", root,
                "The viewer sits on the raised surface token.");
            StringAssert.Contains("var(--appui-sizing-250)", root,
                "Its padding comes off the density-aware sizing ramp.");

            var caption = RuleBody(uss, @"\.appui-imagelightbox__caption");
            StringAssert.Contains("var(--appui-alias-text-secondary)", caption,
                "The caption is quiet rather than a heading.");

            Assert.IsFalse(uss.Contains("vw"), "UI Toolkit has no viewport unit; the C# measures instead.");
            Assert.IsFalse(uss.Contains("vh"), "UI Toolkit has no viewport unit; the C# measures instead.");
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "No literal colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"rgba?\("), "No rgb()/rgba() colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @":\s*[0-9]+(\.[0-9]+)?px"), "No magic pixel value.");
        }

        /// <summary>
        /// Anchored to the start of a line, so a bare class pattern cannot match a descendant rule
        /// further up the file and quietly assert against someone else's declarations.
        /// </summary>
        static string RuleBody(string uss, string selector)
        {
            var match = Regex.Match(uss, @"^" + selector + @"\s*\{(?<body>[^}]*)\}", RegexOptions.Multiline);
            Assert.IsTrue(match.Success, $"ImageLightbox.uss has no rule starting a line with {selector}.");
            return match.Groups["body"].Value;
        }
    }
}
