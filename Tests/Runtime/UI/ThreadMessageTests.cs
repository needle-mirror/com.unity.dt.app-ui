using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ThreadMessage))]
    class ThreadMessageTests : VisualElementTests<ThreadMessage>
    {
        protected override string mainUssClassName => ThreadMessage.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ThreadMessage />",
            @"<appui:ThreadMessage author-name=""John"" timestamp=""2m ago"" timestamp-tooltip=""March 11, 2026"" content=""Hello world"" state=""Default"" like-count=""3"" dislike-count=""1"" is-liked=""true"" is-disliked=""false"" author-initials=""JD"" is-editing=""false"" />",
        };

        [Test, Order(2)]
        [TestCase("")]
        [TestCase("Alice")]
        public void AuthorName_ShouldBeSettable(string name)
        {
            element.authorName = name;
            Assert.AreEqual(name, element.authorName);
        }

        [Test, Order(2)]
        [TestCase("")]
        [TestCase("5m ago")]
        public void Timestamp_ShouldBeSettable(string ts)
        {
            element.timestamp = ts;
            Assert.AreEqual(ts, element.timestamp);
        }

        [Test, Order(2)]
        [TestCase("")]
        [TestCase("March 11, 2026 at 10:00 AM")]
        public void TimestampTooltip_ShouldBeSettable(string tip)
        {
            element.timestampTooltip = tip;
            Assert.AreEqual(tip, element.timestampTooltip);
        }

        [Test, Order(2)]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("Hello world")]
        public void Content_ShouldBeSettable(string text)
        {
            element.message = text;
            Assert.AreEqual(text, element.message);
        }

        [Test, Order(2)]
        [TestCase(ThreadMessageState.Default)]
        [TestCase(ThreadMessageState.Sending)]
        [TestCase(ThreadMessageState.Draft)]
        public void State_ShouldBeSettable(ThreadMessageState state)
        {
            element.state = state;
            Assert.AreEqual(state, element.state);
        }

        [Test, Order(2)]
        public void State_ShouldUpdateUssClass()
        {
            element.state = ThreadMessageState.Draft;
            Assert.IsTrue(element.ClassListContains(ThreadMessage.GetStateUssClassName(ThreadMessageState.Draft)));
            Assert.IsFalse(element.ClassListContains(ThreadMessage.GetStateUssClassName(ThreadMessageState.Default)));

            element.state = ThreadMessageState.Sending;
            Assert.IsTrue(element.ClassListContains(ThreadMessage.GetStateUssClassName(ThreadMessageState.Sending)));
            Assert.IsFalse(element.ClassListContains(ThreadMessage.GetStateUssClassName(ThreadMessageState.Draft)));
        }

        [Test, Order(2)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(42)]
        public void LikeCount_ShouldBeSettable(int count)
        {
            element.likeCount = count;
            Assert.AreEqual(count, element.likeCount);
        }

        [Test, Order(2)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(42)]
        public void DislikeCount_ShouldBeSettable(int count)
        {
            element.dislikeCount = count;
            Assert.AreEqual(count, element.dislikeCount);
        }

        [Test, Order(2)]
        [TestCase(true)]
        [TestCase(false)]
        public void IsLiked_ShouldBeSettable(bool val)
        {
            element.isLiked = val;
            Assert.AreEqual(val, element.isLiked);
        }

        [Test, Order(2)]
        [TestCase(true)]
        [TestCase(false)]
        public void IsDisliked_ShouldBeSettable(bool val)
        {
            element.isDisliked = val;
            Assert.AreEqual(val, element.isDisliked);
        }

        [Test, Order(2)]
        [TestCase("")]
        [TestCase("JD")]
        public void AuthorInitials_ShouldBeSettable(string initials)
        {
            element.authorInitials = initials;
            Assert.AreEqual(initials, element.authorInitials);
        }

        [Test, Order(2)]
        public void AuthorAvatarColor_ShouldBeSettable()
        {
            var color = new Optional<Color>(Color.red);
            element.authorAvatarColor = color;
            Assert.AreEqual(color, element.authorAvatarColor);
        }

        [Test, Order(2)]
        public void Reactions_ShouldBeSettable()
        {
            var reactions = new List<ReactionInfo>
            {
                new ReactionInfo { emoji = "👍", count = 2, isOwnReaction = true },
                new ReactionInfo { emoji = "❤️", count = 1, isOwnReaction = false },
            };
            element.reactions = reactions;
            Assert.AreEqual(reactions, element.reactions);
        }

        [Test, Order(2)]
        public void Reactions_CanBeSetToNull()
        {
            element.reactions = null;
            Assert.IsNull(element.reactions);
        }

        [Test, Order(2)]
        public void ContentContainer_ShouldBeAttachmentsContainer()
        {
            Assert.IsNotNull(element.contentContainer);
        }

        [Test, Order(2)]
        public void MakeActionMenuItems_CanBeSetToNull()
        {
            element.makeActionMenuItems = null;
            Assert.IsNull(element.makeActionMenuItems);
        }

        [Test, Order(2)]
        public void DefaultValues_ShouldBeCorrect()
        {
            var msg = new ThreadMessage();
            Assert.AreEqual(ThreadMessageState.Default, msg.state);
            Assert.AreEqual(0, msg.likeCount);
            Assert.AreEqual(0, msg.dislikeCount);
            Assert.IsFalse(msg.isLiked);
            Assert.IsFalse(msg.isDisliked);
            Assert.IsNull(msg.makeActionMenuItems);
        }
    }
}
