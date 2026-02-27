# App UI Reference Documentation

## Components Reference

### Action Components

| Component | Description | UXML |
|-----------|-------------|------|
| Button | Basic button | `<appui:Button title="Text" />` |
| IconButton | Icon-only button | `<appui:IconButton icon="add" />` |
| ActionButton | Toggle button with icon/label | `<appui:ActionButton icon="edit" label="Edit" />` |
| ActionGroup | Group of toggle buttons | `<appui:ActionGroup>...</appui:ActionGroup>` |
| ActionBar | Selection action bar | `<appui:ActionBar>...</appui:ActionBar>` |

### Input Components

| Component | Description | UXML |
|-----------|-------------|------|
| TextField | Single-line text | `<appui:TextField />` |
| TextArea | Multi-line text | `<appui:TextArea />` |
| Checkbox | Boolean checkbox | `<appui:Checkbox />` |
| Toggle | Boolean toggle switch | `<appui:Toggle />` |
| Dropdown | Selection dropdown | `<appui:Dropdown />` |
| RadioGroup | Radio button group | `<appui:RadioGroup />` |
| SliderFloat | Float slider | `<appui:SliderFloat />` |
| SliderInt | Integer slider | `<appui:SliderInt />` |
| TouchSliderFloat | Touch-optimized slider | `<appui:TouchSliderFloat />` |
| ColorField | Color picker field | `<appui:ColorField />` |

### Layout Components

| Component | Description | UXML |
|-----------|-------------|------|
| Panel | Root container with contexts | `<appui:Panel />` |
| StackView | Z-axis stacking with animations | `<appui:StackView />` |
| SwipeView | Horizontal/vertical swiping | `<appui:SwipeView />` |
| PageView | SwipeView + PageIndicator | `<appui:PageView />` |
| SplitView | Resizable pane divider | `<appui:SplitView direction="Horizontal" />` |
| Dialog | Basic dialog | `<appui:Dialog />` |
| AlertDialog | Styled alert dialog | `<appui:AlertDialog />` |

### Typography Components

| Component | Description | UXML |
|-----------|-------------|------|
| Text | General text | `<appui:Text text="Text" />` |
| Heading | Heading text | `<appui:Heading text="Title" />` |
| Icon | Icon display | `<appui:Icon name="info" />` |

## Overlay Layers

| Layer | Priority | Use For |
|-------|----------|---------|
| main-content | Lowest | Default UI content |
| popup | Medium | Modals, dialogs, menus |
| notification | High | Toasts, banners |
| tooltip | Highest | Tooltips |

## Context System

### Available Contexts

| Context | Purpose | Example Values |
|---------|---------|----------------|
| ThemeContext | UI appearance | light, dark, editor-light, editor-dark |
| LangContext | Localization | Locale identifiers |
| ScaleContext | UI size | small, medium, large |
| DirContext | Layout direction | Ltr, Rtl |

### Using Contexts

```csharp
// Provide context
element.ProvideContext(new MyContext(value));

// Consume context
element.RegisterContextChangedCallback<MyContext>(evt => {
    var ctx = evt.context;
});

// Get current context
var ctx = element.GetContext<MyContext>();
```

## MVVM Source Generators

### Attributes

| Attribute | Purpose | Usage |
|-----------|---------|-------|
| `[ObservableObject]` | Makes class observable | On partial class |
| `[ObservableProperty]` | Auto-generates property | On private field |
| `[RelayCommand]` | Auto-generates ICommand | On method |
| `[AlsoNotifyChangeFor]` | Notify dependent properties | On observable property |
| `[Service]` | Property injection | On property/field |

### Example ViewModel

```csharp
[ObservableObject]
public partial class UserViewModel
{
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(FullName))]
    private string _firstName;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(FullName))]
    private string _lastName;

    public string FullName => $"{FirstName} {LastName}";

    [RelayCommand]
    async Task SaveAsync()
    {
        // Save logic
    }
}
```

## Redux State Management

### Action Creator Declaration

```csharp
// Without payload
static readonly ActionCreator Increment = "counter/Increment";

// With payload
static readonly ActionCreator<int> AddAmount = nameof(AddAmount);
```

### Reducer Pattern

```csharp
static CounterState IncrementReducer(CounterState state, IAction action)
{
    return state with { Count = state.Count + 1 };
}

static CounterState AddAmountReducer(CounterState state, IAction<int> action)
{
    return state with { Count = state.Count + action.payload };
}
```

### Async Thunk

```csharp
var fetchUser = new AsyncThunkCreator<int, User>("user/fetch", async (userId, api) => {
    return await userService.GetUserAsync(userId);
});

builder.AddCase(fetchUser.pending, (state, action) =>
    state with { Loading = true });
builder.AddCase(fetchUser.fulfilled, (state, action) =>
    state with { Loading = false, User = action.payload });
builder.AddCase(fetchUser.rejected, (state, action) =>
    state with { Loading = false, Error = action.error });
```

## Navigation System

### NavController Methods

| Method | Description |
|--------|-------------|
| `Navigate(actionName)` | Navigate to destination |
| `PopBackStack()` | Go back |
| `PopBackStack(destinationName, inclusive)` | Pop to specific destination |

### INavVisualController Interface

```csharp
public class MyNavController : INavVisualController
{
    public void SetupAppBar(AppBar appBar, NavDestination dest, NavController nav)
    {
        appBar.title = dest.label;
    }

    public void SetupBottomNavBar(BottomNavBar bar, NavDestination dest, NavController nav)
    {
        // Add navigation items
    }

    public void SetupDrawer(Drawer drawer, NavDestination dest, NavController nav)
    {
        // Configure drawer
    }

    public void SetupNavigationRail(NavigationRail rail, NavDestination dest, NavController nav)
    {
        // Configure rail
    }
}
```

## Input Validation & Events

### Event Types

| Event | When Fired | Use For |
|-------|------------|---------|
| ChangingEvent | During interaction | Live preview, real-time feedback |
| ChangeEvent | After interaction | Final value, saving |

### Validation

```csharp
textField.validateValue = value => !string.IsNullOrEmpty(value);
numericField.validateValue = value => value >= 0 && value <= 100;
```

### Formatting

```csharp
numericField.formatString = "N2";  // 2 decimal places
slider.formatString = "P0";        // Percentage
```

## Platform Integration

### Platform API

| Property/Method | Description |
|-----------------|-------------|
| `Platform.scaleFactor` | Screen scale factor |
| `Platform.darkMode` | System dark mode |
| `Platform.highContrast` | Accessibility high contrast |
| `Platform.reduceMotion` | Accessibility reduce motion |
| `Platform.textScaleFactor` | System text scale |
| `Platform.layoutDirection` | System RTL/LTR |
| `Platform.RunHapticFeedback()` | Trigger haptic (iOS/Android) |

### Clipboard

```csharp
// Get text
byte[] data = Platform.GetPasteboardData(PasteboardType.Text);
string text = Encoding.UTF8.GetString(data);

// Set text
Platform.SetPasteboardData(PasteboardType.Text, Encoding.UTF8.GetBytes("text"));

// Check for data
bool hasText = Platform.HasPasteboardData(PasteboardType.Text);
```

## UXML Source Generators

### UxmlFilePath Attribute

```csharp
[UxmlFilePath("UI/MyElement", UxmlFilePathType.Resources)]
public partial class MyElement : VisualElement
{
    public MyElement()
    {
        UxmlCloneTree();  // Auto-generated
    }
}
```

### UxmlElementName Attribute

```csharp
[UxmlElementName("submitButton")]
public Button SubmitButton { get; private set; }

[UxmlElementName("titleLabel")]
private Label m_TitleLabel;
```

## Custom Components

### Base Classes

| Class | Use For |
|-------|---------|
| BaseVisualElement | Non-text controls |
| BaseTextElement | Text-based controls |
| ExVisualElement | Advanced styling (box-shadow, outline) |
| LocalizedTextElement | Localized text display |

### Helper Classes

| Class | Purpose |
|-------|---------|
| KeyboardFocusController | Distinguish keyboard/pointer focus |
| Pressable | Handle press events from all sources |
| IDismissInvocator | Allow popup content to dismiss itself |

## USS Design Tokens

### Common Variables

```css
/* Spacing */
--appui-spacing-100
--appui-spacing-200

/* Colors */
--appui-primary-100
--appui-primary-200

/* Font weights */
--appui-font-weights-100  /* Regular */
--appui-font-weights-200  /* SemiBold */
```

### Icon USS Pattern

```css
.appui-icon--iconname--variant {
    --unity-image: url("path/to/icon.png");
}
```

Variants: Regular, Bold, Light, DuoTone, Thin, Fill

## Troubleshooting

### Common Issues

1. **Components not visible in UI Builder**: Enable Developer Mode (About Unity > type "internal")
2. **Components look unstyled**: Select App UI theme in UI Builder Theme dropdown
3. **WorldSpaceUIDocument not refreshing**: Enable Clear Color and Clear Depth Stencil in PanelSettings
4. **Addressables error on Play**: Enable "Initialize Synchronously" in Localization settings
5. **macOS plugin error**: Run `xattr -d com.apple.quarantine` on the bundle
