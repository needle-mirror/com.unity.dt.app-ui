using System;
using UnityEngine;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// A struct representing a color entry in a gradient.
    /// </summary>
    public readonly struct ColorEntry : IComparable, IEquatable<ColorEntry>
    {
        /// <summary>
        /// The color of the entry.
        /// </summary>
        public readonly Color color;

        /// <summary>
        /// The position of the entry in the gradient.
        /// </summary>
        public readonly float position;

        /// <summary>
        /// Creates a new ColorEntry.
        /// </summary>
        /// <param name="color">The color of the entry.</param>
        /// <param name="position">The position of the entry in the gradient.</param>
        public ColorEntry(Color color, float position)
        {
            this.color = color;
            this.position = position;
        }

        /// <summary>
        /// Returns a string representation of the ColorEntry.
        /// </summary>
        /// <returns>A string representation of the ColorEntry.</returns>
        public override string ToString() => $"Color Entry : position {position} / value {color}";

        /// <summary>
        /// Compares this ColorEntry to another object.
        /// </summary>
        /// <param name="other">The other object to compare to.</param>
        /// <returns>1 if the other object is not a ColorEntry, otherwise the result of comparing the position of the two ColorEntries.</returns>
        public int CompareTo(object other)
        {
            if (!(other is ColorEntry))
                return 1;
            var otherEntry = (ColorEntry)other;
            return position.CompareTo(otherEntry.position);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        public bool Equals(ColorEntry other)
        {
            return color.Equals(other.color) && position.Equals(other.position);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        public override bool Equals(object obj)
        {
            return obj is ColorEntry other && Equals(other);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(color, position);
        }
    }
}
