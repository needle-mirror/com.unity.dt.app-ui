#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Toaster"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ToasterTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Toaster))]
    class ToasterSnapshotTests : UISnapshotTests<Toaster>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx =>
                {
                    ToastManager.Clear();
                    var toaster = new Toaster();
                    ToastManager.Add("Saved");
                    ToastManager.Add("Import failed", NotificationStyle.Negative);
                    return toaster;
                });
            }
        }
    }
}
#endif
