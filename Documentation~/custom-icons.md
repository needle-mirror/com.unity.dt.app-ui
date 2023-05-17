---
uid: custom-icons
---

# Custom Icons

Every icon provided by App UI is available as a PNG file.
They are all referenced inside USS files, which are used to load the icons.

## Adding Custom Icons

To add a custom icon, you need to add a PNG file to your project and reference it in a USS file.

The naming convention used for the USS class name is `appui-icon--<name>--<variant in lowercase>`.

The variant by default is `Regular`, but you can use the following variants:

- `Regular`
- `Bold`
- `Light`
- `DuoTone`
- `Thin`
- `Fill`

Here's an example of how to add a custom icon named `home` with the `Regular` variant:

```css
.appui-icon--home--regular {
    --unity-image: url("path/to/home.png");
}
```

> [!IMPORTANT]
> Your USS class name must start with `appui-icon--` followed by the name of your icon
> in order to work with the [Icon](xref:Unity.AppUI.UI.Icon) UI component.

## Using Phosphor Icons

Phosphor Icons is a set of over 2,000 open-source icons, designed for the modern web.
Each icon is designed on a 24x24 grid with an emphasis on simplicity, consistency, and flexibility.

To use Phosphor Icons, you need to add the `com.unity.replica.phosphor-icons` package to your project.

Then, you can reference the icons in your USS files.

Here's an example of how to use the `horse` icon:

```css
.appui-icon--horse--regular {
    --unity-image: url("/Packages/com.unity.replica.phosphor-icons/PackageResources/Icons/regular/horse.png");
}
```

Then you can use the `Icon` UI component to display the icon:

```xml
<appui:Icon name="horse" variant="Regular" />
```
