#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(PropertyGroup))]
    class PropertyGroupBehaviourTests : UIComponentTests<PropertyGroup>
    {
        PropertyGroup MountGroupWithContent()
        {
            var group = new PropertyGroup { label = "Transform" };
            group.Add(new VisualElement { style = { width = 100, height = 20 } });
            return MountElement(group);
        }

        [Test]
        public void PropertyGroup_ClickToggle_ShouldToggleOpen()
        {
            var group = MountGroupWithContent();
            Assert.IsTrue(group.open, "A PropertyGroup is open by default.");

            simulate.Click(group.toggleElement);
            Assert.IsFalse(group.open, "Clicking the toggle should fold the group.");

            simulate.Click(group.toggleElement);
            Assert.IsTrue(group.open, "Clicking the toggle again should unfold the group.");
        }

        [Test]
        public void PropertyGroup_ClickToggle_ShouldInvokeOpenChanged()
        {
            var group = MountGroupWithContent();
            var notifications = 0;
            group.openChanged += _ => notifications++;

            simulate.Click(group.toggleElement);

            Assert.AreEqual(1, notifications, "Folding the group should raise openChanged exactly once.");
        }

        [Test]
        public void PropertyGroup_WhenEmpty_ClickToggle_ShouldNotToggleOpen()
        {
            // An empty group has nothing to fold, so its header is disabled and a press should not
            // look like it did something.
            var group = MountElement(new PropertyGroup { label = "Empty" });
            Assert.IsTrue(group.open, "An empty group still reports open by default.");

            simulate.Click(group.toggleElement);

            Assert.IsTrue(group.open, "An empty group's toggle should be a no-op.");
        }

        [Test]
        public void PropertyGroup_SubmitKeyOnFocusedToggle_ShouldToggleOpen()
        {
            var group = MountGroupWithContent();

            group.toggleElement.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.IsFalse(group.open, "Submitting while the toggle is focused should fold the group.");
        }

        [Test]
        public void PropertyGroup_Expandable_WithoutChildren_ClickToggle_ShouldToggleOpen()
        {
            // Rows laid out elsewhere (a timeline's gutter, say): the header still folds the group.
            var group = MountElement(new PropertyGroup { label = "Animations", expandable = true });
            simulate.FrameUpdate();

            Assert.IsTrue(group.hasContent, "An expandable group has content even without children.");
            Assert.IsTrue(group.toggleElement.enabledSelf, "An expandable group's toggle stays enabled.");
            Assert.IsFalse(group.ClassListContains(PropertyGroup.emptyUssClassName),
                "An expandable group is not empty.");

            simulate.Click(group.toggleElement);
            Assert.IsFalse(group.open, "Clicking an expandable group's toggle should fold it.");
        }

        [Test]
        public void PropertyGroup_Expandable_WhenClosed_ShouldShowEllipsis()
        {
            var group = MountElement(new PropertyGroup { label = "Animations", expandable = true });
            var ellipsis = group.Q(PropertyGroup.ellipsisUssClassName);

            Assert.IsTrue(ellipsis.ClassListContains(Styles.hiddenUssClassName), "An open group shows no ellipsis.");
            group.open = false;
            Assert.IsFalse(ellipsis.ClassListContains(Styles.hiddenUssClassName),
                "A closed expandable group shows the ellipsis for the rows folded away.");
        }

        [Test]
        public void PropertyGroup_SetExpandable_ShouldRefreshWithoutLayout()
        {
            // No child means no layout pass of the content, so the setter has to re-read the state.
            var group = new PropertyGroup { label = "Animations" };
            Assert.IsFalse(group.toggleElement.enabledSelf, "A childless group starts inert.");

            group.expandable = true;
            Assert.IsTrue(group.toggleElement.enabledSelf, "Setting expandable should enable the toggle at once.");
            Assert.IsFalse(group.ClassListContains(PropertyGroup.emptyUssClassName));

            group.expandable = false;
            Assert.IsFalse(group.toggleElement.enabledSelf, "Clearing expandable should make the toggle inert again.");
            Assert.IsTrue(group.ClassListContains(PropertyGroup.emptyUssClassName));
        }

        [Test]
        public void PropertyGroup_Expandable_WithoutRows_ContentShouldTakeNoGap()
        {
            var group = MountElement(new PropertyGroup { label = "Animations", expandable = true });
            simulate.FrameUpdate();

            Assert.IsTrue(group.ClassListContains(PropertyGroup.withoutRowsUssClassName));
            var content = group.Q(PropertyGroup.contentUssClassName);
            Assert.That(content.resolvedStyle.marginTop, Is.EqualTo(0f).Within(0.01f),
                "An open expandable group with no rows of its own should not grow by a row gap.");
        }
    }
}
#endif
