using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The Avatar variant context.
    /// </summary>
    /// <param name="variant"> The Avatar variant.</param>
    public record AvatarVariantContext(AvatarVariant variant) : IContext
    {
        /// <summary>
        /// The Avatar variant.
        /// </summary>
        public AvatarVariant variant { get; } = variant;
    }

    /// <summary>
    /// The component size context.
    /// </summary>
    /// <param name="size"> The component size.</param>
    public record SizeContext(Size size) : IContext
    {
        /// <summary>
        /// The component size.
        /// </summary>
        public Size size { get; } = size;
    }

    /// <summary>
    /// The AvatarGroup spacing.
    /// </summary>
    public enum AvatarGroupSpacing
    {
        /// <summary>
        /// Small spacing.
        /// </summary>
        S,

        /// <summary>
        /// Medium spacing.
        /// </summary>
        M,

        /// <summary>
        /// Large spacing.
        /// </summary>
        L,

        /// <summary>
        /// No overlap between avatars.
        /// </summary>
        NoOverlap,
    }

    /// <summary>
    /// Groups multiple avatars together in a horizontal layout with configurable spacing and overflow handling.
    /// </summary>
    /// <remarks>
    /// The AvatarGroup component is used to group multiple Avatar components together. It's particularly useful
    /// for displaying a list of users, team members, or any collection of entities represented by avatars.
    ///
    /// The component automatically handles overflow by showing a configurable number of avatars and collapsing
    /// the rest into a '+N' indicator.
    ///
    /// Note: For optimal visual presentation, it's recommended to keep the max property value between 4 and 6
    /// avatars.
    /// </remarks>
    /// <example>
    /// <para>Basic usage with different variants. Shows 5 circular avatars with medium spacing and '+1' overflow
    /// indicator.</para>
    /// <code lang="xml"><![CDATA[
    /// <AvatarGroup spacing="M" max="5" variant="Circular">
    ///     <Avatar src="user1.png" />
    ///     <Avatar src="user2.png" />
    ///     <Avatar src="user3.png" />
    ///     <Avatar src="user4.png" />
    ///     <Avatar src="user5.png" />
    ///     <Avatar src="user6.png" />
    /// </AvatarGroup>
    /// ]]></code>
    /// <para>Data binding example. Dynamically create avatars from a data source with custom binding logic.</para>
    /// <code lang="csharp"><![CDATA[
    /// var avatarGroup = new AvatarGroup();
    /// avatarGroup.sourceItems = usersList;
    /// avatarGroup.max = 4;
    /// avatarGroup.spacing = AvatarGroupSpacing.S;
    /// avatarGroup.bindItem = (avatar, index) => {
    ///     var user = usersList[index];
    ///     avatar.src = user.avatarUrl;
    ///     avatar.backgroundColor = user.color;
    /// };
    /// ]]></code>
    /// <para>Custom surplus rendering. Customize the appearance of the overflow indicator.</para>
    /// <code lang="csharp"><![CDATA[
    /// avatarGroup.renderSurplus = (surplusCount) => {
    ///     var surplusAvatar = new Avatar();
    ///     surplusAvatar.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
    ///     var label = new Text($"+{surplusCount} more");
    ///     label.style.color = Color.white;
    ///     surplusAvatar.Add(label);
    ///     return surplusAvatar;
    /// };
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("feedbacks")]
    public partial class AvatarGroup : BaseVisualElement
    {
        internal static readonly BindingId maxProperty = nameof(max);

        internal static readonly BindingId spacingProperty = nameof(spacing);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId variantProperty = nameof(variant);

        internal static readonly BindingId totalProperty = nameof(total);

        internal static readonly BindingId sourceItemsProperty = nameof(sourceItems);

        internal static readonly BindingId bindItemProperty = nameof(bindItem);

        internal static readonly BindingId makeItemProperty = nameof(makeItem);
        /// <summary>
        /// The render surplus delegate.
        /// </summary>
        /// <param name="surplusCount">The number of surplus avatars.</param>
        /// <returns>The VisualElement to render.</returns>
        public delegate VisualElement RenderSurplusDelegate(int surplusCount);

        /// <summary>
        /// The AvatarGroup main styling class.
        /// </summary>
        public const string ussClassName = "appui-avatar-group";

        /// <summary>
        /// The AvatarGroup surplus styling class.
        /// </summary>
        public const string surplusUssClassName = ussClassName + "__surplus";

        /// <summary>
        /// The AvatarGroup spacing styling class.
        /// </summary>
        [EnumName("GetSpacingUssClassName", typeof(AvatarGroupSpacing))]
        public const string spacingUssClassName = ussClassName + "--spacing-";

        const int k_DefaultMax = 5;

        const AvatarGroupSpacing k_DefaultSpacing = AvatarGroupSpacing.M;

        const Size k_DefaultSize = Size.M;

        const AvatarVariant k_DefaultVariant = AvatarVariant.Circular;

        int? m_Total;

        AvatarVariant m_Variant;

        IList m_SourceItems;

        Action<Avatar, int> m_BindItem;

        Func<Avatar> m_MakeItem;

        Size m_Size;

        AvatarGroupSpacing m_Spacing;

        int m_Max = k_DefaultMax;

        /// <summary>
        /// The AvatarGroup content container.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// The maximum number of avatars to display before the overflow.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        [Header("Avatar Group")]
        public int max
        {
            get => m_Max;
            set
            {
                var changed = m_Max != value;
                m_Max = value;
                if (changed)
                    NotifyPropertyChanged(in maxProperty);
            }
        }

        /// <summary>
        /// The custom render function for the surplus avatars.
        /// </summary>
        public RenderSurplusDelegate renderSurplus { get; set; } = GetDefaultSurplusElement;

        /// <summary>
        /// The spacing between avatars.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public AvatarGroupSpacing spacing
        {
            get => m_Spacing;
            set
            {
                var changed = m_Spacing != value;
                RemoveFromClassList(GetSpacingUssClassName(m_Spacing));
                m_Spacing = value;
                AddToClassList(GetSpacingUssClassName(m_Spacing));
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in spacingProperty);
            }
        }

        /// <summary>
        /// The size of avatars.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                m_Size = value;
                this.ProvideContext(new SizeContext(value));
                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The AvatarGroup total count.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public int total => m_Total ?? m_SourceItems?.Count ?? 0;

        /// <summary>
        /// The AvatarGroup variant.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public AvatarVariant variant
        {
            get => m_Variant;
            set
            {
                var changed = m_Variant != value;
                m_Variant = value;
                this.ProvideContext(new AvatarVariantContext(value));
                if (changed)
                    NotifyPropertyChanged(in variantProperty);
            }
        }

        /// <summary>
        /// Set a custom AvatarGroup total count instead of the child count.
        /// </summary>
        /// <param name="customTotal"> The custom total count.</param>
        /// <remarks>
        /// You can pass a null value to reset the custom total count and use the child count instead.
        /// </remarks>
        public void SetCustomTotal(int? customTotal)
        {
            var previousTotal = total;
            m_Total = customTotal;
            Refresh();
            if (previousTotal != total)
                NotifyPropertyChanged(in totalProperty);
        }

        /// <summary>
        /// The collection of items that will be displayed as Radio component.
        /// </summary>
        [CreateProperty]
        public IList sourceItems
        {
            get => m_SourceItems;
            set
            {
                var changed = m_SourceItems != value;
                var previousTotal = total;
                m_SourceItems = value;
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in sourceItemsProperty);
                if (previousTotal != total)
                    NotifyPropertyChanged(in totalProperty);
            }
        }

        /// <summary>
        /// Method used to bind an item to a child Avatar.
        /// </summary>
        [CreateProperty]
        public Action<Avatar, int> bindItem
        {
            get => m_BindItem;
            set
            {
                var changed = m_BindItem != value;
                m_BindItem = value;
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in bindItemProperty);
            }
        }

        /// <summary>
        /// Method used to create an Avatar instance.
        /// </summary>
        [CreateProperty]
        public Func<Avatar> makeItem
        {
            get => m_MakeItem;
            set
            {
                var changed = m_MakeItem != value;
                m_MakeItem = value;
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in makeItemProperty);
            }
        }

        /// <summary>
        /// Defines the AvatarGroup constructor.
        /// </summary>
        public AvatarGroup()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(ussClassName);

            max = k_DefaultMax;
            variant = k_DefaultVariant;
            size = k_DefaultSize;
            spacing = k_DefaultSpacing;
        }

        /// <summary>
        /// Refresh the AvatarGroup.
        /// </summary>
        public void Refresh()
        {
            var currentTotal = total;

            if (m_SourceItems == null || currentTotal == 0)
            {
                Clear();
                return;
            }

            var displayCount = Mathf.Min(currentTotal, max);
            var hasSurplus = currentTotal > max;

            // Collect existing avatars and find the current surplus element.
            var existingAvatars = new List<Avatar>();
            VisualElement existingSurplus = null;
            for (var c = childCount - 1; c >= 0; c--)
            {
                var child = ElementAt(c);
                if (child is Avatar avatar)
                    existingAvatars.Add(avatar);
                else if (child.ClassListContains(surplusUssClassName))
                    existingSurplus = child;
            }

            // Handle surplus element.
            if (hasSurplus)
            {
                var surplusCount = currentTotal - max;

                // Remove old surplus and create a new one since the count may have changed.
                if (existingSurplus != null)
                    Remove(existingSurplus);

                var surplus = renderSurplus?.Invoke(surplusCount);
                if (surplus != null)
                {
                    surplus.AddToClassList(surplusUssClassName);
                    Insert(0, surplus);
                }
            }
            else if (existingSurplus != null)
            {
                Remove(existingSurplus);
            }

            // Remove excess avatars.
            while (existingAvatars.Count > displayCount)
            {
                var avatarToRemove = existingAvatars[existingAvatars.Count - 1];
                existingAvatars.RemoveAt(existingAvatars.Count - 1);
                Remove(avatarToRemove);
            }

            // Add missing avatars.
            while (existingAvatars.Count < displayCount)
            {
                var avatar = m_MakeItem?.Invoke() ?? new Avatar();
                Add(avatar);
                existingAvatars.Add(avatar);
            }

            // Rebind all visible avatars. Avatars are displayed in reverse order.
            for (var i = 0; i < displayCount; i++)
            {
                var avatar = existingAvatars[displayCount - 1 - i];
                bindItem?.Invoke(avatar, i);
            }
        }

        /// <summary>
        /// The default surplus VisualElement.
        /// </summary>
        /// <param name="surplus"> The number of surplus avatars.</param>
        /// <returns> The default surplus VisualElement.</returns>
        public static VisualElement GetDefaultSurplusElement(int surplus)
        {
            var avatar = new Avatar
            {
                backgroundColor = Color.gray
            };
            avatar.label = $"+{surplus}";

            return avatar;
        }

    }
}
