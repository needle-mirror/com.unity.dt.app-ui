using System.Globalization;
using UnityEngine;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Extension methods for the <see cref="Rect"/> class.
    /// </summary>
    public static class RectExtensions
    {
        /// <summary>
        /// Check if a given <see cref="Rect"/> has a valid width and height.
        /// </summary>
        /// <param name="rect">The given <see cref="Rect"/> object.</param>
        /// <returns>True if its width and height are valid, False otherwise.</returns>
        public static bool IsValid(this Rect rect)
        {
            // Compare against zero directly. Mathf.Approximately(0, x) is not safe here: it
            // tolerates Mathf.Epsilon * 8, and on some engine builds that evaluates to 0, which makes
            // Approximately(0, 0) false and lets a zero-area rect through as valid.
            // NaN fails both '> 0' checks and negatives and zero are rejected by them.
            return rect != default &&
                rect.width > 0f && rect.height > 0f &&
                !float.IsInfinity(rect.width) && !float.IsInfinity(rect.height);
        }

        /// <summary>
        /// Check if a given <see cref="Rect"/> has a valid width and height to be used as Texture size.
        /// </summary>
        /// <param name="rect">The given <see cref="Rect"/> object.</param>
        /// <returns>True if its width and height are valid, False otherwise.</returns>
        public static bool IsValidForTextureSize(this Rect rect)
        {
            return rect.IsValid() && rect.size.IsValidForTextureSize();
        }

        /// <summary>
        /// Check if a given <see cref="Vector2"/> has a valid value to be used as Texture size.
        /// </summary>
        /// <param name="vec">The given <see cref="Vector2"/> object.</param>
        /// <returns>True if its width and height are valid, False otherwise.</returns>
        public static bool IsValidForTextureSize(this Vector2 vec)
        {
            return vec.x is >= 1 and <= 4096 && vec.y is >= 1 and <= 4096;
        }

        /// <summary>
        /// Try to parse a <see cref="Rect"/> from a string.
        /// </summary>
        /// <param name="str"> The string to parse.</param>
        /// <param name="rect"> The parsed <see cref="Rect"/>.</param>
        /// <returns> True if the string was successfully parsed, False otherwise.</returns>
        public static bool TryParse(string str, out Rect rect)
        {
            rect = default;
            var parts = str.Split(',');
            if (parts.Length != 4)
            {
                return false;
            }

            if (!float.TryParse(parts[0], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var x) ||
                !float.TryParse(parts[1], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var y) ||
                !float.TryParse(parts[2], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var w) ||
                !float.TryParse(parts[3], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var h))
            {
                return false;
            }

            rect = new Rect(x, y, w, h);

            return true;
        }

        /// <summary>
        /// Check if two <see cref="Rect"/> objects are approximately equal.
        /// </summary>
        /// <param name="rect1"> The first <see cref="Rect"/> object.</param>
        /// <param name="rect2"> The second <see cref="Rect"/> object.</param>
        /// <returns> True if the two <see cref="Rect"/> objects are approximately equal, False otherwise.</returns>
        public static bool Approximately(Rect rect1, Rect rect2)
        {
            return Mathf.Approximately(rect1.x, rect2.x) &&
                Mathf.Approximately(rect1.y, rect2.y) &&
                Mathf.Approximately(rect1.width, rect2.width) &&
                Mathf.Approximately(rect1.height, rect2.height);
        }
    }
}
