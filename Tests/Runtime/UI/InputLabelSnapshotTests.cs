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
    /// Snapshot stories for <see cref="InputLabel"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="InputLabelTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(InputLabel))]
    class InputLabelSnapshotTests : UISnapshotTests<InputLabel>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var label = new InputLabel("Project name");
                    label.Add(new Unity.AppUI.UI.TextField { value = "My Project" });
                    return label;
                });
            }
        }
    }
}
#endif
