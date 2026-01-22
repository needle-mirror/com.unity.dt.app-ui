---
uid: uxml-source-generators
---

# UXML Source Generators

App UI provides source generators that simplify working with UXML files in your custom UI components.
These generators automatically create boilerplate code to load UXML templates and bind UI elements to your C# properties and fields.

## UxmlFilePathAttribute

The [UxmlFilePathAttribute](xref:Unity.AppUI.UI.UxmlFilePathAttribute) allows you to specify the path to a UXML file
that should be cloned into your custom VisualElement. The source generator creates a `UxmlCloneTree()` method
that loads and clones the UXML template into the element.

### Basic Usage

```csharp
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace MyApp.UI
{
    [UxmlFilePath("UI/MyElement", UxmlFilePathType.Resources)]
    public partial class MyElement : VisualElement
    {
        public MyElement()
        {
            // Call the generated method to clone the UXML tree
            UxmlCloneTree();
        }
    }
}
```

> [!IMPORTANT]
> The class must be marked as `partial` for the source generator to work.

### Path Types

The `UxmlFilePathAttribute` supports two path types via the [UxmlFilePathType](xref:Unity.AppUI.UI.UxmlFilePathType) enum:

| Path Type | Description |
|-----------|-------------|
| `Resources` | Loads the UXML file from a Resources folder using `Resources.Load<VisualTreeAsset>()`. The path should be relative to the Resources folder, without the `.uxml` extension. |
| `AssetDatabase` | In the Unity Editor, loads via `AssetDatabase.LoadAssetAtPath()`. At runtime, falls back to `Resources.Load()`. Use the full asset path including the `.uxml` extension. |

#### Resources Example

Place your UXML file in a Resources folder (e.g., `Assets/Resources/UI/MyElement.uxml`):

```csharp
// Path is relative to Resources folder, without .uxml extension
[UxmlFilePath("UI/MyElement", UxmlFilePathType.Resources)]
public partial class MyElement : VisualElement
{
    public MyElement()
    {
        UxmlCloneTree();
    }
}
```

#### AssetDatabase Example

For editor-only tools or when you need the full asset path:

```csharp
// Full asset path with .uxml extension
[UxmlFilePath("Assets/UI/MyElement.uxml", UxmlFilePathType.AssetDatabase)]
public partial class MyElement : VisualElement
{
    public MyElement()
    {
        UxmlCloneTree();
    }
}
```

## UxmlElementNameAttribute

The [UxmlElementNameAttribute](xref:Unity.AppUI.UI.UxmlElementNameAttribute) allows you to bind
properties or fields to specific elements in your UXML file by their name.
The source generator automatically queries these elements after cloning the UXML tree.

### Basic Usage

Given a UXML file (`MyElement.uxml`):

```xml
<UXML xmlns="UnityEngine.UIElements">
    <Button name="submitButton" text="Submit" />
    <Label name="titleLabel" text="Title" />
    <TextField name="inputField" />
</UXML>
```

You can bind these elements to C# properties or fields:

```csharp
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace MyApp.UI
{
    [UxmlFilePath("UI/MyElement", UxmlFilePathType.Resources)]
    public partial class MyElement : VisualElement
    {
        [UxmlElementName("submitButton")]
        public Button SubmitButton { get; private set; }

        [UxmlElementName("titleLabel")]
        private Label m_TitleLabel;

        [UxmlElementName("inputField")]
        protected TextField m_InputField;

        public MyElement()
        {
            UxmlCloneTree();

            // Elements are now bound and ready to use
            SubmitButton.clicked += OnSubmitClicked;
            m_TitleLabel.text = "Welcome!";
        }

        private void OnSubmitClicked()
        {
            var inputValue = m_InputField.value;
            // Handle submit...
        }
    }
}
```

### Supported Member Types

The `UxmlElementNameAttribute` can be applied to:

- **Properties** with a setter (public, protected, or private)
- **Fields** (public, protected, or private)

The element type must derive from `VisualElement`.

### How It Works

The source generator creates a `UxmlCloneTree()` method that:

1. Loads the UXML template based on the `UxmlFilePathAttribute`
2. Clones the template into the current element using `CloneTree(this)`
3. Queries each decorated member using `this.Q<T>("elementName")` and assigns the result

Generated code example:

```csharp
// Auto-generated code
using global::UnityEngine.UIElements;

namespace MyApp.UI
{
    public partial class MyElement
    {
        [global::System.Runtime.CompilerServices.CompilerGenerated]
        private void UxmlCloneTree()
        {
            var template = global::UnityEngine.Resources.Load<global::UnityEngine.UIElements.VisualTreeAsset>("UI/MyElement");
            if (template)
            {
                template.CloneTree(this);
                SubmitButton = this.Q<global::UnityEngine.UIElements.Button>("submitButton");
                m_TitleLabel = this.Q<global::UnityEngine.UIElements.Label>("titleLabel");
                m_InputField = this.Q<global::UnityEngine.UIElements.TextField>("inputField");
            }
        }
    }
}
```

## Complete Example

Here's a complete example combining both attributes with App UI base classes:

```csharp
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace MyApp.UI
{
    [UxmlFilePath("UI/UserCard", UxmlFilePathType.Resources)]
    public partial class UserCard : BaseVisualElement
    {
        [UxmlElementName("avatar")]
        private Image m_Avatar;

        [UxmlElementName("userName")]
        private Label m_UserName;

        [UxmlElementName("userEmail")]
        private Label m_UserEmail;

        [UxmlElementName("editButton")]
        public Button EditButton { get; private set; }

        [UxmlElementName("deleteButton")]
        public Button DeleteButton { get; private set; }

        public UserCard()
        {
            UxmlCloneTree();
            AddToClassList("user-card");
        }

        public void SetUser(string name, string email, Texture2D avatar)
        {
            m_UserName.text = name;
            m_UserEmail.text = email;
            m_Avatar.image = avatar;
        }
    }
}
```

With the corresponding UXML file (`Assets/Resources/UI/UserCard.uxml`):

```xml
<UXML xmlns="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    <VisualElement class="user-card__container">
        <Image name="avatar" class="user-card__avatar" />
        <VisualElement class="user-card__info">
            <Label name="userName" class="user-card__name" />
            <Label name="userEmail" class="user-card__email" />
        </VisualElement>
        <VisualElement class="user-card__actions">
            <appui:Button name="editButton" title="Edit" />
            <appui:Button name="deleteButton" title="Delete" />
        </VisualElement>
    </VisualElement>
</UXML>
```

## Best Practices

1. **Use meaningful element names**: Choose descriptive names in your UXML that match your C# member names for clarity.

2. **Prefer private fields with public accessors**: Keep your internal elements private and expose only what's needed:
   ```csharp
   [UxmlElementName("titleLabel")]
   private Label m_TitleLabel;

   public string Title
   {
       get => m_TitleLabel.text;
       set => m_TitleLabel.text = value;
   }
   ```

3. **Use Resources for runtime components**: For UI components used at runtime, prefer `UxmlFilePathType.Resources` to ensure the UXML can be loaded in builds.

4. **Null checks**: The generated code assigns `null` if an element is not found. Consider adding null checks if elements might be optional:
   ```csharp
   public MyElement()
   {
       UxmlCloneTree();

       if (SubmitButton != null)
       {
           SubmitButton.clicked += OnSubmitClicked;
       }
   }
   ```
