---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.0.0-pre.21] - 2025-06-13

### Changed

- The `NavDestination` node now uses a `NavDestinationTemplate` as a delegate to create and set up a `INavigationScreen` when reaching this destination.
- Moved `NavDestination` specific settings such as `showAppBar` inside the new `DefaultNavDestinationTemplate`.

### Added

- Added a default implementation of `NavDestinationTemplate` named `DefaultNavDestinationTemplate` which handles the creation of default `NavigationScreen` objects.
- Added `INavigationScreen` interface, more extensible than the base class `NavigationScreen`.

