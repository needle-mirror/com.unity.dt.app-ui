using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A container component that manages a set of radio buttons, allowing users to select one option from
    /// multiple choices.
    /// </summary>
    /// <remarks>
    /// RadioGroup is a form control that manages a group of <see cref="Radio"/> buttons, ensuring that only one
    /// option can be selected at a time. It's commonly used in forms and settings interfaces where users need to
    /// choose exactly one option from a set of mutually exclusive choices.
    ///
    /// The RadioGroup automatically handles the mutual exclusivity of its Radio children - when one radio button
    /// is selected, all others in the group are automatically deselected.
    ///
    /// **Note:** Each Radio element within the group must have a unique 'key' property to function properly.
    /// </remarks>
    /// <example>
    /// <para>Basic RadioGroup with multiple options — Creating a simple radio group with three options.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <RadioGroup>
    ///     <Radio key="option1" label="Option 1" />
    ///     <Radio key="option2" label="Option 2" />
    ///     <Radio key="option3" label="Option 3" />
    /// </RadioGroup>
    /// ]]></code>
    /// <para>RadioGroup with validation and event handling — Creating a radio group programmatically with validation
    /// and change event handling.</para>
    /// <code lang="csharp"><![CDATA[
    /// var radioGroup = new RadioGroup();
    ///
    /// // Add radio buttons
    /// var radio1 = new Radio { key = "small", label = "Small" };
    /// var radio2 = new Radio { key = "medium", label = "Medium" };
    /// var radio3 = new Radio { key = "large", label = "Large" };
    ///
    /// radioGroup.Add(radio1);
    /// radioGroup.Add(radio2);
    /// radioGroup.Add(radio3);
    ///
    /// // Add validation
    /// radioGroup.validateValue = (value) => value != "large";
    ///
    /// // Listen for changes
    /// radioGroup.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Selected size: {evt.newValue}");
    /// });
    /// ]]></code>
    /// <para>RadioGroup with default selection and styling — Creating a radio group with a pre-selected option and
    /// styled radio buttons.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <RadioGroup value="option2">
    ///     <Radio key="option1" label="Standard" size="M" />
    ///     <Radio key="option2" label="Premium" size="M" emphasized="true" />
    ///     <Radio key="option3" label="Enterprise" size="M" />
    /// </RadioGroup>
    /// ]]></code>
    /// </example>
    [VisualDocPage("inputs")]
    [UxmlElement]
    public partial class RadioGroup : BaseVisualElement, IInputElement<string>
    {

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);


        /// <summary>
        /// The RadioGroup main styling class.
        /// </summary>
        public const string ussClassName = "appui-radiogroup";

        string m_Value = null;

        Func<string, bool> m_ValidateValue;

        readonly Dictionary<string, Radio> m_RadioByKey = new Dictionary<string, Radio>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RadioGroup()
        {
            AddToClassList(ussClassName);
            RegisterCallback<ChangeEvent<bool>>(OnItemChosen);
        }

        /// <summary>
        /// The RadioGroup content container.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// The selected item key.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"> if the value is out of range.</exception>
        [CreateProperty]
        [UxmlAttribute]
        public string value
        {
            get => m_Value;
            set
            {
                if (value == m_Value)
                    return;
                using var evt = ChangeEvent<string>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// The RadioGroup invalid state.
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
        /// The RadioGroup validation function.
        /// </summary>
        [CreateProperty]
        public Func<string, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;

                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
            }
        }

        /// <summary>
        /// Set the value without notifying the listeners.
        /// </summary>
        /// <param name="newValue"> The new value.</param>
        public void SetValueWithoutNotify(string newValue)
        {
            foreach (var radioByKey in m_RadioByKey)
            {
                radioByKey.Value.SetValueWithoutNotify(radioByKey.Key == newValue);
            }

            m_Value = newValue;

            if (validateValue != null)
                invalid = !validateValue.Invoke(newValue);
        }

        void OnItemChosen(ChangeEvent<bool> evt)
        {
            if (evt.target is Radio radio && m_RadioByKey.ContainsKey(radio.key))
            {
                evt.StopPropagation();

                if (evt.newValue)
                    value = radio.key;
            }
        }

        internal void AddRadio(Radio radio)
        {
            if (string.IsNullOrEmpty(radio.key))
                return;

            m_RadioByKey[radio.key] = radio;

            value ??= radio.key;

            radio.SetValueWithoutNotify(radio.key == value);
        }

        internal void RemoveRadio(Radio radio)
        {
            m_RadioByKey.Remove(radio.key);
            if (value == radio.key)
                value = m_RadioByKey.Keys.FirstOrDefault();
        }

    }
}