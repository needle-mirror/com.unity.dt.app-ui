#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Icon"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="IconTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Icon))]
    class IconSnapshotTests : UISnapshotTests<Icon>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Icon { iconName = "info" });
                yield return new Story("Sizes", (ctx) =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    foreach (var size in AllSizes)
                        row.Add(new Icon { iconName = "info", size = size });
                    return row;
                });
                yield return new Story("Disabled", (ctx) =>
                {
                    // Disabled coverage from the UUM-108227 audit.
                    var icon = new Icon { iconName = "info" };
                    icon.SetEnabled(false);
                    return icon;
                });
            }
        }


        static readonly IconSize[] AllSizes =
        {
            IconSize.Xxs, IconSize.Xs, IconSize.Sm, IconSize.Md, IconSize.Lg
        };
    }
}
#endif
