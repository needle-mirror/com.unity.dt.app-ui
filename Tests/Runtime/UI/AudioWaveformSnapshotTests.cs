#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="AudioWaveform"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="AudioWaveformTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(AudioWaveform))]
    class AudioWaveformSnapshotTests : UISnapshotTests<AudioWaveform>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new AudioWaveform());
                yield return new Story("WithPeaks", (ctx) => new AudioWaveform
                {
                    peaks = new List<float> { 0.1f, 0.6f, 1f, 0.4f, 0.2f }
                });
                yield return new Story("WithPlayhead", (ctx) => new AudioWaveform
                {
                    peaks = new List<float> { 0.1f, 0.6f, 1f, 0.4f, 0.2f },
                    progress = new Optional<float>(0.5f)
                });
            }
        }
    }
}
#endif
