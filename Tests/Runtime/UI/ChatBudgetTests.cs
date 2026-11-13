using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatBudget))]
    class ChatBudgetTests : VisualElementTests<ChatBudget>
    {
        const string k_Uss = "ChatBudget.uss";

        protected override string mainUssClassName => ChatBudget.ussClassName;

        // Handed over to ChatBudgetSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Partial", (ctx) => new ChatBudget { spent = 120, total = 200 });
                yield return new Story("Exhausted", (ctx) => new ChatBudget { spent = 200, total = 200 });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ChatBudget />",
            @"<appui:ChatBudget spent=""200"" total=""200"" unit=""credits"" />",
        };

        [Test]
        public void Progress_TracksTheRatioAndClampsAtFull()
        {
            var budget = new ChatBudget { total = 200, spent = 50 };
            Assert.AreEqual(0.25f, budget.progress, 0.001f);
            Assert.AreEqual(0.25f, budget.bar.value, 0.001f, "The bar must follow the ratio.");

            budget.spent = 400;
            Assert.AreEqual(1f, budget.progress, 0.001f, "Overspending must not push the bar past full.");
        }

        [Test]
        public void ZeroTotal_ReadsAsEmptyRatherThanDividingByZero()
        {
            var budget = new ChatBudget { total = 0, spent = 10 };
            Assert.AreEqual(0f, budget.progress, 0.001f);
            Assert.IsFalse(budget.exhausted);
        }

        [Test]
        public void Reading_PairsTheAmountsWithTheUnit()
        {
            var budget = new ChatBudget { spent = 120, total = 200 };
            Assert.AreEqual("120 / 200 pts", budget.reading);

            budget.unit = "credits";
            Assert.AreEqual("120 / 200 credits", budget.reading);
        }

        /// <summary>
        /// An exhausted budget has to be unmistakable, so it carries its own class for the destructive
        /// styling rather than relying on the bar happening to be full.
        /// </summary>
        [Test]
        public void Exhausted_MarksItselfAndClearsAgain()
        {
            var budget = new ChatBudget { spent = 200, total = 200 };
            Assert.IsTrue(budget.exhausted);
            Assert.IsTrue(budget.ClassListContains(ChatBudget.exhaustedUssClassName));

            budget.spent = 199;
            Assert.IsFalse(budget.ClassListContains(ChatBudget.exhaustedUssClassName),
                "The exhausted class must come off, not accumulate.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        /// <summary>
        /// The bar colour goes through Progress's --progress-color, which is a colour custom property
        /// and therefore safe. A dimension published the same way would silently resolve to zero.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Bar_TakesItsColourFromTheAccentAndDestructiveTokens()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            StringAssert.Contains(
                "--progress-color: var(--appui-alias-actions-accent-background-color-default)",
                ChatTestUtils.RuleBody(uss, @"\.appui-chat-budget__bar"));
            StringAssert.Contains(
                "--progress-color: var(--appui-alias-semantic-destructive)",
                ChatTestUtils.RuleBody(uss, @"\.appui-chat-budget--exhausted > \.appui-chat-budget__bar"));
        }
    }

    [TestFixture]
    [TestOf(typeof(LoadingDots))]
    class LoadingDotsTests : VisualElementTests<LoadingDots>
    {
        // Handed over to LoadingDotsSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override string mainUssClassName => LoadingDots.ussClassName;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new LoadingDots());
                yield return new Story("Static", (ctx) => new LoadingDots { animated = false });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:LoadingDots />",
            @"<appui:LoadingDots animated=""false"" />",
        };

        [Test]
        public void Default_IsAnimatedAndHasThreeDots()
        {
            var dots = new LoadingDots();
            Assert.IsTrue(dots.animated);
            Assert.AreEqual(LoadingDots.dotCount, dots.hierarchy.childCount);
        }

        /// <summary>
        /// Pinned to the literal values of the web implementation, so retuning the pulse fails here
        /// rather than drifting silently.
        /// </summary>
        [Test]
        public void PulseConstants_MatchTheWebAnimation()
        {
            Assert.AreEqual(2000f, LoadingDots.pulsePeriodMs, 0.001f);
            Assert.AreEqual(200f, LoadingDots.dotDelayMs, 0.001f, "The dots trail by 0 / 200 / 400ms.");
            Assert.AreEqual(0.5f, LoadingDots.minPulseOpacity, 0.001f);
        }

        /// <summary>
        /// The stagger is the whole point: three dots pulsing in lockstep read as a blink, not a wave.
        /// </summary>
        [Test]
        public void Dots_AreOffsetFromOneAnother()
        {
            const float t = 0.35f;
            var first = LoadingDots.ComputeDotOpacity(t, 0);
            var second = LoadingDots.ComputeDotOpacity(t, 1);
            var third = LoadingDots.ComputeDotOpacity(t, 2);

            Assert.Greater(UnityEngine.Mathf.Abs(first - second), 0.01f, "Dot 1 must lag dot 0.");
            Assert.Greater(UnityEngine.Mathf.Abs(second - third), 0.01f, "Dot 2 must lag dot 1.");

            var lagSeconds = LoadingDots.dotDelayMs / 1000f;
            Assert.AreEqual(first, LoadingDots.ComputeDotOpacity(t + lagSeconds, 1), 0.001f,
                "Dot 1 must be dot 0 delayed by exactly one step.");
        }

        [Test]
        public void PulseEnvelope_MatchesTheWebAnimatePulse()
        {
            var period = LoadingDots.pulsePeriodMs / 1000f;
            Assert.AreEqual(1f, LoadingDots.ComputeDotOpacity(0f, 0), 0.001f);
            Assert.AreEqual(LoadingDots.minPulseOpacity, LoadingDots.ComputeDotOpacity(period * 0.5f, 0), 0.001f);
            Assert.AreEqual(1f, LoadingDots.ComputeDotOpacity(period, 0), 0.001f);
        }

        [UnityTest]
        public IEnumerator Ticking_WritesTheOpacityOntoEveryDot()
        {
            var dots = new LoadingDots();
            yield return ChatTestUtils.Mount(m_TestUI, dots);

            var previous = TimeUtils.timeOverride;
            try
            {
                TimeUtils.timeOverride = LoadingDots.pulsePeriodMs / 2000f;
                dots.TickPulse();
                yield return null;

                for (var i = 0; i < LoadingDots.dotCount; i++)
                {
                    Assert.AreEqual(LoadingDots.ComputeDotOpacity(TimeUtils.GetCurrentTime(), i),
                        dots.hierarchy[i].resolvedStyle.opacity, 0.01f, $"Dot {i} was not driven.");
                }
            }
            finally
            {
                TimeUtils.timeOverride = previous;
            }
        }

        [UnityTest]
        public IEnumerator TurningTheAnimationOff_StopsTheDriverAndRestoresFullOpacity()
        {
            var dots = new LoadingDots();
            yield return ChatTestUtils.Mount(m_TestUI, dots);
            Assert.IsTrue(dots.isPulsing);

            var previous = TimeUtils.timeOverride;
            try
            {
                TimeUtils.timeOverride = LoadingDots.pulsePeriodMs / 2000f;
                dots.TickPulse();
                dots.animated = false;
                yield return null;

                Assert.IsFalse(dots.isPulsing);
                for (var i = 0; i < LoadingDots.dotCount; i++)
                    Assert.AreEqual(1f, dots.hierarchy[i].resolvedStyle.opacity, 0.01f);
            }
            finally
            {
                TimeUtils.timeOverride = previous;
            }
        }

        [UnityTest]
        public IEnumerator DetachingFromThePanel_StopsTheDriver()
        {
            var dots = new LoadingDots();
            yield return ChatTestUtils.Mount(m_TestUI, dots);
            Assert.IsTrue(dots.isPulsing);

            dots.RemoveFromHierarchy();
            yield return null;
            Assert.IsFalse(dots.isPulsing, "A detached element must not keep a scheduled item alive.");
        }
    }

    [TestFixture]
    [TestOf(typeof(CreditsPill))]
    class CreditsPillTests : VisualElementTests<CreditsPill>
    {
        // Handed over to CreditsPillSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override string mainUssClassName => CreditsPill.ussClassName;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Resolved", (ctx) => new CreditsPill { credits = 51200 });
                yield return new Story("Loading", (ctx) => new CreditsPill { loading = true });
                yield return new Story("Error", (ctx) => new CreditsPill { error = true });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:CreditsPill />",
            @"<appui:CreditsPill credits=""51200"" prefix=""+"" />",
            @"<appui:CreditsPill loading=""true"" />",
            @"<appui:CreditsPill credits=""51200"" tooltip-label=""Resets on the 1st"" />",
        };

        /// <summary>
        /// The compact rendering is the registry's formatCompact, which is not a plain thousands
        /// divide: it keeps one decimal below 100 and drops a trailing .0.
        /// </summary>
        [Test]
        public void CompactRendering_MatchesTheRegistry()
        {
            Assert.AreEqual("999", ChatFormat.Compact(999));
            Assert.AreEqual("1k", ChatFormat.Compact(1000));
            Assert.AreEqual("1.5k", ChatFormat.Compact(1500));
            Assert.AreEqual("51.2k", ChatFormat.Compact(51200));
            Assert.AreEqual("120k", ChatFormat.Compact(120000));
            Assert.AreEqual("2.4M", ChatFormat.Compact(2400000));
        }

        [Test]
        public void Resolved_ShowsTheCompactValueAndTheExactOneInTheTooltip()
        {
            var pill = new CreditsPill { credits = 51200 };
            var value = pill.Q<LocalizedTextElement>(CreditsPill.valueUssClassName);

            Assert.AreEqual("51.2k Credits", value.text);
            StringAssert.Contains("51,200", pill.tooltip,
                "The tooltip is where the exact balance lives.");
        }

        /// <summary>
        /// Loading wins over error: there is nothing to report about a request that has not finished,
        /// and there is no exact value to reveal, so the tooltip stays empty too.
        /// </summary>
        [Test]
        public void Loading_TakesPrecedenceOverErrorAndSuppressesTheTooltip()
        {
            var pill = new CreditsPill { credits = 10, loading = true, error = true };

            Assert.IsFalse(pill.Q(CreditsPill.dotsUssClassName).ClassListContains("unity-hidden"));
            Assert.IsTrue(pill.Q(CreditsPill.warningUssClassName).ClassListContains("unity-hidden"));
            Assert.IsTrue(pill.Q(CreditsPill.valueUssClassName).ClassListContains("unity-hidden"));
            Assert.IsTrue(string.IsNullOrEmpty(pill.tooltip));
        }

        [Test]
        public void Error_ShowsTheWarningGlyphAndTheReasonInTheTooltip()
        {
            var pill = new CreditsPill { error = true, errorMessage = "No network" };

            Assert.IsFalse(pill.Q(CreditsPill.warningUssClassName).ClassListContains("unity-hidden"));
            Assert.IsTrue(pill.Q(CreditsPill.valueUssClassName).ClassListContains("unity-hidden"));
            Assert.AreEqual("No network", pill.tooltip);
        }

        [Test]
        public void Prefix_IsGluedToTheNumberInBothRenderings()
        {
            var pill = new CreditsPill { credits = 1500, prefix = "+" };
            var value = pill.Q<LocalizedTextElement>(CreditsPill.valueUssClassName);

            Assert.AreEqual("+1.5k Credits", value.text);
            StringAssert.StartsWith("+1,500", pill.tooltip);
        }

        /// <summary>
        /// The registry's <c>tooltip</c> prop replaces the exact-balance tooltip, and a balance update
        /// must not write over it.
        /// </summary>
        [Test]
        public void TooltipLabel_ReplacesTheBalanceAndSurvivesAnUpdate()
        {
            var pill = new CreditsPill { credits = 51200, tooltipLabel = "Resets on the 1st" };
            Assert.AreEqual("Resets on the 1st", pill.tooltip);

            pill.credits = 50000;
            pill.label = "Points";
            pill.prefix = "+";
            Assert.AreEqual("Resets on the 1st", pill.tooltip, "A consumer's tooltip must win over the default.");

            pill.tooltipLabel = null;
            StringAssert.Contains("50,000", pill.tooltip, "Clearing the override brings the exact balance back.");
        }

        /// <summary>
        /// A value written straight to <see cref="VisualElement.tooltip"/> is the consumer's too: the
        /// next refresh adopts it instead of writing over it, and clearing it restores the balance.
        /// </summary>
        [Test]
        public void ADirectTooltipWrite_SurvivesABalanceUpdate()
        {
            var pill = new CreditsPill { credits = 51200 };
            pill.tooltip = "Resets on the 1st";

            pill.credits = 50000;
            Assert.AreEqual("Resets on the 1st", pill.tooltip, "A consumer-set tooltip must win over the default.");
            Assert.AreEqual("Resets on the 1st", pill.tooltipLabel, "The write is adopted as the override.");

            pill.tooltip = null;
            pill.credits = 1500;
            StringAssert.Contains("1,500", pill.tooltip, "Clearing it brings the exact balance back.");
        }

        /// <summary>
        /// Mixing the two ways in: the latest assignment wins, so a direct write still pending does not
        /// undo a later <see cref="CreditsPill.tooltipLabel"/>, whether it sets or clears.
        /// </summary>
        [Test]
        public void TooltipLabel_SetAfterADirectWrite_Wins()
        {
            var pill = new CreditsPill { credits = 51200 };
            pill.tooltip = "Old";
            pill.tooltipLabel = "New";
            Assert.AreEqual("New", pill.tooltip);
            pill.credits = 50000;
            Assert.AreEqual("New", pill.tooltip, "A later balance update must not bring the older write back.");

            pill.tooltip = "Old";
            pill.tooltipLabel = null;
            StringAssert.Contains("50,000", pill.tooltip, "Clearing after a direct write restores the balance.");
        }

        /// <summary>
        /// The registry's precedence: the error message still wins over a consumer tooltip, loading shows
        /// none, and once either clears, the consumer's tooltip is back.
        /// </summary>
        [Test]
        public void TooltipLabel_KeepsTheRegistrysPrecedence()
        {
            var pill = new CreditsPill { credits = 51200, errorMessage = "No network", tooltipLabel = "Resets on the 1st" };

            pill.error = true;
            Assert.AreEqual("No network", pill.tooltip, "An error still explains itself.");
            pill.error = false;
            Assert.AreEqual("Resets on the 1st", pill.tooltip);

            pill.loading = true;
            Assert.IsTrue(string.IsNullOrEmpty(pill.tooltip), "There is nothing to reveal while loading.");
            pill.loading = false;
            Assert.AreEqual("Resets on the 1st", pill.tooltip);
        }
    }

    [TestFixture]
    [TestOf(typeof(BudgetIndicator))]
    class BudgetIndicatorTests : VisualElementTests<BudgetIndicator>
    {
        // Handed over to BudgetIndicatorSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override string mainUssClassName => BudgetIndicator.ussClassName;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Partial", (ctx) => new BudgetIndicator { spent = 40, total = 200 });
                yield return new Story("Exhausted", (ctx) => new BudgetIndicator { spent = 200, total = 200 });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:BudgetIndicator />",
            @"<appui:BudgetIndicator spent=""200"" total=""200"" />",
        };

        [Test]
        public void Percent_RoundsAndTheRingFollows()
        {
            var indicator = new BudgetIndicator { total = 200, spent = 41 };
            Assert.AreEqual(21, indicator.percent);
            Assert.AreEqual(0.205f, indicator.ring.value, 0.001f);
        }

        /// <summary>
        /// A non-positive total is not a zero-percent budget, it is the absence of one. The registry
        /// renders nothing at all in that case.
        /// </summary>
        [Test]
        public void NoBudget_HidesTheWholeIndicator()
        {
            var indicator = new BudgetIndicator { total = 0 };
            Assert.IsTrue(indicator.ClassListContains("unity-hidden"));

            indicator.total = 100;
            Assert.IsFalse(indicator.ClassListContains("unity-hidden"));
        }

        [Test]
        public void Exhausted_MarksItselfAndClearsAgain()
        {
            var indicator = new BudgetIndicator { spent = 200, total = 200 };
            Assert.IsTrue(indicator.ClassListContains(BudgetIndicator.exhaustedUssClassName));

            indicator.spent = 100;
            Assert.IsFalse(indicator.ClassListContains(BudgetIndicator.exhaustedUssClassName));
        }

        [Test]
        public void Tooltip_ShowsTheFullReadingAndCanBeReplaced()
        {
            var indicator = new BudgetIndicator { spent = 40, total = 200 };
            Assert.AreEqual("40 / 200 pts", indicator.tooltip);

            indicator.tooltipLabel = "Almost out";
            Assert.AreEqual("Almost out", indicator.tooltip);
        }
    }
}
