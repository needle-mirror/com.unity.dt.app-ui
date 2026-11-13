using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatActivityTimeline))]
    class ChatActivityTimelineTests : VisualElementTests<ChatActivityTimeline>
    {
        const string k_Uss = "ChatActivityTimeline.uss";

        protected override string mainUssClassName => ChatActivityTimeline.ussClassName;

        // Handed over to ChatActivityTimelineSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        static ChatActivityPart[] SampleParts() => new[]
        {
            new ChatActivityPart(ChatActivityPartKind.Thinking, "The jump force lives on the controller.", elapsedMs: 800),
            new ChatActivityPart(ChatActivityPartKind.Tool, "Read PlayerController.cs", elapsedMs: 4200),
            new ChatActivityPart(ChatActivityPartKind.Tool, "Write PlayerController.cs", pending: true),
        };

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Collapsed", (ctx) => new ChatActivityTimeline { parts = SampleParts() });
                yield return new Story("Open", (ctx) => new ChatActivityTimeline { parts = SampleParts(), open = true });
                yield return new Story("Streaming", (ctx) => new ChatActivityTimeline
                {
                    parts = SampleParts(),
                    isStreaming = true,
                    open = true,
                    label = "Running tool",
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ChatActivityTimeline />",
            @"<appui:ChatActivityTimeline label=""Running tool"" is-streaming=""true"" open=""true"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var timeline = new ChatActivityTimeline();
            Assert.AreEqual(ChatActivityTimeline.defaultLabel, timeline.label);
            Assert.IsFalse(timeline.open, "The timeline is a footnote, so it starts collapsed.");
            Assert.IsFalse(timeline.isStreaming);
        }

        /// <summary>
        /// The timeline shows the caller's parts and nothing else. It used to append a synthetic
        /// "Done" row on finish; the registry dropped it, because a collapsed summary already says
        /// the activity is over and the extra row was a second, weaker way of saying so.
        /// </summary>
        [Test]
        public void FinishingTheTurn_AddsNoRowOfItsOwn()
        {
            var timeline = new ChatActivityTimeline { parts = SampleParts(), isStreaming = true };
            Assert.AreEqual(3, timeline.rowCount);

            timeline.isStreaming = false;
            Assert.AreEqual(3, timeline.rowCount,
                "A finished turn must show the caller's parts and nothing else.");
        }

        /// <summary>
        /// Closed, a finished timeline reports what it did rather than repeating the label.
        /// </summary>
        [Test]
        public void FinishedSummary_TalliesThoughtsAndTools()
        {
            var timeline = new ChatActivityTimeline { parts = SampleParts(), isStreaming = false };

            Assert.AreEqual(timeline.thoughtCount + timeline.toolCount, 3);
            var summary = timeline.SummaryText();
            if (timeline.thoughtCount > 0)
                StringAssert.Contains("thought", summary);
            if (timeline.toolCount > 0)
                StringAssert.Contains("tool", summary);
            StringAssert.DoesNotContain("…", summary, "A finished timeline is not still working.");
        }

        [Test]
        public void RunningSummary_ShowsTheLabelAndKeepsSweeping()
        {
            var timeline = new ChatActivityTimeline
            {
                parts = SampleParts(), label = "Thinking", isStreaming = true,
            };

            StringAssert.Contains("Thinking", timeline.SummaryText());
            Assert.IsFalse(timeline.isFinalized);

            var label = timeline.Q<Shimmer>(className: ChatActivityTimeline.summaryLabelUssClassName);
            Assert.IsNotNull(label, "The header must be a Shimmer so it can sweep while work runs.");
            Assert.IsTrue(label.active);
        }

        /// <summary>
        /// Once the answer's own prose has begun, the activity above it is finished even though the
        /// turn is not. Without this the timeline keeps counting while the reader is already
        /// reading the answer.
        /// </summary>
        [Test]
        public void IsFollowedByText_FinalizesWhileTheTurnIsStillStreaming()
        {
            var timeline = new ChatActivityTimeline { parts = SampleParts(), isStreaming = true };
            Assert.IsFalse(timeline.isFinalized);

            timeline.isFollowedByText = true;

            Assert.IsTrue(timeline.isFinalized,
                "Prose having started must finalize the activity above it.");
            var label = timeline.Q<Shimmer>(className: ChatActivityTimeline.summaryLabelUssClassName);
            Assert.IsFalse(label.active, "A finalized header must stop sweeping.");
        }

        /// <summary>
        /// While the activity runs the elapsed count comes from the wall clock, so a rebuilt
        /// element does not restart it; once finished it is the fixed sum of the parts.
        /// </summary>
        [Test]
        public void StartTime_DrivesTheElapsedCountUntilTheActivityFinishes()
        {
            var timeline = new ChatActivityTimeline
            {
                parts = SampleParts(),
                isStreaming = true,
                startTime = System.DateTime.UtcNow.AddSeconds(-30),
            };

            Assert.Greater(timeline.effectiveElapsedMs, 25000,
                "A running timeline counts from its start stamp.");

            timeline.isStreaming = false;
            Assert.AreEqual(timeline.totalElapsedMs, timeline.effectiveElapsedMs,
                "A finished timeline reports the sum of its parts, not a still-running clock.");
        }

        [Test]
        public void Rows_AreCollapsedByDefault()
        {
            Assert.IsFalse(new ChatActivityTimeline().open,
                "The registry's defaultOpen is false: the timeline opens closed.");
        }

        /// <summary>
        /// The connector must stop at the last row, otherwise the line dangles past the timeline.
        /// </summary>
        [Test]
        public void OnlyTheLastRow_DropsItsConnector()
        {
            var timeline = new ChatActivityTimeline { parts = SampleParts() };
            var rows = timeline.Q(ChatActivityTimeline.rowsUssClassName);

            for (var i = 0; i < rows.childCount; i++)
            {
                Assert.AreEqual(i == rows.childCount - 1,
                    rows[i].ClassListContains(ChatActivityTimeline.lastRowUssClassName),
                    $"Row {i} has the wrong last-row marker.");
            }
        }

        [Test]
        public void PendingToolCalls_SpinAndSettledOnesDoNot()
        {
            var timeline = new ChatActivityTimeline { parts = SampleParts(), isStreaming = true };
            var rows = timeline.Q(ChatActivityTimeline.rowsUssClassName);

            Assert.IsInstanceOf<Icon>(rows[1].Q(ChatActivityTimeline.rowGlyphUssClassName)[0],
                "A returned tool call is a wrench.");
            Assert.IsInstanceOf<CircularProgress>(rows[2].Q(ChatActivityTimeline.rowGlyphUssClassName)[0],
                "A pending tool call reuses the package spinner.");
        }

        [Test]
        public void TotalElapsed_SumsOnlyMeasuredParts()
        {
            var timeline = new ChatActivityTimeline { parts = SampleParts() };
            Assert.AreEqual(5000, timeline.totalElapsedMs,
                "The unmeasured pending part must contribute nothing.");
        }

        /// <summary>
        /// A sub-second duration reads "&lt;1s", never "0s" — the registry is explicit about that,
        /// and it is what stops a fast tool call from looking like it never ran.
        /// </summary>
        [Test]
        public void ElapsedBadge_CollapsesSubSecondDurations()
        {
            Assert.AreEqual("<1s", ChatActivityTimeline.FormatElapsed(1));
            Assert.AreEqual("<1s", ChatActivityTimeline.FormatElapsed(999));
            Assert.AreEqual("1s", ChatActivityTimeline.FormatElapsed(1000));
            Assert.AreEqual("4s", ChatActivityTimeline.FormatElapsed(4200));
        }

        [Test]
        public void Streaming_AddsTheEllipsisToTheSummary()
        {
            var timeline = new ChatActivityTimeline { isStreaming = true };
            var label = timeline.Q<LocalizedTextElement>(ChatActivityTimeline.summaryLabelUssClassName);
            Assert.AreEqual("Thinking…", label.text);

            timeline.isStreaming = false;
            Assert.AreEqual("Thinking", label.text);
        }

        [Test]
        public void Open_TogglesTheRows()
        {
            var timeline = new ChatActivityTimeline { parts = SampleParts() };
            var rows = timeline.Q(ChatActivityTimeline.rowsUssClassName);
            Assert.IsTrue(rows.ClassListContains("unity-hidden"));

            timeline.open = true;
            Assert.IsFalse(rows.ClassListContains("unity-hidden"));
            Assert.IsTrue(timeline.ClassListContains("is-open"));
        }

        [Test]
        public void Parts_AreCopiedNotAliased()
        {
            var source = new List<ChatActivityPart>
            {
                new ChatActivityPart(ChatActivityPartKind.Thinking, "One"),
            };
            var timeline = new ChatActivityTimeline { parts = source, isStreaming = true };
            source.Add(new ChatActivityPart(ChatActivityPartKind.Thinking, "Two"));

            Assert.AreEqual(1, timeline.rowCount);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        /// <summary>
        /// The connector is a token-width line in the border colour, drawn behind the row rather than
        /// masked by an opaque glyph background that would not match the host surface.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Connector_IsATokenWidthLineInTheBorderColour()
        {
            var body = ChatTestUtils.RuleBody(ChatTestUtils.ReadUss(k_Uss), @"\.appui-chat-activity-timeline__connector");
            StringAssert.Contains("position: absolute", body);
            StringAssert.Contains("width: var(--appui-alias-control-border-width)", body);
            StringAssert.Contains("background-color: var(--appui-alias-control-border-color-default)", body);
        }

        // -------------------------------------------------------------------
        // #1460: in-flight names, open change, stats, header placement, folding, pinning
        // -------------------------------------------------------------------

        static ChatActivityPart Call(string tool, string text, bool pending = false, int elapsedMs = -1) =>
            new ChatActivityPart(ChatActivityPartKind.Tool, text, tool, pending, elapsedMs);

        static ChatActivityPart Thought(string text, int elapsedMs = -1) =>
            new ChatActivityPart(ChatActivityPartKind.Thinking, text, elapsedMs: elapsedMs);

        static string RowText(VisualElement row) =>
            row.Q<LocalizedTextElement>(name: ChatActivityTimeline.rowTextUssClassName).text;

        [Test]
        public void ToolName_DefaultsToNullOnTheOriginalConstructor()
        {
            Assert.IsNull(new ChatActivityPart(ChatActivityPartKind.Tool, "Read A").toolName);
            Assert.AreEqual("read_file", Call("read_file", "Read A").toolName);
        }

        [Test]
        public void RunningHeader_NamesTheNewestCallInFlight()
        {
            var timeline = new ChatActivityTimeline
            {
                isStreaming = true,
                parts = new[]
                {
                    Call("read_file", "Read A", pending: true),
                    Call("search", "Search B", pending: true),
                    Call("write_file", "Write C"),
                },
            };

            CollectionAssert.AreEqual(new[] { "search", "read_file" }, timeline.activeToolNames,
                "Every call still out, newest first; a settled call is not in flight.");
            Assert.AreEqual("search… 3 tools", timeline.SummaryText());
        }

        [Test]
        public void RunningHeader_FallsBackToTheLabelWithNothingInFlight()
        {
            var timeline = new ChatActivityTimeline
            {
                isStreaming = true,
                label = "Working",
                parts = new[] { Call("read_file", "Read A"), Thought("hmm") },
            };
            Assert.AreEqual(0, timeline.activeToolNames.Count);
            Assert.AreEqual("Working… 1 tool", timeline.SummaryText());
        }

        [Test]
        public void FinishedHeader_NamesNoCalls()
        {
            var timeline = new ChatActivityTimeline
            {
                isStreaming = true,
                isFollowedByText = true,
                parts = new[] { Call("read_file", "Read A", pending: true) },
            };
            Assert.AreEqual(0, timeline.activeToolNames.Count, "A finished group has nothing in flight.");
            Assert.AreEqual("1 tool", timeline.SummaryText());
        }

        [Test]
        public void RequestOpen_SetsTheState()
        {
            // The ChangeEvent it sends needs a panel; ChatActivityTimelineBehaviourTests covers it.
            var timeline = new ChatActivityTimeline { open = true };
            timeline.RequestOpen(false);
            Assert.IsFalse(timeline.open);
        }

        [Test]
        public void Stats_ShowOnTheHeaderOnlyWhenSet()
        {
            var timeline = new ChatActivityTimeline();
            var stats = timeline.Q<LocalizedTextElement>(name: ChatActivityTimeline.summaryStatsUssClassName);
            Assert.IsNotNull(stats);
            Assert.IsTrue(stats.ClassListContains(Styles.hiddenUssClassName));

            timeline.stats = "1.2k tokens";
            Assert.AreEqual("1.2k tokens", stats.text);
            Assert.IsFalse(stats.ClassListContains(Styles.hiddenUssClassName));

            timeline.stats = null;
            Assert.IsTrue(stats.ClassListContains(Styles.hiddenUssClassName));
        }

        [Test]
        public void HeaderPlacementBottom_PutsTheHeaderAfterTheRowsWithAnUpCaret()
        {
            var timeline = new ChatActivityTimeline { parts = SampleParts() };
            var summary = timeline.Q(name: ChatActivityTimeline.summaryUssClassName);
            var caret = timeline.Q<Icon>(name: ChatActivityTimeline.caretUssClassName);
            Assert.AreEqual(0, timeline.hierarchy.IndexOf(summary));
            Assert.AreEqual("caret-right", caret.iconName);

            timeline.headerPlacement = ChatActivityHeaderPlacement.Bottom;
            Assert.IsTrue(timeline.ClassListContains(ChatActivityTimeline.headerBottomUssClassName));
            Assert.AreEqual(timeline.hierarchy.childCount - 1, timeline.hierarchy.IndexOf(summary),
                "At the foot the header follows the rows.");
            Assert.AreEqual("caret-up", caret.iconName, "Closed, the caret points the way the rows open.");

            timeline.open = true;
            Assert.AreEqual("caret-down", caret.iconName);

            timeline.open = false;
            timeline.headerPlacement = ChatActivityHeaderPlacement.Top;
            Assert.AreEqual(0, timeline.hierarchy.IndexOf(summary));
            Assert.AreEqual("caret-right", caret.iconName);
            Assert.IsFalse(timeline.ClassListContains(ChatActivityTimeline.headerBottomUssClassName));
        }

        [Test]
        public void FoldRowsByKind_FoldsPerToolAndOrdersByLastActivity()
        {
            var timeline = new ChatActivityTimeline
            {
                open = true,
                parts = new[]
                {
                    Thought("first thought", 100),
                    Call("read_file", "Read A", elapsedMs: 1000),
                    Call("search", "Search B", elapsedMs: 2000),
                    Call("read_file", "Read C", elapsedMs: 3000),
                    Thought("second thought"),
                },
            };
            Assert.AreEqual(5, timeline.rowCount, "Unfolded: one row per part.");

            timeline.foldRowsByKind = true;
            Assert.AreEqual(3, timeline.rowCount, "One row for thinking, one per tool.");

            var rows = timeline.Query(className: ChatActivityTimeline.rowUssClassName).ToList();
            Assert.AreEqual("Search B", RowText(rows[0]), "search last saw activity first.");
            Assert.AreEqual("Read C", RowText(rows[1]), "A folded row shows its newest part.");
            Assert.AreEqual("second thought", RowText(rows[2]), "The thought that just happened is last.");

            var readElapsed = rows[1].Q<LocalizedTextElement>(name: ChatActivityTimeline.rowElapsedUssClassName);
            Assert.AreEqual("4s", readElapsed.text, "A folded row reads as the time its calls took.");
            Assert.IsNull(rows[0].Q(name: ChatActivityTimeline.rowPagerUssClassName), "A single part has no pager.");
            Assert.IsNotNull(rows[1].Q(name: ChatActivityTimeline.rowPagerUssClassName));
        }

        [Test]
        public void FoldRowsByKind_KeepsARowWorkingWhileAnyCallIsOut()
        {
            var timeline = new ChatActivityTimeline
            {
                open = true,
                foldRowsByKind = true,
                parts = new[]
                {
                    Call("read_file", "Read A", pending: true),
                    Call("read_file", "Read B"),
                },
            };
            Assert.AreEqual(1, timeline.rowCount);
            Assert.IsNotNull(timeline.Q<CircularProgress>(),
                "The newest call came back, but an older one is still out.");
        }

        [Test]
        public void FoldRowsByKind_LeavesUnnamedCallsUnfolded()
        {
            var timeline = new ChatActivityTimeline
            {
                foldRowsByKind = true,
                parts = new[]
                {
                    new ChatActivityPart(ChatActivityPartKind.Tool, "Read A"),
                    new ChatActivityPart(ChatActivityPartKind.Tool, "Read B"),
                },
            };
            Assert.AreEqual(2, timeline.rowCount);
        }

        [Test]
        public void Pager_StepsBackThroughAFoldedRow()
        {
            var timeline = new ChatActivityTimeline
            {
                open = true,
                foldRowsByKind = true,
                parts = new[] { Thought("one"), Thought("two"), Thought("three") },
            };
            VisualElement Row() => timeline.Q(className: ChatActivityTimeline.rowUssClassName);
            string Label() => Row().Q<LocalizedTextElement>(name: ChatActivityTimeline.rowPagerLabelUssClassName).text;

            Assert.AreEqual("three", RowText(Row()));
            Assert.AreEqual("Thought 3 of 3", Label());
            Assert.IsFalse(Row().Q<IconButton>("next").enabledSelf, "Already on the newest.");

            Row().Q<IconButton>("previous").clickable.InvokePressed(null);
            Assert.AreEqual("two", RowText(Row()));
            Assert.AreEqual("Thought 2 of 3", Label());

            Row().Q<IconButton>("previous").clickable.InvokePressed(null);
            Assert.AreEqual("one", RowText(Row()));
            Assert.IsFalse(Row().Q<IconButton>("previous").enabledSelf, "Already on the oldest.");
        }

        [Test]
        public void PinEntries_PinsBothEndsWithinTheCap()
        {
            var timeline = new ChatActivityTimeline
            {
                open = true,
                parts = new[] { Thought("a"), Thought("b"), Thought("c") },
                pinEntries = ChatActivityPinEntries.HeadAndFoot,
            };
            Assert.IsTrue(timeline.ClassListContains(ChatActivityTimeline.entriesUssClassName));
            var rows = timeline.Query(className: ChatActivityTimeline.rowUssClassName).ToList();
            foreach (var row in rows)
            {
                Assert.IsTrue(row.ClassListContains(ChatActivityTimeline.rowPinnedHeadUssClassName));
                Assert.IsTrue(row.ClassListContains(ChatActivityTimeline.rowPinnedFootUssClassName));
            }

            timeline.pinEntries = ChatActivityPinEntries.Foot;
            foreach (var row in rows)
            {
                Assert.IsFalse(row.ClassListContains(ChatActivityTimeline.rowPinnedHeadUssClassName),
                    "Foot mode never pins at the head.");
                Assert.IsTrue(row.ClassListContains(ChatActivityTimeline.rowPinnedFootUssClassName));
            }

            timeline.pinEntries = ChatActivityPinEntries.None;
            Assert.IsFalse(timeline.ClassListContains(ChatActivityTimeline.entriesUssClassName));
            foreach (var row in rows)
                Assert.IsFalse(row.ClassListContains(ChatActivityTimeline.rowPinnedFootUssClassName));
        }

        [Test]
        public void PinEntries_StandsDownPastSixRows()
        {
            var parts = new List<ChatActivityPart>();
            for (var i = 0; i < ChatActivityTimeline.maxPinnedEntries + 1; i++)
                parts.Add(Thought("t" + i));
            var timeline = new ChatActivityTimeline
            {
                open = true,
                parts = parts,
                pinEntries = ChatActivityPinEntries.HeadAndFoot,
            };
            Assert.AreEqual(7, timeline.rowCount);
            Assert.IsNull(timeline.Q(className: ChatActivityTimeline.rowPinnedHeadUssClassName));
            Assert.IsNull(timeline.Q(className: ChatActivityTimeline.rowPinnedFootUssClassName));

            parts.RemoveAt(0);
            timeline.parts = parts;
            Assert.IsNotNull(timeline.Q(className: ChatActivityTimeline.rowPinnedHeadUssClassName),
                "Six rows are still a shape the reader can hold.");
        }

        [Test]
        public void PinBudget_IsTheShareOfTheViewportInWholeEntries()
        {
            Assert.AreEqual(5, ChatActivityTimeline.ComputePinBudget(500f, 20f, 0.2f));
            Assert.AreEqual(2, ChatActivityTimeline.ComputePinBudget(100f, 20f, 0.5f));
            Assert.AreEqual(0, ChatActivityTimeline.ComputePinBudget(100f, 20f, 0f));
            Assert.AreEqual(ChatActivityTimeline.maxPinnedEntries,
                ChatActivityTimeline.ComputePinBudget(float.NaN, 20f, 0.5f), "Nothing to measure: cap only.");
            Assert.AreEqual(ChatActivityTimeline.maxPinnedEntries,
                ChatActivityTimeline.ComputePinBudget(100f, 0f, 0.5f));
        }

        [Test]
        public void MaxPinnedShare_IsClampedToZeroToOne()
        {
            var timeline = new ChatActivityTimeline();
            Assert.That(timeline.maxPinnedShare, Is.EqualTo(1f).Within(1e-6f));
            timeline.maxPinnedShare = 2f;
            Assert.That(timeline.maxPinnedShare, Is.EqualTo(1f).Within(1e-6f));
            timeline.maxPinnedShare = -1f;
            Assert.That(timeline.maxPinnedShare, Is.EqualTo(0f).Within(1e-6f));
            timeline.maxPinnedShare = 0.25f;
            Assert.That(timeline.maxPinnedShare, Is.EqualTo(0.25f).Within(1e-6f));
        }

#if UNITY_EDITOR
        [Test]
        public void RowIcons_AreInMinimalIconSet()
        {
            // Themes that load only the minimal icon set resolve icons through these variables, so a
            // glyph missing here renders as a blank square.
            // Resolve through the Package Manager: projects such as DevProject6 reference the
            // package with a file: dependency, so Packages/<name> is not a physical directory.
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.dt.app-ui");
            Assert.IsNotNull(info, "Package Manager does not know Packages/com.unity.dt.app-ui.");
            var uss = File.ReadAllText(Path.Combine(info.resolvedPath,
                "PackageResources", "Icons", "Icons - Minimal.uss"));
            foreach (var iconName in ChatActivityTimeline.requiredIconNames)
                Assert.That(uss, Does.Contain($"--appui-icon-{iconName}-regular:"),
                    $"'{iconName}' is missing from Icons - Minimal.uss and k_RequiredIcons in IconBrowser.cs.");
        }
#endif
    }
}
