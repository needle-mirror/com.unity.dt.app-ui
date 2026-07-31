using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Unity.AppUI.Editor
{
    // Generic Converters

    /// <summary>
    /// Base UXML attribute converter for <see cref="Optional{T}"/> values.
    /// </summary>
    /// <remarks>
    /// An empty or unparsable attribute value is converted to <see cref="Optional{T}.none"/>,
    /// so an attribute that is absent from the UXML behaves the same as one left blank.
    /// </remarks>
    /// <typeparam name="T"> The type of the value wrapped by the <see cref="Optional{T}"/>. </typeparam>
    public abstract class OptionalConverter<T> : UxmlAttributeConverter<Optional<T>>
    {
        /// <summary>
        /// Tries to parse the string representation of the wrapped value.
        /// </summary>
        /// <param name="value"> The string to parse. </param>
        /// <param name="v"> The parsed value, or the default value of <typeparamref name="T"/> if parsing failed. </param>
        /// <returns> True if the value could be parsed, False otherwise. </returns>
        protected virtual bool TryParse(string value, out T v)
        {
            v = default;
            return false;
        }

        /// <summary>
        /// Converts a UXML attribute value into an <see cref="Optional{T}"/>.
        /// </summary>
        /// <param name="value"> The attribute value to convert. </param>
        /// <returns> The converted value, or <see cref="Optional{T}.none"/> if it is empty or cannot be parsed. </returns>
        public override Optional<T> FromString(string value)
        {
            if (string.IsNullOrEmpty(value) || !TryParse(value, out var v))
                return Optional<T>.none;

            return v;
        }

        /// <summary>
        /// Converts an <see cref="Optional{T}"/> into its UXML attribute representation.
        /// </summary>
        /// <param name="value"> The value to convert. </param>
        /// <returns> The string representation of the wrapped value, or an empty string if no value is set. </returns>
        public override string ToString(Optional<T> value)
        {
            return value.IsSet ? value.Value.ToString() : string.Empty;
        }
    }

    /// <summary>
    /// UXML attribute converter for <see cref="Optional{T}"/> values whose wrapped type is a comparable,
    /// formattable value type.
    /// </summary>
    /// <typeparam name="T"> The type of the value wrapped by the <see cref="Optional{T}"/>. </typeparam>
    public class OptionalValueConverter<T> : OptionalConverter<T>
        where T : IComparable, IComparable<T>, IFormattable { }

    /// <summary>
    /// Base UXML attribute converter for <see cref="OptionalEnum{T}"/> values.
    /// </summary>
    /// <remarks>
    /// An empty or unrecognized attribute value is converted to <see cref="OptionalEnum{T}.none"/>.
    /// </remarks>
    /// <typeparam name="T"> The enum type wrapped by the <see cref="OptionalEnum{T}"/>. </typeparam>
    public abstract class OptionalEnumConverter<T> : UxmlAttributeConverter<OptionalEnum<T>>
        where T : struct, Enum
    {
        /// <summary>
        /// Converts a UXML attribute value into an <see cref="OptionalEnum{T}"/>.
        /// </summary>
        /// <param name="value"> The attribute value to convert. </param>
        /// <returns> The converted value, or <see cref="OptionalEnum{T}.none"/> if it is empty or not a member of <typeparamref name="T"/>. </returns>
        public override OptionalEnum<T> FromString(string value)
        {
            if (string.IsNullOrEmpty(value) || !Enum.TryParse<T>(value, out var v))
                return OptionalEnum<T>.none;

            return v;
        }

        /// <summary>
        /// Converts an <see cref="OptionalEnum{T}"/> into its UXML attribute representation.
        /// </summary>
        /// <param name="value"> The value to convert. </param>
        /// <returns> The name of the enum member, or an empty string if no value is set. </returns>
        public override string ToString(OptionalEnum<T> value)
        {
            return value.IsSet ? value.Value.ToString() : string.Empty;
        }
    }

    // Specific Converters

    /// <summary>
    /// UXML attribute converter for optional <see cref="PopoverPlacement"/> values.
    /// </summary>
    public class OptionalPopoverPlacementConverter : OptionalEnumConverter<PopoverPlacement> { }

    /// <summary>
    /// UXML attribute converter for optional <see cref="Dir"/> values.
    /// </summary>
    public class OptionalDirConverter : OptionalEnumConverter<Dir> { }

    /// <summary>
    /// UXML attribute converter for optional <see cref="int"/> values.
    /// </summary>
    public class OptionalIntConverter : OptionalValueConverter<int>
    {
        /// <summary>
        /// Tries to parse the attribute value as an <see cref="int"/>.
        /// </summary>
        /// <param name="value"> The string to parse. </param>
        /// <param name="v"> The parsed value, or zero if parsing failed. </param>
        /// <returns> True if the value could be parsed, False otherwise. </returns>
        protected override bool TryParse(string value, out int v)
        {
            return int.TryParse(value, out v);
        }
    }

    /// <summary>
    /// UXML attribute converter for optional <see cref="long"/> values.
    /// </summary>
    public class OptionalLongConverter : OptionalValueConverter<long>
    {
        /// <summary>
        /// Tries to parse the attribute value as a <see cref="long"/>.
        /// </summary>
        /// <param name="value"> The string to parse. </param>
        /// <param name="v"> The parsed value, or zero if parsing failed. </param>
        /// <returns> True if the value could be parsed, False otherwise. </returns>
        protected override bool TryParse(string value, out long v)
        {
            return long.TryParse(value, out v);
        }
    }

    /// <summary>
    /// UXML attribute converter for optional <see cref="float"/> values.
    /// </summary>
    public class OptionalFloatConverter : OptionalValueConverter<float>
    {
        /// <summary>
        /// Tries to parse the attribute value as a <see cref="float"/>.
        /// </summary>
        /// <param name="value"> The string to parse. </param>
        /// <param name="v"> The parsed value, or zero if parsing failed. </param>
        /// <returns> True if the value could be parsed, False otherwise. </returns>
        protected override bool TryParse(string value, out float v)
        {
            return float.TryParse(value, out v);
        }
    }

    /// <summary>
    /// UXML attribute converter for optional <see cref="double"/> values.
    /// </summary>
    public class OptionalDoubleConverter : OptionalValueConverter<double>
    {
        /// <summary>
        /// Tries to parse the attribute value as a <see cref="double"/>.
        /// </summary>
        /// <param name="value"> The string to parse. </param>
        /// <param name="v"> The parsed value, or zero if parsing failed. </param>
        /// <returns> True if the value could be parsed, False otherwise. </returns>
        protected override bool TryParse(string value, out double v)
        {
            return double.TryParse(value, out v);
        }
    }

    /// <summary>
    /// UXML attribute converter for optional <see cref="string"/> values.
    /// </summary>
    public class OptionalStringConverter : OptionalConverter<string>
    {
        /// <summary>
        /// Tries to parse the attribute value as a <see cref="string"/>.
        /// </summary>
        /// <param name="value"> The string to parse. </param>
        /// <param name="v"> The attribute value, unchanged. </param>
        /// <returns> True if the value is neither null nor empty, False otherwise. </returns>
        protected override bool TryParse(string value, out string v)
        {
            v = value;
            return !string.IsNullOrEmpty(value);
        }
    }

    /// <summary>
    /// UXML attribute converter for optional <see cref="Color"/> values.
    /// </summary>
    public class OptionalColorConverter : OptionalConverter<Color>
    {
        /// <summary>
        /// Tries to parse the attribute value as an HTML color string.
        /// </summary>
        /// <param name="value"> The string to parse, such as <c>#RRGGBB</c> or a color name. </param>
        /// <param name="v"> The parsed color, or the default color if parsing failed. </param>
        /// <returns> True if the value could be parsed, False otherwise. </returns>
        protected override bool TryParse(string value, out Color v)
        {
            return ColorUtility.TryParseHtmlString(value, out v);
        }
    }

    /// <summary>
    /// UXML attribute converter for optional <see cref="Rect"/> values.
    /// </summary>
    public class OptionalRectConverter : OptionalConverter<Rect>
    {
        /// <summary>
        /// Tries to parse the attribute value as a <see cref="Rect"/>.
        /// </summary>
        /// <param name="value"> The string to parse, in the form <c>x,y,width,height</c>. </param>
        /// <param name="v"> The parsed rectangle, or the default rectangle if parsing failed. </param>
        /// <returns> True if the value could be parsed, False otherwise. </returns>
        protected override bool TryParse(string value, out Rect v)
        {
            return RectExtensions.TryParse(value, out v);
        }

        /// <summary>
        /// Converts an optional <see cref="Rect"/> into its UXML attribute representation.
        /// </summary>
        /// <param name="value"> The value to convert. </param>
        /// <returns> The rectangle formatted as <c>x,y,width,height</c>, or an empty string if no value is set. </returns>
        public override string ToString(Optional<Rect> value)
        {
            if (!value.IsSet)
                return string.Empty;

            var rect = value.Value;
            return $"{rect.x},{rect.y},{rect.width},{rect.height}";
        }
    }

    /// <summary>
    /// UXML attribute converter for <see cref="Date"/> values.
    /// </summary>
    public class DateConverter : UxmlAttributeConverter<Date>
    {
        /// <summary>
        /// Converts a UXML attribute value into a <see cref="Date"/>.
        /// </summary>
        /// <param name="value"> The attribute value to convert. </param>
        /// <returns> The converted date, or the default date if the value is empty or cannot be parsed. </returns>
        public override Date FromString(string value)
        {
            if (string.IsNullOrEmpty(value) || !DateTime.TryParse(value, out var v))
                return default;

            return new Date(v);
        }

        /// <summary>
        /// Converts a <see cref="Date"/> into its UXML attribute representation.
        /// </summary>
        /// <param name="value"> The date to convert. </param>
        /// <returns> The date formatted using the invariant culture. </returns>
        public override string ToString(Date value)
        {
            return ((DateTime)value).ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// UXML attribute converter for <see cref="DateRange"/> values.
    /// </summary>
    public class DateRangeConverter : UxmlAttributeConverter<DateRange>
    {
        /// <summary>
        /// Converts a UXML attribute value into a <see cref="DateRange"/>.
        /// </summary>
        /// <param name="value"> The attribute value to convert. </param>
        /// <returns> The converted range, or the default range if the value is empty or cannot be parsed. </returns>
        public override DateRange FromString(string value)
        {
            if (string.IsNullOrEmpty(value) || !DateRange.TryParse(value, out var v))
                return default;

            return v;
        }

        /// <summary>
        /// Converts a <see cref="DateRange"/> into its UXML attribute representation.
        /// </summary>
        /// <param name="value"> The range to convert. </param>
        /// <returns> The string representation of the range. </returns>
        public override string ToString(DateRange value)
        {
            return value.ToString();
        }
    }

}
