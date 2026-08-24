---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.2] - 2026-08-24

### Fixed

- Fixed `Unity.AppUI.Markdown` failing when `APPUI_ENABLE_MARKDOWN` is defined. The bundled `Markdig.dll` was the `netstandard2.0` build, which references the `System.Runtime.CompilerServices.Unsafe`, `System.Memory` and `System.Buffers` facades. Unity ships shims for the latter two but not for `Unsafe`, so parsing markdown threw `FileNotFoundException` at runtime. Replaced it with Markdig's `netstandard2.1` build, which targets the same profile Unity uses and references nothing but `netstandard` itself.
- Fixed ArgumentOutOfRangeException in Tabs when all items are disabled, and cancelled pending indicator refreshes when the selection is cleared.
- Fixed Tabs arrow-key navigation throwing ArgumentOutOfRangeException and not skipping disabled tabs (it scanned the empty content container instead of the tab items).
- Fixed `TextField.isPassword = true` throwing a `NullReferenceException` on Unity versions before 6000.0. Enabling UITK's `isPasswordField` forces multiline off, and that setter calls `text.Replace("\n", "")` with no null check on those versions. `TextField` seeded the underlying UITK field with a null value, so `new TextField { isPassword = true }` — and any `isPassword` toggle after the value was cleared — threw. The underlying field is now never given a null text, which also means `TextField.value` returns an empty string rather than null for an empty field.

