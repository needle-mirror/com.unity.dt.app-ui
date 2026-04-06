using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// AssetTarget Field UI element.
    /// </summary>
    // todo This has to work with an AssetReferencePicker
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class AssetTargetField : BaseVisualElement, IInputElement<AssetReference>, ISizeableElement, IPressable
    {
#if ENABLE_RUNTIME_DATA_BINDINGS
        internal static readonly BindingId typeProperty = nameof(type);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId clickableProperty = nameof(clickable);
#endif
        const string k_DefaultIconName = "scene";

        /// <summary>
        /// The AssetTargetField main styling class.
        /// </summary>
        public const string ussClassName = "appui-assettargetfield";

        /// <summary>
        /// The AssetTargetField icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The AssetTargetField label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The AssetTargetField type label styling class.
        /// </summary>
        public const string typeLabelUssClassName = ussClassName + "__typelabel";

        /// <summary>
        /// The AssetTargetField size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        readonly Icon m_IconElement;

        readonly LocalizedTextElement m_LabelElement;

        readonly LocalizedTextElement m_TypeLabelElement;

        AssetReference m_AssetReference;

        Size m_Size;

        Type m_Type;

        Pressable m_Clickable;

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
            clickable = new Pressable();

            m_IconElement = new Icon
            {
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

            m_TypeLabelElement = new LocalizedTextElement
            {
                name = typeLabelUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_TypeLabelElement.AddToClassList(typeLabelUssClassName);

            hierarchy.Add(m_IconElement);
            hierarchy.Add(m_LabelElement);
            hierarchy.Add(m_TypeLabelElement);

            size = Size.M;
            type = typeof(GameObject);
            SetValueWithoutNotify(null);
        }

        /// <inheritdoc/>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// Clickable Manipulator for this AssetTargetField.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
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
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in clickableProperty);
#endif
            }
        }

        /// <summary>
        /// The type of the AssetReference that this field accepts.
        /// This is used to filter the assets that can be assigned to this field.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
        public Type type
        {
            get => m_Type;
            set
            {
                var changed = m_Type != value;
                m_Type = value;
                if (m_AssetReference != null && m_Type != null)
                {
                    var valueType = m_AssetReference.GetType();
                    if (!m_Type.IsAssignableFrom(valueType))
                        this.value = null;
                }

                m_IconElement.iconName = m_Type?.Name.ToLower();
                m_TypeLabelElement.text = m_Type?.Name.ToUpper();

#if  ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in typeProperty);
#endif
            }
        }

        /// <summary>
        /// The size of the element.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
#endif
            }
        }

        /// <summary>
        /// Whether the current value of the AssetTargetField is valid or not.
        /// This is determined by the validateValue function.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                var changed = invalid != value;
                EnableInClassList(Styles.invalidUssClassName, value);
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in invalidProperty);
#endif
            }
        }

        /// <summary>
        /// A function that validates the value of the AssetTargetField.
        /// It returns true if the value is valid, false otherwise.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
        public Func<AssetReference, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;
                invalid = !m_ValidateValue?.Invoke(this.value) ?? false;

#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
#endif
            }
        }

        /// <summary>
        /// Sets the value of the AssetTargetField without sending a change event.
        /// </summary>
        /// <param name="newValue"> The new value to set.</param>
        public void SetValueWithoutNotify(AssetReference newValue)
        {
            m_AssetReference = newValue;
            m_LabelElement.text = m_AssetReference?.name ?? "<None>";
            if (validateValue != null) invalid = !validateValue(m_AssetReference);
        }

        /// <summary>
        /// The current value of the AssetTargetField.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
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
#if ENABLE_RUNTIME_DATA_BINDINGS
                NotifyPropertyChanged(in valueProperty);
#endif
            }
        }

#if ENABLE_UXML_TRAITS
        /// <summary>
        /// The UXML factory for the <see cref="AssetTargetField"/>.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<AssetTargetField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="AssetTargetField"/>.
        /// </summary>
        public new class UxmlTraits : BaseVisualElement.UxmlTraits
        {

            readonly UxmlBoolAttributeDescription m_Invalid = new UxmlBoolAttributeDescription
            {
                name = "invalid",
                defaultValue = false
            };

            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var element = (AssetTargetField)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.invalid = m_Invalid.GetValueFromBag(bag, cc);

            }
        }
#endif
    }
}
