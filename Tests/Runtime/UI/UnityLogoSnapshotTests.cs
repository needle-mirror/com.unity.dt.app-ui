#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="UnityLogo"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="UnityLogoTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(UnityLogo))]
    class UnityLogoSnapshotTests : UISnapshotTests<UnityLogo>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new UnityLogo());
                yield return new Story("Variants", (ctx) =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    row.Add(new UnityLogo { variant = UnityLogoVariant.Filled });
                    row.Add(new UnityLogo { variant = UnityLogoVariant.Empty });
                    row.Add(new UnityLogo { variant = UnityLogoVariant.Animated });
                    return row;
                });
                yield return new Story("Sizes", (ctx) =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    foreach (var size in AllSizes)
                        row.Add(new UnityLogo { size = size });
                    return row;
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
