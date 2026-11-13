using System;
using System.Collections.Generic;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// One row a <see cref="TreeListVirtual"/> draws: an item of a flattened tree, with its depth,
    /// parent and position among its siblings, or a placeholder standing in for rows that cannot be
    /// shown yet.
    /// </summary>
    /// <remarks>
    /// Mirrors the web registry's <c>TreeListRow</c>. Build a list of these with
    /// <see cref="TreeListFlatten.Flatten{T}"/>.
    /// </remarks>
    public class TreeListRow
    {
        /// <summary>
        /// The row's id. Unique among item rows; a placeholder is keyed by its parent.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The id of the row's parent, or null at the top level.
        /// </summary>
        public string parentId { get; set; }

        /// <summary>
        /// The row's depth, 0 at the top level.
        /// </summary>
        public int level { get; set; }

        /// <summary>
        /// Whether the row has children to disclose, loaded or not.
        /// </summary>
        public bool expandable { get; set; }

        /// <summary>
        /// Whether the row is open.
        /// </summary>
        public bool expanded { get; set; }

        /// <summary>
        /// How many siblings the row has, itself included. 0 on a placeholder for a whole level.
        /// </summary>
        public int setSize { get; set; }

        /// <summary>
        /// The row's 1-based position among its siblings. 0 on a placeholder for a whole level.
        /// </summary>
        public int posInSet { get; set; }

        /// <summary>
        /// Whether the row is a placeholder rather than an item.
        /// </summary>
        public bool placeholder { get; set; }

        /// <summary>
        /// On a placeholder, how many rows it stands in for, at least 1.
        /// </summary>
        public int placeholderCount { get; set; }

        /// <summary>
        /// On a placeholder for one child of a partly loaded level, its position among the children
        /// still to load; -1 otherwise.
        /// </summary>
        public int pendingIndex { get; set; } = -1;
    }

    /// <summary>
    /// A <see cref="TreeListRow"/> carrying the item it draws.
    /// </summary>
    /// <typeparam name="T">The caller's item type.</typeparam>
    public class TreeListRow<T> : TreeListRow
    {
        /// <summary>
        /// The item this row draws. Default on a placeholder.
        /// </summary>
        public T item { get; set; }
    }

    /// <summary>
    /// What <see cref="TreeListFlatten.Flatten{T}"/> reads a lazily loaded tree through.
    /// </summary>
    /// <typeparam name="T">The caller's item type.</typeparam>
    public class TreeListFlattenOptions<T>
    {
        /// <summary>
        /// The loaded children of a parent, null being the top level. Returning null means the level
        /// has not been loaded, and it is drawn as a placeholder. Required.
        /// </summary>
        public Func<string, IReadOnlyList<T>> getChildren { get; set; }

        /// <summary>
        /// The id of an item. Required.
        /// </summary>
        public Func<T, string> getId { get; set; }

        /// <summary>
        /// Whether the item has children to disclose, loaded or not. Required.
        /// </summary>
        public Func<T, bool> hasChildren { get; set; }

        /// <summary>
        /// Whether the row with this id is open. Required.
        /// </summary>
        public Func<string, bool> isExpanded { get; set; }

        /// <summary>
        /// An open parent whose children should not be listed because they are about to be replaced.
        /// It gets a placeholder and is not reported as a level to load. Optional.
        /// </summary>
        public Func<T, bool> isPending { get; set; }

        /// <summary>
        /// How many rows a placeholder for this parent stands in for (the parent is default for the
        /// top level). Defaults to 1. Optional.
        /// </summary>
        public Func<T, int> placeholderCount { get; set; }

        /// <summary>
        /// How many more children a loaded level has that have not arrived yet. Each is drawn as a
        /// one-row placeholder after the loaded children. Defaults to none. Optional.
        /// </summary>
        public Func<string, int> pendingCount { get; set; }
    }

    /// <summary>
    /// The result of <see cref="TreeListFlatten.Flatten{T}"/>.
    /// </summary>
    /// <typeparam name="T">The caller's item type.</typeparam>
    public class FlattenedTree<T>
    {
        /// <summary>
        /// Every visible row in display order, placeholders included.
        /// </summary>
        public List<TreeListRow<T>> rows { get; } = new List<TreeListRow<T>>();

        /// <summary>
        /// Every level the tree shows open, the top level (null) first, loaded or not.
        /// </summary>
        public List<string> openLevels { get; } = new List<string>();

        /// <summary>
        /// The open levels that have not been loaded.
        /// </summary>
        public List<string> unfetched { get; } = new List<string>();
    }

    /// <summary>
    /// Lays a nested tree out as the flat row list a <see cref="TreeListVirtual"/> draws. A port of
    /// the web registry's <c>flattenTree</c>.
    /// </summary>
    public static class TreeListFlatten
    {
        /// <summary>
        /// Walks a lazily loaded tree through its expansion state and lays it out as the flat,
        /// ordered row list a windowed tree renders.
        /// </summary>
        /// <remarks>
        /// An id is emitted at most once. A cache refreshed one level at a time can briefly hold an
        /// item under its old and its new parent after a move, or even a cycle, and a duplicate row
        /// would collide with its own id.
        /// </remarks>
        /// <param name="options">How to read the tree.</param>
        /// <typeparam name="T">The caller's item type.</typeparam>
        /// <returns>The rows, the open levels and the levels still to load.</returns>
        /// <exception cref="ArgumentNullException">A required option is missing.</exception>
        public static FlattenedTree<T> Flatten<T>(TreeListFlattenOptions<T> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (options.getChildren == null || options.getId == null ||
                options.hasChildren == null || options.isExpanded == null)
                throw new ArgumentNullException(nameof(options),
                    "getChildren, getId, hasChildren and isExpanded are required.");

            var result = new FlattenedTree<T>();
            var seen = new HashSet<string>();
            Walk(options, result, seen, default, null, 0);
            return result;
        }

        static TreeListRow<T> Placeholder<T>(TreeListFlattenOptions<T> options, T parent, string parentId, int level)
        {
            return new TreeListRow<T>
            {
                id = parentId ?? string.Empty,
                parentId = parentId,
                level = level,
                placeholder = true,
                placeholderCount = Math.Max(1, options.placeholderCount?.Invoke(parent) ?? 1),
            };
        }

        static void Walk<T>(TreeListFlattenOptions<T> options, FlattenedTree<T> result, HashSet<string> seen,
            T parent, string parentId, int level)
        {
            result.openLevels.Add(parentId);
            var children = options.getChildren(parentId);
            if (children == null)
            {
                result.unfetched.Add(parentId);
                result.rows.Add(Placeholder(options, parent, parentId, level));
                return;
            }

            var visible = new List<T>(children.Count);
            foreach (var child in children)
            {
                if (seen.Add(options.getId(child)))
                    visible.Add(child);
            }

            var pending = Math.Max(0, options.pendingCount?.Invoke(parentId) ?? 0);
            var setSize = visible.Count + pending;
            for (var index = 0; index < visible.Count; index++)
            {
                var child = visible[index];
                var id = options.getId(child);
                var expandable = options.hasChildren(child);
                var expanded = expandable && options.isExpanded(id);
                result.rows.Add(new TreeListRow<T>
                {
                    id = id,
                    parentId = parentId,
                    level = level,
                    expandable = expandable,
                    expanded = expanded,
                    setSize = setSize,
                    posInSet = index + 1,
                    item = child,
                });
                if (!expanded)
                    continue;
                if (options.isPending != null && options.isPending(child))
                    result.rows.Add(Placeholder(options, child, id, level + 1));
                else
                    Walk(options, result, seen, child, id, level + 1);
            }

            // Keyed by parent and position among the children still to load: no other placeholder
            // shares the id, and loaded rows are keyed apart from placeholders.
            for (var i = 0; i < pending; i++)
            {
                result.rows.Add(new TreeListRow<T>
                {
                    id = (parentId ?? string.Empty) + "#" + i,
                    parentId = parentId,
                    level = level,
                    placeholder = true,
                    placeholderCount = 1,
                    pendingIndex = i,
                    setSize = setSize,
                    posInSet = visible.Count + i + 1,
                });
            }
        }
    }
}
