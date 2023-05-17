---
uid: migrate
---

# Migration Guide

This guide will help you to migrate from the previous version of App UI to the latest one.

## Namespace changes

The namespaces used by the package **has changed** from `UnityEngine.Dt.AppUI` to `Unity.AppUI`.

To migrate your code, you can use a refactoring tool from your IDE like [JetBrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio](https://visualstudio.microsoft.com/).

### UXML references

If you are using UXML files, you will need to update the namespace references in your UXML files too.

You will need to delete the UIElementsSchema folder at the root of your project (if it exists) and reimport the package to regenerate the schema.

## Deprecated components

The following components are deprecated and will be removed in a future release:

- `Header` component is now named `Heading`. The old name is still supported but will be removed in a future release.
