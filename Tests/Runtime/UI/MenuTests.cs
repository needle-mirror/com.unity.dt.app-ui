using NUnit.Framework;
using Unity.AppUI.UI;
using System.Collections.Generic;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Menu))]
    class MenuTests : VisualElementTests<Menu>
    {

        // An empty Menu lays out to nothing. The items are the component.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var menu = new Menu();
                    menu.Add(new MenuItem { label = "Open" });
                    menu.Add(new MenuItem { label = "Duplicate" });
                    menu.Add(new MenuDivider());
                    menu.Add(new MenuItem { label = "Delete" });
                    return menu;
                });
            }
        }

        protected override string mainUssClassName => Menu.ussClassName;

        // Handed over to MenuSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
