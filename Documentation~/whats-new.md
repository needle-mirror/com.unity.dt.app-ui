---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.3.3] - 2026-11-18

### Fixed

- Marked every App UI type that holds static state with `[NoAutoStaticsCleanup]`, so Unity 6.5 and later no longer log UAL0010 and UAL0013 warnings when domain reload is off. Older editors get an internal stand-in for the attribute.
- Added the missing `ScriptedImporter` block to 93 `.uss`, `.tss` and `.uxml` metas, which Unity 6.6 and later reported as "below the supported minimum (2)".

