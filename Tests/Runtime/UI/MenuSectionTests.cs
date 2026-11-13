using NUnit.Framework;
using Unity.AppUI.UI;
using System.Collections.Generic;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(MenuSection))]
    class MenuSectionTests : VisualElementTests<MenuSection>
    {

        // An empty MenuSection lays out to nothing.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var section = new MenuSection { title = "Recent" };
                    section.Add(new MenuItem { label = "Scene.unity" });
                    section.Add(new MenuItem { label = "Player.prefab" });
                    return section;
                });
            }
        }

        protected override string mainUssClassName => MenuSection.ussClassName;

        // Handed over to MenuSectionSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
