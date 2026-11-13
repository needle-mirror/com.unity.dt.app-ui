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
    [TestOf(typeof(ZoomTriggerButton))]
    class ZoomTriggerButtonTests : VisualElementTests<ZoomTriggerButton>
    {
        protected override string mainUssClassName => ZoomTriggerButton.ussClassName;

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/ZoomTriggerButton.uss";

        /// <remarks>
        /// There is deliberately no bare <c>Default</c>: the base rule sets <c>opacity: 0</c>, so a
        /// standalone resting control captures nothing at all. The three stories below are the
        /// three ways the control is actually on screen -- the host reveals it, the platform has no
        /// hover so it never hides, or it takes keyboard focus.
        /// </remarks>
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Revealed",
                    (ctx) => new ZoomTriggerButton("cliff-face-01.png") { revealed = true });
                yield return new Story("AlwaysVisible",
                    (ctx) => new ZoomTriggerButton("cliff-face-01.png") { alwaysVisible = true });
                yield return new Story("Focused", Focused);
            }
        }

        /// <summary>
        /// The control revealed by its own <c>:focus</c>, which only a mounted element can take.
        /// </summary>
        static IEnumerator Focused(StoryContext ctx)
        {
            var button = new ZoomTriggerButton("cliff-face-01.png");
            ctx.container.Add(button);
            yield return null;

            button.Focus();

            // The reveal is a 125ms opacity transition, which focus merely starts.
            yield return WaitUntil(() => button.resolvedStyle.opacity >= 0.999f);
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ZoomTriggerButton />",
            @"<appui:ZoomTriggerButton asset-name=""cliff-face-01.png"" always-visible=""true"" />",
        };

        [UnityTest]
        public IEnumerator EveryStory_IsActuallyOnScreen()
        {
            foreach (var name in new[] { "Revealed", "AlwaysVisible", "Focused" })
            {
                var storyName = name;
                yield return RunStory(storyName, ctx =>
                {
                    var button = ctx.container.Q<ZoomTriggerButton>();
                    Assert.IsNotNull(button);
                    Assert.AreEqual(1f, button.resolvedStyle.opacity, 0.001f,
                        $"`{storyName}` captures nothing: the base rule rests at opacity 0 and this "
                        + "story does not lift it.");
                });
            }
        }

        [Test]
        public void NoBareDefaultStory_BecauseItWouldCaptureNothing()
        {
            CollectionAssert.DoesNotContain(stories.Select(story => story.name), "Default",
                "A standalone resting ZoomTriggerButton is invisible by design.");
        }

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var button = new ZoomTriggerButton();

            Assert.AreEqual(ZoomTriggerButton.zoomIconName, button.icon,
                "The zoom affordance is one glyph everywhere.");
            Assert.IsTrue(button.quiet, "The control sits on top of a picture; it must not paint a chrome.");
            Assert.AreEqual(Size.Sm, button.size);
            Assert.IsFalse(button.revealed, "The control rests hidden so a dense grid stays calm.");
        }

        [Test]
        public void AssetName_BecomesTheAccessibleWording()
        {
            var button = new ZoomTriggerButton("cliff-face-01.png");
            Assert.AreEqual("View larger preview of cliff-face-01.png", button.tooltip,
                "The accessible-name wording must live in exactly one place.");

            button.assetName = null;
            Assert.IsTrue(string.IsNullOrEmpty(button.tooltip),
                "With nothing to name, the control must not offer a half-written sentence.");
        }

        [Test]
        public void Revealed_TogglesItsClass()
        {
            var button = new ZoomTriggerButton();
            Assert.IsFalse(button.ClassListContains(ZoomTriggerButton.revealedUssClassName));

            button.revealed = true;
            Assert.IsTrue(button.ClassListContains(ZoomTriggerButton.revealedUssClassName));

            button.revealed = false;
            Assert.IsFalse(button.ClassListContains(ZoomTriggerButton.revealedUssClassName),
                "A stale reveal class leaves the control on screen for good.");
        }

        [Test]
        public void AlwaysVisible_TogglesItsClass()
        {
            var button = new ZoomTriggerButton { alwaysVisible = true };
            Assert.IsTrue(button.ClassListContains(ZoomTriggerButton.alwaysVisibleUssClassName));

            button.alwaysVisible = false;
            Assert.IsFalse(button.ClassListContains(ZoomTriggerButton.alwaysVisibleUssClassName));
        }

        [UnityTest]
        public IEnumerator PointerDown_DoesNotReachTheHostUnderneath()
        {
            var host = new VisualElement();
            var button = new ZoomTriggerButton("cliff-face-01.png");
            host.Add(button);
            m_TestUI.rootVisualElement.Add(host);
            yield return null;

            var reachedHost = false;
            host.RegisterCallback<PointerDownEvent>(_ => reachedHost = true);

            using (var evt = PointerDownEvent.GetPooled())
            {
                evt.target = button;
                button.SendEvent(evt);
            }

            Assert.IsFalse(reachedHost,
                "The host commonly selects on pointer down, and enlarging is not selecting.");
        }

        [UnityTest]
        public IEnumerator ActivationKeys_DoNotReachTheHostUnderneath()
        {
            var host = new VisualElement();
            var button = new ZoomTriggerButton("cliff-face-01.png");
            host.Add(button);
            m_TestUI.rootVisualElement.Add(host);
            yield return null;

            var reachedHost = 0;
            host.RegisterCallback<KeyDownEvent>(_ => reachedHost++);

            foreach (var key in new[] { KeyCode.Return, KeyCode.Space })
            {
                using var evt = KeyDownEvent.GetPooled('\0', key, EventModifiers.None);
                evt.target = button;
                button.SendEvent(evt);
            }

            Assert.AreEqual(0, reachedHost,
                "Enter/Space on the control means enlarge, and must not also mean select.");
        }

        /// <summary>
        /// The canary for the two tests above: it proves the events really are being dispatched, so
        /// their "did not reach the host" assertions cannot pass for want of a dispatcher.
        /// </summary>
        [UnityTest]
        public IEnumerator UnrelatedKeys_StillReachTheHost()
        {
            var host = new VisualElement();
            var button = new ZoomTriggerButton("cliff-face-01.png");
            host.Add(button);
            m_TestUI.rootVisualElement.Add(host);
            yield return null;

            var reachedHost = 0;
            host.RegisterCallback<KeyDownEvent>(_ => reachedHost++);

            using (var evt = KeyDownEvent.GetPooled('\0', KeyCode.DownArrow, EventModifiers.None))
            {
                evt.target = button;
                button.SendEvent(evt);
            }

            Assert.AreEqual(1, reachedHost,
                "Only the activation keys are swallowed; arrow-key navigation still belongs to the host.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_HidesAtRestAndRevealsOnEveryRoute()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = Regex.Replace(File.ReadAllText(fullPath), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            var rest = Regex.Match(uss, @"^\.appui-zoomtriggerbutton\s*\{(?<body>[^}]*)\}", RegexOptions.Multiline);
            Assert.IsTrue(rest.Success, "ZoomTriggerButton.uss has no base rule.");
            StringAssert.Contains("opacity: 0", rest.Groups["body"].Value,
                "The control must rest invisible.");

            // USS has no :focus-within, so the reveal has to name the control's own :focus explicitly.
            var reveal = Regex.Match(uss,
                @"^(\.appui-zoomtriggerbutton(--revealed|--always-visible|:focus|:hover),?\s*)+\{(?<body>[^}]*)\}",
                RegexOptions.Multiline);
            Assert.IsTrue(reveal.Success, "ZoomTriggerButton.uss has no reveal rule.");
            StringAssert.Contains("opacity: 1", reveal.Groups["body"].Value);
            foreach (var route in new[] { "--revealed", "--always-visible", ":focus" })
            {
                StringAssert.Contains(".appui-zoomtriggerbutton" + route, reveal.Value,
                    $"The reveal must include the `{route}` route.");
            }

            Assert.IsFalse(uss.Contains(":focus-within"),
                ":focus-within is not a USS selector; the reveal must be keyed off the control's own focus.");
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "No literal colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @":\s*[0-9]+(\.[0-9]+)?px"), "No magic pixel value.");
        }
    }
}
