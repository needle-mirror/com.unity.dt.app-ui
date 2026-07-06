---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.0] - 2026-07-06

### Added

- A reference of the latest opened StyleSheet in IconBrowser tool is now stored per project. The next time you will open the tool, it will reload the latest StyleSheet.
- Added `sizeModifier` callback property to `ResizeHandle` to allow custom size adjustments during resize.
- Added Thread UI components including Thread, ThreadMessage, ThreadComposer, ThreadReaction, ThreadReactionBar, ThreadResolveMessage, ThreadUnresolveMessage, MentionPicker, and ReactionPicker, along with their associated interfaces (IEmojiProvider, IMentionProvider, IThreadAttachment), base classes, enums, and USS styles. Renamed ThreadMessage `content` property to `message`, removed `isEditing` property in favor of `state = Draft`, moved `emojiProvider` and `mentionProvider` from ThreadMessage to ThreadContext, removed `Active` message state, and exposed explicit `attachments` container on ThreadMessage and ThreadComposer
- Added new icons (arrow-up, paperclip, pen, paper-plane-tilt, smiley, thumbs-up, thumbs-down) to the minimal icon set and IconBrowser required icons list
- Added TextMate grammar-based syntax highlighting support via native TextMateLib integration, including C# bindings (Registry, Grammar, Theme, Token), ScriptableObject assets (TextMateGrammarAsset, TextMateThemeAsset), ScriptedImporters for .tmLanguage and .tmTheme files, and a high-level TextMateSyntaxHighlighter API
- Added support of DevicePixelRatio in WebGL native plugin to determine the current ScaleFactor applied to the canvas.
- Added `UxmlTraits` support for `ResizeHandle` component.
- Added Edit and Delete text action constants with localization support in TextActions
- Added Uxml cloneTree and element binding by name source generators. See the documentation for more information.
- Added Conditional Resolution in Dependency Injection system.
- Add `destroyItem` callback to BaseGridView and fix doc comments
- Added HelpBox UI element
- Added runtime data binding support to `BaseGridView` component.
- Added the support of custom ColorPicker inside the ColorField element via customPicker property.
- Added `disableAnimation` property on Popup elements in order to force disable display animation for a given Popup UI Element.
- New `CodeBlock` UITK component (`Unity.AppUI.UI.CodeBlock`): rounded card with a header row (language label + hover-revealed copy button using `Platform.SetPasteboardData`) above a monospaced body. Syntax highlighting is configured declaratively from USS via two `CustomStyleProperty<string>` values (`--codeblock-grammar`, `--codeblock-theme`) holding TextMate asset GUIDs; setting the `language` property toggles a `appui-code-block--lang-<value>` modifier class so per-language grammar mappings can be expressed in the design system. When a `TextMateThemeAsset` resolves, the body's background and foreground colors are pulled from `Theme.GetDefaultBackground()`/`GetDefaultForeground()` so the card adopts the theme's palette.
- `IServiceCollection.AddSingleton<T>(T instance)` and `AddSingleton(Type, object)` overloads (plus matching `AddSingletonWhen(Type, object, ContextMatch)`) for registering pre-existing instances as singletons in the MVVM DI container. The supplied instance is returned as-is on every resolution; the container does not invoke a constructor or apply `[Service]` field/property injection on it.
- Added `SetPosition` and `SetSize` methods to `Popover` for programmatic positioning and sizing.
- Added `movable` property to `Popover` UI elements.
- Shipped Roboto Mono (`PackageResources/Fonts/RobotoMono-Regular.ttf`) as a runtime font asset and exposed it as a new design token `--appui-code-font-family` (declared in `Components/CodeBlock.uss`). The font was previously buried under `Editor/Editor Default Resources/Fonts/` and only reachable from editor windows; the Redux DevTools `--mono-font` reference was migrated to the new token.
- SplitViewSplitterCountChangedEvent dispatched when the number of splitters changes in a SplitView, allowing consumers to defer collapse/expand operations until splitters are ready
- Added `movable` property to `DialogTrigger` to support movable popovers via UXML.
- Bundled TextMate grammars (csharp, lua, css, xml, json) and themes (dark-plus, light-plus) under `PackageResources/SyntaxHighlighting/`. The XML and Lua grammars were stripped of their cross-language `source.java` / `source.c` includes so they tokenize standalone in our single-grammar `Registry`. Used by the new `CodeBlock` component for syntax highlighting when `APPUI_ENABLE_SYNTAX_HIGHLIGHTING` is enabled.
- Added support of system theme in WebGL. Use `Platform.darkMode` or register your callback on the `Platform.darkModeChanged` event to make changes in your application based on the current system theme.
- Added SortingOrder proeprty in Popups. This enable the possibility to insert popups in behind others exisiting ones.
- PanelRenderer support in UIToolkitAppBuilder<T> for hosting MVVM apps on Unity 6.5+
- Added support of drag-to-increment-value feature in `InputLabel` UI element.
- Added support of system clipboard in WebGL platform for text fields.
- New `MarkdownView` UITK component (`Unity.AppUI.Markdown` namespace) gated behind the `APPUI_ENABLE_MARKDOWN` scripting symbol. It parses CommonMark + GFM via the bundled Markdig 0.37.0 (`Runtime/Markdown/Plugins/Markdig.dll`) and renders into App UI components only — `Heading`, `Quote`, `Divider`, `LocalizedTextElement`, `Link`, plus the new `CodeBlock` — through a custom `RendererBase` (`UITKMarkdownRenderer`) with one object-renderer per CommonMark node type.
- Added **AI Agent Skills** pane in **Unity Package Manager UI**: You can now install/remove App UI skills for AI Agents in your workspace from the Package Manager window.
- Added support of Unity 6000.6
- UxmlCloneTreeGenerator now collects UxmlElementNameAttribute bindings from inherited base classes, respecting C# shadowing rules.
- `CodeBlock.showLineNumbers` property: when enabled, a 1-based line-number gutter appears on the left of the body. Both the gutter and the body live inside the same `ScrollView`, so vertical and horizontal scrolling keep the numbers aligned with their corresponding source lines.
- Added `check-circle` and `x-circle` icons as part of the minimal icons set
- Added support of CoreCLR for the migration from Mono scripting backend for Unity 6.5+
- Added support for the new EntityId API in Unity 6000.3.0a1 and newer.
- Registered the `copy` icon (regular variant) in `Icons - Minimal.uss` and `IconBrowser`'s required-icons list, so any consumer using the App UI minimal icon set has access to it. Used by the new `CodeBlock` component's hover-revealed copy-to-clipboard button.
- Added documentation about getting started using UI Builder.

### Removed

- Removed Android Activity (AppUIActivity, AppUIGameActivity) and AndroidManifest.xml from the App UI package. The Android platform integration now operates without a custom Activity or manifest.

### Changed

- `AnchorPopup.RefreshPosition` is now `public` instead of `protected`.
- Streamlined AI skill content by removing duplicated sections from the main skill, consolidating reference documentation pointers, and fixing a Task/TodoItem naming conflict in the MVVM example
- `ColorField` popover is now movable by default.
- Improved AI skill descriptions with richer trigger phrases for more reliable skill activation
- Default `resizeDirection` for `DialogTrigger` changed from `Vertical` to `Free`.
- Added CSS transition animations for background-color, border-color, and color properties on Button and ActionButton components
- Switch CodeBlock syntax highlighting to themed batch tokenization (TextMateLib v0.2.0) for faster rendering and reduced GC allocations

### Fixed

- Fixed the AppUI settings assets search and auto-assignation before building a project.
- Fix theme color matching in syntax-highlighted code blocks
- Fixed runtime bindings for `DateField` and `DateRangeField` `value` property.
- Composite Numerical fields (such as Vector fields, Bounds fields, etc) previous value returned in the ChangeEvent is now the correct one.
- Fixed UpHandler calls in Draggable manipulator.
- Fixed the help url in the inspector for App UI Settings assets.
- Fixed exception thrown on Tabs element when refreshing the indicator with an out of range value.
- Fixed columns size and added more info in Storybook window.
- Added missing `[CreateProperty]` attribute on `clickable` property across all pressable components to fix runtime data binding for `clickable.command` path (UUM-138492)
- Fixed the search bar in the Icon Browser remaining enabled when filtering yields no results
- Disable "Delete selected icons" context menu option in Icon Browser when only required icons are selected
- Set minimum window sizes for IconBrowser, NavigationGraphWindow, and DevToolsWindow to prevent layout issues when resized too small
- Fixed an issue when trying to dismiss ColorPicker popup consecutively from ColorField
- Fixed the use of obsolete API in Trackpad sample
- Fixed `VisualElementExtensions.IsOnScreen` method which gave wrong result in world-space panels.
- Fixed applying anchored popup positions only if the computed values are different than the existing ones to avoid floating point precision issues in the layout.
- Build the Linux x86_64 TextMateLib native plugin against an older glibc baseline with a statically linked C++ runtime, so it loads on Unity 2021.3 Linux (resolves the `GLIBC_2.32 not found` load error)
- Fixed TextArea auto-resize calculation to use explicit padding/border/margin measurements and trigger resize on geometry changes
- Added `CreateProperty` attribute to generated command properties for Unity 2023.2+ to enable Unity Properties Serialization support in the MVVM CommandGenerator.
- Fixed overlapping scrollbar arrow icons that appeared when resizing the Properties section in the App UI Storybook window
- Fixed code path activation for native plugins depending on target platform
- Fixed ActionButton CSS selector ordering for `.is-selected` to follow BEM conventions and corrected `--appui-alias-actions-border-color-selected` to reference the selected background color alias instead of a hardcoded base token
- Fixed iOS Simulator linker errors by shipping separate Device and Simulator native plugin binaries (libAppUINativePlugin.a and libAppUITextMateLib.a with bundled oniguruma)
- Fixed exception thrown inside the MVVM & Redux sample.
- Fixed sending ChangeEvent in numerical fields as soon as they are attached to a panel if their value has been changed already even without notification.
- App UI Icons StyleSheet Asset now has a StyleSheet icon in the Project window when creating the StyleSheet Asset.
- Fixed coordinates conversion in AnchorPopupUtils.ComputePosition method
- Fix Icon Browser "Add icons" button not working on Unity 6000.4+ due to ObjectSelector.Show API change (List\<int\> to List\<EntityId\>)
- Fixed a NullReferenceException in MenuItem.OpenSubMenu when Menu/MenuItem are declared inline in UXML without a MenuTrigger or MenuBuilder.Build() wrapper.
- Fix headless Windows IL2CPP build crash caused by the native plugin activating WinRT settings objects at static scope on Windows Server Core (game-ci/unity-builder#702)

### Deprecated

- Deprecated Badge `content`, `max`, and `showZero` properties in favor of using child elements for badge content

