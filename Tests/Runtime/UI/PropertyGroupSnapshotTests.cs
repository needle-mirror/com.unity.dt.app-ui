#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="PropertyGroup"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="PropertyGroupTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(PropertyGroup))]
    class PropertyGroupSnapshotTests : UISnapshotTests<PropertyGroup>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Section", (ctx) => Build("Transform", PropertyGroupLevel.Section, true));
                yield return new Story("SectionClosed", (ctx) => Build("Transform", PropertyGroupLevel.Section, false));
                yield return new Story("Sub", (ctx) => Build("Advanced", PropertyGroupLevel.Sub, true));
                yield return new Story("SubClosed", (ctx) => Build("Advanced", PropertyGroupLevel.Sub, false));
                yield return new Story("WithTrailing", (ctx) =>
                {
                    var group = Build("Transform", PropertyGroupLevel.Section, true);
                    group.trailingContainer.Add(new IconButton("dots-three-vertical") { quiet = true });
                    group.RefreshTrailing();
                    return group;
                });
            }
        }


        static PropertyGroup Build(string label, PropertyGroupLevel level, bool open)
        {
            var group = new PropertyGroup { label = label, level = level, open = open };
            var row = new InputLabel("Position") { rowLayout = InputLabelRowLayout.Grid };
            row.Add(new Unity.AppUI.UI.FloatField { value = 1f });
            group.Add(row);
            return group;
        }
    }
}
#endif
