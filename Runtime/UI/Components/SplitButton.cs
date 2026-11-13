using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// One bordered control in two parts, <c>[main | caret]</c>. The main part is a button, or a toggle
    /// when the caller drives <see cref="selected"/>; the caret opens that control's options.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The two parts are separate press targets inside one rim, so a click on the main part runs the
    /// action and a click on the caret only offers the alternatives. The caret is not wired to anything
    /// on its own: build a <see cref="Popover"/> or a menu against <see cref="caretElement"/> from
    /// <see cref="caretClicked"/>, and the caret carries <c>is-open</c> for as long as it is showing.
    /// </para>
    /// <para>
    /// The chrome is the secondary <see cref="Button"/>'s, so a row of plain buttons and split buttons
    /// reads as one row. It is one control with an options caret rather than a set of choices, so it
    /// takes no group or selector role: <see cref="ActionGroup"/> is the element for a segmented
    /// selector.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>A split button whose caret opens a menu of the other things it could do.</para>
    /// <code><![CDATA[
    /// <UXML xmlns:ui="Unity.AppUI.UI">
    ///     <ui:SplitButton label="Play" icon="play" caret-tooltip="Play options" />
    /// </UXML>
    /// ]]></code>
    /// <code><![CDATA[
    /// splitButton.clicked += Play;
    /// splitButton.caretClicked += () => ShowPlayOptions(splitButton.caretElement);
    /// ]]></code>
    /// </example>
    [VisualDocPage("actions")]
    [UxmlElement]
    public partial class SplitButton : BaseVisualElement, ISizeableElement, ISelectableElement, IPressable
    {
        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId iconProperty = nameof(icon);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId selectedProperty = nameof(selected);

        internal static readonly BindingId caretTooltipProperty = nameof(caretTooltip);

        internal static readonly BindingId clickableProperty = nameof(clickable);

        internal static readonly BindingId caretClickableProperty = nameof(caretClickable);

        /// <summary>
        /// The SplitButton main styling class.
        /// </summary>
        public const string ussClassName = "appui-split-button";

        /// <summary>
        /// The styling class of the SplitButton's main part.
        /// </summary>
        public const string mainUssClassName = ussClassName + "__main";

        /// <summary>
        /// The styling class of the SplitButton's caret part.
        /// </summary>
        public const string caretUssClassName = ussClassName + "__caret";

        /// <summary>
        /// The styling class of the label inside the SplitButton's main part.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The styling class of the leading icon inside the SplitButton's main part.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The styling class of the glyph inside the SplitButton's caret part.
        /// </summary>
        public const string caretIconUssClassName = ussClassName + "__caret-icon";

        /// <summary>
        /// The SplitButton size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The SplitButton styling class applied when its main part carries an icon and no label.
        /// </summary>
        public const string iconOnlyUssClassName = ussClassName + "--icon-only";

        /// <summary>
        /// The SplitButton styling class applied when its main part carries both an icon and a label.
        /// </summary>
        public const string iconAndLabelUssClassName = ussClassName + "--icon-and-label";

        const string k_CaretIconName = "caret-down";

        readonly VisualElement m_Main;

        readonly VisualElement m_Caret;

        readonly LocalizedTextElement m_Label;

        readonly Icon m_Icon;

        readonly Icon m_CaretIcon;

        Size m_Size;

        Pressable m_Clickable;

        Pressable m_CaretClickable;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitButton"/> class.
        /// </summary>
        public SplitButton() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitButton"/> class, with the action its main
        /// part runs when clicked.
        /// </summary>
        /// <param name="clickEvent"> The action triggered by the main part. </param>
        public SplitButton(Action clickEvent)
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_Main = new VisualElement
            {
                name = mainUssClassName,
                pickingMode = PickingMode.Position,
                focusable = true,
                tabIndex = 0
            };
            m_Main.AddToClassList(mainUssClassName);

            m_Icon = new Icon
            {
                primary = true,
                name = iconUssClassName,
                iconName = null,
                pickingMode = PickingMode.Ignore
            };
            m_Icon.AddToClassList(iconUssClassName);

            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);

            m_Main.hierarchy.Add(m_Icon);
            m_Main.hierarchy.Add(m_Label);

            m_Caret = new VisualElement
            {
                name = caretUssClassName,
                pickingMode = PickingMode.Position,
                focusable = true,
                tabIndex = 0
            };
            m_Caret.AddToClassList(caretUssClassName);

            m_CaretIcon = new Icon
            {
                primary = true,
                name = caretIconUssClassName,
                iconName = k_CaretIconName,
                size = IconSize.Xs,
                pickingMode = PickingMode.Ignore
            };
            m_CaretIcon.AddToClassList(caretIconUssClassName);
            m_Caret.hierarchy.Add(m_CaretIcon);

            hierarchy.Add(m_Main);
            hierarchy.Add(m_Caret);

            clickable = new Pressable(clickEvent);
            caretClickable = new Pressable();

            label = null;
            icon = null;
            size = Size.Md;
            selected = false;
            caretTooltip = null;

            m_Main.AddManipulator(new KeyboardFocusController());
            m_Caret.AddManipulator(new KeyboardFocusController());
        }

        /// <summary>
        /// The main part of the SplitButton. Exposed so a caller can set an accessible tooltip or a
        /// USS class of its own on the part that carries the action.
        /// </summary>
        public VisualElement mainElement => m_Main;

        /// <summary>
        /// The caret part of the SplitButton. This is the element to anchor a menu or a
        /// <see cref="Popover"/> to.
        /// </summary>
        public VisualElement caretElement => m_Caret;

        /// <summary>
        /// The label element of the SplitButton's main part.
        /// </summary>
        public LocalizedTextElement labelElement => m_Label;

        /// <summary>
        /// The leading icon element of the SplitButton's main part.
        /// </summary>
        public Icon iconElement => m_Icon;

        /// <summary>
        /// The glyph element inside the caret part, for a caller that wants a different glyph.
        /// </summary>
        public Icon caretIconElement => m_CaretIcon;

        /// <summary>
        /// Event triggered when the main part has been clicked.
        /// </summary>
        public event Action clicked
        {
            add => m_Clickable.clicked += value;
            remove => m_Clickable.clicked -= value;
        }

        /// <summary>
        /// Event triggered when the caret part has been clicked: the caller's cue to show the options.
        /// </summary>
        public event Action caretClicked
        {
            add => m_CaretClickable.clicked += value;
            remove => m_CaretClickable.clicked -= value;
        }

        /// <summary>
        /// The Pressable manipulator of the main part.
        /// </summary>
        [CreateProperty]
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                var changed = m_Clickable != value;
                if (m_Clickable != null && m_Clickable.target == m_Main)
                    m_Main.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable != null)
                    m_Main.AddManipulator(m_Clickable);

                if (changed)
                    NotifyPropertyChanged(in clickableProperty);
            }
        }

        /// <summary>
        /// The Pressable manipulator of the caret part.
        /// </summary>
        [CreateProperty]
        public Pressable caretClickable
        {
            get => m_CaretClickable;
            set
            {
                var changed = m_CaretClickable != value;
                if (m_CaretClickable != null && m_CaretClickable.target == m_Caret)
                    m_Caret.RemoveManipulator(m_CaretClickable);
                m_CaretClickable = value;
                if (m_CaretClickable != null)
                    m_Caret.AddManipulator(m_CaretClickable);

                if (changed)
                    NotifyPropertyChanged(in caretClickableProperty);
            }
        }

        /// <summary>
        /// The label of the SplitButton's main part. Null or empty leaves an icon-only main part.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_Label.text;
            set
            {
                var changed = m_Label.text != value;
                m_Label.text = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// The name of the leading icon of the SplitButton's main part. Null or empty hides it.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string icon
        {
            get => m_Icon.iconName;
            set
            {
                var changed = m_Icon.iconName != value;
                m_Icon.iconName = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in iconProperty);
            }
        }

        /// <summary>
        /// The tooltip of the caret part. The caret has no visible text, so this is what says what its
        /// options are for.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string caretTooltip
        {
            get => m_Caret.tooltip;
            set
            {
                var changed = m_Caret.tooltip != value;
                m_Caret.tooltip = value;

                if (changed)
                    NotifyPropertyChanged(in caretTooltipProperty);
            }
        }

        /// <summary>
        /// The size of the SplitButton. Both parts take their height from the control-sizing alias for
        /// this step, so a split button lines up with any other control of the same size.
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
        /// The selected state of the SplitButton's main part, which is what makes the main part read as
        /// a toggle rather than a plain button. The control does not flip it on its own: set it from
        /// <see cref="clicked"/>, so the state stays the caller's.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool selected
        {
            get => m_Main.ClassListContains(Styles.selectedUssClassName);
            set
            {
                var changed = selected != value;
                SetSelectedWithoutNotify(value);

                if (changed)
                    NotifyPropertyChanged(in selectedProperty);
            }
        }

        /// <summary>
        /// Sets the selected state without sending an event through the visual tree.
        /// </summary>
        /// <param name="newValue"> The new selected state. </param>
        public void SetSelectedWithoutNotify(bool newValue)
        {
            // The fill belongs to the main part, not to the whole control: a pressed toggle must not
            // light the caret, which offers options rather than carrying the state.
            m_Main.EnableInClassList(Styles.selectedUssClassName, newValue);
        }

        void Refresh()
        {
            var hasIcon = !string.IsNullOrEmpty(icon);
            var hasLabel = !string.IsNullOrEmpty(label);
            EnableInClassList(iconOnlyUssClassName, hasIcon && !hasLabel);
            EnableInClassList(iconAndLabelUssClassName, hasIcon && hasLabel);
            m_Icon.EnableInClassList(Styles.hiddenUssClassName, !hasIcon);
            m_Label.EnableInClassList(Styles.hiddenUssClassName, !hasLabel);
        }
    }
}
