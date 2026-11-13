---
uid: styling
---

# Styling

App UI Unity Package provides a set of pre-defined styles
based on the CSS App UI library.
These styles can be used to quickly style your UI components,
without the need to create custom styles from scratch.

The package uses the [BEM](http://getbem.com/)
naming convention for its [USS](xref:UIE-USS)
(Unity Style Sheet) classes.
This makes it easy to create and apply consistent styles to different elements in your UI.
You can use these classes to define styles for individual UI elements
or to create custom stylesheets for your entire app.

In addition, App UI provides a set of pre-defined themes to help you
quickly style your UI. These themes are designed to ensure consistency throughout your app.
You can choose from several color schemes and typography styles that best
fit your app's visual style. For more advanced theming options, you can also
customize the themes or create your own from scratch.
The [Theming](xref:theming) section goes into more detail on how to use and customize the provided
themes.

## Using App UI Styles

When using the default App UI theme (`App UI.tss`),
the default styling will be applied to all UI elements in your app.

Each component styling is defined in a separate stylesheet.
And every stylesheet is referenced into the `App UI.uss` stylesheet.

## Using Custom Styles

You can create your own custom stylesheets to override the default styles.
You can follow the steps given in the [theming](xref:theming) section to create your own custom stylesheets.

Then use provided stylesheet per component as reference to create your own styling.

Here is an example with the button component:

```css
/* Button.uss */
.appui-button {
    padding: 0 var(--appui-sizing-100);
}
```

> [!NOTE]
> It is good practice to use USS variables coming from our **Themes** folder
> since they are designed to follow the App UI Design System guidelines.

## Disabled states

A disabled state propagates to every descendant: `SetEnabled(false)` puts the element
and its whole subtree in the `:disabled` pseudo-state. A rule written as
`opacity: 0.5` then matches at every level of that subtree, and opacity multiplies
down the tree (0.5 × 0.5 × …), so a field inside a disabled row inside a disabled
panel all but disappears.

Colours don't compound, so style a disabled state with the `*-disabled` colour
variables of the element's role, never with opacity. Each theme sets its own values
for them, so the disabled look follows the theme too.

```css
/* MyField.uss */
.my-field:disabled {
    background-color: var(--appui-alias-field-background-color-disabled);
    border-color: var(--appui-alias-field-border-color-disabled);
    color: var(--appui-alias-text-disabled);
    cursor: var(--appui-alias-control-cursor-disabled);
}
```

| Role | Variables |
| --- | --- |
| Field | `--appui-alias-field-background-color-disabled`, `--appui-alias-field-border-color-disabled`, `--appui-alias-text-disabled` |
| Button | `--appui-alias-actions-background-color-disabled`, `--appui-alias-actions-border-color-disabled`, `--appui-alias-actions-foreground-color-disabled` |
| Quiet button | `--appui-alias-actions-quiet-background-color-disabled`, `--appui-alias-actions-quiet-border-color-disabled`, `--appui-alias-actions-quiet-foreground-color-disabled` |
| Control | `--appui-alias-control-background-color-disabled`, `--appui-alias-control-border-color-disabled`, `--appui-alias-control-foreground-color-disabled` |
| Text and icons | `--appui-alias-text-disabled`, `--appui-alias-icon-disabled` |

The web components of the App UI registry follow the same rule, so a control looks
disabled the same way in both implementations.

