#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="AudioPreview"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="AudioPreviewTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(AudioPreview))]
    class AudioPreviewSnapshotTests : UISnapshotTests<AudioPreview>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new AudioPreview
                {
                    peaks = Envelope(), durationLabel = "0:12", style = { width = 280 },
                });
                yield return new Story("Playing", (ctx) => new AudioPreview
                {
                    peaks = Envelope(), progress = 0.4f, playing = true, style = { width = 280 },
                });
                yield return new Story("Loading", (ctx) => new AudioPreview
                {
                    loading = true, style = { width = 280 },
                });
                yield return new Story("Message", (ctx) => new AudioPreview
                {
                    message = "Clip too large to preview", style = { width = 280 },
                });
                yield return new Story("Fallback", (ctx) => new AudioPreview
                {
                    fallbackIcon = "music-note", style = { width = 280 },
                });
            }
        }


        static List<float> Envelope()
        {
            var peaks = new List<float>();
            for (var i = 0; i < 32; i++)
                peaks.Add(0.2f + 0.6f * (i % 8) / 8f);
            return peaks;
        }
    }
}
#endif
