#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SearchBar))]
    class SearchBarBehaviourTests : UIComponentTests<SearchBar>
    {
        static UnityEngine.UIElements.TextField InputFieldOf(SearchBar searchBar) =>
            searchBar.Q<UnityEngine.UIElements.TextField>(className: TextField.inputUssClassName);

        SearchBar MountSearchBar() => MountElement(new SearchBar { style = { width = 200 } });

        [Test]
        public void SearchBar_TypingIntoInput_ShouldUpdateValueAndNotify()
        {
            var searchBar = MountSearchBar();
            var notified = string.Empty;
            searchBar.RegisterCallback<ChangingEvent<string>>(evt => notified = evt.newValue);

            var input = InputFieldOf(searchBar);
            Assert.IsNotNull(input, "The SearchBar should carry an inner text input field.");
            input.value = "unity";

            Assert.AreEqual("unity", searchBar.value, "Typing into the inner field should update the SearchBar value.");
            Assert.AreEqual("unity", notified, "Typing should report the value as changing through ChangingEvent<string>, since SearchBar only fires ChangeEvent<string> on commit.");
        }

        [Test]
        public void SearchBar_Click_ShouldFocusTheField()
        {
            var searchBar = MountSearchBar();

            simulate.Click(searchBar);
            simulate.FrameUpdate();

            Assert.IsTrue(searchBar.ClassListContains(Styles.focusedUssClassName),
                "Clicking the SearchBar should focus its input field.");
        }

        [Test]
        public void SearchBar_WhenDisabled_Click_ShouldNotFocus()
        {
            var searchBar = MountSearchBar();
            searchBar.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(searchBar);
            simulate.FrameUpdate();

            Assert.IsFalse(searchBar.ClassListContains(Styles.focusedUssClassName),
                "A disabled SearchBar should not accept focus from a click.");
        }
    }
}
#endif
