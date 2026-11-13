#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="AssetTargetField"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="AssetTargetFieldTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(AssetTargetField))]
    class AssetTargetFieldSnapshotTests : UISnapshotTests<AssetTargetField>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new AssetTargetField());
                yield return new Story("Assigned", ctx => new AssetTargetField { value = Bronze() });
                yield return new Story("Sizes", ctx =>
                {
                    var container = new VisualElement();
                    foreach (var (size, _) in k_Sizes)
                        container.Add(new AssetTargetField { size = size, value = Bronze() });
                    return container;
                });
                yield return new Story("NoClear", ctx => new AssetTargetField
                {
                    value = Bronze(),
                    showClearButton = false,
                });
                yield return new Story("Invalid", ctx => new AssetTargetField
                {
                    value = Bronze(),
                    validateValue = v => v != null && v.name == "Gold",
                });
            }
        }


        static AssetReference Bronze() => new AssetReference
        {
            name = "Bronze",
            type = "Material",
            typeIcon = "sphere",
        };


        static readonly (Size size, string suffix)[] k_Sizes =
        {
            (Size.Sm, "sm"),
            (Size.Md, "md"),
            (Size.Lg, "lg"),
        };
    }
}
#endif
