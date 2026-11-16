#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Linq;
using System;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Button"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ButtonTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Button))]
    class ButtonSnapshotTests : UISnapshotTests<Button>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                foreach (var (variant, name) in Utils.CurrentEnumMembers<ButtonVariant>())
                {
                    var v = variant;
                    yield return new Story(name, (ctx) => new Button
                    {
                        variant = v,
                        title = name,
                        leadingIcon = "info",
                    });
                }

                // Disabled coverage from the UUM-108227 audit. Ghost is the variant that
                // replaced the former `quiet` flag, and it is the one whose disabled state
                // has the least contrast to fall back on, so it is the one worth pinning.
                yield return new Story("GhostDisabled", (ctx) =>
                {
                    var button = new Button { title = "Button", variant = ButtonVariant.Ghost };
                    button.SetEnabled(false);
                    return button;
                });
            }
        }
    }
}
#endif
