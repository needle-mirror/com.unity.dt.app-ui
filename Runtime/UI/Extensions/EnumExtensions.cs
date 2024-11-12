using System;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Methods to extend the functionality of enums.
    /// </summary>
    public static partial class EnumExtensions
    {
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
