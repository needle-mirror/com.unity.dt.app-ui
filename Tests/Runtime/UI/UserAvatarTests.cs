using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(UserAvatar))]
    class UserAvatarTests : VisualElementTests<UserAvatar>
    {
        protected override string mainUssClassName => UserAvatar.ussClassName;

        // Handed over to UserAvatarSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/UserAvatar.uss";

        static readonly Color k_Blue = new Color(0.04f, 0.43f, 0.78f);

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new UserAvatar
                {
                    userName = "Ada Lovelace",
                    userColor = k_Blue,
                });
                yield return new Story("Ring", (ctx) => new UserAvatar
                {
                    userName = "Ada Lovelace",
                    userColor = k_Blue,
                    ring = true,
                    size = Size.Lg,
                });
                yield return new Story("Unknown", (ctx) => new UserAvatar());
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:UserAvatar />",
            @"<appui:UserAvatar user-id=""u-42"" user-name=""Ada Lovelace"" ring=""true"" size=""Lg"" />",
            @"<appui:UserAvatar user-name=""Madonna"" variant=""Rounded"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var avatar = new UserAvatar();

            Assert.AreEqual(Size.Md, avatar.size, "The registry defaults UserAvatar to the medium size.");
            Assert.AreEqual(AvatarVariant.Circular, avatar.variant);
            Assert.IsFalse(avatar.ring, "The presence ring is opt-in.");
            Assert.IsTrue(avatar.autoLabelColor,
                "The initials colour must be derived from the identity colour, not the theme.");
            Assert.AreEqual(UserAvatar.unknownLabel, avatar.label,
                "An identity with no name yet must fall back to a neutral marker.");
        }

        [Test]
        public void ItIsAnAvatar_SoItCarriesBothClassNames()
        {
            var avatar = new UserAvatar();
            Assert.IsTrue(avatar.ClassListContains(Unity.AppUI.UI.Avatar.ussClassName),
                "UserAvatar builds on Avatar and must keep its styling class.");
            Assert.IsTrue(avatar.ClassListContains(UserAvatar.ussClassName));
        }

        [TestCase("Ada Lovelace", "AL")]
        [TestCase("  Ada   Lovelace  ", "AL")]
        [TestCase("Madonna", "MA")]
        [TestCase("X", "X")]
        [TestCase("Ada Byron King Lovelace", "AL")]
        [TestCase("", "")]
        [TestCase(null, "")]
        public void GetInitials_MatchesTheRegistry(string name, string expected)
        {
            Assert.AreEqual(expected, UserAvatar.GetInitials(name));
        }

        [Test]
        public void Name_DrivesTheInitials()
        {
            var avatar = new UserAvatar { userName = "Ada Lovelace" };
            Assert.AreEqual("AL", avatar.label);

            avatar.userName = "Grace Hopper";
            Assert.AreEqual("GH", avatar.label);

            avatar.userName = null;
            Assert.AreEqual(UserAvatar.unknownLabel, avatar.label,
                "Clearing the name must go back to the neutral marker, not keep stale initials.");
        }

        [Test]
        public void Color_BecomesTheBackdrop()
        {
            var avatar = new UserAvatar { userColor = k_Blue };
            Assert.IsTrue(avatar.backgroundColor.IsSet);
            Assert.AreEqual(k_Blue, avatar.backgroundColor.Value);

            avatar.userColor = Optional<Color>.none;
            Assert.IsFalse(avatar.backgroundColor.IsSet,
                "With no identity colour the avatar must keep the component's default surface.");
        }

        [Test]
        public void Identity_SetsEveryFieldAtOnce()
        {
            var avatar = new UserAvatar
            {
                identity = new UserIdentity { id = "u-42", name = "Ada Lovelace", color = k_Blue },
            };

            Assert.AreEqual("u-42", avatar.userId);
            Assert.AreEqual("Ada Lovelace", avatar.userName);
            Assert.AreEqual("AL", avatar.label);
            Assert.AreEqual(k_Blue, avatar.userColor.Value);
        }

        [Test]
        public void Ring_TogglesItsClass()
        {
            var avatar = new UserAvatar();
            Assert.IsFalse(avatar.ClassListContains(UserAvatar.ringUssClassName));

            avatar.ring = true;
            Assert.IsTrue(avatar.ClassListContains(UserAvatar.ringUssClassName));

            avatar.ring = false;
            Assert.IsFalse(avatar.ClassListContains(UserAvatar.ringUssClassName),
                "A stale ring class would leave the outline painted after the ring is turned off.");
        }

        [Test]
        public void Ring_LeavesTheOutlineWidthToUss()
        {
            var avatar = new UserAvatar();
            Assert.IsFalse(avatar.outlineWidth.IsSet,
                "Avatar writes an inline 2px border; UserAvatar must clear it so the token widths win.");
            Assert.AreEqual(StyleKeyword.Null, avatar.style.borderTopWidth.keyword);
        }

        [UnityTest]
        public IEnumerator Ring_TintsWithTheIdentityColourOnlyWhileHovered()
        {
            var avatar = new UserAvatar { ring = true, userColor = k_Blue };
            m_TestUI.rootVisualElement.Add(avatar);
            yield return null;

            Assert.AreEqual(StyleKeyword.Null, avatar.style.borderTopColor.keyword,
                "At rest the ring must use the neutral token colour from USS.");

            using (var enter = PointerEnterEvent.GetPooled())
            {
                enter.target = avatar;
                avatar.SendEvent(enter);
            }

            Assert.AreEqual(k_Blue, avatar.style.borderTopColor.value,
                "On hover the ring must adopt the person's colour.");
            Assert.AreEqual(k_Blue, avatar.style.borderRightColor.value);
            Assert.AreEqual(k_Blue, avatar.style.borderBottomColor.value);
            Assert.AreEqual(k_Blue, avatar.style.borderLeftColor.value);

            using (var leave = PointerLeaveEvent.GetPooled())
            {
                leave.target = avatar;
                avatar.SendEvent(leave);
            }

            Assert.AreEqual(StyleKeyword.Null, avatar.style.borderTopColor.keyword,
                "Leaving must hand the colour back to USS, not freeze the person's colour on.");
        }

        [UnityTest]
        public IEnumerator Ring_WithNoIdentityColour_StaysOnTheTokenColour()
        {
            var avatar = new UserAvatar { ring = true };
            m_TestUI.rootVisualElement.Add(avatar);
            yield return null;

            using (var enter = PointerEnterEvent.GetPooled())
            {
                enter.target = avatar;
                avatar.SendEvent(enter);
            }

            Assert.AreEqual(StyleKeyword.Null, avatar.style.borderTopColor.keyword,
                "With no colour on the identity there is nothing to tint with; the token must stay.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_BindsTheRingToTokensAndCarriesNoLiterals()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = Regex.Replace(File.ReadAllText(fullPath), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            var rest = RuleBody(uss, @"\.appui-useravatar--ring");
            StringAssert.Contains("var(--appui-border-width-xs)", rest,
                "The resting ring must be one border-width token thick.");
            StringAssert.Contains("var(--appui-alias-control-border-color-default)", rest,
                "The resting ring colour is the registry's `ring-border`.");

            var hover = RuleBody(uss, @"\.appui-useravatar--ring:hover");
            StringAssert.Contains("var(--appui-border-width-sm)", hover,
                "The hovered ring must thicken to the next border-width token.");

            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"),
                "UserAvatar.uss must carry no literal colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"rgba?\("),
                "UserAvatar.uss must carry no rgb()/rgba() colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @":\s*[0-9]+(\.[0-9]+)?px"),
                "UserAvatar.uss must carry no magic pixel value.");
        }

        /// <summary>
        /// Anchored to the start of a line so a bare class pattern cannot match a descendant rule
        /// further up the file and quietly assert against someone else's declarations.
        /// </summary>
        static string RuleBody(string uss, string selector)
        {
            var match = Regex.Match(uss, @"^" + selector + @"\s*\{(?<body>[^}]*)\}",
                RegexOptions.Multiline);
            Assert.IsTrue(match.Success, $"UserAvatar.uss has no rule starting a line with {selector}.");
            return match.Groups["body"].Value;
        }
    }
}
