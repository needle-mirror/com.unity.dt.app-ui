using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Thread))]
    class ThreadTests : VisualElementTests<Thread>
    {
        protected override string mainUssClassName => Thread.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Thread />",
            @"<appui:Thread enable-resolution=""true"" is-resolved=""false"" enable-reactions=""true"" enable-likes=""true"" enable-dislikes=""false"" enable-composer=""true"" replies-collapsed=""false"" reply-count=""3"" composer-only=""false"" />",
        };

        [Test, Order(2)]
        [TestCase(true)]
        [TestCase(false)]
        public void EnableResolution_ShouldBeSettable(bool val)
        {
            element.enableResolution = val;
            Assert.AreEqual(val, element.enableResolution);
        }

        [Test, Order(2)]
        [TestCase(true)]
        [TestCase(false)]
        public void IsResolved_ShouldBeSettable(bool val)
        {
            element.isResolved = val;
            Assert.AreEqual(val, element.isResolved);
        }

        [Test, Order(2)]
        public void IsResolved_ShouldToggleResolvedUssClass()
        {
            element.isResolved = true;
            Assert.IsTrue(element.ClassListContains(Thread.resolvedUssClassName));

            element.isResolved = false;
            Assert.IsFalse(element.ClassListContains(Thread.resolvedUssClassName));
        }

        [Test, Order(2)]
        [TestCase(true)]
        [TestCase(false)]
        public void EnableReactions_ShouldBeSettable(bool val)
        {
            element.enableReactions = val;
            Assert.AreEqual(val, element.enableReactions);
        }

        [Test, Order(2)]
        [TestCase(true)]
        [TestCase(false)]
        public void EnableLikes_ShouldBeSettable(bool val)
        {
            element.enableLikes = val;
            Assert.AreEqual(val, element.enableLikes);
        }

        [Test, Order(2)]
        [TestCase(true)]
        [TestCase(false)]
        public void EnableDislikes_ShouldBeSettable(bool val)
        {
            element.enableDislikes = val;
            Assert.AreEqual(val, element.enableDislikes);
        }

        [Test, Order(2)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void ReplyCount_ShouldBeSettable(int count)
        {
            element.replyCount = count;
            Assert.AreEqual(count, element.replyCount);
        }

        [Test, Order(2)]
        public void RepliesContainer_ShouldNotBeNull()
        {
            Assert.IsNotNull(element.repliesListView);
        }

        [Test, Order(2)]
        public void Participants_ShouldBeSettable()
        {
            var list = new List<string> { "A", "B" };
            element.participants = list;
            Assert.AreEqual(list, element.participants);
        }

        [Test, Order(2)]
        public void DefaultValues_ShouldBeCorrect()
        {
            var thread = new Thread();
            Assert.IsFalse(thread.enableResolution);
            Assert.IsFalse(thread.isResolved);
            Assert.IsTrue(thread.enableReactions);
            Assert.IsTrue(thread.enableLikes);
            Assert.AreEqual(0, thread.replyCount);
        }
    }
}
