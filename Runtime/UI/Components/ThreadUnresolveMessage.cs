using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// ThreadUnresolveMessage UI element. Displays a system message indicating a thread was unresolved.
    /// </summary>
    [UxmlElement]
    public partial class ThreadUnresolveMessage : BaseVisualElement
    {
        internal static readonly BindingId authorNameProperty = nameof(authorName);

        internal static readonly BindingId timestampProperty = nameof(timestamp);

        /// <summary>
        /// The ThreadUnresolveMessage main styling class.
        /// </summary>
        public const string ussClassName = "appui-thread-unresolve-message";

        /// <summary>
        /// The ThreadUnresolveMessage icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The ThreadUnresolveMessage text styling class.
        /// </summary>
        public const string textUssClassName = ussClassName + "__text";

        /// <summary>
        /// The ThreadUnresolveMessage timestamp styling class.
        /// </summary>
        public const string timestampUssClassName = ussClassName + "__timestamp";

        readonly Icon m_IconElement;

        readonly LocalizedTextElement m_TextElement;

        readonly LocalizedTextElement m_TimestampElement;

        string m_AuthorName;

        string m_Timestamp;

        /// <summary>
        /// The content container of the ThreadUnresolveMessage.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThreadUnresolveMessage()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;
            focusable = false;

            m_IconElement = new Icon { name = iconUssClassName, iconName = "x-circle", pickingMode = PickingMode.Ignore };
            m_IconElement.AddToClassList(iconUssClassName);
            hierarchy.Add(m_IconElement);

            var content = new VisualElement {pickingMode = PickingMode.Ignore};
            hierarchy.Add(content);

            m_TextElement = new LocalizedTextElement { name = textUssClassName, pickingMode = PickingMode.Ignore };
            m_TextElement.AddToClassList(textUssClassName);
            content.Add(m_TextElement);

            m_TimestampElement = new LocalizedTextElement { name = timestampUssClassName, pickingMode = PickingMode.Ignore };
            m_TimestampElement.AddToClassList(timestampUssClassName);
            content.Add(m_TimestampElement);

            authorName = null;
            timestamp = null;
        }

        /// <summary>
        /// The name of the author who unresolved the thread.
        /// </summary>
        [Tooltip("The name of the author who unresolved the thread.")]
        [CreateProperty]
        [UxmlAttribute]
        [Header("Thread Unresolve Message")]
        public string authorName
        {
            get => m_AuthorName;
            set
            {
                var changed = m_AuthorName != value;
                m_AuthorName = value;
                RefreshText();
                if (changed)
                    NotifyPropertyChanged(in authorNameProperty);
            }
        }

        /// <summary>
        /// The timestamp of when the thread was unresolved.
        /// </summary>
        [Tooltip("The timestamp of when the thread was unresolved.")]
        [CreateProperty]
        [UxmlAttribute]
        public string timestamp
        {
            get => m_Timestamp;
            set
            {
                var changed = m_Timestamp != value;
                m_Timestamp = value;
                m_TimestampElement.text = m_Timestamp;
                if (changed)
                    NotifyPropertyChanged(in timestampProperty);
            }
        }

        void RefreshText()
        {
            m_TextElement.text = string.IsNullOrEmpty(m_AuthorName)
                ? "unresolved this thread"
                : $"{m_AuthorName} unresolved this thread";
        }

    }
}
