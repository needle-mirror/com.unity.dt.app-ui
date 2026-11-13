#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ZoomableThumbnail"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ZoomableThumbnailTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ZoomableThumbnail))]
    class ZoomableThumbnailSnapshotTests : UISnapshotTests<ZoomableThumbnail>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Corner", (ctx) => new ZoomableThumbnail
                {
                    src = Image(64, 64),
                    caption = "cliff-face-01.png",
                    alt = "Cliff face, tiling",
                    style = { width = 96, height = 96 },
                });
                yield return new Story("Whole", (ctx) => new ZoomableThumbnail
                {
                    src = Image(64, 64),
                    caption = "cliff-face-01.png",
                    triggerMode = ZoomTriggerMode.Whole,
                    style = { width = 48, height = 48 },
                });
                yield return new Story("NoImage", (ctx) => new ZoomableThumbnail
                {
                    caption = "missing.png",
                    fallback = new Text { text = "No preview" },
                    style = { width = 96, height = 96 },
                });
            }
        }


        Background Image(int width, int height)
        {
            var texture = new Texture2D(width, height);
            m_Textures.Add(texture);
            return Background.FromTexture2D(texture);
        }


        readonly List<Texture2D> m_Textures = new List<Texture2D>();
    }
}
#endif
