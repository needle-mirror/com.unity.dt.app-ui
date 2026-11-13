using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Sizing values for <see cref="Icon"/> elements.
    /// </summary>
    public enum IconSize
    {
        /// <summary>
        /// Extra extra small. A glyph-only step, bound to <c>--appui-alias-control-sizing-icon-xxs</c>.
        /// It has no <see cref="Size"/> counterpart.
        /// </summary>
        Xxs,

        /// <summary>
        /// Extra small. A glyph-only step, bound to <c>--appui-alias-control-sizing-icon-xs</c>.
        /// It has no <see cref="Size"/> counterpart.
        /// </summary>
        Xs,

        /// <summary>
        /// Small. Matches <see cref="Size.Sm"/>.
        /// </summary>
        Sm,

        /// <summary>
        /// Medium. Matches <see cref="Size.Md"/>. This is the default size for icons.
        /// </summary>
        Md,

        /// <summary>
        /// Large. Matches <see cref="Size.Lg"/>.
        /// </summary>
        Lg
    }

    /// <summary>
    /// Variant values for <see cref="Icon"/> elements.
    /// </summary>
    [GenerateLowerCaseStrings]
    public enum IconVariant
    {
        /// <summary>
        /// Regular
        /// </summary>
        Regular = 1,

        /// <summary>
        /// Bold
        /// </summary>
        Bold,

        /// <summary>
        /// DuoTone
        /// </summary>
        DuoTone,

        /// <summary>
        /// Light
        /// </summary>
        Light,

        /// <summary>
        /// Fill
        /// </summary>
        Fill,

        /// <summary>
        /// Thin
        /// </summary>
        Thin
    }

    /// <summary>
    /// A versatile icon component that displays graphical symbols with customizable styles and sizes.
    /// </summary>
    /// <remarks>
    /// The Icon component is a fundamental UI element that represents graphical symbols or pictograms in your
    /// application. It extends the Image component and provides additional functionality for displaying icons
    /// with various styles, sizes, and variants.
    ///
    /// Icons are essential for creating intuitive user interfaces, helping users quickly identify actions, states,
    /// or categories within your application. The Icon component supports different variants (Regular, Bold,
    /// DuoTone, Light, Fill, and Thin) and sizes (Xxs to Lg) to accommodate various design needs.
    ///
    /// By default, Icons are non-focusable and ignore picking mode, making them ideal for decorative purposes
    /// while maintaining optimal performance.
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("iconography", id = "icon-component", displayName = "Icon Component")]
    public partial class Icon : Image
    {

        internal static readonly BindingId iconNameProperty = new BindingId(nameof(iconName));

        internal static readonly BindingId primaryProperty = new BindingId(nameof(primary));

        internal static readonly BindingId sizeProperty = new BindingId(nameof(size));

        internal static readonly BindingId variantProperty = new BindingId(nameof(variant));


        /// <summary>
        /// The Icon main styling class.
        /// </summary>
        public new const string ussClassName = "appui-icon";

        /// <summary>
        /// The Icon primary variant styling class.
        /// </summary>
        public const string primaryUssClassName = ussClassName + "--primary";

        /// <summary>
        /// The Icon size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(IconSize))]
        public const string sizeUssClassName = ussClassName + "--size-";

        string m_IconName;

        IconSize m_Size;

        IconVariant m_Variant = IconVariant.Regular;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Icon()
        {
            AddToClassList(ussClassName);

            focusable = false;
            pickingMode = PickingMode.Ignore;
            usageHints |= UsageHints.DynamicColor;

            iconName = "info";
            size = IconSize.Md;
            primary = false;
            scaleMode = ScaleMode.ScaleToFit;
        }

        /// <summary>
        /// The primary variant of the Icon.
        /// </summary>
        /// <remarks>
        /// `false` by default, matching the registry's `primary = false`: a bare icon takes the muted
        /// `--appui-foregrounds-50` tint and blends with the surrounding text. Set it to `true` to
        /// promote the icon to `--appui-foregrounds-100`, which is what every icon a component builds
        /// for itself does.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool primary
        {
            get => ClassListContains(primaryUssClassName);
            set
            {
                var changed = ClassListContains(primaryUssClassName) != value;
                EnableInClassList(primaryUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in primaryProperty);
            }
        }

        /// <summary>
        /// The size of the Icon.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public IconSize size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The name of the Icon.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string iconName
        {
            get => m_IconName;
            set
            {
                var changed = m_IconName != value;
                RemoveFromClassList(MemoryUtils.Concatenate(ussClassName, "--", m_IconName, "--", m_Variant.ToLowerCase()));
                RemoveFromClassList(MemoryUtils.Concatenate(ussClassName, "--", m_IconName));
                m_IconName = value;
                AddToClassList(MemoryUtils.Concatenate(ussClassName, "--", m_IconName, "--", m_Variant.ToLowerCase()));
                AddToClassList(MemoryUtils.Concatenate(ussClassName, "--", m_IconName));

                if (changed)
                    NotifyPropertyChanged(in iconNameProperty);
            }
        }

        /// <summary>
        /// The variant of the Icon.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public IconVariant variant
        {
            get => m_Variant;
            set
            {
                var changed = m_Variant != value;
                RemoveFromClassList(MemoryUtils.Concatenate(ussClassName, "--", m_IconName, "--", m_Variant.ToLowerCase()));
                RemoveFromClassList(MemoryUtils.Concatenate(ussClassName, "--", m_IconName));
                m_Variant = value;
                AddToClassList(MemoryUtils.Concatenate(ussClassName, "--", m_IconName, "--", m_Variant.ToLowerCase()));
                AddToClassList(MemoryUtils.Concatenate(ussClassName, "--", m_IconName));

                if (changed)
                    NotifyPropertyChanged(in variantProperty);
            }
        }

    }
}
