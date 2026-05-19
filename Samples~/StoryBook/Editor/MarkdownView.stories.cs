#if APPUI_ENABLE_MARKDOWN
using System;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.AppUI.Editor
{
    public class MarkdownViewPage : StoryBookPage
    {
        public override string displayName => "MarkdownView";

        public override Type componentType => null;

        public MarkdownViewPage()
        {
            m_Stories.Add(new StoryBookStory("Headings", HeadingsStory));
            m_Stories.Add(new StoryBookStory("Inline Emphasis", InlineEmphasisStory));
            m_Stories.Add(new StoryBookStory("Lists", ListsStory));
            m_Stories.Add(new StoryBookStory("Blockquote", BlockquoteStory));
            m_Stories.Add(new StoryBookStory("Code Block", CodeBlockStory));
            m_Stories.Add(new StoryBookStory("Thematic Break", ThematicBreakStory));
            m_Stories.Add(new StoryBookStory("Kitchen Sink", KitchenSinkStory));
            m_Stories.Add(new StoryBookStory("Live Editor", LiveEditorStory));
        }

        const string k_StylesheetGuid = "dcc331107dce43d1956d70254b9b84e0";
        const string k_ScrollClass = "appui-markdown-story__scroll";

        static VisualElement Wrap(MarkdownView view)
        {
            var scroll = new ScrollView
            {
                verticalScrollerVisibility = ScrollerVisibility.Auto,
            };
            scroll.AddToClassList(k_ScrollClass);
            AttachStylesheet(scroll);
            scroll.Add(view);
            return scroll;
        }

        static void AttachStylesheet(VisualElement root)
        {
            var path = AssetDatabase.GUIDToAssetPath(k_StylesheetGuid);
            if (string.IsNullOrEmpty(path))
                return;
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            if (sheet != null)
                root.styleSheets.Add(sheet);
        }

        static VisualElement HeadingsStory() => Wrap(new MarkdownView
        {
            content =
                "# Heading 1 (H1 → XXL)\n\n" +
                "## Heading 2 (H2 → XL)\n\n" +
                "### Heading 3 (H3 → L)\n\n" +
                "#### Heading 4 (H4 → M)\n\n" +
                "##### Heading 5 (H5 → S)\n\n" +
                "###### Heading 6 (H6 → XS)\n",
        });

        static VisualElement InlineEmphasisStory() => Wrap(new MarkdownView
        {
            content =
                "Plain text with **bold**, *italic*, ***bold-italic***, " +
                "`inline code`, and a [Unity App UI link](https://unity.com).\n\n" +
                "Hard break (two trailing spaces):  \nsecond line.\n\n" +
                "Autolink: <https://docs.unity3d.com/Manual/UIElements.html>\n",
        });

        static VisualElement ListsStory() => Wrap(new MarkdownView
        {
            content =
                "Unordered list:\n\n" +
                "- First item\n" +
                "- Second item with **emphasis** and `code`\n" +
                "- Nested\n" +
                "  - child a\n" +
                "  - child b\n" +
                "- Last\n\n" +
                "Ordered list:\n\n" +
                "1. Step one\n" +
                "2. Step two\n" +
                "3. Step three\n",
        });

        static VisualElement BlockquoteStory() => Wrap(new MarkdownView
        {
            content =
                "Some context above the quote.\n\n" +
                "> A wise person once said:\n" +
                ">\n" +
                "> *Markdown is just structured plain text.*\n\n" +
                "And life goes on.\n",
        });

        static VisualElement CodeBlockStory() => Wrap(new MarkdownView
        {
            content =
                "Fenced code blocks pick up syntax highlighting automatically when " +
                "a `csharp`/`lua`/`css`/`xml` info string matches one of the grammar " +
                "mappings declared in `Components/CodeBlock.uss`:\n\n" +
                "```csharp\n" +
                "public sealed class Greeter\n" +
                "{\n" +
                "    public static void SayHello(string name)\n" +
                "    {\n" +
                "        UnityEngine.Debug.Log($\"Hello, {name}!\");\n" +
                "    }\n" +
                "}\n" +
                "```\n\n" +
                "```lua\n" +
                "local function greet(name)\n" +
                "    print(\"Hello, \" .. name .. \"!\")\n" +
                "end\n" +
                "\n" +
                "greet(\"world\")\n" +
                "```\n\n" +
                "Indented code block (no language → plain monospace):\n\n" +
                "    int x = 1;\n" +
                "    int y = 2;\n" +
                "    return x + y;\n",
        });

        static VisualElement ThematicBreakStory() => Wrap(new MarkdownView
        {
            content =
                "Section A — short paragraph.\n\n" +
                "---\n\n" +
                "Section B — separated from Section A by a horizontal divider.\n",
        });

        static VisualElement KitchenSinkStory() => Wrap(new MarkdownView
        {
            content =
                "# Project Update\n\n" +
                "Hi team — here's a quick summary of where we landed this week.\n\n" +
                "## Highlights\n\n" +
                "- Shipped the **MarkdownView** component (App UI UITK).\n" +
                "- Switched the chat surface to use it for assistant responses.\n" +
                "- *Stretch goal:* tables and footnotes (deferred).\n\n" +
                "### Code change\n\n" +
                "```csharp\n" +
                "var view = new MarkdownView { content = response };\n" +
                "panel.Add(view);\n" +
                "```\n\n" +
                "### Decision log\n\n" +
                "1. Use Markdig as the parser.\n" +
                "2. Render to App UI components (no plain UITK fallbacks).\n" +
                "3. Gate behind `APPUI_ENABLE_MARKDOWN`.\n\n" +
                "> Reminder from product:\n" +
                ">\n" +
                "> Keep the rendered output **legible at small sizes** — especially code fences.\n\n" +
                "---\n\n" +
                "Questions? See the [docs](https://docs.unity3d.com/Manual/UIElements.html) or ping me.\n",
        });

        const string k_LiveEditorSeed =
            "# Live Markdown editor\n\n" +
            "Type on the **left**, see the rendered tree update on the **right** as you go.\n\n" +
            "- inline `code`, *italic*, **bold**\n" +
            "- a [link](https://unity.com)\n\n" +
            "```csharp\n" +
            "var view = new MarkdownView { content = source };\n" +
            "```\n\n" +
            "> Try editing this text — the view re-renders on every keystroke.\n";

        static VisualElement LiveEditorStory()
        {
            var root = new VisualElement();
            root.AddToClassList("appui-markdown-story__live");
            AttachStylesheet(root);

            // ---- Input pane (left) ------------------------------------------------
            var inputPane = new VisualElement();
            inputPane.AddToClassList("appui-markdown-story__live-pane");
            root.Add(inputPane);

            var inputLabel = new Heading("Markdown source") { size = HeadingSize.S };
            inputLabel.AddToClassList("appui-markdown-story__live-label");
            inputPane.Add(inputLabel);

            var input = new TextArea
            {
                value = k_LiveEditorSeed,
                placeholder = "Type some markdown…",
            };
            input.AddToClassList("appui-markdown-story__live-input");
            inputPane.Add(input);

            // ---- Output pane (right) ----------------------------------------------
            var outputPane = new VisualElement();
            outputPane.AddToClassList("appui-markdown-story__live-pane");
            outputPane.AddToClassList("appui-markdown-story__live-pane--right");
            root.Add(outputPane);

            var outputLabel = new Heading("Rendered output") { size = HeadingSize.S };
            outputLabel.AddToClassList("appui-markdown-story__live-label");
            outputPane.Add(outputLabel);

            var view = new MarkdownView { content = k_LiveEditorSeed };
            var scroll = new ScrollView { verticalScrollerVisibility = ScrollerVisibility.Auto };
            scroll.AddToClassList("appui-markdown-story__live-output");
            scroll.Add(view);
            outputPane.Add(scroll);

            input.RegisterValueChangingCallback(evt => view.content = evt.newValue ?? string.Empty);

            return root;
        }
    }
}
#endif
