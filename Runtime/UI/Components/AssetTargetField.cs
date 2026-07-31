using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// AssetTarget Field UI element.
    /// </summary>
    // todo This has to work with an AssetReferencePicker
    [UxmlElement]
    partial class AssetTargetField : BaseVisualElement, IInputElement<AssetReference>, ISizeableElement, IPressable
    {
        internal static readonly BindingId typeProperty = nameof(type);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId clickableProperty = nameof(clickable);
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
        /// The type of the AssetReference that this field accepts.
        /// This is used to filter the assets that can be assigned to this field.
        /// </summary>
        [CreateProperty]
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
            m_LabelElement.text = m_AssetReference?.name ?? "<None>";
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

    }
}
