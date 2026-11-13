using System;
using Unity.AppUI.Core;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A labelled numeric property row: a scrubbable label paired with a numeric field, both
    /// driven by a single value.
    /// </summary>
    /// <remarks>
    /// NumericProperty is the inspector row of the design system. It derives from
    /// <see cref="InputLabel"/>, so it inherits the whole label/input layout — direction, required
    /// indicator, help text, leading <see cref="InputLabel.icon"/> and
    /// <see cref="InputLabel.capValue"/> — and it owns the <see cref="NumericalField{TValue}"/>
    /// placed inside it.
    ///
    /// The point of the type is that the label drag-scrub cannot be forgotten:
    /// <see cref="InputLabel.draggable"/> defaults to <c>true</c> here, so dragging the label
    /// horizontally always adjusts the field (Shift for a coarse step, Alt for a fine one). Build
    /// the same row by hand out of an InputLabel and a field and you can ship one whose label
    /// silently does nothing.
    ///
    /// Scalar only. Vector, rect and bounds rows scrub per-axis on their own sub-field labels, so
    /// pair those with a plain <see cref="InputLabel"/>.
    ///
    /// Use the concrete <see cref="NumericPropertyFloat"/> and <see cref="NumericPropertyInt"/>.
    /// </remarks>
    /// <typeparam name="TValue"> The numeric value type. </typeparam>
    /// <typeparam name="TField"> The <see cref="NumericalField{TValue}"/> implementation used as
    /// the value editor. </typeparam>
    public abstract class NumericProperty<TValue, TField> : InputLabel, IInputElement<TValue>, ISizeableElement
        where TValue : struct, IComparable, IComparable<TValue>, IFormattable
        where TField : NumericalField<TValue>, new()
    {
        internal static readonly BindingId valueProperty = new BindingId(nameof(value));

        internal static readonly BindingId sizeProperty = new BindingId(nameof(size));

        internal static readonly BindingId unitProperty = new BindingId(nameof(unit));

        internal static readonly BindingId lowValueProperty = new BindingId(nameof(lowValue));

        internal static readonly BindingId highValueProperty = new BindingId(nameof(highValue));

        internal static readonly BindingId formatStringProperty = new BindingId(nameof(formatString));

        /// <summary>
        /// The NumericProperty main styling class.
        /// </summary>
        public new const string ussClassName = "appui-numericproperty";

        /// <summary>
        /// The NumericProperty field styling class.
        /// </summary>
        public const string fieldUssClassName = ussClassName + "__field";

        readonly TField m_Field;

        /// <summary>
        /// The numeric field used as the value editor.
        /// </summary>
        public TField field => m_Field;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected NumericProperty()
        {
            AddToClassList(ussClassName);

            m_Field = new TField { name = fieldUssClassName };
            m_Field.AddToClassList(fieldUssClassName);
            Add(m_Field);

            // The whole reason this type exists: the scrub handle is always wired. Both halves are
            // needed and both are easy to forget by hand — InputLabel.draggable publishes the drag
            // as a DragContext, and NumericalField.acceptDragging (off by default) is what makes
            // the field subscribe to it.
            draggable = true;
            m_Field.acceptDragging = true;

            m_Field.RegisterValueChangedCallback(OnFieldValueChanged);
        }

        void OnFieldValueChanged(ChangeEvent<TValue> evt)
        {
            // The field is an implementation detail; re-target the notification at the row so
            // consumers bind to the NumericProperty and never to its inner field.
            evt.StopPropagation();
            using var rowEvt = ChangeEvent<TValue>.GetPooled(evt.previousValue, evt.newValue);
            rowEvt.target = this;
            SendEvent(rowEvt);

            NotifyPropertyChanged(in valueProperty);
        }

        /// <summary>
        /// The value of the property.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public TValue value
        {
            get => m_Field.value;
            set => m_Field.value = value;
        }

        /// <summary>
        /// Set the value without notifying the listeners.
        /// </summary>
        /// <param name="newValue"> The new value. </param>
        public void SetValueWithoutNotify(TValue newValue)
        {
            m_Field.SetValueWithoutNotify(newValue);
        }

        /// <summary>
        /// The invalid state of the property.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool invalid
        {
            get => m_Field.invalid;
            set => m_Field.invalid = value;
        }

        /// <summary>
        /// The validation function applied to the value.
        /// </summary>
        [CreateProperty]
        public Func<TValue, bool> validateValue
        {
            get => m_Field.validateValue;
            set => m_Field.validateValue = value;
        }

        /// <summary>
        /// The size of the numeric field.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Field.size;
            set
            {
                var changed = m_Field.size != value;
                m_Field.size = value;

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The unit suffix displayed by the numeric field. The suffix is a second scrub handle.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string unit
        {
            get => m_Field.unit;
            set
            {
                var changed = m_Field.unit != value;
                m_Field.unit = value;

                if (changed)
                    NotifyPropertyChanged(in unitProperty);
            }
        }

        /// <summary>
        /// The minimum allowed value, clamped on both a typed edit and a scrub.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<TValue> lowValue
        {
            get => m_Field.lowValue;
            set
            {
                var changed = !m_Field.lowValue.Equals(value);
                m_Field.lowValue = value;

                if (changed)
                    NotifyPropertyChanged(in lowValueProperty);
            }
        }

        /// <summary>
        /// The maximum allowed value, clamped on both a typed edit and a scrub.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<TValue> highValue
        {
            get => m_Field.highValue;
            set
            {
                var changed = !m_Field.highValue.Equals(value);
                m_Field.highValue = value;

                if (changed)
                    NotifyPropertyChanged(in highValueProperty);
            }
        }

        /// <summary>
        /// The format string applied to the displayed value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string formatString
        {
            get => m_Field.formatString;
            set
            {
                var changed = m_Field.formatString != value;
                m_Field.formatString = value;

                if (changed)
                    NotifyPropertyChanged(in formatStringProperty);
            }
        }
    }

    /// <summary>
    /// A labelled floating-point property row with a scrubbable label.
    /// </summary>
    /// <remarks>
    /// See <see cref="NumericProperty{TValue,TField}"/>. This is the <see cref="FloatField"/> flavour.
    /// </remarks>
    /// <example>
    /// <para>A clamped opacity row.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:NumericPropertyFloat label="Opacity" value="1" low-value="0" high-value="1" cap-value="true" />
    /// ]]></code>
    /// <para>Reacting to a change, whether it came from typing or from scrubbing the label.</para>
    /// <code lang="csharp"><![CDATA[
    /// var row = new NumericPropertyFloat { label = "Opacity", value = 1f, lowValue = 0f, highValue = 1f };
    /// row.RegisterValueChangedCallback(evt => Debug.Log($"Opacity: {evt.newValue}"));
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class NumericPropertyFloat : NumericProperty<float, FloatField>
    {
    }

    /// <summary>
    /// A labelled integer property row with a scrubbable label.
    /// </summary>
    /// <remarks>
    /// See <see cref="NumericProperty{TValue,TField}"/>. This is the <see cref="IntField"/> flavour.
    /// </remarks>
    /// <example>
    /// <para>A subdivision count row with a leading icon.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:NumericPropertyInt label="Subdivisions" icon="grid-four" value="4" low-value="1" />
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class NumericPropertyInt : NumericProperty<int, IntField>
    {
    }
}
