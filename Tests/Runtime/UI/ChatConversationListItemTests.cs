using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatConversationListItem))]
    class ChatConversationListItemTests : VisualElementTests<ChatConversationListItem>
    {
        const string k_Uss = "ChatConversationListItem.uss";

        protected override string mainUssClassName => ChatConversationListItem.ussClassName;

        // Handed over to ChatConversationListItemSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new ChatConversationListItem
                {
                    title = "Megaman clone",
                    subtitle = "2 hours ago",
                });
                yield return new Story("Active", (ctx) => new ChatConversationListItem
                {
                    title = "Megaman clone",
                    isActive = true,
                });
                yield return new Story("Favorite", (ctx) => new ChatConversationListItem
                {
                    title = "Megaman clone",
                    isFavorite = true,
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ChatConversationListItem />",
            @"<appui:ChatConversationListItem title=""Chat"" subtitle=""now"" is-active=""true"" is-favorite=""true"" />",
        };

        [Test]
        public void Title_AlsoBecomesTheTooltipBecauseItTruncates()
        {
            var item = new ChatConversationListItem { title = "A very long conversation title" };
            Assert.AreEqual("A very long conversation title", item.tooltip);
        }

        [Test]
        public void Subtitle_HidesItselfWhenThereIsNothingToShow()
        {
            var item = new ChatConversationListItem();
            var subtitle = item.Q(ChatConversationListItem.subtitleUssClassName);
            Assert.IsTrue(subtitle.ClassListContains("unity-hidden"));

            item.subtitle = "2 hours ago";
            Assert.IsFalse(subtitle.ClassListContains("unity-hidden"));
        }

        [Test]
        public void IsActive_MarksTheRowSelectedAndClearsAgain()
        {
            var item = new ChatConversationListItem { isActive = true };
            Assert.IsTrue(item.ClassListContains("is-selected"));

            item.isActive = false;
            Assert.IsFalse(item.ClassListContains("is-selected"),
                "The selected class must come off, not accumulate.");
        }

        /// <summary>
        /// The favourite state has to reach both the row (which reveals the star) and the glyph itself
        /// (which fills in), otherwise a favourited conversation loses its star the moment the pointer
        /// leaves the row.
        /// </summary>
        [Test]
        public void IsFavorite_MarksTheRowAndFillsTheGlyph()
        {
            var item = new ChatConversationListItem { isFavorite = true };
            Assert.IsTrue(item.ClassListContains(ChatConversationListItem.favoriteUssClassName));
            Assert.AreEqual(IconVariant.Fill, item.favoriteButton.variant);

            item.isFavorite = false;
            Assert.IsFalse(item.ClassListContains(ChatConversationListItem.favoriteUssClassName));
            Assert.AreEqual(IconVariant.Regular, item.favoriteButton.variant);
        }

        [Test]
        public void Favoritable_HidesTheStarEntirely()
        {
            var item = new ChatConversationListItem { favoritable = false };
            Assert.IsTrue(item.favoriteButton.ClassListContains("unity-hidden"));

            item.favoritable = true;
            Assert.IsFalse(item.favoriteButton.ClassListContains("unity-hidden"));
        }

        /// <summary>
        /// The two targets have to stay separate: starring a conversation must not also open it.
        /// </summary>
        [UnityTest]
        public IEnumerator TheStarAndTheBody_AreSeparateTargets()
        {
            var item = new ChatConversationListItem { title = "Chat" };
            yield return ChatTestUtils.Mount(m_TestUI, item);

            var opened = 0;
            var starred = 0;
            item.clicked += () => opened++;
            item.favoriteToggled += () => starred++;

            item.favoriteButton.clickable.InvokePressed(null);
            Assert.AreEqual(1, starred);
            Assert.AreEqual(0, opened, "Starring a conversation must not open it.");

            item.clickable.InvokePressed(null);
            Assert.AreEqual(1, opened);
            Assert.AreEqual(1, starred);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        /// <summary>
        /// The star fades rather than being removed from the layout, so it stays focusable for a
        /// keyboard user and revealing it never reflows the row.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Star_FadesRatherThanBeingRemovedFromTheLayout()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            var body = ChatTestUtils.RuleBody(uss, @"\.appui-chat-conversation-list-item__favorite-button");
            StringAssert.Contains("opacity: 0", body);
            Assert.IsFalse(body.Contains("display: none"),
                "Removing the star from the layout would make it unreachable by keyboard.");
            StringAssert.Contains(".appui-chat-conversation-list-item__favorite-button:focus", uss,
                "The star must reveal itself on focus, not only on hover.");
        }
    }
}
