namespace Unity.AppUI.Navigation
{
    /// <summary>
    /// Representation of an entry in the back stack of a <see cref="NavController"/>.
    /// </summary>
    public record NavBackStackEntry(NavDestination destination, NavOptions options, Argument[] arguments)
    {
        /// <summary>
        /// The destination associated with this entry.
        /// </summary>
        public NavDestination destination { get; } = destination;

        /// <summary>
        /// The options associated with this entry.
        /// </summary>
        public NavOptions options { get; } = options;
        
        /// <summary>
        /// The arguments associated with this entry.
        /// </summary>
        public Argument[] arguments { get; } = arguments;
    }
}
