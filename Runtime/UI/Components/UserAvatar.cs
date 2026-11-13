using System;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The minimum a UI needs to draw a person.
    /// </summary>
    /// <remarks>
    /// Modelled after the registry's `UserIdentity`: an <see cref="id"/> that is always present, plus a
    /// <see cref="name"/>, <see cref="portrait"/> and <see cref="color"/> that may still be resolving.
    /// An initials avatar can render from <see cref="id"/> and <see cref="color"/> alone, before a
    /// directory lookup returns the display name.
    ///
    /// <see cref="color"/> is a colour the *host* supplies, typically derived from a stable hash of the
    /// person so they keep one colour everywhere, including outside UI Toolkit. It deliberately does not
    /// come from the theme: it identifies a person, not a surface.
    /// </remarks>
    public struct UserIdentity : IEquatable<UserIdentity>
    {
        /// <summary>
        /// A stable identifier for the person. Always present.
        /// </summary>
        public string id;

        /// <summary>
        /// The display name, if it has resolved yet. Drives the initials.
        /// </summary>
        public string name;

        /// <summary>
        /// The profile image, if the person has one. Takes precedence over the initials.
        /// </summary>
        public Background portrait;

        /// <summary>
        /// The person's identity colour, used as the initials backdrop. Not a theme colour.
        /// </summary>
        public Optional<Color> color;

        /// <summary>
        /// Compares two identities field by field.
        /// </summary>
        /// <param name="other"> The other identity. </param>
        /// <returns> True if every field matches. </returns>
        public bool Equals(UserIdentity other) =>
            id == other.id && name == other.name && portrait == other.portrait && color.Equals(other.color);

        /// <summary>
        /// Compares this identity with an arbitrary object.
        /// </summary>
        /// <param name="obj"> The object to compare with. </param>
        /// <returns> True if <paramref name="obj"/> is an identical <see cref="UserIdentity"/>. </returns>
        public override bool Equals(object obj) => obj is UserIdentity other && Equals(other);

        /// <summary>
        /// Returns a hash code for this identity.
        /// </summary>
        /// <returns> A hash code. </returns>
        public override int GetHashCode() => HashCode.Combine(id, name, portrait, color);
    }

    /// <summary>
    /// The one avatar for a person: account menus, content authors and live-collaboration presence.
    /// </summary>
    /// <remarks>
    /// UserAvatar is an <see cref="Avatar"/> that knows about people. It renders the profile image when
    /// <see cref="UserIdentity.portrait"/> is set, otherwise the name's initials on the identity's
    /// <see cref="UserIdentity.color"/>, so a person keeps one colour everywhere. It falls back to a
    /// neutral `?` when the identity has no name yet.
    ///
    /// The initials colour is derived from the backdrop's own luminance rather than from the theme —
    /// that is <see cref="Avatar.autoLabelColor"/>, which UserAvatar turns on. A themed foreground would
    /// vanish on some inputs: a pale identity colour in a light theme, a dark one in a dark theme.
    ///
    /// With <see cref="ring"/> the avatar gets a thin neutral outline that thickens and turns the
    /// person's colour on hover — the presence affordance (*who is this?*), reusable by an
    /// <see cref="AvatarGroup"/> of live collaborators. The neutral outline and its hover thickness are
    /// USS, on tokens; only the person's colour comes from C#, because it is data rather than design.
    ///
    /// An avatar shows only initials or an image, so set `tooltip` (typically the person's name) to
    /// surface who it is on hover, and `preferredTooltipPlacement` to steer it — the App UI tooltip,
    /// never an unstyled native one.
    ///
    /// Everything else — <see cref="Avatar.size"/>, <see cref="Avatar.variant"/> — is inherited, so a
    /// <see cref="AvatarGroup"/> and a <see cref="SizeContext"/> drive a UserAvatar exactly as they
    /// drive a plain <see cref="Avatar"/>.
    /// </remarks>
    /// <example>
    /// <para>A presence avatar with a ring and a tooltip.</para>
    /// <code lang="csharp"><![CDATA[
    /// var avatar = new UserAvatar
    /// {
    ///     identity = new UserIdentity
    ///     {
    ///         id = "u-42",
    ///         name = "Ada Lovelace",
    ///         color = new Color(0.05f, 0.43f, 0.78f),
    ///     },
    ///     ring = true,
    ///     size = Size.Lg,
    ///     tooltip = "Ada Lovelace",
    /// };
    /// ]]></code>
    /// <para>UXML declaration — the identity is spelled out attribute by attribute.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:UserAvatar user-id="u-42" user-name="Ada Lovelace" user-color="#0A6EC8" ring="true" size="Lg" />
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("feedbacks", isNew = true)]
    public partial class UserAvatar : Avatar
    {
        internal static readonly BindingId identityProperty = nameof(identity);

        internal static readonly BindingId userIdProperty = nameof(userId);

        internal static readonly BindingId userNameProperty = nameof(userName);

        internal static readonly BindingId userColorProperty = nameof(userColor);

        internal static readonly BindingId ringProperty = nameof(ring);

        /// <summary>
        /// The UserAvatar main styling class.
        /// </summary>
        public new const string ussClassName = "appui-useravatar";

        /// <summary>
        /// The styling class carried while <see cref="ring"/> is enabled.
        /// </summary>
        public const string ringUssClassName = ussClassName + "--ring";

        /// <summary>
        /// The label shown when the identity has no name yet.
        /// </summary>
        public const string unknownLabel = "?";

        UserIdentity m_Identity;

        bool m_Ring;

        bool m_Hovered;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UserAvatar()
        {
            AddToClassList(ussClassName);

            // Avatar's constructor writes an inline 2px border width. Clearing it hands the outline
            // back to USS, which is where the ring's token-driven widths live.
            outlineWidth = Optional<float>.none;
            autoLabelColor = true;

            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);

            identity = default;
            ring = false;
        }

        /// <summary>
        /// The person this avatar stands for.
        /// </summary>
        /// <remarks>
        /// Setting it rewrites the initials, the backdrop colour and the portrait in one go. The
        /// individual <see cref="userId"/>, <see cref="userName"/> and <see cref="userColor"/>
        /// properties are the UXML-friendly view of the same state.
        /// </remarks>
        [CreateProperty]
        public UserIdentity identity
        {
            get => m_Identity;
            set
            {
                var changed = !m_Identity.Equals(value);
                m_Identity = value;
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in identityProperty);
            }
        }

        /// <summary>
        /// The person's stable identifier.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("user-id")]
        [Header("User Avatar")]
        public string userId
        {
            get => m_Identity.id;
            set
            {
                var changed = m_Identity.id != value;
                m_Identity.id = value;
                if (changed)
                    NotifyPropertyChanged(in userIdProperty);
            }
        }

        /// <summary>
        /// The person's display name. Drives the initials, and is `null` until it resolves.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("user-name")]
        public string userName
        {
            get => m_Identity.name;
            set
            {
                var changed = m_Identity.name != value;
                m_Identity.name = value;
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in userNameProperty);
            }
        }

        /// <summary>
        /// The person's identity colour, used as the initials backdrop.
        /// </summary>
        /// <remarks>
        /// Not a theme colour: it identifies a person, not a surface. Leave it unset to keep the
        /// component's default surface styling.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute("user-color")]
        public Optional<Color> userColor
        {
            get => m_Identity.color;
            set
            {
                var changed = !m_Identity.color.Equals(value);
                m_Identity.color = value;
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in userColorProperty);
            }
        }

        /// <summary>
        /// The person's profile image. Takes precedence over the initials when set.
        /// </summary>
        [CreateProperty]
        public Background portrait
        {
            get => m_Identity.portrait;
            set
            {
                m_Identity.portrait = value;
                Refresh();
            }
        }

        /// <summary>
        /// Whether the avatar carries the presence ring.
        /// </summary>
        /// <remarks>
        /// A thin neutral outline that thickens on hover and adopts <see cref="userColor"/> while
        /// hovered. With no colour on the identity, the neutral token colour stays.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute("ring")]
        public bool ring
        {
            get => m_Ring;
            set
            {
                var changed = m_Ring != value;
                m_Ring = value;
                EnableInClassList(ringUssClassName, m_Ring);
                UpdateRingColor();
                if (changed)
                    NotifyPropertyChanged(in ringProperty);
            }
        }

        /// <summary>
        /// Two-letter initials from a display name, for an avatar fallback.
        /// </summary>
        /// <remarks>
        /// "Ada Lovelace" becomes "AL", "Madonna" becomes "MA", and an empty or whitespace-only name
        /// becomes an empty string, which the avatar then renders as <see cref="unknownLabel"/>.
        /// </remarks>
        /// <param name="name"> The display name. May be null. </param>
        /// <returns> The initials, in upper case. </returns>
        public static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var parts = name.Trim().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return string.Empty;
            if (parts.Length == 1)
                return (parts[0].Length >= 2 ? parts[0].Substring(0, 2) : parts[0]).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        void Refresh()
        {
            var initials = GetInitials(m_Identity.name);
            label = string.IsNullOrEmpty(initials) ? unknownLabel : initials;
            backgroundColor = m_Identity.color;
            src = m_Identity.portrait;
            UpdateRingColor();
        }

        void OnPointerEnter(PointerEnterEvent evt)
        {
            m_Hovered = true;
            UpdateRingColor();
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            m_Hovered = false;
            UpdateRingColor();
        }

        /// <summary>
        /// The ring's *colour* is the only part of it that C# owns: it is the person's identity colour,
        /// which is data. Its width, its neutral resting colour and its transition all stay in USS, on
        /// tokens. Clearing the inline colour lets the USS token win again.
        /// </summary>
        void UpdateRingColor()
        {
            var tint = m_Ring && m_Hovered && m_Identity.color.IsSet
                ? new StyleColor(m_Identity.color.Value)
                : new StyleColor(StyleKeyword.Null);

            style.borderTopColor = tint;
            style.borderBottomColor = tint;
            style.borderLeftColor = tint;
            style.borderRightColor = tint;
        }
    }
}
