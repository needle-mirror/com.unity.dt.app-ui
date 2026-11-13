using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatMessages))]
    class ChatMessagesTests : VisualElementTests<ChatMessages>
    {
        const string k_Uss = "ChatMessages.uss";

        protected override string mainUssClassName => ChatMessages.ussClassName;

        // Handed over to ChatMessagesSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        static readonly DateTime k_T0 = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        static ChatMessage User(string id, string text, DateTime? at = null) => new ChatMessage
        {
            id = id,
            role = ChatRole.User,
            content = text,
            createdAt = at,
        };

        static ChatMessage Assistant(string id, string text, DateTime? at = null, int durationMs = ChatMessage.unknownDuration) =>
            new ChatMessage
            {
                id = id,
                role = ChatRole.Assistant,
                content = text,
                createdAt = at,
                durationMs = durationMs,
            };

        static ChatMessage AssistantParts(string id, params ChatMessagePart[] parts) => new ChatMessage
        {
            id = id,
            role = ChatRole.Assistant,
            parts = parts,
        };

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Empty", (ctx) => new ChatMessages());
                yield return new Story("Conversation", (ctx) => new ChatMessages
                {
                    messages = new[]
                    {
                        User("u1", "Make the player jump higher"),
                        AssistantParts("a1",
                            ChatMessagePart.Thinking("The jump force lives on the controller."),
                            ChatMessagePart.ToolCall("t1", "read_file", "Read PlayerController.cs"),
                            ChatMessagePart.Text("I raised the jump force to 8.")),
                    },
                });
                yield return new Story("LoadingConversation", (ctx) => new ChatMessages
                {
                    isLoadingConversation = true,
                });
                yield return new Story("BudgetExhausted", (ctx) => new ChatMessages
                {
                    budgetExhausted = true,
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ChatMessages />",
            @"<appui:ChatMessages empty-title=""Ask me anything"" />",
            @"<appui:ChatMessages budget-exhausted=""true"" />",
            @"<appui:ChatMessages is-loading-conversation=""true"" />",
            @"<appui:ChatMessages merge-thinking=""false"" show-completed-indicator=""false"" />",
            @"<appui:ChatMessages show-streaming-indicator=""false"" />",
        };

        // -------------------------------------------------------------------
        // States
        // -------------------------------------------------------------------

        [Test]
        public void Default_ShowsTheEmptyState()
        {
            var list = new ChatMessages();
            Assert.IsTrue(list.ClassListContains(ChatMessages.emptyVariantUssClassName),
                "With no messages the container must be in its empty state.");
            var title = list.Q<LocalizedTextElement>(className: ChatMessages.emptyTitleUssClassName);
            Assert.IsNotNull(title);
            Assert.AreEqual(ChatMessages.defaultEmptyTitle, title.text);
        }

        [Test]
        public void EmptyState_SwapsToTheExhaustedCopyWhenTheBudgetIsGone()
        {
            var list = new ChatMessages { budgetExhausted = true };
            var title = list.Q<LocalizedTextElement>(className: ChatMessages.emptyTitleUssClassName);
            Assert.AreEqual(ChatMessages.defaultExhaustedTitle, title.text);
            Assert.IsNotNull(list.Q<Icon>(className: ChatMessages.emptyIconUssClassName),
                "The exhausted empty state is fronted by a warning glyph.");
        }

        [Test]
        public void EmptyState_HasNoWarningGlyphWhenTheBudgetIsFine()
        {
            var list = new ChatMessages();
            Assert.IsNull(list.Q<Icon>(className: ChatMessages.emptyIconUssClassName));
        }

        [Test]
        public void LoadingConversation_TakesPrecedenceOverEverythingElse()
        {
            var list = new ChatMessages
            {
                isLoadingConversation = true,
                messages = new[] { User("u1", "hi") },
            };
            Assert.IsTrue(list.ClassListContains(ChatMessages.loadingVariantUssClassName));
            Assert.IsNull(list.Q<ChatMessageBubble>(),
                "A conversation that is still loading renders no messages.");
            Assert.IsNotNull(list.Q<CircularProgress>(className: ChatMessages.loadingSpinnerUssClassName));
        }

        [Test]
        public void RenderLoadingConversation_ReplacesTheDefaultSpinner()
        {
            var custom = new VisualElement { name = "custom-loader" };
            var list = new ChatMessages
            {
                renderLoadingConversation = () => custom,
                isLoadingConversation = true,
            };
            Assert.IsNull(list.Q<CircularProgress>(className: ChatMessages.loadingSpinnerUssClassName));
            Assert.IsNotNull(list.Q("custom-loader"));
        }

        // -------------------------------------------------------------------
        // Message rendering
        // -------------------------------------------------------------------

        [Test]
        public void Messages_RenderOneBubbleEachInOrder()
        {
            var list = new ChatMessages
            {
                messages = new[] { User("u1", "one"), Assistant("a1", "two") },
            };
            var bubbles = list.Query<ChatMessageBubble>().ToList();
            Assert.AreEqual(2, bubbles.Count);
            Assert.AreEqual(ChatMessageRole.User, bubbles[0].role);
            Assert.AreEqual(ChatMessageRole.Assistant, bubbles[1].role);
            Assert.AreEqual("one", bubbles[0].content);
        }

        [Test]
        public void SystemMessage_RendersADestructiveBannerAndNotABubble()
        {
            var list = new ChatMessages
            {
                messages = new[]
                {
                    new ChatMessage { id = "s1", role = ChatRole.System, content = "Model unavailable" },
                },
            };
            Assert.IsNull(list.Q<ChatMessageBubble>(),
                "A system message is a banner, not a message bubble.");
            var banner = list.Q(className: ChatMessages.bannerUssClassName);
            Assert.IsNotNull(banner);
            Assert.AreEqual("Model unavailable",
                banner.Q<LocalizedTextElement>(className: ChatMessages.bannerLabelUssClassName).text);
        }

        [Test]
        public void RenderSystemMessage_ReplacesTheDefaultBanner()
        {
            var list = new ChatMessages
            {
                renderSystemMessage = m => new VisualElement { name = "custom-system-" + m.id },
                messages = new[] { new ChatMessage { id = "s1", role = ChatRole.System, content = "boom" } },
            };
            Assert.IsNull(list.Q(className: ChatMessages.bannerUssClassName));
            Assert.IsNotNull(list.Q("custom-system-s1"));
        }

        [Test]
        public void UserMessage_PutsAttachmentsAndContextItemsInTheSentWithSlot()
        {
            var list = new ChatMessages
            {
                messages = new[]
                {
                    new ChatMessage
                    {
                        id = "u1",
                        role = ChatRole.User,
                        content = "Fix this",
                        attachments = new[] { "PlayerController.cs" },
                        contextItems = new[] { "Selection" },
                    },
                },
            };
            var bubble = list.Q<ChatMessageBubble>();
            Assert.AreEqual(2, bubble.sentWith.childCount,
                "Both the attachment and the context item are chips in the sent-with slot.");
        }

        [Test]
        public void Rebuilding_DoesNotAccumulateStaleRows()
        {
            var list = new ChatMessages { messages = new[] { User("u1", "one"), Assistant("a1", "two") } };
            list.messages = new[] { User("u2", "three") };

            Assert.AreEqual(1, list.Query<ChatMessageBubble>().ToList().Count,
                "Setting messages must replace the rendered rows, not append to them.");
            Assert.AreEqual("three", list.Q<ChatMessageBubble>().content);
        }

        [Test]
        public void SettingMessagesToNull_ReturnsToTheEmptyState()
        {
            var list = new ChatMessages { messages = new[] { User("u1", "one") } };
            list.messages = null;
            Assert.IsTrue(list.ClassListContains(ChatMessages.emptyVariantUssClassName));
            Assert.IsNull(list.Q<ChatMessageBubble>());
        }

        // -------------------------------------------------------------------
        // Turn resolution
        // -------------------------------------------------------------------

        [Test]
        public void AnswerElapsed_IsTheGapFromTheNearestPrecedingUserMessage()
        {
            ChatMessageMetaContext captured = default;
            var list = new ChatMessages
            {
                renderMessageMeta = ctx =>
                {
                    captured = ctx;
                    return null;
                },
                messages = new[]
                {
                    User("u1", "first", k_T0),
                    Assistant("a1", "answer", k_T0.AddMilliseconds(2500)),
                },
            };
            Assert.IsNotNull(list);
            Assert.AreEqual("u1", captured.precedingUserMessage.id);
            Assert.AreEqual(2500, captured.answerElapsedMs);
        }

        [Test]
        public void AnswerElapsed_PrefersTheDurationTheMessageCarries()
        {
            ChatMessageMetaContext captured = default;
            var unused = new ChatMessages
            {
                renderMessageMeta = ctx =>
                {
                    captured = ctx;
                    return null;
                },
                messages = new[]
                {
                    User("u1", "first", k_T0),
                    Assistant("a1", "answer", k_T0.AddMilliseconds(2500), durationMs: 900),
                },
            };
            Assert.IsNotNull(unused);
            Assert.AreEqual(900, captured.answerElapsedMs,
                "A duration reported by the backend always wins over the derived gap.");
        }

        [Test]
        public void AnswerElapsed_IsUnknownWhenTheTimestampsCannotSupportIt()
        {
            ChatMessageMetaContext captured = default;
            var unused = new ChatMessages
            {
                renderMessageMeta = ctx =>
                {
                    captured = ctx;
                    return null;
                },
                messages = new[] { User("u1", "first"), Assistant("a1", "answer") },
            };
            Assert.IsNotNull(unused);
            Assert.AreEqual(ChatMessage.unknownDuration, captured.answerElapsedMs);
        }

        [Test]
        public void PrecedingUserMessage_IsTheNearestOneBeforeNotTheFirst()
        {
            var seen = new List<string>();
            var unused = new ChatMessages
            {
                renderMessageMeta = ctx =>
                {
                    seen.Add(ctx.precedingUserMessage?.id ?? "none");
                    return null;
                },
                messages = new[]
                {
                    User("u1", "a"),
                    Assistant("a1", "1"),
                    User("u2", "b"),
                    Assistant("a2", "2"),
                },
            };
            Assert.IsNotNull(unused);
            CollectionAssert.AreEqual(new[] { "u1", "u2" }, seen);
        }

        [Test]
        public void AnAssistantMessageWithNoPromptBeforeIt_HasNoPrecedingUserMessage()
        {
            ChatMessageMetaContext captured = default;
            var unused = new ChatMessages
            {
                renderMessageMeta = ctx =>
                {
                    captured = ctx;
                    return null;
                },
                messages = new[] { Assistant("a1", "unprompted") },
            };
            Assert.IsNotNull(unused);
            Assert.IsNull(captured.precedingUserMessage);
        }

        // -------------------------------------------------------------------
        // Streaming
        // -------------------------------------------------------------------

        [Test]
        public void Streaming_FallsBackToTheLastAssistantMessageWhileLoading()
        {
            var list = new ChatMessages
            {
                isLoading = true,
                messages = new[] { User("u1", "hi"), Assistant("a1", "...") },
            };
            Assert.IsNotNull(list.Q<ChatStreamingIndicator>(),
                "Without an explicit id, isLoading streams the last message.");
            Assert.IsFalse(list.Q<ChatStreamingIndicator>().completed);
        }

        [Test]
        public void Streaming_AnExplicitIdWinsOverTheIsLoadingHeuristic()
        {
            var list = new ChatMessages
            {
                isLoading = true,
                streamingMessageId = "a1",
                messages = new[] { Assistant("a1", "one"), Assistant("a2", "two") },
            };
            var bubbles = list.Query<ChatMessageBubble>().ToList();
            Assert.IsNotNull(bubbles[0].Q<ChatStreamingIndicator>(),
                "The named message streams even though it is not the last one.");
            Assert.IsNull(bubbles[1].Q<ChatStreamingIndicator>());
        }

        [Test]
        public void NotStreaming_ShowsNoStreamingIndicatorAtAll()
        {
            var list = new ChatMessages { messages = new[] { Assistant("a1", "done") } };
            Assert.IsNull(list.Q<ChatStreamingIndicator>());
        }

        [Test]
        public void CompletedIndicator_OnlyOnTheLastMessageAndOnlyWithAMeasuredDuration()
        {
            var list = new ChatMessages
            {
                messages = new[]
                {
                    Assistant("a1", "one", durationMs: 1000),
                    Assistant("a2", "two", durationMs: 2000),
                },
            };
            var bubbles = list.Query<ChatMessageBubble>().ToList();
            Assert.IsNull(bubbles[0].Q<ChatStreamingIndicator>(),
                "Only the last message reports how long it took.");
            var indicator = bubbles[1].Q<ChatStreamingIndicator>();
            Assert.IsNotNull(indicator);
            Assert.IsTrue(indicator.completed);
            Assert.AreEqual(2000, indicator.durationMs);

            var noDuration = new ChatMessages { messages = new[] { Assistant("a1", "one") } };
            Assert.IsNull(noDuration.Q<ChatStreamingIndicator>(),
                "An unmeasured message has nothing to report.");
        }

        [Test]
        public void ShowCompletedIndicator_False_SuppressesIt()
        {
            var list = new ChatMessages
            {
                showCompletedIndicator = false,
                messages = new[] { Assistant("a1", "one", durationMs: 1000) },
            };
            Assert.IsNull(list.Q<ChatStreamingIndicator>());
        }

        [Test]
        public void StreamingStats_ReachOnlyTheStreamingMessage()
        {
            var list = new ChatMessages
            {
                streamingMessageId = "a2",
                streamingStats = "1.2k tokens",
                messages = new[] { Assistant("a1", "one"), Assistant("a2", "two") },
            };
            var indicators = list.Query<ChatStreamingIndicator>().ToList();
            Assert.AreEqual(1, indicators.Count);
            Assert.AreEqual("1.2k tokens", indicators[0].stats);
        }

        // -------------------------------------------------------------------
        // Part grouping
        // -------------------------------------------------------------------

        [Test]
        public void ThinkingAndToolParts_CollapseIntoOneActivityTimeline()
        {
            var list = new ChatMessages
            {
                messages = new[]
                {
                    AssistantParts("a1",
                        ChatMessagePart.Thinking("first"),
                        ChatMessagePart.ToolCall("t1", "read_file", "Read a.cs"),
                        ChatMessagePart.Text("Done.")),
                },
            };
            var timelines = list.Query<ChatActivityTimeline>().ToList();
            Assert.AreEqual(1, timelines.Count);
            Assert.AreEqual(2, timelines[0].parts.Count());
        }

        [Test]
        public void ATextPart_FlushesTheActivityGroupSoOrderIsPreserved()
        {
            var list = new ChatMessages
            {
                messages = new[]
                {
                    AssistantParts("a1",
                        ChatMessagePart.Thinking("before"),
                        ChatMessagePart.Text("mid"),
                        ChatMessagePart.Thinking("after")),
                },
            };
            var bubble = list.Q<ChatMessageBubble>();
            var kinds = bubble.Children().Select(c => c is ChatActivityTimeline ? "activity" : "text").ToList();
            CollectionAssert.AreEqual(new[] { "activity", "text", "activity" }, kinds,
                "A text part must close the running activity group, not be reordered around it.");
        }

        [Test]
        public void MergeThinking_IsOnByDefaultAndCollapsesConsecutiveThoughts()
        {
            var list = new ChatMessages
            {
                messages = new[]
                {
                    AssistantParts("a1",
                        ChatMessagePart.Thinking("one"),
                        ChatMessagePart.Thinking("two")),
                },
            };
            var timeline = list.Q<ChatActivityTimeline>();
            Assert.AreEqual(1, timeline.parts.Count());
            StringAssert.Contains("one", timeline.parts.First().text);
            StringAssert.Contains("two", timeline.parts.First().text);
        }

        [Test]
        public void MergeThinking_False_GivesEachThoughtItsOwnRow()
        {
            var list = new ChatMessages
            {
                mergeThinking = false,
                messages = new[]
                {
                    AssistantParts("a1",
                        ChatMessagePart.Thinking("one"),
                        ChatMessagePart.Thinking("two")),
                },
            };
            Assert.AreEqual(2, list.Q<ChatActivityTimeline>().parts.Count());
        }

        [Test]
        public void AToolResult_IsPairedOntoItsCallAndNeverBecomesItsOwnRow()
        {
            var list = new ChatMessages
            {
                messages = new[]
                {
                    AssistantParts("a1",
                        ChatMessagePart.ToolCall("t1", "read_file", "Read a.cs", pending: true),
                        ChatMessagePart.ToolResult("t1", "read_file", "contents")),
                },
            };
            var timeline = list.Q<ChatActivityTimeline>();
            Assert.AreEqual(1, timeline.parts.Count(),
                "A result belongs to its call; it is not a separate entry.");
            Assert.IsFalse(timeline.parts.First().pending,
                "A call whose result has arrived is no longer pending.");
        }

        [Test]
        public void AToolCallWithNoResultYet_StaysPending()
        {
            var list = new ChatMessages
            {
                messages = new[]
                {
                    AssistantParts("a1", ChatMessagePart.ToolCall("t1", "read_file", "Read a.cs", pending: true)),
                },
            };
            Assert.IsTrue(list.Q<ChatActivityTimeline>().parts.First().pending);
        }

        [Test]
        public void StreamingActivity_MarksOnlyTheLastTimelineOfTheStreamingMessage()
        {
            var list = new ChatMessages
            {
                streamingMessageId = "a1",
                messages = new[]
                {
                    AssistantParts("a1",
                        ChatMessagePart.Thinking("one"),
                        ChatMessagePart.Text("mid"),
                        ChatMessagePart.Thinking("two")),
                },
            };
            var timelines = list.Query<ChatActivityTimeline>().ToList();
            Assert.AreEqual(2, timelines.Count);
            Assert.IsFalse(timelines[0].isStreaming);
            Assert.IsTrue(timelines[1].isStreaming);
        }

        // -------------------------------------------------------------------
        // Seams
        // -------------------------------------------------------------------

        [Test]
        public void ToolRenderer_IsGivenEveryToolCallWithItsResult()
        {
            var renderer = new RecordingToolRenderer();
            var list = new ChatMessages
            {
                toolRenderer = renderer,
                messages = new[]
                {
                    AssistantParts("a1",
                        ChatMessagePart.ToolCall("t1", "read_file", "Read a.cs"),
                        ChatMessagePart.ToolResult("t1", "read_file", "contents")),
                },
            };
            Assert.AreEqual(1, renderer.calls.Count);
            Assert.AreEqual("read_file", renderer.calls[0].Item1.toolName);
            Assert.AreEqual("contents", renderer.calls[0].Item2);
            Assert.IsNotNull(list.Q("tool-t1"),
                "What the renderer returns has to reach the tree.");
        }

        [Test]
        public void ToolRenderer_ReturningNull_ChangesNothing()
        {
            var list = new ChatMessages
            {
                toolRenderer = new NullToolRenderer(),
                messages = new[]
                {
                    AssistantParts("a1", ChatMessagePart.ToolCall("t1", "read_file", "Read a.cs")),
                },
            };
            Assert.AreEqual(1, list.Query<ChatActivityTimeline>().ToList().Count);
        }

        [Test]
        public void RenderActions_ReplacesTheDefaultActionRowForBothRoles()
        {
            var roles = new List<ChatRole>();
            var list = new ChatMessages
            {
                renderActions = ctx =>
                {
                    roles.Add(ctx.role);
                    return new VisualElement { name = "actions-" + ctx.message.id };
                },
                messages = new[] { User("u1", "hi"), Assistant("a1", "hello") },
            };
            CollectionAssert.AreEqual(new[] { ChatRole.User, ChatRole.Assistant }, roles);
            Assert.IsNotNull(list.Q("actions-u1"));
            Assert.IsNotNull(list.Q("actions-a1"));
            Assert.IsNull(list.Q<ChatMessageActions>(),
                "The custom row replaces the default one rather than sitting next to it.");
        }

        [Test]
        public void WithoutRenderActions_AnAssistantMessageGetsTheDefaultActionRow()
        {
            var list = new ChatMessages { messages = new[] { Assistant("a1", "hello") } };
            var actions = list.Q<ChatMessageActions>();
            Assert.IsNotNull(actions);
            Assert.AreEqual("hello", actions.content,
                "The copy action must carry the message's plain text.");
        }

        [Test]
        public void TheDefaultActionRow_CarriesTheJoinedTextOfAMultiPartMessage()
        {
            var list = new ChatMessages
            {
                messages = new[]
                {
                    AssistantParts("a1",
                        ChatMessagePart.Text("one"),
                        ChatMessagePart.Thinking("hidden"),
                        ChatMessagePart.Text("two")),
                },
            };
            var actions = list.Q<ChatMessageActions>();
            StringAssert.Contains("one", actions.content);
            StringAssert.Contains("two", actions.content);
            StringAssert.DoesNotContain("hidden", actions.content,
                "Copying an answer must not leak the reasoning.");
        }

        [Test]
        public void Retry_ReportsTheMessageItWasAskedFor()
        {
            ChatMessage retried = null;
            var list = new ChatMessages { messages = new[] { Assistant("a1", "hello") } };
            list.retryRequested += m => retried = m;

            list.Q<ChatMessageActions>().retryButton.clickable.InvokePressed(null);
            Assert.IsNotNull(retried);
            Assert.AreEqual("a1", retried.id);
        }

        // -------------------------------------------------------------------
        // Banner
        // -------------------------------------------------------------------

        [Test]
        public void BudgetExhausted_AppendsABannerUnderAPopulatedConversation()
        {
            var list = new ChatMessages
            {
                budgetExhausted = true,
                messages = new[] { Assistant("a1", "hello") },
            };
            var banner = list.Q(className: ChatMessages.bannerUssClassName);
            Assert.IsNotNull(banner);
            Assert.AreEqual(ChatMessages.defaultBudgetBanner,
                banner.Q<LocalizedTextElement>(className: ChatMessages.bannerLabelUssClassName).text);
        }

        [Test]
        public void ClearingBudgetExhausted_RemovesTheBanner()
        {
            var list = new ChatMessages
            {
                budgetExhausted = true,
                messages = new[] { Assistant("a1", "hello") },
            };
            list.budgetExhausted = false;
            Assert.IsNull(list.Q(className: ChatMessages.bannerUssClassName));
        }

        // -------------------------------------------------------------------
        // Token contract
        // -------------------------------------------------------------------

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Uss_CarriesNoLiteralColour()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Uss_CarriesNoMagicPixelValue()
        {
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Uss_DefinesEveryStateClassTheComponentToggles()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            StringAssert.Contains("." + ChatMessages.ussClassName, uss);
            StringAssert.Contains("." + ChatMessages.emptyVariantUssClassName, uss);
            StringAssert.Contains("." + ChatMessages.loadingVariantUssClassName, uss);
            StringAssert.Contains("." + ChatMessages.bannerUssClassName, uss);
        }

        class RecordingToolRenderer : IChatToolRenderer
        {
            public readonly List<Tuple<ChatMessagePart, object>> calls = new();

            public VisualElement CreateToolContent(ChatMessagePart call, object result)
            {
                calls.Add(Tuple.Create(call, result));
                return new VisualElement { name = "tool-" + call.toolCallId };
            }
        }

        class NullToolRenderer : IChatToolRenderer
        {
            public VisualElement CreateToolContent(ChatMessagePart call, object result) => null;
        }

        /// <summary>
        /// A part the host renders itself must be pulled out of the bubble AND kept out of the
        /// activity group, otherwise the same tool call shows twice: once as the host's block and
        /// once as a one-line activity row.
        /// </summary>
        [Test]
        public void StandalonePart_IsRenderedByTheHostAndExcludedFromTheActivityGroup()
        {
            var messages = new ChatMessages
            {
                standalonePart = part => part.toolName == "generate_image",
                renderStandalonePart = ctx =>
                {
                    var block = new VisualElement();
                    block.AddToClassList("test-standalone");
                    return block;
                },
                messages = new[]
                {
                    AssistantParts("a1",
                        new ChatMessagePart(ChatMessagePartKind.ToolCall, "Generating",
                            toolCallId: "t1", toolName: "generate_image"),
                        new ChatMessagePart(ChatMessagePartKind.ToolCall, "Reading",
                            toolCallId: "t2", toolName: "read_file")),
                },
            };

            Assert.AreEqual(1, messages.Query(className: "test-standalone").ToList().Count,
                "The picked part must be rendered by the host.");

            var timelines = messages.Query<ChatActivityTimeline>().ToList();
            Assert.AreEqual(1, timelines.Count);
            Assert.AreEqual(1, timelines[0].rowCount,
                "The picked part must not also appear as an activity row.");
        }

        /// <summary>
        /// Returning null renders nothing, but the part still has to stay out of the activity
        /// group -- a host that declines to draw a part is not asking for the fallback row.
        /// </summary>
        [Test]
        public void StandalonePart_RenderingNull_StillExcludesThePart()
        {
            var messages = new ChatMessages
            {
                standalonePart = part => part.toolName == "generate_image",
                renderStandalonePart = ctx => null,
                messages = new[]
                {
                    AssistantParts("a1",
                        new ChatMessagePart(ChatMessagePartKind.ToolCall, "Generating",
                            toolCallId: "t1", toolName: "generate_image"),
                        new ChatMessagePart(ChatMessagePartKind.ToolCall, "Reading",
                            toolCallId: "t2", toolName: "read_file")),
                },
            };

            var timelines = messages.Query<ChatActivityTimeline>().ToList();
            Assert.AreEqual(1, timelines.Count);
            Assert.AreEqual(1, timelines[0].rowCount);
        }

        /// <summary>
        /// A tool result is folded onto its call, so pulling one out on its own would orphan it.
        /// The predicate is never consulted for one.
        /// </summary>
        [Test]
        public void StandalonePart_IsNeverConsultedForAToolResult()
        {
            var seen = new List<ChatMessagePartKind>();
            var unused = new ChatMessages
            {
                standalonePart = part =>
                {
                    seen.Add(part.kind);
                    return false;
                },
                renderStandalonePart = ctx => new VisualElement(),
                messages = new[]
                {
                    AssistantParts("a1",
                        new ChatMessagePart(ChatMessagePartKind.ToolCall, "Reading", toolCallId: "t1"),
                        new ChatMessagePart(ChatMessagePartKind.ToolResult, toolCallId: "t1",
                            result: "done")),
                },
            };

            CollectionAssert.DoesNotContain(seen, ChatMessagePartKind.ToolResult,
                "A tool result is folded onto its call and must never be offered as standalone.");
        }

        /// <summary>
        /// The block has to land in reading order, which means closing any activity run it
        /// followed rather than letting that run swallow the parts after it.
        /// </summary>
        [Test]
        public void StandalonePart_ClosesTheActivityRunItFollows()
        {
            var messages = new ChatMessages
            {
                standalonePart = part => part.toolName == "generate_image",
                renderStandalonePart = ctx =>
                {
                    var block = new VisualElement();
                    block.AddToClassList("test-standalone");
                    return block;
                },
                messages = new[]
                {
                    AssistantParts("a1",
                        new ChatMessagePart(ChatMessagePartKind.ToolCall, "Before", toolCallId: "t1"),
                        new ChatMessagePart(ChatMessagePartKind.ToolCall, "Generating",
                            toolCallId: "t2", toolName: "generate_image"),
                        new ChatMessagePart(ChatMessagePartKind.ToolCall, "After", toolCallId: "t3")),
                },
            };

            Assert.AreEqual(2, messages.Query<ChatActivityTimeline>().ToList().Count,
                "The standalone block must split the run in two rather than sitting outside it.");
        }

        // -------------------------------------------------------------------
        // showStreamingIndicator / renderActivity
        // -------------------------------------------------------------------

        [Test]
        public void ShowStreamingIndicator_DefaultsTrueAndFalseHidesTheLiveIndicator()
        {
            var list = new ChatMessages
            {
                isLoading = true,
                messages = new[] { User("u1", "hi"), Assistant("a1", "...") },
            };
            Assert.IsTrue(list.showStreamingIndicator);
            Assert.IsNotNull(list.Q<ChatStreamingIndicator>());

            list.showStreamingIndicator = false;
            Assert.IsNull(list.Q<ChatStreamingIndicator>(),
                "The host reports progress elsewhere, so the list must not say it twice.");
        }

        [Test]
        public void ShowStreamingIndicator_FalseKeepsTheCompletedIndicator()
        {
            var list = new ChatMessages
            {
                showStreamingIndicator = false,
                messages = new[] { Assistant("a1", "done", durationMs: 1200) },
            };
            var indicator = list.Q<ChatStreamingIndicator>();
            Assert.IsNotNull(indicator, "Only the live indicator stands down.");
            Assert.IsTrue(indicator.completed);
        }

        [Test]
        public void RenderActivity_ReplacesEveryTimelineWithTheHostElement()
        {
            var contexts = new List<ChatActivityRenderContext>();
            var list = new ChatMessages
            {
                renderActivity = ctx =>
                {
                    contexts.Add(ctx);
                    return new VisualElement { name = "custom-activity-" + ctx.groupIndex };
                },
                messages = new[]
                {
                    AssistantParts("a1",
                        ChatMessagePart.Thinking("first"),
                        ChatMessagePart.ToolCall("t1", "read_file", "Read A"),
                        ChatMessagePart.Text("between"),
                        ChatMessagePart.ToolCall("t2", "write_file", "Write B")),
                },
            };

            Assert.IsNull(list.Q<ChatActivityTimeline>(), "No default timeline is drawn.");
            Assert.AreEqual(2, contexts.Count);
            Assert.IsNotNull(list.Q("custom-activity-0"));
            Assert.IsNotNull(list.Q("custom-activity-1"));

            Assert.AreEqual("a1", contexts[0].message.id);
            Assert.AreEqual(0, contexts[0].groupIndex);
            Assert.AreEqual(2, contexts[0].parts.Count);
            Assert.IsTrue(contexts[0].isFollowedByText);
            Assert.AreEqual(1, contexts[1].groupIndex);
            Assert.AreEqual(1, contexts[1].parts.Count);
            Assert.IsFalse(contexts[1].isFollowedByText);
            Assert.IsFalse(contexts[0].isStreaming);
            Assert.IsFalse(contexts[1].isStreaming);

            var bubble = list.Q<ChatMessageBubble>();
            var names = bubble.Children().Select(c => c.name).ToList();
            Assert.Less(names.IndexOf("custom-activity-0"), names.IndexOf(ChatMessages.textUssClassName),
                "The host element sits where the timeline would, in reading order.");
        }

        [Test]
        public void RenderActivity_OnlyTheLastGroupOfAStreamingMessageIsLive()
        {
            var contexts = new List<ChatActivityRenderContext>();
            var start = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var message = AssistantParts("a1",
                ChatMessagePart.ToolCall("t1", "read_file", "Read A"),
                ChatMessagePart.Text("between"),
                ChatMessagePart.ToolCall("t2", "write_file", "Write B"));
            message.createdAt = start;
            var list = new ChatMessages
            {
                isLoading = true,
                renderActivity = ctx =>
                {
                    contexts.Add(ctx);
                    return new VisualElement();
                },
                messages = new[] { message },
            };

            Assert.AreEqual(2, contexts.Count);
            Assert.IsFalse(contexts[0].isStreaming);
            Assert.IsTrue(contexts[1].isStreaming);
            Assert.AreEqual(start, contexts[1].startTime);
            Assert.IsNotNull(list.Q<ChatStreamingIndicator>(),
                "A host-drawn group carries no figures, so the indicator stays.");
        }

        [Test]
        public void RenderActivity_MessagePartsPairEachCallWithItsResult()
        {
            ChatActivityRenderContext? seen = null;
            var payload = new object();
            new ChatMessages
            {
                renderActivity = ctx =>
                {
                    seen = ctx;
                    return new VisualElement();
                },
                messages = new[]
                {
                    AssistantParts("a1",
                        ChatMessagePart.Thinking("hmm"),
                        ChatMessagePart.ToolCall("t1", "read_file", "Read A"),
                        ChatMessagePart.ToolCall("t2", "write_file", "Write B"),
                        ChatMessagePart.ToolResult("t1", "read_file", payload)),
                },
            };

            Assert.IsTrue(seen.HasValue);
            var source = seen.Value.messageParts;
            Assert.AreEqual(4, source.Count, "thinking, t1, t1's result, t2 (no result yet)");
            Assert.AreEqual(ChatMessagePartKind.Thinking, source[0].kind);
            Assert.AreEqual("read_file", source[1].toolName);
            Assert.AreEqual(ChatMessagePartKind.ToolResult, source[2].kind);
            Assert.AreEqual("t1", source[2].toolCallId);
            Assert.AreSame(payload, source[2].result);
            Assert.AreEqual("t2", source[3].toolCallId);
        }

        [Test]
        public void RenderActivity_AStandaloneBlockClosesTheGroupBeforeIt()
        {
            var contexts = new List<ChatActivityRenderContext>();
            new ChatMessages
            {
                isLoading = true,
                standalonePart = p => p.toolName == "image",
                renderStandalonePart = _ => new VisualElement(),
                renderActivity = ctx =>
                {
                    contexts.Add(ctx);
                    return new VisualElement();
                },
                messages = new[]
                {
                    AssistantParts("a1",
                        ChatMessagePart.ToolCall("t1", "read_file", "Read A"),
                        ChatMessagePart.ToolCall("t2", "image", "Generate")),
                },
            };

            Assert.AreEqual(1, contexts.Count);
            Assert.IsTrue(contexts[0].isFollowedByText,
                "A standalone block after the group counts as a following block.");
        }

        [Test]
        public void RenderActivity_NullRendersNothingForThatGroup()
        {
            var list = new ChatMessages
            {
                renderActivity = _ => null,
                messages = new[]
                {
                    AssistantParts("a1", ChatMessagePart.Thinking("hmm"), ChatMessagePart.Text("answer")),
                },
            };
            Assert.IsNull(list.Q<ChatActivityTimeline>());
            Assert.AreEqual(1, list.Q<ChatMessageBubble>().childCount);
        }

        [Test]
        public void RenderActivity_ClearingItRestoresTheDefaultTimeline()
        {
            var list = new ChatMessages
            {
                renderActivity = _ => new VisualElement(),
                messages = new[] { AssistantParts("a1", ChatMessagePart.Thinking("hmm")) },
            };
            Assert.IsNull(list.Q<ChatActivityTimeline>());
            list.renderActivity = null;
            Assert.IsNotNull(list.Q<ChatActivityTimeline>());
        }

        // -------------------------------------------------------------------
        // #1460: activity group options and the live group's state
        // -------------------------------------------------------------------

        // A finished group (closed by text), then the live one.
        static ChatMessage TwoGroupTurn() => AssistantParts("a1",
            ChatMessagePart.ToolCall("t1", "read_file", "Read A"),
            ChatMessagePart.Text("between"),
            ChatMessagePart.ToolCall("t2", "search", "Search B", pending: true));

        [Test]
        public void ActivityOptions_ReachEveryDefaultGroup()
        {
            var list = new ChatMessages
            {
                activityHeaderPlacement = ChatActivityHeaderPlacement.Bottom,
                activityFoldRowsByKind = true,
                activityPinEntries = ChatActivityPinEntries.Foot,
                activityMaxPinnedShare = 0.4f,
                messages = new[] { TwoGroupTurn() },
            };
            var timelines = list.Query<ChatActivityTimeline>().ToList();
            Assert.AreEqual(2, timelines.Count);
            foreach (var timeline in timelines)
            {
                Assert.AreEqual(ChatActivityHeaderPlacement.Bottom, timeline.headerPlacement);
                Assert.IsTrue(timeline.foldRowsByKind);
                Assert.AreEqual(ChatActivityPinEntries.Foot, timeline.pinEntries);
                Assert.That(timeline.maxPinnedShare, Is.EqualTo(0.4f).Within(1e-6f));
            }

            list.activityMaxPinnedShare = 3f;
            Assert.That(list.activityMaxPinnedShare, Is.EqualTo(1f).Within(1e-6f));
        }

        [Test]
        public void ToolNames_ReachTheTimelineSoTheLiveHeaderNamesTheCall()
        {
            var list = new ChatMessages { isLoading = true, messages = new[] { TwoGroupTurn() } };
            var live = list.Query<ChatActivityTimeline>().ToList().Last();
            CollectionAssert.AreEqual(new[] { "search" }, live.activeToolNames);
            Assert.AreEqual("search… 1 tool", live.SummaryText());
        }

        [Test]
        public void RenderActivity_PartsCarryTheToolName()
        {
            var contexts = new List<ChatActivityRenderContext>();
            var list = new ChatMessages
            {
                renderActivity = ctx =>
                {
                    contexts.Add(ctx);
                    return new VisualElement();
                },
                messages = new[] { TwoGroupTurn() },
            };
            Assert.AreEqual(2, contexts.Count);
            Assert.AreEqual("read_file", contexts[0].parts[0].toolName);
            Assert.AreEqual("search", contexts[1].parts[0].toolName);
        }

        [Test]
        public void ActivityOpen_OpensOnlyTheLiveGroup()
        {
            var list = new ChatMessages
            {
                isLoading = true,
                activityOpen = true,
                messages = new[] { TwoGroupTurn() },
            };
            var timelines = list.Query<ChatActivityTimeline>().ToList();
            Assert.IsFalse(timelines[0].open, "A finished group answers to its own header.");
            Assert.IsTrue(timelines[1].open, "The live group follows the held state.");

            list.isLoading = false;
            foreach (var timeline in list.Query<ChatActivityTimeline>().ToList())
                Assert.IsFalse(timeline.open, "Once the turn ends no group is live.");
        }

        [Test]
        public void ActivityOpen_DoesNotReachAGroupTextHasClosed()
        {
            var list = new ChatMessages
            {
                isLoading = true,
                activityOpen = true,
                messages = new[]
                {
                    AssistantParts("a1", ChatMessagePart.Thinking("hmm"), ChatMessagePart.Text("answer")),
                },
            };
            Assert.IsFalse(list.Q<ChatActivityTimeline>().open,
                "The answer has begun, so the group above it is finished.");
        }

        [Test]
        public void FootHeader_MakesTheLiveGroupTheStatusLine()
        {
            var list = new ChatMessages
            {
                isLoading = true,
                streamingStats = "1.2k tokens",
                activityHeaderPlacement = ChatActivityHeaderPlacement.Bottom,
                messages = new[] { TwoGroupTurn() },
            };
            var timelines = list.Query<ChatActivityTimeline>().ToList();
            Assert.IsNull(list.Q<ChatStreamingIndicator>(), "The group says what the indicator would.");
            Assert.AreEqual("1.2k tokens", timelines[1].stats);
            Assert.IsNull(timelines[0].stats, "Only the live group carries the figures.");
        }

        [Test]
        public void FootHeader_KeepsTheIndicatorWithoutALiveGroup()
        {
            var list = new ChatMessages
            {
                isLoading = true,
                streamingStats = "1.2k tokens",
                activityHeaderPlacement = ChatActivityHeaderPlacement.Bottom,
                messages = new[]
                {
                    AssistantParts("a1", ChatMessagePart.Thinking("hmm"), ChatMessagePart.Text("answer")),
                },
            };
            Assert.IsNotNull(list.Q<ChatStreamingIndicator>(), "No group is live, so the indicator reports.");
            Assert.IsNull(list.Q<ChatActivityTimeline>().stats);
        }

        [Test]
        public void TopHeader_KeepsTheIndicatorAndLeavesTheGroupWithoutFigures()
        {
            var list = new ChatMessages
            {
                isLoading = true,
                streamingStats = "1.2k tokens",
                messages = new[] { TwoGroupTurn() },
            };
            Assert.IsNotNull(list.Q<ChatStreamingIndicator>());
            Assert.IsNull(list.Query<ChatActivityTimeline>().ToList().Last().stats);
        }

        [Test]
        public void FoldedPager_SurvivesAStreamingRebuild()
        {
            var list = new ChatMessages
            {
                isLoading = true,
                activityFoldRowsByKind = true,
                messages = new[]
                {
                    AssistantParts("a1",
                        ChatMessagePart.ToolCall("t1", "read_file", "Read A"),
                        ChatMessagePart.ToolCall("t2", "read_file", "Read B"),
                        ChatMessagePart.ToolCall("t3", "read_file", "Read C", pending: true)),
                },
            };
            string Shown() => list.Q<ChatActivityTimeline>()
                .Q<LocalizedTextElement>(name: ChatActivityTimeline.rowTextUssClassName).text;

            Assert.AreEqual("Read C", Shown());
            list.Q<ChatActivityTimeline>().Q<IconButton>("previous").clickable.InvokePressed(null);
            Assert.AreEqual("Read B", Shown());

            list.streamingStats = "42 tokens";
            Assert.AreEqual("Read B", Shown(), "A rebuild must not throw the reader back to the newest call.");
        }

        [Test]
        public void FoldedPager_AnonymousMessagesKeepTheirOwnSelections()
        {
            static ChatMessage Folded() => AssistantParts(null,
                ChatMessagePart.ToolCall("t1", "read_file", "Read A"),
                ChatMessagePart.ToolCall("t2", "read_file", "Read B"));
            var list = new ChatMessages { activityFoldRowsByKind = true, messages = new[] { Folded(), Folded() } };
            string Shown(int i) => list.Query<ChatActivityTimeline>().ToList()[i]
                .Q<LocalizedTextElement>(name: ChatActivityTimeline.rowTextUssClassName).text;

            list.Query<ChatActivityTimeline>().ToList()[0].Q<IconButton>("previous").clickable.InvokePressed(null);
            list.streamingStats = "rebuild";

            Assert.AreEqual("Read A", Shown(0));
            Assert.AreEqual("Read B", Shown(1), "The other message was never paged.");
        }

        [Test]
        public void RenderActivity_KeepsTheIndicatorEvenWithAFootHeader()
        {
            var list = new ChatMessages
            {
                isLoading = true,
                activityHeaderPlacement = ChatActivityHeaderPlacement.Bottom,
                renderActivity = _ => new VisualElement(),
                messages = new[] { TwoGroupTurn() },
            };
            Assert.IsNotNull(list.Q<ChatStreamingIndicator>(), "A host-built group carries no figures.");
        }
    }
}
