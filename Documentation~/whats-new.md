---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.3.0] - 2026-10-30

### Added

- Factory registration for the dependency injection container: `ServiceDescriptor.Singleton`, `Scoped` and `Transient` accept a `Func<IServiceProvider, object>` (or a generic `Func<IServiceProvider, TService>`), and `AddSingleton`, `AddScoped` and `AddTransient` have matching overloads. A factory can forward to another registration, so one singleton can be exposed under two interfaces. `ServiceDescriptor.Singleton<TService>(TService instance)`, a public `ServiceDescriptor(Type, Func<IServiceProvider, object>, ServiceLifetime)` constructor and the `ServiceDescriptor.implementationFactory` property were added too.

