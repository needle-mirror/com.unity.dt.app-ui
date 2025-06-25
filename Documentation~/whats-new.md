---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.0.0] - 2025-06-25

### Removed

- Removed `Unity.AppUI.Core.AppUI.DismissAnyPopups` method. Popups are now registered per `Panel` element.
- Removed `Canvas.scrollDirection` property in order to support correctly the scroll direction set in the Operating System's preferences.
- Removed programatic construction of RadioGroup with IList object. Since App UI offers the possibility to have Radio component as deep as you want in the visual tree compared to its RadioGroup ancestor, we wanted to limit conflicts between construction kinds.
- TextFieldExtensions.BlinkingCursor extension method has become obsolete. Please use the new BlinkingCursor manipulator instead.
- Removed intrusive Debug.Log calls from Platform class on Windows platform.
- Removed warning message when using Single selection type in an overflown ActionGroup.
- Removed EventBaseExtensionsBridge class
- [Canvas] Removed applying cursor styles during pointer events as it was too costly in terms of performance.
- Removed the Popup message Handler that was used to dispatch the display or dismissal of Popups. While this removes thread safety, it fixes issues with ordering of events to dismiss popups.

### Changed

- Refactored SliderBase and BaseSlider for performance improvements and better features. The RangeSliderBase class has been removed and RangeSliderFloat/RangeSliderInt are now derived from SliderBase directly.
- The `Platform`, `AppUIInput` and `GestureRecognizer` classes now use pre-allocated buffers to deal with Touch events in a single frame, and expects a `ReadOnlySpan<AppUITouch>` type to work with instead of `AppUITouch[]`.
- Refactored switch statement builder for Redux slice construction.
- The `NavDestination` node now uses a `NavDestinationTemplate` as a delegate to create and set up a `INavigationScreen` when reaching this destination.
- Subscribing to a Redux Store returns a `IDisposableSubscription` object instead of a method. You can dispose it by calling its `Dispose` method.
- Changed DialogTrigger.keyboardDismissDisabled to DialogTrigger.keyboardDismissEnabled for consistency.
- Make Host optional when intializing an `App` implementation.
- Renamed `IUIToolkitApp.mainPage` property into `IUIToolkitApp.rootVisualElement` for more clarity.
- Moved `NavDestination` specific settings such as `showAppBar` inside the new `DefaultNavDestinationTemplate`.
- Replaced ClickEvent action in MenuItem builder by an EventBase action
- Refactored completely the DropZone UI element. Now the DropZone doesn't embed any logic, but uses a `DropZoneController` instead. You can access this controller via `DropZone.controller` property and attaches a callback method to accept dragged objects and listens to drop events.
- Changed `ticks` related properties in Slider components with a new `marks` property. To configure marks you can you the new `step` property or directly set your `customMarks`
- Changed StoryBookEnumProperty class to become a generic type. You need to specify the Enum type as typedef parameter.
- Refactored Redux API fore more flexibility. See the migration guide in the package documentation.
- Renamed Popup.parentView to Popup.containerView for more clarity.
- Complete rewrite of the SplitView component. The SplitView is no more a derived from TwoPaneSplitView from UI-Toolkit, but a full custom component that supports any number of panes.
- Scrollable manipulator now stops the propagation of WheelEvent. This affects only the Drawer and SwipeView elements.
- Defer checking Popup's container candidate when the Popup is about to be shown, instead of during Popup creation.
- Use `resolvedStyle.translate` instead of `transform.position` to move elements such as the `Canvas` container.
- Changed DropZoneController to support any VisualElement as target instead of a DropZone element.
- Changed the Text element inside the ColorField to become a selectable text.
- Others assembly modules such as `Redux`, `MVVM` and `Navigation` has been configured to be auto-referenced in the Unity project's assemblies.
- Refactored every native plugin provided by the package.
- Updated App UI Native plugins for MacOS and iOS.
- Refactored the styling of Chip UI element.
- Moved Toast animation logic from code to USS.
- Changed the Trackpad sample project to work properly with the new events coming from the new Gesture Recognizer System.
- Refactored the SwipeView element logic, without impacting the public API.
- The `RadioGroup` component uses a `string` type for its `value` property. This string value is equal to the currently checked `Radio` component's `key` property.
- The `Toast.AddAction` method will now ask for a callback that takes a `Toast` object as argument (instead of no argument at all). This will give you an easier way to dismiss the toast from the action callback.
- You can now pass an `autoDismiss` argument to the `Toast.AddAction` method. This will automatically dismiss the toast when the action is triggered. This argument is optional and defaults to `true` for backward compatibility.
- Rewrite of App UI shaders in pure `HLSL` instead of legacy `CG` language.
- Changed MemoryUtils.Concatenate implementation to not use variadic parameters and avoid implicit allocations.
- Replaced the MacOS native plugin by a `.dylib` library instead of a `.bundle` one.
- App UI shaders are now optionally embedded in Player builds. You can change this setting in your main App UI settings instance. The default value is `true`.
- Storybook stories are now sorted alphabetically.
- Upgraded the old Drag And Drop Sample to use the refactored DropZone and the new Drag And Drop system.
- Renamed `Create/App UI/Settings` menu item into `Create/App UI/App UI Settings`
- Changed the way MarkDirtyRepaint is scheduled on elements containing animated textures (check properly for visibility).
- Replaced the dropdown with an ObjectField to pick an available App UI settings asset.
- The Canvas now uses `Experimental.Animation` system from UI-Toolkit for its damping effect when releasing the mouse with some velocity. That replaces the previous implementation that was using the `VisualElementScheduledItem`.
- Changed fade in animation in Tooltip to use USS transitions.
- Changed the styling of primary action buttons in AlertDialog UI element.
- Changed the Context API to propagate contexts over _internal_ components hierarchies and not just the high-level hierarchy (via `contentContainer`).
- Refactored the `FindResponderInChain` native method in MacOS platform to avoid undefined behaviours.
- Refactored the styling of DropZone UI element.
- `CircularProgress` and `LinearProgress` elements with `Indeterminate` variant won't be marked as `DirtyRepaint` if they are off-screen anymore.
- Refactored the dispatch process of Redux AsyncThunk.

### Added

- Added `scale` property on slider components to support custom non-linear scaling of displayed values in the UI.
- Added the support of inverted track progress in slider components.
- Added MaskField UI component.
- Added support of Selectors for subscribing to state changes in Redux implementation.
- Added support of UITK Runtime DataBinding system in ObervableObject class.
- Added Redux Debugging tool.
- Added a default implementation of `NavDestinationTemplate` named `DefaultNavDestinationTemplate` which handles the creation of default `NavigationScreen` objects.
- Added the support `string` key in Context API to identify a Context not only byt its type. This will give the ability to provide and propagate contexts of the same type but with different keys.
- Added `formatFunction` along with `formatString` and grouped them into a new `IFormattable<TValueType>` interface. The `formatFunction` will give you the possibility to customize entirely the string formatted value of a given `TValueType`.
- Added a `RestrictedValuePolicy` on slider components where you can choose if the slider can take a value related to the `step` or `customMarks` with modifier keys or not.
- Added support of Enhancers and Middlewares in Redux implementation.
- Navigation: Added `NavigationRail` component which can be used inside NavigationScreen views.
- Added AsyncThunk support for Redux implementation.
- Added the `Unity.AppUI.UI.DropZoneController` Manipulator for a lower level approach to create your own "drop zones".
- Added support of Attributes on fields and properties in the Dependency Injection system.
- Added Anchor Position support for Toast UI elements.
- Added `IApp.services` property which returns the current `IServiceProvider` available for this instance of `IApp`. Thanks to this property you should be able to load a service from anywhere in your application by calling `App.current.services`.
- Added `PinchGestureRecognizer` implementation for the new Gesture Recognizer System.
- Added `LangContext.GetLocalizedStringAsyncFunc` to delegate the localization operation to a user-defined function.
- Added `justified` property on Tabs component to jusitfy tabs layout in horizontal direction.
- `VisualElementExtensions.HasAncestorsOfType{T}` method to verify if an element as any ancestor of a specific type.
- Added `IInitializableComponent` interface to `IApp` interface. Now when initializing a MVVM `App` object, the related `AppBuilder` will call `app.InitializeComponent` before hosting.
- Added `editorOnly` setting in App UI's Settings panel. Enabling EditorOnly mode will add a scripting define symbol that will prevent App UI assemblies to get compiled for runtime builds. This can be useful to work in Editor only UI and avoid increasing the footprint of your builds.
- Added `--appui-splitview-splitter-anchor-size` design token.
- `Added Unity.AppUI.Core.DragAndDrop` class to handle drag and drop (in-panel and/or with the Editor). The support of external drag and drop at runtime is planned for future releases.
- Added the `key` string property on `Radio` component to be used as unique identifier in their group.
- Added PanAndZoom Manipulator
- Added `EnumField` UI component.
- Added the serialzation of last selected indices in Storybook window to save and restore selection.
- Added support of UnityEditor ColorPicker in ColorField component.
- Added `DatePicker`, `DateRangePicker`, `DateField` and `DateRangeField` components. Theses components use the new `Date` and `DateRange` data structure also provided by App UI.
- Added Modal.outsideClickDismissEnabled and Modal.outsideClickStrategy properties to support dismissing Modals by clicking outside of them.
- Added `VisualElementExtensions.SetTooltipContent` method to populate a tooltip template with new content.
- Added `orientation` property on slider components, to support vertical sliders.
- Added `RegisterUpdateCallback` to register a callback on VisualElement that needs to be notified when App UI's main loop has done a new iteration.
- Added support of `ICommand` in `Pressable` manipulator.
- Added CircularProgress story in Storybook sample.
- Added AlertDialog icon Design Tokens to customize icons directly from USS.
- Added ActionBar story in Storybook sample.
- Added scoped service provider implementation.
- Added support of Color without alpha information in ColorField and ColorPicker.
- Added `rounded-progress-corners` boolean property in `CircularProgress` and `LinearProgress` to be able to disable rounded corners.
- Added `SelectedLocaleListener` manipulator that reacts to Localization Package's SelectedLocale changes in order to provide a new `LangContext` in the visual tree.
- Added the support of App UI settings asset file from packages. If there's no persistent App UI settings asset defined for your project, App UI Manager will try to find one not only in the Project assets but also in Packages. You can always switch between settings via the **Project Settings > App UI** settings pane.
- Added `Unity.AppUI.UI.Chip.deletedWithEventInfo` and `Unity.AppUI.UI.Chip.clickedWithEventInfo` events.
- Added `trailing-icon` property in `ActionButton` component.
- Added `damping-effect-duration` property in Canvas element. The default value is 750ms.
- Added an experimental method `Platform.GetSystemColor` to fetch color values defined by the Operating System for specific UI element types. This can be useful if you want to precisely follow the color palette of a high-contrast theme directed by the OS.
- Added `MasonryGridView` component.
- Added `autoShrink` property to the `TextArea` component to automatically shrink the component when the text fits in a smaller area. You need to set `autoResize` property to `true` to use the shrinking feature.
- Added customization support for the size of the Color swatch inside the ColorField.
- Added `VisualElementExtensions.GetLastAncestorOfType{T}` method to retrieve the last ancestor of a specific type in the tree.
- Added AlertDialog examples in the UI Kit sample.
- Added "Icon Browser", a new Editor tool that enables users to generate UI-Toolkit stylesheets with a specific list of icons.
- Added a new experimental Gesture Recognizer System.
- Added the ability to subscribe and check if the current operating system is in Reduce-Motion Accessibility Mode (Windows/Mac/Android/iOS).
- Added the ability to subscribe and check if the current Text Scale Factor of the currently used window (Unity Player window or the Game view window in the Editor) (Windows/Mac/Android/iOS).
- Added Phosphor Icons
- Added `VisualElementExtensions.IsOnScreen` method to determine if a `VisualElement` is on-screen or off-screen.
- Added `ResizeHandle` component. This special component can be added to your layout to resize a given `target` when dragging it. This component is currently used in the new resizable `Popover` element.
- Added `resizable` and `resizableDirection` properties in `Popover` class. You are now able to resize a `Popover` using the bottom-right corner of the popover pane. A resizable Popover will not be repositionned.
- Added `VisualElementExtensions.EnableDynamicTransform` method to set/unset the `UsageHints.DynamicTransform` flag in your element's usageHints.
- Added the ability to subscribe and check if the current operating system is in High-Contrast Mode (Windows/Mac/Android/iOS).
- Added `IsContextClick()` extension method for PointerEvent.
- Added runtime binding support for commands in `Pressable` via `clickable.command` property in most interactable UI elements.
- Added `RectExtensions.Approximately` method to verify if two Rect object are approximately the same.
- Added Clipboard handling support for **UTF8 Text** and **PNG** format on _iOS_, _Windows_, _macOS_, and _Linux_ platforms. Please refer to the **Native Integration** documentation page for more information.
- Added support of Localization package in the Storybook window. You can now change the current used Locale in the window via a dropdown in the Storybook context toolbar. This dropdown will appear only if you have the Unity Localization package installed and have at least one existing Locale set up in your Localiztion settings.
- Added the ability to subscribe and check if the current operating system is in LeftToRight or RightToLeft layout direction (Windows/Mac/Android/iOS).
- Added `INavigationScreen` interface, more extensible than the base class `NavigationScreen`.
- Added `leadingContainer` and `leadingContentTemplate` properties to the `AccordionItem` element.
- Popup elements such as `Popover`, `Modal` or `MenuBuilder` now get their view's `userData` filled with the `Popup` instance itself. This way you can retrieve information about the `Popup` instance that created this popup element from within the visual tree.
- Added the ability to subscribe and check if the current Scale Factor of the currently used window (Unity Player window or the Game view window in the Editor) (Windows/Mac/Android/iOS).
- Added `showExpandButtons` property to the `SplitView` UI element.
- Added Popup<T>.SetContainerView method to set a custom container which will be the parent of the popup's view.
- Added Source Generators for MVVM module. Source Generators helps to get rid of boilerplate code and focus on business logic.
- Added `NavHost.makeScreen` property. By setting your own callback to this property, you will be able to customize the way to instantiate a `NavigationScreen` when navigating to a new destination. This can be useful when coupled to Dependency Injection so you can retreive instances of your screens from a `IServiceProvider`. By default, the property is set with the use of `System.Activator.CreateInstance`.
- Added `indicatorPosition` property in `AccordionItem` component, in order to swap the indicator position either at start or end of the heading row.
- Added tests for Pan and Magnify gesture data structures.
- Made `AnchorPopup.GetMovableElement` method public for easier access and increase customization possibilities.
- Added new Story in the Drag And Drop sample.
- Added support of graceful fallback to lambda `Plafform` implementation if native plugins can not be loaded by the current plaftorm.
- Added `PreserveAttribute` on certain constructor that are only called via `Activator` reflection class.
- Added arrow-square-in icon.
- Added USS custom properties to choose the font style of AccordionItem header and TabItem text.
- Added a search bar and "Save as" button in the Icon Browser window.
- Added the ability to subscribe and check if the current operating system is in Dark Mode (Windows/Mac/Android/iOS).
- Added some warning messages (Standalone builds only) when a LocalizedTextElement value cannot be localized.
- Added a safety check in TooltipManipulator to ensure the anchor element of the scheduled tooltip is still valid (not _invisible_) when it is time to _animate in_ the tooltip.
- Added monitoring of key press events in the TooltipManipulator in order to dismiss any existing tooltip when the user interacts with the keyboard.
- Added `check` regular icon as a required icon in App UI themes.
- Added support of `TextElement.displayTooltipWhenElided` to show elided text as a tooltip using the App UI tooltip system.
- Added unit tests for MemoryUtils utility class.

### Fixed

- Fixed the wrap system of the SwipeView element when swiping between elements quickly.
- Fixed tooltip that weren't showing up anymore since last pre-release.
- Fixed backgrounds color of the input fields.
- Fixed pinch gesture recognition sensitivity.
- Fixed styling of MenuItem element
- Fixed TouchSlider progress element overlapping parent's borders.
- Fixed reset of Dropdown value when changing its source items.
- Fixed the merge of default and new `Argument` objects during navigation to a `NavDestination`.
- Fixed `VisualElementExtensions.IsInvisible` method to check recursively in ancestors if any element is considered as _invisible_.
- Fixed compilation errors when the Unity project's Input Handling is set to `Both` or `New Input System` and the package `com.unity.inputsystem` is not installed.
- Fixed Daisy chaining window procedures on Windows platform.
- Fixed Action Dispatch to every Slice Reducers
- Every shared libraries of native plugins are now correctly signed with the correct Unity Technologies certificate (MacOS and Windows only)
- Fixed styling issues in `ActionButton` component.
- Fixed random segmentation fault on MacOS platform which appeared after domain reloads.
- Fixed meta files for native plugins on Windows platform.
- Fixed force mark as dirty repaint Progress UI element when swtiching its variant.
- Fixed some SplitView styling issues when nesting SplitViews in a visual tree.
- Fixed "Shape" icon.
- Fixed drag events in drag and drop samples to disable the Dropzone when leaving the Editor window.
- Fixed Dark Gray 1200 color value in Design Tokens for Editor-Dark theme.
- Fixed a visual bug where the checkmark symbol didn't appear on DropdownItem or MenuItem that have a `selected` state.
- Fixed Menu's backdrop to block pointer events
- Fixed a bug where `Popover` elements were not correctly anchored in Unity versions older than 6.
- Added one frame delay before processing Popover dismissal.
- Fixed calling `shown` event callbacks when a `Modal` is displayed.
- Fixed usage of AppCompat theme for AppUI GameActivity on Android platform.
- Fixed MacOS native plugin memory leak when opening the Help menu in the Editor.
- Fixed Tabs emphasized color in Editor-light theme.
- Fixed notify property changes for Picker.selectedIndex.
- Fixed text cursor for selectable text elements such as TextArea.
- Fixed an edge case when popovers are dismissed as OutOfBounds as soon as Show() is called.
- Fixed some namespace usage to avoid relative ones.
- Fixed IL2CPP Compilation errors on Windows Platform due to non-static MonoPInvokeCallback.
- Fixed Localization support in the ActionBar UI element.
- Fixed path resolution of Stylesheets coming from Packages in Icon Browser tool.
- Invoke click event only if Pressable is still hovered.
- Fixed PInvoke delegate types on Windows platform.
- Fixed ColorField styling issues.
- Use correct color variables for Radio and Checkbox components
- Use `RenderTexture.GetTemporary` instead of `new RenderTexture` to optimize RT allocations, especially on tile-based renderers such as mobile platforms.
- Fixed an exception thrown when trying to update UXML schemas in the Editor.
- Removed force blurring the BaseSlider (SliderFloat, TouchSliderFloat, etc) when the users stops interacting with it.
- Fixed border color variable for AccordionItem.
- Fixed warnings related to Dialog styling.
- Register trickledown events for popups on the first child of the visual tree root element instead of the root element itself to avoid leaks.
- Fixed styling of SplitView's Splitter Anchor size.
- Fixed a regression where components using `Pressable` manipulator will not able to be clicked more than once if the cursor doesn't leave the component's layout rect.
- Fixed context propoagation per key
- Fixed styling of BaseGridView when containing a single column.
- Popovers and Modals now correctly start checking for PointerDown events in the visual tree when they become visible.
- Prevent GC to collect Platform configuration to not break communication with native plugins (MacOS and iOS).
- Fixed NullReferenceException thrown when fetching Localization tables.
- Removed console message when trying to add an Editor MonoBehaviour in the scene during PlayMode.
- Fixed Screen Height calculation. UITK does not use Camera rect but blit on the whole screen instead.
- Fixed `InvalidOperationException` thrown by the damping effect animation of the `Canvas` component.
- Fixed CultureInfo used during source code generation.
- Fixed support of Radio component that are deeper than the direct child of a RadioGroup.
- Fixed styling of emphasized checkboxes.
- Fixed an exception thrown when dismiss any popup of a panel when this panel becomes out of focus.
- Fixed the calculation of off-screen items in SwipeView with vertical direction.
- Fixed an early return in the PreProcessBuild callback of App UI when no persistent AppUISettings have been found.
- Fixed a bug where tooltips stop being shown when the window is docked/undocked.

