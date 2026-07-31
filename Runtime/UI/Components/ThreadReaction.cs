using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// ThreadReaction UI element. Displays an emoji reaction with a count.
    /// </summary>
    [UxmlElement]
    public partial class ThreadReaction : BaseVisualElement, IPressable
    {
        internal static readonly BindingId emojiProperty = nameof(emoji);

        internal static readonly BindingId countProperty = nameof(count);

        internal static readonly BindingId isOwnReactionProperty = nameof(isOwnReaction);

        internal static readonly BindingId clickableProperty = nameof(clickable);

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
        [CreateProperty]
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
                if (changed)
                    NotifyPropertyChanged(in clickableProperty);
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
        [CreateProperty]
        [UxmlAttribute]
        [Header("Thread Reaction")]
        public string emoji
        {
            get => m_Emoji;
            set
            {
                var changed = m_Emoji != value;
                m_Emoji = value;
                m_EmojiElement.text = m_Emoji;
                if (changed)
                    NotifyPropertyChanged(in emojiProperty);
            }
        }

        /// <summary>
        /// The reaction count.
        /// </summary>
        [Tooltip("The reaction count.")]
        [CreateProperty]
        [UxmlAttribute]
        public int count
        {
            get => m_Count;
            set
            {
                var changed = m_Count != value;
                m_Count = value;
                m_CountElement.text = m_Count.ToString();
                if (changed)
                    NotifyPropertyChanged(in countProperty);
            }
        }

        /// <summary>
        /// Whether this reaction belongs to the current user.
        /// </summary>
        [Tooltip("Whether this reaction belongs to the current user.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool isOwnReaction
        {
            get => ClassListContains(Styles.selectedUssClassName);
            set
            {
                var changed = ClassListContains(Styles.selectedUssClassName) != value;
                EnableInClassList(Styles.selectedUssClassName, value);
                if (changed)
                    NotifyPropertyChanged(in isOwnReactionProperty);
            }
        }

    }
}
