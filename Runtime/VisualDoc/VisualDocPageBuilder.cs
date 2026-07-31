using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using System.IO;
#endif

namespace Unity.AppUI.VisualDoc
{
    /// <summary>
    /// Builds documentation pages dynamically: component pages from the compile-time
    /// <see cref="VisualDocRegistry"/>, guide pages from the package's markdown documentation.
    /// Markdown is rendered with <see cref="MarkdownView"/> and code samples with
    /// <see cref="CodeBlock"/>. The produced hierarchy reuses the USS classes of the
    /// previous UXML-based pages (title, subtitle, shields, section, table...).
    /// </summary>
    static class VisualDocPageBuilder
    {
        static readonly Regex k_H2 = new Regex(@"^##\s+(.+?)\s*$", RegexOptions.Compiled);

        static readonly Regex k_Fence = new Regex(@"^\s*```", RegexOptions.Compiled);

        static readonly Regex k_FrontMatter = new Regex(@"\A---\s*\n.*?\n---\s*\n", RegexOptions.Compiled | RegexOptions.Singleline);

        static readonly Regex k_XrefLink = new Regex(@"\[([^\]]+)\]\(xref:[^)]+\)", RegexOptions.Compiled);

        static readonly Regex k_Callout = new Regex(@"^>\s*\[!(\w+)\]\s*$", RegexOptions.Compiled | RegexOptions.Multiline);

        static readonly Regex k_HtmlTagLine = new Regex(@"^\s*(?:</?(?:p|div|a|br|video|source|picture)\b[^>]*>\s*)+$|^\s*<img\b[^>]*>?\s*$", RegexOptions.Compiled);

        static readonly Regex k_ImageLink = new Regex(@"\[!\[[^\]]*\]\([^)]*\)\]\([^)]*\)", RegexOptions.Compiled);

        static readonly Regex k_MarkdownImage = new Regex(@"!\[[^\]]*\]\([^)]*\)", RegexOptions.Compiled);

        /// <summary>
        /// Builds a component page from a registry entry.
        /// </summary>
        public static void BuildComponentPage(VisualElement container, VisualDocPageInfo page)
        {
            var title = new Label(page.displayName);
            title.AddToClassList("title");
            container.Add(title);

            if (!string.IsNullOrEmpty(page.summary))
            {
                var subtitle = new Label(page.summary);
                subtitle.AddToClassList("subtitle");
                container.Add(subtitle);
            }

            container.Add(BuildShields(page));

            var content = new VisualElement();
            content.AddToClassList("content");
            container.Add(content);

            var demos = VisualDocDemoRegistry.GetDemos(page.id);
            var consumedDemos = new HashSet<VisualDocDemo>();

            foreach (var section in page.sections)
            {
                var sectionElement = CreateSection(content, section.title, showLabel: section.title != "Overview");
                foreach (var demo in demos)
                {
                    if (demo.section == section.title && consumedDemos.Add(demo))
                        sectionElement.Add(BuildDemoHost(demo));
                }

                sectionElement.Add(new MarkdownView { content = section.markdown });
            }

            // Demos bound to a section the doc comment does not author get their own
            // section so they stay reachable from the table of contents.
            var extraSections = new Dictionary<string, VisualElement>();
            foreach (var demo in demos)
            {
                if (consumedDemos.Contains(demo))
                    continue;

                if (!extraSections.TryGetValue(demo.section, out var sectionElement))
                {
                    sectionElement = CreateSection(content, demo.section, showLabel: true);
                    extraSections[demo.section] = sectionElement;
                }

                sectionElement.Add(BuildDemoHost(demo));
            }

            if (page.properties.Count > 0)
                BuildApiReference(content, page);

            if (page.examples.Count > 0)
            {
                var section = CreateSection(content, "Examples", showLabel: true);
                var examples = new VisualElement();
                examples.AddToClassList("examples");
                section.Add(examples);
                foreach (var example in page.examples)
                    examples.Add(BuildExample(example));
            }
        }

        /// <summary>
        /// Builds a guide page from a markdown file of the package documentation.
        /// The markdown is split into sections on level-2 headings so the table of
        /// contents stays functional.
        /// </summary>
        public static void BuildGuidePage(VisualElement container, string fileName)
        {
#if UNITY_EDITOR
            var path = Path.Combine(Path.GetFullPath(DocTree.documentationRoot), fileName);
            if (!File.Exists(path))
            {
                container.Add(new Label($"Documentation file not found: {fileName}"));
                return;
            }

            var markdown = PreprocessGuideMarkdown(File.ReadAllText(path));

            var content = new VisualElement();
            content.AddToClassList("content");
            container.Add(content);

            foreach (var (heading, body) in SplitOnH2(markdown))
            {
                // The preamble (before the first level-2 heading) has no name so it does
                // not appear in the table of contents.
                var sectionElement = CreateSection(content, heading, showLabel: false);
                sectionElement.Add(new MarkdownView { content = body });
            }
#else
            container.Add(new Label("Guides are available in the Unity Editor."));
#endif
        }

        static VisualElement BuildDemoHost(VisualDocDemo demo)
        {
            var host = new VisualElement();
            host.AddToClassList("visual-example");
            try
            {
                host.Add(demo.factory());
            }
            catch (Exception e)
            {
                host.Add(new Label($"Live example failed: {e.Message}"));
            }

            return host;
        }

        static VisualElement CreateSection(VisualElement parent, string name, bool showLabel)
        {
            var section = new VisualElement { name = name };
            section.AddToClassList("section");
            parent.Add(section);
            if (showLabel && !string.IsNullOrEmpty(name))
            {
                var label = new Label(name);
                label.AddToClassList("section-label");
                section.Add(label);
            }

            return section;
        }

        static VisualElement BuildShields(VisualDocPageInfo page)
        {
            var shields = new VisualElement();
            shields.AddToClassList("shields");
            shields.Add(BuildShield("Type", CategoryDisplayName(page.category), page.category));
            shields.Add(BuildShield("C#", "Supported", "positive"));
            shields.Add(BuildShield("UXML", page.uxmlSupported ? "Supported" : "Not supported", page.uxmlSupported ? "positive" : "negative"));
            return shields;
        }

        static VisualElement BuildShield(string label, string value, string valueClass)
        {
            var shield = new VisualElement();
            shield.AddToClassList("shield");
            var labelElement = new Label(label);
            labelElement.AddToClassList("shield-label");
            shield.Add(labelElement);
            var valueElement = new Label(value);
            valueElement.AddToClassList("shield-value");
            if (!string.IsNullOrEmpty(valueClass))
                valueElement.AddToClassList(valueClass);
            shield.Add(valueElement);
            return shield;
        }

        static string CategoryDisplayName(string category)
        {
            if (string.IsNullOrEmpty(category))
                return "Component";

            var name = category.EndsWith("s") ? category.Substring(0, category.Length - 1) : category;
            return char.ToUpperInvariant(name[0]) + name.Substring(1).Replace("-", " ");
        }

        static void BuildApiReference(VisualElement content, VisualDocPageInfo page)
        {
            var section = CreateSection(content, "API Reference", showLabel: true);

            var table = new VisualElement();
            table.AddToClassList("table");
            section.Add(table);

            var header = new VisualElement();
            header.AddToClassList("table-header");
            header.Add(CreateCellLabel("Name", "table-header--name"));
            header.Add(CreateCellLabel("Type", "table-header--type"));
            header.Add(CreateCellLabel("Default", "table-header--default"));
            table.Add(header);

            var body = new VisualElement();
            body.AddToClassList("table-body");
            table.Add(body);

            foreach (var property in page.properties)
            {
                var row = new VisualElement();
                row.AddToClassList("table-row");
                row.Add(CreateCell(property.name, "prop-name-value"));
                row.Add(CreateCell(property.isEvent ? $"event {property.type}" : property.type, "prop-type-value"));
                row.Add(CreateCell(property.defaultValue ?? "—", "prop-default-value"));
                body.Add(row);
            }

            foreach (var property in page.properties)
            {
                if (string.IsNullOrEmpty(property.remarksMarkdown) && property.examples.Count == 0)
                    continue;

                var details = new VisualElement();
                details.AddToClassList("prop");
                var name = new Label(property.name);
                name.AddToClassList("prop-name-value");
                details.Add(name);
                if (!string.IsNullOrEmpty(property.summary) || !string.IsNullOrEmpty(property.remarksMarkdown))
                {
                    var description = new VisualElement();
                    description.AddToClassList("prop-description");
                    description.Add(new MarkdownView
                    {
                        content = string.IsNullOrEmpty(property.remarksMarkdown)
                            ? property.summary
                            : property.remarksMarkdown
                    });
                    details.Add(description);
                }

                foreach (var example in property.examples)
                    details.Add(BuildExample(example));

                section.Add(details);
            }
        }

        static Label CreateCellLabel(string text, string className)
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }

        static VisualElement CreateCell(string text, string valueClassName)
        {
            var cell = new VisualElement();
            cell.AddToClassList("table-cell");
            var label = new Label(text)
            {
                selection = { isSelectable = true },
            };
            label.AddToClassList(valueClassName);
            cell.Add(label);
            return cell;
        }

        static VisualElement BuildExample(VisualDocExample example)
        {
            var container = new VisualElement();
            container.AddToClassList("example");
            if (!string.IsNullOrEmpty(example.description))
            {
                var description = new Label(example.description);
                description.AddToClassList("scenario-description");
                container.Add(description);
            }

            var codeBlock = new CodeBlock
            {
                code = example.code,
                language = NormalizeLanguage(example.language),
                showLineNumbers = true,
            };
            codeBlock.AddToClassList("pre");
            codeBlock.AddToClassList("code");
            container.Add(codeBlock);
            return container;
        }

        static string NormalizeLanguage(string language)
        {
            switch (language?.ToLowerInvariant())
            {
                case "uxml":
                    return "xml";
                case null:
                case "":
                    return "csharp";
                default:
                    return language.ToLowerInvariant();
            }
        }

        /// <summary>
        /// Strips syntax that the markdown renderer does not understand: DocFX YAML front
        /// matter, xref links (kept as plain text), callout markers, and content the panel
        /// cannot display — raw HTML blocks (image wrappers, line breaks...) and images.
        /// Fenced code blocks are left untouched.
        /// </summary>
        internal static string PreprocessGuideMarkdown(string markdown)
        {
            markdown = k_FrontMatter.Replace(markdown, "");
            markdown = k_XrefLink.Replace(markdown, "$1");
            markdown = k_Callout.Replace(markdown, match => $"> **{ToTitleCase(match.Groups[1].Value)}:**");
            markdown = StripUnrenderableContent(markdown);
            return markdown.Trim('\n');
        }

        static string StripUnrenderableContent(string markdown)
        {
            var result = new System.Text.StringBuilder();
            var inFence = false;
            foreach (var line in markdown.Replace("\r\n", "\n").Split('\n'))
            {
                if (k_Fence.IsMatch(line))
                    inFence = !inFence;

                if (!inFence && !k_Fence.IsMatch(line))
                {
                    if (k_HtmlTagLine.IsMatch(line))
                        continue;

                    var stripped = k_MarkdownImage.Replace(k_ImageLink.Replace(line, ""), "");
                    if (stripped != line && stripped.Trim().Length == 0)
                        continue;

                    result.Append(stripped).Append('\n');
                    continue;
                }

                result.Append(line).Append('\n');
            }

            return result.ToString();
        }

        static string ToTitleCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return char.ToUpperInvariant(value[0]) + value.Substring(1).ToLowerInvariant();
        }

        /// <summary>
        /// Splits markdown into (heading, body) chunks on level-2 headings, ignoring
        /// headings inside fenced code blocks. The preamble is returned with an empty
        /// heading. Each chunk keeps its heading line so the renderer displays it.
        /// </summary>
        internal static List<(string heading, string body)> SplitOnH2(string markdown)
        {
            var chunks = new List<(string, string)>();
            var currentHeading = "";
            var current = new System.Text.StringBuilder();
            var inFence = false;

            void Flush()
            {
                var text = current.ToString().Trim('\n');
                if (text.Length > 0)
                    chunks.Add((currentHeading, text));
            }

            foreach (var line in markdown.Replace("\r\n", "\n").Split('\n'))
            {
                if (k_Fence.IsMatch(line))
                    inFence = !inFence;

                var match = !inFence ? k_H2.Match(line) : Match.Empty;
                if (match.Success)
                {
                    Flush();
                    currentHeading = match.Groups[1].Value;
                    current = new System.Text.StringBuilder();
                }

                current.AppendLine(line);
            }

            Flush();
            return chunks;
        }
    }
}
