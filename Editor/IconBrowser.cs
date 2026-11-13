using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using HelpBox = UnityEngine.UIElements.HelpBox;
using HelpBoxMessageType = UnityEngine.UIElements.HelpBoxMessageType;
using Object = UnityEngine.Object;
using Toggle = UnityEngine.UIElements.Toggle;

namespace Unity.AppUI.Editor
{
    class IconBrowser : EditorWindow
    {
        const string k_RegularVariant = "regular";

        const string k_FillVariant = "fill";

        const string k_BoldVariant = "bold";

        [UnityEditor.MenuItem("Window/App UI/Icon Browser", priority = 2111)]
        public static void ShowWindow()
        {
            var window = GetWindow<IconBrowser>();
            window.titleContent = new GUIContent("App UI - Icon Browser");
            window.minSize = new Vector2(740, 380);
            window.Show();
        }

        [SerializeField]
        StyleSheet styleSheetAsset;

        [SerializeField]
        string tempContent;

        void CreateGUI()
        {
            var root = rootVisualElement;
            root.AddToClassList("icon-browser");
            root.RegisterCallback<GeometryChangedEvent>(OnWindowResized);
            var panel = new VisualElement();
            panel.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.unity.dt.app-ui/Editor/IconBrowser.uss"));
            panel.AddToClassList("appui");
            panel.AddToClassList(EditorGUIUtility.isProSkin ? "appui--editor-dark" : "appui--editor-light");
            panel.AddToClassList("appui--compact");

            // create UI
            var header = new Label("Choose or create a new style sheet to browse its icons.\n" +
                "Then you can add new icons by dragging and dropping them from the Project panel, or remove existing ones.");
            header.AddToClassList("header");
            root.Add(header);
            var hint = new HelpBox("Select or create a new style sheet to browse icons", HelpBoxMessageType.Info);
            root.Add(hint);
            var objectField = new ObjectField("Style Sheet")
            {
                objectType = typeof(StyleSheet),
                allowSceneObjects = false,
            };
            root.Add(objectField);

            // Restore last used stylesheet
            var lastUsedStyleSheet = IconBrowserSettings.instance.LastUsedStyleSheet;
            if (lastUsedStyleSheet)
            {
                objectField.SetValueWithoutNotify(lastUsedStyleSheet);
                styleSheetAsset = lastUsedStyleSheet;
                tempContent = System.IO.File.ReadAllText(GetStyleSheetPath(lastUsedStyleSheet));
            }

            var generateButton = new Button(OnGenerateButtonClicked)
            {
                text = "Create a new App UI Icons style sheet"
            };
            root.Add(generateButton);

            var searchBar = new ToolbarSearchField { name = "search" };
            searchBar.RegisterValueChangedCallback(OnSearchTextChanged);
            searchBar.AddToClassList("search");
            root.Add(searchBar);

            var gridView = new GridView();
            gridView.AddToClassList("grid-view");
            gridView.dragger.acceptStartDrag = _ => false;
            gridView.dragger.acceptDrag = () => false;
            gridView.style.flexGrow = 1;
            gridView.makeItem = () => new IconEntryElement();
            gridView.bindItem = (element, i) => ((IconEntryElement)element).icon = (IconEntry)gridView.itemsSource[i];
            gridView.itemHeight = 100;
            gridView.columnCount = 4;
            gridView.selectionType = SelectionType.Multiple;
            root.Add(panel);
            panel.Add(gridView);

            var footer = new VisualElement { name = "footer" };
            footer.AddToClassList("footer");
            root.Add(footer);
            var msg = new Label { name = "message" };
            msg.AddToClassList("footer__message");
            footer.Add(msg);

            var buttonsGroup = new VisualElement();
            buttonsGroup.AddToClassList("footer__buttons");
            footer.Add(buttonsGroup);

            var showRequiredIconsButton = new Toggle("Show required icons") { name = "show-required-icons" };
            showRequiredIconsButton.AddToClassList("footer__checkbox");
            showRequiredIconsButton.RegisterValueChangedCallback(OnShowRequiredIconsClicked);

            var addIconsButton = new Button(OnAddIconsClicked);
            addIconsButton.name = "add";
            addIconsButton.text = "Add icons...";
            addIconsButton.AddToClassList("footer__button");

            var saveFileButton = new Button(SaveFile);
            saveFileButton.name = "save";
            saveFileButton.text = "Save file";
            saveFileButton.AddToClassList("footer__button");

            var saveAsButton = new Button(SaveAsFile);
            saveAsButton.name = "save-as";
            saveAsButton.text = "Save as...";
            saveAsButton.AddToClassList("footer__button");

            buttonsGroup.Add(showRequiredIconsButton);
            buttonsGroup.Add(addIconsButton);
            buttonsGroup.Add(saveFileButton);
            buttonsGroup.Add(saveAsButton);

            // setup data binding
            panel.RegisterCallback<DragEnterEvent>(OnDragEnter);
            panel.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            panel.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            panel.RegisterCallback<DragPerformEvent>(OnDragPerform);
            objectField.RegisterValueChangedCallback(OnSelectedStyleSheetChanged);
            gridView.RegisterCallback<GeometryChangedEvent>(OnGridViewGeometryChanged);
            gridView.RegisterCallback<KeyUpEvent>(OnGridViewKeyUp);
            gridView.RegisterCallback<PointerUpEvent>(OnContextClicked);

            // refresh UI
            RefreshUI();
        }

        void OnWindowResized(GeometryChangedEvent _)
        {
            var searchBar = rootVisualElement.Q<ToolbarSearchField>("search");
            searchBar.style.width = rootVisualElement.contentRect.width - 7f;
        }

        void OnSearchTextChanged(ChangeEvent<string> evt)
        {
            RefreshUI();
        }

        void OnContextClicked(PointerUpEvent evt)
        {
            if (evt.button == (int) MouseButton.RightMouse)
            {
                evt.StopPropagation();

                var menu = new GenericMenu();
                var gridView = rootVisualElement.Q<GridView>();
                var selectedIndices = gridView.selectedIndices.ToList();
                var source = gridView.itemsSource;
                var canDelete = source != null && selectedIndices
                    .Select(i => (IconEntry)source[i])
                    .Any(ie => k_RequiredIcons.All(icn => icn.path != ie.path));

                if (canDelete)
                    menu.AddItem(new GUIContent("Delete selected icons"), false, DeleteSelectedIcons);
                else
                    menu.AddDisabledItem(new GUIContent("Delete selected icons"), false);
                menu.ShowAsContext();
            }
        }

        void DeleteSelectedIcons()
        {
            var gridView = rootVisualElement.Q<GridView>();
            var selectedIndices = gridView.selectedIndices.ToList();

            if (selectedIndices.Count > 0)
            {
#if ENABLE_ENTITY_ID
                var confirm = EditorDialog.DisplayDecisionDialog("Delete selected icons",
                    "Are you sure you want to delete the selected icons from the stylesheet?", "Yes", "No", DialogIconType.Warning);
#else
                var confirm = EditorUtility.DisplayDialog("Delete selected icons",
                    "Are you sure you want to delete the selected icons from the stylesheet?", "Yes", "No");
#endif
                if (confirm)
                {
                    var source = gridView.itemsSource.Cast<IconEntry>().ToList();
                    var toDelete = selectedIndices.Select(i => source[i])
                        .Where(ie => k_RequiredIcons.All(icn1 => icn1.path != ie.path)).ToList();

                    if (toDelete.Count == 0)
                        return;

                    var additionalIcons = ParseStyleSheet(tempContent);
                    var newIcons = additionalIcons
                        .Where(icn => toDelete.FindIndex(icn2 => icn2.path == icn.path) < 0).ToList();
                    tempContent = GetStyleSheetContent(newIcons);
                    RefreshUI();
                }
            }
        }

        void OnDragEnter(DragEnterEvent evt)
        {
            evt.StopPropagation();
        }

        void OnDragLeave(DragLeaveEvent evt)
        {
            evt.StopPropagation();
        }

        void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (DragAndDrop.objectReferences.Length > 0 && DragAndDrop.objectReferences.Any(IsIconAsset))
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            else
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

            evt.StopPropagation();
        }

        void OnDragPerform(DragPerformEvent evt)
        {
            if (DragAndDrop.objectReferences.Length > 0 && DragAndDrop.objectReferences.Any(IsIconAsset))
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            else
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

            if (DragAndDrop.objectReferences.Length > 0)
            {
                var additionalIcons = ParseStyleSheet(tempContent);
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (IsIconAsset(obj))
                    {
                        var path = AssetDatabase.GetAssetPath(obj);
                        var folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));
                        var iconName = ToKebabCase(obj.name);
                        var iconVariant = folderName.ToLowerInvariant();
                        var variants = Enum.GetNames(typeof(IconVariant)).Select(iv => iv.ToLowerInvariant()).ToList();
                        if (!variants.Contains(iconVariant))
                            iconVariant = k_RegularVariant;
                        if (additionalIcons.FindIndex(icn => icn.path == path) < 0)
                            additionalIcons.Add(new IconEntry
                            {
                                name = iconName,
                                variant = iconVariant,
                                path = path
                            });
                    }
                }
                DragAndDrop.AcceptDrag();
                tempContent = GetStyleSheetContent(additionalIcons);
                RefreshUI();
            }

            evt.StopPropagation();
        }

        void OnGridViewKeyUp(KeyUpEvent evt)
        {
            if (evt.keyCode is KeyCode.Delete or KeyCode.Backspace)
            {
                evt.StopPropagation();
                DeleteSelectedIcons();
            }
        }

        void OnAddIconsClicked()
        {
            // Both asset types are valid icons (see IsIconAsset), but the Object
            // Selector filters on a single type, so offer one entry per type
            // instead of leaving the texture path reachable by drag & drop only.
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add a single icon from a vector image..."), false,
                () => OnAddSingleIconClicked(typeof(VectorImage)));
            menu.AddItem(new GUIContent("Add a single icon from a texture..."), false,
                () => OnAddSingleIconClicked(typeof(Texture2D)));

            menu.ShowAsContext();
        }

        void OnShowRequiredIconsClicked(ChangeEvent<bool> evt)
        {
            RefreshUI();
        }

        void OnAddIconsFromAppUIOnlineRepositoryClicked()
        {
            throw new NotImplementedException();
        }

        void OnAddSingleIconClicked(Type assetType)
        {
            var hiddenType = typeof(UnityEditor.Editor).Assembly.GetType( "UnityEditor.ObjectSelector" );
            var piGet = hiddenType.GetProperty( "get", BindingFlags.Public | BindingFlags.Static );
            var miShow = hiddenType.GetMethod("Show",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[]
                {
                    typeof(Texture2D),
                    typeof(Type),
                    typeof(Object),
                    typeof(bool),
#if ENABLE_ENTITY_ID
                    typeof(List<EntityId>),
#else
                    typeof(List<int>),
#endif
                    typeof(Action<Object>),
                    typeof(Action<Object>),
                    typeof(bool),
                }, Array.Empty<ParameterModifier>());

            if (piGet == null || miShow == null)
                return;

            var objectSelector = piGet.GetValue(null);
            if (objectSelector == null)
                return;

            miShow.Invoke(objectSelector, new object[]
            {
                null,
                assetType,
                null,
                false,
                null,
                new Action<Object>(OnTexturePicked),
                new Action<Object>(o => { }),
                true
            });
        }

        void OnTexturePicked(Object obj)
        {
            if (IsIconAsset(obj) && styleSheetAsset)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                var folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));
                var iconName = ToKebabCase(obj.name);
                var iconVariant = folderName.ToLowerInvariant();
                var variants = Enum.GetNames(typeof(IconVariant)).Select(iv => iv.ToLowerInvariant()).ToList();
                if (!variants.Contains(iconVariant))
                    iconVariant = k_RegularVariant;

                var additionalIcons = ParseStyleSheet(tempContent);
                if (additionalIcons.FindIndex(icn => icn.path == path) < 0)
                    additionalIcons.Add(new IconEntry
                    {
                        name = iconName,
                        variant = iconVariant,
                        path = path
                    });

                tempContent = GetStyleSheetContent(additionalIcons);
                RefreshUI();
            }
        }

        void OnGridViewGeometryChanged(GeometryChangedEvent evt)
        {
            var gridView = rootVisualElement.Q<GridView>();
            var totalWidth = gridView.scrollView.contentContainer.layout.width;
            var columnCount = Mathf.Max(1, Mathf.FloorToInt(totalWidth / 100));
            rootVisualElement.Q<GridView>().columnCount = columnCount;
        }

        [UnityEditor.MenuItem("Assets/Create/App UI/Icons Style Sheet Asset")]
        static void CreateIconsStyleSheetAsset()
        {
#if ENABLE_ENTITY_ID
            var instanceId = EntityId.None;
#else
            var instanceId = 0;
#endif
            var endAction = CreateInstance<IconBrowserActions>();
            var icon = Utils.FindTextureByType(typeof(StyleSheet));
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                instanceId,
                endAction,
                "AppUIIcons.uss",
                icon,
                null);
        }

        void OnGenerateButtonClicked()
        {
            var outPath = EditorUtility.SaveFilePanel(
                "Save App UI Icons style sheet", Application.dataPath, "AppUIIcons", "uss");
            if (string.IsNullOrEmpty(outPath))
                return;

            var adbPath = ToAdbPath(outPath);
            if (string.IsNullOrEmpty(adbPath))
            {
#if ENABLE_ENTITY_ID
                EditorDialog.DisplayAlertDialog("Invalid path",
                    "The selected path must be inside the Assets or Packages folder", "OK", DialogIconType.Error);
#else
                EditorUtility.DisplayDialog("Invalid path",
                    "The selected path must be inside the Assets or Packages folder", "OK");
#endif
                return;
            }

            System.IO.File.WriteAllText(outPath, GetStyleSheetContent(Array.Empty<IconEntry>()));

            AssetDatabase.Refresh();

            styleSheetAsset = AssetDatabase.LoadAssetAtPath<StyleSheet>(adbPath);
            IconBrowserSettings.instance.LastUsedStyleSheet = styleSheetAsset;
            tempContent = System.IO.File.ReadAllText(outPath);
            RefreshUI();
        }

        void OnSelectedStyleSheetChanged(ChangeEvent<Object> evt)
        {
            styleSheetAsset = evt.newValue as StyleSheet;
            IconBrowserSettings.instance.LastUsedStyleSheet = styleSheetAsset;
            tempContent = styleSheetAsset ? System.IO.File.ReadAllText(GetStyleSheetPath(styleSheetAsset)) : string.Empty;
            RefreshUI();
        }

        bool IsDirty => styleSheetAsset && tempContent != System.IO.File.ReadAllText(GetStyleSheetPath(styleSheetAsset));

        void RefreshUI()
        {
            var objectField = rootVisualElement.Q<ObjectField>();
            objectField.SetValueWithoutNotify(styleSheetAsset);

            rootVisualElement.Q<HelpBox>().style.display = styleSheetAsset == null ? DisplayStyle.Flex : DisplayStyle.None;

            var additionalIcons = ParseStyleSheet(tempContent);
            var gridView = rootVisualElement.Q<GridView>();
            gridView.ClearSelection();
            var searchBar = rootVisualElement.Q<ToolbarSearchField>("search");
            var search = searchBar.value.ToLowerInvariant();
            var includeRequired = rootVisualElement.Q<Toggle>("show-required-icons").value;
            if (string.IsNullOrEmpty(search))
            {
                gridView.itemsSource =
                    includeRequired ? additionalIcons.Concat(k_RequiredIcons).ToList() : additionalIcons;
            }
            else
            {
                var filteredIcons = additionalIcons.Where(icn => icn.name.ToLowerInvariant().Contains(search)).ToList();
                if (includeRequired)
                    filteredIcons.AddRange(k_RequiredIcons.Where(icn => icn.name.ToLowerInvariant().Contains(search)));
                gridView.itemsSource = filteredIcons;
            }
            gridView.SetEnabled(gridView.itemsSource is { Count: > 0 });

            var footer = rootVisualElement.Q("footer");
            footer.SetEnabled(styleSheetAsset);

            var saveButton = footer.Q<Button>("save");
            saveButton.SetEnabled(IsDirty);

            var message = footer.Q<Label>("message");
            message.text = styleSheetAsset ? $"Total of {additionalIcons.Count + k_RequiredIcons.Length} icons " +
                $"({additionalIcons.Count} additional and {k_RequiredIcons.Length} required)" : "";

            OnWindowResized(null);
        }

        void SaveFile()
        {
            if (IsDirty)
                System.IO.File.WriteAllText(GetStyleSheetPath(styleSheetAsset), tempContent);

            var saveButton = rootVisualElement.Q<Button>("save");
            saveButton.SetEnabled(false);
        }

        void SaveAsFile()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save As...", "AppUIIcons", "uss", "Select a location to save the style sheet");
            if (string.IsNullOrEmpty(path))
                return;

            System.IO.File.WriteAllText(path, tempContent);
            AssetDatabase.Refresh();

            styleSheetAsset = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            IconBrowserSettings.instance.LastUsedStyleSheet = styleSheetAsset;
            tempContent = System.IO.File.ReadAllText(path);
            RefreshUI();
        }

        /// <summary>
        /// The `d_`-prefixed dark counterpart of an icon asset, when the project holds one.
        /// </summary>
        /// <remarks>
        /// Colored icons (Unity's blue prefab cube and friends) ship a second artwork for dark
        /// surfaces as a `d_&lt;Name&gt;.svg` sibling. Monochrome glyphs have none - they tint through
        /// `--unity-image-tint-color` - so this returns null for them and they stay in `.appui`.
        /// </remarks>
        /// <param name="path"> The light asset's project path. </param>
        /// <returns> The dark asset's path, or null when there is no dark counterpart. </returns>
        static string ResolveDarkPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var dir = System.IO.Path.GetDirectoryName(path);
            var file = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(file) || file.StartsWith("d_"))
                return null;

            // Keep forward slashes: this is a project path, not a filesystem one.
            var darkPath = $"{dir.Replace('\\', '/')}/d_{file}";
            return AssetDatabase.LoadMainAssetAtPath(darkPath) != null ? darkPath : null;
        }

        static string GetStyleSheetContent(IEnumerable<IconEntry> additionalIcons)
        {
            var required = k_RequiredIcons;
            var additional = additionalIcons as IList<IconEntry> ?? additionalIcons.ToList();

            // Icons that ship both artworks are declared in `.light` / `.dark` and left out of
            // `.appui`, so no var is ever declared twice. `.appui` and `.light` are both
            // single-class selectors matching the same root panel, so an icon in both would be
            // resolved by USS declaration order alone.
            var darkPaths = new Dictionary<string, string>();
            foreach (var icon in required.Concat(additional))
            {
                var key = $"{icon.name}-{icon.variant}";
                if (darkPaths.ContainsKey(key))
                    continue;
                var darkPath = ResolveDarkPath(icon.path);
                if (!string.IsNullOrEmpty(darkPath))
                    darkPaths[key] = darkPath;
            }

            bool HasDarkVariant(IconEntry icon) => darkPaths.ContainsKey($"{icon.name}-{icon.variant}");

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine(" * This style sheet was generated by the App UI Icon Browser.");
            sb.AppendLine("*/");

            sb.AppendLine(".appui {");

            sb.AppendLine("    /* Required icons */");
            foreach (var icon in required)
            {
                if (HasDarkVariant(icon))
                    continue;
                sb.AppendLine($"    --appui-icon-{icon.name}-{icon.variant}: url(\"project:/{icon.path}\");");
            }

            sb.AppendLine("    /* Additional icons */");
            foreach (var icon in additional)
            {
                if (HasDarkVariant(icon))
                    continue;
                sb.AppendLine($"    --appui-icon-{icon.name}-{icon.variant}: url(\"project:/{icon.path}\");");
            }

            sb.AppendLine("}");
            sb.AppendLine("");

            // The two skin blocks declare the same var names against different assets, and the
            // `dark` / `light` marker classes are mutually exclusive, so they never match one
            // element and their relative order is irrelevant. Omitted entirely when no icon in the
            // sheet has a dark counterpart, which keeps a monochrome-only sheet byte-identical.
            if (darkPaths.Count > 0)
            {
                foreach (var skin in new[] { "light", "dark" })
                {
                    sb.AppendLine($".{skin} {{");
                    foreach (var icon in required.Concat(additional))
                    {
                        if (!darkPaths.TryGetValue($"{icon.name}-{icon.variant}", out var darkPath))
                            continue;
                        var assetPath = skin == "dark" ? darkPath : icon.path;
                        sb.AppendLine($"    --appui-icon-{icon.name}-{icon.variant}: url(\"project:/{assetPath}\");");
                    }

                    sb.AppendLine("}");
                    sb.AppendLine("");
                }
            }

            foreach (var icon in required)
            {
                sb.AppendLine($".appui-icon--{icon.name}--{icon.variant} {{");
                sb.AppendLine($"    --unity-image: var(--appui-icon-{icon.name}-{icon.variant});");
                sb.AppendLine("}");
                sb.AppendLine("");
            }

            foreach (var icon in additional)
            {
                sb.AppendLine($".appui-icon--{icon.name}--{icon.variant} {{");
                sb.AppendLine($"    --unity-image: var(--appui-icon-{icon.name}-{icon.variant});");
                sb.AppendLine("}");
                sb.AppendLine("");
            }

            return sb.ToString();
        }

        static List<IconEntry> ParseStyleSheet(string content)
        {
            var list = new List<IconEntry>();

            if (string.IsNullOrEmpty(content))
                return list;

            var lines = content.Split('\n');

            // The sheet may hold a `.dark` block declaring the same vars as `.light` against the
            // `d_` artwork. Those are not separate icons, and the regex below reads var names only,
            // so without this the dark lines would come back as extra icons pointing at `d_` assets
            // and re-serializing would flatten them into `.appui`. The dark counterpart is
            // re-resolved from disk at write time instead, so it self-heals if one is added later.
            var inDarkBlock = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed == ".dark {" || trimmed == ".dark{")
                {
                    inDarkBlock = true;
                    continue;
                }

                if (trimmed == "}")
                {
                    inDarkBlock = false;
                    continue;
                }

                if (inDarkBlock)
                    continue;

                if (line.Contains("url(\"project:/"))
                {
                    var regexPattern = @"--appui-icon-([\w-]+)-(\w+): url\(""project:\/(.+)""\);";
                    var match = Regex.Match(line, regexPattern, RegexOptions.None, TimeSpan.FromSeconds(2));
                    if (match.Success)
                    {
                        var path = match.Groups[3].Value;
                        if (k_RequiredIcons.Any(icon => icon.path == path))
                            continue;

                        if (list.Any(icon => icon.name == match.Groups[1].Value
                                && icon.variant == match.Groups[2].Value))
                            continue;

                        list.Add(new IconEntry
                        {
                            name = match.Groups[1].Value,
                            variant = match.Groups[2].Value,
                            path = match.Groups[3].Value
                        });
                    }
                }
            }

            return list;
        }

        static readonly IconEntry[] k_RequiredIcons = new[]
        {
            new IconEntry { name = "arrow-up", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/ArrowUp.svg" },

            new IconEntry { name = "calendar", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Calendar.svg" },

            new IconEntry { name = "caret-down", variant = k_FillVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Fill/CaretDown.svg" },
            new IconEntry { name = "caret-down", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/CaretDown.svg" },
            new IconEntry { name = "caret-left", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/CaretLeft.svg" },
            new IconEntry { name = "caret-right", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/CaretRight.svg" },
            new IconEntry { name = "caret-double-left", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/CaretDoubleLeft.svg" },
            new IconEntry { name = "caret-double-right", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/CaretDoubleRight.svg" },
            new IconEntry { name = "caret-up", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/CaretUp.svg" },

            new IconEntry { name = "check", variant = k_BoldVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Bold/Check.svg" },
            new IconEntry { name = "check-circle", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/CheckCircle.svg" },
            new IconEntry { name = "check", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Check.svg" },
            new IconEntry { name = "clock", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Clock.svg" },
            new IconEntry { name = "color-picker", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/ColorPicker.svg" },
            new IconEntry { name = "copy", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Copy.svg" },
            new IconEntry { name = "crosshair", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Crosshair.svg" },

            new IconEntry { name = "delete", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Delete.svg" },
            new IconEntry { name = "dots-three", variant = k_BoldVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Bold/DotsThree.svg" },
            new IconEntry { name = "dots-three-vertical", variant = k_BoldVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Bold/DotsThreeVertical.svg" },

            new IconEntry { name = "info", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Info.svg" },

            new IconEntry { name = "list", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/List.svg" },
            new IconEntry { name = "magnifying-glass", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/MagnifyingGlass.svg" },
            new IconEntry { name = "menu", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Menu.svg" },
            new IconEntry { name = "minus", variant = k_BoldVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Bold/Minus.svg" },
            new IconEntry { name = "minus", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Minus.svg" },

            new IconEntry { name = "paper-plane-tilt", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/PaperPlaneTilt.svg" },
            new IconEntry { name = "paperclip", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Paperclip.svg" },
            new IconEntry { name = "pen", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Pen.svg" },
            new IconEntry { name = "plus", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Plus.svg" },

            new IconEntry { name = "resize-handle", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/ResizeHandle.svg" },

            new IconEntry { name = "scene", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Scene.svg" },
            new IconEntry { name = "smiley", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Smiley.svg" },
            new IconEntry { name = "sub-menu-indicator", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/SubMenuIndicator.svg" },

            new IconEntry { name = "thumbs-down", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/ThumbsDown.svg" },
            new IconEntry { name = "thumbs-up", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/ThumbsUp.svg" },

            new IconEntry { name = "users", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Users.svg" },

            new IconEntry { name = "warning", variant = k_FillVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Fill/Warning.svg" },
            new IconEntry { name = "warning", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Warning.svg" },
            new IconEntry { name = "wrench", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Wrench.svg" },
            new IconEntry { name = "x", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/X.svg" },
            new IconEntry { name = "x-circle", variant = k_RegularVariant, path = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/XCircle.svg" },
        };

        struct IconEntry
        {
            public string name;
            public string variant;
            public string path;
        }

        class IconEntryElement : VisualElement
        {
            IconEntry m_Icon;

            readonly Image m_Image;

            readonly VisualElement m_Element;

            readonly Label m_Label;

            public IconEntry icon
            {
                get => m_Icon;
                set
                {
                    m_Icon = value;
                    RefreshUI();
                }
            }

            public IconEntryElement()
            {
                pickingMode = PickingMode.Ignore;
                AddToClassList("icon-entry");
                m_Element = new VisualElement
                {
                    pickingMode = PickingMode.Ignore
                };
                m_Element.AddToClassList("icon-entry__element");
                hierarchy.Add(m_Element);

                m_Image = new Image
                {
                    pickingMode = PickingMode.Ignore,
                };
                m_Image.AddToClassList("icon-entry__image");
                m_Element.hierarchy.Add(m_Image);

                m_Label = new Label
                {
                    pickingMode = PickingMode.Ignore,
                };
                m_Label.AddToClassList("icon-entry__label");
                m_Element.hierarchy.Add(m_Label);
            }

            void RefreshUI()
            {
                m_Image.image = null;
                m_Image.vectorImage = null;
                switch (AssetDatabase.LoadAssetAtPath<Object>(m_Icon.path))
                {
                    case VectorImage vectorImage: m_Image.vectorImage = vectorImage; break;
                    case Texture2D texture: m_Image.image = texture; break;
                }
                m_Label.text = $"{m_Icon.name}";
            }
        }

        /// <summary>
        /// Whether an asset can back an Icon. App UI's own icons are SVGs imported as
        /// <see cref="VectorImage"/>; textures are still accepted for custom icon sets.
        /// </summary>
        /// <param name="obj">The dragged or picked asset.</param>
        /// <returns>True if the asset can be referenced by an Icon USS class.</returns>
        static bool IsIconAsset(Object obj) => obj is VectorImage or Texture2D;

        static string ToKebabCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return Regex.Replace(
                    value,
                    "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])",
                    "-$1",
                    RegexOptions.Compiled, TimeSpan.FromSeconds(2))
                .Trim()
                .ToLower()
                .Replace(".", "-");
        }

        static string ToAdbPath(string absolutePath)
        {
            var projectPath = Application.dataPath[..^"Assets".Length];
            var adbPath = absolutePath;
            if (adbPath.Length > projectPath.Length)
                adbPath = adbPath[projectPath.Length..];

            if (!adbPath.StartsWith("Assets") && !adbPath.StartsWith("Packages"))
            {
                Debug.LogError("Invalid path: " + adbPath);
                return null;
            }

            return adbPath;
        }

        static string GetStyleSheetPath(StyleSheet styleSheetAsset)
        {
            if (!styleSheetAsset)
                return null;

            var path = AssetDatabase.GetAssetPath(styleSheetAsset);
            if (path.StartsWith("Packages"))
            {
                // resolve path using PackageManager
                var pkgInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path);
                if (pkgInfo == null)
                {
                    Debug.LogError("Unable to read Stylesheet: Failed to resolve package info for " + path);
                    return null;
                }
                var pkgPath = pkgInfo.resolvedPath;
                var assetRelativePath = path[pkgInfo.assetPath.Length..];
                path = pkgPath + assetRelativePath;
            }

            return path;
        }

        static void HandleAction(string pathName)
        {
            System.IO.File.WriteAllText(pathName, GetStyleSheetContent(Array.Empty<IconEntry>()));
            AssetDatabase.Refresh();

            var obj = AssetDatabase.LoadAssetAtPath<StyleSheet>(pathName);
            Selection.activeObject = obj;
        }

#if ENABLE_ENTITY_ID
        class IconBrowserActions : AssetCreationEndAction
        {
            public override void Action(EntityId entityId, string pathName, string resourceFile)
            {
                HandleAction(pathName);
            }
        }
#else
        class IconBrowserActions : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                HandleAction(pathName);
            }
        }
#endif
    }
}
