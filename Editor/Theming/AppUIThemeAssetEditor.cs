using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

// App UI ships its own HelpBox component, so the name is ambiguous once the runtime namespace is
// in scope. This inspector is editor chrome and wants the built-in one.
using HelpBox = UnityEngine.UIElements.HelpBox;

namespace Unity.AppUI.Editor.Theming
{
    /// <summary>
    /// Inspector for an imported theme: what it defines, and how to apply it.
    /// </summary>
    [CustomEditor(typeof(AppUIThemeAsset))]
    class AppUIThemeAssetEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var theme = (AppUIThemeAsset)target;
            var root = new VisualElement { style = { paddingTop = 4, paddingBottom = 4 } };

            root.Add(Row("Theme name", theme.themeName));
            root.Add(Row("Selector", $".{AppUIThemeUssWriter.themeClassPrefix}{theme.themeName}"));
            if (!string.IsNullOrEmpty(theme.baseTheme))
                root.Add(Row("Based on", theme.baseTheme));
            root.Add(Row("Tokens", theme.tokenCount.ToString()));

            if (theme.styleSheet == null)
            {
                root.Add(new HelpBox(
                    "This theme has no compiled stylesheet, so it will not paint anything. Check " +
                    "the import errors above.", HelpBoxMessageType.Warning));
                return root;
            }

            root.Add(new HelpBox(
                "Add the stylesheet to a panel and set its theme:\n\n" +
                "    panel.styleSheets.Add(theme.styleSheet);\n" +
                $"    panel.theme = \"{theme.themeName}\";\n\n" +
                "This layers over the active App UI theme, so any token this theme does not " +
                "define keeps its value from the base theme.",
                HelpBoxMessageType.Info));

            root.Add(Swatches(theme));
            return root;
        }

        static VisualElement Row(string label, string value)
        {
            var row = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, marginBottom = 2 },
            };
            row.Add(new Label(label) { style = { width = 100, opacity = 0.7f } });
            row.Add(new Label(value) { style = { flexGrow = 1 } });
            return row;
        }

        static VisualElement Swatches(AppUIThemeAsset theme)
        {
            var container = new VisualElement { style = { marginTop = 8 } };
            var grid = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap },
            };

            var names = new List<string>(theme.tokens.Keys);
            names.Sort(System.StringComparer.Ordinal);

            foreach (var name in names)
            {
                if (!ColorUtility.TryParseHtmlString(theme.tokens[name], out var color))
                    continue;

                grid.Add(new VisualElement
                {
                    tooltip = $"{AppUIThemeUssWriter.PropertyNameOf(name)}: {theme.tokens[name]}",
                    style =
                    {
                        width = 22,
                        height = 22,
                        marginRight = 2,
                        marginBottom = 2,
                        backgroundColor = color,
                    },
                });
            }

            container.Add(grid);
            return container;
        }
    }
}
