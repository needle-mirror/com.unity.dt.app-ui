---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [3.0.0-pre.1] - 2026-07-31

### Changed

- The minimum supported Unity version is now **6000.3 LTS** (previously 2021.3). The package manifest declares `"unity": "6000.3"`, and the CI editor matrix (validation, API validation, preview APV, package/project tests and player tests) now covers 6000.3, 6000.5, 6000.6 and trunk only. This is a major bump because the new floor drops Unity 6000.0 LTS, which is still under support — raising the minimum editor is only a non-breaking change when every dropped release line is already past end-of-support.

### Removed

- The APIs that were marked `[Obsolete]` are now gone, as part of the 3.0 major release. `Badge.content`, `Badge.max` and `Badge.showZero` have been removed — a Badge can hold any content, so use `Badge.label` for textual content or add child elements. The `TextFieldExtensions.BlinkingCursor()` extension method has been removed in favour of the `Unity.AppUI.UI.BlinkingCursor` manipulator (`textField.AddManipulator(new BlinkingCursor())`). `AppUISettings.autoOverrideAndroidManifest` has been removed; it had no effect, since App UI applies the changes it needs to the existing Android manifest during the build instead of shipping its own.
- Conditional compilation for editor versions below the new 6000.3 minimum. The `versionDefines` that can no longer change value at 6000.3+ were dropped from the `.asmdef` files and the `#if` branches they guarded were resolved in place: `ENABLE_UXML_TRAITS` and `ENABLE_ENABLED_UXML_PROPERTY` (never defined at 6000.3+) were deleted along with the code they guarded, while `ENABLE_UXML_SERIALIZED_DATA`, `ENABLE_RUNTIME_DATA_BINDINGS`, `ENABLE_VALUEFIELD_INTERFACE`, `ENABLE_UITK_TEXT_SELECTION`, `UITK_NESTED_INTERACTION_KIND`, `UITK_SELECTED_INDICES_CHANGED`, `UITK_MAKE_NONE_ELEMENT`, `CONDITIONAL_WEAK_TABLE_IL2CPP`, `UI_DOCUMENT_ROOT_ELEMENT_TYPE_EXISTS`, `UNITY_ENABLE_TABVIEW` and `UNITY_ENABLE_SLIDER_FILL` (always defined at 6000.3+) are now unconditional. Legacy `#if !UNITY_2022_*` and `#if !UNITY_2023_*` fallbacks were removed too. Because every one of these symbols already had a fixed value on 6000.3, the compiled API surface is unchanged for supported editors.

### Added

- Added App UI Visual Documentation window. This window builds its pages dynamically from the XML documentation of the components (extracted at compile time by a Roslyn source generator) and from the package's markdown documentation, instead of loading pre-generated UXML pages. The Visual Documentation window requires the `APPUI_ENABLE_MARKDOWN` scripting define.
- Add display scale factor and DPI support on Linux, exposing `scaleFactor`, `textScaleFactor` and `referenceDpi` on the Linux platform via new native plugin display-info APIs.

### Fixed

- Fixed `Unity.AppUI.Markdown` failing when `APPUI_ENABLE_MARKDOWN` is defined. The bundled `Markdig.dll` was the `netstandard2.0` build, which references the `System.Runtime.CompilerServices.Unsafe`, `System.Memory` and `System.Buffers` facades. Unity ships shims for the latter two but not for `Unsafe`, so parsing markdown threw `FileNotFoundException` at runtime. Replaced it with Markdig's `netstandard2.1` build, which targets the same profile Unity uses and references nothing but `netstandard` itself.
- Fixed ArgumentOutOfRangeException in Tabs when all items are disabled, and cancelled pending indicator refreshes when the selection is cleared.
- Fixed Tabs arrow-key navigation throwing ArgumentOutOfRangeException and not skipping disabled tabs (it scanned the empty content container instead of the tab items).
- Fixed `TextField.isPassword` throwing a `NullReferenceException` when set before the field is attached to a panel, which made `new TextField { isPassword = true }` unusable. Enabling UITK's `isPasswordField` forces multiline off and walks text backing that only exists once the field belongs to a panel, so the value is now stored and applied on attach.

