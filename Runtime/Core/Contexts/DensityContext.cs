using System;
using UnityEngine;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// The Density context of the application.
    /// </summary>
    /// <remarks>
    /// Density is the design-system sizing axis. Valid values are <c>normal</c>, <c>compact</c>
    /// and <c>comfort</c>, matching the density token sets shipped by <c>@unity/app-ui-tokens</c>.
    /// It is unrelated to <see cref="Platform.scaleFactor"/>, which is the native display DPI.
    /// </remarks>
    /// <param name="density"> The density. </param>
    public record DensityContext(string density) : IContext
    {
        /// <summary>
        /// The current density.
        /// </summary>
        public string density { get; } = density;
    }
}
