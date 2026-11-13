using System;
using Unity.AppUI.Core;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Interface for the picker an <see cref="AssetTargetField"/> opens when its pick button is
    /// pressed.
    /// </summary>
    /// <remarks>
    /// The field itself knows nothing about where assets come from, so it does not ship a default
    /// implementation the way <see cref="ColorField"/> does. Assign
    /// <see cref="AssetTargetField.customPicker"/> with something that can enumerate candidates —
    /// an <see cref="ObjectPicker"/> hosted in a <see cref="Popover"/> or a
    /// <see cref="FloatingWindow"/> is the intended host.
    /// </remarks>
    public interface ICustomAssetTargetFieldPicker
    {
        /// <summary>
        /// Show the picker.
        /// </summary>
        /// <param name="owner"> The field that requested the picker. </param>
        /// <param name="onValueChanging">
        /// Callback invoked as the user moves through candidates. It sends a
        /// <see cref="ChangingEvent{T}"/> from the field; the field commits the final value itself.
        /// </param>
        /// <param name="initialValue"> The value assigned to the field when the picker opened. </param>
        /// <param name="type">
        /// The <see cref="AssetTargetField.type"/> filter, or null when the field accepts anything.
        /// </param>
        void Show(AssetTargetField owner, Action<AssetReference> onValueChanging, AssetReference initialValue, Type type);
    }

    /// <summary>
    /// A field that displays a reference to an asset and lets the user reassign or clear it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the UITK counterpart of BOTH the registry's <c>asset-target-field</c> and its
    /// <c>object-field</c>. The two registry components are the same control — <c>object-field</c>
    /// is <c>asset-target-field</c> without the <c>size</c> prop and without the clear button — so
    /// UITK deliberately ships one class rather than a near-duplicate.
    /// </para>
    ///
    /// <para>
    /// The field renders, left to right: an optional leading type icon, the referenced asset's
    /// name (or the empty-state label), a clear button shown only while something is assigned, and
    /// a pick button that opens <see cref="customPicker"/>.
    /// </para>
    ///
    /// <para>Example</para>
    /// <code>
    /// var field = new AssetTargetField
    /// {
    ///     type = typeof(Material),
    ///     value = new AssetReference { name = "Bronze", type = "Material", typeIcon = "sphere" },
    /// };
    /// field.customPicker = myObjectPickerHost;
    /// </code>
    ///
    /// <para>UXML</para>
    /// <code>
    /// &lt;appui:AssetTargetField size="Md" show-clear-button="true" empty-label="None" /&gt;
    /// </code>
    /// </remarks>
    [UxmlElement]
    public partial class AssetTargetField : BaseVisualElement, IInputElement<AssetReference>, ISizeableElement, IPressable
    {
        internal static readonly BindingId typeProperty = nameof(type);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId clickableProperty = nameof(clickable);

        internal static readonly BindingId showClearButtonProperty = nameof(showClearButton);

        internal static readonly BindingId emptyLabelProperty = nameof(emptyLabel);

        internal static readonly BindingId customPickerProperty = nameof(customPicker);

        const string k_DefaultIconName = "cube";

        const string k_PickIconName = "crosshair";

        const string k_ClearIconName = "x";

        /// <summary>
        /// The AssetTargetField main styling class.
        /// </summary>
        public const string ussClassName = "appui-assettargetfield";

        /// <summary>
        /// The AssetTargetField leading icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The AssetTargetField label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The AssetTargetField clear button styling class.
        /// </summary>
        public const string clearButtonUssClassName = ussClassName + "__clear-button";

        /// <summary>
        /// The AssetTargetField pick button styling class.
        /// </summary>
        public const string pickButtonUssClassName = ussClassName + "__pick-button";

        /// <summary>
        /// The AssetTargetField size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        readonly Icon m_IconElement;

        readonly LocalizedTextElement m_LabelElement;

        readonly IconButton m_ClearButton;

        readonly IconButton m_PickButton;

        AssetReference m_AssetReference;

        Size m_Size;

        Type m_Type;

        Pressable m_Clickable;

        bool m_ShowClearButton;

        string m_EmptyLabel;

        ICustomAssetTargetFieldPicker m_CustomPicker;

        Func<AssetReference, bool> m_ValidateValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AssetTargetField()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;
            clickable = new Pressable(OpenPicker);

            m_IconElement = new Icon
            {
                primary = true,
                name = iconUssClassName,
                pickingMode = PickingMode.Ignore,
                iconName = k_DefaultIconName
            };
            m_IconElement.AddToClassList(iconUssClassName);

            m_LabelElement = new LocalizedTextElement
            {
                name = labelUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_LabelElement.AddToClassList(labelUssClassName);

            m_ClearButton = new IconButton(k_ClearIconName, ClearValue)
            {
                name = clearButtonUssClassName,
                quiet = true,
                size = Size.Sm,
            };
            m_ClearButton.AddToClassList(clearButtonUssClassName);

            m_PickButton = new IconButton(k_PickIconName, OpenPicker)
            {
                name = pickButtonUssClassName,
                quiet = true,
                size = Size.Sm,
            };
            m_PickButton.AddToClassList(pickButtonUssClassName);

            hierarchy.Add(m_IconElement);
            hierarchy.Add(m_LabelElement);
            hierarchy.Add(m_ClearButton);
            hierarchy.Add(m_PickButton);

            size = Size.Md;
            emptyLabel = "None";
            showClearButton = true;
            type = null;
            SetValueWithoutNotify(null);
        }

        /// <inheritdoc/>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The leading type icon.
        /// </summary>
        public Icon iconElement => m_IconElement;

        /// <summary>
        /// The button that clears the reference. Only visible while a value is assigned and
        /// <see cref="showClearButton"/> is true.
        /// </summary>
        public IconButton clearButton => m_ClearButton;

        /// <summary>
        /// The button that opens <see cref="customPicker"/>.
        /// </summary>
        public IconButton pickButton => m_PickButton;

        /// <summary>
        /// Clickable Manipulator for this AssetTargetField.
        /// </summary>
        [CreateProperty]
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                var changed = m_Clickable != value;
                if (m_Clickable != null && m_Clickable.target == this)
                    this.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
                if (changed)
                    NotifyPropertyChanged(in clickableProperty);
            }
        }

        /// <summary>
        /// The picker opened when the field, or its pick button, is pressed.
        /// </summary>
        /// <remarks>
        /// Null by default — the field has no way to enumerate assets on its own. Pressing the
        /// pick button with no picker assigned raises <see cref="pickRequested"/> and does nothing
        /// else, which is what lets the field ship before a picker host exists.
        /// </remarks>
        [CreateProperty]
        public ICustomAssetTargetFieldPicker customPicker
        {
            get => m_CustomPicker;
            set
            {
                var changed = m_CustomPicker != value;
                m_CustomPicker = value;
                if (changed)
                    NotifyPropertyChanged(in customPickerProperty);
            }
        }

        /// <summary>
        /// Raised when the user asks for the picker, whether or not a <see cref="customPicker"/>
        /// is assigned. The registry's <c>onPick</c> callback.
        /// </summary>
        public event Action pickRequested;

        /// <summary>
        /// Raised when the user clears the reference with the clear button. The registry's
        /// <c>onClear</c> callback. The value change itself is also reported through the usual
        /// <see cref="ChangeEvent{T}"/>.
        /// </summary>
        public event Action cleared;

        /// <summary>
        /// The label shown when no asset is assigned.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>None</c>, matching the registry. It is fed through
        /// <see cref="LocalizedTextElement"/>, so it can be a localization key.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string emptyLabel
        {
            get => m_EmptyLabel;
            set
            {
                var changed = m_EmptyLabel != value;
                m_EmptyLabel = value;
                RefreshLabel();
                if (changed)
                    NotifyPropertyChanged(in emptyLabelProperty);
            }
        }

        /// <summary>
        /// Whether the clear button is offered at all. When true the button still only shows while
        /// a value is assigned, because there is nothing to clear otherwise.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool showClearButton
        {
            get => m_ShowClearButton;
            set
            {
                var changed = m_ShowClearButton != value;
                m_ShowClearButton = value;
                RefreshClearButton();
                if (changed)
                    NotifyPropertyChanged(in showClearButtonProperty);
            }
        }

        /// <summary>
        /// The type of the AssetReference that this field accepts.
        /// This is used to filter the assets that can be assigned to this field, and it drives the
        /// leading icon when the assigned value carries no <see cref="AssetReference.typeIcon"/>.
        /// </summary>
        [CreateProperty]
        public Type type
        {
            get => m_Type;
            set
            {
                var changed = m_Type != value;
                m_Type = value;
                RefreshIcon();

                if (changed)
                    NotifyPropertyChanged(in typeProperty);
            }
        }

        /// <summary>
        /// The size of the element.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));
                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// Whether the current value of the AssetTargetField is valid or not.
        /// This is determined by the validateValue function.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                var changed = invalid != value;
                EnableInClassList(Styles.invalidUssClassName, value);
                if (changed)
                    NotifyPropertyChanged(in invalidProperty);
            }
        }

        /// <summary>
        /// A function that validates the value of the AssetTargetField.
        /// It returns true if the value is valid, false otherwise.
        /// </summary>
        [CreateProperty]
        public Func<AssetReference, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;
                invalid = !m_ValidateValue?.Invoke(this.value) ?? false;

                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
            }
        }

        /// <summary>
        /// Sets the value of the AssetTargetField without sending a change event.
        /// </summary>
        /// <param name="newValue"> The new value to set.</param>
        public void SetValueWithoutNotify(AssetReference newValue)
        {
            m_AssetReference = newValue;
            RefreshLabel();
            RefreshIcon();
            RefreshClearButton();
            if (validateValue != null) invalid = !validateValue(m_AssetReference);
        }

        /// <summary>
        /// The current value of the AssetTargetField.
        /// </summary>
        [CreateProperty]
        public AssetReference value
        {
            get => m_AssetReference;
            set
            {
                if (m_AssetReference == value)
                    return;
                using var evt = ChangeEvent<AssetReference>.GetPooled(m_AssetReference, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// Clears the assigned reference, as the clear button does.
        /// </summary>
        /// <remarks>
        /// Named <c>ClearValue</c> rather than <c>Clear</c> because
        /// <see cref="VisualElement.Clear"/> removes every child, so a field held in a
        /// <see cref="VisualElement"/> variable would tear out its own icon, label and
        /// buttons instead of dropping the reference.
        /// </remarks>
        public void ClearValue()
        {
            if (m_AssetReference == null)
                return;
            value = null;
            cleared?.Invoke();
        }

        /// <summary>
        /// Opens <see cref="customPicker"/>, as the pick button does.
        /// </summary>
        public void OpenPicker()
        {
            pickRequested?.Invoke();
            customPicker?.Show(this, OnPickerValueChanging, m_AssetReference, m_Type);
        }

        void OnPickerValueChanging(AssetReference newValue)
        {
            if (m_AssetReference == newValue)
                return;

            var previous = m_AssetReference;
            SetValueWithoutNotify(newValue);
            using var evt = ChangingEvent<AssetReference>.GetPooled();
            evt.previousValue = previous;
            evt.newValue = newValue;
            evt.target = this;
            SendEvent(evt);
        }

        void RefreshLabel()
        {
            m_LabelElement.text = m_AssetReference?.name ?? m_EmptyLabel;
        }

        void RefreshIcon()
        {
            // The value's own icon wins, then the accepted type's name, then the generic default.
            // Never leave iconName null: Icon renders nothing and the label jumps left.
            var iconName = m_AssetReference?.typeIcon;
            if (string.IsNullOrEmpty(iconName))
                iconName = m_Type?.Name.ToLower();
            if (string.IsNullOrEmpty(iconName))
                iconName = k_DefaultIconName;
            m_IconElement.iconName = iconName;
        }

        void RefreshClearButton()
        {
            m_ClearButton.EnableInClassList(Styles.hiddenUssClassName,
                !m_ShowClearButton || m_AssetReference == null);
        }
    }
}
