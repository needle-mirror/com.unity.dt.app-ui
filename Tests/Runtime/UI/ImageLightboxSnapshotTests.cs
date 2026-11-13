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
    /// Snapshot stories for <see cref="ImageLightbox"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ImageLightboxTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ImageLightbox))]
    class ImageLightboxSnapshotTests : UISnapshotTests<ImageLightbox>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new ImageLightbox
                {
                    src = Image(64, 32),
                    caption = "cliff-face-01.png",
                    alt = "Cliff face, tiling",
                });
                yield return new Story("NoImage", (ctx) => new ImageLightbox
                {
                    caption = "cliff-face-01.png",
                    fallback = new Text { text = "Preview unavailable" },
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
