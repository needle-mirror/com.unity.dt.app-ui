using System;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The sizing of UI components.
    /// </summary>
    public enum Size
    {
        /// <summary>
        /// Small
        /// </summary>
        Sm = 1,

        /// <summary>
        /// Medium. This is the default size for controls.
        /// </summary>
        Md,

        /// <summary>
        /// Large
        /// </summary>
        Lg,

        /// <summary>
        /// Small. Obsolete name of <see cref="Sm"/>.
        /// </summary>
        [Obsolete("Use Size.Sm instead.")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        S = Sm,

        /// <summary>
        /// Medium. Obsolete name of <see cref="Md"/>.
        /// </summary>
        [Obsolete("Use Size.Md instead.")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        M = Md,

        /// <summary>
        /// Large. Obsolete name of <see cref="Lg"/>.
        /// </summary>
        [Obsolete("Use Size.Lg instead.")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        L = Lg,
    }

    /// <summary>
    /// The spacing of UI components.
    /// </summary>
    public enum Spacing
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Small
        /// </summary>
        S,

        /// <summary>
        /// Medium
        /// </summary>
        M,

        /// <summary>
        /// Large
        /// </summary>
        L
    }

    /// <summary>
    /// General usage styling classes for UI components.
    /// </summary>
    public static class Styles
    {
        /// <summary>
        /// The delay between Animation Refresh calls.
        /// </summary>
        public const uint animationRefreshDelayMs = 16;

        /// <summary>
        /// The styling class used to hide an element completely.
        /// </summary>
        public const string hiddenUssClassName = "unity-hidden";

        /// <summary>
        /// The styling class used to set an "invalid" pseudo-state on a element.
        /// </summary>
        public const string invalidUssClassName = "is-invalid";

        /// <summary>
        /// The styling class used to set a "checked" pseudo-state on a element.
        /// </summary>
        public const string checkedUssClassName = "is-checked";

        /// <summary>
        /// The styling class used to set an "intermediate" pseudo-state on a element.
        /// </summary>
        public const string intermediateUssClassName = "is-intermediate";

        /// <summary>
        /// The styling class used to set an "open" pseudo-state on a element.
        /// </summary>
        public const string openUssClassName = "is-open";

        /// <summary>
        /// The styling class used to qualify an element as animated.
        /// </summary>
        public const string animatedClassName = "with-animations";

        /// <summary>
        /// The styling class used to start animating in an element.
        /// </summary>
        public const string animateInUssClassName = "animate-in";

        /// <summary>
        /// The styling class used to set a "selected" pseudo-state on a element.
        /// </summary>
        public const string selectedUssClassName = "is-selected";

        /// <summary>
        /// The styling class used to set an "active" pseudo-state on a element.
        /// </summary>
        public const string activeUssClassName = "is-active";

        /// <summary>
        /// The styling class used to set a "hover" pseudo-state on a element.
        /// </summary>
        public const string hoveredUssClassName = "is-hovered";

        /// <summary>
        /// The styling class used to set an element's elevation.
        /// </summary>
        public const string elevationUssClassName = "appui-elevation-";

        /// <summary>
        /// The styling class used to set a ":first-child" pseudo-state on a element.
        /// </summary>
        public const string firstChildUssClassName = "unity-first-child";

        /// <summary>
        /// The styling class used to set a ":last-child" pseudo-state on a element.
        /// </summary>
        public const string lastChildUssClassName = "unity-last-child";

        /// <summary>
        /// The styling class used to set a "focus" pseudo-state on a element.
        /// </summary>
        public const string focusedUssClassName = "is-focused";

        /// <summary>
        /// The styling class used to set a "required" pseudo-state on a element.
        /// </summary>
        public const string requiredUssClassName = "is-required";

        /// <summary>
        /// Used in popups to hide the arrow/tip.
        /// </summary>
        public const string noArrowUssClassName = "no-arrow";

        /// <summary>
        /// The styling class prefix used to set a cursor.
        /// </summary>
        public const string cursorUsClassNamePrefix = "cursor--";

        /// <summary>
        /// The styling class used to set a "keyboard-focus" pseudo-state on a element.
        /// </summary>
        public const string keyboardFocusUssClassName = "keyboard-focus";

        /// <summary>
        /// The lowercase USS class suffix for a <see cref="Size"/>: <c>sm</c>, <c>md</c> or <c>lg</c>.
        /// </summary>
        /// <remarks>
        /// Use this instead of <c>ToString()</c>. <see cref="Size"/> keeps obsolete aliases that share
        /// values with the current members, so <c>ToString()</c> may return either name.
        /// </remarks>
        /// <param name="size"> The size to convert. </param>
        /// <returns> The USS class suffix, or the numeric value for an undefined <see cref="Size"/>. </returns>
        internal static string ToUssSuffix(this Size size)
        {
            switch (size)
            {
                case Size.Sm:
                    return "sm";
                case Size.Md:
                    return "md";
                case Size.Lg:
                    return "lg";
                default:
                    // An unset field holds (Size)0, which callers remove before the first assignment.
                    return ((int)size).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Converts a <see cref="Size"/> to a <see cref="IconSize"/>.
        /// </summary>
        /// <param name="size"> The size to convert. </param>
        /// <returns> The converted <see cref="IconSize"/>. </returns>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when an unknown <see cref="Size"/> is provided. </exception>
        public static IconSize ToIconSize(this Size size)
        {
            switch (size)
            {
                case Size.Sm:
                    return IconSize.Sm;
                case Size.Md:
                    return IconSize.Md;
                case Size.Lg:
                    return IconSize.Lg;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, null);
            }
        }

        /// <summary>
        /// Converts a <see cref="IconSize"/> to a <see cref="Size"/>.
        /// </summary>
        /// <remarks>
        /// Only <see cref="IconSize.Sm"/>, <see cref="IconSize.Md"/> and <see cref="IconSize.Lg"/> have a
        /// control-size counterpart. <see cref="IconSize.Xxs"/> and <see cref="IconSize.Xs"/> are glyph-only
        /// steps of the icon ramp, so this method throws for them rather than silently rounding up to
        /// <see cref="Size.Sm"/> and returning a control that is visibly the wrong size.
        /// </remarks>
        /// <param name="size"> The size to convert. </param>
        /// <returns> The converted <see cref="Size"/>. </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when an unknown <see cref="IconSize"/> is provided, or when it is
        /// <see cref="IconSize.Xxs"/> or <see cref="IconSize.Xs"/>, which have no control-size equivalent.
        /// </exception>
        public static Size ToSize(this IconSize size)
        {
            switch (size)
            {
                case IconSize.Sm:
                    return Size.Sm;
                case IconSize.Md:
                    return Size.Md;
                case IconSize.Lg:
                    return Size.Lg;
                case IconSize.Xxs:
                case IconSize.Xs:
                    throw new ArgumentOutOfRangeException(nameof(size), size,
                        $"{size} is a glyph-only icon size and has no {nameof(Size)} equivalent.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, null);
            }
        }
    }
}
