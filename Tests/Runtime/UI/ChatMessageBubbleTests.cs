using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatMessageBubble))]
    class ChatMessageBubbleTests : VisualElementTests<ChatMessageBubble>
    {
        const string k_Uss = "ChatMessageBubble.uss";

        protected override string mainUssClassName => ChatMessageBubble.ussClassName;

        // Handed over to ChatMessageBubbleSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("User", (ctx) => new ChatMessageBubble
                {
                    role = ChatMessageRole.User,
                    content = "Make the player jump higher",
                });
                yield return new Story("Assistant", (ctx) => new ChatMessageBubble
                {
                    role = ChatMessageRole.Assistant,
                    content = "I raised the jump force to 8.",
                });
                yield return new Story("UserWithChips", (ctx) =>
                {
                    var bubble = new ChatMessageBubble { role = ChatMessageRole.User, content = "Fix this" };
                    bubble.sentWith.Add(new Chip { label = "PlayerController.cs" });
                    bubble.actions.Add(new ChatMessageActions { content = "Fix this" });
                    return bubble;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ChatMessageBubble />",
            @"<appui:ChatMessageBubble role=""User"" content=""Hello"" />",
            @"<appui:ChatMessageBubble role=""Assistant"" content=""Hi"" trim-content=""true"" />",
        };

        [Test]
        public void Default_IsAnAssistantMessage()
        {
            var bubble = new ChatMessageBubble();
            Assert.AreEqual(ChatMessageRole.Assistant, bubble.role,
                "The registry's assistant rendering is the neutral one, so it is the default.");
            Assert.IsTrue(bubble.ClassListContains("appui-chat-message-bubble--assistant"));
        }

        /// <summary>
        /// The generated USS class names are the enum member names lowercased. If the rendering ever
        /// stops matching what ChatMessageBubble.uss defines, that role silently loses every style, so
        /// the two are pinned against each other here.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void RoleClasses_MatchWhatTheGeneratorEmits()
        {
            var user = new ChatMessageBubble { role = ChatMessageRole.User };
            Assert.IsTrue(user.ClassListContains("appui-chat-message-bubble--user"));

            var uss = ChatTestUtils.ReadUss(k_Uss);
            StringAssert.Contains(".appui-chat-message-bubble--user", uss);
            StringAssert.Contains(".appui-chat-message-bubble--assistant", uss);
        }

        [Test]
        public void ChangingRole_RemovesTheStaleClass()
        {
            var bubble = new ChatMessageBubble { role = ChatMessageRole.User };
            bubble.role = ChatMessageRole.Assistant;

            Assert.IsFalse(bubble.ClassListContains("appui-chat-message-bubble--user"),
                "The previous role's class must be removed, not accumulated.");
            Assert.IsTrue(bubble.ClassListContains("appui-chat-message-bubble--assistant"));
        }

        /// <summary>
        /// Trimming is opt-in, and reading `content` back must return what was set rather than the
        /// trimmed rendering — a caller hands that same string to a copy action.
        /// </summary>
        [Test]
        public void TrimContent_OnlyAffectsTheRenderingAndOnlyWhenAsked()
        {
            var bubble = new ChatMessageBubble { content = "  padded  " };
            Assert.AreEqual("  padded  ", bubble.displayedContent,
                "Whitespace must survive by default; the bubble does not rewrite caller content.");

            bubble.trimContent = true;
            Assert.AreEqual("padded", bubble.displayedContent);
            Assert.AreEqual("  padded  ", bubble.content,
                "The raw content must be readable back unchanged.");
        }

        [Test]
        public void TrimContent_PreservesInternalNewlines()
        {
            var bubble = new ChatMessageBubble { trimContent = true, content = "  a\nb  " };
            Assert.AreEqual("a\nb", bubble.displayedContent);
        }

        [Test]
        public void Children_LandInTheContentContainerNotTheBubble()
        {
            var bubble = new ChatMessageBubble();
            var child = new ChatTodoList();
            bubble.Add(child);

            Assert.AreEqual(1, bubble.childCount);
            Assert.AreSame(bubble.contentContainer, child.hierarchy.parent,
                "Extra content belongs in the content container, not inside the user bubble surface.");
        }

        [Test]
        public void Slots_AreDistinctContainers()
        {
            var bubble = new ChatMessageBubble();
            Assert.AreNotSame(bubble.sentWith, bubble.footer);
            Assert.AreNotSame(bubble.footer, bubble.actions);
            Assert.AreNotSame(bubble.sentWith, bubble.contentContainer);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        /// <summary>
        /// The user bubble paints from its own component tokens, as the registry's
        /// chat-message-bubble does, so a theme can restyle it without retinting every accent
        /// action. They default to the accent action aliases, so App UI itself looks the same.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void UserBubble_PaintsWithTheUserBubbleComponentTokens()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            var body = ChatTestUtils.RuleBody(uss,
                @"^\.appui-chat-message-bubble--user > \.appui-chat-message-bubble__bubble");
            StringAssert.Contains("background-color: var(--appui-chat-message-bubble-user-background-color)", body);
            StringAssert.Contains("color: var(--appui-chat-message-bubble-user-foreground-color)", body);
            StringAssert.Contains("border-width: var(--appui-chat-message-bubble-user-border-width)", body);
            StringAssert.Contains("border-color: var(--appui-chat-message-bubble-user-border-color)", body);

            var content = ChatTestUtils.RuleBody(uss,
                @"^\.appui-chat-message-bubble--user > \.appui-chat-message-bubble__bubble > \.appui-chat-message-bubble__content");
            StringAssert.Contains("color: var(--appui-chat-message-bubble-user-foreground-color)", content);

            Assert.IsFalse(uss.Contains("--appui-alias-actions-accent-"),
                "The user bubble must read its component tokens, not the accent aliases they default to.");
        }

        /// <summary>
        /// A context chip sits on the bubble fill, so its label takes the bubble's chip token rather
        /// than the page foreground a bare Chip paints.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void UserBubble_ContextChipLabel_ReadsTheChipForegroundToken()
        {
            var body = ChatTestUtils.RuleBody(ChatTestUtils.ReadUss(k_Uss),
                @"^\.appui-chat-message-bubble--user \.appui-chat-message-bubble__sent-with \.appui-chip__label");
            StringAssert.Contains("color: var(--appui-chat-message-bubble-user-chip-foreground-color)", body);
        }

        /// <summary>
        /// The bubble's context-chip rules outrank Chip.uss's <c>:disabled</c> rules, so a disabled chip on
        /// the bubble has to restate the disabled look. Its label must leave the bubble foreground and match
        /// a bare disabled chip's label.
        /// </summary>
        [UnityTest]
        public IEnumerator UserBubble_DisabledContextChip_LooksDisabled()
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEngine.Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "light", density = "normal", lang = "en" };
            m_TestUI.rootVisualElement.Add(panel);

            var bubble = new ChatMessageBubble { role = ChatMessageRole.User, content = "Fix this" };
            var inBubble = new Chip { label = "PlayerController.cs" };
            inBubble.SetEnabled(false);
            bubble.sentWith.Add(inBubble);
            // The bubble turns its chips transparent, which is the outlined look, so compare with an outlined chip.
            var bare = new Chip { label = "PlayerController.cs", variant = Chip.Variant.Outlined };
            bare.SetEnabled(false);
            panel.Add(bubble);
            panel.Add(bare);
            for (var i = 0; i < 4; i++)
                yield return null;

            var contentColor = bubble.Q(className: ChatMessageBubble.contentUssClassName).resolvedStyle.color;
            var chipColor = inBubble.Q(className: Chip.labelUssClassName).resolvedStyle.color;
            var bareColor = bare.Q(className: Chip.labelUssClassName).resolvedStyle.color;

            Assert.AreNotEqual(contentColor, bareColor,
                "Precondition: under App UI Light the bubble foreground and the disabled foreground differ.");
            Assert.AreEqual(bareColor, chipColor,
                "A disabled context chip's label must take the disabled foreground, not the bubble's chip foreground.");

            // The rim is where the bubble's four-class chip rule outranks Chip.uss's `:disabled` rule.
            Assert.AreEqual(bare.resolvedStyle.borderTopColor, inBubble.resolvedStyle.borderTopColor,
                "A disabled context chip must take the disabled rim, not the bubble's translucent white rim.");
        }

        /// <summary>
        /// Resolved under App UI Light, where a bare Chip's label is dark and the bubble's foreground
        /// is white: the chip inside the bubble must land on the bubble's colour, not the page's.
        /// </summary>
        [UnityTest]
        public IEnumerator UserBubble_ContextChipLabel_ResolvesToTheBubbleForeground()
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEngine.Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "light", density = "normal", lang = "en" };
            m_TestUI.rootVisualElement.Add(panel);

            var bubble = new ChatMessageBubble { role = ChatMessageRole.User, content = "Fix this" };
            var inBubble = new Chip { label = "PlayerController.cs" };
            bubble.sentWith.Add(inBubble);
            var bare = new Chip { label = "PlayerController.cs" };
            panel.Add(bubble);
            panel.Add(bare);
            for (var i = 0; i < 4; i++)
                yield return null;

            var contentColor = bubble.Q(className: ChatMessageBubble.contentUssClassName).resolvedStyle.color;
            var chipColor = inBubble.Q(className: Chip.labelUssClassName).resolvedStyle.color;
            var bareColor = bare.Q(className: Chip.labelUssClassName).resolvedStyle.color;

            Assert.AreNotEqual(bareColor, contentColor,
                "Precondition: under App UI Light the bubble foreground and the page foreground differ.");
            Assert.AreEqual(contentColor, chipColor,
                "A context chip's label must resolve to the bubble's chip foreground, which defaults to "
                + "the bubble foreground.");
        }

        /// <summary>
        /// The chip's label turns to the bubble foreground, so its fill has to go too: a Filled chip
        /// keeps a light base fill under App UI Light, and white text on it is unreadable. The
        /// registry's context chip is an outlined, transparent one over the bubble.
        /// </summary>
        [UnityTest]
        public IEnumerator UserBubble_ContextChips_AreTransparentOverTheBubble()
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEngine.Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "light", density = "normal", lang = "en" };
            m_TestUI.rootVisualElement.Add(panel);

            var bubble = new ChatMessageBubble { role = ChatMessageRole.User, content = "Fix this" };
            var filled = new Chip { label = "Scene", variant = Chip.Variant.Filled };
            var outlined = new Chip { label = "Asset", variant = Chip.Variant.Outlined };
            bubble.sentWith.Add(filled);
            bubble.sentWith.Add(outlined);
            panel.Add(bubble);
            for (var i = 0; i < 4; i++)
                yield return null;

            foreach (var chip in new[] { filled, outlined })
                Assert.AreEqual(0f, chip.resolvedStyle.backgroundColor.a, 0.01f,
                    $"A {chip.variant} chip on the user bubble must not paint its own fill under the bubble foreground.");
        }

        /// <summary>
        /// With the chip fill gone, its glyphs sit on the bubble too: an ornament icon and the delete
        /// glyph must take the chip foreground, not the page tint Icon.uss gives them.
        /// </summary>
        [UnityTest]
        public IEnumerator UserBubble_ContextChipGlyphs_TakeTheChipForeground()
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEngine.Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "light", density = "normal", lang = "en" };
            m_TestUI.rootVisualElement.Add(panel);

            var bubble = new ChatMessageBubble { role = ChatMessageRole.User, content = "Fix this" };
            var ornament = new Icon { iconName = "info" };
            var chip = new Chip { label = "Scene", ornament = ornament, deletable = true };
            bubble.sentWith.Add(chip);
            panel.Add(bubble);
            for (var i = 0; i < 4; i++)
                yield return null;

            var labelColor = chip.Q(className: Chip.labelUssClassName).resolvedStyle.color;
            Assert.AreEqual(labelColor, ornament.tintColor, "The ornament icon must take the chip foreground.");

            var deleteIcon = chip.Q<Icon>(className: Chip.deleteIconUssClassName);
            Assert.IsNotNull(deleteIcon, "Expected a delete glyph on a deletable chip.");
            Assert.AreEqual(labelColor, deleteIcon.tintColor, "The delete glyph must take the chip foreground.");
        }

        [UnityTest]
        public IEnumerator UserBubble_IsTrailingAligned()
        {
            var bubble = new ChatMessageBubble { role = ChatMessageRole.User, content = "hi" };
            yield return ChatTestUtils.Mount(m_TestUI, bubble);

            Assert.AreEqual(UnityEngine.UIElements.Align.FlexEnd, bubble.resolvedStyle.alignItems,
                "A user message hugs the trailing edge of the conversation.");
        }
    }
}
