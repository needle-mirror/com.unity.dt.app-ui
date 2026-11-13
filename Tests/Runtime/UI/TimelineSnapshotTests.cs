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
    using Timeline = Unity.AppUI.UI.Timeline;

    /// <summary>
    /// Snapshot stories for <see cref="Timeline"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="TimelineTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Timeline))]
    class TimelineSnapshotTests : UISnapshotTests<Timeline>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => BuildTimeline(0));
                yield return new Story("With gutter", (ctx) => BuildTimeline(120));
            }
        }


        static Timeline BuildTimeline(float labelWidth)
        {
            var timeline = new Timeline
            {
                duration = 60f,
                labelWidth = labelWidth,
                value = 12f,
                style = { width = 640, height = 160 },
            };

            var track = new TimelineTrack { id = "position", label = "Position" };
            track.Add(new TimelineClip { start = 4f, duration = 12f, label = "Walk" });
            track.Add(new TimelineKeyframe { time = 24f });
            timeline.Add(track);

            var group = new TimelineTrackGroup { id = "cube", title = "Cube" };
            var rotation = new TimelineTrack { id = "rotation", label = "Rotation" };
            rotation.Add(new TimelineKeyframe { time = 8f });
            group.Add(rotation);
            timeline.Add(group);

            timeline.AddMarker(new TimelineMarker { time = 30f, label = "Cut" });
            return timeline;
        }
    }
}
#endif
