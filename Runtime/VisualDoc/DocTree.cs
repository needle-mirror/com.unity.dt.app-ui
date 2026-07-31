using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
#endif

namespace Unity.AppUI.VisualDoc
{
    /// <summary>
    /// Builds the documentation navigation tree dynamically: guide pages come from the
    /// package's <c>Documentation~</c> table of contents, component pages come from the
    /// compile-time <see cref="VisualDocRegistry"/>.
    /// </summary>
    static class DocTree
    {
        internal const string documentationRoot = "Packages/com.unity.dt.app-ui/Documentation~";

        static readonly (string slug, string displayName)[] k_ComponentCategories =
        {
            ("actions", "Actions"),
            ("inputs", "Inputs"),
            ("layouts", "Layouts"),
            ("feedbacks", "Feedbacks"),
            ("typography", "Typography"),
            ("iconography", "Iconography"),
            ("context-components", "Context Components"),
            ("drag-and-drop", "Drag And Drop"),
            ("nav-components", "Navigation"),
            ("popups", "Popups"),
        };

        static List<TreeViewItemModel> s_Tree;

        /// <summary>
        /// The documentation tree root items.
        /// </summary>
        public static List<TreeViewItemModel> BuildTree()
        {
            if (s_Tree != null)
                return s_Tree;

            var roots = new List<TreeViewItemModel>();
#if UNITY_EDITOR
            roots.AddRange(BuildGuideTree());
#endif
            roots.Add(BuildComponentsTree());
            s_Tree = roots;
            return s_Tree;
        }

        static TreeViewItemModel BuildComponentsTree()
        {
            var componentsRoot = new TreeViewItemModel("components", "Components", isCategory: true);
            var categories = new Dictionary<string, TreeViewItemModel>();

            TreeViewItemModel GetCategory(string slug)
            {
                if (categories.TryGetValue(slug, out var existing))
                    return existing;

                var known = k_ComponentCategories.FirstOrDefault(c => c.slug == slug);
                var displayName = known.displayName ?? ToDisplayName(slug);
                var category = new TreeViewItemModel("category-" + slug, displayName);
                category.SetParent(componentsRoot);
                categories[slug] = category;
                return category;
            }

            foreach (var page in VisualDocRegistry.pages)
            {
                if (k_ComponentCategories.All(c => c.slug != page.category))
                    Debug.LogWarning($"Unknown Visual Documentation category '{page.category}' for page '{page.id}'.");
            }

            // Instantiate the known categories first so they keep their curated order.
            foreach (var (slug, _) in k_ComponentCategories)
            {
                if (VisualDocRegistry.pages.Any(p => p.category == slug))
                    GetCategory(slug);
            }

            foreach (var page in VisualDocRegistry.pages.OrderBy(p => p.displayName, System.StringComparer.OrdinalIgnoreCase))
            {
                var item = new TreeViewItemModel(
                    page.id,
                    page.displayName,
                    isNew: page.isNew,
                    pageKind: PageKind.Component,
                    contentRef: page.id);
                item.SetParent(GetCategory(page.category));
            }

            return componentsRoot;
        }

        static string ToDisplayName(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return "Other";

            return string.Join(" ", slug
                .Split('-')
                .Where(part => part.Length > 0)
                .Select(part => char.ToUpperInvariant(part[0]) + part.Substring(1)));
        }

#if UNITY_EDITOR
        static readonly Regex k_TocEntry = new Regex(
            @"^(?<indent>\s*)\*\s+(?:\[(?<label>[^\]]+)\]\(xref:(?<uid>[\w\-\.]+)\)|(?<category>.+?))\s*$",
            RegexOptions.Compiled);

        static readonly Regex k_FrontMatterUid = new Regex(
            @"^uid:\s*(?<uid>[\w\-\.]+)\s*$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        static List<TreeViewItemModel> BuildGuideTree()
        {
            var roots = new List<TreeViewItemModel>();
            var docPath = Path.GetFullPath(documentationRoot);
            if (!Directory.Exists(docPath))
                return roots;

            var tocPath = Path.Combine(docPath, "TableOfContents.md");
            if (!File.Exists(tocPath))
                return roots;

            var uidToFile = new Dictionary<string, string>();
            foreach (var file in Directory.EnumerateFiles(docPath, "*.md"))
            {
                var match = k_FrontMatterUid.Match(ReadHead(file));
                if (match.Success)
                    uidToFile[match.Groups["uid"].Value] = Path.GetFileName(file);
            }

            // indentation level -> last item created at that level
            var lastAtDepth = new Dictionary<int, TreeViewItemModel>();
            foreach (var line in File.ReadAllLines(tocPath))
            {
                var match = k_TocEntry.Match(line);
                if (!match.Success)
                    continue;

                var depth = match.Groups["indent"].Value.Replace("\t", "  ").Length / 2;
                TreeViewItemModel item;
                if (match.Groups["uid"].Success)
                {
                    var uid = match.Groups["uid"].Value;
                    if (!uidToFile.TryGetValue(uid, out var file))
                    {
                        Debug.LogWarning($"Table of contents entry '{uid}' has no matching markdown file in Documentation~.");
                        continue;
                    }

                    item = new TreeViewItemModel(
                        "guide-" + uid,
                        match.Groups["label"].Value,
                        pageKind: PageKind.Guide,
                        contentRef: file);
                }
                else
                {
                    var label = match.Groups["category"].Value;
                    item = new TreeViewItemModel(
                        "guide-category-" + ToSlug(label),
                        label,
                        isCategory: depth == 0);
                }

                if (depth > 0 && lastAtDepth.TryGetValue(depth - 1, out var parent))
                    item.SetParent(parent);
                else
                    roots.Add(item);

                lastAtDepth[depth] = item;
            }

            return roots;
        }

        static string ReadHead(string file)
        {
            using var reader = new StreamReader(file);
            var sb = new System.Text.StringBuilder();
            for (var i = 0; i < 8 && !reader.EndOfStream; i++)
                sb.AppendLine(reader.ReadLine());
            return sb.ToString();
        }

        static string ToSlug(string label)
        {
            return string.Join("-", label.ToLowerInvariant().Split(' ', '/', '\\'));
        }
#endif
    }
}
