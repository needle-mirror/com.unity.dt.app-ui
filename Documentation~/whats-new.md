---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.0-pre.9] - 2026-05-19

### Added

- Added Thread UI components including Thread, ThreadMessage, ThreadComposer, ThreadReaction, ThreadReactionBar, ThreadResolveMessage, ThreadUnresolveMessage, MentionPicker, and ReactionPicker, along with their associated interfaces (IEmojiProvider, IMentionProvider, IThreadAttachment), base classes, enums, and USS styles. Renamed ThreadMessage `content` property to `message`, removed `isEditing` property in favor of `state = Draft`, moved `emojiProvider` and `mentionProvider` from ThreadMessage to ThreadContext, removed `Active` message state, and exposed explicit `attachments` container on ThreadMessage and ThreadComposer
- Added new icons (arrow-up, paperclip, pen, paper-plane-tilt, smiley, thumbs-up, thumbs-down) to the minimal icon set and IconBrowser required icons list
- Added TextMate grammar-based syntax highlighting support via native TextMateLib integration, including C# bindings (Registry, Grammar, Theme, Token), ScriptableObject assets (TextMateGrammarAsset, TextMateThemeAsset), ScriptedImporters for .tmLanguage and .tmTheme files, and a high-level TextMateSyntaxHighlighter API
- Added Edit and Delete text action constants with localization support in TextActions
- New `CodeBlock` UITK component (`Unity.AppUI.UI.CodeBlock`): rounded card with a header row (language label + hover-revealed copy button using `Platform.SetPasteboardData`) above a monospaced body. Syntax highlighting is configured declaratively from USS via two `CustomStyleProperty<string>` values (`--codeblock-grammar`, `--codeblock-theme`) holding TextMate asset GUIDs; setting the `language` property toggles a `appui-code-block--lang-<value>` modifier class so per-language grammar mappings can be expressed in the design system. When a `TextMateThemeAsset` resolves, the body's background and foreground colors are pulled from `Theme.GetDefaultBackground()`/`GetDefaultForeground()` so the card adopts the theme's palette.
- Shipped Roboto Mono (`PackageResources/Fonts/RobotoMono-Regular.ttf`) as a runtime font asset and exposed it as a new design token `--appui-code-font-family` (declared in `Components/CodeBlock.uss`). The font was previously buried under `Editor/Editor Default Resources/Fonts/` and only reachable from editor windows; the Redux DevTools `--mono-font` reference was migrated to the new token.
- Bundled TextMate grammars (csharp, lua, css, xml, json) and themes (dark-plus, light-plus) under `PackageResources/SyntaxHighlighting/`. The XML and Lua grammars were stripped of their cross-language `source.java` / `source.c` includes so they tokenize standalone in our single-grammar `Registry`. Used by the new `CodeBlock` component for syntax highlighting when `APPUI_ENABLE_SYNTAX_HIGHLIGHTING` is enabled.
- New `MarkdownView` UITK component (`Unity.AppUI.Markdown` namespace) gated behind the `APPUI_ENABLE_MARKDOWN` scripting symbol. It parses CommonMark + GFM via the bundled Markdig 0.37.0 (`Runtime/Markdown/Plugins/Markdig.dll`) and renders into App UI components only — `Heading`, `Quote`, `Divider`, `LocalizedTextElement`, `Link`, plus the new `CodeBlock` — through a custom `RendererBase` (`UITKMarkdownRenderer`) with one object-renderer per CommonMark node type.
- `CodeBlock.showLineNumbers` property: when enabled, a 1-based line-number gutter appears on the left of the body. Both the gutter and the body live inside the same `ScrollView`, so vertical and horizontal scrolling keep the numbers aligned with their corresponding source lines.
- Registered the `copy` icon (regular variant) in `Icons - Minimal.uss` and `IconBrowser`'s required-icons list, so any consumer using the App UI minimal icon set has access to it. Used by the new `CodeBlock` component's hover-revealed copy-to-clipboard button.

### Deprecated

- Deprecated Badge `content`, `max`, and `showZero` properties in favor of using child elements for badge content

### Fixed

- Fixed TextArea auto-resize calculation to use explicit padding/border/margin measurements and trigger resize on geometry changes
- Fixed ActionButton CSS selector ordering for `.is-selected` to follow BEM conventions and corrected `--appui-alias-actions-border-color-selected` to reference the selected background color alias instead of a hardcoded base token
- Fixed a NullReferenceException in MenuItem.OpenSubMenu when Menu/MenuItem are declared inline in UXML without a MenuTrigger or MenuBuilder.Build() wrapper.

### Changed

- Added CSS transition animations for background-color, border-color, and color properties on Button and ActionButton components

