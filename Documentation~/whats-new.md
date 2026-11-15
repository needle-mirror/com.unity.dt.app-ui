---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.7] - 2026-11-15

### Fixed

- `MaskField` shows its localized Nothing text when Unity Localization is installed, like its Everything and Mixed texts.
- The Linux TextMateLib plugin exports only its `textmate_*` C API. It used to export about 1,100 symbols (oniguruma, libstdc++ instantiations, rapidjson) into the Editor process, which could interfere with the Editor's own native code.

