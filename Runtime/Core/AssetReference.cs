using System;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// A object representing an asset for the AssetTargetField.
    /// </summary>
    /// <remarks>
    /// This is the value type of <see cref="Unity.AppUI.UI.AssetTargetField"/>. It is a lightweight,
    /// serializable descriptor rather than a live object handle, so the same field works in a player
    /// build, in the Editor, and against a remote asset catalogue.
    /// </remarks>
    [Serializable]
    public class AssetReference
    {
        /// <summary>
        /// The name of the asset.
        /// </summary>
        public string name;

        /// <summary>
        /// The type of the asset.
        /// </summary>
        public string type;

        /// <summary>
        /// The type icon of the asset.
        /// </summary>
        public string typeIcon;
    }
}
