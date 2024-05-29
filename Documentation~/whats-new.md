---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.0.0-pre.3] - 2024-05-30

### Added

- Added AsyncThunk support for Redux implementation.
- Added Anchor Position support for Toast UI elements.
- Added the `key` string property on `Radio` component to be used as unique identifier in their group.
- Added unit tests for MemoryUtils utility class.

### Removed

- Removed programatic construction of RadioGroup with IList object. Since App UI offers the possibility to have Radio component as deep as you want in the visual tree compared to its RadioGroup ancestor, we wanted to limit conflicts between construction kinds.

### Changed

- Moved Toast animation logic from code to USS.
- The `RadioGroup` component uses a `string` type for its `value` property. This string value is equal to the currently checked `Radio` component's `key` property.
- The `Toast.AddAction` method will now ask for a callback that takes a `Toast` object as argument (instead of no argument at all). This will give you an easier way to dismiss the toast from the action callback.
- You can now pass an `autoDismiss` argument to the `Toast.AddAction` method. This will automatically dismiss the toast when the action is triggered. This argument is optional and defaults to `true` for backward compatibility.
- Changed MemoryUtils.Concatenate implementation to not use variadic parameters and avoid implicit allocations.

### Fixed

- Fixed Action Dispatch to every Slice Reducers
- Every shared libraries of native plugins are now correctly signed with the correct Unity Technologies certificate (MacOS and Windows only)
- Fixed support of Radio component that are deeper than the direct child of a RadioGroup.

