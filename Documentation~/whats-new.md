---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [3.0.0-pre.3] - 2026-11-16

### Added

- The Unity Editor 7 themes, `App UI - Editor 7 Dark.tss` and `App UI - Editor 7 Light.tss` (`editor-7-dark` and `editor-7-light` for `Panel.theme`), with compact density like the Editor themes. Their mapped colours come from Unity 7's editor skin: buttons have no border and selected buttons are grey rather than blue. `TweakMenu` and the Storybook window offer them.

### Fixed

- A selected `SplitButton` paints its label and icon with the selected foreground instead of the white accent foreground, so they stay readable on the light selected fill in the light themes.
- `App UI - Editor Light` now reads the same Unity Editor variables as `App UI - Editor Dark` for the same role. The field hover border is `#6C6C6C` instead of the dark theme's `[#656565](https://github.com/Unity-Technologies/unity-app-ui/issues/656565)`, the action border's selected and disabled states use the button border `#B2B2B2`, and a pressed selected button keeps the pressed-hover blue `#B0D2FC` instead of `#5a90d3`. The accent button's focus fill is the strong blue `#066ebf` like its other states, instead of a pale `#B0D2FC`.

