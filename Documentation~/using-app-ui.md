---
uid: using-app-ui
---

# Using App UI

Once you have installed App UI Unity Package, you can start using it in your Unity projects.

App UI Unity Package consists mainly of a set of [UI Toolkit](xref:UIElements) components. 
If you are not familiar with UI Toolkit, we recommend you to read the [UI Toolkit documentation](xref:UIElements)
and the overall [Unity Documentation](https://docs.unity3d.com/Manual/index.html) before using App UI.

## App UI Panel - The root context for all App UI components

App UI components are designed to be used inside a [`ContextProvider`](xref:Unity.AppUI.UI.ContextProvider) component.
The [`ContextProvider`](xref:Unity.AppUI.UI.ContextProvider) component is a [`VisualElement`](xref:UnityEngine.UIElements.VisualElement) that provides an [`ApplicationContext`](xref:Unity.AppUI.Core.ApplicationContext) object to its children. This context is vital for App UI components to be displayed correctly, as they provide information such as the current theme, the current language, etc.

For each [UIDocument](xref:UnityEngine.UIElements.UIDocument) in your project, you will need to have a [`Panel`](xref:Unity.AppUI.UI.Panel) component as the root component of your UI hierarchy. This [`Panel`](xref:Unity.AppUI.UI.Panel) inherits from [`ContextProvider`](xref:Unity.AppUI.UI.ContextProvider) and provides a default [`ApplicationContext`](xref:Unity.AppUI.Core.ApplicationContext) object, based on the current system information, and a layering system to handle popups, notifications, and tooltips.

To know more about Context management, see the [Context documentation page](xref:contexts).

Here is how to use the [`Panel`](xref:Unity.AppUI.UI.Panel) component in a [`UI Document`](xref:UnityEngine.UIElements.UIDocument):

```xml
<UXML xmlns="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    <appui:Panel>
        <!-- Your UI elements -->
    </appui:Panel>
</UXML>
```

## App UI Components

App UI components are defined as [`VisualElement`](xref:UnityEngine.UIElements.VisualElement) in [UI Toolkit](xref:UIElements).
You can find them in the `Unity.AppUI.UI` namespace. 

If you are working directly by editing UXML files, we recommend to define the `appui` namespace inside.

```xml
<UXML xmlns="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    [...]
</UXML>
```

You can also update the UXML Schema definition to get autocompletion in your IDE by selecting **Assets > Update UXML Schema** in the Editor.

Here is an example of usage inside a [`UI Document`](xref:UnityEngine.UIElements.UIDocument):

```xml
<UXML xmlns="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    <appui:Panel>
        <appui:Button title="Hello World!" />
    </appui:Panel>
</UXML>
```

<p align="center">
  <img src="images/app-ui-hello-world.png" alt="App UI Hello World">
</p>
