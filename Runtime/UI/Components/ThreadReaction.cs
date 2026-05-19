using System;
using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// ThreadReaction UI element. Displays an emoji reaction with a count.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class ThreadReaction : BaseVisualElement, IPressable
    {
#if ENABLE_RUNTIME_DATA_BINDINGS
        internal static readonly BindingId emojiProperty = nameof(emoji);

        internal static readonly BindingId countProperty = nameof(count);

        internal static readonly BindingId isOwnReactionProperty = nameof(isOwnReaction);

        internal static readonly BindingId clickableProperty = nameof(clickable);
#endif

        /// <summary>
        /// The ThreadReaction main styling class.
        /// </summary>
        public const string ussClassName = "appui-thread-reaction";

        /// <summary>
        /// The ThreadReaction emoji styling class.
        /// </summary>
        public const string emojiUssClassName = ussClassName + "__emoji";

        /// <summary>
        /// The ThreadReaction count styling class.
        /// </summary>
        public const string countUssClassName = ussClassName + "__count";

        readonly LocalizedTextElement m_EmojiElement;

        readonly LocalizedTextElement m_CountElement;

        string m_Emoji;

        int m_Count;

        Pressable m_Clickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThreadReaction() : this(null) { }

        /// <summary>
        /// Construct a <see cref="ThreadReaction"/> with a given click event callback.
        /// </summary>
        /// <param name="clickEvent">The given click event callback.</param>
        public ThreadReaction(Action clickEvent)
        {
            AddToClassList(ussClassName);
            AddToClassList(ActionButton.ussClassName);
            AddToClassList(ActionButton.GetSizeUssClassName(Size.M));
            AddToClassList(ActionButton.quietUssClassName);

            clickable = new Pressable(clickEvent);
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            m_EmojiElement = new LocalizedTextElement { name = emojiUssClassName, pickingMode = PickingMode.Ignore, enableRichText = true };
            m_EmojiElement.AddToClassList(ActionButton.labelUssClassName);
            m_EmojiElement.AddToClassList(emojiUssClassName);
            hierarchy.Add(m_EmojiElement);

            m_CountElement = new LocalizedTextElement { name = countUssClassName, pickingMode = PickingMode.Ignore };
            m_CountElement.AddToClassList(ActionButton.labelUssClassName);
            m_CountElement.AddToClassList(countUssClassName);
            hierarchy.Add(m_CountElement);

            emoji = null;
            count = 0;
            isOwnReaction = false;
        }

        /// <summary>
        /// Clickable Manipulator for this ThreadReaction.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                var changed = value != m_Clickable;
                if (m_Clickable != null)
                {
                    if (m_Clickable.target == this)
                        this.RemoveManipulator(m_Clickable);
                }
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
        /// The ThreadReaction click event.
        /// </summary>
        public event Action clicked
        {
            add => clickable.clicked += value;
            remove => clickable.clicked -= value;
        }

        /// <summary>
        /// The content container of the ThreadReaction.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The emoji string to display.
        /// </summary>
        [Tooltip("The emoji string to display.")]
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
        [Header("Thread Reaction")]
#endif
        public string emoji
        {
            get => m_Emoji;
            set
            {
                var changed = m_Emoji != value;
                m_Emoji = value;
                m_EmojiElement.text = m_Emoji;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in emojiProperty);
#endif
            }
        }

        /// <summary>
        /// The reaction count.
        /// </summary>
        [Tooltip("The reaction count.")]
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int count
        {
            get => m_Count;
            set
            {
                var changed = m_Count != value;
                m_Count = value;
                m_CountElement.text = m_Count.ToString();
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in countProperty);
#endif
            }
        }

        /// <summary>
        /// Whether this reaction belongs to the current user.
        /// </summary>
        [Tooltip("Whether this reaction belongs to the current user.")]
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool isOwnReaction
        {
            get => ClassListContains(Styles.selectedUssClassName);
            set
            {
                var changed = ClassListContains(Styles.selectedUssClassName) != value;
                EnableInClassList(Styles.selectedUssClassName, value);
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in isOwnReactionProperty);
#endif
            }
        }

#if ENABLE_UXML_TRAITS
        /// <summary>
        /// Defines the UxmlFactory for the ThreadReaction.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<ThreadReaction, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="ThreadReaction"/>.
        /// </summary>
        public new class UxmlTraits : BaseVisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Emoji = new UxmlStringAttributeDescription
            {
                name = "emoji",
                defaultValue = null
            };

            readonly UxmlIntAttributeDescription m_Count = new UxmlIntAttributeDescription
            {
                name = "count",
                defaultValue = 0
            };

            readonly UxmlBoolAttributeDescription m_IsOwnReaction = new UxmlBoolAttributeDescription
            {
                name = "is-own-reaction",
                defaultValue = false,
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

                var el = (ThreadReaction)ve;
                el.emoji = m_Emoji.GetValueFromBag(bag, cc);
                el.count = m_Count.GetValueFromBag(bag, cc);
                el.isOwnReaction = m_IsOwnReaction.GetValueFromBag(bag, cc);
            }
        }
#endif
    }
}
