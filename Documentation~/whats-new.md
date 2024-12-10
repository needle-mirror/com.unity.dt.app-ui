---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.0.0-pre.14] - 2024-12-10

### Changed

- Refactored switch statement builder for Redux slice construction.
- Subscribing to a Redux Store returns a `IDisposableSubscription` object instead of a method. You can dispose it by calling its `Dispose` method.
- Refactored Redux API fore more flexibility. See the migration guide in the package documentation.
- Changed DropZoneController to support any VisualElement as target instead of a DropZone element.
- Refactored the dispatch process of Redux AsyncThunk.

### Added

- Added support of Selectors for subscribing to state changes in Redux implementation.
- Added the support `string` key in Context API to identify a Context not only byt its type. This will give the ability to provide and propagate contexts of the same type but with different keys.
- Added support of Enhancers and Middlewares in Redux implementation.
- Added `editorOnly` setting in App UI's Settings panel. Enabling EditorOnly mode will add a scripting define symbol that will prevent App UI assemblies to get compiled for runtime builds. This can be useful to work in Editor only UI and avoid increasing the footprint of your builds.
- Added the support of App UI settings asset file from packages. If there's no persistent App UI settings asset defined for your project, App UI Manager will try to find one not only in the Project assets but also in Packages. You can always switch between settings via the **Project Settings > App UI** settings pane.
- Added `PreserveAttribute` on certain constructor that are only called via `Activator` reflection class.
- Added a search bar and "Save as" button in the Icon Browser window.

### Fixed

- Fixed drag events in drag and drop samples to disable the Dropzone when leaving the Editor window.
- Fixed text cursor for selectable text elements such as TextArea.
- Fixed warnings related to Dialog styling.

### Removed

- Removed the Popup message Handler that was used to dispatch the display or dismissal of Popups. While this removes thread safety, it fixes issues with ordering of events to dismiss popups.

