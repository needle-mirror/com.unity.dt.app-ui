using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Counter-checks the story harness itself, on a probe element that belongs to no component.
    /// </summary>
    /// <remarks>
    /// The snapshot path asserts nothing, and in the Editor it does not even write a file, so the
    /// two properties every story depends on -- a capture host with a real size, and a story that
    /// can act after attachment -- would otherwise be covered by nothing at all.
    /// </remarks>
    [TestFixture]
    class SnapshotHarnessTests : VisualElementTests<SnapshotHarnessTests.Probe>
    {
        /// <summary>
        /// A component-free stand-in, so this fixture's stories cannot be confused with a real
        /// component's captures.
        /// </summary>
        internal class Probe : VisualElement { }

        protected override string mainUssClassName => null;

        protected override bool uxmlConstructable => false;

        // The two stories below exist to be asserted on, not photographed.
        protected override bool capturesSnapshots => false;

        static bool s_AttachedWhenStoryResumed;

        protected override IEnumerable<Story> stories
        {
            get
            {
                // A child that only has a size if its parent has one: absolute positioning
                // contributes no layout, so this measures the capture host rather than itself.
                yield return new Story("Stretched", (ctx) => new Probe
                {
                    style =
                    {
                        position = Position.Absolute,
                        top = 0,
                        bottom = 0,
                        left = 0,
                        right = 0,
                    }
                });

                yield return new Story("Deferred", Deferred);
            }
        }

        static IEnumerator Deferred(StoryContext ctx)
        {
            var probe = new Probe { style = { width = 40, height = 40 } };
            ctx.container.Add(probe);
            s_AttachedWhenStoryResumed = false;

            yield return null;

            s_AttachedWhenStoryResumed = probe.panel != null;
        }

        [UnityTest]
        public IEnumerator CaptureHost_FillsThePanel()
        {
            yield return RunStory("Stretched", ctx =>
            {
                var host = ctx.container;
                Assert.Greater(host.resolvedStyle.height, 0f,
                    "The capture host has no height, so an absolutely positioned story is framed "
                    + "against nothing.");
                Assert.AreEqual(ctx.panel.resolvedStyle.height, host.resolvedStyle.height, 0.5f,
                    "The capture host must fill the themed panel.");

                var stretched = host.ElementAt(0);
                Assert.Greater(stretched.resolvedStyle.height, 0f,
                    "A child pinned to the capture host's edges must resolve to a real height.");
            });
        }

        [UnityTest]
        public IEnumerator CoroutineStory_ResumesAfterAttachment()
        {
            yield return RunStory("Deferred", _ =>
            {
                Assert.IsTrue(s_AttachedWhenStoryResumed,
                    "A story must be able to resume once its element is in a panel; that is what "
                    + "lets it do layout-dependent setup, open an overlay or take focus.");
            });
        }
    }
}
