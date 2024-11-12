using System;
using Unity.AppUI.UI;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// The TooltipPlacement context of the application.
    /// </summary>
    /// <param name="placement"> The new placement of the tooltip.</param>
    public record TooltipPlacementContext(PopoverPlacement placement) : IContext
    {
        /// <summary>
        /// The current placement of the tooltip.
        /// </summary>
        /// <value> The current placement of the tooltip.</value>
        public PopoverPlacement placement { get; } = placement;
    }
}
