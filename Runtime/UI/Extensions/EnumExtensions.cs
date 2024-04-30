using System;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Extension methods for enums.
    /// </summary>
    public static partial class EnumExtensions
    {
        /// <summary>
        /// Converts a UnityEngine.UIElements.Align enum value to a string.
        /// </summary>
        /// <param name="enumValue"> The enum value to convert. </param>
        /// <returns> The string representation of the enum value. </returns>
        internal static string ToLowerCase(this UnityEngine.UIElements.Align enumValue)
        {
            return enumValue switch
            {
                UnityEngine.UIElements.Align.Auto => "auto",
                UnityEngine.UIElements.Align.FlexStart => "flexstart",
                UnityEngine.UIElements.Align.Center => "center",
                UnityEngine.UIElements.Align.FlexEnd => "flexend",
                UnityEngine.UIElements.Align.Stretch => "stretch",
                _ => throw new ArgumentOutOfRangeException(nameof(enumValue), enumValue, null)
            };
        }
    }
}
