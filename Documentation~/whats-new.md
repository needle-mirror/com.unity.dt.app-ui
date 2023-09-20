---
uid: whats-new
---

# What's New in **0.6.0**

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

### Added

- Added the `Quote` component.
- Added `FieldLabel` component.
- Added `HelpText` component.
- Added `AvatarGroup` component.
- Added `required` property to the `InputLabel` component.
- Added `indicatorType` property to the `InputLabel` component.
- Added `helpMessage` property to the `InputLabel` component.
- Added `helpVariant` property to the `InputLabel` component.
- Added `variant` property to the `Avatar` component to support `square`, `rounded` and `circular` variants.

### Changed

- Added the `Button` `variant` property and removed the `primary` property.
- Updated App UI icons with a default size of 256x256 and moved into a folder named `Regular`.
- The `InputLabel` component uses the `FieldLabel` and `HelpText` components to display the label and the help text.
- The `Avatar` component now listens to `AvatarVarianContext` and `SizeContext` changes to update the variant and size of the avatar.

### Removed 

- Removed the `size` property from the `InputLabel` component.