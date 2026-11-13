using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(UserSwitcher))]
    class UserSwitcherTests : VisualElementTests<UserSwitcher>
    {
        protected override string mainUssClassName => UserSwitcher.ussClassName;

        // Handed over to UserSwitcherSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/UserSwitcher.uss";

        static List<UserAccount> Accounts(int count)
        {
            var accounts = new List<UserAccount>();
            for (var i = 0; i < count; i++)
                accounts.Add(new UserAccount { id = "acc-" + i, name = "Account " + i });
            return accounts;
        }

        static UserSwitcher Build(int count = 3)
        {
            return new UserSwitcher
            {
                accounts = Accounts(count),
                activeAccountId = "acc-1",
            };
        }

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

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:UserSwitcher />",
            @"<appui:UserSwitcher show-name=""true"" section-label=""Organizations"" collapsible=""true"" size=""Lg"" />",
            @"<appui:UserSwitcher max-visible-accounts=""3"" show-account-avatars=""false"" menu-width=""320"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var switcher = new UserSwitcher();
            Assert.IsFalse(switcher.showName, "The registry defaults to an avatar-only trigger.");
            Assert.AreEqual(Size.Md, switcher.size);
            Assert.AreEqual(UserSwitcher.defaultSectionLabel, switcher.sectionLabel);
            Assert.IsFalse(switcher.collapsible);
            Assert.AreEqual(5, switcher.maxVisibleAccounts);
            Assert.IsTrue(switcher.showAccountAvatars);
            Assert.AreEqual(PopoverPlacement.BottomEnd, switcher.placement,
                "The registry aligns the popover to the trigger's end edge.");
        }

        [Test]
        public void ActiveAccount_FallsBackToTheFirst()
        {
            var switcher = new UserSwitcher { accounts = Accounts(3) };
            Assert.AreEqual("acc-0", switcher.activeAccount.id,
                "With no active id named, the first account is the active one.");

            switcher.activeAccountId = "acc-2";
            Assert.AreEqual("acc-2", switcher.activeAccount.id);

            switcher.activeAccountId = "nope";
            Assert.AreEqual("acc-0", switcher.activeAccount.id,
                "An id matching nothing must fall back rather than leave the switcher blank.");
        }

        [Test]
        public void Rows_AreRealSelectableMenuItems_WithTheActiveOneChecked()
        {
            var switcher = Build();
            var rows = switcher.accountRows.ToList();

            Assert.AreEqual(3, rows.Count);
            Assert.IsTrue(rows.All(r => r.selectable),
                "The checkmark must be the menu's own, not a glyph bolted on.");
            Assert.AreEqual("Account 1", rows.Single(r => r.value).label,
                "Exactly the active account is checked.");
        }

        [Test]
        public void Rows_CarryAnAvatarOnlyWhenAsked()
        {
            var switcher = Build();
            Assert.IsTrue(switcher.accountRows.All(r => r.Q<UserAvatar>() != null));

            switcher.showAccountAvatars = false;
            Assert.IsTrue(switcher.accountRows.All(r => r.Q<UserAvatar>() == null),
                "A list of organizations gains nothing from avatars.");
        }

        [Test]
        public void RowAvatar_SitsWhereMenuItemPutsItsOwnIcon()
        {
            var row = Build().accountRows.First();
            var avatar = row.Q<UserAvatar>();
            var label = row.hierarchy.Children()
                .First(c => c.ClassListContains(MenuItem.labelUssClassName));

            Assert.Less(row.hierarchy.IndexOf(avatar), row.hierarchy.IndexOf(label),
                "The avatar is a leading marker; behind the label it would read as a trailing one.");
        }

        [Test]
        public void DisabledAccount_IsUnselectableAndSaysWhy()
        {
            var switcher = new UserSwitcher
            {
                accounts = new[]
                {
                    new UserAccount { id = "ok", name = "Personal" },
                    new UserAccount
                    {
                        id = "locked", name = "Difference Engines",
                        disabled = true, warning = "No seat on this org.",
                    },
                },
            };

            var selected = new List<string>();
            switcher.accountSelected += selected.Add;

            var locked = switcher.accountRows.Single(r => r.label == "Difference Engines");
            Assert.IsFalse(locked.enabledSelf, "A row that cannot be switched to must look like it.");
            Assert.AreEqual("No seat on this org.", locked.tooltip,
                "The explanation belongs on the whole row, where the user will point.");
            Assert.IsNotNull(locked.Q(className: UserSwitcher.accountWarningUssClassName),
                "The row needs a marker, not just a tooltip.");

            locked.clickable.SimulateSingleClickInternal(ClickEvent.GetPooled());
            Assert.IsEmpty(selected, "A disabled row must never report a selection.");
        }

        [Test]
        public void SelectingARow_ReportsItAndMovesTheCheckmark()
        {
            var switcher = Build();
            var selected = new List<string>();
            switcher.accountSelected += selected.Add;

            var row = switcher.accountRows.Single(r => r.label == "Account 2");
            row.clickable.SimulateSingleClickInternal(ClickEvent.GetPooled());

            Assert.AreEqual(new[] { "acc-2" }, selected);
            Assert.AreEqual("acc-2", switcher.activeAccountId);
            Assert.AreEqual("Account 2", switcher.accountRows.Single(r => r.value).label,
                "The checkmark must follow the selection, not stay on the old row.");
        }

        [Test]
        public void Search_AppearsOnlyOnceTheListWouldScroll()
        {
            var switcher = new UserSwitcher { accounts = Accounts(5) };
            Assert.IsFalse(switcher.searchVisible,
                "A list that fits is not long enough to need searching.");

            switcher.accounts = Accounts(6);
            Assert.IsTrue(switcher.searchVisible);

            switcher.maxVisibleAccounts = 10;
            Assert.IsFalse(switcher.searchVisible,
                "Raising the cap makes the same list fit again.");
        }

        [Test]
        public void Collapsible_HidesTheListBehindAnExpander()
        {
            var switcher = Build();
            Assert.IsTrue(switcher.expanded, "A non-collapsible list is never hidden.");

            switcher.collapsible = true;
            Assert.IsFalse(switcher.expanded);
            Assert.IsEmpty(switcher.accountRows, "A collapsed list shows no rows.");

            var expander = switcher.menu.Q<MenuItem>(className: UserSwitcher.expanderUssClassName);
            Assert.IsNotNull(expander);
            Assert.AreEqual(switcher.sectionLabel, expander.label);

            expander.clickable.SimulateSingleClickInternal(ClickEvent.GetPooled());
            Assert.IsTrue(switcher.expanded);
            Assert.AreEqual(3, switcher.accountRows.Count());
        }

        [Test]
        public void Profile_FeaturesTheSignedInPerson_AndWidensTheMenu()
        {
            var switcher = Build();
            Assert.AreEqual(UserSwitcher.defaultMenuWidth, switcher.menu.style.width.value.value);
            Assert.IsNull(switcher.menu.Q(className: UserSwitcher.profileUssClassName));

            switcher.profile = new UserProfile { name = "Ada Lovelace", email = "ada@unity3d.com" };
            Assert.IsNotNull(switcher.menu.Q(className: UserSwitcher.profileUssClassName));
            Assert.AreEqual(UserSwitcher.defaultProfileMenuWidth, switcher.menu.style.width.value.value,
                "The registry widens the popover when it features a profile.");
        }

        [Test]
        public void TriggerAvatar_FollowsTheProfileWhenThereIsOne()
        {
            var switcher = Build();
            var avatar = switcher.triggerElement.Q<UserAvatar>();
            Assert.AreEqual("Account 1", avatar.userName);

            switcher.profile = new UserProfile { name = "Ada Lovelace" };
            Assert.AreEqual("Ada Lovelace", avatar.userName,
                "With a profile featured, the trigger is the signed-in person, not the active account.");
        }

        [Test]
        public void TriggerLabel_OverridesTheName()
        {
            var switcher = Build();
            switcher.showName = true;
            var label = switcher.Q<LocalizedTextElement>(className: UserSwitcher.triggerLabelUssClassName);
            Assert.AreEqual("Account 1", label.text);

            switcher.triggerLabel = "Analytical Engines";
            Assert.AreEqual("Analytical Engines", label.text,
                "A caller labelling the trigger with an org name must win over the account name.");
        }

        [Test]
        public void NoAccounts_DropsTheCardEntirely()
        {
            var switcher = new UserSwitcher
            {
                profile = new UserProfile { name = "Ada Lovelace" },
            };
            switcher.Add(new MenuItem { label = "Sign out" });

            Assert.IsNull(switcher.menu.Q(className: UserSwitcher.accountsUssClassName),
                "With nothing to switch between, the card is dropped and only the profile and the " +
                "extra rows remain.");
            Assert.IsNotNull(switcher.menu.Q(className: UserSwitcher.profileUssClassName));
        }

        [Test]
        public void ExtraRows_LandAfterTheGeneratedCards()
        {
            var switcher = Build();
            var signOut = new MenuItem { label = "Sign out" };
            switcher.Add(signOut);

            var extra = switcher.menu.Q(className: UserSwitcher.extraUssClassName);
            Assert.AreEqual(extra, switcher.contentContainer,
                "The switcher's content container is the menu's tail, so `Add` lands there.");
            Assert.AreEqual(extra, signOut.hierarchy.parent, "Extra rows go into the tail container.");

            var menuChildren = switcher.menu.Children().ToList();
            Assert.AreEqual(extra, menuChildren.Last(),
                "Settings and sign-out belong under the account list, not above it.");
        }

        [UnityTest]
        public IEnumerator ListHeight_ComesFromAMeasuredTokenRow_NotAHardCodedNumber()
        {
            var switcher = Build(20);
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(switcher);
            yield return null;
            yield return null;

            var probe = switcher.Q(className: UserSwitcher.rowProbeUssClassName);
            var list = switcher.menu.Q(className: UserSwitcher.listUssClassName);

            Assert.Greater(probe.resolvedStyle.height, 0f,
                "The probe is how a token px length reaches C# at all — a float custom property " +
                "silently resolves to 0.");
            Assert.AreEqual(probe.resolvedStyle.height * switcher.maxVisibleAccounts,
                list.style.maxHeight.value.value, 0.5f,
                "The list caps at maxVisibleAccounts rows of the medium control height.");
        }

        [UnityTest]
        public IEnumerator ListHeightCap_TracksDensity()
        {
            var normal = Build(20);
            var compact = Build(20);

            var normalPanel = new Panel { density = "normal" };
            var compactPanel = new Panel { density = "compact" };
            m_TestUI.rootVisualElement.Add(normalPanel);
            m_TestUI.rootVisualElement.Add(compactPanel);
            normalPanel.Add(normal);
            compactPanel.Add(compact);
            yield return null;
            yield return null;

            var normalHeight = normal.menu.Q(className: UserSwitcher.listUssClassName).style.maxHeight.value.value;
            var compactHeight = compact.menu.Q(className: UserSwitcher.listUssClassName).style.maxHeight.value.value;

            Assert.Greater(normalHeight, 0f);
            Assert.Less(compactHeight, normalHeight,
                "The cap is a token, so a compact panel must shrink it — a hard-coded number would not.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsOnTokensAndProbesTheControlSizingAlias()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = Regex.Replace(File.ReadAllText(fullPath), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            var probe = RuleBody(uss, @"\.appui-userswitcher__row-probe");
            StringAssert.Contains("var(--appui-alias-control-sizing-md-height)", probe,
                "The row height must be the medium control-sizing alias.");
            Assert.IsFalse(probe.Contains("--appui-sizing-"),
                "A raw sizing step resolves to the same pixels today and breaks density silently.");

            var caret = RuleBody(uss, @"\.appui-userswitcher__caret");
            StringAssert.Contains("var(--appui-alias-control-sizing-icon-sm)", caret,
                "A glyph inside a control is sized by the control-sizing icon alias.");

            var warning = RuleBody(uss, @"\.appui-userswitcher__account-warning");
            StringAssert.Contains("var(--appui-alias-semantic-warning)", warning,
                "The warning marker is the semantic warning token, not an eyeballed amber.");

            var menu = RuleBody(uss, @"\.appui-userswitcher__menu");
            StringAssert.Contains("var(--appui-alias-surface-raised)", menu);

            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "No literal colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"rgba?\("), "No rgb()/rgba() colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @":\s*[0-9]+(\.[0-9]+)?px"), "No magic pixel value.");
        }

        /// <summary>
        /// Anchored to the start of a line, so a bare class pattern cannot match a descendant rule
        /// further up the file and quietly assert against someone else's declarations.
        /// </summary>
        static string RuleBody(string uss, string selector)
        {
            var match = Regex.Match(uss, @"^" + selector + @"\s*\{(?<body>[^}]*)\}", RegexOptions.Multiline);
            Assert.IsTrue(match.Success, $"UserSwitcher.uss has no rule starting a line with {selector}.");
            return match.Groups["body"].Value;
        }
    }
}
