#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Accordion"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="AccordionTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Accordion))]
    class AccordionSnapshotTests : UISnapshotTests<Accordion>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => BuildAccordion(false));
                yield return new Story("Exclusive", (ctx) => BuildAccordion(true));
            }
        }


        static Accordion BuildAccordion(bool exclusive)
        {
            var accordion = new Accordion { isExclusive = exclusive, style = { width = 320 } };
            accordion.Add(new AccordionItem { title = "Item 1", value = true });
            accordion.Add(new AccordionItem { title = "Item 2" });
            return accordion;
        }
    }
}
#endif
