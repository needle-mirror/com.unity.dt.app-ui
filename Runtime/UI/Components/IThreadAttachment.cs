namespace Unity.AppUI.UI
{
    /// <summary>
    /// Interface for custom attachments that can be added to a <see cref="ThreadMessage"/>.
    /// </summary>
    public interface IThreadAttachment
    {
        /// <summary>
        /// The name of the attachment.
        /// </summary>
        string attachmentName { get; }

        /// <summary>
        /// The icon name for the attachment.
        /// </summary>
        string attachmentIcon { get; }
    }
}
