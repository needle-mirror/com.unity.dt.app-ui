using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChoiceList))]
    class ChoiceListTests : VisualElementTests<ChoiceList>
    {
        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/ChoiceList.uss";

        protected override string mainUssClassName => ChoiceList.ussClassName;

        // Handed over to ChoiceListSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var list = new ChoiceList();
                    list.Add(new ChoiceListItem { key = "a", label = "Favor speed" });
                    list.Add(new ChoiceListItem { key = "b", label = "Balanced" });
                    list.Add(new ChoiceListItem { key = "c", label = "Favor quality" });
                    return list;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ChoiceList />",
            @"<appui:ChoiceList value=""b""><appui:ChoiceListItem key=""a"" label=""A option"" /><appui:ChoiceListItem key=""b"" label=""B option"" /></appui:ChoiceList>",
            @"<appui:ChoiceList><appui:ChoiceListItem key=""a"" label=""Pinned"" letter=""Z"" /></appui:ChoiceList>",
        };

        static ChoiceList MakeList(params string[] keys)
        {
            var list = new ChoiceList();
            foreach (var key in keys)
                list.Add(new ChoiceListItem { key = key, label = key });
            return list;
        }

        [Test]
        [Order(10)]
        public void ChoiceList_IsARadioGroup_AndItsItemsAreRadios()
        {
            // The reuse is the design: nothing about selection is re-implemented here.
            Assert.IsInstanceOf<RadioGroup>(new ChoiceList());
            Assert.IsInstanceOf<Radio>(new ChoiceListItem());
        }

        [Test]
        [Order(11)]
        public void Items_CarryBothTheRadioAndTheChoiceListItemClasses()
        {
            var item = new ChoiceListItem();
            Assert.IsTrue(item.ClassListContains(ChoiceListItem.ussClassName));
            Assert.IsTrue(item.ClassListContains(Radio.ussClassName));
        }

        [UnityTest]
        [Order(12)]
        public IEnumerator Items_AreAutoLetteredInDocumentOrder()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var list = MakeList("a", "b", "c");
            panel.Add(list);
            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            Assert.AreEqual("A", ((ChoiceListItem)list[0]).letter);
            Assert.AreEqual("B", ((ChoiceListItem)list[1]).letter);
            Assert.AreEqual("C", ((ChoiceListItem)list[2]).letter);
        }

        [UnityTest]
        [Order(13)]
        public IEnumerator Letters_AreReassignedWhenAnItemIsInserted()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var list = MakeList("a", "b");
            panel.Add(list);
            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            list.Insert(0, new ChoiceListItem { key = "z", label = "z" });

            yield return null;

            Assert.AreEqual("A", ((ChoiceListItem)list[0]).letter);
            Assert.AreEqual("B", ((ChoiceListItem)list[1]).letter);
            Assert.AreEqual("C", ((ChoiceListItem)list[2]).letter);
        }

        [UnityTest]
        [Order(14)]
        public IEnumerator ExplicitLetter_IsPinnedAndDoesNotConsumeASequenceSlot()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var list = new ChoiceList();
            var pinned = new ChoiceListItem { key = "a", label = "a", letter = "Z" };
            list.Add(pinned);
            list.Add(new ChoiceListItem { key = "b", label = "b" });
            list.Add(new ChoiceListItem { key = "c", label = "c" });
            panel.Add(list);
            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            Assert.AreEqual("Z", pinned.letter);
            Assert.AreEqual("A", ((ChoiceListItem)list[1]).letter);
            Assert.AreEqual("B", ((ChoiceListItem)list[2]).letter);
        }

        [Test]
        [Order(15)]
        public void EmptyLetter_HidesTheBadge()
        {
            var item = new ChoiceListItem { key = "a", label = "a", letter = "" };
            var badge = item.Q(className: ChoiceListItem.badgeUssClassName);
            Assert.IsNotNull(badge, "ChoiceListItem must build a letter badge element.");
            Assert.IsTrue(badge.ClassListContains(Styles.hiddenUssClassName));
        }

        [Test]
        [Order(16)]
        public void Letters_RollOverPastZ()
        {
            Assert.AreEqual("A", ChoiceList.GetLetter(0));
            Assert.AreEqual("Z", ChoiceList.GetLetter(25));
            Assert.AreEqual("AA", ChoiceList.GetLetter(26));
            Assert.AreEqual("AB", ChoiceList.GetLetter(27));
        }

        [UnityTest]
        [Order(17)]
        public IEnumerator SelectingAnItem_DrivesTheGroupValue()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var list = MakeList("a", "b", "c");
            panel.Add(list);
            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            // Inherited from RadioGroup: first registered radio becomes the value.
            Assert.AreEqual("a", list.value);

            ((ChoiceListItem)list[2]).value = true;

            yield return null;

            Assert.AreEqual("c", list.value);
            Assert.IsFalse(((ChoiceListItem)list[0]).value);
        }

        [UnityTest]
        [Order(18)]
        public IEnumerator SelectedItem_CarriesTheCheckedClass()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var list = MakeList("a", "b");
            panel.Add(list);
            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            list.value = "b";

            yield return null;

            Assert.IsFalse(list[0].ClassListContains(Styles.checkedUssClassName));
            Assert.IsTrue(list[1].ClassListContains(Styles.checkedUssClassName));
        }

        [Test]
        [Order(19)]
        [ConditionalIgnore("IgnoreInPlayer", "USS source is not available in a player build")]
        public void RowHeight_ComesFromTheControlSizingAlias_ForEverySize()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = File.ReadAllText(fullPath);

            foreach (var name in new[] { "sm", "md", "lg" })
            {
                var rule = Regex.Match(uss, @"\.appui-choicelistitem\.appui-radio--size-" + name + @"\s*\{(?<body>[^}]*)\}");
                Assert.IsTrue(rule.Success, $"ChoiceList.uss has no .appui-choicelistitem.appui-radio--size-{name} rule.");
                var body = rule.Groups["body"].Value;

                StringAssert.Contains("var(--appui-alias-control-sizing-" + name + "-height)", body,
                    $".appui-choicelistitem.appui-radio--size-{name} must bind its height to the control-sizing alias.");
                Assert.IsFalse(Regex.IsMatch(body, @"--appui-sizing-\d"),
                    $".appui-choicelistitem.appui-radio--size-{name} uses a raw --appui-sizing step instead of the alias.");
                Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"),
                    $".appui-choicelistitem.appui-radio--size-{name} uses a literal pixel value instead of the alias.");
            }
        }

        [Test]
        [Order(20)]
        [ConditionalIgnore("IgnoreInPlayer", "USS source is not available in a player build")]
        public void Uss_UsesTokensOnly_NoLiteralColours()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "ChoiceList.uss must not contain a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "ChoiceList.uss must not contain a literal rgb()/rgba() colour.");
        }

        [Test]
        [Order(21)]
        public void Size_EmitsTheMatchingUssClass()
        {
            var item = new ChoiceListItem();
            // Size.Md is the default for controls.
            Assert.AreEqual(Size.Md, item.size);
            Assert.IsTrue(item.ClassListContains(Radio.ussClassName + "--size-md"));

            item.size = Size.Lg;
            Assert.IsTrue(item.ClassListContains(Radio.ussClassName + "--size-lg"));
            Assert.IsFalse(item.ClassListContains(Radio.ussClassName + "--size-md"),
                "The stale size class must be removed.");
        }
    }
}
