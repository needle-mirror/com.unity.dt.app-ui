---
uid: syntax-highlighting
---

# Syntax Highlighting

App UI provides TextMate-based syntax highlighting for displaying formatted source code.
This feature uses TextMate grammars for tokenization and themes for styling,
producing UITK-compatible rich text output.

## Overview

The syntax highlighting system consists of three main components:

- **TextMateGrammarAsset** - Stores a TextMate grammar definition for a language
- **TextMateThemeAsset** - Stores a TextMate theme for colors and styles
- **TextMateSyntaxHighlighter** - Converts source code to rich text using grammar and theme

## Grammar Assets

[TextMateGrammarAsset](xref:Unity.AppUI.UI.TextMateGrammarAsset) is a ScriptableObject
that stores a TextMate grammar definition. Import `.tmLanguage.json` files into your project
and the importer will create grammar assets automatically.

### Properties

| Property | Description |
|----------|-------------|
| `jsonContent` | The raw JSON content of the grammar file |
| `scopeName` | Unique identifier (e.g., "source.csharp") |
| `displayName` | Human-readable name (e.g., "C#") |

## Theme Assets

[TextMateThemeAsset](xref:Unity.AppUI.UI.TextMateThemeAsset) stores a TextMate theme definition.
Import VS Code theme JSON files to create theme assets.

### Properties

| Property | Description |
|----------|-------------|
| `jsonContent` | The raw JSON content of the theme file |
| `displayName` | Human-readable name (e.g., "Dark Plus") |

## Usage

### Basic Usage

```cs
using Unity.AppUI.UI;

// Load assets (e.g., from Resources or Addressables)
var grammar = Resources.Load<TextMateGrammarAsset>("CSharpGrammar");
var theme = Resources.Load<TextMateThemeAsset>("DarkPlusTheme");

// Create highlighter
using var highlighter = new TextMateSyntaxHighlighter(grammar, theme);

// Highlight source code
string sourceCode = "public class Example { }";
string richText = highlighter.Highlight(sourceCode);

// Use richText with UITK Text element
myTextElement.text = richText;
```

### Line-by-Line Highlighting

For incremental highlighting (e.g., in a text editor), use `HighlightLine`:

```cs
using System;
using Unity.AppUI.UI;

var highlighter = new TextMateSyntaxHighlighter(grammar, theme);
var state = IntPtr.Zero;

foreach (var line in lines)
{
    string richLine = highlighter.HighlightLine(line, state, out state);
    // Append richLine to output
}

highlighter.Dispose();
```

## API Reference

### TextMateSyntaxHighlighter

| Member | Description |
|--------|-------------|
| `TextMateSyntaxHighlighter(grammarAsset, themeAsset)` | Creates a highlighter with the specified grammar and theme |
| `Highlight(string sourceCode)` | Highlights full source code, returns rich text |
| `HighlightLine(string line, IntPtr prevState, out IntPtr newState)` | Highlights a single line with state continuity |
| `Dispose()` | Releases native resources |

> [!IMPORTANT]
> `TextMateSyntaxHighlighter` uses native resources and must be disposed when no longer needed.
> Use the `using` statement or call `Dispose()` explicitly.

## Related

- [Typography](xref:typography)
- [Styling](xref:styling)
