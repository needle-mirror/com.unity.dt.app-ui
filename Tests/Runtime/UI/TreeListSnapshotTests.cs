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
    /// Snapshot stories for <see cref="TreeList"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="TreeListTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(TreeList))]
    class TreeListSnapshotTests : UISnapshotTests<TreeList>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => MakeTree(out _));
                yield return new Story("Multiple", MultipleSelection);
            }
        }


        /// <summary>
        /// Builds the canonical fixture tree:
        /// <code>
        /// a           (expanded)
        ///   a1
        ///   a2        (collapsed)
        ///     a2x
        /// b
        /// </code>
        /// Visible order is therefore a, a1, a2, b.
        /// </summary>
        static TreeList MakeTree(out TreeViewItem[] items)
        {
            var a = new TreeViewItem { label = "a", showCaret = true, expanded = true };
            var a1 = new TreeViewItem { label = "a1" };
            var a2 = new TreeViewItem { label = "a2", showCaret = true };
            var a2x = new TreeViewItem { label = "a2x" };
            var b = new TreeViewItem { label = "b" };

            a2.Add(a2x);
            a.Add(a1);
            a.Add(a2);

            var tree = new TreeList();
            tree.Add(a);
            tree.Add(b);

            items = new[] { a, a1, a2, a2x, b };
            return tree;
        }


        /// <summary>
        /// Two items actually selected under <see cref="SelectionType.Multiple"/>.
        /// </summary>
        /// <remarks>
        /// <c>selectionType</c> on its own has no resting visual representation, so a story that
        /// only sets it is pixel-identical to <c>Default</c>.
        /// </remarks>
        static IEnumerator MultipleSelection(StoryContext ctx)
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            ctx.container.Add(tree);

            for (var i = 0; i < 3; i++)
                yield return null;

            // a and a1, both visible in the canonical fixture tree.
            tree.SetSelectionWithoutNotify(new[] { items[0], items[1] });
            yield return null;
        }
    }
}
#endif
