# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.2.7] - 2026-11-15

### Fixed

- `MaskField` shows its localized Nothing text when Unity Localization is installed, like its Everything and Mixed texts.
- The Linux TextMateLib plugin exports only its `textmate_*` C API. It used to export about 1,100 symbols (oniguruma, libstdc++ instantiations, rapidjson) into the Editor process, which could interfere with the Editor's own native code.

## [2.3.2] - 2026-11-15

### Fixed

- `MaskField` shows its localized Nothing text when Unity Localization is installed, like its Everything and Mixed texts.
- The Linux TextMateLib plugin exports only its `textmate_*` C API. It used to export about 1,100 symbols (oniguruma, libstdc++ instantiations, rapidjson) into the Editor process, which could interfere with the Editor's own native code.

## [3.0.0-pre.2] - 2026-11-14

### Changed

- Breaking: `App UI - Alias.uss` defines `--appui-alias-field-padding-horizontal-sm`, `-md` and `-lg` in place of `--appui-alias-control-padding-horizontal-*`, following the design-token rename. The values are unchanged (`var(--appui-sizing-200)`). No UITK component reads them, so nothing changes size.
- `AssetTargetField` and `AssetReference` are now public, and the class covers both the registry's `asset-target-field` and `object-field` — the two registry files are identical apart from `object-field` dropping the size prop and the clear button, so a separate `ObjectField` type would have been a strict subset duplicate. Adds `showClearButton`, `emptyLabel`, `customPicker`, `pickRequested`, `cleared`, `ClearValue()` and `OpenPicker()`, and removes the type label the registry does not have. `type` no longer nulls the value on a mismatch: it compared `AssetReference`'s runtime type against the filter and so could never match.
- `Icon.primary` now defaults to `false`, matching the registry's `icon.tsx`. Every component in the package that builds its own icons asks for `primary = true` explicitly, so nothing in the package or the samples changes appearance; the difference is visible only to callers who construct a bare `Icon` themselves, which is exactly the case the registry governs.
- `InputLabel` now covers the registry's `PropertyField`: it gains `icon` and `capValue`, and its dead `sizeUssClassName` constant is removed (it was declared, never applied and matched by no USS rule). No new `PropertyField` type was added — `InputLabel` already was one, `draggable` already published the `DragContext` the registry calls `scrub`, and a duplicate class would have been the worse outcome.
- `ButtonVariant` now matches the registry's six variants: `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link`. The `quiet` bool folds into `Ghost`, which is the same treatment under the registry's name. Note that `Default` now means the registry's `default`, the solid accent button — the old neutral `Default` became `Secondary` and `Accent` is gone, so every existing `new Button()` changes appearance.
- The design sizing axis is now **density** (`normal`/`compact`/`comfort`) instead of **scale** (`small`/`medium`/`large`). `Panel.scale` becomes `Panel.density`, `ScaleContext` becomes `DensityContext`, and the per-element `scaleOverride` becomes `densityOverride` (UXML attribute `density`). This fixes a live bug rather than only renaming: the tokens package moved to a theme × density matrix some time ago, so `Panel` was writing an `appui--medium` class that no stylesheet had defined and the sizing axis was inert at runtime. `Platform.scaleFactor` is display DPI, is unrelated, and is unchanged.
- `Size` members are renamed `S`/`M`/`L` → `Sm`/`Md`/`Lg`, emitting `--size-sm`/`--size-md`/`--size-lg`. This matches the shadcn registry's control sizes and the `--appui-alias-control-sizing-{sm,md,lg}-*` tokens the USS already consumed, so the class names were the last place the two vocabularies disagreed. `Spacing` is a separate axis with no registry counterpart and is unchanged.
- `Text` gains a colour axis and its sizes match the registry. `bool primary` is replaced by `TextVariant { Default, Secondary, Muted }` bound to `--appui-alias-text-{primary,secondary,disabled}`, and `TextSize` drops `XXS` and re-binds `{XS, S, M, L, XL, XXL, XXXL}` (names unchanged from 2.3, `M` the default), each step binding to `var(--appui-font-sizes-N)` so the ramp responds to density.
- The font-size steps have a 10px floor in every density. `--appui-font-sizes-25` (was 7px) and `--appui-font-sizes-50` (was 9px) are 10px at normal and comfort density. In compact, steps 25, 50 and 75 (were 5, 7 and 9px) are 10px and step 100 is 12px (was 10px), level with step 200. Steps 200 and up are unchanged, so text that reads the small steps, such as `body.xxs`, `body.xs`, `body.sm` in compact and `heading.xxs`, gets larger.
- Icons are now Vector Graphics assets. The 3,750 PNGs under `PackageResources/Icons/` are replaced by the SVGs they were generated from, imported by Unity's built-in `SVGImporter` into UI Toolkit `VectorImage` assets. Icons are now resolution independent — they stay sharp at every `IconSize` and on high-DPI displays, where the 64x64 PNGs used to soften. Tracked size drops from 19.7 MB to 7.7 MB, most of that saving coming from the `TextureImporter` metas.
- The Icon Browser accepts and previews `VectorImage` assets, not just textures. Textures still work for custom icon sets.
- Custom icons are authored as SVG. Two importer constraints are worth knowing, since both fail quietly rather than loudly: `currentColor` is rejected by the importer and its geometry is dropped, and monochrome glyphs must be authored white so `--unity-image-tint-color` can recolor them. Both are documented in `Documentation~/custom-icons.md`. The `@2x` high-DPI guidance is gone — it no longer applies.

  Known limitation: Unity's SVG importer ignores `<filter>` elements, so a handful of icons that relied on one lose that effect and render flat — `ArvrArvr`, `EditorUiPin`, `EditorUiPinned`, `ScriptsShader`, `ScriptsShaderVariantCollection`, the five `MachineLearningMl*` glyphs, and their `d_` dark variants. They import and draw, just without the filtered detail. This is an importer limitation rather than something the icon set can work around.
- The colored Editor icons' dark artwork now rides in `Icons.uss` under those marker classes, and the unused `Icons-Dark.uss` is deleted. That sheet declared its overrides under a bare `.appui`, so it could only apply by being loaded after `Icons.uss` and could not be scoped to a dark theme; nothing imported it. `Icons.uss` is instead partitioned - icons with a single artwork under `.appui`, icons with both under `.light` and `.dark` and nowhere else - so no icon variable is declared twice and the outcome does not depend on USS declaration order. `Icons - Minimal.uss`, the set the package actually loads, is unaffected: none of its monochrome glyphs has dark artwork.
- The Icon Browser generates the same shape. A custom icon with a `d_`-prefixed sibling is emitted into `.light` / `.dark` blocks, and a sheet whose icons have no dark counterpart is generated exactly as before. Reading a sheet back no longer mistakes its `.dark` block for extra icons.
- `Timeline` clips now paint from `--appui-alias-editor-clip`, and frame clips from the `--appui-alias-editor-track-*` set, instead of the generic accent. These tokens are deliberately the same value in every theme mode, so a clip's identity does not change when the accent does.
- Field-shaped controls take their left and right padding from `--appui-alias-field-padding-horizontal-*` instead of `--appui-alias-control-gap-md`, as the registry's fields do. `TextField` and `NumericalField` read the size-matched variant, `Dropdown` and `MaskField` read `sm` at `Sm` and `md` otherwise, `TextArea` reads `md`, and `ColorField` reads `md` on its text edge and a step 2px tighter on its swatch edge, swapped under RTL. **This widens these controls:** the inset goes from 5px to 10px at Normal density (4px to 8px Compact, 6px to 12px Comfort), and from 8px to 10px for `TextArea`.
- `Button` takes its left and right padding from `--appui-alias-actions-padding-horizontal-{sm,md,lg}` by size and `ActionButton` from `--appui-alias-actions-compact-padding-horizontal-{sm,md,lg}`, as the registry does. A `Button`'s icon and title containers no longer pad the outer edges with their own margins; the gap now sits only between two visible parts. **Labelled buttons get wider:** a Normal `Md` `Button` goes from a 5px to a 16px inset, and an `Md` `ActionButton` from 8px to 12px. Icon-only buttons stay square.
- Field text takes the step the registry's field reads, `--appui-font-sizes-100` (its `text-sm`) or `-200` (its `text-base`), instead of `body-md` at every size. **Values and placeholders grow one step at `Md` and `Lg`:** 12px to 14px at Normal density, 10px to 12px Compact, in `TextField`, `NumericalField` and every composite built on it (vector, rect and bounds fields, `IntField`, `FloatField`), `DateField`, `TimeField`, `DateTimeField`, `SearchBar`, the `AssetTargetField` label, the `ColorField` hex and alpha readout, and the `Dropdown`, `MaskField` and `EnumField` trigger. At `Sm` they stay at `-100`; for `SearchBar`, `AssetTargetField` and `ColorField` this departs from the registry, which keeps `text-base` there, because a `text-base` line does not fit a compact `Sm` control. `Dropdown` rows read `-200` at every size, as the registry's `MenuItem` rows do. A `TextField` suffix and a `NumericalField` unit read `-100` at every size. `TextArea` pins `-100`, the registry's desktop step.
- **Breaking:** `BaseDialog.size` (so `Dialog`, `AlertDialog` and `ConfirmDialog`) is now a `DialogSize` (`Sm`, `Md`, `Lg`, `Xl`) instead of the shared `Size`, and `BaseDialog` no longer implements `ISizeableElement`. Replace `Size.Sm`/`Md`/`Lg` with `DialogSize.Sm`/`Md`/`Lg`; UXML `size="Sm|Md|Lg"` is unchanged. The new `Xl` is the registry's 48rem dialog (768px), and every width now reads its `--appui-dialog-width-*` token.
- ClampedContent fades with a real gradient drawn through `generateVisualContent`, coloured by `--clamped-content-fade-color`.
- The test suite now uses the built-in `com.unity.ui.test-framework` for interaction coverage, through a new `UIComponentTests<T>` fixture hosting an App UI `Panel`, alongside the existing `VisualElementTests<T>` story and snapshot harness. Input goes through the framework's `simulate` API, whose calls are synchronous because `FrameUpdate()` runs the real UI Toolkit updater pass against a simulated clock rather than waiting on real frames; interaction tests are therefore plain `[Test]` methods with no `yield return null` and no `TimeUtils.timeOverride`. ActionButton, AccordionItem, Tabs, Dropdown, TouchSliderFloat and SplitView are converted, which retires the two placeholder `Accordion` event tests. A second snapshot harness, `UISnapshotTests<T>`, is built on the framework fixture and compiles only where the bundled package is 6.7 or newer, which is the first version that pins a runtime panel's size and scale to `panelSize`. Snapshots are now filed under a subdirectory of `SNAPSHOTS_OUTPUT_DIR` named for the running editor, and there is exactly one set per editor: a component with a fixture in the new harness sets `capturesSnapshotsElsewhere` so `VisualElementTests` stands down for it there, leaving every other component on the existing harness. Story file names are unchanged and identical across both, so two editors' folders can be compared image for image. Both keep `WaitForEndOfFrame`, because `ScreenCapture` needs a real frame that simulated time cannot produce.
- The `com.unity.ui.test-framework` harnesses now cover the whole component library rather than the six components that piloted them. Every component fixture that captures stories has a `UISnapshotTests<T>` counterpart carrying the same story list, and sets `capturesSnapshotsElsewhere` so `VisualElementTests` stands down for it where the framework path compiles; the pair is verified in both directions, since an editor bundling the package below 6.7 keeps capturing everything on the old harness. Interaction coverage moves from a handful of components to every one with a reachable input surface, as `<Component>BehaviourTests` fixtures on `UIComponentTests<T>`. `UIComponentTests.testPanelSize` is now clamped to the screen: `simulate` builds its events in screen space, so a panel wider than the screen laid out fine and reported a valid `worldBound` while everything past the screen edge silently received no pointer event at all, which in a 640x480 batchmode editor put every right-aligned control — a field's picker icon, a SplitButton's caret, a card's close button — out of reach and failed their tests as though the components were broken. Card, TweakMenu and ZoomTriggerButton stay on the old harness because their stories wait on transitions through `WaitUntil`/`WaitForTransition`, which the framework fixture does not provide and which simulated time would otherwise resolve into a capture of a half-finished animation.
- `AppUIServiceCollection` and its base `ServiceCollection` now throw `InvalidOperationException` on `Add`, `Insert`, `Remove`, `RemoveAt`, `Clear` and indexer changes once `IsReadOnly` is `true`, and `ICollection<ServiceDescriptor>.IsReadOnly` reports `true` then. Services added after `AppBuilder.BuildWith`, for example in `OnAppInitialized`, now throw.
- HelpBox defaults to the Md size.
- `ChatTodoList`'s in-progress glyph is now a static filled ring rather than a `CircularProgress` spinner, matching the registry. A checklist shows several rows at once, and a spinning one pulls the eye away from the text, which is the part that actually changes.
- The story and snapshot harness now captures states it previously could not. A story is a coroutine rather than a synchronous element factory, and it is handed the capture container, so it can mount, yield for layout, then select a generated row, open an overlay, take focus or raise a pointer state before the frame is captured. The container itself now fills the themed `Panel` instead of hugging its content, which is what an absolutely positioned component — `Toaster`, `FloatingWindow`, `TweakMenu` — is framed against; `Toaster` was clipping its whole stack against an effectively zero-height box. Fixtures can also opt out of the inherited bare `new T()` capture, for a component that paints nothing without content or a host. Both harnesses share the contract, and `UISnapshotTests` drives the coroutine off the test framework's updater pass.
- `Menu` arrow keys now wrap, as the registry's menu and dropdown-menu do: Down on the last row moves to the first and Up on the first moves to the last. Home and End jump to the first and last rows, and disabled rows are skipped because they cannot take focus. Up or End on the menu surface itself starts from the last row.

### Fixed

- `Link.size` now actually styles the link. `Link.uss` defined no `--size-*` rules at all, so the property was dead. It also typed `size` as the 7-member `TextSize` while the registry's `linkVariants` offers only `sm | base | lg` — keeping the wider enum with three rules would have left four steps matching no rule, the same silent bug that had broken `Icon`. `size` is now a new `LinkSize { Sm, Base, Lg }`, default `Base`, with each step bound to the matching `--appui-font-sizes-*` token so the ramp is density-aware.
- Three of `Icon`'s five size steps, including the default, rendered with no width or height. `Icon.uss` defines `--size-{xxs,xs,sm,md,lg}` but `IconSize` was `{XXS, XS, S, M, L}`, and the `[EnumName]` generator lowercases member names, so `S`/`M`/`L` produced `--size-s`/`-m`/`-l` and matched no rule. `IconSize` is renamed to `{Xxs, Xs, Sm, Md, Lg}` and every step now binds to its `--appui-alias-control-sizing-icon-*` token rather than a raw `--appui-sizing-*` step. `ToSize()` throws for `Xxs`/`Xs`, which have no control-size equivalent.
- `SwitchBuilder` runs every matching reducer case on the state returned by the previous one, so a slice no longer keeps only the last case's result. Several `AddDefault` calls now chain the same way.
- `Action<TPayload>` instances with different payloads, and async thunk actions with different `meta` or creators, no longer compare as equal through `IEquatable<Action>`. `Action.Equals(Action)` is now `virtual`.
- `StoreFactory.ComposeEnhancers` returns the identity enhancer when it is called with no enhancers instead of throwing.
- The Redux DevTools no longer keep committed actions in `actionsById` when `maxAge` is set, and `Dispatch` accepts a `maxAge` of 0 or 1.
- Async thunks run when DevTools is enabled through `StoreFactory.DefaultEnhancer`.
- `IThunkAPI.Abort` cancels the async thunk instead of throwing when it is called on the main thread or from another thread, and so do `RejectWithValue` and `FulFillWithValue`.
- `TryAddTransient` registers a transient service, and `TryAddTransient(Type)` and `TryAddScoped(Type)` add it only once.
- `NavController.Navigate(Uri)` resolves deep links with two or more path segments.
- `GradientExtensions.TryParse` returns false for a gradient key with a single value instead of throwing, and accepts keys with surrounding spaces.
- `PinchGesture.GetHashCode` is equal for gestures that compare equal.
- `paper-plane-tilt` was missing from the Icon Browser's required-icons list while being present in `Icons - Minimal.uss`, so regenerating that sheet through the browser silently dropped it.

  Icon USS class names, the `--appui-icon-*` variables and the `Icon` component's API are all unchanged. Custom stylesheets that reference the icon PNG paths directly need to swap the `.png` extension for `.svg`.
- Samples, VisualDoc and skill examples read removed `--appui-spacing-*` variables, so Storybook stories lost their padding and the console filled with warnings. They read `--appui-sizing-*` now, and a new editor test fails on any `var(--appui-*)` that nothing defines.
- Slider and RangeSlider thumbs disappeared outside Editor Dark because they read `--appui-color-dark-gray-*` tokens. They read alias tokens defined in every theme.
- ColorSlider thumb sits on a checkerboard so a translucent value never leaves it looking transparent.
- ChatActivityTimeline rows used `clock` and `wrench` icons missing from the minimal icon set, so they rendered blank.
- CodeEditor drew its text twice. The input glyphs are now hidden through USS and the caret blinks again.
- ContextCounter is a pill instead of an ellipse, ChoiceList badge letters are centered, and the selected ChatConversationListItem text is readable.
- VuMeter marker arrow points at the meter, and the CanvasEdge midpoint caret has the right orientation on level step edges.
- The Toggle thumb uses the full width of the pill in both densities.
- Toaster looks like Toast: shadows are no longer clipped, toasts wrap, and entries update in place and fade in and out.
- The first drag of a FloatingWindow keeps the pointer offset instead of jumping.
- Checkbox and Toggle sizes follow the web components. The checkbox box is 12, 16 and 17px at normal density with a 4px radius, the md toggle is 30 x 17px, and compact rescales them through the sizing ramp. The Checkbox and Toggle label gap is `sizing-160` (8px), matching the web `gap-2`.
- `MaskField` shows its localized Nothing text when Unity Localization is installed, like its Everything and Mixed texts.
- `Rect.IsValid` and `VisualElement.IsOnScreen` no longer treat a zero-area rect as valid on engine builds where `Mathf.Epsilon` evaluates to 0.
- The Styling manual page now says how to style a disabled state: `SetEnabled(false)` puts the whole subtree in `:disabled`, so an `opacity` rule multiplies down the tree, and a disabled rule reads the role's `*-disabled` colour variables instead.
- `EnumField(Enum defaultEnumValue)` now populates its items. The constructor guarded the call to `Init` on `defaultValue` rather than on its own `defaultEnumValue` parameter, and `defaultValue` resolves to the inherited `Picker.defaultValue`, the array of default selected indices. Since the constructor chains through `base(null, ...)`, that array was always null, the guard was never true, and `PopulateDataFromType` never ran — so the constructor the class documents as the preferred one produced a field whose menu had no items and could not open, because `Picker.OnClicked` bails out when `sourceItems` is empty.
- `FloatingWindowTests` no longer depends on the display's DPI scale. The clamp test asserted an absolute starting position for a window restored at 400,300 with a 200x150 box, but the host it is clamped against fills the panel, whose size is the screen divided by its DPI scale — so a 1200x600 screen is a 1200x600 host on a 1x display and only 600x300 on a 2x one. On a 2x display the saved position no longer fit, `ClampPosition` pulled the window up to y=150, and the test failed in a player while passing in the editor. `Mount` now takes an optional host size, which that test pins, with `flexShrink` disabled so a panel shorter than the requested host does not pull it back down.
- The Linux TextMateLib plugin exports only its `textmate_*` C API. It used to export about 1,100 symbols (oniguruma, libstdc++ instantiations, rapidjson) into the Editor process, which could interfere with the Editor's own native code.
- A navigation destination with no template set no longer throws `ArgumentNullException` from `DefaultDestinationTemplate.CreateScreen`. `Type.GetType(null)` throws rather than returning null, so an unset template crashed instead of falling back to the default screen, and the "template type is not set" warning was only ever reachable via an empty string.
- The native platform P/Invoke declarations that return a `bool` now pin one-byte marshalling with `[return: MarshalAs(UnmanagedType.I1)]`. Without it a `[DllImport]` returning `bool` marshals as the 4-byte `UnmanagedType.Bool`, while every `NativeAppUI_*` entry point returns a 1-byte C++ `bool`; on x86-64 `setcc %al` writes only the low byte, so register residue in bits 8-31 could read back as `true` when the native answer was `false`.
- The theme dropdown shown for `Panel.theme` and `BaseVisualElement.themeOverride` offered only four of the six shipped themes: `cloud-dark` and `cloud-light` were missing, despite both shipping as `.class.uss` files and being wired into `App UI.tss`. All six are now listed, hyphenated names render as "Editor Dark" rather than "Editor-dark", and a theme name the dropdown does not recognise — one defined by a hand-written stylesheet, say — is preserved instead of being silently rewritten to the first entry.
- A `Thread`'s action menu button can now be shown. `Thread.uss` set `display: none` on `.appui-thread__actions` and never lifted it, so the button was unreachable no matter what: assigning `makeActionMenuItems` cleared the hidden class, but the USS rule still kept the element out of layout, leaving the menu impossible to open. The stylesheet now carries the hover and `--actions-open` reveal rule that `ThreadMessage.uss` already had, so the button appears while the thread is hovered and stays visible while its menu is open.
- `Store.Dispatch` no longer throws `InvalidOperationException` when a listener subscribes or unsubscribes while it is being notified. Subscribers are now notified from a snapshot, and a subscription that is unsubscribed while it is being notified no longer throws `NullReferenceException`. Its listener may still be called once.
- `IDisposableSubscription.IsValid()` returns `false` instead of throwing a `NullReferenceException` once the subscription or its store is disposed.
- The `Optional<float>`, `Optional<double>` and `Optional<Rect>` UXML converters, `RectExtensions.TryParse`, `GradientExtensions.TryParse` and the float sliders now read and write numbers with the invariant culture, so `0.5` is no longer read as `5` under a decimal comma culture. Values saved with the current culture by earlier versions, such as `0,5` under `de-DE`, still load, and the sliders' input field still accepts them. The `Optional<int>` and `Optional<long>` converters read the same way.
- `DateConverter` and `DateRange.TryParse` now parse serialized dates with the invariant culture, so dates no longer swap day and month or shift calendars under other cultures.
- `DatePicker` rebuilds its day grid for months before 1970, and keeps it valid at the first and last supported month, where the outward month, year and decade arrows are now disabled instead of throwing.
- `Slider` arrow keys with `restrictedValues` set to `Step` or `CtrlStep` now move by the right step with Shift, step without Ctrl for `CtrlStep`, and no longer throw with Ctrl.
- Dragging with touch and pen pointers works again on `Draggable`-based controls such as sliders, `SplitView` and `Tray`.
- `ActionGroup` children and its overflow button now follow an inherited `visibility: hidden` instead of forcing themselves visible.
- `Radio` registers with its `RadioGroup` again when it is re-attached to a panel, so radio buttons keep working and keep their selection in content that is shown more than once, such as a `Popover` or `DialogTrigger`.
- Setting `UndoStack.index` to `UndoStack.count`, or to `0` on an empty stack, now throws `ArgumentOutOfRangeException` instead of hanging.
- `UndoStack.isClean` no longer reports `true` after the clean state was reset with `ResetClean`, dropped by a `Push` or evicted by `undoLimit`, including when the clean state was the initial one. A `Push` that drops the clean command now raises `cleanStateChanged`, and `ResetClean` raises it only when `isClean` or `cleanIndex` changes.
- `AsyncRelayCommand<T>` with `AllowConcurrentExecutions` now cancels the previous run when `ExecuteAsync` is called again, so `Cancel()` reaches every run.
- `AsyncRelayCommand` and `AsyncRelayCommand<T>` now raise `CanExecuteChanged` and their completion `PropertyChanged` notifications when a run is cancelled, instead of throwing `TaskCanceledException` and leaving bound buttons disabled. A faulted run also raises them now, and its exception is still logged, including an `OperationCanceledException` that did not come from cancelling the run.
- `App.current` is no longer cleared when a different `App` instance is disposed or finalized.
- `Handler.RemoveCallbacksAndMessages` no longer removes messages sent through other handlers that share the same `Looper`. Messages whose target is not the calling handler, such as one from `Message.Obtain(null, ...)`, are no longer removed by it either.
- `Message.Obtain` keeps reusing pooled messages after more than 50 messages have been recycled.
- `NavDestination.MergeArguments` now replaces a default argument with a provided argument of the same name instead of keeping both.
- `NavGraphViewAsset.FindDestinationByRoute` no longer throws `InvalidCastException` when the route names a navigation action. It returns `null`, or the destination of that name if there is one.
- `ColorField` no longer throws `ArgumentException` when `hdr` is `true` and a channel is above 1. `ColorExtensions.ColorToRgbaHex` now clamps each channel.
- The `Breadcrumbs` separator no longer shrinks. A cramped bar squashed the `/` separators to nothing instead of ellipsizing the crumb labels, which is the one thing in the trail that cannot afford to lose width.
- The component design tokens now reach the UPM package. `generate-uss.js` copies the tokens' `components.uss` to `Themes/App UI - Components.uss`, and every theme's `.tss` imports it after the Alias sheet and before the component sheets, so a value a component sheet already defines still wins and nothing changes on screen. UITK's Slider progress now reads `--appui-slider-progress-background-color`, which resolves to the same accent as before in every theme.
- Eight icons no longer lose their outline in UI Toolkit. `AnimationAnimationClip`, `AnimationAnimator`, `AnimationAvatarMask`, `AnimationLegacyAnimation`, `AnimationPlayableDirector`, `AnimationTimeline`, `EditorUiLog` and `MachineLearningMlBrainPlayer` were Figma "outside stroke" exports, where the expanded stroke is a separate path carrying `mask="url(#path-N-outside-N)"`. Unity's importer implements `<mask>` as a geometric clip, so that path was dropped and each glyph rendered as a thin skeleton of itself. They are reauthored as plain filled paths; measured against a player capture the eight regain between 18% and 77% of their ink, and each rasterizes to within a per-channel delta of 1 or 2 of the original artwork. Reported upstream as UUM-153815.
- `IconContactSheetTests` pins its own `PanelSettings` to `ConstantPixelSize` at scale 1 instead of inheriting the shared fixture's `ConstantPhysicalSize`. That mode scales the panel by `Screen.dpi / referenceDpi`, which a headless CI agent resolves to 1 but a Retina workstation resolves to 2, so half the grid fell outside the capture and every cell held a different icon than the oracle's. The sheets still came out the right size and nothing failed, so the comparison simply reported several hundred icons broken. The grid is a pixel contract with the oracle rasterizer, so the fixture now fixes the scale rather than inheriting the display's.
- `PlatformAds` and `EditorUiMarker` render whole in UI Toolkit. `PlatformAds` carried a stray zero-area path far outside its `viewBox`, and the importer sizes a `VectorImage` from tessellated geometry rather than the declared `viewBox`, so the glyph was squashed into a third of its cell. `EditorUiMarker` was a Figma "outside stroke" export whose outline path was masked, and the importer implements `<mask>` as a geometric clip (`SVGParser.ParseMask` assigns the mask's shapes to `SceneNode.Clipper`), which dropped two thirds of the artwork. Both are reauthored as plain filled paths that rasterize identically to the originals. Reported upstream as UUM-153815.
- Dialog secondary, cancel and close buttons follow their semantic variants again instead of all rendering accent.
- Popovers no longer dismiss as soon as they open, and ZoomableThumbnail opens a dismissable Modal.
- ContextCounter is a true pill, ChoiceList badge letters are centered, and DropZone and VuMeter samples render with spacing.
- Component text sizes follow the web registry tiers (for example Button, Tabs, Menu and HelpBox text is 14px at normal density instead of 12px).
- Component gaps, paddings and icon sizes match the web registry at every density: Button, ActionButton, SplitButton and Dropdown icon gaps, TextField adornments, Tabs, Menu items, HelpBox, Toast, Badge, Chip, Kbd and ActionGroup.
- The panel root text size is 14px at normal density (12px compact) like the web, so unsized labels no longer render one tier smaller.
- App UI shaders no longer fail to compile on Unity 7000, where the `UnityCG.hlsl` / `UnityUI.hlsl` includes they expected do not exist; they now include `UnityCG.cginc` on every Unity version.
- Fixed Tabs throwing "Cannot scroll to a VisualElement that's not a child of the ScrollView content-container" when TabItems are deleted from it in UI Builder. Tabs now picks up TabItems added to or removed from its item container directly, and keeps the same tab selected when it can (UUM-154224).
- `AssetTargetField`'s leading icon had no styling at all. The stylesheet targeted `__leading-icon` while the C# added `__icon`, so no rule ever applied. A test now pins that every class the C# adds has a matching USS rule.
- `CardGrid` now lays out correctly under `.appui--rtl`. It computes its column arithmetic in C# because UI Toolkit has no CSS grid, and the gap was written only as `margin-right`, so a right-to-left panel flowed from the wrong edge. It now tracks `DirContext` the way `SplitView` does, flips the row in USS the way `Breadcrumbs` does, and always writes both margins so a runtime direction change leaves nothing stale.
- The documentation now describes the v3 API. The UITK code panes still showed `Size.S/M/L`, `TextSize.XS…XXXL`, `ButtonVariant.Accent` and `Text.primary`, and Button's variants pane listed three while the prose above it promised six. The "Platform Scale" fundamentals page is retitled "Density" — its body already documented density, only the title and the pages linking to it still said scale.
- `GradientEditor` no longer reports its own internals' change events as its value changing. `ColorSwatch`'s setter sends a bubbling `ChangeEvent<Gradient>`, and the editor refreshes two swatch-based children on every edit, so a single change reached a consumer three times and merely selecting a stop reached it once — carrying the swatch's gradient rather than the edited one.
- `ObjectPicker` no longer leaks its internals' change events to consumers. Its value is an item id, and so is what the search field and every preview text element report as they bubble — a consumer saw four change events for one selection, with preview metadata reading as selection changes. The picker now stops foreign string events at its boundary.
- Fixed the navigation graph code generator emitting node names without escaping. Names containing `-`, `.`, a leading digit, a C# keyword, or a double quote previously produced a file that either failed to compile or injected arbitrary members into the generated source. Node names are now converted to valid, unique C# identifiers and emitted as properly escaped string literals.
- The AI Agent Skills panel in the Package Manager now only appears for the App UI package, instead of for any installed package that happens to ship a `Plugins~/skills.json`. Skill installation is also now confined to the project: the search for a `.claude` directory no longer walks above the project root, so skills are installed to `<project>/.claude/skills` rather than potentially to a shared `~/.claude/skills`.
- `DrawerHeader` and `ThreadComposer` render correctly at normal and comfort density again. `DrawerHeader.uss` sets `min-width: var(--appui-sizing-3000)` and `ThreadComposer.uss` sets `max-height: var(--appui-sizing-2000)`, but neither step existed outside `.appui--compact`, so both properties silently resolved to nothing for anyone on the default density. Picked up by regenerating the theme USS after the tokens ramp was made uniform.
- `TimeSpan` and `DateTime` UXML attribute converters are now registered. Unity ships neither, so a `value=` attribute on a time or date-time field silently did nothing.
- Fixed XML documentation on `AssetTargetField`, `AudioWaveform`, `AnimationCurveField`, `CurveEditor`, `ColorGradientField`, `GradientEditor`, `ObjectPicker`, `Timeline`, `TreeList` and `VuMeter`, which failed PVP-150-1. Loose prose and `<list>` elements sitting directly under `<remarks>` are now wrapped in `<para>`, `Timeline.zoom` no longer nests `<see>` inside `<c>`, and `TreeList`'s two `<example>` blocks are merged into one.
- The UI Kit sample's TWEAK MENU section no longer fails the player build. It set `min-height` through an inline `style="min-height: var(--appui-alias-control-sizing-lg-height);"` attribute, but UI Toolkit resolves `var()` against the element's stylesheets and an inline style has no stylesheet context, so the variable resolved to null and `VisualTreeAsset.CloneTree` threw a `NullReferenceException` out of `StyleVariableResolver.ResolveVariable`. The scene failed to open with "The UXML file set for the UIDocument could not be cloned", which took the StandaloneLinux64 player build down with it. The value now comes from an `.example-section-min-control-lg` rule in `Examples.uss`, where the token resolves.
- `Timeline`'s `fitWidth` now divides the width left after the fixed `origin` and `endPadding` insets rather than the whole viewport, so a fitted axis actually fits instead of overflowing by 40 px and staying pannable.
- A fixed-size `FloatingWindow` is re-clamped when only its container resizes. `GeometryChangedEvent` was registered on the window itself and does not bubble down from the parent, so a window whose own geometry never changed could be left hanging off the edge of a shrunken container.
- `ObjectPicker` keeps previewing the selected item when the current search or tab filters its row out of the list. The preview resolved the item from the visible rows, so filtering appeared to clear an assignment that was still set.
- Removing a selected `TreeViewItem` now goes through the normal selection path: the detached row loses its `selected` flag instead of coming back highlighted when reused, an `allowNoSelection = false` tree falls back to a visible row instead of reporting an empty selection, and `selectionChanged` is raised.
- Arrow-key stepping in `DateTimeField` no longer throws at `DateTime.MinValue` or `DateTime.MaxValue`, both of which are legal values for the field. Stepping past the boundary saturates.
- `GradientEditor` and `CurveEditor` report the loaded value as `previousValue` on the first interactive edit. `SetValueWithoutNotify` cleared the baseline, so the first change after initialization — including the normal `Show(..., initialValue)` path — handed consumers a null previous gradient or curve.
- A queued `ConfirmDialog` request whose host element left the panel before its turn no longer strands the queue. `Modal.Build` threw with the queue already marked busy, so that request never resolved and every later confirmation stayed queued forever. Detached hosts are now skipped and resolved, and a failure to show releases the queue.
- `ImageLightbox` shows a `VectorImage` instead of its fallback. `hasImage` asked `NaturalSize()`, which deliberately reports zero for a vector image because it has no pixel resolution to cap against — a sizing question that was deciding whether an image existed at all.
- Seven capture cases that were blank or duplicated another story. `TweakMenu.CloudDark` set its theme and density before mounting, which `OnAttachedToPanel` then overwrote with the host panel's, so it photographed the same closed trigger as `Default`; it now sets them after attachment and opens the menu. `ListBox.MultipleSelection` and `TreeList.Multiple` only set `selectionType`, which has no resting appearance, and now select two items once the rows exist. `Card.Interactive` was pixel-identical to `Default`, since `interactive` only changes the cursor at rest, and now carries its hover state. `ZoomTriggerButton` rests at `opacity: 0`, so its standalone `Default` could only ever be blank; it is replaced by `AlwaysVisible` and `Focused`, the latter waiting out the 125 ms reveal transition. `Breadcrumbs` and `Accordion` captured empty containers and now carry content, `Heading` and `LocalizedTextElement` captured empty text and now carry some, and `Spacer` and `ExVisualElement` — which paint nothing in any theme — contribute no capture at all. Each of these is now pinned by a test that asserts the story establishes the state it is named for, since a capture asserts nothing on its own.
- A story that lays out to nothing now fails instead of writing a blank capture. Auditing the fixtures once would only have held until the next empty container, so the harness refuses the capture and names the fixture; a component that genuinely paints nothing at any size says so through `hasDefaultStory`. That turned up 21 more fixtures inheriting a contentless `new T()`: `Menu`, `MenuSection`, `RadioGroup`, `AvatarGroup`, `StackView`, `SplitView`, `PageView`, `SwipeView`, `Canvas` and `MarkdownView` captured empty containers, `BreadcrumbItem`, `FieldLabel`, `HelpText` and `InputLabel` captured empty text, `PageIndicator` captured no pages, and `ColorWheel`, `SVSquare` and `Mask` are shader-drawn with no intrinsic size, so an unsized one had no box to draw into. All now carry content or a size. `NavHost`, `DialogTrigger` and `MenuTrigger` are anchors and handlers with no visual of their own and contribute no capture.
- Three stories were captured mid-transition, which the earlier fix for `ZoomTriggerButton.Focused` had only found in that one place. `TweakMenu.CloudDark` yielded once after opening its menu, but a `Popover` fades in over 150ms, so the capture caught it at 18% opacity; `Card.Interactive` yielded once after raising its hover class, against a 200ms background-color transition. The harness now offers `WaitUntil` and `WaitForTransition`, both bounded by wall clock rather than a frame count, because a headless frame is a fraction of a millisecond and no plausible number of them adds up to a transition. `Card`'s counter-check also compared the attached card against a freshly built, unattached one, whose styles never resolved — it would have passed with the hover rule deleted; it now compares the same card before and after.
- A disabled `Dropdown` or `MaskField` item dimmed with 0.5 opacity, which compounds down a disabled subtree. It now takes `--appui-alias-text-disabled` for its text, glyphs and checkmark, at full opacity.
- Disabled `MenuItem`, `Chip`, `Slider` and the `DatePicker` month and year buttons now paint the role's `*-disabled` colour tokens instead of dimming with opacity. A disabled subtree matches a `:disabled` rule at every level, so the opacity compounded, and `MenuItem` applied it on top of its disabled colours. Menu rows read `--appui-alias-text-disabled` (a selected row keeps `--appui-alias-actions-selected-background-color-disabled`), chips read `--appui-alias-actions-{background,border,foreground}-color-disabled`, and the month and year buttons read `--appui-alias-text-disabled`. The slider's track reads `--appui-alias-field-background-color-disabled`, its range and thumb `--appui-alias-text-disabled`, and the thumb rim and marks `--appui-alias-control-border-color-disabled`. `ColorSlider` still dims with opacity, because its track and thumb show a colour value. A menu row's shortcut now reads `--appui-alias-text-tertiary` while enabled, rather than a disabled colour.
- `InputLabel`'s label now reads `--appui-alias-inspector-label-size` and `--appui-alias-inspector-label-color`, as the registry's `PropertyField` label does, instead of the form-label size a standalone `FieldLabel` uses. At normal density it renders at 14px rather than 12px. A standalone `FieldLabel` is unchanged. The inspector label line-height has no USS equivalent and is not read.
- A hovered `Toast` action now fills with `--appui-alias-actions-quiet-background-color-hover`, the hover of a transparent action, as the registry's toast action does, instead of the raw `--appui-color-transparent-black-100` palette step. That fill is opaque, so the hovered action text takes `--appui-alias-actions-quiet-foreground-color-hover` with it, keeping it readable on the coloured variants in a light theme. A pressed action pairs `--appui-alias-actions-quiet-background-color-active` with its quiet foreground the same way, in place of `--appui-color-transparent-black-300`.
- The `ColorField` opacity readout was as wide as an `Lg` control is tall, 30px at Normal density, which once the field inset was added left the value no room at all. It now reads `--appui-sizing-1350`, the step nearest the registry's `w-16`, so a full `100` fits at every size and density. `ColorPicker`'s hex field is now `Sm`, as the registry's value field is.
- A `Chip` with a long label now cuts the label on one line with an ellipsis and stays inside a narrower parent, as the registry's chip does. Before, the chip refused to shrink and overflowed its parent with the whole label.
- A pan on a `Canvas` no longer reaches the content under the pointer, as the registry's canvas does. A middle-button press, or a primary press with Space held (Alt in the Editor control scheme), is now taken in the capture phase, so whatever sits under it is never pressed, selected or dragged. Before, a child that handles the press, such as a draggable element, took the press first and the canvas never panned.
- `Dialog` and `AlertDialog` keep their actions inside the dialog, as the registry's dialogs do. Actions too wide for one row now wrap onto another instead of running past the dialog's edge, content taller than the modal scrolls between the heading and the actions instead of pushing them out of reach, and the title wraps a long unbroken token such as a URL instead of clipping it, up to about four lines. The content now sits in a `ScrollView` with the class `appui-dialog__content-scroll`, which carries the content's top margin, and the dialog's horizontal padding is split between the dialog and a 4px gutter on each part so a focused control's outline is not clipped; a stylesheet that targeted `.appui-dialog > .appui-dialog__content` or its margin needs to target the scroll view instead.
- An emphasized `Checkbox` that is checked or indeterminate now paints its box and mark from `--appui-checkbox__box-checked-{background,border,foreground}-color`, as the registry checkbox does, instead of the raw `--appui-accent-100` step. Cloud Light and Editor Light, whose accent alias is not that step, now show the intended fill, and a theme that overrides the checked-box tokens reaches UITK.
- `ColorPicker` brackets a drag on its wheel, saturation-value square or any channel slider as one gesture, as the registry's color picker does. It now implements `INotifyValueChanging<Color>` and sends `ChangingEvent<Color>` while the pointer is down and a single `ChangeEvent<Color>` at release, so undo and value-changed callbacks see one change per drag instead of one per pointer move. A key press on a sub-control is still a discrete edit and commits at once. A `ColorField` picker, inline or in a popover, still previews live through the changing events, and a drag that loses the pointer to another element (a `SwipeView`, say) still commits.

### Added

- Added `ListBox`, the container `ListViewItem` was missing. It is a single-column `GridView`, so it inherits the App UI collection contract — `itemsSource`, `makeItem`/`bindItem` recycling, row virtualization, `selectionType` (single by default, matching the registry's `selectedId` model) and keyboard navigation — rather than wrapping `UnityEngine.UIElements.ListView`. `ListBox.size` publishes a `SizeContext` so every realized `ListViewItem` adopts it, and with no `makeItem` the list builds `ListViewItem` rows titled from the data item. New `ListBox.uss` binds each size step to the control-sizing height aliases, and `ListViewItem.uss` now takes its hover and selected backgrounds from the control and actions-selected aliases instead of a raw background step. It is named `ListBox`, not `ListView`, so a file that imports both `Unity.AppUI.UI` and `UnityEngine.UIElements` still resolves `ListView` to UI Toolkit's.
- Added `TreeList`, the container `TreeViewItem` was missing: expand/collapse, the three selection modes, and keyboard navigation over the visible rows (arrows, Home/End, Right to expand or step in, Left to collapse or jump to the parent, shift-extend, action+A, Escape). Selection matches the registry — `selectedItems` is primary-first, a shift-range puts the clicked item first, and the anchor holds across repeated shift-clicks. Like `ListBox`, it is named `TreeList`, not `TreeView`, so it never shadows `UnityEngine.UIElements.TreeView`.
- Closed the remaining `TreeList` / `TreeViewItem` gaps against the registry. `TreeList` gains
  drag-to-reorder — an `onReparent` handler that gates the whole surface, a `Dragger` built the way
  `BaseGridView` builds its own, before/after/inside drop positions read from the row's 25% edge bands,
  per-row drop indicators, and a typed `TreeListDragPayload` carrying `TreeList.treeItemMimeType`
  (`application/x-appui-tree-item`) in place of the web's `TREE_ITEM_MIME` — plus `onDeleteSelection`
  on Delete/Backspace with no default behaviour. `TreeViewItem`
  gains an `icon` slot, a public `labelElement` in place of the web's `labelClassName`, a `trailing`
  slot with `trailingVisible`, hover reveal, a measured label reserve and a narrow-row overflow that
  collapses the strip into a `Popover`, `caretPosition` (`Start` by default, matching the registry,
  where UITK previously hardcoded an end caret) and `draggable`. Breaking: the caret now sits in a
  leading gutter by default rather than after the label, and the row markup gained a
  `__caret-gutter`, `__icon`, `__trailing-container`, `__overflow-button` and drop-indicator elements.
  The web's gradient label fade has no UI Toolkit equivalent, so an over-long label ellipsises and the
  row reserves the measured strip width instead of letting text run under the actions.
- Factory registration for the dependency injection container: `ServiceDescriptor.Singleton`, `Scoped` and `Transient` accept a `Func<IServiceProvider, object>` (or a generic `Func<IServiceProvider, TService>`), and `AddSingleton`, `AddScoped` and `AddTransient` have matching overloads. A factory can forward to another registration, so one singleton can be exposed under two interfaces. `ServiceDescriptor.Singleton<TService>(TService instance)`, a public `ServiceDescriptor(Type, Func<IServiceProvider, object>, ServiceLifetime)` constructor and the `ServiceDescriptor.implementationFactory` property were added too.
- `components.dialog.width.{sm,md,lg,xl}` (`--appui-components-dialog-width-{sm,md,lg,xl}`, `--appui-dialog-width-{sm,md,lg,xl}` in USS), the widths of a small, medium, large and extra-large Dialog: 400px, 500px, 640px and 768px.
- `RichTextUtils.Escape` and `RichTextUtils.NeutralizeNoParse` for safely embedding untrusted text in UI Toolkit rich text, and `HrefUtils.IsSafeHref` for validating markdown link destinations against an allow-list of schemes. `IMentionProvider.ConvertToRichText` implementations should use `RichTextUtils.Escape` on every user-supplied segment.
- A ScriptedImporter for `.appuitheme.json` files exported by the App UI Theme Builder. Dropping one into a project produces an `AppUIThemeAsset` carrying a compiled `StyleSheet` and `ThemeStyleSheet`, so a custom theme no longer has to be hand-written USS. Apply one with `panel.styleSheets.Add(theme.styleSheet)` followed by `panel.theme = theme.themeName`, which layers over the active App UI theme so undefined tokens keep their base values, or assign its `themeStyleSheet` to `PanelSettings` to replace the theme file outright. Imported themes also appear in the theme dropdown in the inspector. A Theme Builder theme is a family exported as one file per appearance (`ocean-dark`, `ocean-light`); besides App UI's tokens a file can carry the consumer's own custom properties under their full `--name`, and values that are lengths, numbers or a single `var()` reference, which is how a density-relative metric such as `var(--appui-sizing-500)` reaches USS.
- `ColorField` gains `showAlphaValue`, an opacity readout beside the hex as an editable, scrubbable percentage that rewrites the value's alpha, so the readout and the picker's alpha slider always agree. It needs `showAlpha`: a field with no alpha channel has no opacity to show, so clearing `showAlpha` hides the readout whatever the flag says, while the flag itself survives the channel going away. The registry's hex-shorthand expansion has no counterpart here because the value is a `Color`, not a string; what does carry over is that a readout event matching the current percent is ignored, so editing another channel cannot quantize an alpha nobody touched. The readout rides the field's own rim with a single hairline rather than drawing a second one.
- `App UI - Alias.uss` defines `--appui-alias-control-padding-horizontal-sm`, `-md` and `-lg` (all `var(--appui-sizing-200)`), the control horizontal inset added to the design tokens. No UITK component reads them yet: UITK fields still take their inset from `--appui-alias-control-gap-md`, so nothing changes size.
- `InputLabel`, the package's property row, gains `rowLayout` with an `InputLabelRowLayout.Grid` mode plus `columns` and `tall`, matching the registry's `PropertyField layout="grid"`. The row sits on the inspector column grid from the new `alias.inspector.*` tokens, so labels and controls line up down a whole panel and a long label truncates instead of pushing its control out of column. The property is `rowLayout` rather than `layout` because `VisualElement.layout` already exists and hiding it would be a bug, not a rename. UI Toolkit has neither CSS grid nor `calc()`, so the label cell's share of the row is computed from the two column-count tokens and applied as a percentage basis — the column edges still coincide across rows, and the gap comes out of the control cell, which is the one that grows. A grid row moves its leading icon inside the label cell, since an icon beside it would put that row's control one icon-width off the column above. `columns = InputLabelColumns.Even` shares the control cell evenly between the children rather than handing out subgrid spans, which UI Toolkit has no equivalent for; a child that needs a bigger share sets its own `flex-grow`.
- The theme stylesheets carry the six new `alias.inspector.*` variables: `--appui-alias-inspector-row-height`, `-row-gap`, `-section-gap`, `-grid-gap`, `-grid-columns` and `-grid-label-columns`. They state the inspector rhythm once, so every property row and every group in a panel resolves the same values, and the four sizing-backed ones rescale with density like the rest of the ramp. Regenerated from `@unity/app-ui-tokens` rather than hand-edited.
- Added `PropertyGroup` and `PropertyGroupStack`, the collapsible group an inspector panel is built from. The title sits on the text line with no caret and a closed group that holds rows shows an ellipsis instead, so a folded section still says it holds something; an empty group is not a disclosure at all, so its toggle is disabled and shows no ellipsis rather than claiming to be open. `trailingContainer` is the slot at the end of the header line, outside the toggle, and the hover fill moves from the toggle to the header row when it is used, so hovering the actions lights the same pill as hovering the title. UI Toolkit has no `gap`, so both the row rhythm and the section rhythm are a leading margin with the first child tagged, refreshed on layout the way `SidebarSlot` does it.
- Every slider built on `BaseSlider` gains `scrubStarted` and `scrubEnded`, plus a `scrubbing` flag: the bracket around one pointer drag that a caller needs to open and close a single undo group while the value streams. An end always follows a start. A pointer released off-window, a capture stolen by another element, a cancelled gesture and the slider leaving the panel mid-drag all close the bracket, because the `Pressable` deactivation paths never reach the track's own pointer-up handler and an undo group would otherwise stay open for the rest of the session. Keyboard steps are not a gesture and raise neither, and a drag that ends on the value it started from still closes its bracket.
- Added `SplitButton`, one bordered control in two parts with the secondary `Button`'s chrome, so a toolbar row of plain buttons and split buttons reads as one row. The registry ships three components because the caret is almost always somebody else's trigger; here it is one element with two press targets, because the trigger contract does not exist in UI Toolkit: `clicked` runs the action, `caretClicked` is the cue to show the options, and `caretElement` is what a `Popover` or a menu anchors to. `selected` makes the main part a toggle and the element never flips it itself, so the state stays the caller's, and the selected fill stops at the main part since the caret offers options and carries none of the state. The registry's adjacent-sibling rule for the glyph-to-label gap becomes a class the component sets, because an unsupported USS selector fails the whole stylesheet rather than its own rule.
- `TreeList` gains `caretIcon`, the disclosure glyph every row in the tree draws, so a denser tree can ask for a shallower chevron in place of the default caret. The glyph still rotates with the row's expansion and the caret gutter keeps its width, so this is a swap inside the existing row anatomy. The tree hands the name down as a context rather than walking its rows, which is what makes a row added later draw it too, and `TreeViewItem`'s caret is now an `Icon` so the glyph comes from an icon name instead of a hardcoded background image.
- `TreeViewItem` gains `labelBadge` and `labelBadgeLabel`: a glyph that belongs to the name — a startup-scene star, a dirty marker — sitting right after the label so it follows the name's length, unlike `trailingContainer`, which pins to the row's end. A badged row stops its label from growing, or the badge would be carried out to the trailing edge and read as an action. The reserve that keeps text clear of the trailing strip moves onto the badge when there is one, since leaving it on the label would push the badge under the very strip the reserve exists to avoid. `labelBadgeLabel` is the words for what the badge means, carried as its tooltip, because a glyph on its own tells a first-time reader nothing and the badge is too small to label in place.
- `com.unity.modules.vectorgraphics` as a dependency. It is a built-in module shipped with the Editor since Unity 6, and the package already required Unity 6000.3, so there is nothing extra for consumers to install.
- `dark` and `light` context USS classes, applied alongside `appui--<theme>` by every element that provides a theme context - `Panel`, `ContextProvider` and any component with `themeOverride` set. `appui--editor-dark` says which theme is active; `dark` says what kind of surface it is, so a single selector covers every dark theme instead of enumerating `appui--dark, appui--editor-dark, appui--cloud-dark` and still missing custom ones. A theme counts as dark when its name ends in `dark`, so a consumer's `acme-dark` works with no registration. The two markers are mutually exclusive, and they sit on the providing element, so a nested `ContextProvider` overriding the theme carries its own and USS variables resolve from the nearest ancestor - a light island inside a dark panel gets light artwork.
- Added `AudioWaveform` and `VuMeter`, both drawn with `Painter2D`. `VuMeter`'s bar fill is a banded safe/warning/clip gradient that USS cannot express, so it is drawn as three filled rects per bar; the registry's controlled/uncontrolled fader triple collapses to one clamped property plus an event. Both bind every painted colour to a token by meaning rather than by hue — the registry hardcodes the Tailwind palette for these, which does not adapt to theme.
- Added `CanvasNode`, `CanvasEdge`, `SelectionRect` and `RectAnchorPresets`. They live inside the existing `Canvas` and add no second pan/zoom system — the registry's `zoom` prop is dropped because it exists only so the web can divide pointer deltas by hand, while `WorldToLocal` already carries the transform. `CanvasEdge` picks against the sampled curve rather than its bounding box, so a click near the line hits and a click in the bend's empty space does not. `CanvasNode`'s exported layout constants are drawn from the _static_ sizing ramp, which no density class overrides, because compile-time constants would otherwise drift under `.appui--compact`.
- Added `Card` (with `CardMedia`, `CardHeader`, `CardTitle`, `CardDescription`, `CardContent`, `CardFooter`) and `CardGrid`. `Card` carries the registry's `interactive` and `selected` states plus an additive `elevation` that defaults to `0`, so out-of-the-box parity holds. `CardGrid` reproduces `repeat(auto-fill, minmax(minItemWidth, 1fr))` by computing the column count on resize, since UI Toolkit has no CSS grid; note that it writes `width`, `marginRight` and `marginBottom` on its children, and is LTR-only for now. The registry's `ring-2` selected treatment becomes a permanently-reserved 1px border that turns accent when selected, because UI Toolkit has no ring and no 2px length token.
- Added the display half of the chat suite: `ChatMessageBubble`, `ChatStreamingIndicator`, `ChatTodoList`, `ChatActivityTimeline`, `ChatPlan`, `ChatBudget` (with `LoadingDots`, `CreditsPill` and `BudgetIndicator`), `ContextCounter`, `ChatConversationListItem`, `ChatAssetRow` and `ChatMessageActions`. These compose existing components rather than reimplementing them — `ChatStreamingIndicator` drives `UnityLogo`'s existing animated variant instead of adding a second animation driver, and every spinner and ring is `CircularProgress`. React callbacks become C# events, `ReactNode` slots become container properties, and optional controls become booleans since C# has no "omit the prop to hide it". The interactive half of the suite remains deferred to [#1235](https://github.com/Unity-Technologies/unity-app-ui/issues/1235).
- Added `ChatMessages`, the conversation container, composing the chat display components already in the package. It exposes an `IChatToolRenderer` seam so the deferred tool-renderer plugin model ([#1235](https://github.com/Unity-Technologies/unity-app-ui/issues/1235)) has something to plug into, modelled on how `ColorGradientField` exposes its picker interface. Since `markdown-renderer` is deferred ([#1231](https://github.com/Unity-Technologies/unity-app-ui/issues/1231)), a text part renders as rich text rather than a Markdown tree.
- Added `ChoiceList` and `ChoiceListItem`, built on `RadioGroup`/`Radio` so the whole selection contract is inherited. The only addition is `letter`, which auto-assigns A…Z then AA, AB…; an explicit letter is pinned and does not consume a sequence slot, and an empty one hides the badge.
- Added `ClampedContent`, which clips its content to `maxHeight` and reveals an expand toggle only when it actually overflows. The fade is a flat translucent band rather than the web's gradient, since USS has no `linear-gradient`; `fadeVisible` lets a host on a non-default surface turn it off.
- Added `CodeEditor`: `CodeBlock`'s TextMate highlighting over `TextArea`'s editing, so focus, caret, selection, undo, context menu and the field chrome all come from the existing component. The registry's scroll-sync is deliberately not ported — it exists because a DOM textarea scrolls its own fixed box, whereas here both layers are content-height and there is no independent offset. Put the editor in a `ScrollView` for long documents.
- Added `ConfirmDialog`, the registry's imperative confirm helper: `ConfirmDialog.Show(...)` and `ConfirmDialog.ConfirmDelete(...)` return `Task<bool>`, matching the package's existing async style rather than importing a promise idiom. It derives from the existing `AlertDialog` and shows through the existing `Modal`, so no new popup machinery was introduced. Concurrent calls queue, and each answer resolves exactly once so a dismiss arriving after a confirm cannot flip the result.
- Added `FloatingWindow`, a draggable and resizable window with persisted layout, plus `IWindowLayoutStorage` and a `PlayerPrefs` implementation. `viewDataKey` was deliberately not used for persistence: it is keyed by serialized hierarchy position rather than a host-chosen name and cannot be injected, so it cannot honour the registry's `storageKey`/`storage` props. The web version's Tab focus trap and window stacking are not ported — the package has no shared window-stack service, so stacking follows hierarchy order.
- Added `GradientEditor` and `CurveEditor`, which implement the `ICustomColorGradientFieldPicker` and `ICustomAnimationCurveFieldPicker` interfaces an earlier slice left open — `field.customPicker = new GradientEditor()` is the whole integration, with no change to the fields. The gradient strip is a `ColorSwatch`, so `GradientMode.Fixed` and the checkerboard render identically to the field's own preview, and stops are edited with the existing `ColorField` and `ColorSlider`. `TangentMode` is a new App UI enum because `AnimationUtility.TangentMode` is editor-only and `Keyframe` stores tangent values without the intent behind them.
- Added `ColorGradientField` and `AnimationCurveField`, following the `ColorField` pattern of separating the field from its editor behind an interface (`ICustomColorGradientFieldPicker`, `ICustomAnimationCurveFieldPicker`) plus a `pickRequested` event, so both are correct displays before their editors exist. `ColorGradientField`'s preview is a `ColorSwatch`, so the gradient-over-checkerboard composite reuses the existing shader rather than adding a second render path; `AnimationCurveField` strokes with `Painter2D`, since a stroked curve needs no fill.
- Added `ImageLightbox`, built on the existing `Modal`. UI Toolkit has no viewport units and percentages cannot be used here — the lightbox's parent is the modal content box, whose size is what the lightbox itself determines, so a percentage would be circular. It measures the panel instead and sizes the image to `natural × min(1, budgetW/natW, budgetH/natH)`, which preserves aspect by construction and never upscales past natural size.
- Declared the `MVVM And Navigation` sample in `package.json`. The sample's files were already in `Samples~` but no `samples` entry pointed at them, so they shipped as unreachable dead weight and tripped PVP-84-2.
- Added `NumericProperty<TValue, TField>` with `NumericPropertyFloat` and `NumericPropertyInt`, the registry's `numeric-property-field`: a label row whose label scrubs the value. It exists because wiring it by hand does not work — `InputLabel { draggable = true }` around a `FloatField` silently does not scrub, since `NumericalField.acceptDragging` defaults to `false`. `NumericProperty` sets both halves.
- Added `ObjectPicker` and `ObjectPickerPreview`, composed from `SearchBar`, `Tabs`, `SplitView` and `ScrollView` and frameless like the registry's, so it drops into a `Popover`, `Dialog` or `FloatingWindow` rather than bringing its own modal.
- Added `OverflowToolbar`, which collapses items that no longer fit into an overflow menu on resize. The menu is the existing `Menu`/`MenuBuilder`; collapsed items are re-parented into it and returned to the bar on dismiss, so a single element tree is maintained rather than two.
- Added `OverflowTooltip`, which shows its tooltip only when the text is actually truncated. It measures with `MeasureTextSize` against the content box and re-checks on geometry and text changes, which is the UI Toolkit equivalent of the web version's ResizeObserver.
- Added `Sidebar` with `SidebarHeader`, `SidebarContent`, `SidebarFooter`, `SidebarSection`, `SidebarSectionHeading` and `SidebarItem`. The registry's controlled/uncontrolled `selectedId`/`defaultSelectedId` pair collapses to a single `selectedId` plus a `selectionChanged` event, which is the UI Toolkit idiom. `width` is a C# property rather than a token because the registry's `w-60` is 240px and the App UI sizing ramp tops out at 154px — the same reasoning `CardGrid.minItemWidth` already uses.
- Added `Skeleton`, `Kbd` and `UnityLogo`. `Skeleton` reproduces the web's `animate-pulse` (2000 ms, opacity 1.0 → 0.5) with a scheduled driver, since USS has no keyframes; it stops on detach and restores full opacity. `Kbd` renders one cap per key and accepts either `text` or a `keys` collection, which round-trips through UXML as `keys="Ctrl,K"`. `UnityLogo` renders through the existing `Icon` component using the brand asset the package already ships at `PackageResources/Logos/`, so it inherits the icon sizing ramp and density rescaling; its `Animated` variant is a linear fill ramp rather than the web's radial sweep, because UI Toolkit has no image masking.
- Added `TimeField`, `TimePicker` with `TimePickerColumn`, and `DateTimeField`. Time of day is modelled as a `System.TimeSpan` normalized into `[00:00:00, 24:00:00)`: the package had no time type, and a new `Time` struct could not carry that name without colliding with `UnityEngine.Time`, while `TimeSpan` is already what `DateTime.TimeOfDay` returns. `DateTimeField` composes the existing `DateField` behaviour with the new time field rather than adding a second parser, and culture handling copies `DateField` exactly instead of introducing a second approach.
- Added the `Timeline` compound: `Timeline`, `TimelineTrack`, `TimelineTrackGroup`, `TimelineClip`, `TimelineKeyframe`, `TimelineRuler`, `TimelineGrid`, `TimelineMarker`, `TimelineRange` and `TimelinePlayhead`. The registry's `useTimelineContext` becomes a `TimelineAxisContext` provided through the package's own context system. `PanAndZoomable` is deliberately not reused — it applies a uniform scale that would stretch lane heights and ruler text vertically, where a timeline zooms the time axis only — but its control-scheme predicates are reused verbatim, and `TimelineControlScheme` mirrors `CanvasControlScheme` member for member so one preference can drive both. Managed multi-keyframe selection and `infinite` adaptive tick thinning are not ported.
- Added `Toaster` and the `ToastManager` store, mirroring the registry's `use-toast` semantics: a limit that clamps to at least one and trims the oldest when lowered, plus add, update, dismiss and clear. The host renders the existing internal toast element, so the popup and stacked channels are styled identically and nothing was duplicated; it owns no queue of its own and unsubscribes on detach.
- `FeedbackTray`, a thumbs up / thumbs down rating row with optional regenerate and copy actions, matching the registry's `feedback-tray`.
- `MenuItem` gains `variant` (`MenuItemVariant.Default` / `Destructive`), `indicator` (`MenuItemIndicator.Checkmark` / `Radio`) and `key`, so a menu row can carry the registry's destructive tint and can report its selection as a radio bullet instead of a checkmark.
- `MenuRadioGroup`, the UI Toolkit counterpart of the registry's `DropdownMenuRadioGroup` / `ContextMenuRadioGroup`. It groups `MenuItem` rows into a mutually exclusive set keyed on `MenuItem.key`, and an already-selected row can no longer be toggled off.
- `TextField` gains `selectAllOnFocus` (default `true`) and `suffix`, a static text slot for a unit or currency. The registry's `text-field` documented `selectTextOnFocus` as mirroring UITK, but the property did not exist and `TextFieldExtensions.RuntimeContextMenu()` hard-set the underlying value to `false`, so the behaviour was the opposite of the documented default.
- `TweakMenu`, a floating gear menu that switches the host `Panel`'s theme (all six) and density (`normal` / `compact` / `comfort`) live, matching the registry's `tweak-menu`.
- Added `UserAvatar` and `UserSwitcher`. `UserAvatar` subclasses the existing `Avatar` and adds a `UserIdentity`, deriving initials and an identity ring; `UserSwitcher` builds its account rows as real `MenuItem`s inside the existing `Menu`, so the selected checkmark is the menu's own rather than a second implementation, and its `contentContainer` is the menu tail so callers can append settings or sign-out items with `Add`.
- Added `ZoomableThumbnail` and `ZoomTriggerButton`, with `triggerMode` covering the registry's corner control and whole-image variants. The reveal keys off the thumbnail's hover plus the button's own `:focus`, because `:focus-within` is not a USS selector.
- Added `Shimmer`, a `LocalizedTextElement` that brightens and dims on a two second loop to mark indeterminate progress. The registry sweeps a gradient through the glyphs with `background-clip: text`; USS has neither gradient text fills nor keyframes, so the port keeps the intent and travels the text colour between its resting value and `--shimmer-highlight-color` instead, driven from C# the way `Skeleton` drives its pulse. The resting colour is whatever the surrounding USS gives the element, so only the highlight belongs to the package.
- `Slider` gains `fieldLowValue` and `fieldHighValue`, the input-field bounds that widen past the track's, which is what the registry's new `SliderField` exposes as `fieldMin`/`fieldMax`. Widening only, so a drag can never reach a number the field would then refuse to take back; a value outside the track pins the thumb at the end while the field keeps the number, and arrow keys on the track still stop at the track's ends. `SliderField` itself needs no new type: `Slider` already pairs a track with its own numeric field on one value binding through `showInputField`.
- Added `AudioPreview`, which auditions a single clip: an envelope drawn by the existing `AudioWaveform`, a play/stop control and a length readout. Purely presentational — the caller owns the peak data, the playback state and every string, so `toggled` reports the intent and never flips `playing` by itself. The body has a precedence order: `message` replaces the envelope outright, then `loading` shows a `Skeleton`, then a null envelope with a `fallbackIcon` draws that symbol, and otherwise a null envelope still draws as a flat baseline, because a clip nobody has analysed is still playable and an empty box would read as a broken component.
- `TreeViewItem` gains `dropIndicator`, which draws the row's drop highlight from the outside for a drag the tree is not running itself. A pointer-driven drag produces none of the events the tree's own drag handling watches for, so feeding the position in keeps such a drop looking exactly like a native one instead of growing a second set of highlight styles. The tree's own drop target wins while it is targeting the row, so the two can never contradict each other.
- `NumericalField` gains `scrubSuffix`, which turns the unit glyph's drag-to-scrub off. `Slider` sets it on the field it mounts through `showInputField`, because the track is already that row's drag surface and a unit that also scrubbed would be a second gesture, at a different rate, on something that does not look draggable.
- `ChatStreamingIndicator` gains `startTime`, so the elapsed time survives the element being rebuilt: a host that refreshes its message list mid-turn no longer restarts the count from zero. `ChatMessages` threads the message's own `createdAt` in. The verb is now a `Shimmer` while the turn runs and settles once it is over; the stats deliberately do not sweep, because they are figures to read rather than a progress cue.
- `ChatAssetRow` gains `fallbackIcon`, `media` and `creatorLink`. `fallbackIcon` replaces the hardcoded cube so an item with no picture still reads as what it is; clearing it restores the cube rather than leaving the row blank. `media` puts a caller-supplied element in the thumbnail's place, for a row whose content is not an image. `creatorLink` offers the creator label as a link and raises `creatorRequested`; the registry's `creatorUrl` has to check for an absolute http(s) URL before building an `href`, and with no `href` here there is nothing to guard.
- `ChatTodoList` gains `collapsible`, a `size` of `Sm` or `Md`, and optional `showMaximize`/`showClose` header actions raising `maximizeRequested`/`closeRequested`. Turning off `collapsible` forces the body open and drops the caret, rather than stranding a list closed with no way to open it. The registry shows each action only when its handler is passed; C# cannot omit a prop, so the controls are explicit flags.
- `ChatActivityTimeline` gains `isFollowedByText` and `startTime`, and its collapsed header now reports what the run did — "3 thoughts, 2 tools" — instead of repeating the label. While the activity runs the header shows the label, sweeps as a `Shimmer` and counts live from `startTime`, so a list rebuilt mid-turn does not restart the count. `isFollowedByText` finalizes the run as soon as the answer's own prose begins, rather than leaving it counting while the reader is already reading. `ChatMessages` threads both in.
- `ChatMessages` lets a host pull chosen message parts out of the bubble and render them itself, through `standalonePart` and `renderStandalonePart` plus a `ChatStandalonePartContext`. A picked part is excluded from the activity group whether or not the host draws it, so the same tool call can never show twice, and it closes the activity run it followed so the block lands in reading order. Tool results are exempt: they are folded onto their call, and pulling one out alone would orphan it.
- `Timeline` gains `minPxPerUnit`, `maxPxPerUnit`, `fitWidth`, `fitDuration` and `adaptiveTicks`; `TimelineClip` gains `frame`, `frameColor`, `frameHeight` and `doubleClicked`; `TimelineKeyframe` gains `muted`. The zoom bounds were compile-time constants, so a host with a different time unit could not widen them; they keep their old values as defaults. `fitWidth` derives the base scale from the measured viewport, so the content fills the strip at any panel width. `adaptiveTicks` chooses the tick interval from the current pixel density on the 1/2/5 ladder, staying on whole multiples of the configured interval so an adaptive ruler still lands on the caller's beats; `infinite` implies it, since an axis with no end cannot have a fixed tick count. A `frame` clip is a translucent placement marker whose body lets pointers through to the lane behind it and which drags only by its handle strip. `Timeline` also gains the rest of the registry's [#1415](https://github.com/Unity-Technologies/unity-app-ui/issues/1415) set: `labelResizable` and `minLabelWidth` with a `labelWidthChanged` event (the gutter now always leaves 120px for the lanes), `hoverLine`, `timeLabel` and `timeSuffix` for a corner time, `laneExtent`, `keySize`, `minTickStep`, `ticksAt`, an overlay `scrollbar` and `RevealTime`; `TimelineTrack` gains `lane`, `laneStart` and `laneEnd`; `TimelineClip` gains `variant` (`Bar`, `Handle`, `Surface`); `TimelineKeyframe` gains `size`; and `TimelineHandle` is new. A keyboard-moved playhead pans into view, the pan stays inside a shrinking axis, and past 10,000 fixed ticks the step coarsens instead of the ruler going blank.
- Added `IconButton.selected`, the pressed or toggle state of an icon button such as an active panel toggle, as a UXML attribute and a bindable property. A selected IconButton paints `--appui-alias-actions-selected-background-color-default`, and `--appui-alias-actions-selected-background-color-hover` under the pointer, as the registry's icon-button does. A disabled selected IconButton drops the selected fill.
- `InputLabel` grid rows take `columns = InputLabelColumns.Pair`, the registry's `PropertyField columns="pair"`: the control cell splits into two equal halves with the inspector grid gap between them, and a lone control keeps to the first half so it still lines up with the pair rows around it. `columns` is an `InputLabelColumns` enum (`None`, `Even`, `Pair`) rather than a bool, since a bool cannot hold three states. Controls in a grid `InputLabel` row and in a `PropertyGroup` body take their medium height from `--appui-alias-inspector-row-height`, which resolves to the same value by default.
- `InputLabel.labelFor` names the control the row labels, so a click on the label operates it, like the registry's `PropertyField labelFor` and an HTML `<label for>`. The control is looked up in the row, then in the panel, and must be `IPressable`, such as a `Checkbox`, `Toggle` or `Radio`. A disabled control is left alone, and the click is ignored while `draggable` is on, so a scrub ending on the label never toggles anything.
- Added `DropdownItem.description`, a secondary line under an option's label, matching the registry `select` option's `description`. Set it from `bindItem`; it reads `--appui-alias-text-secondary` at `--appui-font-sizes-100`, hides while empty, stays out of the trigger, and the menu grows past the field so it is not clipped.
- `MaskField` gains the registry mask field's sentinels: `everythingValue` (0 keeps the OR of the items; set -1 for a mask whose every bit means everything), `summary` (`MaskFieldSummary.Mixed` by default, or `List` to name the set items), and `nothingLabel`, `everythingLabel` and `mixedLabel` for the summary and sentinel rows. A set bit no item names now stays in `value` and reads as mixed instead of being dropped.
- An enabled control's tooltip now opens when it takes keyboard focus and closes on blur, matching the registry's tooltip trigger. Any key, Escape included, still dismisses it, focus from a pointer click does not open it, and hover behaviour is unchanged. Disabled controls keep their hover tooltip and get no focus tooltip, since UI Toolkit cannot focus a disabled element and no soft-disabled state was added.
- `ImageLightbox` can step through a set of images, like the registry's `navigation`: set `navigationCount` above 1 and `navigationIndex`, and previous and next buttons (`caret-left` / `caret-right`, named by `previousLabel` and `nextLabel`) sit either side of the caption, Left / Right arrow keys step too, and the caption leads with the position ("2 / 5"). Stepping never wraps; each step raises `navigationIndexChanged` so the caller swaps `src` and `caption`. A step that disables the focused button moves focus to the other one. An image more than `tallRatio` (1.25) times taller than wide is now shown at full width and scrolls instead of being shrunk.
- `AudioPreview` gains `previewLayout` (UXML `layout`), like the registry's `layout`: `AudioPreviewLayout.Overlay` (the default, unchanged) or `Strip`, one compact row with the play/stop control first, a short envelope, then the length, styled through the `appui-audio-preview--strip` class. A strip draws the flat baseline for a null envelope even when `fallbackIcon` is set.
- Added `SplitView.indicator`, which picks how each splitter shows: the full-length line (`Stretch`, the default) or a centered grip spanning a quarter of the splitter that appears on hover and drag (`Centered`), matching the registry's `ResizableHandle`. Both shapes are always in the tree, so a theme can pick instead by setting `--appui-resizable-handle-stretch-display` and `--appui-resizable-handle-centered-display` to `flex` or `none`.
- `CanvasNode.lod` and `CanvasEdge.lod` take a `CanvasLevelOfDetail`, like the registry's `lod`: `Compact` hides a node's port labels and body, `Box` also hides its title and port targets and leaves an edge its line only, with no hit area or arrow. Port rows keep their geometry at every level. `CanvasCulling.LevelOfDetailForZoom` picks the level from the zoom (compact under 0.5, box under 0.3), and `Canvas.TryGetVisibleContentRect` with `CanvasCulling.IsVisible` culls nodes and edges outside the viewport plus a 200 px margin, always keeping selected ones.
- `ChatMessages` gains `renderActivity`, a `Func<ChatActivityRenderContext, VisualElement>` that replaces the activity timeline for every activity group (with the group's parts, index, live state, whether text follows, and start time), and `showStreamingIndicator`, which hides the live indicator under a streaming message when the host reports progress elsewhere. Both match the registry's `chat-messages`.
- `ChatActivityTimeline` gains the [#1460](https://github.com/Unity-Technologies/unity-app-ui/issues/1460) options: a running header names the newest tool call in flight (`ChatActivityPart.toolName`, `activeToolNames`), `stats`, `headerPlacement` (a status chip at the foot whose rows open upward), `foldRowsByKind` with a pager through each folded row, and `pinEntries` / `maxPinnedShare`, which lay rows out as one-line entries and mark the ones that pin within a six-entry cap and a share of the enclosing `ScrollView`. USS has no `position: sticky`, so the marked entries still scroll. Pressing the header now sends a `ChangeEvent<bool>`. `ChatMessages` forwards them as `activityHeaderPlacement`, `activityFoldRowsByKind`, `activityPinEntries`, `activityMaxPinnedShare` and `activityOpen` plus `activityOpenChanged`. A live group with its header at the foot carries `streamingStats` and replaces the streaming indicator.
- Added `TreeListVirtual` and `TreeListFlatten.Flatten`, the registry's `TreeViewVirtual` and `flattenTree`. `Flatten` lays a lazily loaded tree out as flat `TreeListRow`s with level, parent, set size and position, plus placeholders for unloaded or partly loaded levels, and emits each id once. `TreeListVirtual` draws those rows by recycling a small pool of `TreeViewItem`s, so only the rows near the view exist, and selects by row id with the same click, Shift-range, action-toggle, release-to-select and keyboard rules as `TreeList`. Expansion goes through `onExpandedChange`. The lead selected row stays realised when scrolled away. `TreeList` itself is unchanged. Drag to reparent is not supported yet in the virtual view.
- `CodeBlock` and `CodeEditor` highlight `hlsl` and `wgsl`, the two shader languages the registry's highlighter loads. The grammars come from Visual Studio Code (HLSL) and vscode-wgsl (WGSL), both MIT.
- `PropertyGroup.expandable` keeps the header toggle and the closed-state ellipsis for a group whose rows another element lays out, and drops the row gap below its header. `TreeList.keyboardNavigation` turns a tree into a selection provider for rows a host lays out and keys itself (the registry's `TreeViewProvider`), where each caret row opens and closes itself on Right and Left. Right on a `showCaret` row with no nested children now expands it, and steps only into a nested child.
- `IconContactSheetTests` captures the whole icon library as grid "contact sheets" — around 300 icons per sheet, ten sheets per theme — so the move to SVG-backed `VectorImage` icons has visual regression coverage without one capture per icon. Unity tessellates icons at runtime and can silently drop a subpath, invert a fill or blank an icon outright on complex geometry, none of which CI could previously see. The layout is read from `icon-sheets.json` rather than computed, because a second renderer outside the package rasterizes the same grid from the source SVGs as an oracle, and the two have to place the same icon in the same cell. Sheets render white-on-black with the icon tint pinned in both themes, so the only thing varying between them is which artwork an icon resolves to; the dark theme exercises the `d_*` artwork. Captures go to `SNAPSHOTS_OUTPUT_DIR` alongside the component snapshots. The fixture loads the full `Icons.uss` catalogue from a test-only `Resources` stylesheet, since `Icon.uss` imports only the minimal subset and `Icon` resolves artwork purely through USS.

### Removed

- `ChatActivityTimeline` no longer appends a synthetic "Done" row when the turn ends, and `doneLabel` is gone with it. The collapsed summary already says the activity is finished, so the extra row was a second and weaker way of saying the same thing.
- TweakMenu section from the Kit sample, and icons missing from the minimal set in the Kit sample.

### Security

- The markdown renderers no longer emit inline HTML tags verbatim, and only link destinations using `http`, `https`, `mailto` or a site-relative path are made clickable. Inline tags now render as literal text, and destinations such as `file:`, `data:`, `javascript:` or a custom app scheme render as plain, non-clickable text.
- `DefaultDestinationTemplate.CreateScreen` now checks that the serialized template type is a `NavigationScreen` before instantiating it. Previously any type named in the navigation graph had its constructor run, and the resulting object was only rejected afterwards.
- The AI Agent Skills panel in the Package Manager is now offered only for an App UI copy resolved from the default Unity registry. `PackageSource.Registry` alone is not enough, since a project can serve any package name from a scoped registry it controls. The package name alone is attacker-controllable, so a repackaged or hand-added copy could previously present third-party agent instructions through first-party UI. The install dialog also shows the resolved source and destination paths.
- Debug symbols are no longer published with the package. The Windows native plugin `.pdb` files added roughly 42 MB to every release and embedded the absolute build paths and account name of the machine that produced them. The Windows plugin is also linked with `/PDBALTPATH:%_PDB%` so the DLL itself records only the symbol file name instead of that full path.
- `ThreadMessage` no longer interprets untrusted message text as rich text when no mention provider is set. The previous `<noparse>` wrapper could be closed early by a message containing its own `</noparse>` (in any casing, or with inner whitespace), which re-enabled `<b>`, `<color>`, `<size>` and sprite tags for the rest of the message and allowed spoofing an official-looking notice.
- The UXML clone-tree source generator now emits attribute values through `SymbolDisplay.FormatLiteral` instead of hand-quoting them, so a quote, backslash, newline or Unicode line separator in a `[UxmlFilePath]` or element name can no longer terminate the generated string literal early.
- `VisualDocPageBuilder.BuildGuidePage` now resolves the requested file and confirms it is still inside the package documentation root, so a rooted or `..`-bearing file name can no longer read a file outside it.
- The WebGL clipboard read callbacks now bound the JavaScript-supplied buffer length before allocating and use a checked conversion, so an out-of-range length fails loudly instead of wrapping.

## [2.2.6] - 2026-11-05

### Fixed

- `SwitchBuilder` runs every matching reducer case on the state returned by the previous one, so a slice no longer keeps only the last case's result. Several `AddDefault` calls now chain the same way.
- `Action<TPayload>` instances with different payloads, and async thunk actions with different `meta` or creators, no longer compare as equal through `IEquatable<Action>`.
- `StoreFactory.ComposeEnhancers` returns the identity enhancer when it is called with no enhancers instead of throwing.
- The Redux DevTools no longer keep committed actions in `actionsById` when `maxAge` is set, and `Dispatch` accepts a `maxAge` of 0 or 1.
- Async thunks run when DevTools is enabled through `StoreFactory.DefaultEnhancer`.
- `IThunkAPI.Abort` cancels the async thunk instead of throwing when it is called on the main thread or from another thread, and so do `RejectWithValue` and `FulFillWithValue`.
- `TryAddTransient` registers a transient service, and `TryAddTransient(Type)` and `TryAddScoped(Type)` add it only once.
- `NavController.Navigate(Uri)` resolves deep links with two or more path segments.
- `GradientExtensions.TryParse` returns false for a gradient key with a single value instead of throwing, and accepts keys with surrounding spaces.
- `PinchGesture.GetHashCode` is equal for gestures that compare equal.

## [2.1.16] - 2026-11-05

### Fixed

- `SwitchBuilder` runs every matching reducer case on the state returned by the previous one, so a slice no longer keeps only the last case's result. Several `AddDefault` calls now chain the same way.
- `Action<TPayload>` instances with different payloads, and async thunk actions with different `meta` or creators, no longer compare as equal through `IEquatable<Action>`.
- `StoreFactory.ComposeEnhancers` returns the identity enhancer when it is called with no enhancers instead of throwing.
- The Redux DevTools no longer keep committed actions in `actionsById` when `maxAge` is set, and `Dispatch` accepts a `maxAge` of 0 or 1.
- Async thunks run when DevTools is enabled through `StoreFactory.DefaultEnhancer`.
- `IThunkAPI.Abort` cancels the async thunk instead of throwing when it is called on the main thread or from another thread, and so do `RejectWithValue` and `FulFillWithValue`.
- `TryAddTransient` registers a transient service, and `TryAddTransient(Type)` and `TryAddScoped(Type)` add it only once.
- `NavController.Navigate(Uri)` resolves deep links with two or more path segments.
- `GradientExtensions.TryParse` returns false for a gradient key with a single value instead of throwing, and accepts keys with surrounding spaces.
- `PinchGesture.GetHashCode` is equal for gestures that compare equal.

## [2.3.1] - 2026-11-03

### Fixed

- `SwitchBuilder` runs every matching reducer case on the state returned by the previous one, so a slice no longer keeps only the last case's result. Several `AddDefault` calls now chain the same way.
- `Action<TPayload>` instances with different payloads, and async thunk actions with different `meta` or creators, no longer compare as equal through `IEquatable<Action>`.
- `StoreFactory.ComposeEnhancers` returns the identity enhancer when it is called with no enhancers instead of throwing.
- The Redux DevTools no longer keep committed actions in `actionsById` when `maxAge` is set, and `Dispatch` accepts a `maxAge` of 0 or 1.
- Async thunks run when DevTools is enabled through `StoreFactory.DefaultEnhancer`.
- `IThunkAPI.Abort` cancels the async thunk instead of throwing when it is called on the main thread or from another thread, and so do `RejectWithValue` and `FulFillWithValue`.
- `TryAddTransient` registers a transient service, and `TryAddTransient(Type)` and `TryAddScoped(Type)` add it only once.
- `NavController.Navigate(Uri)` resolves deep links with two or more path segments.
- `GradientExtensions.TryParse` returns false for a gradient key with a single value instead of throwing, and accepts keys with surrounding spaces.
- `PinchGesture.GetHashCode` is equal for gestures that compare equal.

## [2.3.0] - 2026-10-30

### Added

- Factory registration for the dependency injection container: `ServiceDescriptor.Singleton`, `Scoped` and `Transient` accept a `Func<IServiceProvider, object>` (or a generic `Func<IServiceProvider, TService>`), and `AddSingleton`, `AddScoped` and `AddTransient` have matching overloads. A factory can forward to another registration, so one singleton can be exposed under two interfaces. `ServiceDescriptor.Singleton<TService>(TService instance)`, a public `ServiceDescriptor(Type, Func<IServiceProvider, object>, ServiceLifetime)` constructor and the `ServiceDescriptor.implementationFactory` property were added too.

## [2.1.15] - 2026-10-29

### Fixed

- `Store.Dispatch` no longer throws `InvalidOperationException` when a listener subscribes or unsubscribes while it is being notified. Subscribers are now notified from a snapshot, and a subscription that is unsubscribed while it is being notified no longer throws `NullReferenceException`. Its listener may still be called once (UUM-157944).
- `IDisposableSubscription.IsValid()` returns `false` instead of throwing a `NullReferenceException` once the subscription or its store is disposed (UUM-157918).
- The `Optional<float>`, `Optional<double>` and `Optional<Rect>` UXML converters, `RectExtensions.TryParse`, `GradientExtensions.TryParse` and the float sliders now read and write numbers with the invariant culture, so `0.5` is no longer read as `5` under a decimal comma culture. Values saved with the current culture by earlier versions, such as `0,5` under `de-DE`, still load, and the sliders' input field still accepts them. The `Optional<int>` and `Optional<long>` converters read the same way (UUM-157942).
- `DateConverter` and `DateRange.TryParse` now parse serialized dates with the invariant culture, so dates no longer swap day and month or shift calendars under other cultures (UUM-157919).
- `DatePicker` rebuilds its day grid for months before 1970, and keeps it valid at the first and last supported month, where the outward month, year and decade arrows are now disabled instead of throwing (UUM-157611).
- `Slider` arrow keys with `restrictedValues` set to `Step` or `CtrlStep` now move by the right step with Shift, step without Ctrl for `CtrlStep`, and no longer throw with Ctrl (UUM-157601).
- `ActionGroup` children and its overflow button now follow an inherited `visibility: hidden` instead of forcing themselves visible (UUM-145391).
- `Radio` registers with its `RadioGroup` again when it is re-attached to a panel, so radio buttons keep working and keep their selection in content that is shown more than once, such as a `Popover` or `DialogTrigger` (UUM-144282).
- Setting `UndoStack.index` to `UndoStack.count`, or to `0` on an empty stack, now throws `ArgumentOutOfRangeException` instead of hanging (UUM-158333).
- `UndoStack.isClean` no longer reports `true` after the clean state was reset with `ResetClean`, dropped by a `Push` or evicted by `undoLimit`, including when the clean state was the initial one. A `Push` that drops the clean command now raises `cleanStateChanged`, and `ResetClean` raises it only when `isClean` or `cleanIndex` changes (UUM-158398).
- `AsyncRelayCommand<T>` with `AllowConcurrentExecutions` now cancels the previous run when `ExecuteAsync` is called again, so `Cancel()` reaches every run (UUM-158357).
- `AsyncRelayCommand` and `AsyncRelayCommand<T>` now raise `CanExecuteChanged` and their completion `PropertyChanged` notifications when a run is cancelled, instead of throwing `TaskCanceledException` and leaving bound buttons disabled. A faulted run also raises them now, and its exception is still logged, including an `OperationCanceledException` that did not come from cancelling the run (UUM-158369).
- `App.current` is no longer cleared when a different `App` instance is disposed or finalized (UUM-158373).
- `Handler.RemoveCallbacksAndMessages` no longer removes messages sent through other handlers that share the same `Looper`. Messages whose target is not the calling handler, such as one from `Message.Obtain(null, ...)`, are no longer removed by it either (UUM-158403).
- `Message.Obtain` keeps reusing pooled messages after more than 50 messages have been recycled (UUM-158417).
- `NavDestination.MergeArguments` now replaces a default argument with a provided argument of the same name instead of keeping both (UUM-158420).
- `NavGraphViewAsset.FindDestinationByRoute` no longer throws `InvalidCastException` when the route names a navigation action. It returns `null`, or the destination of that name if there is one (UUM-158351).
- `ColorField` no longer throws `ArgumentException` when `hdr` is `true` and a channel is above 1. `ColorExtensions.ColorToRgbaHex` now clamps each channel (UUM-158350).
- `Rect.IsValid` and `VisualElement.IsOnScreen` no longer treat a zero-area rect as valid on engine builds where `Mathf.Epsilon` evaluates to 0.

### Changed

- `AppUIServiceCollection` and its base `ServiceCollection` now throw `InvalidOperationException` on `Add`, `Insert`, `Remove`, `RemoveAt`, `Clear` and indexer changes once `IsReadOnly` is `true`, and `ICollection<ServiceDescriptor>.IsReadOnly` reports `true` then. Services added after `AppBuilder.BuildWith`, for example in `OnAppInitialized`, now throw (UUM-158414).

## [2.2.5] - 2026-10-28

### Fixed

- `Rect.IsValid` and `VisualElement.IsOnScreen` no longer treat a zero-area rect as valid on engine builds where `Mathf.Epsilon` evaluates to 0.
- `Store.Dispatch` no longer throws `InvalidOperationException` when a listener subscribes or unsubscribes while it is being notified. Subscribers are now notified from a snapshot, and a subscription that is unsubscribed while it is being notified no longer throws `NullReferenceException`. Its listener may still be called once (UUM-157944).
- `IDisposableSubscription.IsValid()` returns `false` instead of throwing a `NullReferenceException` once the subscription or its store is disposed (UUM-157918).
- The `Optional<float>`, `Optional<double>` and `Optional<Rect>` UXML converters, `RectExtensions.TryParse`, `GradientExtensions.TryParse` and the float sliders now read and write numbers with the invariant culture, so `0.5` is no longer read as `5` under a decimal comma culture. Values saved with the current culture by earlier versions, such as `0,5` under `de-DE`, still load, and the sliders' input field still accepts them. The `Optional<int>` and `Optional<long>` converters read the same way (UUM-157942).
- `DateConverter` and `DateRange.TryParse` now parse serialized dates with the invariant culture, so dates no longer swap day and month or shift calendars under other cultures (UUM-157919).
- `DatePicker` rebuilds its day grid for months before 1970, and keeps it valid at the first and last supported month, where the outward month, year and decade arrows are now disabled instead of throwing (UUM-157611).
- `Slider` arrow keys with `restrictedValues` set to `Step` or `CtrlStep` now move by the right step with Shift, step without Ctrl for `CtrlStep`, and no longer throw with Ctrl (UUM-157601).
- Dragging with touch and pen pointers works again on `Draggable`-based controls such as sliders, `SplitView` and `Tray` (UUM-154905).
- `ActionGroup` children and its overflow button now follow an inherited `visibility: hidden` instead of forcing themselves visible (UUM-145391).
- `Radio` registers with its `RadioGroup` again when it is re-attached to a panel, so radio buttons keep working and keep their selection in content that is shown more than once, such as a `Popover` or `DialogTrigger` (UUM-144282).
- Setting `UndoStack.index` to `UndoStack.count`, or to `0` on an empty stack, now throws `ArgumentOutOfRangeException` instead of hanging (UUM-158333).
- `UndoStack.isClean` no longer reports `true` after the clean state was reset with `ResetClean`, dropped by a `Push` or evicted by `undoLimit`, including when the clean state was the initial one. A `Push` that drops the clean command now raises `cleanStateChanged`, and `ResetClean` raises it only when `isClean` or `cleanIndex` changes (UUM-158398).
- `AsyncRelayCommand<T>` with `AllowConcurrentExecutions` now cancels the previous run when `ExecuteAsync` is called again, so `Cancel()` reaches every run (UUM-158357).
- `AsyncRelayCommand` and `AsyncRelayCommand<T>` now raise `CanExecuteChanged` and their completion `PropertyChanged` notifications when a run is cancelled, instead of throwing `TaskCanceledException` and leaving bound buttons disabled. A faulted run also raises them now, and its exception is still logged, including an `OperationCanceledException` that did not come from cancelling the run (UUM-158369).
- `App.current` is no longer cleared when a different `App` instance is disposed or finalized (UUM-158373).
- `Handler.RemoveCallbacksAndMessages` no longer removes messages sent through other handlers that share the same `Looper`. Messages whose target is not the calling handler, such as one from `Message.Obtain(null, ...)`, are no longer removed by it either (UUM-158403).
- `Message.Obtain` keeps reusing pooled messages after more than 50 messages have been recycled (UUM-158417).
- `NavDestination.MergeArguments` now replaces a default argument with a provided argument of the same name instead of keeping both (UUM-158420).
- `NavGraphViewAsset.FindDestinationByRoute` no longer throws `InvalidCastException` when the route names a navigation action. It returns `null`, or the destination of that name if there is one (UUM-158351).
- `ColorField` no longer throws `ArgumentException` when `hdr` is `true` and a channel is above 1. `ColorExtensions.ColorToRgbaHex` now clamps each channel (UUM-158350).

### Changed

- `AppUIServiceCollection` and its base `ServiceCollection` now throw `InvalidOperationException` on `Add`, `Insert`, `Remove`, `RemoveAt`, `Clear` and indexer changes once `IsReadOnly` is `true`, and `ICollection<ServiceDescriptor>.IsReadOnly` reports `true` then. Services added after `AppBuilder.BuildWith`, for example in `OnAppInitialized`, now throw (UUM-158414).

## [2.1.14] - 2026-09-27

### Fixed

- Fixed Tabs throwing "Cannot scroll to a VisualElement that's not a child of the ScrollView content-container" when TabItems are deleted from it in UI Builder. Tabs now picks up TabItems added to or removed from its item container directly, and keeps the same tab selected when it can (UUM-154226).

## [2.2.4] - 2026-09-26

### Fixed

- Fixed Tabs throwing "Cannot scroll to a VisualElement that's not a child of the ScrollView content-container" when TabItems are deleted from it in UI Builder. Tabs now picks up TabItems added to or removed from its item container directly, and keeps the same tab selected when it can (UUM-154225).

## [2.2.3] - 2026-09-19

### Fixed

- The native platform P/Invoke declarations that return a `bool` now pin one-byte marshalling with `[return: MarshalAs(UnmanagedType.I1)]`. Without it a `[DllImport]` returning `bool` marshals as the 4-byte `UnmanagedType.Bool`, while every `NativeAppUI_*` entry point returns a 1-byte C++ `bool`; on x86-64 `setcc %al` writes only the low byte, so register residue in bits 8-31 could read back as `true` when the native answer was `false`.
- App UI shaders no longer fail to compile on Unity 7000, where the `UnityCG.hlsl` / `UnityUI.hlsl` includes they expected do not exist; they now include `UnityCG.cginc` on every Unity version.

## [2.1.13] - 2026-09-16

### Fixed

- The native platform P/Invoke declarations that return a `bool` now pin one-byte marshalling with `[return: MarshalAs(UnmanagedType.I1)]`. Without it a `[DllImport]` returning `bool` marshals as the 4-byte `UnmanagedType.Bool`, while every `NativeAppUI_*` entry point returns a 1-byte C++ `bool`; on x86-64 `setcc %al` writes only the low byte, so register residue in bits 8-31 could read back as `true` when the native answer was `false`.
- App UI shaders no longer fail to compile on Unity 7000, where the `UnityCG.hlsl` / `UnityUI.hlsl` includes they expected do not exist; they now include `UnityCG.cginc` on every Unity version.

## [2.2.2] - 2026-08-24

### Fixed

- Fixed `Unity.AppUI.Markdown` failing when `APPUI_ENABLE_MARKDOWN` is defined. The bundled `Markdig.dll` was the `netstandard2.0` build, which references the `System.Runtime.CompilerServices.Unsafe`, `System.Memory` and `System.Buffers` facades. Unity ships shims for the latter two but not for `Unsafe`, so parsing markdown threw `FileNotFoundException` at runtime. Replaced it with Markdig's `netstandard2.1` build, which targets the same profile Unity uses and references nothing but `netstandard` itself.
- Fixed ArgumentOutOfRangeException in Tabs when all items are disabled, and cancelled pending indicator refreshes when the selection is cleared.
- Fixed Tabs arrow-key navigation throwing ArgumentOutOfRangeException and not skipping disabled tabs (it scanned the empty content container instead of the tab items).
- Fixed `TextField.isPassword = true` throwing a `NullReferenceException` on Unity versions before 6000.0. Enabling UITK's `isPasswordField` forces multiline off, and that setter calls `text.Replace("\n", "")` with no null check on those versions. `TextField` seeded the underlying UITK field with a null value, so `new TextField { isPassword = true }` — and any `isPassword` toggle after the value was cleared — threw. The underlying field is now never given a null text, which also means `TextField.value` returns an empty string rather than null for an empty field.

### Changed

- The `[UxmlFilePath]` source generator now emits `UxmlCloneTree(VisualElement parent = null)` instead of a parameterless method. Passing a container clones the UXML tree (and resolves `[UxmlElementName]` bindings) into that container rather than onto the element itself, letting `NavigationScreen` subclasses and other custom elements direct authored content into a chosen part of their hierarchy. Existing parameterless `UxmlCloneTree()` calls keep working unchanged.

## [3.0.0-pre.1] - 2026-08-21

### Changed

- The minimum supported Unity version is now **6000.3 LTS** (previously 2021.3). The package manifest declares `"unity": "6000.3"`, and the CI editor matrix (validation, API validation, preview APV, package/project tests and player tests) now covers 6000.3, 6000.5, 6000.6 and trunk only. This is a major bump because the new floor drops Unity 6000.0 LTS, which is still under support — raising the minimum editor is only a non-breaking change when every dropped release line is already past end-of-support.
- The `[UxmlFilePath]` source generator now emits `UxmlCloneTree(VisualElement parent = null)` instead of a parameterless method. Passing a container clones the UXML tree (and resolves `[UxmlElementName]` bindings) into that container rather than onto the element itself, letting `NavigationScreen` subclasses and other custom elements direct authored content into a chosen part of their hierarchy. Existing parameterless `UxmlCloneTree()` calls keep working unchanged.

### Removed

- The APIs that were marked `[Obsolete]` are now gone, as part of the 3.0 major release. `Badge.content`, `Badge.max` and `Badge.showZero` have been removed — a Badge can hold any content, so use `Badge.label` for textual content or add child elements. The `TextFieldExtensions.BlinkingCursor()` extension method has been removed in favour of the `Unity.AppUI.UI.BlinkingCursor` manipulator (`textField.AddManipulator(new BlinkingCursor())`). `AppUISettings.autoOverrideAndroidManifest` has been removed; it had no effect, since App UI applies the changes it needs to the existing Android manifest during the build instead of shipping its own.
- Conditional compilation for editor versions below the new 6000.3 minimum. The `versionDefines` that can no longer change value at 6000.3+ were dropped from the `.asmdef` files and the `#if` branches they guarded were resolved in place: `ENABLE_UXML_TRAITS` and `ENABLE_ENABLED_UXML_PROPERTY` (never defined at 6000.3+) were deleted along with the code they guarded, while `ENABLE_UXML_SERIALIZED_DATA`, `ENABLE_RUNTIME_DATA_BINDINGS`, `ENABLE_VALUEFIELD_INTERFACE`, `ENABLE_UITK_TEXT_SELECTION`, `UITK_NESTED_INTERACTION_KIND`, `UITK_SELECTED_INDICES_CHANGED`, `UITK_MAKE_NONE_ELEMENT`, `CONDITIONAL_WEAK_TABLE_IL2CPP`, `UI_DOCUMENT_ROOT_ELEMENT_TYPE_EXISTS`, `UNITY_ENABLE_TABVIEW` and `UNITY_ENABLE_SLIDER_FILL` (always defined at 6000.3+) are now unconditional. Legacy `#if !UNITY_2022_*` and `#if !UNITY_2023_*` fallbacks were removed too. Because every one of these symbols already had a fixed value on 6000.3, the compiled API surface is unchanged for supported editors.

### Added

- Added App UI Visual Documentation window. This window builds its pages dynamically from the XML documentation of the components (extracted at compile time by a Roslyn source generator) and from the package's markdown documentation, instead of loading pre-generated UXML pages. The Visual Documentation window requires the `APPUI_ENABLE_MARKDOWN` scripting define.
- Add display scale factor and DPI support on Linux, exposing `scaleFactor`, `textScaleFactor` and `referenceDpi` on the Linux platform via new native plugin display-info APIs.
- Added a Linux native plugin, bringing the TextMate syntax highlighter and DPI scale factor support to the Linux Editor and player.

### Fixed

- Fixed `Unity.AppUI.Markdown` failing when `APPUI_ENABLE_MARKDOWN` is defined. The bundled `Markdig.dll` was the `netstandard2.0` build, which references the `System.Runtime.CompilerServices.Unsafe`, `System.Memory` and `System.Buffers` facades. Unity ships shims for the latter two but not for `Unsafe`, so parsing markdown threw `FileNotFoundException` at runtime. Replaced it with Markdig's `netstandard2.1` build, which targets the same profile Unity uses and references nothing but `netstandard` itself.
- Fixed ArgumentOutOfRangeException in Tabs when all items are disabled, and cancelled pending indicator refreshes when the selection is cleared.
- Fixed Tabs arrow-key navigation throwing ArgumentOutOfRangeException and not skipping disabled tabs (it scanned the empty content container instead of the tab items).
- Fixed a crash (SIGABRT) on Linux when the TextMate syntax highlighter handled invalid input (e.g. `Registry.AddGrammarFromJson` with malformed JSON). The Linux native plugin was built with a statically-linked libgcc unwinder that collided with the one loaded by the Unity player, so C++ exceptions aborted during unwinding instead of being caught. It is now linked dynamically and built on an old-glibc base (Ubuntu 18.04) for portability.
- Fixed `TextField.isPassword` throwing a `NullReferenceException` when set before the field is attached to a panel, which made `new TextField { isPassword = true }` unusable. Enabling UITK's `isPasswordField` forces multiline off and walks text backing that only exists once the field belongs to a panel, so the value is now stored and applied on attach.

## [2.2.1] - 2026-07-19

### Fixed

- Fixed a crash (SIGABRT) on Linux when the TextMate syntax highlighter handled invalid input (e.g. `Registry.AddGrammarFromJson` with malformed JSON). The Linux native plugin was built with a statically-linked libgcc unwinder that collided with the one loaded by the Unity player, so C++ exceptions aborted during unwinding instead of being caught. It is now linked dynamically and built on an old-glibc base (Ubuntu 18.04) for portability.

## [2.1.12] - 2026-07-16

### Fixed

- Fix headless Windows IL2CPP build crash caused by the native plugin activating WinRT settings objects at static scope on Windows Server Core (game-ci/unity-builder#702)

## [2.2.0] - 2026-07-15

### Added

- A reference of the latest opened StyleSheet in IconBrowser tool is now stored per project. The next time you will open the tool, it will reload the latest StyleSheet.
- Added `sizeModifier` callback property to `ResizeHandle` to allow custom size adjustments during resize.
- Added Thread UI components including Thread, ThreadMessage, ThreadComposer, ThreadReaction, ThreadReactionBar, ThreadResolveMessage, ThreadUnresolveMessage, MentionPicker, and ReactionPicker, along with their associated interfaces (IEmojiProvider, IMentionProvider, IThreadAttachment), base classes, enums, and USS styles. Renamed ThreadMessage `content` property to `message`, removed `isEditing` property in favor of `state = Draft`, moved `emojiProvider` and `mentionProvider` from ThreadMessage to ThreadContext, removed `Active` message state, and exposed explicit `attachments` container on ThreadMessage and ThreadComposer
- Added new icons (arrow-up, paperclip, pen, paper-plane-tilt, smiley, thumbs-up, thumbs-down) to the minimal icon set and IconBrowser required icons list
- Added TextMate grammar-based syntax highlighting support via native TextMateLib integration, including C# bindings (Registry, Grammar, Theme, Token), ScriptableObject assets (TextMateGrammarAsset, TextMateThemeAsset), ScriptedImporters for .tmLanguage and .tmTheme files, and a high-level TextMateSyntaxHighlighter API
- Added support of DevicePixelRatio in WebGL native plugin to determine the current ScaleFactor applied to the canvas.
- Added `UxmlTraits` support for `ResizeHandle` component.
- Added Edit and Delete text action constants with localization support in TextActions
- Added Uxml cloneTree and element binding by name source generators. See the documentation for more information.
- Added Conditional Resolution in Dependency Injection system.
- Add `destroyItem` callback to BaseGridView and fix doc comments
- Added HelpBox UI element
- Added runtime data binding support to `BaseGridView` component.
- Added the support of custom ColorPicker inside the ColorField element via customPicker property.
- Added `disableAnimation` property on Popup elements in order to force disable display animation for a given Popup UI Element.
- New `CodeBlock` UITK component (`Unity.AppUI.UI.CodeBlock`): rounded card with a header row (language label + hover-revealed copy button using `Platform.SetPasteboardData`) above a monospaced body. Syntax highlighting is configured declaratively from USS via two `CustomStyleProperty<string>` values (`--codeblock-grammar`, `--codeblock-theme`) holding TextMate asset GUIDs; setting the `language` property toggles a `appui-code-block--lang-<value>` modifier class so per-language grammar mappings can be expressed in the design system. When a `TextMateThemeAsset` resolves, the body's background and foreground colors are pulled from `Theme.GetDefaultBackground()`/`GetDefaultForeground()` so the card adopts the theme's palette.
- `IServiceCollection.AddSingleton<T>(T instance)` and `AddSingleton(Type, object)` overloads (plus matching `AddSingletonWhen(Type, object, ContextMatch)`) for registering pre-existing instances as singletons in the MVVM DI container. The supplied instance is returned as-is on every resolution; the container does not invoke a constructor or apply `[Service]` field/property injection on it.
- Added `SetPosition` and `SetSize` methods to `Popover` for programmatic positioning and sizing.
- Added `movable` property to `Popover` UI elements.
- Shipped Roboto Mono (`PackageResources/Fonts/RobotoMono-Regular.ttf`) as a runtime font asset and exposed it as a new design token `--appui-code-font-family` (declared in `Components/CodeBlock.uss`). The font was previously buried under `Editor/Editor Default Resources/Fonts/` and only reachable from editor windows; the Redux DevTools `--mono-font` reference was migrated to the new token.
- SplitViewSplitterCountChangedEvent dispatched when the number of splitters changes in a SplitView, allowing consumers to defer collapse/expand operations until splitters are ready
- Added `movable` property to `DialogTrigger` to support movable popovers via UXML.
- Bundled TextMate grammars (csharp, lua, css, xml, json) and themes (dark-plus, light-plus) under `PackageResources/SyntaxHighlighting/`. The XML and Lua grammars were stripped of their cross-language `source.java` / `source.c` includes so they tokenize standalone in our single-grammar `Registry`. Used by the new `CodeBlock` component for syntax highlighting when `APPUI_ENABLE_SYNTAX_HIGHLIGHTING` is enabled.
- Added support of system theme in WebGL. Use `Platform.darkMode` or register your callback on the `Platform.darkModeChanged` event to make changes in your application based on the current system theme.
- Added SortingOrder proeprty in Popups. This enable the possibility to insert popups in behind others exisiting ones.
- PanelRenderer support in UIToolkitAppBuilder<T> for hosting MVVM apps on Unity 6.5+
- Added support of drag-to-increment-value feature in `InputLabel` UI element.
- Added support of system clipboard in WebGL platform for text fields.
- New `MarkdownView` UITK component (`Unity.AppUI.Markdown` namespace) gated behind the `APPUI_ENABLE_MARKDOWN` scripting symbol. It parses CommonMark + GFM via the bundled Markdig 0.37.0 (`Runtime/Markdown/Plugins/Markdig.dll`) and renders into App UI components only — `Heading`, `Quote`, `Divider`, `LocalizedTextElement`, `Link`, plus the new `CodeBlock` — through a custom `RendererBase` (`UITKMarkdownRenderer`) with one object-renderer per CommonMark node type.
- Added **AI Agent Skills** pane in **Unity Package Manager UI**: You can now install/remove App UI skills for AI Agents in your workspace from the Package Manager window.
- Added support of Unity 6000.6
- UxmlCloneTreeGenerator now collects UxmlElementNameAttribute bindings from inherited base classes, respecting C# shadowing rules.
- `CodeBlock.showLineNumbers` property: when enabled, a 1-based line-number gutter appears on the left of the body. Both the gutter and the body live inside the same `ScrollView`, so vertical and horizontal scrolling keep the numbers aligned with their corresponding source lines.
- Added `check-circle` and `x-circle` icons as part of the minimal icons set
- Added support of CoreCLR for the migration from Mono scripting backend for Unity 6.5+
- Added support for the new EntityId API in Unity 6000.3.0a1 and newer.
- Registered the `copy` icon (regular variant) in `Icons - Minimal.uss` and `IconBrowser`'s required-icons list, so any consumer using the App UI minimal icon set has access to it. Used by the new `CodeBlock` component's hover-revealed copy-to-clipboard button.
- Added documentation about getting started using UI Builder.

### Removed

- Removed Android Activity (AppUIActivity, AppUIGameActivity) and AndroidManifest.xml from the App UI package. The Android platform integration now operates without a custom Activity or manifest.

### Changed

- `AnchorPopup.RefreshPosition` is now `public` instead of `protected`.
- Streamlined AI skill content by removing duplicated sections from the main skill, consolidating reference documentation pointers, and fixing a Task/TodoItem naming conflict in the MVVM example
- `ColorField` popover is now movable by default.
- Improved AI skill descriptions with richer trigger phrases for more reliable skill activation
- Default `resizeDirection` for `DialogTrigger` changed from `Vertical` to `Free`.
- Added CSS transition animations for background-color, border-color, and color properties on Button and ActionButton components
- Switch CodeBlock syntax highlighting to themed batch tokenization (TextMateLib v0.2.0) for faster rendering and reduced GC allocations

### Fixed

- Fixed the AppUI settings assets search and auto-assignation before building a project.
- Fix theme color matching in syntax-highlighted code blocks
- Fixed runtime bindings for `DateField` and `DateRangeField` `value` property.
- Composite Numerical fields (such as Vector fields, Bounds fields, etc) previous value returned in the ChangeEvent is now the correct one.
- Fixed UpHandler calls in Draggable manipulator.
- Fixed the help url in the inspector for App UI Settings assets.
- Fixed exception thrown on Tabs element when refreshing the indicator with an out of range value.
- Fixed columns size and added more info in Storybook window.
- Added missing `[CreateProperty]` attribute on `clickable` property across all pressable components to fix runtime data binding for `clickable.command` path (UUM-138492)
- Fixed the search bar in the Icon Browser remaining enabled when filtering yields no results
- Disable "Delete selected icons" context menu option in Icon Browser when only required icons are selected
- Set minimum window sizes for IconBrowser, NavigationGraphWindow, and DevToolsWindow to prevent layout issues when resized too small
- Fixed an issue when trying to dismiss ColorPicker popup consecutively from ColorField
- Fixed the use of obsolete API in Trackpad sample
- Fixed `VisualElementExtensions.IsOnScreen` method which gave wrong result in world-space panels.
- Fixed applying anchored popup positions only if the computed values are different than the existing ones to avoid floating point precision issues in the layout.
- Build the Linux x86_64 TextMateLib native plugin against an older glibc baseline with a statically linked C++ runtime, so it loads on Unity 2021.3 Linux (resolves the `GLIBC_2.32 not found` load error)
- Fixed TextArea auto-resize calculation to use explicit padding/border/margin measurements and trigger resize on geometry changes
- Added `CreateProperty` attribute to generated command properties for Unity 2023.2+ to enable Unity Properties Serialization support in the MVVM CommandGenerator.
- Fixed overlapping scrollbar arrow icons that appeared when resizing the Properties section in the App UI Storybook window
- Fixed code path activation for native plugins depending on target platform
- Fixed ActionButton CSS selector ordering for `.is-selected` to follow BEM conventions and corrected `--appui-alias-actions-border-color-selected` to reference the selected background color alias instead of a hardcoded base token
- Fixed iOS Simulator linker errors by shipping separate Device and Simulator native plugin binaries (libAppUINativePlugin.a and libAppUITextMateLib.a with bundled oniguruma)
- Fixed exception thrown inside the MVVM & Redux sample.
- Fixed sending ChangeEvent in numerical fields as soon as they are attached to a panel if their value has been changed already even without notification.
- App UI Icons StyleSheet Asset now has a StyleSheet icon in the Project window when creating the StyleSheet Asset.
- Fixed coordinates conversion in AnchorPopupUtils.ComputePosition method
- Fix Icon Browser "Add icons" button not working on Unity 6000.4+ due to ObjectSelector.Show API change (List\<int\> to List\<EntityId\>)
- Fixed a NullReferenceException in MenuItem.OpenSubMenu when Menu/MenuItem are declared inline in UXML without a MenuTrigger or MenuBuilder.Build() wrapper.
- Fix headless Windows IL2CPP build crash caused by the native plugin activating WinRT settings objects at static scope on Windows Server Core (game-ci/unity-builder#702)

### Deprecated

- Deprecated Badge `content`, `max`, and `showZero` properties in favor of using child elements for badge content

## [2.2.0-pre.12] - 2026-07-07

### Fixed

- Fix theme color matching in syntax-highlighted code blocks
- Build the Linux x86_64 TextMateLib native plugin against an older glibc baseline with a statically linked C++ runtime, so it loads on Unity 2021.3 Linux (resolves the `GLIBC_2.32 not found` load error)
- Fix headless Windows IL2CPP build crash caused by the native plugin activating WinRT settings objects at static scope on Windows Server Core (game-ci/unity-builder#702)

### Changed

- Switch CodeBlock syntax highlighting to themed batch tokenization (TextMateLib v0.2.0) for faster rendering and reduced GC allocations

## [2.2.0-pre.11] - 2026-06-04

### Added

- `IServiceCollection.AddSingleton<T>(T instance)` and `AddSingleton(Type, object)` overloads (plus matching `AddSingletonWhen(Type, object, ContextMatch)`) for registering pre-existing instances as singletons in the MVVM DI container. The supplied instance is returned as-is on every resolution; the container does not invoke a constructor or apply `[Service]` field/property injection on it.

### Fixed

- Disable "Delete selected icons" context menu option in Icon Browser when only required icons are selected
- Fixed iOS Simulator linker errors by shipping separate Device and Simulator native plugin binaries (libAppUINativePlugin.a and libAppUITextMateLib.a with bundled oniguruma)
- Fix Icon Browser "Add icons" button not working on Unity 6000.4+ due to ObjectSelector.Show API change (List\<int\> to List\<EntityId\>)

## [2.1.11] - 2026-06-04

### Fixed

- Improve Icon Browser selection visibility and tile contrast in light theme
- Disable "Delete selected icons" context menu option in Icon Browser when only required icons are selected
- Fixed iOS Simulator linker errors by shipping separate Device and Simulator native plugin binaries
- Fix Icon Browser "Add icons" button not working on Unity 6000.4+ due to ObjectSelector.Show API change (List\<int\> to List\<EntityId\>)

### Added

- PanelRenderer support in UIToolkitAppBuilder<T> for hosting MVVM apps on Unity 6.5+

## [2.0.6] - 2026-05-25

### Fixed

- Fixed a NullReferenceException in MenuItem.OpenSubMenu when Menu/MenuItem are declared inline in UXML without a MenuTrigger or MenuBuilder.Build() wrapper.

## [2.1.10] - 2026-05-22

### Fixed

- Fixed a NullReferenceException in MenuItem.OpenSubMenu when Menu/MenuItem are declared inline in UXML without a MenuTrigger or MenuBuilder.Build() wrapper.

## [2.2.0-pre.9] - 2026-05-20

### Added

- Added Thread UI components including Thread, ThreadMessage, ThreadComposer, ThreadReaction, ThreadReactionBar, ThreadResolveMessage, ThreadUnresolveMessage, MentionPicker, and ReactionPicker, along with their associated interfaces (IEmojiProvider, IMentionProvider, IThreadAttachment), base classes, enums, and USS styles. Renamed ThreadMessage `content` property to `message`, removed `isEditing` property in favor of `state = Draft`, moved `emojiProvider` and `mentionProvider` from ThreadMessage to ThreadContext, removed `Active` message state, and exposed explicit `attachments` container on ThreadMessage and ThreadComposer
- Added new icons (arrow-up, paperclip, pen, paper-plane-tilt, smiley, thumbs-up, thumbs-down) to the minimal icon set and IconBrowser required icons list
- Added TextMate grammar-based syntax highlighting support via native TextMateLib integration, including C# bindings (Registry, Grammar, Theme, Token), ScriptableObject assets (TextMateGrammarAsset, TextMateThemeAsset), ScriptedImporters for .tmLanguage and .tmTheme files, and a high-level TextMateSyntaxHighlighter API
- Added Edit and Delete text action constants with localization support in TextActions
- New `CodeBlock` UITK component (`Unity.AppUI.UI.CodeBlock`): rounded card with a header row (language label + hover-revealed copy button using `Platform.SetPasteboardData`) above a monospaced body. Syntax highlighting is configured declaratively from USS via two `CustomStyleProperty<string>` values (`--codeblock-grammar`, `--codeblock-theme`) holding TextMate asset GUIDs; setting the `language` property toggles a `appui-code-block--lang-<value>` modifier class so per-language grammar mappings can be expressed in the design system. When a `TextMateThemeAsset` resolves, the body's background and foreground colors are pulled from `Theme.GetDefaultBackground()`/`GetDefaultForeground()` so the card adopts the theme's palette.
- Shipped Roboto Mono (`PackageResources/Fonts/RobotoMono-Regular.ttf`) as a runtime font asset and exposed it as a new design token `--appui-code-font-family` (declared in `Components/CodeBlock.uss`). The font was previously buried under `Editor/Editor Default Resources/Fonts/` and only reachable from editor windows; the Redux DevTools `--mono-font` reference was migrated to the new token.
- Bundled TextMate grammars (csharp, lua, css, xml, json) and themes (dark-plus, light-plus) under `PackageResources/SyntaxHighlighting/`. The XML and Lua grammars were stripped of their cross-language `source.java` / `source.c` includes so they tokenize standalone in our single-grammar `Registry`. Used by the new `CodeBlock` component for syntax highlighting when `APPUI_ENABLE_SYNTAX_HIGHLIGHTING` is enabled.
- New `MarkdownView` UITK component (`Unity.AppUI.Markdown` namespace) gated behind the `APPUI_ENABLE_MARKDOWN` scripting symbol. It parses CommonMark + GFM via the bundled Markdig 0.37.0 (`Runtime/Markdown/Plugins/Markdig.dll`) and renders into App UI components only — `Heading`, `Quote`, `Divider`, `LocalizedTextElement`, `Link`, plus the new `CodeBlock` — through a custom `RendererBase` (`UITKMarkdownRenderer`) with one object-renderer per CommonMark node type.
- `CodeBlock.showLineNumbers` property: when enabled, a 1-based line-number gutter appears on the left of the body. Both the gutter and the body live inside the same `ScrollView`, so vertical and horizontal scrolling keep the numbers aligned with their corresponding source lines.
- Registered the `copy` icon (regular variant) in `Icons - Minimal.uss` and `IconBrowser`'s required-icons list, so any consumer using the App UI minimal icon set has access to it. Used by the new `CodeBlock` component's hover-revealed copy-to-clipboard button.

### Deprecated

- Deprecated Badge `content`, `max`, and `showZero` properties in favor of using child elements for badge content

### Fixed

- Fixed TextArea auto-resize calculation to use explicit padding/border/margin measurements and trigger resize on geometry changes
- Fixed ActionButton CSS selector ordering for `.is-selected` to follow BEM conventions and corrected `--appui-alias-actions-border-color-selected` to reference the selected background color alias instead of a hardcoded base token
- Fixed a NullReferenceException in MenuItem.OpenSubMenu when Menu/MenuItem are declared inline in UXML without a MenuTrigger or MenuBuilder.Build() wrapper.

### Changed

- Added CSS transition animations for background-color, border-color, and color properties on Button and ActionButton components

## [2.1.9] - 2026-04-07

### Fixed

- Added missing `[CreateProperty]` attribute on `clickable` property across all pressable components to fix runtime data binding for `clickable.command` path (UUM-138492)

## [2.0.5] - 2026-04-07

### Fixed

- Added missing `[CreateProperty]` attribute on `clickable` property across all pressable components to fix runtime data binding for `clickable.command` path (UUM-138492)

## [2.2.0-pre.8] - 2026-04-06

### Added

- Added `sizeModifier` callback property to `ResizeHandle` to allow custom size adjustments during resize.
- Added `UxmlTraits` support for `ResizeHandle` component.
- Added `SetPosition` and `SetSize` methods to `Popover` for programmatic positioning and sizing.
- Added `movable` property to `Popover` UI elements.
- Added `movable` property to `DialogTrigger` to support movable popovers via UXML.

### Changed

- `AnchorPopup.RefreshPosition` is now `public` instead of `protected`.
- `ColorField` popover is now movable by default.
- Default `resizeDirection` for `DialogTrigger` changed from `Vertical` to `Free`.

### Fixed

- Added missing `[CreateProperty]` attribute on `clickable` property across all pressable components to fix runtime data binding for `clickable.command` path (UUM-138492)

## [2.2.0-pre.7] - 2026-03-25

### Removed

- Removed Android Activity (AppUIActivity, AppUIGameActivity) and AndroidManifest.xml from the App UI package. The Android platform integration now operates without a custom Activity or manifest.

### Added

- Added **AI Agent Skills** pane in **Unity Package Manager UI**: You can now install/remove App UI skills for AI Agents in your workspace from the Package Manager window.
- Added support of Unity 6000.6

### Changed

- Streamlined AI skill content by removing duplicated sections from the main skill, consolidating reference documentation pointers, and fixing a Task/TodoItem naming conflict in the MVVM example
- Improved AI skill descriptions with richer trigger phrases for more reliable skill activation

## [2.1.8] - 2026-03-25

### Added

- Added support of Unity 6000.6

## [2.1.7] - 2026-03-20

### Fixed

- Fixed the use of deprecated EndNameEditAction in IconBrowser

## [2.2.0-pre.6] - 2026-03-02

### Added

- Add `destroyItem` callback to BaseGridView and fix doc comments
- Added HelpBox UI element
- Added `disableAnimation` property on Popup elements in order to force disable display animation for a given Popup UI Element.
- Added SortingOrder proeprty in Popups. This enable the possibility to insert popups in behind others exisiting ones.
- UxmlCloneTreeGenerator now collects UxmlElementNameAttribute bindings from inherited base classes, respecting C# shadowing rules.
- Added `check-circle` and `x-circle` icons as part of the minimal icons set

### Fixed

- Fixed UpHandler calls in Draggable manipulator.
- Fixed exception thrown on Tabs element when refreshing the indicator with an out of range value.
- Fixed sending ChangeEvent in numerical fields as soon as they are attached to a panel if their value has been changed already even without notification.

## [1.3.6] - 2026-02-18

### Fixed

- Fixed overlapping scrollbar arrow icons that appeared when resizing the Properties section in the App UI Storybook window

## [1.3.5] - 2026-01-31

### Fixed

- Fixed the help url in the inspector for App UI Settings assets.

## [2.2.0-pre.5] - 2026-01-30

### Fixed

- Fixed the help url in the inspector for App UI Settings assets.
- Fixed the search bar in the Icon Browser remaining enabled when filtering yields no results
- Set minimum window sizes for IconBrowser, NavigationGraphWindow, and DevToolsWindow to prevent layout issues when resized too small
- Fixed the use of obsolete API in Trackpad sample
- Fixed overlapping scrollbar arrow icons that appeared when resizing the Properties section in the App UI Storybook window
- Fixed exception thrown inside the MVVM & Redux sample.

## [2.1.6] - 2026-01-30

### Fixed

- Fixed type name conflict for MaskField in the UI Kit sample.
- Fixed the help url in the inspector for App UI Settings assets.
- Fixed the search bar in the Icon Browser remaining enabled when filtering yields no results
- Set minimum window sizes for IconBrowser, NavigationGraphWindow, and DevToolsWindow to prevent layout issues when resized too small
- Fixed the use of obsolete API in Trackpad sample
- Fixed overlapping scrollbar arrow icons that appeared when resizing the Properties section in the App UI Storybook window
- Fixed exception thrown inside the MVVM & Redux sample.

## [2.0.4] - 2026-01-30

### Fixed

- Fixed type name conflict for MaskField in the UI Kit sample.
- Fixed the help url in the inspector for App UI Settings assets.
- Fixed the search bar in the Icon Browser remaining enabled when filtering yields no results
- Set minimum window sizes for IconBrowser, NavigationGraphWindow, and DevToolsWindow to prevent layout issues when resized too small
- Fixed overlapping scrollbar arrow icons that appeared when resizing the Properties section in the App UI Storybook window
- Fixed exception thrown inside the MVVM & Redux sample.

## [2.0.3] - 2026-01-25

### Fixed

- Composite Numerical fields (such as Vector fields, Bounds fields, etc) previous value returned in the ChangeEvent is now the correct one.
- Fixed the use of obsolete API in Trackpad sample
- Fixed applying anchored popup positions only if the computed values are different than the existing ones to avoid floating point precision issues in the layout.
- Fixed the usage of obsolete InstanceId API.
- App UI Icons StyleSheet Asset now has a StyleSheet icon in the Project window when creating the StyleSheet Asset.

## [1.3.4] - 2026-01-25

### Fixed

- Fixed the usage of obsolete InstanceId API.
- Fixed the trackpad sample.

## [2.1.5] - 2026-01-24

### Fixed

- Fixed the usage of obsolete InstanceId API.

## [2.2.0-pre.4] - 2026-01-23

### Added

- Added support of DevicePixelRatio in WebGL native plugin to determine the current ScaleFactor applied to the canvas.
- Added Uxml cloneTree and element binding by name source generators. See the documentation for more information.
- Added runtime data binding support to `BaseGridView` component.
- Added the support of custom ColorPicker inside the ColorField element via customPicker property.
- Added support of system theme in WebGL. Use `Platform.darkMode` or register your callback on the `Platform.darkModeChanged` event to make changes in your application based on the current system theme.
- Added support of drag-to-increment-value feature in `InputLabel` UI element.
- Added support of system clipboard in WebGL platform for text fields.

### Fixed

- Composite Numerical fields (such as Vector fields, Bounds fields, etc) previous value returned in the ChangeEvent is now the correct one.
- Fixed applying anchored popup positions only if the computed values are different than the existing ones to avoid floating point precision issues in the layout.
- App UI Icons StyleSheet Asset now has a StyleSheet icon in the Project window when creating the StyleSheet Asset.

## [2.1.4] - 2026-01-23

### Fixed

- Composite Numerical fields (such as Vector fields, Bounds fields, etc) previous value returned in the ChangeEvent is now the correct one.
- Fixed applying anchored popup positions only if the computed values are different than the existing ones to avoid floating point precision issues in the layout.
- App UI Icons StyleSheet Asset now has a StyleSheet icon in the Project window when creating the StyleSheet Asset.

## [2.1.3] - 2025-12-10

### Fixed

- Fixed the AppUI settings assets search and auto-assignation before building a project.
- Fixed columns size and added more info in Storybook window.
- Fixed `VisualElementExtensions.IsOnScreen` method which gave wrong results in world-space panels.
- Fixed code path activation for native plugins depending on target platform

### Added

- Added support of CoreCLR for the migration from Mono scripting backend for Unity 6.5+

## [2.0.2] - 2025-12-10

### Fixed

- Fixed the AppUI settings assets search and auto-assignation before building a project.
- Fixed columns size and added more info in Storybook window.
- Fixed `VisualElementExtensions.IsOnScreen` method which gave wrong results in world-space panels.
- Fixed code path activation for native plugins depending on target platform

### Added

- Added support of CoreCLR for the migration from Mono scripting backend for Unity 6.5+

## [1.3.3] - 2025-12-10

### Fixed

- Fixed columns size and added more info in Storybook window.
- Fixed the AppUI settings assets search and auto-assignation before building a project.
- Fixed code path activation for native plugins depending on target platform

### Added

- Added support of CoreCLR for the migration from Mono scripting backend for Unity 6.5+

## [2.2.0-pre.3] - 2025-12-09

### Added

- A reference of the latest opened StyleSheet in IconBrowser tool is now stored per project. The next time you will open the tool, it will reload the latest StyleSheet.
- Added support of CoreCLR for the migration from Mono scripting backend for Unity 6.5+

### Fixed

- Fixed the AppUI settings assets search and auto-assignation before building a project.
- Fixed columns size and added more info in Storybook window.
- Fixed `VisualElementExtensions.IsOnScreen` method which gave wrong result in world-space panels.
- Fixed code path activation for native plugins depending on target platform

## [2.0.1] - 2025-11-17

### Fixed

- Added `CreateProperty` attribute to generated command properties for Unity 2023.2+ to enable Unity Properties Serialization support in the MVVM CommandGenerator.

## [2.1.2] - 2025-11-13

### Fixed

- Added `CreateProperty` attribute to generated command properties for Unity 2023.2+ to enable Unity Properties Serialization support in the MVVM CommandGenerator.

## [1.3.2] - 2025-11-07

### Fixed

- Added `CreateProperty` attribute to generated command properties for Unity 2023.2+ to enable Unity Properties Serialization support in the MVVM CommandGenerator.
- Fixed Source Generators culture for Turkish region.

## [2.2.0-pre.2] - 2025-11-06

### Fixed

- Fixed runtime bindings for `DateField` and `DateRangeField` `value` property.
- Added `CreateProperty` attribute to generated command properties for Unity 2023.2+ to enable Unity Properties Serialization support in the MVVM CommandGenerator.

### Added

- Added support for the new EntityId API in Unity 6000.3.0a1 and newer.

## [2.2.0-pre.1] - 2025-08-31

### Added

- Added Conditional Resolution in Dependency Injection system.
- Added documentation about getting started using UI Builder.

### Fixed

- Fixed an issue when trying to dismiss ColorPicker popup consecutively from ColorField
- Fixed coordinates conversion in AnchorPopupUtils.ComputePosition method

## [2.1.1] - 2025-08-01

### Fixed

- Fixed label of the blue channel slider in the ColorPicker
- Fixed hiding Alpha channel label in `ColorPicker` when `showAlpha` is set to `false`.

## [2.1.1-pre.1] - 2025-07-30

### Fixed

- Fixed label of the blue channel slider in the ColorPicker
- Fixed hiding Alpha channel label in `ColorPicker` when `showAlpha` is set to `false`.

## [2.1.0] - 2025-07-26

### Added

- Added input field text selection context menu to cut/copy/paste the currently selected text (starting Unity 2023.1).
- Added EyeDropper system (runtime-only) to pick a color in the last game frame.
- Added `goToStrategy` property in `SwipeView` component to choose how `GoTo` method should behave regarding animation direction.

### Fixed

- Fixed some Icons that had white horizontal or vertical borders.
- fix: Add missing InsideTopLeft case in ComputePosition switch statement
- Fixed App UI Settings registration in EditorBuildSettings.
- fix: Add tip/arrow placement logic for Inside\* popover placements
- Fixed event propagation bug in `Pressable` which sent the wrong pointer position value to ancestors when `keepEventPropagation` was `true`.
- Fixed GTK initialization on Linux platform when checking if any windowing system is available.

## [2.1.0-pre.3] - 2025-07-23

### Fixed

- Fixed GTK initialization on Linux platform when checking if any windowing system is available.

## [1.3.1] - 2025-07-21

### Removed

- Removed warning messages about using obsolete code.

## [2.1.0-pre.2] - 2025-07-11

### Fixed

- Fixed some Icons that had white horizontal or vertical borders.

## [2.1.0-pre.1] - 2025-07-11

### Added

- Added input field text selection context menu to cut/copy/paste the currently selected text (starting Unity 2023.1).
- Added EyeDropper system (runtime-only) to pick a color in the last game frame.
- Added `goToStrategy` property in `SwipeView` component to choose how `GoTo` method should behave regarding animation direction.

### Fixed

- fix: Add missing InsideTopLeft case in ComputePosition switch statement
- Fixed App UI Settings registration in EditorBuildSettings.
- fix: Add tip/arrow placement logic for Inside\* popover placements
- Fixed event propagation bug in `Pressable` which sent the wrong pointer position value to ancestors when `keepEventPropagation` was `true`.

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
- Use `resolvedStyle.scale` instead of `transform.scale` to scale elements such as the `Canvas` container.
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

## [2.0.0-pre.22] - 2025-06-18

### Added

- Added support of `ICommand` in `Pressable` manipulator.
- Added runtime binding support for commands in `Pressable` via `clickable.command` property in most interactable UI elements.

## [2.0.0-pre.21] - 2025-06-13

### Changed

- The `NavDestination` node now uses a `NavDestinationTemplate` as a delegate to create and set up a `INavigationScreen` when reaching this destination.
- Moved `NavDestination` specific settings such as `showAppBar` inside the new `DefaultNavDestinationTemplate`.

### Added

- Added a default implementation of `NavDestinationTemplate` named `DefaultNavDestinationTemplate` which handles the creation of default `NavigationScreen` objects.
- Added `INavigationScreen` interface, more extensible than the base class `NavigationScreen`.

## [2.0.0-pre.20] - 2025-06-11

### Removed

- Removed `Canvas.scrollDirection` property in order to support correctly the scroll direction set in the Operating System's preferences.

### Fixed

- Fixed tooltip that weren't showing up anymore since last pre-release.
- Use `resolvedStyle.scale` instead of `transform.scale` to scale elements such as the `Canvas` container.
- Fixed `VisualElementExtensions.IsInvisible` method to check recursively in ancestors if any element is considered as _invisible_.

### Changed

- Use `resolvedStyle.translate` instead of `transform.position` to move elements such as the `Canvas` container.
- Replaced the dropdown with an ObjectField to pick an available App UI settings asset.

### Added

- Added a safety check in TooltipManipulator to ensure the anchor element of the scheduled tooltip is still valid (not _invisible_) when it is time to _animate in_ the tooltip.
- Added monitoring of key press events in the TooltipManipulator in order to dismiss any existing tooltip when the user interacts with the keyboard.

## [2.0.0-pre.19] - 2025-04-22

### Fixed

- Fixed pinch gesture recognition sensitivity.
- Added one frame delay before processing Popover dismissal.

### Removed

- [Canvas] Removed applying cursor styles during pointer events as it was too costly in terms of performance.

## [2.0.0-pre.18] - 2025-03-04

### Added

- Added MaskField UI component.
- Added `EnumField` UI component.
- Added Source Generators for MVVM module. Source Generators helps to get rid of boilerplate code and focus on business logic.

### Fixed

- Fixed backgrounds color of the input fields.
- Fixed the merge of default and new `Argument` objects during navigation to a `NavDestination`.

### Changed

- Others assembly modules such as `Redux`, `MVVM` and `Navigation` has been configured to be auto-referenced in the Unity project's assemblies.

## [2.0.0-pre.17] - 2025-01-31

### Changed

- Replaced ClickEvent action in MenuItem builder by an EventBase action

### Fixed

- Fixed styling of MenuItem element
- Fixed some SplitView styling issues when nesting SplitViews in a visual tree.
- Fixed context propoagation per key

### Added

- Added scoped service provider implementation.
- Added `showExpandButtons` property to the `SplitView` UI element.

### Removed

- Removed EventBaseExtensionsBridge class

## [2.0.0-pre.16] - 2025-01-19

### Removed

- Removed `Unity.AppUI.Core.AppUI.DismissAnyPopups` method. Popups are now registered per `Panel` element.

### Changed

- Refactored SliderBase and BaseSlider for performance improvements and better features. The RangeSliderBase class has been removed and RangeSliderFloat/RangeSliderInt are now derived from SliderBase directly.
- The `Platform`, `AppUIInput` and `GestureRecognizer` classes now use pre-allocated buffers to deal with Touch events in a single frame, and expects a `ReadOnlySpan<AppUITouch>` type to work with instead of `AppUITouch[]`.
- Changed `ticks` related properties in Slider components with a new `marks` property. To configure marks you can you the new `step` property or directly set your `customMarks`
- Updated App UI Native plugins for MacOS and iOS.

### Added

- Added `scale` property on slider components to support custom non-linear scaling of displayed values in the UI.
- Added the support of inverted track progress in slider components.
- Added Redux Debugging tool.
- Added `formatFunction` along with `formatString` and grouped them into a new `IFormattable<TValueType>` interface. The `formatFunction` will give you the possibility to customize entirely the string formatted value of a given `TValueType`.
- Added a `RestrictedValuePolicy` on slider components where you can choose if the slider can take a value related to the `step` or `customMarks` with modifier keys or not.
- `VisualElementExtensions.HasAncestorsOfType{T}` method to verify if an element as any ancestor of a specific type.
- Added the serialzation of last selected indices in Storybook window to save and restore selection.
- Added `orientation` property on slider components, to support vertical sliders.
- Added `VisualElementExtensions.GetLastAncestorOfType{T}` method to retrieve the last ancestor of a specific type in the tree.
- Added `VisualElementExtensions.EnableDynamicTransform` method to set/unset the `UsageHints.DynamicTransform` flag in your element's usageHints.
- Added `RectExtensions.Approximately` method to verify if two Rect object are approximately the same.
- Popup elements such as `Popover`, `Modal` or `MenuBuilder` now get their view's `userData` filled with the `Popup` instance itself. This way you can retrieve information about the `Popup` instance that created this popup element from within the visual tree.
- Added USS custom properties to choose the font style of AccordionItem header and TabItem text.

### Fixed

- Fixed calling `shown` event callbacks when a `Modal` is displayed.
- Fixed Tabs emphasized color in Editor-light theme.
- Use `RenderTexture.GetTemporary` instead of `new RenderTexture` to optimize RT allocations, especially on tile-based renderers such as mobile platforms.
- Fixed an exception thrown when dismiss any popup of a panel when this panel becomes out of focus.

## [1.2.1] - 2024-12-17

### Fixed

- Avoid calling `WaitForCompletion` during Localization initialization to not get any error message in WebGL builds.

## [2.0.0-pre.15] - 2024-12-12

### Changed

- Renamed `Create/App UI/Settings` menu item into `Create/App UI/App UI Settings`

## [2.0.0-pre.14] - 2024-12-11

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

## [2.0.0-pre.13] - 2024-12-02

### Added

- Added `RegisterUpdateCallback` to register a callback on VisualElement that needs to be notified when App UI's main loop has done a new iteration.
- Added `VisualElementExtensions.IsOnScreen` method to determine if a `VisualElement` is on-screen or off-screen.
- Added `leadingContainer` and `leadingContentTemplate` properties to the `AccordionItem` element.

### Changed

- Rewrite of App UI shaders in pure `HLSL` instead of legacy `CG` language.
- Refactored the `FindResponderInChain` native method in MacOS platform to avoid undefined behaviours.
- `CircularProgress` and `LinearProgress` elements with `Indeterminate` variant won't be marked as `DirtyRepaint` if they are off-screen anymore.

### Fixed

- Fixed random segmentation fault on MacOS platform which appeared after domain reloads.
- Prevent GC to collect Platform configuration to not break communication with native plugins (MacOS and iOS).

## [1.2.0] - 2024-11-30

### Changed

- Backported Localization package support from App UI 2.x.

## [1.1.1] - 2024-11-27

### Changed

- App UI shaders are now optionally embedded in Player builds. You can change this setting in your main App UI settings instance. The default value is `true`.

### Fixed

- Fixed an exception thrown when trying to update UXML schemas in the Editor.

## [2.0.0-pre.12] - 2024-11-18

### Added

- Navigation: Added `NavigationRail` component which can be used inside NavigationScreen views.
- Added `autoShrink` property to the `TextArea` component to automatically shrink the component when the text fits in a smaller area. You need to set `autoResize` property to `true` to use the shrinking feature.
- Added `ResizeHandle` component. This special component can be added to your layout to resize a given `target` when dragging it. This component is currently used in the new resizable `Popover` element.
- Added `resizable` and `resizableDirection` properties in `Popover` class. You are now able to resize a `Popover` using the bottom-right corner of the popover pane. A resizable Popover will not be repositionned.
- Added Clipboard handling support for **UTF8 Text** and **PNG** format on _iOS_, _Windows_, _macOS_, and _Linux_ platforms. Please refer to the **Native Integration** documentation page for more information.

### Changed

- App UI shaders are now optionally embedded in Player builds. You can change this setting in your main App UI settings instance. The default value is `true`.

### Fixed

- Fixed Dark Gray 1200 color value in Design Tokens for Editor-Dark theme.
- Fixed a bug where `Popover` elements were not correctly anchored in Unity versions older than 6.
- Fixed usage of AppCompat theme for AppUI GameActivity on Android platform.
- Fixed an exception thrown when trying to update UXML schemas in the Editor.
- Register trickledown events for popups on the first child of the visual tree root element instead of the root element itself to avoid leaks.

## [2.0.0-pre.11] - 2024-10-01

### Changed

- Make Host optional when intializing an `App` implementation.
- Changed StoryBookEnumProperty class to become a generic type. You need to specify the Enum type as typedef parameter.
- Changed the styling of primary action buttons in AlertDialog UI element.
- Changed the Context API to propagate contexts over _internal_ components hierarchies and not just the high-level hierarchy (via `contentContainer`).

### Added

- Added support of UITK Runtime DataBinding system in ObervableObject class.
- Added PanAndZoom Manipulator
- Added CircularProgress story in Storybook sample.
- Added AlertDialog icon Design Tokens to customize icons directly from USS.
- Added ActionBar story in Storybook sample.
- Added AlertDialog examples in the UI Kit sample.
- Added Phosphor Icons
- Added support of Localization package in the Storybook window. You can now change the current used Locale in the window via a dropdown in the Storybook context toolbar. This dropdown will appear only if you have the Unity Localization package installed and have at least one existing Locale set up in your Localiztion settings.

### Fixed

- Fixed TouchSlider progress element overlapping parent's borders.
- Fixed force mark as dirty repaint Progress UI element when swtiching its variant.
- Fixed Localization support in the ActionBar UI element.
- Removed force blurring the BaseSlider (SliderFloat, TouchSliderFloat, etc) when the users stops interacting with it.

## [2.0.0-pre.10] - 2024-09-11

### Changed

- Renamed `IUIToolkitApp.mainPage` property into `IUIToolkitApp.rootVisualElement` for more clarity.
- Storybook stories are now sorted alphabetically.

### Added

- Added `IApp.services` property which returns the current `IServiceProvider` available for this instance of `IApp`. Thanks to this property you should be able to load a service from anywhere in your application by calling `App.current.services`.
- Added `LangContext.GetLocalizedStringAsyncFunc` to delegate the localization operation to a user-defined function.
- Added `IInitializableComponent` interface to `IApp` interface. Now when initializing a MVVM `App` object, the related `AppBuilder` will call `app.InitializeComponent` before hosting.
- Added `SelectedLocaleListener` manipulator that reacts to Localization Package's SelectedLocale changes in order to provide a new `LangContext` in the visual tree.
- Added `NavHost.makeScreen` property. By setting your own callback to this property, you will be able to customize the way to instantiate a `NavigationScreen` when navigating to a new destination. This can be useful when coupled to Dependency Injection so you can retreive instances of your screens from a `IServiceProvider`. By default, the property is set with the use of `System.Activator.CreateInstance`.
- Added some warning messages (Standalone builds only) when a LocalizedTextElement value cannot be localized.

### Fixed

- Fixed notify property changes for Picker.selectedIndex.
- Fixed path resolution of Stylesheets coming from Packages in Icon Browser tool.
- Fixed a regression where components using `Pressable` manipulator will not able to be clicked more than once if the cursor doesn't leave the component's layout rect.
- Fixed NullReferenceException thrown when fetching Localization tables.
- Fixed Screen Height calculation. UITK does not use Camera rect but blit on the whole screen instead.
- Fixed `InvalidOperationException` thrown by the damping effect animation of the `Canvas` component.
- Fixed the calculation of off-screen items in SwipeView with vertical direction.

## [2.0.0-pre.9] - 2024-09-03

### Changed

- Scrollable manipulator now stops the propagation of WheelEvent. This affects only the Drawer and SwipeView elements.
- Refactored the SwipeView element logic, without impacting the public API.
- The Canvas now uses `Experimental.Animation` system from UI-Toolkit for its damping effect when releasing the mouse with some velocity. That replaces the previous implementation that was using the `VisualElementScheduledItem`.
- Changed fade in animation in Tooltip to use USS transitions.

### Fixed

- Fixed the wrap system of the SwipeView element when swiping between elements quickly.
- Fixed Daisy chaining window procedures on Windows platform.
- Fixed an edge case when popovers are dismissed as OutOfBounds as soon as Show() is called.
- Invoke click event only if Pressable is still hovered.
- Fixed styling of emphasized checkboxes.
- Fixed a bug where tooltips stop being shown when the window is docked/undocked.

### Added

- Added `justified` property on Tabs component to jusitfy tabs layout in horizontal direction.
- Added `damping-effect-duration` property in Canvas element. The default value is 750ms.
- Added `IsContextClick()` extension method for PointerEvent.
- Added support of graceful fallback to lambda `Plafform` implementation if native plugins can not be loaded by the current plaftorm.

## [2.0.0-pre.8] - 2024-08-12

### Changed

- Refactored completely the DropZone UI element. Now the DropZone doesn't embed any logic, but uses a `DropZoneController` instead. You can access this controller via `DropZone.controller` property and attaches a callback method to accept dragged objects and listens to drop events.
- Defer checking Popup's container candidate when the Popup is about to be shown, instead of during Popup creation.
- Refactored the styling of Chip UI element.
- Upgraded the old Drag And Drop Sample to use the refactored DropZone and the new Drag And Drop system.
- Changed the way MarkDirtyRepaint is scheduled on elements containing animated textures (check properly for visibility).
- Refactored the styling of DropZone UI element.

### Added

- Added the `Unity.AppUI.UI.DropZoneController` Manipulator for a lower level approach to create your own "drop zones".
- Added `--appui-splitview-splitter-anchor-size` design token.
- `Added Unity.AppUI.Core.DragAndDrop` class to handle drag and drop (in-panel and/or with the Editor). The support of external drag and drop at runtime is planned for future releases.
- Added `Unity.AppUI.UI.Chip.deletedWithEventInfo` and `Unity.AppUI.UI.Chip.clickedWithEventInfo` events.
- Added new Story in the Drag And Drop sample.
- Added support of `TextElement.displayTooltipWhenElided` to show elided text as a tooltip using the App UI tooltip system.

### Fixed

- Fixed compilation errors when the Unity project's Input Handling is set to `Both` or `New Input System` and the package `com.unity.inputsystem` is not installed.
- Fixed IL2CPP Compilation errors on Windows Platform due to non-static MonoPInvokeCallback.
- Fixed PInvoke delegate types on Windows platform.
- Fixed styling of SplitView's Splitter Anchor size.
- Fixed styling of BaseGridView when containing a single column.
- Popovers and Modals now correctly start checking for PointerDown events in the visual tree when they become visible.
- Removed console message when trying to add an Editor MonoBehaviour in the scene during PlayMode.

## [2.0.0-pre.7] - 2024-07-30

### Changed

- Changed DialogTrigger.keyboardDismissDisabled to DialogTrigger.keyboardDismissEnabled for consistency.
- Renamed Popup.parentView to Popup.containerView for more clarity.

### Added

- Added Modal.outsideClickDismissEnabled and Modal.outsideClickStrategy properties to support dismissing Modals by clicking outside of them.
- Added Popup<T>.SetContainerView method to set a custom container which will be the parent of the popup's view.
- Made `AnchorPopup.GetMovableElement` method public for easier access and increase customization possibilities.

### Fixed

- Fixed "Shape" icon.
- Fixed border color variable for AccordionItem.
- Fixed CultureInfo used during source code generation.

### Removed

- Removed intrusive Debug.Log calls from Platform class on Windows platform.
- Removed warning message when using Single selection type in an overflown ActionGroup.

## [2.0.0-pre.6] - 2024-07-08

### Added

- Added support of Attributes on fields and properties in the Dependency Injection system.
- Added support of UnityEditor ColorPicker in ColorField component.
- Added support of Color without alpha information in ColorField and ColorPicker.
- Added `rounded-progress-corners` boolean property in `CircularProgress` and `LinearProgress` to be able to disable rounded corners.
- Added `trailing-icon` property in `ActionButton` component.
- Added customization support for the size of the Color swatch inside the ColorField.
- Added arrow-square-in icon.

### Changed

- Changed the Text element inside the ColorField to become a selectable text.

### Fixed

- Fixed styling issues in `ActionButton` component.
- Fixed Menu's backdrop to block pointer events
- Fixed ColorField styling issues.
- Use correct color variables for Radio and Checkbox components

## [2.0.0-pre.5] - 2024-06-18

### Changed

- Complete rewrite of the SplitView component. The SplitView is no more a derived from TwoPaneSplitView from UI-Toolkit, but a full custom component that supports any number of panes.

### Fixed

- Fixed reset of Dropdown value when changing its source items.
- Fixed a visual bug where the checkmark symbol didn't appear on DropdownItem or MenuItem that have a `selected` state.

### Added

- Added `indicatorPosition` property in `AccordionItem` component, in order to swap the indicator position either at start or end of the heading row.
- Added `check` regular icon as a required icon in App UI themes.

## [2.0.0-pre.4] - 2024-06-05

### Fixed

- Fixed MacOS native plugin memory leak when opening the Help menu in the Editor.

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

## [2.0.0-pre.2] - 2024-05-08

### Added

- Added `PinchGestureRecognizer` implementation for the new Gesture Recognizer System.
- Added an experimental method `Platform.GetSystemColor` to fetch color values defined by the Operating System for specific UI element types. This can be useful if you want to precisely follow the color palette of a high-contrast theme directed by the OS.
- Added "Icon Browser", a new Editor tool that enables users to generate UI-Toolkit stylesheets with a specific list of icons.
- Added a new experimental Gesture Recognizer System.
- Added the ability to subscribe and check if the current operating system is in Reduce-Motion Accessibility Mode (Windows/Mac/Android/iOS).
- Added the ability to subscribe and check if the current Text Scale Factor of the currently used window (Unity Player window or the Game view window in the Editor) (Windows/Mac/Android/iOS).
- Added the ability to subscribe and check if the current operating system is in High-Contrast Mode (Windows/Mac/Android/iOS).
- Added the ability to subscribe and check if the current operating system is in LeftToRight or RightToLeft layout direction (Windows/Mac/Android/iOS).
- Added the ability to subscribe and check if the current Scale Factor of the currently used window (Unity Player window or the Game view window in the Editor) (Windows/Mac/Android/iOS).
- Added the ability to subscribe and check if the current operating system is in Dark Mode (Windows/Mac/Android/iOS).

### Changed

- Refactored every native plugin provided by the package.
- Changed the Trackpad sample project to work properly with the new events coming from the new Gesture Recognizer System.

### Fixed

- Fixed meta files for native plugins on Windows platform.
- Fixed an early return in the PreProcessBuild callback of App UI when no persistent AppUISettings have been found.

## [1.0.6] - 2024-03-18

### Fixed

- Fixed the tooltip tip size to not be displayed over tooltip content.
- Fixed the handling of Tab key to focus the next component from a TextArea
- Fixed size of the Radio button for pixel alignment on 96dpi screens.

## [1.0.5] - 2024-03-11

### Fixed

- Fixed Hero banner in the documentation homepage

## [1.0.4] - 2024-03-11

### Fixed

- Fixed GridFour icon PNG file
- Fixed font weight on AppBar title elements.
- Fixed accent color in Editor Dark theme
- Fixed ActionGroup Refresh strategy.

### Added

- Added gutter USS custom property for the GridView component.
- Added answer in the FAQ documentation about MacOS quarantine attribute.

## [1.0.3] - 2024-03-04

### Fixed

- Use equlity check while setting a new `text` value in a `LocalizedTextElement`.
- Fixed performance issues with the blinking text cursor in input fields.
- Fixed styling of the Toast Action container

## [1.0.2] - 2024-02-28

### Fixed

- Fixed the clipping flag on the GridView's ScrollView content container to prevent overscroll on touch devices.
- Fixed unit tests
- Disabled the dragging feature in the GridView when used on touch devices.

### Added

- Improved Localization Unity Package support with `LangContext` propagation.

## [1.0.1] - 2024-02-16

### Fixed

- Fixed ColorSwatch shader to support Shader Model older than 5.0

## [1.0.0-pre.15] - 2024-02-09

### Removed

- Removed the `margin` property from the Tray component.
- Removed the `size` property of the `Tray` component. The Tray component will fit its content in the screen automatically and doen't need anymore any specific size to be set.
- Removed the `expandable` property from the Tray component. A Tray component is should not be expandable by definition. If you are willing to have some scrollable content inside a Tray component, a new component called ScrollableTray will be avilable in future release of App UI.

### Fixed

- Fixed styling on BottomNavBar items
- Fixed refresh of the `ActionGroup` component
- Fixed Tooltip maximum size

### Added

- Added monitoring of AccordionItem content size to make AccordionItem fit its content when it is already open.

### Changed

- Use the ConditionalWeakTable for new releases of Unity where a fix from IL2CPP has landed.

## [1.0.0-pre.14] - 2024-01-31

### Fixed

- Fixed ColorSwatch refresh when the Gradient reference value didn't change but Gradient keys have changed
- Added support of Windows 11 for ARM
- Fixed ColorPicker Alpha channel slider refresh when the picked color has changed
- Fixed calls for ContextChanged callbacks registered by the context provider itself

### Added

- Added new IInputElement interface in the public API
- Added the possibility to setup navigation visual components from your NavigationScreen implementation directly. You are still able to create a global setup using your own implementation of the INavVisualController interface that you have set on your NavHost.
- Added the support of a specific drag direction and drag threshold in the Draggable manipulator. Specifying a direction will avoid to prevent scrolling in the opposite direction if this maniuplator is used inside a ScrollView for example.
- Added forceUseTooltipSystem property on the Panel component to force the use of App UI tooltips system regardless the state of UI-Toolkit default tooltip system. This can be useful in a context where UITK tooltips can't be displayed in the Editor.
- Added mention about full integration of UI Toolkit with the New Input System starting 2023.2 in the documentation

### Changed

- All user input related UI controls now inherit from the new IInputElement interface. This has no impact on the current implementation (some addition in certain components).
- Use ConditionalWeakTable whenever possible (Editor and no IL2CPP builds)

## [1.0.0-pre.13] - 2024-01-15

### Removed

- Removed Relocations folder from MacOS native plugin debug symbols bundle

### Fixed

- Fixed TextField and NumericalField text offset during FocusOut event

### Added

- Added missing API documentation
- Added more tests

### Changed

- Changed assembly definition files to support new UXML Serialization starting Unity 2023.3.0a1

## [1.0.0-pre.12] - 2024-01-04

### Changed

- Color related components, such as `ColorSlider` or `ColorSwatch`, support now a value of type `UnityEngine.Gradient` instead of `UnityEngine.Color` for more flexibility.
- Removed the `disabled` boolean property on App UI components from the public C# API. The `disabled` attribute in UXML has been replaced by `enabled` UXML attribute. This change has been made in order to be more consistent with Unity 2023.3, where `enabled` UXML attribute is provided by UI Toolkit on any VisualElement.
- Replaced `Nullable<T>` properties in components by a custom serializable implementation called `Optional<T>`
- Renamed `Panel.dir` property with `Panel.layoutDirection`.
- Replaced the `Divider.vertical` property by `Divider.direction` enum property.
- Removed the `ApplicationContext` class and `VisualElementExtensions.GetContext()` method, replaced by the ne `ProvideContext` and `RegisterContextChangedCallback` API.
- Replaced `ActionGroup.vertical` property by `ActionGroup.direction` enum property.

### Added

- Added support of new UI Toolkit Runtime Bindings feature through bindable properties in each App UI components. More than 420 properties can now be bound (2023.2+).
- Added support to new UI Toolkit Uxml Serialization using source code generators.
- Added `BaseVisualElement` and `BaseTextElement` classes which are used as base class for mostly every App UI component
- Added numerous custom PropertyDrawers for a better experience in UIBuilder (2023.3+)
- Added the support of `Fixed` gradient blend mode in ColorSwatch shader.
- Added `.appui-row` USS classes which support current layout direction context.

### Removed

- Starting Unity 2023.3, App UI will not provide any `UxmlFactory` or `UxmlTraits` implementation, as they become obsolete and replaced by the new UITK Uxml serialization system.

### Fixed

- Fixed Sliders handles to not exceed the range of track element.
- Fixed color blending in the ColorSwatch custom shader.
- Fixed `AppBar` story in Storybook samples.
- Fixed some styling issues on different components.
- Cleaned up some warning messages to not get anything written in the console during package installation.
- Fixed small errors in UI Kit sample.
- Fixed a refresh bug in the App UI Storybook window.

## [1.0.0-pre.11] - 2023-12-21

### Fixed

- Fixed Scroller styling.
- Fixed styling for Disabled Quiet Button component.
- Fixed compilation errors while not having the new Input System installed but the Input Manager setting is set to 'New' in the Project Settings.
- Fixed border gap in TouchSlider component.
- Fixed margins on Checkbox component without label.

### Changed

- Removed `size` property from Checkbox and Toggle components.

## [1.0.0-pre.10] - 2023-12-13

### Fixed

- Fixed auto scrolling to follow text edition's cursor in TextArea component.
- Fixed layout for Bounds, Rect and Vector fields
- Fixed some examples in the UI Kit Sample

### Added

- Added `maxLength` property in TextArea and TextField components.
- Added `isPassword` and `maskChar` properties to TextField component.
- Added Editor-Dark and Editor-Light themes.
- Added `isReadonly` property to TextArea and TextField components.

### Changed

- Readjusted font sizes used on components to follow new design tokens
- Most App UI component styling now use component-level design tokens for background and border colors

## [1.0.0-pre.9] - 2023-12-06

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

## [1.0.0-pre.5] - 2023-11-16

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

## [1.0.0-pre.4] - 2023-11-13

### Changed

- The `Canvas` component now listens to `PointerDownEvent` in `TrickleDown` phase to avoid to miss the event when the pointer is over a child element.

## [1.0.0-pre.3] - 2023-11-07

### Added

- Added public access to `Pressable.InvokePressed` and `Pressable.InvokeLongPressed` methods.
- Added `trailingContainer` property to the `AccordionItem` component.

### Changed

- The `Pressable` manipulator now uses the `keepPropagation` property also in its `PointerUpEvent` callback to avoid to propagate the event to the parent element.

### Fixed

- Fixed warning message about Z-axis scale in the `AnchorPopup` component.
- Fixed styling of the `AccordionItem` header component.
- Fixed the `not-allowed` (disabled) cursor texture for Windows support at Runtime.
- Fixed images in the package documentation.

## [1.0.0-pre.2] - 2023-11-06

### Added

- Added missing XML documentation parts for the public API of the package.

## [1.0.0-pre.1] - 2023-11-06

### Added

- Added debug symbols for native plugins.

### Changed

- Moved internal Engine API related code into a new assembly `Unity.AppUI.InternalAPIBridge`.

## [0.6.5] - 2023-11-06

### Added

- Added missing XML documentation parts for the public API of the package.
- Added the `primaryManipulator` property to the `Canvas` component to get the primary pointer manipulator used by the canvas when no modifier is pressed.
- Added the `autoResize` property to the `TextArea` component to grow the text area when the text overflows.
- Added the handling of double-click in the `TextArea` resizing handle to re-enable the auto-resize feature.
- Added the `outsideScrollEnabled` property to the `AnchorPopup` components to enable or disable the outside scroll handling. The default value is `false`.
- Added a smoother animation for the `AnchorPopup` components when the popup is about to be shown.

### Changed

- Changed the USS selector for component-level aliases to use directly `:root` selector instead of `<component_main_uss_class>` selector.

### Fixed

- Fixed the Canvas background to support light theme.
- Fixed the styling of DropdownItem checkmark.
- Fixed the styling of MenuItem checkmark.
- Fixed the styling of Slider components.
- Fixed the styling of TextArea component.
- Fixed the styling of TouchSlider components.
- Fixed the previous value sent in `ChangeEvent` of `NumericalField`, `VectorField` and `Picker` (Dropdown) components.
- Improved the synchronization of the `AnchorPopup` components to refresh their position in the layout faster.

## [0.6.4] - 2023-11-01

### Fixed

- Fixed `NullReferenceException` when testing accept drag in `GridView` component.

## [0.6.3] - 2023-11-01

### Added

- Added `getItemId` property to the `GridView` component to get the id of an item.

### Fixed

- Fixed items selection persistence between refreshes in `GridView` component.

## [0.6.2] - 2023-10-30

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

## [0.5.5] - 2023-10-30

### Added

- [Backport] Added `closeOnSelection` property to the `MenuTrigger` component.
- [Backport] Added `closeOnSelection` property to the `ActionGroup` component to close the popover menu (if any) when an item is selected.

### Fixed

- [Backport] Fixed `closeOnSelection` property in `MenuBuilder` component.
- [Backport] Fixed `TextInput` styling for Unity 2023.1+
- [Backport] Fixed `InvalidCastException` at startup of santandlone builds (in both Mono and IL2CPP)
- [Backport] Fixed `NullReferenceException` when dismissing a Popup from a destroyed UI-Toolkit panel.
- [Backport] Fixed context click handling in `GridView` component.

## [0.6.1] - 2023-10-23

### Added

- Added `allowNoSelection` property to the `GridView` component to enable or disable the selection of no items.

### Fixed

- Fixed the support of nested components inside the `Button` component.
- Fixed the reset of the previous selection when `GridView.SetSelectionWithoutNotify` method is called.

## [0.5.4] - 2023-10-23

### Added

- Added `allowNoSelection` property to the `GridView` component to enable or disable the selection of no items.

### Fixed

- Fixed the support of nested components inside the `Button` component.
- Fixed the reset of the previous selection when `GridView.SetSelectionWithoutNotify` method is called.

## [0.6.0] - 2023-10-21

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

## [0.5.3] - 2023-10-18

### Added

- Added `useSpaceBar` property in `Canvas` component to enable or disable the handling of the space bar key.

### Fixed

- Removed the handling of the space bar key in the `Canvas` component when the used control scheme is `Editor`.

## [0.5.2] - 2023-10-13

### Added

- Added `acceptDrag` property to the `GridView` component to enable or disable the drag and drop feature.
- Added `menu` icon.
- Added `AddDivider` and `AddSection` methods to the `MenuBuilder` component.

### Fixed

- Fixed handling `acceptDrag` property in `Dragger` manipulator.
- Fixed MenuItem opening sub menus when the item is disabled.
- Fixed mipmap limit for Icons when a global limit is set in the Quality settings of the project.
- Fixed the capture of pointer during PointerDown event in `Dragger` manipulator. Now the pointer is captured only if the `Dragger` manipulator is active (i.e. the threshold has been reached).

## [0.5.1] - 2023-09-21

### Added

- Added `selectedIndex` property to the `Picker` component to get or set the selected index for a single selection mode.
- Added a new `Enumerable` extension method called `GetFirst` to get the first item of an enumerable collection or a default value if the collection is empty.
- Added `isPrimaryActionDisabled` and `isSecondaryActionDisabled` properties to the `AlertDialog` component.
- Added styling for `AlertDialog` component semantic variants.

### Changed

- Changed the picking mode of the `DropdownItem` component to `Position` instead of `Ignore`.

### Fixed

- Fixed the `Picker` component to avoid to select multiple items if the selection mode is set to `Single`.
- Fixed `Menu.CloseSubMenus` method to close sub-menus opened by `MenuItem` components contained inside `MenuSection` components.
- Fixed double click handling in `GridView` component.

## [0.5.0] - 2023-09-18

### Added

- Added `outsideClickStrategy` property in `AnchorPopup` component.
- Added `DropZone` component.
- Added `DropTarget` manipulator.
- Added a sample for `DropZone` and `DropTarget` components usage.
- Added `--border-style` custom USS property to support `dotted` and `dashed` border styles.
- Added `--border-speed` custom USS property to animate the border style.
- Added `Picker` component.
- Added `closeOnSelection` property for `Picker` and `Dropdown` components to close the picker when an item is selected.

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

### Fixed

- Fixed the `Pressable` manipulator to only handle the Left Mouse Button by default.
- Fixed current tooltip element check in `TooltipManipulator`.
- Fixed incremental value when interacting with keyboard in `ColorSlider` component.
- Fixed clearing selection in `GridView` when a user clicks outside of the grid.
- Fixed `itemsChosen` event in `GridView` to be fired only when the selection is not empty.
- Fixed Navigation keys handling in the `GridView` to clamp the selection to the grid items.

## [0.4.2] - 2023-09-07

### Added

- Added the `Picker` component.

### Changed

- Changed `Dropdown` implementation to use the new `Picker` component.

### Fixed

- Fixed the soft selection handling on `PointerUpEvent` in the `GridView` component.

## [0.4.1] - 2023-08-30

### Added

- Added mouse and touchpad mapping presets to the `Canvas` component.
- Added a setting to enable or disable gestures recognition on MacOS.
- Added selection mode in `ActionGroup` component.

## [0.4.0] - 2023-08-21

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

## [0.3.8] - 2023-08-09

### Added

- Added `tooltip-delay-ms` property to the `ContextProvider` component to customize the tooltip delay.
- Added more shortcuts to the `Canvas` component.

### Fixed

- Fixed Editor crash when updating packages from UPM window.

## [0.3.5] - 2023-07-26

### Fixed

- Regenerated the meta files inside MacOS bundle folder to avoid error messages in the console.

## [0.3.4] - 2023-07-19

### Changed

- Draggable manipulator now is publicly accessible.

### Fixed

- Ensure shaders exist before creating materials.
- Fixed random crashes during domain unload in Unity 2022.3+.
- Fixed cursors variables for Editor context.

## [0.3.3] - 2023-07-17

### Added

- Added Magic Trackpad gesture support for MacOS.
- Added `PanGesture` and `MagnificationGesture` events for UITK dispatcher.

### Fixed

- Fixed `Pressable` post-processing for disabled targets.
- Fixed visual content rendering synchronization in `Progress` and `ExVisualElement` components.
- Fixed Animation Recycling in `NavHost` component.

## [0.3.2] - 2023-06-14

### Fixed

- Fixed NavAction being added twice in NavGraph when deleting a linked NavDestination.

## [0.2.12] - 2023-06-14

### Added

- Added `isExclusive` property to the `Accordion` component.
- Added `shortcut` property to the `MenuItem` component.

### Fixed

- Fixed TextArea input size.
- Fixed ValueChanged events on text based inputs.
- Fixed depth tests in custom shaders for WebGL platform.

## [0.3.1] - 2023-05-22

### Fixed

- Fixed warning messages about styling.
- Fixed warning messages about GUID conflicts in UI Kit samples.
- Fixed warning messages about unused events in NavController.

## [0.2.11] - 2023-05-15

### Added

- Added `maxDistance` property to the World-Space UI Document component.

## [0.2.9] - 2023-05-05

### Changed

- Removed `Replica` word from the documentation.

## [0.2.7] - 2023-05-03

### Added

- Added `Refresh` method to the `Dropdown` component.
- Added AutoPlay to the `SwipeView` component.

### Fixed

- Fixed the support of the New Input System.
- Fixed the box-shadow shader with portrait aspect ratio.
- Fixed async operations on LocalizedTextElement.

