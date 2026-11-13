#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ChoiceList"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ChoiceListTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ChoiceList))]
    class ChoiceListSnapshotTests : UISnapshotTests<ChoiceList>
    {
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
    }
}
#endif
