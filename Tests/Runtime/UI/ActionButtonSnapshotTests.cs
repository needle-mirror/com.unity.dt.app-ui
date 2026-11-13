#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// ActionButton is the one component wired to the Unity 7 snapshot path so far, so that the
    /// path is actually exercised on an editor where it is live rather than sitting inert. Its
    /// stories mirror <see cref="ActionButtonTests"/> exactly, which is what makes the two sets of
    /// captures comparable.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ActionButton))]
    class ActionButtonSnapshotTests : UISnapshotTests<ActionButton>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", _ => new ActionButton());
                yield return new Story("WithLabel", _ => new ActionButton { label = "Test" });
                yield return new Story("WithIconAndLabel", _ => new ActionButton { label = "Test", icon = "info", iconVariant = IconVariant.Regular });
                yield return new Story("IconOnly", _ => new ActionButton { icon = "info" });
                yield return new Story("WithTrailingIcon", _ => new ActionButton { label = "My Label", trailingIcon = "info", trailingIconVariant = IconVariant.Regular });
            }
        }
    }
}
#endif
