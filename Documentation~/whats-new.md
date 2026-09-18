---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.3] - 2026-09-18

### Fixed

- The native platform P/Invoke declarations that return a `bool` now pin one-byte marshalling with `[return: MarshalAs(UnmanagedType.I1)]`. Without it a `[DllImport]` returning `bool` marshals as the 4-byte `UnmanagedType.Bool`, while every `NativeAppUI_*` entry point returns a 1-byte C++ `bool`; on x86-64 `setcc %al` writes only the low byte, so register residue in bits 8-31 could read back as `true` when the native answer was `false`.
- App UI shaders no longer fail to compile on Unity 7000, where the `UnityCG.hlsl` / `UnityUI.hlsl` includes they expected do not exist; they now include `UnityCG.cginc` on every Unity version.

