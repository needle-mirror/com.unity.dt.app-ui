using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A compact element that represents an input, attribute, or action.
    /// </summary>
    /// <remarks>
    /// Chips are compact elements that represent an input, attribute, or action. They allow users to enter
    /// information, make selections, filter content, or trigger actions.
    ///
    /// Chips can be used for various purposes including:
    /// - Filtering content or search results
    /// - Entering data or tags
    /// - Making selections from a set of options
    /// - Triggering actions
    ///
    /// **Tip:** The Chip component supports two visual variants (Filled and Outlined), can include an ornament
    /// (like an icon or avatar), and optionally provides delete functionality.
    /// </remarks>
    /// <example>
    /// <para>Basic Usage — Creating a simple chip with just a label.</para>
    /// <code lang="xml"><![CDATA[
    /// <Chip label="Basic Chip" />
    /// ]]></code>
    /// <para>Interactive Chip with Events — Creating an interactive chip with click and delete events.</para>
    /// <code lang="csharp"><![CDATA[
    /// var chip = new Chip { label = "Click Me" };
    /// chip.clicked += () => Debug.Log("Chip clicked!");
    /// chip.deleted += () => Debug.Log("Chip deleted!");
    /// ]]></code>
    /// <para>Filter Chips Example — Creating a group of filter chips.</para>
    /// <code lang="xml"><![CDATA[
    /// <Group>
    ///     <Chip variant="Outlined" label="Filter 1" deletable="true" />
    ///     <Chip variant="Outlined" label="Filter 2" deletable="true" />
    ///     <Chip variant="Outlined" label="Filter 3" deletable="true" />
    /// </Group>
    /// ]]></code>
    /// <para>Chip with Custom Styling — Customizing chip colors using custom properties.</para>
    /// <code lang="csharp"><![CDATA[
    /// var chip = new Chip { label = "Custom Style" };
    /// chip.style.SetCustomProperty("--chip-color", new Color(1, 0, 0, 1));
    /// chip.style.SetCustomProperty("--chip-background-color", new Color(0, 0, 1, 0.1f));
    /// ]]></code>
    /// </example>
    [VisualDocPage("actions")]
    [UxmlElement]
    public partial class Chip : BaseVisualElement, IPressable
    {

        internal static readonly BindingId deleteIconProperty = nameof(deleteIcon);

        internal static readonly BindingId ornamentProperty = nameof(ornament);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId variantProperty = nameof(variant);

        internal static readonly BindingId deletableProperty = nameof(deletable);

        internal static readonly BindingId clickableProperty = nameof(clickable);

        internal static readonly BindingId deleteProperty = nameof(delete);


        /// <summary>
        /// The possible variants for a <see cref="Chip"/>.
        /// </summary>
        public enum Variant
        {
            /// <summary>
            /// The <see cref="Chip"/> is displayed with a fill color.
            /// </summary>
            Filled,
            /// <summary>
            /// The <see cref="Chip"/> is displayed with an outline.
            /// </summary>
            Outlined,
        }

        const string k_DefaultDeleteIconName = "x";

        /// <summary>
        /// The Chip main styling class.
        /// </summary>
        public const string ussClassName = "appui-chip";

        /// <summary>
        /// The Chip variant styling class.
        /// </summary>
        [EnumName("GetVariantUssClassName", typeof(Variant))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The Chip Clickable variant styling class.
        /// </summary>
        public const string clickableUssClassName = ussClassName + "--clickable";

        /// <summary>
        /// The Chip Deletable variant styling class.
        /// </summary>
        public const string deletableUssClassName = ussClassName + "--deletable";

        /// <summary>
        /// The Chip with ornament variant styling class.
        /// </summary>
        public const string withOrnamentUssClassName = ussClassName + "--with-ornament";

        /// <summary>
        /// The Chip label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The Chip ornament container styling class.
        /// </summary>
        public const string ornamentContainerUssClassName = ussClassName + "__ornament-container";

        /// <summary>
        /// The Chip delete Button styling class.
        /// </summary>
        public const string deleteButtonUssClassName = ussClassName + "__delete-button";

        /// <summary>
        /// The Chip delete Icon styling class.
        /// </summary>
        public const string deleteIconUssClassName = ussClassName + "__delete-icon";

        static readonly CustomStyleProperty<Color> k_UssColor = new CustomStyleProperty<Color>("--chip-color");

        static readonly CustomStyleProperty<Color> k_UssBgColor = new CustomStyleProperty<Color>("--chip-background-color");

        readonly VisualElement m_DeleteButton;

        readonly Icon m_DeleteIcon;

        Pressable m_Clickable;

        readonly LocalizedTextElement m_Label;

        Variant m_Variant;

        EventHandler m_Clicked;

        EventHandler m_Deleted;

        VisualElement m_Ornament;

        readonly VisualElement m_OrnamentContainer;

        Pressable m_DeleteHandler;

        /// <summary>
        /// The content container of the Chip. This is the ornament container.
        /// </summary>
        public override VisualElement contentContainer => m_OrnamentContainer;

        /// <summary>
        /// The icon name for the delete button.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string deleteIcon
        {
            get => m_DeleteIcon.iconName;
            set
            {
                var changed = m_DeleteIcon.iconName != value;
                m_DeleteIcon.iconName = value;

                if (changed)
                    NotifyPropertyChanged(in deleteIconProperty);
            }
        }

        /// <summary>
        /// The Chip variant.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Variant variant
        {
            get => m_Variant;
            set
            {
                var changed = m_Variant != value;
                RemoveFromClassList(GetVariantUssClassName(m_Variant));
                m_Variant = value;
                AddToClassList(GetVariantUssClassName(m_Variant));

                if (changed)
                    NotifyPropertyChanged(in variantProperty);
            }
        }

        /// <summary>
        /// The Chip ornament.
        /// </summary>
        [CreateProperty]
        public VisualElement ornament
        {
            get => m_Ornament;
            set
            {
                var changed = m_Ornament != value;
                if (m_Ornament != null && m_Ornament.parent == m_OrnamentContainer)
                    m_OrnamentContainer.Remove(m_Ornament);
                m_Ornament = value;
                if (m_Ornament != null)
                    m_OrnamentContainer.Add(m_Ornament);
                EnableInClassList(withOrnamentUssClassName, m_Ornament != null);

                if (changed)
                    NotifyPropertyChanged(in ornamentProperty);
            }
        }

        /// <summary>
        /// The Chip label.
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

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// Set the Chip as deletable.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool deletable
        {
            get => ClassListContains(deletableUssClassName);
            set
            {
                var changed = ClassListContains(deletableUssClassName) != value;
                EnableInClassList(deletableUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in deletableProperty);
            }
        }

        /// <summary>
        /// Clickable Manipulator for this Chip.
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
                RemoveFromClassList(clickableUssClassName);
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
                AddToClassList(clickableUssClassName);
                if (changed)
                    NotifyPropertyChanged(in clickableProperty);
            }
        }

        /// <summary>
        /// Deletion Manipulator for this Chip.
        /// </summary>
        [CreateProperty]
        public Pressable delete
        {
            get => m_DeleteHandler;
            set
            {
                var changed = m_DeleteHandler != value;
                if (m_DeleteHandler != null && m_DeleteHandler.target == this)
                    m_DeleteButton.RemoveManipulator(m_DeleteHandler);
                m_DeleteHandler = value;
                if (m_DeleteHandler == null)
                    return;
                m_DeleteButton.AddManipulator(m_DeleteHandler);
                if (changed)
                    NotifyPropertyChanged(in deleteProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Chip()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            clickable = new Pressable();
            focusable = true;
            tabIndex = 0;

            m_OrnamentContainer = new VisualElement { name = ornamentContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_OrnamentContainer.AddToClassList(ornamentContainerUssClassName);
            hierarchy.Add(m_OrnamentContainer);

            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            hierarchy.Add(m_Label);

            m_DeleteButton = new VisualElement { name = deleteButtonUssClassName, pickingMode = PickingMode.Position, focusable = true };
            m_DeleteButton.AddToClassList(deleteButtonUssClassName);
            hierarchy.Add(m_DeleteButton);

            m_DeleteIcon = new Icon { name = deleteIconUssClassName, pickingMode = PickingMode.Ignore };
            m_DeleteIcon.AddToClassList(deleteIconUssClassName);
            m_DeleteButton.hierarchy.Add(m_DeleteIcon);

            delete = new Pressable();
            deleteIcon = k_DefaultDeleteIconName;
            variant = Variant.Filled;
            ornament = null;
        }

    }
}
