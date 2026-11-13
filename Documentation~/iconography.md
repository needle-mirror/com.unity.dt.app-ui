---
uid: iconography
---

# Iconography

The Iconography of App UI regroups more than 100 icons in total,
but for the sake of optimization, we only provide a subset of them in the default [themes](xref:theming).
The icons are provided as SVG files and are set up to be loaded via USS classes.

If you are willing to expand the set of icons, you can add your own icons to the project and reference them in a USS file.
We also provide an Editor tool called [Icon Browser](xref:custom-icons#icon-browser) to generate the USS classes for you.

For more information about Icon customization, see [Custom Icons](xref:custom-icons).

## Usage

The [Icon](xref:Unity.AppUI.UI.Icon) UI component is used to display an icon.
The Icon UI component uses these USS classes to load the appropriate icon.

```xml
<Icon name="icon-name" />
```

Note that by default, all icons are white.
To change the color of an icon, you can use the `--unity-image-tint-color` custom USS property.
For example, to tint the add icon blue, you would use the following code:

```css
.icon-blue {
    --unity-image-tint-color: blue;
}
```

## Light and dark artwork

Most App UI icons are monochrome glyphs. They are authored white and recolored at runtime through
`--unity-image-tint-color`, so one asset serves every theme.

Unity's colored Editor icons are different: their hues carry meaning, so they cannot be tinted, and
they ship a second artwork drawn for dark surfaces. `Icons.uss` declares those icons twice - once
under `.light`, once under `.dark` - and App UI applies the matching marker class to whichever
element provides the theme context. Nothing is needed to opt in: an
[Icon](xref:Unity.AppUI.UI.Icon) picks up the right artwork from the theme it sits in, including
inside a nested [ContextProvider](xref:Unity.AppUI.UI.ContextProvider) that overrides the theme.
See [light and dark contexts](xref:contexts#light-and-dark-contexts) for how the markers work.

Icons that have only one artwork are declared once, unconditionally, so they cost nothing extra.

## Vector Graphics

Icons are Vector Graphics assets. Each `.svg` in `PackageResources/Icons/` is imported by Unity's
built-in `SVGImporter` into a UI Toolkit `VectorImage`, which the
[Icon](xref:Unity.AppUI.UI.Icon) component picks up through the `--unity-image` custom USS
property. This is why icons stay sharp at every [IconSize](xref:Unity.AppUI.UI.IconSize) and on
high-DPI displays, where the previous 64x64 PNGs would soften.

The package declares `com.unity.modules.vectorgraphics` as a dependency. It is a built-in module,
shipped with the Editor since Unity 6, so there is nothing extra to install. If your project also
has the standalone `com.unity.vectorgraphics` package installed, that package's importer takes
over; the icons import the same way either way.

Because the importer has no CSS context, it does not resolve `currentColor`. Monochrome glyphs are
therefore authored white and recolored at runtime by `--unity-image-tint-color`, exactly as
described under [Usage](#usage). An icon authored with `currentColor` imports as empty geometry and
renders as nothing - see [Custom Icons](xref:custom-icons) if you are adding your own.

