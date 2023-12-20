using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The possible states for a <see cref="Checkbox"/>.
    /// </summary>
    public enum CheckboxState
    {
        /// <summary>
        /// The <see cref="Checkbox"/> is completely unchecked.
        /// </summary>
        Unchecked,

        /// <summary>
        /// The <see cref="Checkbox"/> is unchecked but at least one of its dependencies is checked.
        /// </summary>
        Intermediate,

        /// <summary>
        ///
        /// </summary>
        Checked
    }

    /// <summary>
    /// Checkbox UI element.
    /// </summary>
    public class Checkbox : VisualElement, IValidatableElement<CheckboxState>, ISizeableElement, IPressable
    {
        /// <summary>
        /// The Checkbox main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-checkbox";
        
        /// <summary>
        /// The Checkbox size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Checkbox emphasized mode styling class.
        /// </summary>
        public static readonly string emphasizedUssClassName = ussClassName + "--emphasized";

        /// <summary>
        /// The Checkbox box styling class.
        /// </summary>
        public static readonly string boxUssClassName = ussClassName + "__box";

        /// <summary>
        /// The Checkbox checkmark styling class.
        /// </summary>
        public static readonly string checkmarkUssClassName = ussClassName + "__checkmark";

        /// <summary>
        /// The Checkbox partial checkmark styling class.
        /// </summary>
        public static readonly string partialCheckmarkUssClassName = ussClassName + "__partialcheckmark";

        /// <summary>
        /// The Checkbox label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        readonly LocalizedTextElement m_Label;

        CheckboxState m_Value;

        Pressable m_Clickable;

        readonly ExVisualElement m_Box;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Checkbox()
        {
            AddToClassList(ussClassName);

            clickable = new Pressable(OnClicked);
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            var checkmark = new Icon { name = checkmarkUssClassName, iconName = "check", variant = IconVariant.Bold, pickingMode = PickingMode.Ignore };
            checkmark.AddToClassList(checkmarkUssClassName);
            var partialCheckmark = new Icon { name = partialCheckmarkUssClassName, iconName = "minus", variant = IconVariant.Bold, pickingMode = PickingMode.Ignore };
            partialCheckmark.AddToClassList(partialCheckmarkUssClassName);
            m_Box = new ExVisualElement { name = boxUssClassName, pickingMode = PickingMode.Ignore, passMask = 0 };
            m_Box.AddToClassList(boxUssClassName);
            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);

            m_Box.hierarchy.Add(checkmark);
            m_Box.hierarchy.Add(partialCheckmark);
            hierarchy.Add(m_Box);
            hierarchy.Add(m_Label);

            emphasized = false;
            invalid = false;
            label = null;
            SetValueWithoutNotify(CheckboxState.Unchecked);

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocus, OnPointerFocus));
        }

        /// <summary>
        /// Clickable Manipulator for this Checkbox.
        /// </summary>
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                if (m_Clickable != null && m_Clickable.target == this)
                    this.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
            }
        }

        /// <summary>
        /// The Checkbox emphasized mode.
        /// </summary>
        public bool emphasized
        {
            get => ClassListContains(emphasizedUssClassName);
            set => EnableInClassList(emphasizedUssClassName, value);
        }

        /// <summary>
        /// The text displayed in the Checkbox label.
        /// </summary>
        public string label
        {
            get => m_Label.text;
            set
            {
                m_Label.text = value;
                m_Label.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
            }
        }
        
        /// <summary>
        /// The Checkbox size
        /// </summary>
        public Size size { get; set; }

        /// <summary>
        /// The Checkbox invalid state.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set => EnableInClassList(Styles.invalidUssClassName, value);
        }

        /// <summary>
        /// The Checkbox validation function.
        /// </summary>
        public Func<CheckboxState, bool> validateValue { get; set; }

        /// <summary>
        /// Set the Checkbox value without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new Checkbox value. </param>
        public void SetValueWithoutNotify(CheckboxState newValue)
        {
            m_Value = newValue;
            EnableInClassList(Styles.checkedUssClassName, m_Value == CheckboxState.Checked);
            EnableInClassList(Styles.intermediateUssClassName, m_Value == CheckboxState.Intermediate);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The Checkbox value.
        /// </summary>
        public CheckboxState value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<CheckboxState>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        void OnClicked()
        {
            // when a user click on the UI element, it can't go into an intermediate state
            // intermediate state can be set programatically.
            value = value == CheckboxState.Checked ? CheckboxState.Unchecked : CheckboxState.Checked;
        }

        void OnPointerFocus(FocusInEvent evt)
        {
            m_Box.passMask = 0;
        }

        void OnKeyboardFocus(FocusInEvent evt)
        {
            m_Box.passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Outline;
        }
        
        /// <summary>
        /// Whether the element is disabled.
        /// </summary>
        public bool disabled
        {
            get => !enabledSelf;
            set => SetEnabled(!value);
        }

        /// <summary>
        /// UXML factory for the Checkbox.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Checkbox, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Checkbox"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_Emphasized = new UxmlBoolAttributeDescription
            {
                name = "emphasized",
                defaultValue = false
            };

            readonly UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
            {
                name = "label",
                defaultValue = null
            };

            readonly UxmlEnumAttributeDescription<CheckboxState> m_Value = new UxmlEnumAttributeDescription<CheckboxState>
            {
                name = "value",
                defaultValue = CheckboxState.Unchecked
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

                var element = (Checkbox)ve;
                element.emphasized = m_Emphasized.GetValueFromBag(bag, cc);
                element.value = m_Value.GetValueFromBag(bag, cc);
                element.label = m_Label.GetValueFromBag(bag, cc);
                element.disabled = m_Disabled.GetValueFromBag(bag, cc);
            }
        }
    }
}
