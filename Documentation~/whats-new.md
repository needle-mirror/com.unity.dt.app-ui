---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.0-pre.11] - 2026-06-03

### Added

- `IServiceCollection.AddSingleton<T>(T instance)` and `AddSingleton(Type, object)` overloads (plus matching `AddSingletonWhen(Type, object, ContextMatch)`) for registering pre-existing instances as singletons in the MVVM DI container. The supplied instance is returned as-is on every resolution; the container does not invoke a constructor or apply `[Service]` field/property injection on it.

### Fixed

- Disable "Delete selected icons" context menu option in Icon Browser when only required icons are selected
- Fixed iOS Simulator linker errors by shipping separate Device and Simulator native plugin binaries (libAppUINativePlugin.a and libAppUITextMateLib.a with bundled oniguruma)
- Fix Icon Browser "Add icons" button not working on Unity 6000.4+ due to ObjectSelector.Show API change (List\<int\> to List\<EntityId\>)

