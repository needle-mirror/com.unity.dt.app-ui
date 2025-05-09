# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0-pre.9] - 2023-12-05

### Fixed

- Fixed Selection handling in ActionGroup.
- Fixed MacOS Native plugin to support Intel 64bits architecture with IL2CPP

### Changed

- In Numerical fields and sliders, the string formatting for percentage values follows now the C# standard. The formatted string of the value `1` using `0P` or `0%` will give you `100%`. If you want the user to be able to type `100` in order to get a `100%` as a formatted string in a numerical field, we suggest to use `0\%` string format (be sure that the `highValue` of your field is set to `100` too).

### Added

- Added `isOpen` property setter to be able to set the open state of the `Drawer` component without animation.

## [1.0.0-pre.8] - 2023-11-30

### Fixed

- Fixed Unity crashes when polling TabItem elements inside Tabs component.
- Fixed mouse capture during Pointer down event in Pressable manipulator for Unity versions older than 2023.1.
- Fixed tab key handling to switch focus in `TextArea` component.
- Fixed UI Kit Sample with Progress components' color overrides.
- Fixed `border-radius` usage in `ExVisualElement` component.
- Fixed typo in App UI Elevation's USS selector.
- Fixed box-shadows border-radius calculation
- Fixed `isPrimaryActionDisabled` and `isSecondaryActionDisabled` property setters

### Added

- Added new methods to push sub-menus in `MenuBuilder` class.
- Added `submit-on-enter` property in `TextArea` component.
- Added `submit-modifiers` property in `TextArea` component.
- Added `submitted` event property in `TextArea` component.
- Added `subMenuOpened` event property in `MenuItem` UI component.
- Added `accent` property to the `FloatingActionButton` component.

### Changed

- Runtime Tooltips won't be displayed if the picked element has `.is-open` USS class currently applied.

## [1.0.0-pre.7] - 2023-11-24

### Added

- Added others Inter font variants

### Fixed

- Fixed some unit tests

## [1.0.0-pre.6] - 2023-11-19

### Added

- Added Layout Direction `dir` context in the App UI `ApplicationContext`.
- Added support of LTR and RTL layout direction in most App UI components

### Changed

- Previously the Panel Constructor set the default scale context to "large" on mobile platforms, now the default scale context for any platform is "medium".

## [1.0.0-pre.5] - 2023-11-15

### Changed

- Migrated code into the App UI monorepo at https://github.com/Unity-Technologies/unity-app-ui
- Renamed `format` UXML Attribute to `format-string` to bind correctly in UIBuilder
- The `MacroCommand.Flush` method will now call `UndoCommand.Flush` on its child commands from the most recent to the oldest one.
  This is the same order as the one used by the `MacroCommand.Undo` method.
- Renamed CircularProgress and LinearProgress `color` Uxml Attribute to `color-override` to bind correctly in UIBuilder
- Float Fields using a string format set on Percentage (P) will now use the same value as what you typed

### Added

- Added `disabled` property on every components that needed it in order to bind correctly with UIBuilder
- Added `Spacer` component.

### Fixed

- Fixed the height of Toast components to be `auto`.
- Fixed `MacroCommand.Undo` and `MacroCommand.Redo` methods.

## [1.0.0-pre.4] - 2023-11-08

### Changed

- The `Canvas` component now listens to `PointerDownEvent` in `TrickleDown` phase to avoid to miss the event when the pointer is over a child element.

## [1.0.0-pre.3] - 2023-11-06

### Added

- Added public access to `Pressable.InvokePressed` and `Pressable.InvokeLongPressed` methods.
- Added `trailingContainer` property to the `AccordionItem` component.

### Fixed

- Fixed warning message about Z-axis scale in the `AnchorPopup` component.
- Fixed styling of the `AccordionItem` header component.
- Fixed the `not-allowed` (disabled) cursor texture for Windows support at Runtime.
- Fixed images in the package documentation.

### Changed

- The `Pressable` manipulator now uses the `keepPropagation` property also in its `PointerUpEvent` callback to avoid to propagate the event to the parent element.

## [1.0.0-pre.2] - 2023-11-04

### Added

- Added missing XML documentation parts for the public API of the package.

## [1.0.0-pre.1] - 2023-10-31

### Added

- Added debug symbols for native plugins.

### Changed

- Moved internal Engine API related code into a new assembly `Unity.AppUI.InternalAPIBridge`.

## [0.6.5] - 2023-11-04

### Added

- Added missing XML documentation parts for the public API of the package.
- Added the `primaryManipulator` property to the `Canvas` component to get the primary pointer manipulator used by the canvas when no modifier is pressed.
- Added the `autoResize` property to the `TextArea` component to grow the text area when the text overflows.
- Added the handling of double-click in the `TextArea` resizing handle to re-enable the auto-resize feature.
- Added the `outsideScrollEnabled` property to the `AnchorPopup` components to enable or disable the outside scroll handling. The default value is `false`.
- Added a smoother animation for the `AnchorPopup` components when the popup is about to be shown.

### Fixed

- Fixed the Canvas background to support light theme.
- Fixed the styling of DropdownItem checkmark.
- Fixed the styling of MenuItem checkmark.
- Fixed the styling of Slider components.
- Fixed the styling of TextArea component.
- Fixed the styling of TouchSlider components.
- Fixed the previous value sent in `ChangeEvent` of `NumericalField`, `VectorField` and `Picker` (Dropdown) components.
- Improved the synchronization of the `AnchorPopup` components to refresh their position in the layout faster.

### Changed 

- Changed the USS selector for component-level aliases to use directly `:root` selector instead of `<component_main_uss_class>` selector.

## [0.6.4] - 2023-10-31

### Fixed

- Fixed `NullReferenceException` when testing accept drag in `GridView` component.

## [0.6.3] - 2023-10-31

### Added

- Added `getItemId` property to the `GridView` component to get the id of an item.

### Fixed

- Fixed items selection persistence between refreshes in `GridView` component. 

## [0.6.2] - 2023-10-27

### Added 

- Added `closeOnSelection` property to the `MenuTrigger` component.
- Added `closeOnSelection` property to the `ActionGroup` component to close the popover menu (if any) when an item is selected.

### Fixed

- Fixed `closeOnSelection` property in `MenuBuilder` component.
- Fixed `TextInput` styling for Unity 2023.1+
- Fixed `InvalidCastException` at startup of santandlone builds (in both Mono and IL2CPP)
- Fixed `NullReferenceException` when dismissing a Popup from a destroyed UI-Toolkit panel.
- Fixed sub-menu indicator icon in `MenuItem` component.
- Fixed context click handling in `GridView` component.

## [0.6.1] - 2023-10-20

### Added

- Added `allowNoSelection` property to the `GridView` component to enable or disable the selection of no items. 

### Fixed

- Fixed the support of nested components inside the `Button` component.
- Fixed the reset of the previous selection when `GridView.SetSelectionWithoutNotify` method is called.

## [0.6.0] - 2023-09-20

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

## [0.5.5] - 2023-10-27

### Added 

- [Backport] Added `closeOnSelection` property to the `MenuTrigger` component.
- [Backport] Added `closeOnSelection` property to the `ActionGroup` component to close the popover menu (if any) when an item is selected.

### Fixed

- [Backport] Fixed `closeOnSelection` property in `MenuBuilder` component.
- [Backport] Fixed `TextInput` styling for Unity 2023.1+
- [Backport] Fixed `InvalidCastException` at startup of santandlone builds (in both Mono and IL2CPP)
- [Backport] Fixed `NullReferenceException` when dismissing a Popup from a destroyed UI-Toolkit panel.
- [Backport] Fixed context click handling in `GridView` component.

## [0.5.4] - 2023-10-19

### Added

- Added `allowNoSelection` property to the `GridView` component to enable or disable the selection of no items. 

### Fixed

- Fixed the support of nested components inside the `Button` component.
- Fixed the reset of the previous selection when `GridView.SetSelectionWithoutNotify` method is called.

## [0.5.3] - 2023-10-16

### Added

- Added `useSpaceBar` property in `Canvas` component to enable or disable the handling of the space bar key.

### Fixed

- Removed the handling of the space bar key in the `Canvas` component when the used control scheme is `Editor`.

## [0.5.2] - 2023-10-12

### Added

- Added `acceptDrag` property to the `GridView` component to enable or disable the drag and drop feature.
- Added `menu` icon.
- Added `AddDivider` and `AddSection` methods to the `MenuBuilder` component.

### Fixed

- Fixed handling `acceptDrag` property in `Dragger` manipulator.
- Fixed MenuItem opening sub menus when the item is disabled.
- Fixed mipmap limit for Icons when a global limit is set in the Quality settings of the project.
- Fixed the capture of pointer during PointerDown event in `Dragger` manipulator. Now the pointer is captured only if the `Dragger` manipulator is active (i.e. the threshold has been reached).

## [0.5.1] - 2023-09-20

### Added

- Added `selectedIndex` property to the `Picker` component to get or set the selected index for a single selection mode.
- Added a new `Enumerable` extension method called `GetFirst` to get the first item of an enumerable collection or a default value if the collection is empty.
- Added `isPrimaryActionDisabled` and `isSecondaryActionDisabled` properties to the `AlertDialog` component.
- Added styling for `AlertDialog` component semantic variants.

### Fixed

- Fixed the `Picker` component to avoid to select multiple items if the selection mode is set to `Single`.
- Fixed `Menu.CloseSubMenus` method to close sub-menus opened by `MenuItem` components contained inside `MenuSection` components.
- Fixed double click handling in `GridView` component.

### Changed

- Changed the picking mode of the `DropdownItem` component to `Position` instead of `Ignore`.

## [0.5.0] - 2023-09-15

### Added

- Added `outsideClickStrategy` property in `AnchorPopup` component.
- Added `DropZone` component.
- Added `DropTarget` manipulator.
- Added a sample for `DropZone` and `DropTarget` components usage.
- Added `--border-style` custom USS property to support `dotted` and `dashed` border styles.
- Added `--border-speed` custom USS property to animate the border style.
- Added `Picker` component.
- Added `closeOnSelection` property for `Picker` and `Dropdown` components to close the picker when an item is selected.

### Fixed

- Fixed the `Pressable` manipulator to only handle the Left Mouse Button by default.
- Fixed current tooltip element check in `TooltipManipulator`.
- Fixed incremental value when interacting with keyboard in `ColorSlider` component.
- Fixed clearing selection in `GridView` when a user clicks outside of the grid.
- Fixed `itemsChosen` event in `GridView` to be fired only when the selection is not empty.
- Fixed Navigation keys handling in the `GridView` to clamp the selection to the grid items.

### Changed

- The `Pressable` manipulator nows inherits from `PointerManipulator` instead of `Manipulator`.
- Changed the `GridView.GetIndexByPosition` method to use a world-space position instead of a local-space position and renamed it to `GetIndexByWorldPosition`.
- TouchSlider component will now loose focus when a slide interaction has ended.
- When calling `GridView.Reset()` method, the selection won't be restored if no custom `GridView.getItemId` function has been provided. 
- When using `--box-shadow-type: 1` (inset box-shadow), the `--box-shadow-spread` value was interpreted with the same direction as outset box-shadow. This has been fixed so you can use a positive spread value to go inside the element and a negative spread value to go outside the element.
- The `Dropdown` component inherits from `Picker` component. Users will be able to create custom dropdown-like components by inheriting from `Picker` component.
- The `Dropdown` component now has a selection mode property to choose between single and multiple selection modes.
- The `Dropdown` component now uses `DropdownItem` component instead of `MenuItem` component.
- Removed `default-value` UXML property from `Dropdown` component. The preferred way to set the default value is by code.

## [0.4.2] - 2023-09-01

### Added

- Added `FrameWorldRect` method to the `Canvas` component.
- Added `frameContainer` property to the `Canvas` component.

### Fixed

- Fixed the soft selection handling on `PointerUpEvent` in the `GridView` component.

### Added

- Added the `Picker` component.

### Changed

- Changed `Dropdown` implementation to use the new `Picker` component.

## [0.4.1] - 2023-08-24

### Added

- Added mouse and touchpad mapping presets to the `Canvas` component.
- Added a setting to enable or disable gestures recognition on MacOS.
- Added selection mode in `ActionGroup` component.

## [0.4.0] - 2023-08-18

### Added

- Added the **Small** scale context to App UI. This context is now the preferred one for desktop platforms.
- Added `RangeSlider` component.

### Fixed

- Fixed `ValueChanged` and `ValueChanging` events propagation in `NumericalField` and Vector components.

## [0.3.9] - 2023-08-17

### Added 

- Added Context API which is accessible through any `VisualElement` instance.
- Added `preventScrollWithModifiers` property to the `GridView` component.

### Fixed

- Fixed `autoPlayDuration` property on PageView component.
- Fixed value validation in `NumericalField` component.

## [0.3.8] - 2023-08-08

### Added

- Added `tooltip-delay-ms` property to the `ContextProvider` component to customize the tooltip delay.
- Added more shortcuts to the `Canvas` component.

### Fixed 

- Fixed Editor crash when updating packages from UPM window.

## [0.3.7] - 2023-08-07

### Added

- Added `tooltipTemplate` property (by code only) on `VisualElement` to customize the tooltip content.

### Fixed

- Fixed Canvas manipulators for mouse devices and generic touchpads.
- Fixed Tooltip manipulator coordinates to pick elements under the cursor.
- Fixed Tooltip display for Editor context.
- Fixed random crashes when switching focused window.

## [0.3.6] - 2023-07-28

### Added

- Added `autoPlayDuration` property to the `PageView` component.
- Added `variant` property to the `IconButton` component.
- Added `Canvas` component.
- Added `GridView` component.

### Changed

- Changed the calibration values for Magic Trackpad gestures recognition.

### Fixed

- Fixed horizontal ScrollView styling.

## [0.3.5] - 2023-07-24

### Fixed

- Regenerated the meta files inside MacOS bundle folder to avoid error messages in the console.

## [0.3.4] - 2023-07-17

### Changed

- Draggable manipulator now is publicly accessible.

### Fixed

- Ensure shaders exist before creating materials.
- Fixed random crashes during domain unload in Unity 2022.3+.
- Fixed cursors variables for Editor context.

## [0.3.3] - 2023-07-06

### Added 

- Added Magic Trackpad gesture support for MacOS.
- Added `PanGesture` and `MagnificationGesture` events for UITK dispatcher.

### Fixed

- Fixed `Pressable` post-processing for disabled targets.
- Fixed visual content rendering synchronization in `Progress` and `ExVisualElement` components.
- Fixed Animation Recycling in `NavHost` component.

## [0.3.2] - 2023-06-01

### Fixed 

- Fixed NavAction being added twice in NavGraph when deleting a linked NavDestination.

## [0.3.1] - 2023-05-17

### Fixed

- Fixed warning messages about styling.
- Fixed warning messages about GUID conflicts in UI Kit samples.
- Fixed warning messages about unused events in NavController.

## [0.3.0] - 2023-05-17

### Added

- Added defaultMessage property on Dropdown component.
- Added ChangingEvent on TextField, TextArea and NumericalField components.
- Added the AppBar navigation component.
- Added the variant property on the Icon component.
- Added the support of Phosphor Icons through the `com.unity.replica.phosphor-icons` package.
- Added the BottomNavBar component.
- Added the FAB component.
- Added the DrawerHeader component.
- Added the NavHost component.
- Added a ListViewItem component.
- Added the Navigation System under the `Unity.AppUI.Navigation` namespace (in its own **non auto-referenced assembly**).
- Added the support of nested Navigation graphs.
- Added the INavVisualController interface to control the content of the navigation components.
- Added the NavController, core component of the navigation system.
- Added a sample called Navigation to demonstrate the navigation system.
- Added the support of Enum properties in the Storybook editor window.
- Added ObservableObject class to implement the INotifyPropertyChanged interface.
- Host and App interfaces to create MVVM apps based on specific hosts.
- Added UITK implementations of the Host and App interfaces.
- Added Dependency Injection support for MVVM Toolkit via constructor injection.
- Added an implementation of a ServiceProvider for MVVM Toolkit.
- Added an App Builder utility MonoBehaviour to create MVVM apps as Unity component in a scene.
- Added RelayCommand and RelayCommand<T> classes to implement the ICommand interface.
- Added AsyncRelayCommand and AsyncRelayCommand<T> classes to implement the ICommand interface.
- Added a sample project to show how to use MVVM Toolkit with App UI.
- Added the MVVM implementation under the `Unity.AppUI.MVVM` namespace (in its own **non auto-referenced assembly**).
- Added a Redux implementation under the `Unity.AppUI.Redux` namespace (in its own **non auto-referenced assembly**).
- Added the UndoRedo system under the `Unity.AppUI.Undo` namespace (in its own **non auto-referenced assembly**).

### Changed

- The namespaces used by the package **has changed** from `UnityEngine.Dt.AppUI` to `Unity.AppUI`. See the migration guide in the documentation for more information.
- `Header` component is now named `Heading`. The old name is still supported but will be removed in a future release.
- CircularProgress innerRadius property is now publicly accessible.
- App UI main Looper instance is now publicly accessible.
- Improved StackView logic to support the navigation system.
- Refactored Avatar component, there are no more containers for notifications and icons.
- Refactored Badge component.

## [0.2.13] - 2023-06-28

### Fixed

- Fixed RenderTextures destruction to avoid memory leaks.

## [0.2.12] - 2023-06-01

### Added

- Added `isExclusive` property to the `Accordion` component.
- Added `shortcut` property to the `MenuItem` component.

### Fixed

- Fixed TextArea input size.
- Fixed ValueChanged events on text based inputs.
- Fixed depth tests in custom shaders for WebGL platform.

## [0.2.11] - 2023-05-12

### Added

- Added `maxDistance` property to the World-Space UI Document component.

## [0.2.10] - 2023-05-08

### Added

- Added resistance property to the `SwipeView` component.

### Fixed

- Fixed `Pressable` PointerDown event propagation.

## [0.2.9] - 2023-05-04

### Changed 

- Removed `Replica` word from the documentation.

## [0.2.8] - 2023-04-30

### Added

- Added transition animations to sliders components.
- Added a `swipeable` property to the `SwipeView` component to be able to disable swipe interaction.
- Added `Preloader` component to the UI Kit sample.
- Added `Link` component.
- Added `Breadcrumbs` component to the UI Kit sample.
- Added `Toolbar` component to the UI Kit sample.

### Fixed

- Fixed `Pressable` event propagation.

### Changed

- Updated USS vars to use the new version of App UI Design Tokens.

## [0.2.7] - 2023-04-24

### Added

- Added `Refresh` method to the `Dropdown` component.
- Added AutoPlay to the `SwipeView` component.

### Fixed

- Fixed the support of the New Input System.
- Fixed the box-shadow shader with portrait aspect ratio.
- Fixed async operations on LocalizedTextElement.

## [0.2.6] - 2023-04-17

### Added

- Added `innerRadius` property to the `CircularProgress` component.

### Fixed

- Fixed the RenderTexture format of the World-Space UI Kit sample.

## [0.2.5] - 2023-03-30

### Added

- Added `SnapTo` and `GoTo` methods in the SwipeView component.
- Added `startSwipeThreshold` property in the SwipeView component.
- Added Scrollable Manipulator which uses a threshold before beginning to scroll.
- Added Pressable Manipulator which can be used to capture a pointer during press but can continue propagation of Move and Up events.

### Fixed

- Removed the use of System.Linq in the App UI package.

### Changed

- SwipeView now uses the Scrollable Manipulator instead of Draggable.

## [0.2.4] - 2023-03-23

### Fixed

- Fixed Localization initialization for WebGL platform.
- Fixed ScrollView tracker styling.
- Fixed Focusable property in constructor for newer versions of Unity.
- Fixed UnityEngine namespace import for newer versions of Unity.

## [0.2.3] - 2023-03-22

### Fixed

- Fixed styling on Scroller.
- Fixed max-height on Dropdown menu.
- Fixed dismissible dialogs in UI Kit sample.
- Fixed flex-shrink value on multiple components.
- Fixed TextField UXML construction.

## [0.2.2] - 2023-03-20

### Fixed

- Fixed NullReferenceException in invalid AnchorPopup updates.

## [0.2.1] - 2023-03-07

### Fixed

- Fixed the localization of the Dropdown component.
- Fixed the sizing of Progress UI components.
- Fixed OutOfBounds dismiss on cascading Popovers.
- Fixed the blocking of placeholder events on TextField and TextArea.
- Fixed compilation error on 2022.2.0a8.

## [0.2.0] - 2023-03-05

### Added

- Added support of Native features such as system themes and scale.
  The support has been done for Android, iOS, MacOS, and Windows.
- Added support of the [Unity Localization package](https://docs.unity3d.com/Packages/com.unity.localization@1.4/manual/index.html).
  You can localize strings from the Application Context or use a
  [LocalizedTextElement](xref:Unity.AppUI.UI.LocalizedTextElement) component to localize strings from the UI.
- Added a global UI component property `preferred-tooltip-position` to set the default tooltip position.
  The property is available by code and UXML.
- Added a App UI manager with a singleton pattern to manage the App UI configuration and lifecycle.
- Added an option in the App UI Settings to auto-override the Android manifest file (Android builds only).
- Added an option in the App UI Settings to enable or disable the usage of a custom loop frequency (Editor only).
- Added a World Space UI Document component to display UI elements in the world space.
- Added Avatar UI component to display an avatar with a name and a status.
- Added a Badge UI component to display a badge with a number.
- Added a BoundsField UI component to define a three-dimensional bounding box.
- Added a BoundsIntField UI component to define a three-dimensional bounding box with integer values.
- Added a Chip UI component to display a chip with a label and an icon.
- Added a CircularProgress UI component to display a circular progress bar.
- Added a LinearProgress UI component to display a linear progress bar.
- Added a ColorField UI component to define a color.
- Added a ColorSlider UI component to choose a value from a range of colors.
- Added a ColorSwatch UI component to display a color.
- Added a ColorWheel UI component to choose a hue from a color wheel.
- Added a ColorPicker UI component to choose a color from a color wheel and a color slider.
- Added a DoubleField UI component to define a double value.
- Added a Drawer UI component to display additional content from the sides of the screen.
- Added the support of `box-shadow` and `outline` using custom USS properties (see [ExVisualElement](xref:Unity.AppUI.UI.ExVisualElement)).
- Added a IconButton UI component to display an icon button.
- Added a LocalizedTextElement UI component to display a localized text.
  Most of the App UI components have been updated to use this component.
- Added a LongField UI component to define a long value.
- Added a Mask UI component to fill an area with a solid color.
- Added a PageIndicator UI component to display dots in a pagination system.
- Added a RectField UI component to define a two-dimensional rectangular area.
- Added a RectIntField UI component to define a two-dimensional rectangular area with integer values.
- Added a TextArea UI component to display a scrollable text area.
- Added the support of expression evaluation in numeric fields (see [ExpressionEvaluator](xref:UnityEditor.ExpressionEvaluator)).
- Added an [ActionTriggered UITK event](xref:Unity.AppUI.UI.ActionTriggeredEvent) that can be triggered by Menu items.
- Added a StackView UI component to display and animate a stack of items.
- Added a SwipeView UI component to display a list of items that can be swiped in a direction.
- Added a PageView UI component which is the combination of a SwipeView and a PageIndicator.
- Added the `Submittable` UI-Toolkit manipulator to handle the submission of Action UI elements via keyboard/mouse/pointer.
- Added the [KeyboardFocusController](xref:Unity.AppUI.UI.KeyboardFocusController)
  UI-Toolkit manipulator to differentiate the focus of a UI element from the keyboard or the pointer.
- Added a MenuBuilder class to create a Menu from code.
- Added the ability for Popover element to use a modal backdrop (Pointer events are blocked).
- Added a simple implementation of a [Storybook](xref:storybook)-like tool to display and test UI components.

### Changed

- The **Application** UI element is now called [Panel](xref:Unity.AppUI.UI.Panel).
- Improved the Slider UI component to display the current value.
- Improved the Tray UI element to be resizable.
- Ability to use the Modal component with any content derived from [VisualElement](xref:UnityEngine.UIElements.VisualElement).
- The App UI main Looper is now part of the App UI manager and is
  not present in the Application UI element anymore.

### Fixed

- Fixed the Notification system when the Notificiation UI element has been destroyed without being dismissed.
- Fixed the Menu system to handle sub-menus.
- Fixed the position calculator of Popups.
- Fixed the Popup system to be able to dismiss a popup when clicking outside of it in an area that is not handled by UI-Toolkit.
- Fixed the formatting of numerical fields.
- Fixed the handle of some edge-cases in TooltipManipulator.

## [0.1.0] - 2022-08-19

### Added

- Package Structure
- First draft of User Manual documentation
- Accordion UI Component
- ActionBar UI Component
- ActionButton UI Component
- ActionGroup UI Component
- Button UI Component
- Checkbox UI Component
- Divider UI Component
- Dropdown UI Component
- NumericalField UI Components
- Header UI Component
- Icon UI Component
- Radio UI Component
- Slider UI Component
- Stepper UI Component
- Tabs UI Component
- Text UI Component
- TextField UI Component
- Toggle UI Component
- TouchSlider UI Component
- VectorField UI Components
- Dialogs & Alerts UI System
- Menu UI System
- Popup UI System
- Notification Manager & Toasts
- Message Queue System (Looper & Handler)
- ContextProvider System
- Tooltip System
