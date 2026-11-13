#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ListBox"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ListBoxTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ListBox))]
    class ListBoxSnapshotTests : UISnapshotTests<ListBox>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", _ => MakeList(20));

                yield return new Story("Sizes", _ =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    foreach (var size in k_AllSizes)
                    {
                        var list = MakeList(10);
                        list.size = size;
                        list.style.width = 200;
                        row.Add(list);
                    }
                    return row;
                });

                yield return new Story("MultipleSelection", MultipleSelection);
            }
        }


        static ListBox MakeList(int count)
        {
            return new ListBox
            {
                style = { height = 400 },
                itemHeight = 40,
                itemsSource = Enumerable.Range(0, count).Select(i => (object)$"Item {i}").ToList(),
            };
        }


        /// <summary>
        /// Two rows actually selected under <see cref="SelectionType.Multiple"/>.
        /// </summary>
        /// <remarks>
        /// <c>selectionType</c> on its own has no resting visual representation, so a story that
        /// only sets it is pixel-identical to <c>Default</c>. The rows are generated, so the
        /// selection can only be made once the list has laid out.
        /// </remarks>
        static IEnumerator MultipleSelection(StoryContext ctx)
        {
            var list = MakeList(20);
            list.selectionType = SelectionType.Multiple;
            ctx.container.Add(list);

            for (var i = 0; i < 3; i++)
                yield return null;

            list.SetSelectionWithoutNotify(new[] { 1, 2 });
            yield return null;
        }


        static readonly Size[] k_AllSizes = { Size.Sm, Size.Md, Size.Lg };
    }
}
#endif
