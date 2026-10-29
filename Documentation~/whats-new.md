---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

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

