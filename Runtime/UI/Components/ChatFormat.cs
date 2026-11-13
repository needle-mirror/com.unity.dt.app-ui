using System;
using System.Globalization;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The number and duration formatting the chat display components share.
    /// </summary>
    /// <remarks>
    /// These mirror <c>formatDuration</c> and <c>formatCompact</c> from the registry's
    /// <c>lib/utils.ts</c> exactly, so a UITK chat surface reads the same as the web one. They are
    /// culture invariant on purpose: the compact suffixes and the <c>h</c>/<c>m</c>/<c>s</c> units are
    /// not localized on the web side either, and a decimal comma would make "1.5k" unparseable to a
    /// snapshot test.
    /// </remarks>
    static class ChatFormat
    {
        /// <summary>
        /// An elapsed duration, in milliseconds, as a spoken duration: 6000 becomes "6s", 90000
        /// becomes "1m 30s".
        /// </summary>
        /// <param name="milliseconds"> The duration in milliseconds. </param>
        /// <returns> The formatted duration. </returns>
        public static string Duration(double milliseconds)
        {
            var totalSeconds = (long)Math.Round(milliseconds / 1000d, MidpointRounding.AwayFromZero);
            if (totalSeconds < 60)
                return totalSeconds.ToString(CultureInfo.InvariantCulture) + "s";

            var hours = totalSeconds / 3600;
            var minutes = totalSeconds % 3600 / 60;
            var seconds = totalSeconds % 60;

            var parts = string.Empty;
            if (hours > 0)
                parts = hours.ToString(CultureInfo.InvariantCulture) + "h";
            if (minutes > 0)
                parts = (parts.Length > 0 ? parts + " " : string.Empty) + minutes.ToString(CultureInfo.InvariantCulture) + "m";
            if (seconds > 0)
                parts = (parts.Length > 0 ? parts + " " : string.Empty) + seconds.ToString(CultureInfo.InvariantCulture) + "s";

            return parts.Length > 0 ? parts : "0s";
        }

        /// <summary>
        /// A count in compact notation: 999 stays "999", 1500 becomes "1.5k", 51000 becomes "51k",
        /// 2400000 becomes "2.4M".
        /// </summary>
        /// <param name="value"> The count to format. </param>
        /// <returns> The compact rendering. </returns>
        public static string Compact(double value)
        {
            if (value < 1000d)
                return ((long)value).ToString(CultureInfo.InvariantCulture);

            return value < 1000000d ? Scaled(value / 1000d, "k") : Scaled(value / 1000000d, "M");
        }


        /// <summary>
        /// A plain reading of a count: no decimals when the value is whole, at most two when it is
        /// not. Culture invariant, for the same reason the rest of this class is.
        /// </summary>
        /// <param name="value"> The value to render. </param>
        /// <returns> The plain rendering. </returns>
        public static string Plain(double value)
        {
            return Math.Abs(value - Math.Round(value)) < 0.0001d
                ? ((long)Math.Round(value)).ToString(CultureInfo.InvariantCulture)
                : value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The exact value with thousands separators, the equivalent of the web implementation's
        /// <c>toLocaleString()</c> in the tooltips that reveal a compacted number.
        /// </summary>
        /// <param name="value"> The value to render. </param>
        /// <returns> The grouped rendering. </returns>
        public static string Grouped(double value)
        {
            return Math.Abs(value - Math.Round(value)) < 0.0001d
                ? ((long)Math.Round(value)).ToString("N0", CultureInfo.InvariantCulture)
                : value.ToString("N2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// One significant decimal below 100, none at or above it, with a trailing ".0" dropped.
        /// </summary>
        static string Scaled(double value, string suffix)
        {
            if (value >= 100d)
                return ((long)Math.Round(value, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture) + suffix;

            var text = value.ToString("F1", CultureInfo.InvariantCulture);
            if (text.EndsWith(".0", StringComparison.Ordinal))
                text = text.Substring(0, text.Length - 2);

            return text + suffix;
        }
    }
}
