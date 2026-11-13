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
    /// Snapshot stories for <see cref="Sidebar"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="SidebarTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Sidebar))]
    class SidebarSnapshotTests : UISnapshotTests<Sidebar>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => BuildSidebar("assets"));
                yield return new Story("NoSelection", (ctx) => BuildSidebar(null));
            }
        }


        static Sidebar BuildSidebar(string selected)
        {
            var sidebar = new Sidebar { selectedId = selected };

            var header = new SidebarHeader();
            header.Add(new Text { text = "My Project" });
            sidebar.Add(header);

            var content = new SidebarContent();
            var section = new SidebarSection { heading = "Workspace" };
            section.Add(new SidebarItem { value = "assets", label = "Assets", icon = "folder" });
            section.Add(new SidebarItem { value = "scenes", label = "Scenes", icon = "stack", shortcut = "Ctrl+2" });
            content.Add(section);
            sidebar.Add(content);

            var footer = new SidebarFooter();
            footer.Add(new SidebarItem { label = "Settings", icon = "gear" });
            sidebar.Add(footer);

            return sidebar;
        }
    }
}
#endif
