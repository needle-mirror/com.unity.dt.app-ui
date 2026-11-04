---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.6] - 2026-11-04

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

