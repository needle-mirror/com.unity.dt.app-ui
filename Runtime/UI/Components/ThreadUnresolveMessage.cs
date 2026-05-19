using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// ThreadUnresolveMessage UI element. Displays a system message indicating a thread was unresolved.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class ThreadUnresolveMessage : BaseVisualElement
    {
#if ENABLE_RUNTIME_DATA_BINDINGS
        internal static readonly BindingId authorNameProperty = nameof(authorName);

        internal static readonly BindingId timestampProperty = nameof(timestamp);
#endif

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
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
        [Header("Thread Unresolve Message")]
#endif
        public string authorName
        {
            get => m_AuthorName;
            set
            {
                var changed = m_AuthorName != value;
                m_AuthorName = value;
                RefreshText();
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in authorNameProperty);
#endif
            }
        }

        /// <summary>
        /// The timestamp of when the thread was unresolved.
        /// </summary>
        [Tooltip("The timestamp of when the thread was unresolved.")]
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string timestamp
        {
            get => m_Timestamp;
            set
            {
                var changed = m_Timestamp != value;
                m_Timestamp = value;
                m_TimestampElement.text = m_Timestamp;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in timestampProperty);
#endif
            }
        }

        void RefreshText()
        {
            m_TextElement.text = string.IsNullOrEmpty(m_AuthorName)
                ? "unresolved this thread"
                : $"{m_AuthorName} unresolved this thread";
        }

#if ENABLE_UXML_TRAITS
        /// <summary>
        /// Defines the UxmlFactory for the ThreadUnresolveMessage.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<ThreadUnresolveMessage, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="ThreadUnresolveMessage"/>.
        /// </summary>
        public new class UxmlTraits : BaseVisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_AuthorName = new UxmlStringAttributeDescription
            {
                name = "author-name",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Timestamp = new UxmlStringAttributeDescription
            {
                name = "timestamp",
                defaultValue = null
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

                var el = (ThreadUnresolveMessage)ve;
                el.authorName = m_AuthorName.GetValueFromBag(bag, cc);
                el.timestamp = m_Timestamp.GetValueFromBag(bag, cc);
            }
        }
#endif
    }
}
