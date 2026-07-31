using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A UI component that allows users to input and edit integer bounds values with position and size in 3D space.
    /// </summary>
    /// <remarks>
    /// The BoundsIntField is a specialized input component designed for editing BoundsInt values in Unity
    /// applications. It provides a user-friendly interface for manipulating 3D bounds defined by integer
    /// coordinates, consisting of a position (x, y, z) and size (width, height, depth).
    ///
    /// The component is organized into two rows: one for position coordinates and another for size dimensions.
    /// Each row contains three integer input fields for the respective x, y, and z values.
    ///
    /// The BoundsIntField is particularly useful when working with grid-based systems, tile maps, or any scenario
    /// requiring precise integer-based 3D boundaries.
    /// </remarks>
    /// <example>
    /// <para>Basic usage example showing how to create and configure a BoundsIntField.</para>
    ///
    /// <para>Creating a BoundsIntField with validation and change callback.</para>
    /// <code lang="csharp"><![CDATA[
    /// var boundsField = new BoundsIntField();
    /// boundsField.value = new BoundsInt(Vector3Int.zero, new Vector3Int(1, 1, 1));
    /// boundsField.validateValue = (bounds) => bounds.size.x > 0 && bounds.size.y > 0 && bounds.size.z > 0;
    ///
    /// // Register to value changes
    /// boundsField.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"New bounds: Pos({evt.newValue.position}), Size({evt.newValue.size})");
    /// });
    /// ]]></code>
    /// <para>UXML definition example.</para>
    ///
    /// <para>Defining a BoundsIntField in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:appui="Unity.AppUI.UI">
    ///     <appui:BoundsIntField
    ///         size="M"
    ///         value="0,0,0,1,1,1"
    ///         name="myBoundsField"
    ///     />
    /// </UXML>
    /// ]]></code>
    /// <para>USS styling example.</para>
    ///
    /// <para>Styling the BoundsIntField with USS.</para>
    /// <code lang="csharp"><![CDATA[
    /// .appui-boundsfield {
    ///     margin: 8px;
    /// }
    ///
    /// .appui-boundsfield__row {
    ///     flex-direction: row;
    ///     align-items: center;
    /// }
    ///
    /// .appui-boundsfield__label {
    ///     min-width: 60px;
    /// }
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class BoundsIntField : BaseVisualElement, IInputElement<BoundsInt>, ISizeableElement, INotifyValueChanging<BoundsInt>
    {
        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);
        /// <summary>
        /// The BoundsIntField main styling class.
        /// </summary>
        public const string ussClassName = "appui-boundsfield";

        /// <summary>
        /// The BoundsIntField row styling class.
        /// </summary>
        public const string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The BoundsIntField size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The BoundsIntField X NumericalField styling class.
        /// </summary>
        public const string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The BoundsIntField Y NumericalField styling class.
        /// </summary>
        public const string yFieldUssClassName = ussClassName + "__y-field";

        /// <summary>
        /// The BoundsIntField Z NumericalField styling class.
        /// </summary>
        public const string zFieldUssClassName = ussClassName + "__z-field";

        /// <summary>
        /// The BoundsIntField X NumericalField styling class.
        /// </summary>
        public const string sxFieldUssClassName = ussClassName + "__sx-field";

        /// <summary>
        /// The BoundsIntField Y NumericalField styling class.
        /// </summary>
        public const string syFieldUssClassName = ussClassName + "__sy-field";

        /// <summary>
        /// The BoundsIntField Z NumericalField styling class.
        /// </summary>
        public const string szFieldUssClassName = ussClassName + "__sz-field";

        /// <summary>
        /// The BoundsIntField Label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        Size m_Size;

        BoundsInt m_LastValue;

        BoundsInt m_Value;

        Func<BoundsInt, bool> m_ValidateValue;

        readonly IntField m_CXField;

        readonly IntField m_CYField;

        readonly IntField m_CZField;

        readonly IntField m_SXField;

        readonly IntField m_SYField;

        readonly IntField m_SZField;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BoundsIntField()
        {
            AddToClassList(ussClassName);

            var cxFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_CXField = new IntField { name = xFieldUssClassName, unit = "X" };
            cxFieldContainer.AddToClassList(xFieldUssClassName);
            cxFieldContainer.Add(m_CXField);

            var cyFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_CYField = new IntField { name = yFieldUssClassName, unit = "Y" };
            cyFieldContainer.AddToClassList(yFieldUssClassName);
            cyFieldContainer.Add(m_CYField);

            var czFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_CZField = new IntField { name = zFieldUssClassName, unit = "Z" };
            czFieldContainer.AddToClassList(zFieldUssClassName);
            czFieldContainer.Add(m_CZField);

            var sxFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_SXField = new IntField { name = sxFieldUssClassName, unit = "X" };
            sxFieldContainer.AddToClassList(sxFieldUssClassName);
            sxFieldContainer.Add(m_SXField);

            var syFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_SYField = new IntField { name = syFieldUssClassName, unit = "Y" };
            syFieldContainer.AddToClassList(syFieldUssClassName);
            syFieldContainer.Add(m_SYField);

            var szFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_SZField = new IntField { name = szFieldUssClassName, unit = "Z" };
            szFieldContainer.AddToClassList(szFieldUssClassName);
            szFieldContainer.Add(m_SZField);

            var centerLabel = new Text("Position") { size = TextSize.S, pickingMode = PickingMode.Ignore };
            centerLabel.AddToClassList(labelUssClassName);
            var sizeLabel = new Text("Size") { size = TextSize.S, pickingMode = PickingMode.Ignore };
            sizeLabel.AddToClassList(labelUssClassName);

            var centerRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            centerRow.AddToClassList(rowUssClassName);
            centerRow.Add(centerLabel);
            centerRow.Add(cxFieldContainer);
            centerRow.Add(cyFieldContainer);
            centerRow.Add(czFieldContainer);

            var sizeRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            sizeRow.AddToClassList(rowUssClassName);
            sizeRow.Add(sizeLabel);
            sizeRow.Add(sxFieldContainer);
            sizeRow.Add(syFieldContainer);
            sizeRow.Add(szFieldContainer);

            hierarchy.Add(centerRow);
            hierarchy.Add(sizeRow);

            size = Size.M;
            SetValueWithoutNotify(new BoundsInt());

            m_CXField.RegisterValueChangedCallback(OnCXFieldChanged);
            m_CYField.RegisterValueChangedCallback(OnCYFieldChanged);
            m_CZField.RegisterValueChangedCallback(OnCZFieldChanged);
            m_SXField.RegisterValueChangedCallback(OnSXFieldChanged);
            m_SYField.RegisterValueChangedCallback(OnSYFieldChanged);
            m_SZField.RegisterValueChangedCallback(OnSZFieldChanged);

            m_CXField.RegisterValueChangingCallback(OnCXFieldChanging);
            m_CYField.RegisterValueChangingCallback(OnCYFieldChanging);
            m_CZField.RegisterValueChangingCallback(OnCZFieldChanging);
            m_SXField.RegisterValueChangingCallback(OnSXFieldChanging);
            m_SYField.RegisterValueChangingCallback(OnSYFieldChanging);
            m_SZField.RegisterValueChangingCallback(OnSZFieldChanging);
        }

        /// <summary>
        /// The content container of the BoundsIntField. Always null.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The BoundsIntField size.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        [Header("BoundsInt Field")]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));
                m_CXField.size = m_Size;
                m_CYField.size = m_Size;
                m_CZField.size = m_Size;
                m_SXField.size = m_Size;
                m_SYField.size = m_Size;
                m_SZField.size = m_Size;

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// Sets the BoundsIntField value without notifying any change event listeners.
        /// </summary>
        /// <param name="newValue"> The new value to set. </param>
        public void SetValueWithoutNotify(BoundsInt newValue)
        {
            m_Value = newValue;
            m_CXField.SetValueWithoutNotify(m_Value.position.x);
            m_CYField.SetValueWithoutNotify(m_Value.position.y);
            m_CZField.SetValueWithoutNotify(m_Value.position.z);
            m_SXField.SetValueWithoutNotify(m_Value.size.x);
            m_SYField.SetValueWithoutNotify(m_Value.size.y);
            m_SZField.SetValueWithoutNotify(m_Value.size.z);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The BoundsIntField value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public BoundsInt value
        {
            get => m_Value;
            set
            {
                if (m_LastValue == m_Value && m_Value == value)
                    return;

                var previousValue = m_LastValue;
                SetValueWithoutNotify(value);
                using var evt = ChangeEvent<BoundsInt>.GetPooled(previousValue, m_Value);
                evt.target = this;
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// Set the validation state of the BoundsIntField.
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

                m_CXField.EnableInClassList(Styles.invalidUssClassName, value);
                m_CYField.EnableInClassList(Styles.invalidUssClassName, value);
                m_CZField.EnableInClassList(Styles.invalidUssClassName, value);
                m_SXField.EnableInClassList(Styles.invalidUssClassName, value);
                m_SYField.EnableInClassList(Styles.invalidUssClassName, value);
                m_SZField.EnableInClassList(Styles.invalidUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in invalidProperty);
            }
        }

        /// <summary>
        /// The validation function to use on the BoundsIntField value.
        /// </summary>
        [CreateProperty]
        public Func<BoundsInt, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;
                if (validateValue != null)
                    invalid = !validateValue(m_Value);

                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
            }
        }

        void OnCZFieldChanged(ChangeEvent<int> evt)
        {
            value = new BoundsInt(new Vector3Int(value.position.x, value.position.y, evt.newValue), value.size);
        }

        void OnCYFieldChanged(ChangeEvent<int> evt)
        {
            value = new BoundsInt(new Vector3Int(value.position.x, evt.newValue, value.position.z), value.size);
        }

        void OnCXFieldChanged(ChangeEvent<int> evt)
        {
            value = new BoundsInt(new Vector3Int(evt.newValue, value.position.y, value.position.z), value.size);
        }

        void OnSXFieldChanged(ChangeEvent<int> evt)
        {
            value = new BoundsInt(value.position, new Vector3Int(evt.newValue, value.size.y, value.size.z));
        }

        void OnSYFieldChanged(ChangeEvent<int> evt)
        {
            value = new BoundsInt(value.position, new Vector3Int(value.size.x, evt.newValue, value.size.z));
        }

        void OnSZFieldChanged(ChangeEvent<int> evt)
        {
            value = new BoundsInt(value.position, new Vector3Int(value.size.x, value.size.y, evt.newValue));
        }

        void OnCZFieldChanging(ChangingEvent<int> evt)
        {
            evt.StopPropagation();
            TrySendChangingEvent(new BoundsInt(new Vector3Int(value.position.x, value.position.y, evt.newValue), value.size));
        }

        void OnCYFieldChanging(ChangingEvent<int> evt)
        {
            evt.StopPropagation();
            TrySendChangingEvent(new BoundsInt(new Vector3Int(value.position.x, evt.newValue, value.position.z), value.size));
        }

        void OnCXFieldChanging(ChangingEvent<int> evt)
        {
            evt.StopPropagation();
            TrySendChangingEvent(new BoundsInt(new Vector3Int(evt.newValue, value.position.y, value.position.z), value.size));
        }

        void OnSXFieldChanging(ChangingEvent<int> evt)
        {
            evt.StopPropagation();
            TrySendChangingEvent(new BoundsInt(value.position, new Vector3Int(evt.newValue, value.size.y, value.size.z)));
        }

        void OnSYFieldChanging(ChangingEvent<int> evt)
        {
            evt.StopPropagation();
            TrySendChangingEvent(new BoundsInt(value.position, new Vector3Int(value.size.x, evt.newValue, value.size.z)));
        }

        void OnSZFieldChanging(ChangingEvent<int> evt)
        {
            evt.StopPropagation();
            TrySendChangingEvent(new BoundsInt(value.position, new Vector3Int(value.size.x, value.size.y, evt.newValue)));
        }

        void TrySendChangingEvent(BoundsInt newVal)
        {
            var previousValue = m_Value;
            m_Value = newVal;

            if (m_Value != previousValue)
            {
                if (validateValue != null) invalid = !validateValue(m_Value);

                using var changeEvent = ChangingEvent<BoundsInt>.GetPooled();
                changeEvent.target = this;
                changeEvent.previousValue = previousValue;
                changeEvent.newValue = m_Value;
                SendEvent(changeEvent);
            }
        }

    }
}
