#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ChatPlan"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ChatPlanTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ChatPlan))]
    class ChatPlanSnapshotTests : UISnapshotTests<ChatPlan>
    {
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
    }
}
#endif
