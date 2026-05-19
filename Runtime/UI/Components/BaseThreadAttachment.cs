using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Abstract base class for custom thread attachments.
    /// </summary>
    public abstract partial class BaseThreadAttachment : BaseVisualElement, IThreadAttachment
    {
        /// <summary>
        /// The BaseThreadAttachment main styling class.
        /// </summary>
        public const string ussClassName = "appui-thread-attachment";

        /// <summary>
        /// The BaseThreadAttachment icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The BaseThreadAttachment name label styling class.
        /// </summary>
        public const string nameLabelUssClassName = ussClassName + "__name";

        readonly Icon m_IconElement;
        readonly LocalizedTextElement m_NameElement;

        /// <inheritdoc/>
        public abstract string attachmentName { get; }

        /// <inheritdoc/>
        public abstract string attachmentIcon { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected BaseThreadAttachment()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            focusable = false;

            m_IconElement = new Icon { name = iconUssClassName, pickingMode = PickingMode.Ignore };
            m_IconElement.AddToClassList(iconUssClassName);
            hierarchy.Add(m_IconElement);

            m_NameElement = new LocalizedTextElement { name = nameLabelUssClassName, pickingMode = PickingMode.Ignore };
            m_NameElement.AddToClassList(nameLabelUssClassName);
            hierarchy.Add(m_NameElement);
        }

        /// <summary>
        /// Refreshes the attachment display with current values.
        /// </summary>
        protected void RefreshAttachment()
        {
            m_IconElement.iconName = attachmentIcon;
            m_NameElement.text = attachmentName;
        }
    }
}
