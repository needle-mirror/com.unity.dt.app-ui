---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.0-pre.12] - 2026-06-25

### Fixed

- Fix theme color matching in syntax-highlighted code blocks
- Build the Linux x86_64 TextMateLib native plugin against an older glibc baseline with a statically linked C++ runtime, so it loads on Unity 2021.3 Linux (resolves the `GLIBC_2.32 not found` load error)
- Fix headless Windows IL2CPP build crash caused by the native plugin activating WinRT settings objects at static scope on Windows Server Core (game-ci/unity-builder#702)

### Changed

- Switch CodeBlock syntax highlighting to themed batch tokenization (TextMateLib v0.2.0) for faster rendering and reduced GC allocations

