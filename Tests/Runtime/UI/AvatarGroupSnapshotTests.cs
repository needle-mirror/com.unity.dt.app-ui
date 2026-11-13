#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="AvatarGroup"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="AvatarGroupTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(AvatarGroup))]
    class AvatarGroupSnapshotTests : UISnapshotTests<AvatarGroup>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var group = new AvatarGroup { max = 3, size = Size.Md };
                    for (var i = 0; i < 5; i++)
                        group.Add(new Unity.AppUI.UI.Avatar { backgroundColor = Color.gray });
                    return group;
                });
            }
        }
    }
}
#endif
