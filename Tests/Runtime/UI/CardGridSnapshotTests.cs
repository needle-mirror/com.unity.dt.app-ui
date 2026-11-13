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
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="CardGrid"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="CardGridTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(CardGrid))]
    class CardGridSnapshotTests : UISnapshotTests<CardGrid>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Fill", (ctx) => BuildGrid(true));
                yield return new Story("FixedColumns", (ctx) => BuildGrid(false));
            }
        }


        static CardGrid BuildGrid(bool fill)
        {
            var grid = new CardGrid { minItemWidth = 160, fill = fill, style = { width = 620 } };
            for (var i = 0; i < 6; i++)
            {
                var card = new Card { style = { height = 90 } };
                var header = new CardHeader();
                header.Add(new CardTitle { text = "Template " + (i + 1) });
                card.Add(header);
                grid.Add(card);
            }
            return grid;
        }
    }
}
#endif
