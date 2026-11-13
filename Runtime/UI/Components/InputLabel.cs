using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The text overflow mode.
    /// </summary>
    public enum TextOverflow
    {
        /// <summary>
        /// The text will be truncated with an ellipsis.
        /// </summary>
        Ellipsis,

        /// <summary>
        /// The text won't be truncated.
        /// </summary>
        Normal,
    }

    /// <summary>
    /// How an <see cref="InputLabel"/> lays its label and its input out.
    /// </summary>
    public enum InputLabelRowLayout
    {
        /// <summary>
        /// The label-plus-control flex row: the label takes a share of the row and the control the rest.
        /// This is the default, and what <see cref="InputLabel.direction"/> and
        /// <see cref="InputLabel.capValue"/> act on.
        /// </summary>
        Flex,

        /// <summary>
        /// The row sits on the inspector column grid from the tokens: the label spans
        /// <c>--appui-alias-inspector-grid-label-columns</c> of
        /// <c>--appui-alias-inspector-grid-columns</c> columns, the control takes the rest,
        /// <c>--appui-alias-inspector-grid-gap</c> separates them and the row is
        /// <c>--appui-alias-inspector-row-height</c> tall. Every grid row in a panel resolves the same
        /// variables, so labels and controls line up down the whole panel and a long label truncates
        /// instead of pushing its control out of column.
        /// </summary>
        Grid,
    }

    /// <summary>
    /// How a grid <see cref="InputLabel"/> hands its control cell to the children.
    /// </summary>
    /// <remarks>
    /// The registry's <c>PropertyField</c> takes <c>columns: false | true | 'pair'</c>. A bool cannot
    /// carry three states, so the UI Toolkit property is this enum.
    /// </remarks>
    public enum InputLabelColumns
    {
        /// <summary>
        /// The children stack in the control cell as an ordinary flex column. The default.
        /// </summary>
        None,

        /// <summary>
        /// The children share the control cell evenly, in a row. The registry hands the cell's columns
        /// to the children as a subgrid; UI Toolkit has no subgrid, so a child that needs a bigger share
        /// sets its own <c>flex-grow</c> instead of a column span.
        /// </summary>
        Even,

        /// <summary>
        /// The control cell splits into exactly two equal halves, with the inspector grid gap between
        /// them, for a pair of controls such as a slider and its number field. The halves stay halves
        /// whatever the children are: a lone child takes the first half, not the whole cell.
        /// </summary>
        Pair,
    }

    /// <summary>
    /// A versatile label component for form inputs with support for required fields, help text, and various
    /// layout options.
    /// </summary>
    /// <remarks>
    /// InputLabel is a specialized UI component designed to provide a consistent and accessible way to label form
    /// inputs. It combines a label with optional required field indicators and help text, making it ideal for form
    /// design and user input validation.
    ///
    /// The component supports both horizontal and vertical layouts, customizable label overflow behavior, and
    /// different styles of required field indicators to match your design system needs.
    ///
    /// Note: InputLabel is designed to work seamlessly with other form input components and follows accessibility
    /// best practices by maintaining proper label-input relationships.
    ///
    /// InputLabel is also the property row of the design system: a horizontal InputLabel with an optional
    /// leading <see cref="icon"/>, a <see cref="draggable"/> label that scrubs the numeric fields it wraps, and
    /// <see cref="capValue"/> to keep long property names readable on wide panels. For a labelled numeric row
    /// with the scrub wired in and impossible to forget, use <see cref="NumericPropertyFloat"/> or
    /// <see cref="NumericPropertyInt"/>.
    /// </remarks>
    /// <example>
    /// <para>A property row with a leading icon and a scrubbable label.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:InputLabel label="Opacity" icon="drop" draggable="true" cap-value="true">
    ///     <appui:FloatField value="1" low-value="0" high-value="1" />
    /// </appui:InputLabel>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class InputLabel : BaseVisualElement
    {

        internal static readonly BindingId labelProperty = new BindingId(nameof(label));

        internal static readonly BindingId directionProperty = new BindingId(nameof(direction));

        internal static readonly BindingId labelOverflowProperty = new BindingId(nameof(labelOverflow));

        internal static readonly BindingId inputAlignmentProperty = new BindingId(nameof(inputAlignment));

        internal static readonly BindingId requiredProperty = new BindingId(nameof(required));

        internal static readonly BindingId indicatorTypeProperty = new BindingId(nameof(indicatorType));

        internal static readonly BindingId requiredTextProperty = new BindingId(nameof(requiredText));

        internal static readonly BindingId helpMessageProperty = new BindingId(nameof(helpMessage));

        internal static readonly BindingId helpVariantProperty = new BindingId(nameof(helpVariant));

        internal static readonly BindingId draggableProperty = new BindingId(nameof(draggable));

        internal static readonly BindingId iconProperty = new BindingId(nameof(icon));

        internal static readonly BindingId capValueProperty = new BindingId(nameof(capValue));

        internal static readonly BindingId rowLayoutProperty = new BindingId(nameof(rowLayout));

        internal static readonly BindingId columnsProperty = new BindingId(nameof(columns));

        internal static readonly BindingId tallProperty = new BindingId(nameof(tall));

        internal static readonly BindingId labelForProperty = new BindingId(nameof(labelFor));


        /// <summary>
        /// The InputLabel main styling class.
        /// </summary>
        public const string ussClassName = "appui-inputlabel";

        /// <summary>
        /// The InputLabel direction styling class.
        /// </summary>
        [EnumName("GetOrientationUssClassName", typeof(Direction))]
        public const string orientationUssClassName = ussClassName + "--";

        /// <summary>
        /// The InputLabel input container styling class.
        /// </summary>
        public const string inputContainerUssClassName = ussClassName + "__input-container";

        /// <summary>
        /// The InputLabel container styling class.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The InputLabel label container styling class.
        /// </summary>
        public const string labelContainerUssClassName = ussClassName + "__label-container";

        /// <summary>
        /// The InputLabel field-label styling class.
        /// </summary>
        public const string fieldLabelUssClassName = ussClassName + "__field-label";

        /// <summary>
        /// The InputLabel leading icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The InputLabel styling class applied when a leading icon is set.
        /// </summary>
        public const string withIconUssClassName = ussClassName + "--with-icon";

        /// <summary>
        /// The InputLabel styling class applied when the input is width-capped.
        /// </summary>
        public const string capValueUssClassName = ussClassName + "--cap-value";

        /// <summary>
        /// The InputLabel help text styling class.
        /// </summary>
        public const string helpTextUssClassName = ussClassName + "__help-text";

        /// <summary>
        /// The InputLabel input alignment styling class.
        /// </summary>
        [EnumName("GetInputAlignmentUssClassName", typeof(Align))]
        public const string inputAlignmentUssClassName = ussClassName + "--input-alignment-";

        /// <summary>
        /// The InputLabel with help text styling class.
        /// </summary>
        public const string withHelpTextUssClassName = ussClassName + "--with-help-text";

        /// <summary>
        /// The InputLabel dragging styling class.
        /// </summary>
        public const string draggingUssClassName = ussClassName + "--dragging";

        /// <summary>
        /// The InputLabel row layout styling class.
        /// </summary>
        [EnumName("GetRowLayoutUssClassName", typeof(InputLabelRowLayout))]
        public const string rowLayoutUssClassName = ussClassName + "--layout-";

        /// <summary>
        /// The InputLabel styling class applied while its control cell is handed to the children as
        /// columns, in either <see cref="InputLabelColumns.Even"/> or <see cref="InputLabelColumns.Pair"/>.
        /// </summary>
        public const string columnsUssClassName = ussClassName + "--columns";

        /// <summary>
        /// The InputLabel styling class prefix carrying the <see cref="columns"/> mode.
        /// </summary>
        [EnumName("GetColumnsUssClassName", typeof(InputLabelColumns))]
        public const string columnsModeUssClassName = ussClassName + "--columns-";

        /// <summary>
        /// The styling class given to each child of the control cell while <see cref="columns"/> hands
        /// the cell out as columns.
        /// </summary>
        public const string columnUssClassName = ussClassName + "__column";

        /// <summary>
        /// The InputLabel styling class applied while a <see cref="InputLabelColumns.Pair"/> cell holds
        /// a single control, which then keeps to the first of the two tracks.
        /// </summary>
        public const string pairLoneUssClassName = ussClassName + "--columns-pair-lone";

        /// <summary>
        /// The InputLabel styling class applied to a grid row taller than one row.
        /// </summary>
        public const string tallUssClassName = ussClassName + "--tall";

        /// <summary>
        /// How many columns the inspector grid has. Read from the tokens rather than hard-coded, since
        /// USS has no grid and no calc(): the label's share has to be worked out in C# and applied as a
        /// percentage basis.
        /// </summary>
        static readonly CustomStyleProperty<float> k_GridColumns = new("--appui-alias-inspector-grid-columns");

        /// <summary>
        /// How many of those columns the label spans.
        /// </summary>
        static readonly CustomStyleProperty<float> k_GridLabelColumns = new("--appui-alias-inspector-grid-label-columns");

        readonly Icon m_Icon;

        readonly VisualElement m_LabelContainer;

        readonly FieldLabel m_FieldLabel;

        readonly VisualElement m_Container;

        readonly HelpText m_HelpText;

        Direction m_Direction = Direction.Horizontal;

        Align m_InputAlignment = Align.Stretch;

        bool m_Draggable;

        Draggable m_DraggableManipulator;

        readonly VisualElement m_InputContainer;

        InputLabelRowLayout m_RowLayout = InputLabelRowLayout.Flex;

        InputLabelColumns m_Columns = InputLabelColumns.None;

        const long k_ColumnWatchIntervalMs = 100;

        IVisualElementScheduledItem m_ColumnWatch;

        // Every child of the control cell as last tagged, in order, so a swap at the same count is seen.
        readonly List<VisualElement> m_WatchedChildren = new List<VisualElement>();

        string m_LabelFor;

        // The shipped token values, used only until the custom properties resolve — a row built and
        // measured before it is in a panel has no stylesheet to read them from. The tokens remain the
        // single source: whatever they say overrides these on the first CustomStyleResolvedEvent.
        float m_GridColumns = 7f;

        float m_GridLabelColumns = 3f;

        /// <summary>
        /// The content container.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The label value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_FieldLabel.label;
            set
            {
                var changed = m_FieldLabel.label != value;
                m_FieldLabel.label = value;
                m_FieldLabel.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// The orientation of the label.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Direction direction
        {
            get => m_Direction;
            set
            {
                var changed = m_Direction != value;
                RemoveFromClassList(GetOrientationUssClassName(m_Direction));
                m_Direction = value;
                AddToClassList(GetOrientationUssClassName(m_Direction));

                if (changed)
                    NotifyPropertyChanged(in directionProperty);
            }
        }

        /// <summary>
        /// The text overflow mode.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public TextOverflow labelOverflow
        {
            get => m_FieldLabel.labelOverflow;
            set
            {
                var changed = m_FieldLabel.labelOverflow != value;
                m_FieldLabel.labelOverflow = value;

                if (changed)
                    NotifyPropertyChanged(in labelOverflowProperty);
            }
        }

        /// <summary>
        /// The alignment of the input.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Align inputAlignment
        {
            get => m_InputAlignment;
            set
            {
                var changed = m_InputAlignment != value;
                RemoveFromClassList(GetInputAlignmentUssClassName(m_InputAlignment));
                m_InputAlignment = value;
                AddToClassList(GetInputAlignmentUssClassName(m_InputAlignment));

                if (changed)
                    NotifyPropertyChanged(in inputAlignmentProperty);
            }
        }

        /// <summary>
        /// Whether the input is required or not in the form. This will add an asterisk next to the label.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool required
        {
            get => m_FieldLabel.required;
            set
            {
                var changed = m_FieldLabel.required != value;
                m_FieldLabel.required = value;
                EnableInClassList(Styles.requiredUssClassName,  m_FieldLabel.required);

                if (changed)
                    NotifyPropertyChanged(in requiredProperty);
            }
        }

        /// <summary>
        /// The requirement indicator to display.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public IndicatorType indicatorType
        {
            get => m_FieldLabel.indicatorType;
            set
            {
                var changed = m_FieldLabel.indicatorType != value;
                m_FieldLabel.indicatorType = value;

                if (changed)
                    NotifyPropertyChanged(in indicatorTypeProperty);
            }
        }

        /// <summary>
        /// The requirement indicator to display.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string requiredText
        {
            get => m_FieldLabel.requiredText;
            set
            {
                var changed = m_FieldLabel.requiredText != value;
                m_FieldLabel.requiredText = value;

                if (changed)
                    NotifyPropertyChanged(in requiredTextProperty);
            }
        }

        /// <summary>
        /// The error message to display.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string helpMessage
        {
            get => m_HelpText.text;
            set
            {
                var changed = m_HelpText.text != value;
                m_HelpText.text = value;
                EnableInClassList(withHelpTextUssClassName, !string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in helpMessageProperty);
            }
        }

        /// <summary>
        /// The variant of the <see cref="HelpText"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public HelpTextVariant helpVariant
        {
            get => m_HelpText.variant;
            set
            {
                var changed = m_HelpText.variant != value;
                m_HelpText.variant = value;

                if (changed)
                    NotifyPropertyChanged(in helpVariantProperty);
            }
        }

        /// <summary>
        /// The name of an optional leading icon displayed before the label.
        /// </summary>
        /// <remarks>
        /// Use it to mark a property row with a glyph, the way an inspector row does. Set it to
        /// <c>null</c> or an empty string to remove the icon.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string icon
        {
            get => m_Icon.iconName;
            set
            {
                var changed = m_Icon.iconName != value;
                m_Icon.iconName = value;
                var hasIcon = !string.IsNullOrEmpty(value);
                m_Icon.EnableInClassList(Styles.hiddenUssClassName, !hasIcon);
                EnableInClassList(withIconUssClassName, hasIcon);

                if (changed)
                    NotifyPropertyChanged(in iconProperty);
            }
        }

        /// <summary>
        /// Whether the input is width-capped so that the label takes the freed space.
        /// </summary>
        /// <remarks>
        /// By default the label occupies a fixed fraction of the row
        /// (<c>--appui-inputlabel-label-size</c>) and the input grows to fill the rest, so a wide
        /// panel stretches the input and truncates a long label. With <c>capValue</c> the label
        /// grows instead and the input is capped at <c>--appui-inputlabel-input-max-size</c>,
        /// which keeps long property names readable. Only affects the horizontal direction.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool capValue
        {
            get => ClassListContains(capValueUssClassName);
            set
            {
                var changed = capValue != value;
                EnableInClassList(capValueUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in capValueProperty);
            }
        }

        /// <summary>
        /// Whether the label is draggable to increment/decrement the value of child input fields.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool draggable
        {
            get => m_Draggable;
            set
            {
                var changed = m_Draggable != value;
                m_Draggable = value;
                EnableInClassList(draggingUssClassName, m_Draggable);

                if (m_Draggable)
                {
                    if (m_DraggableManipulator == null)
                    {
                        m_DraggableManipulator = new Draggable(null, OnDrag, OnDragEnd, OnDragStart)
                        {
                            dragDirection = Draggable.DragDirection.Horizontal
                        };
                        m_DraggableManipulator.activators.Add(new ManipulatorActivationFilter
                        {
                            button = MouseButton.LeftMouse,
                            modifiers = EventModifiers.Shift
                        });
                        m_DraggableManipulator.activators.Add(new ManipulatorActivationFilter
                        {
                            button = MouseButton.LeftMouse,
                            modifiers = EventModifiers.Alt
                        });
                        m_DraggableManipulator.activators.Add(new ManipulatorActivationFilter
                        {
                            button = MouseButton.LeftMouse,
                            modifiers = EventModifiers.Shift | EventModifiers.Alt
                        });
                        m_LabelContainer.AddManipulator(m_DraggableManipulator);
                    }
                }
                else
                {
                    if (m_DraggableManipulator != null)
                    {
                        m_LabelContainer.RemoveManipulator(m_DraggableManipulator);
                        m_DraggableManipulator = null;
                    }
                }

                if (changed)
                    NotifyPropertyChanged(in draggableProperty);
            }
        }

        void OnDragStart(Draggable d)
        {
            var s = d.shiftKey ? DeltaSpeed.Fast : d.altKey ? DeltaSpeed.Slow : DeltaSpeed.Normal;
            this.ProvideContext(new DragContext(DragPhase.Started, Vector3.zero, s));
        }

        void OnDrag(Draggable d)
        {
            var s = d.shiftKey ? DeltaSpeed.Fast : d.altKey ? DeltaSpeed.Slow : DeltaSpeed.Normal;
            this.ProvideContext(new DragContext(DragPhase.Dragging, d.deltaPos, s));
        }

        void OnDragEnd(Draggable d)
        {
            var s = d.shiftKey ? DeltaSpeed.Fast : d.altKey ? DeltaSpeed.Slow : DeltaSpeed.Normal;
            this.ProvideContext(new DragContext(DragPhase.Ended, Vector3.zero, s));
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InputLabel()
            : this(null)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="label"> The label value. </param>
        public InputLabel(string label)
        {
            AddToClassList(ussClassName);
            focusable = true;
            delegatesFocus = true;
            pickingMode = PickingMode.Position;

            m_Icon = new Icon { name = iconUssClassName, size = IconSize.Sm, pickingMode = PickingMode.Ignore };
            m_Icon.AddToClassList(iconUssClassName);
            hierarchy.Add(m_Icon);

            m_LabelContainer = new VisualElement { name = labelContainerUssClassName, pickingMode = PickingMode.Position };
            m_LabelContainer.AddToClassList(labelContainerUssClassName);
            hierarchy.Add(m_LabelContainer);

            m_FieldLabel = new FieldLabel(label) { name = fieldLabelUssClassName, pickingMode = PickingMode.Ignore };
            m_FieldLabel.AddToClassList(fieldLabelUssClassName);
            m_LabelContainer.hierarchy.Add(m_FieldLabel);

            var cell = new HelpText { pickingMode = PickingMode.Ignore };
            cell.AddToClassList(helpTextUssClassName);
            m_LabelContainer.hierarchy.Add(cell);

            m_InputContainer = new VisualElement { name = inputContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_InputContainer.AddToClassList(inputContainerUssClassName);
            hierarchy.Add(m_InputContainer);

            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            m_InputContainer.hierarchy.Add(m_Container);

            m_HelpText = new HelpText
            {
                name = helpTextUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_HelpText.AddToClassList(helpTextUssClassName);
            m_InputContainer.hierarchy.Add(m_HelpText);

            icon = null;
            capValue = false;
            this.label = label;
            direction = Direction.Horizontal;
            inputAlignment = Align.Stretch;
            labelOverflow = TextOverflow.Ellipsis;
            requiredText = "(Required)";
            indicatorType = IndicatorType.Asterisk;
            required = false;
            helpMessage = null;
            helpVariant = HelpTextVariant.Destructive;
            draggable = false;
            rowLayout = InputLabelRowLayout.Flex;
            columns = InputLabelColumns.None;
            tall = false;

            m_LabelContainer.RegisterCallback<ClickEvent>(OnLabelClicked);
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            // The control cell's own children are the caller's, and an element cannot observe children
            // being added, so the column markers are refreshed on every layout pass of that cell — the
            // same way SidebarSlot keeps its gaps right.
            m_Container.RegisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
        }

        /// <summary>
        /// The container holding the input(s), between the label cell and the help text.
        /// </summary>
        public VisualElement inputContainer => m_InputContainer;

        /// <summary>
        /// How the label and the control are laid out.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="InputLabelRowLayout.Grid"/> puts the row on the inspector column grid, which is
        /// what makes a stack of rows share one set of column edges. <see cref="capValue"/> has no
        /// effect there: the token columns already size both cells, and capping the control would move
        /// the column edge on that row alone.
        /// </para>
        /// <para>
        /// A grid row keeps its leading <see cref="icon"/> inside the label cell rather than beside it,
        /// so the icon comes out of the label's share and the control still starts on the column edge.
        /// </para>
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public InputLabelRowLayout rowLayout
        {
            get => m_RowLayout;
            set
            {
                var changed = m_RowLayout != value;
                RemoveFromClassList(GetRowLayoutUssClassName(m_RowLayout));
                m_RowLayout = value;
                AddToClassList(GetRowLayoutUssClassName(m_RowLayout));
                PlaceIcon();
                ApplyGridProportions();

                if (changed)
                    NotifyPropertyChanged(in rowLayoutProperty);
            }
        }

        /// <summary>
        /// Grid layout only: how the control cell is handed to the children.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="InputLabelColumns.Even"/> lays the children out in a row that shares the cell
        /// evenly; a child that needs a bigger share sets its own <c>flex-grow</c>, the UI Toolkit
        /// stand-in for a subgrid span. <see cref="InputLabelColumns.Pair"/> splits the cell into
        /// exactly two equal halves with the inspector grid gap between them, for a slider and its
        /// number field, and the halves line up with the next pair row's.
        /// </para>
        /// <para>
        /// The registry's <c>PropertyField</c> takes <c>columns: boolean | 'pair'</c>. Three states do
        /// not fit a bool, so this is an enum.
        /// </para>
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public InputLabelColumns columns
        {
            get => m_Columns;
            set
            {
                var changed = m_Columns != value;
                RemoveFromClassList(GetColumnsUssClassName(m_Columns));
                m_Columns = value;
                AddToClassList(GetColumnsUssClassName(m_Columns));
                EnableInClassList(columnsUssClassName, m_Columns != InputLabelColumns.None);
                RefreshColumnMarkers();
                UpdateColumnWatch();

                if (changed)
                    NotifyPropertyChanged(in columnsProperty);
            }
        }

        /// <summary>
        /// Grid layout only: a control taller than one row, a multi-line text area for instance. The row
        /// grows to fit it and aligns its cells to the top, so the label stays on the first line.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool tall
        {
            get => ClassListContains(tallUssClassName);
            set
            {
                var changed = tall != value;
                EnableInClassList(tallUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in tallProperty);
            }
        }

        /// <summary>
        /// The <see cref="VisualElement.name"/> of the control this row labels. Set it and a click on the
        /// label operates that control, the way a click on an HTML <c>&lt;label for&gt;</c> toggles its
        /// checkbox.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The control is looked up inside this row first, then anywhere in the panel, and must be
        /// <see cref="IPressable"/>: a <see cref="Checkbox"/>, a <see cref="Toggle"/> or a
        /// <see cref="Radio"/>, for instance. A disabled control is left alone.
        /// </para>
        /// <para>
        /// Ignored while <see cref="draggable"/> is on, as the registry ignores <c>labelFor</c> while
        /// <c>scrub</c> is active: a drag that ends on the label would otherwise toggle the control at
        /// the end of every scrub, and a row only scrubs when its value is numeric, which is not a
        /// control a label click operates anyway.
        /// </para>
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string labelFor
        {
            get => m_LabelFor;
            set
            {
                var changed = m_LabelFor != value;
                m_LabelFor = value;

                if (changed)
                    NotifyPropertyChanged(in labelForProperty);
            }
        }

        /// <summary>
        /// Re-tags the children of the control cell: the first one renders without a leading gap, and
        /// each one is styled as a column while <see cref="columns"/> is on.
        /// </summary>
        /// <remarks>
        /// This runs on every layout pass of that cell and, while <see cref="columns"/> is on, whenever
        /// the children change. Call it directly only after mutating the children when the markers are
        /// needed before the next frame.
        /// </remarks>
        public void RefreshColumnMarkers()
        {
            // The children are the caller's, and `> *` does not reliably match them, so the stylesheet
            // reaches them through a class of their own while the cell is handed out as columns.
            var asColumns = m_Columns != InputLabelColumns.None;
            for (var i = 0; i < m_Container.childCount; i++)
            {
                m_Container[i].EnableInClassList(Styles.firstChildUssClassName, i == 0);
                m_Container[i].EnableInClassList(columnUssClassName, asColumns);
            }

            // A pair keeps two tracks even with one control in it, so the lone control is sized as if
            // the gap and the second half were there.
            EnableInClassList(pairLoneUssClassName, m_Columns == InputLabelColumns.Pair && m_Container.childCount == 1);

            m_WatchedChildren.Clear();
            for (var i = 0; i < m_Container.childCount; i++)
                m_WatchedChildren.Add(m_Container[i]);
        }

        /// <summary>
        /// Starts or stops watching the control cell's children. A child added to a cell whose size does
        /// not change raises no layout event, so the markers would otherwise miss it.
        /// </summary>
        void UpdateColumnWatch()
        {
            if (m_Columns == InputLabelColumns.None)
            {
                m_ColumnWatch?.Pause();
                return;
            }

            if (m_ColumnWatch == null)
                m_ColumnWatch = m_Container.schedule.Execute(WatchColumnChildren).Every(k_ColumnWatchIntervalMs);
            else
                m_ColumnWatch.Resume();
        }

        void WatchColumnChildren()
        {
            var changed = m_Container.childCount != m_WatchedChildren.Count;
            for (var i = 0; !changed && i < m_WatchedChildren.Count; i++)
                changed = m_Container[i] != m_WatchedChildren[i];
            if (changed)
                RefreshColumnMarkers();
        }

        void OnLabelClicked(ClickEvent evt)
        {
            if (m_Draggable || string.IsNullOrEmpty(m_LabelFor))
                return;

            var control = this.Q(name: m_LabelFor) ?? panel?.visualTree.Q(name: m_LabelFor);
            if (control is IPressable { clickable: not null } pressable && control.enabledInHierarchy)
            {
                // As with an HTML label, the control takes focus too, so the keyboard carries on where
                // the click left off.
                if (control.focusable)
                    control.Focus();
                pressable.clickable.InvokePressed(evt);
            }
        }

        void OnContainerGeometryChanged(GeometryChangedEvent evt)
        {
            RefreshColumnMarkers();
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            var changed = false;

            if (evt.customStyle.TryGetValue(k_GridColumns, out var gridColumns)
                && gridColumns > 0f
                && !Mathf.Approximately(gridColumns, m_GridColumns))
            {
                m_GridColumns = gridColumns;
                changed = true;
            }

            if (evt.customStyle.TryGetValue(k_GridLabelColumns, out var labelColumns)
                && labelColumns > 0f
                && !Mathf.Approximately(labelColumns, m_GridLabelColumns))
            {
                m_GridLabelColumns = labelColumns;
                changed = true;
            }

            if (changed)
                ApplyGridProportions();
        }

        /// <summary>
        /// Gives the label cell the share of the row its column span is worth.
        /// </summary>
        /// <remarks>
        /// The basis is a percentage of the row, not of what is left after the gap, so the column edge
        /// lands in the same place whatever the gap resolves to at this density. The control cell then
        /// grows into the remainder, which is where the gap comes out of.
        /// </remarks>
        void ApplyGridProportions()
        {
            if (m_RowLayout != InputLabelRowLayout.Grid)
            {
                m_LabelContainer.style.flexBasis = StyleKeyword.Null;
                return;
            }

            var span = Mathf.Clamp(m_GridLabelColumns, 1f, m_GridColumns);
            m_LabelContainer.style.flexBasis = Length.Percent(span / m_GridColumns * 100f);
        }

        /// <summary>
        /// Moves the leading icon in or out of the label cell, depending on the layout.
        /// </summary>
        /// <remarks>
        /// In the flex row the icon sits before the label cell, as it always has. On the grid it belongs
        /// inside that cell: an icon beside it would eat into the row before the columns are measured,
        /// so a row with an icon would put its control one icon-width off the column the row above uses.
        /// </remarks>
        void PlaceIcon()
        {
            m_Icon.RemoveFromHierarchy();
            if (m_RowLayout == InputLabelRowLayout.Grid)
                m_LabelContainer.hierarchy.Insert(0, m_Icon);
            else
                hierarchy.Insert(0, m_Icon);
        }

    }
}
