using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatPlan))]
    class ChatPlanTests : VisualElementTests<ChatPlan>
    {
        const string k_Uss = "ChatPlan.uss";

        protected override string mainUssClassName => ChatPlan.ussClassName;

        // Handed over to ChatPlanSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var plan = new ChatPlan { title = "Megaman clone" };
                    plan.Add(new Text { text = "1. Add a player controller" });
                    return plan;
                });
                yield return new Story("WithPathAndControls", (ctx) =>
                {
                    var plan = new ChatPlan
                    {
                        title = "Megaman clone",
                        filePath = "Assets/Plans/megaman-clone.md",
                        editable = true,
                        expandable = true,
                    };
                    plan.Add(new Text { text = "1. Add a player controller" });
                    return plan;
                });
                yield return new Story("Editing", (ctx) =>
                {
                    var plan = new ChatPlan { title = "Megaman clone", editable = true, isEditing = true };
                    plan.Add(new TextArea { value = "1. Add a player controller" });
                    return plan;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ChatPlan />",
            @"<appui:ChatPlan title=""Plan"" file-path=""Assets/Plans/p.md"" editable=""true"" expandable=""true"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var plan = new ChatPlan();
            Assert.AreEqual(ChatPlan.defaultStatus, plan.status);
            Assert.IsFalse(plan.editable, "A plan is read-only until the caller offers the edit control.");
            Assert.IsFalse(plan.expandable);
            Assert.IsFalse(plan.isEditing);
        }

        [Test]
        public void FooterButtons_ReadTheRegistrysLabelsAndEmphasis()
        {
            var plan = new ChatPlan();
            Assert.AreEqual("Discard", plan.discardButton.title);
            Assert.AreEqual(ButtonVariant.Ghost, plan.discardButton.variant,
                "Discard is the low-emphasis half of the pair.");
            Assert.AreEqual("Run plan", plan.runButton.title);
            Assert.AreEqual(ButtonVariant.Default, plan.runButton.variant,
                "Run plan is the primary action of the card.");
        }

        [Test]
        public void OptionalHeaderControls_AreHiddenUntilOffered()
        {
            var plan = new ChatPlan();
            Assert.IsTrue(plan.editButton.ClassListContains("unity-hidden"));
            Assert.IsTrue(plan.expandButton.ClassListContains("unity-hidden"));

            plan.editable = true;
            plan.expandable = true;
            Assert.IsFalse(plan.editButton.ClassListContains("unity-hidden"));
            Assert.IsFalse(plan.expandButton.ClassListContains("unity-hidden"));
        }

        [Test]
        public void FilePath_HidesItselfWhenThereIsNothingToShow()
        {
            var plan = new ChatPlan();
            var path = plan.Q(ChatPlan.filePathUssClassName);
            Assert.IsTrue(path.ClassListContains("unity-hidden"));

            plan.filePath = "Assets/Plans/p.md";
            Assert.IsFalse(path.ClassListContains("unity-hidden"));
        }

        [Test]
        public void EveryButton_RaisesItsEvent()
        {
            var plan = new ChatPlan { editable = true, expandable = true };
            var seen = new List<string>();
            plan.editRequested += () => seen.Add("edit");
            plan.expandRequested += () => seen.Add("expand");
            plan.discardRequested += () => seen.Add("discard");
            plan.runRequested += () => seen.Add("run");

            plan.editButton.clickable.InvokePressed(null);
            plan.expandButton.clickable.InvokePressed(null);
            plan.discardButton.clickable.InvokePressed(null);
            plan.runButton.clickable.InvokePressed(null);

            CollectionAssert.AreEqual(new[] { "edit", "expand", "discard", "run" }, seen);
        }

        /// <summary>
        /// Editing is the caller's state; the card only reflects it. That has to be visible in the
        /// class list, otherwise nothing can style the pressed edit control.
        /// </summary>
        [Test]
        public void IsEditing_MarksTheCardAndTheEditControl()
        {
            var plan = new ChatPlan { editable = true, isEditing = true };
            Assert.IsTrue(plan.ClassListContains(ChatPlan.editingUssClassName));
            Assert.IsTrue(plan.editButton.ClassListContains("is-selected"));

            plan.isEditing = false;
            Assert.IsFalse(plan.ClassListContains(ChatPlan.editingUssClassName));
            Assert.IsFalse(plan.editButton.ClassListContains("is-selected"));
        }

        [Test]
        public void Children_LandInTheScrollingBody()
        {
            var plan = new ChatPlan();
            var child = new Text { text = "step" };
            plan.Add(child);

            Assert.AreEqual(1, plan.childCount);
            Assert.AreSame(plan.contentContainer, child.hierarchy.parent,
                "The plan body is what Add reaches, so the plan scrolls rather than the card growing.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Card_PaintsWithTheSurfaceAndBorderTokens()
        {
            var body = ChatTestUtils.RuleBody(ChatTestUtils.ReadUss(k_Uss), @"\.appui-chat-plan");
            StringAssert.Contains("background-color: var(--appui-alias-surface-card)", body);
            StringAssert.Contains("border-color: var(--appui-alias-control-border-color-default)", body);
        }
    }
}
