#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
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
    /// Snapshot stories for <see cref="UserAvatar"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="UserAvatarTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(UserAvatar))]
    class UserAvatarSnapshotTests : UISnapshotTests<UserAvatar>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new UserAvatar
                {
                    userName = "Ada Lovelace",
                    userColor = k_Blue,
                });
                yield return new Story("Ring", (ctx) => new UserAvatar
                {
                    userName = "Ada Lovelace",
                    userColor = k_Blue,
                    ring = true,
                    size = Size.Lg,
                });
                yield return new Story("Unknown", (ctx) => new UserAvatar());
            }
        }


        static readonly Color k_Blue = new Color(0.04f, 0.43f, 0.78f);
    }
}
#endif
