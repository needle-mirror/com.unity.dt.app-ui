#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// The menu keyboard walk shared by every <see cref="Menu"/> (and so by the dropdown-menu
    /// <see cref="MenuTrigger"/> surface): Down/Up wrap, Home/End jump to the ends, and a disabled
    /// row is skipped because it cannot take focus. Mirrors the registry's <c>walkRows</c> (#1409).
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Menu))]
    class MenuBehaviourTests : UIComponentTests<Menu>
    {
        MenuItem[] m_Rows;

        Menu MountMenu()
        {
            var menu = new Menu();
            m_Rows = new[]
            {
                new MenuItem { label = "First" },
                new MenuItem { label = "Disabled" },
                new MenuItem { label = "Third" },
                new MenuItem { label = "Last" },
            };
            foreach (var row in m_Rows)
                menu.Add(row);
            m_Rows[1].SetEnabled(false);

            MountElement(menu);
            return menu;
        }

        Focusable focused => element.focusController.focusedElement;

        void Press(KeyCode key)
        {
            simulate.KeyPress(key);
            simulate.FrameUpdate();
        }

        void FocusRow(int index)
        {
            m_Rows[index].Focus();
            simulate.FrameUpdate();
            Assert.AreEqual(m_Rows[index], focused, "Positive control: the row should take focus.");
        }

        [Test]
        public void Menu_DownArrow_SkipsADisabledRow()
        {
            MountMenu();
            FocusRow(0);

            Press(KeyCode.DownArrow);

            Assert.AreEqual(m_Rows[2], focused, "Down should step over the disabled row to the next one.");
        }

        [Test]
        public void Menu_DownArrow_OnTheLastRow_WrapsToTheFirst()
        {
            MountMenu();
            FocusRow(3);

            Press(KeyCode.DownArrow);

            Assert.AreEqual(m_Rows[0], focused, "Down on the last row should wrap to the first.");
        }

        [Test]
        public void Menu_UpArrow_OnTheFirstRow_WrapsToTheLast()
        {
            MountMenu();
            FocusRow(0);

            Press(KeyCode.UpArrow);

            Assert.AreEqual(m_Rows[3], focused, "Up on the first row should wrap to the last.");
        }

        [Test]
        public void Menu_UpArrow_SkipsADisabledRow()
        {
            MountMenu();
            FocusRow(2);

            Press(KeyCode.UpArrow);

            Assert.AreEqual(m_Rows[0], focused, "Up should step over the disabled row to the previous one.");
        }

        [Test]
        public void Menu_HomeAndEnd_JumpToTheEnds()
        {
            MountMenu();
            FocusRow(2);

            Press(KeyCode.End);
            Assert.AreEqual(m_Rows[3], focused, "End should focus the last row.");

            Press(KeyCode.Home);
            Assert.AreEqual(m_Rows[0], focused, "Home should focus the first row.");
        }

        [Test]
        public void Menu_DownArrow_SkipsAnInvisibleRow()
        {
            MountMenu();
            m_Rows[3].visible = false;
            simulate.FrameUpdate();
            FocusRow(2);

            Press(KeyCode.DownArrow);

            Assert.AreEqual(m_Rows[0], focused, "Down should skip the invisible last row and wrap to the first.");
        }

        [Test]
        public void Menu_UpArrow_OnTheSurface_FocusesTheLastFocusableRow()
        {
            var menu = MountMenu();
            m_Rows[3].focusable = false;
            menu.Focus();
            simulate.FrameUpdate();
            Assert.AreEqual(menu, focused, "Positive control: the surface should take focus.");

            Press(KeyCode.UpArrow);

            Assert.AreEqual(m_Rows[2], focused, "Up on the surface should start from the last row that can take focus.");
        }

        [Test]
        public void Menu_End_WhenTheLastRowIsDisabled_FocusesTheLastEnabledRow()
        {
            MountMenu();
            m_Rows[3].SetEnabled(false);
            simulate.FrameUpdate();
            FocusRow(0);

            Press(KeyCode.End);

            Assert.AreEqual(m_Rows[2], focused, "End should land on the last row that can take focus.");
        }
    }
}
#endif
