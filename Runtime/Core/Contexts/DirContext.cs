namespace Unity.AppUI.Core
{
    /// <summary>
    /// The layout direction of the application.
    /// </summary>
    public enum Dir
    {
        /// <summary>
        /// Left to right.
        /// </summary>
        Ltr,

        /// <summary>
        /// Right to left.
        /// </summary>
        Rtl
    }

    /// <summary>
    /// The layout direction context of the application.
    /// </summary>
    public record DirContext(Dir dir) : IContext
    {
        /// <summary>
        /// The current layout direction.
        /// </summary>
        public Dir dir { get; } = dir;
    }
}
