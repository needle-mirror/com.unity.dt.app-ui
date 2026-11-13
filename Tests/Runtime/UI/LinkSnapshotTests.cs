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
    /// Snapshot stories for <see cref="Link"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="LinkTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Link))]
    class LinkSnapshotTests : UISnapshotTests<Link>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Link("Read the docs", "https://example.com"));
                yield return new Story("Sizes", (ctx) =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    foreach (var size in AllSizes)
                        row.Add(new Link(size.ToString(), "https://example.com") { size = size });
                    return row;
                });
            }
        }


        static readonly LinkSize[] AllSizes = { LinkSize.Sm, LinkSize.Base, LinkSize.Lg };
    }
}
#endif
