#if APPUI_UITF_PRESENT
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(UserSwitcher))]
    class UserSwitcherBehaviourTests : UIComponentTests<UserSwitcher>
    {
        static List<UserAccount> Accounts(int count)
        {
            var accounts = new List<UserAccount>();
            for (var i = 0; i < count; i++)
                accounts.Add(new UserAccount { id = "acc-" + i, name = "Account " + i });
            return accounts;
        }

        UserSwitcher MountSwitcher(int count = 3) => MountElement(new UserSwitcher
        {
            accounts = Accounts(count),
            activeAccountId = "acc-0",
            style = { width = 240 },
        });

        /// <summary>
        /// Opens the account menu the way a user would, then settles the popover's own layout pass
        /// so its rows have a real <c>worldBound</c> to click. Mirrors DropdownBehaviourTests, whose
        /// popover needs the same extra frame.
        /// </summary>
        void OpenMenu(UserSwitcher switcher)
        {
            simulate.Click(switcher.triggerElement);
            simulate.FrameUpdate();
            simulate.FrameUpdate();
        }

        [Test]
        public void UserSwitcher_ClickTrigger_ShouldOpenMenuWithAccountRows()
        {
            var switcher = MountSwitcher();

            OpenMenu(switcher);

            var rows = switcher.accountRows.ToList();
            Assert.AreEqual(3, rows.Count, "Opening the menu should populate one row per account.");
            Assert.Greater(rows[0].worldBound.width, 0f,
                "A row in the open menu should have a resolved layout to be clickable.");
        }

        [Test]
        public void UserSwitcher_ClickAccountRow_ShouldSelectAccountAndRaiseEvent()
        {
            var switcher = MountSwitcher();
            var selected = new List<string>();
            switcher.accountSelected += selected.Add;

            OpenMenu(switcher);
            var row = switcher.accountRows.Single(r => r.label == "Account 2");

            simulate.Click(row);

            Assert.AreEqual(new[] { "acc-2" }, selected.ToArray(),
                "Clicking a row should report the clicked account's id.");
            Assert.AreEqual("acc-2", switcher.activeAccountId,
                "Clicking a row should make it the active account.");
        }

        [Test]
        public void UserSwitcher_ClickDisabledAccountRow_ShouldNotSelect()
        {
            var switcher = MountElement(new UserSwitcher
            {
                accounts = new[]
                {
                    new UserAccount { id = "ok", name = "Personal" },
                    new UserAccount { id = "locked", name = "Locked", disabled = true, warning = "No seat." },
                },
                style = { width = 240 },
            });
            var selected = new List<string>();
            switcher.accountSelected += selected.Add;

            OpenMenu(switcher);
            var row = switcher.accountRows.Single(r => r.label == "Locked");

            simulate.Click(row);

            Assert.IsEmpty(selected, "A disabled row must never report a selection.");
        }
    }
}
#endif
