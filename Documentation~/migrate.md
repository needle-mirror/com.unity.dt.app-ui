---
uid: migrate
---

# Migration Guide

This guide will help you to migrate from the previous version of App UI to the latest one.

## Button variants

The `Button` component has been updated to use the `variant` prop instead of `primary` boolean prop. 

The `variant` prop accepts the following values:

- `Default`
- `Accent`
- `Destructive`

If you want to get the same result as the previous `primary` prop, you can use the `Accent` variant.

## Icons

Icons PNG files has been updated with newer version of the App UI Design System available in Figma.

Certain file names has been fixed to match our naming convention without any exception. 
You may need to update your code to use the new icon names.

App UI doesn't offer any `@2x` or `@3x` icons anymore. The unique size available is `256x256` pixels 
but mipmap generation is still supported.

Icons has been moved to a new `Regular` folder to follow the same structure as Phosphor icons.
