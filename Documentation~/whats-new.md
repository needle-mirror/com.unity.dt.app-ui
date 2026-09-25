---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.4] - 2026-09-25

### Fixed

- Fixed Tabs throwing "Cannot scroll to a VisualElement that's not a child of the ScrollView content-container" when TabItems are deleted from it in UI Builder. Tabs now picks up TabItems added to or removed from its item container directly, and keeps the same tab selected when it can (UUM-154225).

