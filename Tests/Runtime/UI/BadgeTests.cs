using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Badge))]
    class BadgeTests : VisualElementTests<Badge>
    {
        protected override string mainUssClassName => Badge.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Badge />",
            @"<ui:VisualElement style=""width: 100px; height: 100px;"">
                <appui:Badge variant=""Default"" background-color=""blue"" overlap-type=""Rectangular"" horizontal-anchor=""Right"" vertical-anchor=""Top"" color=""white"">
                    <ui:TextElement text=""5"" />
                </appui:Badge>
            </ui:VisualElement>",
        };

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new Badge
                {
                    style = { width = 100, height = 100 }
                });
                yield return new Story("Blue", ctx => new Badge
                {
                    backgroundColor = Color.blue,
                    variant = BadgeVariant.Dot,
                    overlapType = BadgeOverlapType.Rectangular,
                    verticalAnchor = VerticalAnchor.Top,
                    horizontalAnchor = HorizontalAnchor.Right,
                    style = { width = 100, height = 100 }
                });
            }
        }
    }
}
