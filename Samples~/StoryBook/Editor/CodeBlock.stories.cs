using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEngine.UIElements;
using Toggle = Unity.AppUI.UI.Toggle;

namespace Unity.AppUI.Editor
{
    /// <summary>
    /// Demonstrates the App UI <see cref="CodeBlock"/> component. Syntax highlighting is
    /// resolved entirely from USS — see the <c>--codeblock-grammar</c> /
    /// <c>--codeblock-theme</c> custom properties in
    /// <c>PackageResources/Styles/Components/CodeBlock.uss</c> for the grammar mappings
    /// shipped with the package (csharp, lua, css, xml).
    /// </summary>
    public class CodeBlockPage : StoryBookPage
    {
        const string k_StylesheetGuid = "c85626e5adf845eda502f5d75ebd6fe3";

        const string k_RootClass = "appui-codeblock-story";
        const string k_HeadingClass = "appui-codeblock-story__heading";
        const string k_ScrollClass = "appui-codeblock-story__scroll";

        public override string displayName => "CodeBlock";

        public override Type componentType => null;

        const string k_CSharpSample =
            "// Greeter — produces friendly greetings for the App UI sample suite.\n" +
            "using System;\n" +
            "using System.Collections.Generic;\n" +
            "\n" +
            "namespace Unity.AppUI.Sample\n" +
            "{\n" +
            "    public sealed class Greeter\n" +
            "    {\n" +
            "        readonly List<string> m_Names = new List<string>();\n" +
            "\n" +
            "        public int Count => m_Names.Count;\n" +
            "\n" +
            "        public void Add(string name)\n" +
            "        {\n" +
            "            if (string.IsNullOrWhiteSpace(name))\n" +
            "                throw new ArgumentException(nameof(name));\n" +
            "            m_Names.Add(name);\n" +
            "        }\n" +
            "\n" +
            "        public string Greet(int index)\n" +
            "        {\n" +
            "            return $\"Hello, {m_Names[index]}!\";\n" +
            "        }\n" +
            "    }\n" +
            "}\n";

        const string k_LuaSample =
            "-- Lua sample\n" +
            "local function greet(name)\n" +
            "    print(\"Hello, \" .. name .. \"!\")\n" +
            "end\n" +
            "\n" +
            "greet(\"world\")\n";

        const string k_CssSample =
            "/* CSS sample */\n" +
            ".container {\n" +
            "    display: flex;\n" +
            "    flex-direction: column;\n" +
            "    color: #4FC1FF;\n" +
            "}\n";

        const string k_XmlSample =
            "<!-- XML sample -->\n" +
            "<root>\n" +
            "    <item id=\"1\" name=\"alpha\" />\n" +
            "    <item id=\"2\" name=\"beta\" />\n" +
            "</root>\n";

        const string k_JsonSample =
            "{\n" +
            "    \"name\": \"App UI\",\n" +
            "    \"version\": \"2.2.0\",\n" +
            "    \"enabled\": true,\n" +
            "    \"tags\": [\"ui\", \"unity\", \"design-system\"],\n" +
            "    \"settings\": {\n" +
            "        \"theme\": \"dark\",\n" +
            "        \"scale\": 1.0,\n" +
            "        \"locale\": null\n" +
            "    }\n" +
            "}\n";

        public CodeBlockPage()
        {
            m_Stories.Add(new StoryBookStory("Plain (no language)", PlainStory));
            m_Stories.Add(new StoryBookStory("C#", CSharpStory));
            m_Stories.Add(new StoryBookStory("Lua", LuaStory));
            m_Stories.Add(new StoryBookStory("CSS", CssStory));
            m_Stories.Add(new StoryBookStory("XML", XmlStory));
            m_Stories.Add(new StoryBookStory("JSON", JsonStory));
            m_Stories.Add(new StoryBookStory("With Line Numbers", WithLineNumbersStory));
            m_Stories.Add(new StoryBookStory("Live Editor", LiveEditorStory));
        }

        static VisualElement PlainStory() => BuildCard(
            header: "Plain CodeBlock (no language → no highlighting)",
            body: new CodeBlock { code = k_CSharpSample });

        static VisualElement CSharpStory() => BuildCard(
            header: "language = csharp",
            body: new CodeBlock { language = "csharp", code = k_CSharpSample });

        static VisualElement LuaStory() => BuildCard(
            header: "language = lua",
            body: new CodeBlock { language = "lua", code = k_LuaSample });

        static VisualElement CssStory() => BuildCard(
            header: "language = css",
            body: new CodeBlock { language = "css", code = k_CssSample });

        static VisualElement XmlStory() => BuildCard(
            header: "language = xml",
            body: new CodeBlock { language = "xml", code = k_XmlSample });

        static VisualElement JsonStory() => BuildCard(
            header: "language = json",
            body: new CodeBlock { language = "json", code = k_JsonSample });

        static VisualElement WithLineNumbersStory() => BuildCard(
            header: "showLineNumbers = true",
            body: new CodeBlock
            {
                language = "csharp",
                showLineNumbers = true,
                code = k_CSharpSample,
            });

        // ------------------------------------------------------------------ live editor

        static readonly string[] k_LiveLanguages = { "plain", "csharp", "lua", "css", "xml", "json", "html" };
        static readonly string[] k_LiveThemes = { "dark", "light" };

        const string k_LiveThemeDarkClass = "appui-codeblock-story__live-preview--theme-dark";
        const string k_LiveThemeLightClass = "appui-codeblock-story__live-preview--theme-light";

        static VisualElement LiveEditorStory()
        {
            var root = new VisualElement();
            root.AddToClassList("appui-codeblock-story__live");
            AttachStylesheet(root);

            // ---- Preview (declared first so the input toolbar can reference it) -
            var preview = new CodeBlock
            {
                language = "csharp",
                showLineNumbers = true,
                code = k_CSharpSample,
            };
            preview.AddToClassList("appui-codeblock-story__live-output");
            preview.AddToClassList(k_LiveThemeDarkClass);

            // ---- Input pane (left) ------------------------------------------------
            var inputPane = new VisualElement();
            inputPane.AddToClassList("appui-codeblock-story__live-pane");
            root.Add(inputPane);

            var inputLabel = new Heading("Source") { size = HeadingSize.S };
            inputLabel.AddToClassList("appui-codeblock-story__live-label");
            inputPane.Add(inputLabel);

            // Toolbar (language + theme dropdowns) — controls live with the input.
            var toolbar = new VisualElement();
            toolbar.AddToClassList("appui-codeblock-story__live-toolbar");
            inputPane.Add(toolbar);

            var languageList = new List<string>(k_LiveLanguages);
            var languageDropdown = new Dropdown(
                items: languageList,
                bindItemFunc: (item, i) => item.label = languageList[i],
                bindTitleFunc: (item, indices) =>
                {
                    var idx = indices?.FirstOrDefault() ?? 0;
                    item.label = "lang: " + languageList[idx];
                },
                defaultIndices: new[] { 1 });
            languageDropdown.RegisterValueChangedCallback(evt =>
            {
                var idx = evt.newValue?.FirstOrDefault() ?? 0;
                var lang = languageList[idx];
                preview.language = lang == "plain" ? null : lang;
            });
            toolbar.Add(languageDropdown);

            var themeList = new List<string>(k_LiveThemes);
            var themeDropdown = new Dropdown(
                items: themeList,
                bindItemFunc: (item, i) => item.label = themeList[i],
                bindTitleFunc: (item, indices) =>
                {
                    var idx = indices?.FirstOrDefault() ?? 0;
                    item.label = "theme: " + themeList[idx];
                },
                defaultIndices: new[] { 0 });
            themeDropdown.RegisterValueChangedCallback(evt =>
            {
                var idx = evt.newValue?.FirstOrDefault() ?? 0;
                var dark = themeList[idx] == "dark";
                preview.EnableInClassList(k_LiveThemeDarkClass, dark);
                preview.EnableInClassList(k_LiveThemeLightClass, !dark);
            });
            toolbar.Add(themeDropdown);

            var lineNumbersToggle = new Toggle { label = "Line numbers", value = preview.showLineNumbers };
            lineNumbersToggle.RegisterValueChangedCallback(evt => preview.showLineNumbers = evt.newValue);
            toolbar.Add(lineNumbersToggle);

            var input = new TextArea
            {
                value = k_CSharpSample,
                placeholder = "Type some source code…",
            };
            input.AddToClassList("appui-codeblock-story__live-input");
            input.RegisterValueChangingCallback(evt => preview.code = evt.newValue ?? string.Empty);
            inputPane.Add(input);

            // ---- Output pane (right) ----------------------------------------------
            var outputPane = new VisualElement();
            outputPane.AddToClassList("appui-codeblock-story__live-pane");
            outputPane.AddToClassList("appui-codeblock-story__live-pane--right");
            root.Add(outputPane);

            var outputLabel = new Heading("Preview") { size = HeadingSize.S };
            outputLabel.AddToClassList("appui-codeblock-story__live-label");
            outputPane.Add(outputLabel);

            outputPane.Add(preview);

            return root;
        }

        // ------------------------------------------------------------------ helpers

        static VisualElement BuildCard(string header, VisualElement body)
        {
            var root = new VisualElement();
            root.AddToClassList(k_RootClass);
            AttachStylesheet(root);

            var heading = new Heading(header) { size = HeadingSize.S };
            heading.AddToClassList(k_HeadingClass);
            root.Add(heading);

            var scroll = new ScrollView
            {
                horizontalScrollerVisibility = ScrollerVisibility.Auto,
                verticalScrollerVisibility = ScrollerVisibility.Auto,
            };
            scroll.AddToClassList(k_ScrollClass);
            scroll.Add(body);
            root.Add(scroll);
            return root;
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
    }
}
