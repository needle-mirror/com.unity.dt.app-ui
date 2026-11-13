using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatStreamingIndicator))]
    class ChatStreamingIndicatorTests : VisualElementTests<ChatStreamingIndicator>
    {
        const string k_Uss = "ChatStreamingIndicator.uss";

        protected override string mainUssClassName => ChatStreamingIndicator.ussClassName;

        // Handed over to ChatStreamingIndicatorSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Streaming", (ctx) => new ChatStreamingIndicator { verbIndex = 0 });
                yield return new Story("WithStats", (ctx) => new ChatStreamingIndicator
                {
                    verbIndex = 0,
                    stats = "2 tools • 1.2k tokens",
                });
                yield return new Story("Completed", (ctx) => new ChatStreamingIndicator
                {
                    verbIndex = 0,
                    durationMs = 6400,
                    completed = true,
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ChatStreamingIndicator />",
            @"<appui:ChatStreamingIndicator completed=""true"" duration-ms=""90000"" />",
            @"<appui:ChatStreamingIndicator stats=""2 tools"" verb-index=""3"" />",
        };

        [Test]
        public void Streaming_ReadsThePresentTenseAndAnimatesTheMark()
        {
            var indicator = new ChatStreamingIndicator { verbIndex = 0 };

            Assert.AreEqual("Thinking", indicator.label);
            Assert.AreEqual(UnityLogoVariant.Animated, indicator.logo.variant,
                "A live indicator must be the animated Unity mark, which owns the only animation driver.");
        }

        [Test]
        public void Completed_ReadsThePastTenseWithTheDurationAndStopsTheAnimation()
        {
            var indicator = new ChatStreamingIndicator { verbIndex = 0, durationMs = 6400, completed = true };

            Assert.AreEqual("Thought for 6s", indicator.label);
            Assert.AreEqual(UnityLogoVariant.Filled, indicator.logo.variant,
                "A completed indicator must settle on the solid mark.");
            Assert.IsTrue(indicator.ClassListContains(ChatStreamingIndicator.completedUssClassName));
        }

        /// <summary>
        /// The duration wording is the registry's formatDuration, which is not a plain second count:
        /// it breaks into hours and minutes and drops empty units.
        /// </summary>
        [Test]
        public void CompletedDuration_UsesTheRegistrysDurationFormat()
        {
            Assert.AreEqual("6s", ChatFormat.Duration(6000));
            Assert.AreEqual("1m 30s", ChatFormat.Duration(90000));
            Assert.AreEqual("1m", ChatFormat.Duration(60000));
            Assert.AreEqual("1h 1s", ChatFormat.Duration(3601000));
            Assert.AreEqual("0s", ChatFormat.Duration(0));
        }

        [Test]
        public void EveryVerbPair_IsDistinctAndUsable()
        {
            var seen = new HashSet<string>();
            for (var i = 0; i < ChatStreamingIndicator.verbs.Length; i++)
            {
                var indicator = new ChatStreamingIndicator { verbIndex = i };
                Assert.IsNotEmpty(indicator.label);
                Assert.IsTrue(seen.Add(indicator.label), $"Verb index {i} repeats an earlier present tense.");
            }
        }

        [Test]
        public void VerbIndex_WrapsInsteadOfThrowing()
        {
            var indicator = new ChatStreamingIndicator { verbIndex = ChatStreamingIndicator.verbs.Length };
            Assert.AreEqual(0, indicator.verbIndex);

            indicator.verbIndex = -1;
            Assert.AreEqual(ChatStreamingIndicator.verbs.Length - 1, indicator.verbIndex);
        }

        /// <summary>
        /// Stats are streaming-only detail. Once the turn is over the line is the past tense and the
        /// duration, so leftover stats must not trail it.
        /// </summary>
        [Test]
        public void Stats_ShowWhileStreamingAndDisappearOnCompletion()
        {
            var indicator = new ChatStreamingIndicator { verbIndex = 0, stats = "2 tools" };
            Assert.IsFalse(indicator.Q("appui-chat-streaming-indicator__stats").ClassListContains("unity-hidden"));

            indicator.completed = true;
            Assert.IsTrue(indicator.Q("appui-chat-streaming-indicator__stats").ClassListContains("unity-hidden"),
                "A completed indicator must drop its streaming stats.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        /// <summary>
        /// A host that rebuilds its message list mid-turn destroys and recreates the indicator. A
        /// construction-time origin would restart the count from zero each time, so an elapsed
        /// measured from the message's own stamp is the whole point of the property.
        /// </summary>
        [Test]
        public void StartTime_MeasuresElapsedFromTheGivenStampNotConstruction()
        {
            var fresh = new ChatStreamingIndicator();
            Assert.Less(fresh.effectiveDurationMs, 1000f,
                "With no start time, a just-built indicator has barely any elapsed time.");

            var resumed = new ChatStreamingIndicator
            {
                startTime = System.DateTime.UtcNow.AddSeconds(-30),
            };
            Assert.Greater(resumed.effectiveDurationMs, 25000f,
                "A rebuilt indicator must carry the elapsed time of the turn, not of itself.");
        }

        /// <summary>
        /// The verb sweeps while the turn runs and settles once it is over; the stats never sweep,
        /// because they are figures to read rather than a progress cue.
        /// </summary>
        [Test]
        public void Verb_ShimmersWhileRunningAndSettlesOnCompletion()
        {
            var indicator = new ChatStreamingIndicator { stats = "12 tools" };
            var verb = indicator.Q<Shimmer>(className: ChatStreamingIndicator.labelUssClassName);

            Assert.IsNotNull(verb, "The verb must be a Shimmer so it can sweep while the turn runs.");
            Assert.IsTrue(verb.active);

            indicator.completed = true;
            Assert.IsFalse(verb.active, "A finished verb must not keep sweeping.");
        }

        /// <summary>
        /// The registry's <c>icon</c> replaces the product mark in both states.
        /// </summary>
        [Test]
        public void Icon_ReplacesTheLogoInBothStates_AndClearingRestoresIt()
        {
            var indicator = new ChatStreamingIndicator { verbIndex = 0 };
            var mark = new Icon { iconName = "sparkle" };
            indicator.icon = mark;

            Assert.AreSame(indicator, mark.hierarchy.parent);
            Assert.AreEqual(0, indicator.hierarchy.IndexOf(mark), "The mark stands before the verb.");
            Assert.IsTrue(mark.ClassListContains(ChatStreamingIndicator.iconUssClassName));
            Assert.IsTrue(indicator.logo.ClassListContains(Styles.hiddenUssClassName), "The custom mark replaces the logo.");

            indicator.completed = true;
            Assert.AreSame(indicator, mark.hierarchy.parent, "The custom mark stays once the turn is over.");
            Assert.IsTrue(indicator.logo.ClassListContains(Styles.hiddenUssClassName));

            indicator.icon = null;
            Assert.IsNull(mark.hierarchy.parent);
            Assert.IsFalse(mark.ClassListContains(ChatStreamingIndicator.iconUssClassName));
            Assert.IsFalse(indicator.logo.ClassListContains(Styles.hiddenUssClassName), "Clearing the mark brings the logo back.");
        }

        /// <summary>
        /// The registry's <c>renderVerb</c> replaces the sweep rather than wrapping it: both paint the
        /// text, and two would fight. It renders the live verb only; the completed line stays plain.
        /// </summary>
        [Test]
        public void RenderVerb_ReplacesTheSweepWhileStreamingOnly()
        {
            var indicator = new ChatStreamingIndicator { verbIndex = 6 };
            var shimmer = indicator.Q<Shimmer>(className: ChatStreamingIndicator.labelUssClassName);
            string seen = null;
            indicator.renderVerb = verb =>
            {
                seen = verb;
                return new Text(verb.ToUpperInvariant()) { name = "custom-verb" };
            };

            Assert.AreEqual("Considering", seen, "The renderer is handed the live verb.");
            var custom = indicator.Q<Text>("custom-verb");
            Assert.IsNotNull(custom, "The rendered verb must be in the indicator.");
            Assert.AreEqual(1, indicator.hierarchy.IndexOf(custom), "The rendered verb takes the sweep's place.");
            Assert.IsTrue(shimmer.ClassListContains(Styles.hiddenUssClassName), "The sweep must not paint the verb too.");
            Assert.IsFalse(shimmer.active, "A hidden sweep must not keep running its driver.");

            indicator.completed = true;
            Assert.IsNull(indicator.Q<Text>("custom-verb"), "The completed line is plain, not rendered.");
            Assert.IsFalse(shimmer.ClassListContains(Styles.hiddenUssClassName));
            StringAssert.StartsWith("Considered for ", indicator.label);

            indicator.completed = false;
            indicator.renderVerb = null;
            Assert.IsNull(indicator.Q<Text>("custom-verb"));
            Assert.IsFalse(shimmer.ClassListContains(Styles.hiddenUssClassName), "Clearing the renderer restores the sweep.");
            Assert.IsTrue(shimmer.active);
        }

        /// <summary>
        /// The renderer builds an element, so it runs when the verb changes, not on every refresh: a
        /// stats tick would otherwise rebuild the verb many times a second.
        /// </summary>
        [Test]
        public void RenderVerb_RunsOnlyWhenTheVerbChanges()
        {
            var indicator = new ChatStreamingIndicator { verbIndex = 0 };
            var calls = 0;
            indicator.renderVerb = verb =>
            {
                calls++;
                return new Text(verb);
            };
            Assert.AreEqual(1, calls);

            indicator.stats = "2 tools";
            indicator.stats = "3 tools";
            indicator.durationMs = 1200;
            Assert.AreEqual(1, calls, "Nothing the verb depends on changed.");

            indicator.verbIndex = 1;
            Assert.AreEqual(2, calls, "A new verb is rendered anew.");
        }

        /// <summary>
        /// The registry picks the override by the renderer's presence, so a renderer that returns null
        /// renders nothing: the sweep stays hidden and still.
        /// </summary>
        [Test]
        public void RenderVerb_ReturningNull_RendersNoVerb()
        {
            var indicator = new ChatStreamingIndicator { verbIndex = 0 };
            var shimmer = indicator.Q<Shimmer>(className: ChatStreamingIndicator.labelUssClassName);
            var calls = 0;
            indicator.renderVerb = _ =>
            {
                calls++;
                return null;
            };

            Assert.IsTrue(shimmer.ClassListContains(Styles.hiddenUssClassName), "A null render must not bring the sweep back.");
            Assert.IsFalse(shimmer.active);
            indicator.stats = "2 tools";
            Assert.AreEqual(1, calls, "A null render is still a render of this verb.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_ACustomMark_StandsFirstWithNoLeadingGap()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            StringAssert.Contains("margin-left: 0",
                ChatTestUtils.RuleBody(uss, @"^\.appui-chat-streaming-indicator > \.appui-chat-streaming-indicator__icon"));
            StringAssert.Contains("margin-right: 0",
                ChatTestUtils.RuleBody(uss, @"^\.appui--rtl \.appui-chat-streaming-indicator > \.appui-chat-streaming-indicator__icon"));
        }
    }
}
