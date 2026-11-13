#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="UserSwitcher"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="UserSwitcherTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(UserSwitcher))]
    class UserSwitcherSnapshotTests : UISnapshotTests<UserSwitcher>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("AvatarOnly", (ctx) => Build());
                yield return new Story("WithName", (ctx) =>
                {
                    var switcher = Build();
                    switcher.showName = true;
                    return switcher;
                });
                yield return new Story("WithProfile", (ctx) =>
                {
                    var switcher = Build();
                    switcher.profile = new UserProfile
                    {
                        name = "Ada Lovelace",
                        email = "ada@unity3d.com",
                        caption = "Analytical Engines",
                    };
                    switcher.showName = true;
                    return switcher;
                });
            }
        }


        static UserSwitcher Build(int count = 3)
        {
            return new UserSwitcher
            {
                accounts = Accounts(count),
                activeAccountId = "acc-1",
            };
        }


        static List<UserAccount> Accounts(int count)
        {
            var accounts = new List<UserAccount>();
            for (var i = 0; i < count; i++)
                accounts.Add(new UserAccount { id = "acc-" + i, name = "Account " + i });
            return accounts;
        }
    }
}
#endif
