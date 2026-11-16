---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [3.0.0-pre.4] - 2026-11-16

### Deprecated

- Restored the v3 API that v4 renamed as obsolete shims, so code written against v3 (such as `com.unity.behavior`) still compiles: the enum members `Size.S`, `Size.M`, `Size.L`, `IconSize.XXS`, `IconSize.XS`, `IconSize.S`, `IconSize.M`, `IconSize.L`, `TextSize.XXS` (now `XS`) and `ButtonVariant.Accent` (now `Default`), `Button.quiet` (sets `ButtonVariant.Ghost`) and `Panel.scale` (maps `small`, `medium` and `large` to the `compact`, `normal` and `comfort` densities).

### Fixed

- `EnumField` no longer lists `[Obsolete]` enum members, and the generated USS class name helpers skip them.

