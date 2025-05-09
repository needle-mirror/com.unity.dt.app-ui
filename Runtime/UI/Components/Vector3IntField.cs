using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Vector3Int Field UI element.
    /// </summary>
    public class Vector3IntField : VisualElement, IValidatableElement<Vector3Int>, ISizeableElement, INotifyValueChanging<Vector3Int>
    {
        /// <summary>
        /// The Vector3Field main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-vector3field";

        /// <summary>
        /// The Vector3Field size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Vector3Field container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The Vector3Field X NumericalField styling class.
        /// </summary>
        public static readonly string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The Vector3Field Y NumericalField styling class.
        /// </summary>
        public static readonly string yFieldUssClassName = ussClassName + "__y-field";

        /// <summary>
        /// The Vector3Field Z NumericalField styling class.
        /// </summary>
        public static readonly string zFieldUssClassName = ussClassName + "__z-field";

        Size m_Size;

        Vector3Int m_Value;

        readonly IntField m_XField;

        readonly IntField m_YField;

        readonly IntField m_ZField;

        Vector3Int m_LastValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Vector3IntField()
        {
            AddToClassList(ussClassName);

            var container = new VisualElement { name = containerUssClassName };
            container.AddToClassList(containerUssClassName);

            m_XField = new IntField { name = xFieldUssClassName, unit = "X" };
            m_XField.AddToClassList(xFieldUssClassName);

            m_YField = new IntField { name = yFieldUssClassName, unit = "Y" };
            m_YField.AddToClassList(yFieldUssClassName);

            m_ZField = new IntField { name = zFieldUssClassName, unit = "Z" };
            m_ZField.AddToClassList(zFieldUssClassName);

            container.Add(m_XField);
            container.Add(m_YField);
            container.Add(m_ZField);

            hierarchy.Add(container);

            size = Size.M;
            SetValueWithoutNotify(Vector3Int.zero);

            m_XField.RegisterValueChangingCallback(OnXFieldChanging);
            m_YField.RegisterValueChangingCallback(OnYFieldChanging);
            m_ZField.RegisterValueChangingCallback(OnZFieldChanging);

            m_XField.RegisterValueChangedCallback(OnXFieldChanged);
            m_YField.RegisterValueChangedCallback(OnYFieldChanged);
            m_ZField.RegisterValueChangedCallback(OnZFieldChanged);
        }

        /// <summary>
        /// The content container of the Vector3IntField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the Vector3IntField.
        /// </summary>
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_XField.size = m_Size;
                m_YField.size = m_Size;
                m_ZField.size = m_Size;
            }
        }

        /// <summary>
        /// Sets the value of the Vector3IntField without notifying any listeners.
        /// </summary>
        /// <param name="newValue"> The new value to set. </param>
        public void SetValueWithoutNotify(Vector3Int newValue)
        {
            m_Value = newValue;
            m_LastValue = m_Value;
            m_XField.SetValueWithoutNotify(m_Value.x);
            m_YField.SetValueWithoutNotify(m_Value.y);
            m_ZField.SetValueWithoutNotify(m_Value.z);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the Vector3IntField.
        /// </summary>
        public Vector3Int value
        {
            get => m_Value;
            set
            {
                if (m_LastValue == m_Value && m_Value == value)
                    return;
                using var evt = ChangeEvent<Vector3Int>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The invalid state of the Vector3IntField.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                EnableInClassList(Styles.invalidUssClassName, value);

                m_XField.EnableInClassList(Styles.invalidUssClassName, value);
                m_YField.EnableInClassList(Styles.invalidUssClassName, value);
                m_ZField.EnableInClassList(Styles.invalidUssClassName, value);
            }
        }

        /// <summary>
        /// The validation function of the Vector3IntField.
        /// </summary>
        public Func<Vector3Int, bool> validateValue { get; set; }
        
        void OnXFieldChanging(ChangingEvent<int> evt)
        {
            
            evt.StopPropagation();
            TrySendChangingEvent(new Vector3Int(evt.newValue, m_Value.y, m_Value.z));
        }
        
        void OnYFieldChanging(ChangingEvent<int> evt)
        {
            
            evt.StopPropagation();
            TrySendChangingEvent(new Vector3Int(m_Value.x, evt.newValue, m_Value.z));
        }
        
        void OnZFieldChanging(ChangingEvent<int> evt)
        {
            
            evt.StopPropagation();
            TrySendChangingEvent(new Vector3Int(m_Value.x, m_Value.y, evt.newValue));
        }
        
        void TrySendChangingEvent(Vector3Int newVector)
        {
            var previousValue = m_Value;
            m_Value = newVector;
            
            if (m_Value != previousValue)
            {
                if (validateValue != null) invalid = !validateValue(m_Value);
                
                using var changeEvent = ChangingEvent<Vector3Int>.GetPooled();
                changeEvent.target = this;
                changeEvent.previousValue = previousValue;
                changeEvent.newValue = m_Value;
                SendEvent(changeEvent);
            }
        }

        void OnZFieldChanged(ChangeEvent<int> evt)
        {
            value = new Vector3Int(value.x, value.y, evt.newValue);
        }

        void OnYFieldChanged(ChangeEvent<int> evt)
        {
            value = new Vector3Int(value.x, evt.newValue, value.z);
        }

        void OnXFieldChanged(ChangeEvent<int> evt)
        {
            value = new Vector3Int(evt.newValue, value.y, value.z);
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
        /// Factory class to instantiate a <see cref="Vector3IntField"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Vector3IntField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Vector3IntField"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
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

                var element = (Vector3IntField)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);

                element.disabled = m_Disabled.GetValueFromBag(bag, cc);
            }
        }
    }
}
