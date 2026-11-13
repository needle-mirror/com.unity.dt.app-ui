using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// One switchable account in a <see cref="UserSwitcher"/>.
    /// </summary>
    public class UserAccount
    {
        /// <summary>
        /// A stable identifier. This is what <see cref="UserSwitcher.accountSelected"/> reports.
        /// </summary>
        public string id;

        /// <summary>
        /// The display name, shown on the row and used for the initials.
        /// </summary>
        public string name;

        /// <summary>
        /// The account's email address, if there is one.
        /// </summary>
        public string email;

        /// <summary>
        /// The profile image. Falls back to the initials when unset.
        /// </summary>
        public Background portrait;

        /// <summary>
        /// The account's identity colour, backing the initials. Typically a stable hash of the person,
        /// so they keep one colour everywhere.
        /// </summary>
        public Optional<Color> color;

        /// <summary>
        /// When true the row cannot be selected: it is dimmed and shows a warning marker. Use it for
        /// accounts the user can see but not switch to, such as an org without the required licence.
        /// </summary>
        public bool disabled;

        /// <summary>
        /// Tooltip text explaining why the row is <see cref="disabled"/>.
        /// </summary>
        public string warning;
    }

    /// <summary>
    /// The profile featured as the trigger avatar and the menu header of a <see cref="UserSwitcher"/>.
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// The signed-in person's display name.
        /// </summary>
        public string name;

        /// <summary>
        /// Their email address.
        /// </summary>
        public string email;

        /// <summary>
        /// A secondary line under the email, typically the current organization.
        /// </summary>
        public string caption;

        /// <summary>
        /// Their profile image. Falls back to the initials when unset.
        /// </summary>
        public Background portrait;

        /// <summary>
        /// Their identity colour, backing the initials.
        /// </summary>
        public Optional<Color> color;
    }

    /// <summary>
    /// An avatar trigger that opens an account menu.
    /// </summary>
    /// <remarks>
    /// UserSwitcher is composed rather than invented: the trigger is a <see cref="UserAvatar"/> with an
    /// optional name and caret, and the popover is the ordinary menu family —
    /// <see cref="MenuBuilder"/> anchoring a <see cref="Menu"/> of <see cref="MenuItem"/> rows, grouped
    /// by a <see cref="MenuSection"/>. It therefore inherits the design system's overlay, dismissal and
    /// keyboard behaviour for free. It goes through <see cref="MenuBuilder"/> rather than
    /// <see cref="MenuTrigger"/> only because the builder is the half of the family that takes a
    /// <see cref="placement"/>, which is what the registry's `align` prop maps onto.
    ///
    /// A row's selected state is a real <see cref="MenuItem.selectable"/> item, so the checkmark, its
    /// alignment and its disabled treatment are the menu's, not a bespoke glyph. A
    /// <see cref="UserAccount.disabled"/> row is disabled outright and carries a warning marker plus the
    /// account's <see cref="UserAccount.warning"/> as a tooltip on the whole row, not just the marker.
    ///
    /// Optionally it features a <see cref="profile"/> header — avatar, name, email, caption — above the
    /// switchable list, and can collapse that list behind a single expander row labelled
    /// <see cref="sectionLabel"/>. Compose extra rows such as settings or sign-out by simply adding them
    /// to the UserSwitcher: its content container is the tail of the menu, after the generated cards.
    ///
    /// A long list scrolls rather than growing without bound. The cap is
    /// <see cref="maxVisibleAccounts"/> rows of the medium control height, measured off a hidden probe
    /// element sized from `--appui-alias-control-sizing-md-height` — UI Toolkit cannot read a `px`
    /// length back through a float custom property, and hard-coding the number would break density
    /// rescaling. Past that cap a <see cref="SearchBar"/> appears above the list to filter it.
    ///
    /// **Width is not a design token.** The registry's popover is `w-64` (256px), or `w-72` (288px) with
    /// a profile header, and the App UI sizing ramp tops out well below that. <see cref="menuWidth"/> is
    /// therefore a plain pixel property, defaulting to the registry's two widths, exactly as
    /// <see cref="Sidebar.width"/> is. Everything else is a token.
    /// </remarks>
    /// <example>
    /// <para>A titlebar switcher with a featured profile and an extra sign-out row.</para>
    /// <code lang="csharp"><![CDATA[
    /// var switcher = new UserSwitcher
    /// {
    ///     profile = new UserProfile { name = "Ada Lovelace", email = "ada@unity3d.com", caption = "Analytical Engines" },
    ///     accounts = new List<UserAccount>
    ///     {
    ///         new UserAccount { id = "personal", name = "Personal" },
    ///         new UserAccount { id = "engines", name = "Analytical Engines" },
    ///         new UserAccount { id = "locked", name = "Difference Engines", disabled = true, warning = "No seat on this org." },
    ///     },
    ///     activeAccountId = "engines",
    /// };
    /// switcher.accountSelected += id => Debug.Log($"switched to {id}");
    /// switcher.Add(new MenuItem { label = "Sign out", icon = "sign-out" });
    /// ]]></code>
    /// <para>UXML declaration — the accounts themselves are data, so they are set from C#.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:UserSwitcher show-name="true" section-label="Organizations" collapsible="true" size="Md" />
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("nav-components", isNew = true)]
    public partial class UserSwitcher : BaseVisualElement
    {
        internal static readonly BindingId activeAccountIdProperty = nameof(activeAccountId);

        internal static readonly BindingId showNameProperty = nameof(showName);

        internal static readonly BindingId triggerLabelProperty = nameof(triggerLabel);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId sectionLabelProperty = nameof(sectionLabel);

        internal static readonly BindingId collapsibleProperty = nameof(collapsible);

        internal static readonly BindingId maxVisibleAccountsProperty = nameof(maxVisibleAccounts);

        internal static readonly BindingId showAccountAvatarsProperty = nameof(showAccountAvatars);

        internal static readonly BindingId menuWidthProperty = nameof(menuWidth);

        internal static readonly BindingId placementProperty = nameof(placement);

        /// <summary>
        /// The UserSwitcher main styling class.
        /// </summary>
        public const string ussClassName = "appui-userswitcher";

        /// <summary>
        /// The styling class of the trigger.
        /// </summary>
        public const string triggerUssClassName = ussClassName + "__trigger";

        /// <summary>
        /// The styling class carried by the trigger while it also shows a name.
        /// </summary>
        public const string triggerWithNameUssClassName = triggerUssClassName + "--with-name";

        /// <summary>
        /// The styling class of the trigger's label.
        /// </summary>
        public const string triggerLabelUssClassName = ussClassName + "__trigger-label";

        /// <summary>
        /// The styling class of the trigger's caret.
        /// </summary>
        public const string caretUssClassName = ussClassName + "__caret";

        /// <summary>
        /// The styling class of the popover menu.
        /// </summary>
        public const string menuUssClassName = ussClassName + "__menu";

        /// <summary>
        /// The styling class of the profile header card.
        /// </summary>
        public const string profileUssClassName = ussClassName + "__profile";

        /// <summary>
        /// The styling class of the profile header's text column.
        /// </summary>
        public const string profileTextUssClassName = ussClassName + "__profile-text";

        /// <summary>
        /// The styling class of the profile header's name.
        /// </summary>
        public const string profileNameUssClassName = ussClassName + "__profile-name";

        /// <summary>
        /// The styling class of the profile header's email.
        /// </summary>
        public const string profileEmailUssClassName = ussClassName + "__profile-email";

        /// <summary>
        /// The styling class of the profile header's caption.
        /// </summary>
        public const string profileCaptionUssClassName = ussClassName + "__profile-caption";

        /// <summary>
        /// The styling class of the card holding the switchable list.
        /// </summary>
        public const string accountsUssClassName = ussClassName + "__accounts";

        /// <summary>
        /// The styling class of the scrolling list of account rows.
        /// </summary>
        public const string listUssClassName = ussClassName + "__list";

        /// <summary>
        /// The styling class of the search field's inset wrapper.
        /// </summary>
        public const string searchUssClassName = ussClassName + "__search";

        /// <summary>
        /// The styling class of the "no matches" message.
        /// </summary>
        public const string emptyUssClassName = ussClassName + "__empty";

        /// <summary>
        /// The styling class of an account row's avatar.
        /// </summary>
        public const string accountAvatarUssClassName = ussClassName + "__account-avatar";

        /// <summary>
        /// The styling class of a disabled account row's warning marker.
        /// </summary>
        public const string accountWarningUssClassName = ussClassName + "__account-warning";

        /// <summary>
        /// The styling class of the expander row shown while <see cref="collapsible"/> is set.
        /// </summary>
        public const string expanderUssClassName = ussClassName + "__expander";

        /// <summary>
        /// The styling class of the hidden element the switcher measures a row's height from.
        /// </summary>
        public const string rowProbeUssClassName = ussClassName + "__row-probe";

        /// <summary>
        /// The styling class of the container extra rows are appended to.
        /// </summary>
        public const string extraUssClassName = ussClassName + "__extra";

        /// <summary>
        /// The popover width, in pixels, with no profile header. The registry's `w-64`.
        /// </summary>
        public const float defaultMenuWidth = 256f;

        /// <summary>
        /// The popover width, in pixels, with a profile header. The registry's `w-72`.
        /// </summary>
        public const float defaultProfileMenuWidth = 288f;

        /// <summary>
        /// The default heading of the switchable list.
        /// </summary>
        public const string defaultSectionLabel = "Accounts";

        /// <summary>
        /// The message shown when the filter matches no account.
        /// </summary>
        public const string noMatchesLabel = "No matches.";

        const int k_DefaultMaxVisibleAccounts = 5;

        const PopoverPlacement k_DefaultPlacement = PopoverPlacement.BottomEnd;

        readonly VisualElement m_Trigger;

        readonly UserAvatar m_TriggerAvatar;

        readonly LocalizedTextElement m_TriggerLabel;

        readonly Icon m_Caret;

        readonly Menu m_Menu;

        readonly VisualElement m_ProfileCard;

        readonly UserAvatar m_ProfileAvatar;

        readonly LocalizedTextElement m_ProfileName;

        readonly LocalizedTextElement m_ProfileEmail;

        readonly LocalizedTextElement m_ProfileCaption;

        readonly VisualElement m_AccountsCard;

        readonly MenuItem m_Expander;

        readonly MenuSection m_Section;

        readonly VisualElement m_SearchWrapper;

        readonly SearchBar m_SearchBar;

        readonly ScrollView m_List;

        readonly LocalizedTextElement m_Empty;

        readonly VisualElement m_RowProbe;

        readonly VisualElement m_Extra;

        readonly List<UserAccount> m_Accounts = new List<UserAccount>();

        MenuBuilder m_Popover;

        UserProfile m_Profile;

        string m_ActiveAccountId;

        string m_TriggerLabelOverride;

        string m_SectionLabel = defaultSectionLabel;

        Size m_Size = Size.Md;

        bool m_ShowName;

        bool m_Collapsible;

        bool m_Expanded;

        bool m_ShowAccountAvatars = true;

        int m_MaxVisibleAccounts = k_DefaultMaxVisibleAccounts;

        Optional<float> m_MenuWidth;

        PopoverPlacement m_Placement = k_DefaultPlacement;

        float m_RowHeight;

        /// <summary>
        /// Raised with an account id when a row is chosen. A disabled row never raises it.
        /// </summary>
        public event Action<string> accountSelected;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UserSwitcher()
        {
            AddToClassList(ussClassName);

            m_TriggerAvatar = new UserAvatar { pickingMode = PickingMode.Ignore };
            m_TriggerLabel = new LocalizedTextElement { name = triggerLabelUssClassName, pickingMode = PickingMode.Ignore };
            m_TriggerLabel.AddToClassList(triggerLabelUssClassName);
            m_Caret = new Icon { iconName = "caret-up-down", name = caretUssClassName, pickingMode = PickingMode.Ignore };
            m_Caret.AddToClassList(caretUssClassName);

            m_Trigger = new VisualElement { name = triggerUssClassName };
            m_Trigger.AddToClassList(triggerUssClassName);
            m_Trigger.focusable = true;
            m_Trigger.tabIndex = 0;
            m_Trigger.pickingMode = PickingMode.Position;
            m_Trigger.AddManipulator(new Pressable(OpenMenu));
            m_Trigger.hierarchy.Add(m_TriggerAvatar);
            m_Trigger.hierarchy.Add(m_TriggerLabel);
            m_Trigger.hierarchy.Add(m_Caret);
            hierarchy.Add(m_Trigger);

            // The list cap is a token, and a token length cannot be read back through a float custom
            // property. A hidden probe sized from the alias can, and rescales with density on its own.
            m_RowProbe = new VisualElement { name = rowProbeUssClassName, pickingMode = PickingMode.Ignore };
            m_RowProbe.AddToClassList(rowProbeUssClassName);
            m_RowProbe.RegisterCallback<GeometryChangedEvent>(OnRowProbeResized);
            hierarchy.Add(m_RowProbe);

            m_ProfileAvatar = new UserAvatar { size = Size.Md, pickingMode = PickingMode.Ignore };
            m_ProfileName = new LocalizedTextElement { name = profileNameUssClassName, pickingMode = PickingMode.Ignore };
            m_ProfileName.AddToClassList(profileNameUssClassName);
            m_ProfileEmail = new LocalizedTextElement { name = profileEmailUssClassName, pickingMode = PickingMode.Ignore };
            m_ProfileEmail.AddToClassList(profileEmailUssClassName);
            m_ProfileCaption = new LocalizedTextElement { name = profileCaptionUssClassName, pickingMode = PickingMode.Ignore };
            m_ProfileCaption.AddToClassList(profileCaptionUssClassName);

            var profileText = new VisualElement { name = profileTextUssClassName, pickingMode = PickingMode.Ignore };
            profileText.AddToClassList(profileTextUssClassName);
            profileText.hierarchy.Add(m_ProfileName);
            profileText.hierarchy.Add(m_ProfileEmail);
            profileText.hierarchy.Add(m_ProfileCaption);

            m_ProfileCard = new VisualElement { name = profileUssClassName, pickingMode = PickingMode.Ignore };
            m_ProfileCard.AddToClassList(profileUssClassName);
            m_ProfileCard.hierarchy.Add(m_ProfileAvatar);
            m_ProfileCard.hierarchy.Add(profileText);

            m_SearchBar = new SearchBar { size = Size.Sm, placeholder = "Search" };
            m_SearchBar.RegisterValueChangedCallback(OnSearchChanged);
            m_SearchWrapper = new VisualElement { name = searchUssClassName };
            m_SearchWrapper.AddToClassList(searchUssClassName);
            m_SearchWrapper.hierarchy.Add(m_SearchBar);

            m_List = new ScrollView(ScrollViewMode.Vertical) { name = listUssClassName };
            m_List.AddToClassList(listUssClassName);

            m_Empty = new LocalizedTextElement(noMatchesLabel) { name = emptyUssClassName, pickingMode = PickingMode.Ignore };
            m_Empty.AddToClassList(emptyUssClassName);

            m_Expander = new MenuItem { name = expanderUssClassName, label = m_SectionLabel };
            m_Expander.AddToClassList(expanderUssClassName);
            m_Expander.clickable.clicked += ToggleExpanded;

            m_Section = new MenuSection { title = m_SectionLabel };

            m_AccountsCard = new VisualElement { name = accountsUssClassName };
            m_AccountsCard.AddToClassList(accountsUssClassName);

            m_Extra = new VisualElement { name = extraUssClassName };
            m_Extra.AddToClassList(extraUssClassName);

            m_Menu = new Menu { name = menuUssClassName };
            m_Menu.AddToClassList(menuUssClassName);

            showName = false;
            size = Size.Md;
            placement = k_DefaultPlacement;
            menuWidth = Optional<float>.none;

            RefreshTrigger();
            RefreshMenu();
        }

        /// <summary>
        /// Extra menu rows land after the generated cards — settings, sign out, and anything else.
        /// </summary>
        public override VisualElement contentContainer => m_Extra;

        /// <summary>
        /// The accounts available to switch between.
        /// </summary>
        /// <remarks>
        /// Assigning copies the sequence, so a later mutation of the caller's list does not silently
        /// desynchronise the menu. Call <see cref="Refresh"/> after mutating an account in place.
        /// </remarks>
        public IEnumerable<UserAccount> accounts
        {
            get => m_Accounts;
            set
            {
                m_Accounts.Clear();
                if (value != null)
                    m_Accounts.AddRange(value);
                RefreshTrigger();
                RefreshMenu();
            }
        }

        /// <summary>
        /// The profile to feature. When set, the trigger avatar reflects this person rather than the
        /// active account, and a header renders above the switchable list.
        /// </summary>
        public UserProfile profile
        {
            get => m_Profile;
            set
            {
                m_Profile = value;
                RefreshTrigger();
                RefreshMenu();
            }
        }

        /// <summary>
        /// The id of the active account. Defaults to the first account.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("active-account-id")]
        [Header("User Switcher")]
        public string activeAccountId
        {
            get => m_ActiveAccountId;
            set
            {
                var changed = m_ActiveAccountId != value;
                m_ActiveAccountId = value;
                RefreshTrigger();
                RefreshMenu();
                if (changed)
                    NotifyPropertyChanged(in activeAccountIdProperty);
            }
        }

        /// <summary>
        /// The account the switcher currently considers active, which is the first one when
        /// <see cref="activeAccountId"/> names none.
        /// </summary>
        public UserAccount activeAccount =>
            m_Accounts.FirstOrDefault(a => a.id == m_ActiveAccountId) ?? m_Accounts.FirstOrDefault();

        /// <summary>
        /// Whether the trigger shows the active name and a caret next to the avatar.
        /// </summary>
        /// <remarks>
        /// The sidebar or header style. Left false, the trigger is avatar-only, which suits a compact
        /// titlebar.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute("show-name")]
        public bool showName
        {
            get => m_ShowName;
            set
            {
                var changed = m_ShowName != value;
                m_ShowName = value;
                m_Trigger.EnableInClassList(triggerWithNameUssClassName, m_ShowName);
                m_TriggerLabel.style.display = m_ShowName ? DisplayStyle.Flex : DisplayStyle.None;
                m_Caret.style.display = m_ShowName ? DisplayStyle.Flex : DisplayStyle.None;
                if (changed)
                    NotifyPropertyChanged(in showNameProperty);
            }
        }

        /// <summary>
        /// Overrides the label shown in the trigger while <see cref="showName"/> is set.
        /// </summary>
        /// <remarks>
        /// Use it to label the trigger with something other than the active account or profile name —
        /// the current organization, say — while the menu header still features the signed-in person.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute("trigger-label")]
        public string triggerLabel
        {
            get => m_TriggerLabelOverride;
            set
            {
                var changed = m_TriggerLabelOverride != value;
                m_TriggerLabelOverride = value;
                RefreshTrigger();
                if (changed)
                    NotifyPropertyChanged(in triggerLabelProperty);
            }
        }

        /// <summary>
        /// The trigger avatar's size.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("size")]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                m_Size = value;
                m_TriggerAvatar.size = m_Size;
                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The heading of the switchable list, and the label of the expander when
        /// <see cref="collapsible"/> is set.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("section-label")]
        public string sectionLabel
        {
            get => m_SectionLabel;
            set
            {
                var changed = m_SectionLabel != value;
                m_SectionLabel = value;
                m_Section.title = value;
                m_Expander.label = value;
                if (changed)
                    NotifyPropertyChanged(in sectionLabelProperty);
            }
        }

        /// <summary>
        /// Whether the switchable list hides behind a single expander row.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("collapsible")]
        public bool collapsible
        {
            get => m_Collapsible;
            set
            {
                var changed = m_Collapsible != value;
                m_Collapsible = value;
                m_Expanded = false;
                RefreshMenu();
                if (changed)
                    NotifyPropertyChanged(in collapsibleProperty);
            }
        }

        /// <summary>
        /// Whether the collapsible list is currently open. Always true when
        /// <see cref="collapsible"/> is false, since the list is then never hidden.
        /// </summary>
        public bool expanded => !m_Collapsible || m_Expanded;

        /// <summary>
        /// How many account rows are visible before the list scrolls.
        /// </summary>
        /// <remarks>
        /// Past this count the list also gains a <see cref="SearchBar"/> to filter it, on the grounds
        /// that a list long enough to scroll is long enough to search.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute("max-visible-accounts")]
        public int maxVisibleAccounts
        {
            get => m_MaxVisibleAccounts;
            set
            {
                var clamped = Mathf.Max(1, value);
                var changed = m_MaxVisibleAccounts != clamped;
                m_MaxVisibleAccounts = clamped;
                ApplyListHeight();
                RefreshMenu();
                if (changed)
                    NotifyPropertyChanged(in maxVisibleAccountsProperty);
            }
        }

        /// <summary>
        /// Whether each account row carries an avatar. Turn it off for lists where an avatar adds no
        /// meaning, such as organizations.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("show-account-avatars")]
        public bool showAccountAvatars
        {
            get => m_ShowAccountAvatars;
            set
            {
                var changed = m_ShowAccountAvatars != value;
                m_ShowAccountAvatars = value;
                RefreshMenu();
                if (changed)
                    NotifyPropertyChanged(in showAccountAvatarsProperty);
            }
        }

        /// <summary>
        /// The popover width, in pixels. Unset picks the registry's default for the current shape:
        /// <see cref="defaultProfileMenuWidth"/> with a profile header, <see cref="defaultMenuWidth"/>
        /// without.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("menu-width")]
        public Optional<float> menuWidth
        {
            get => m_MenuWidth;
            set
            {
                var changed = !m_MenuWidth.Equals(value);
                m_MenuWidth = value;
                ApplyMenuWidth();
                if (changed)
                    NotifyPropertyChanged(in menuWidthProperty);
            }
        }

        /// <summary>
        /// Where the popover sits relative to the trigger. The registry's `align` prop.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("placement")]
        public PopoverPlacement placement
        {
            get => m_Placement;
            set
            {
                var changed = m_Placement != value;
                m_Placement = value;
                if (changed)
                    NotifyPropertyChanged(in placementProperty);
            }
        }

        /// <summary>
        /// The trigger element, so a caller can focus it or read its accessible tooltip.
        /// </summary>
        public VisualElement triggerElement => m_Trigger;

        /// <summary>
        /// The menu the switcher fills and shows.
        /// </summary>
        public Menu menu => m_Menu;

        /// <summary>
        /// Whether the list currently shows a search field, which happens once there are more accounts
        /// than <see cref="maxVisibleAccounts"/>.
        /// </summary>
        public bool searchVisible => m_Accounts.Count > m_MaxVisibleAccounts;

        /// <summary>
        /// The account rows currently in the menu, in order.
        /// </summary>
        public IEnumerable<MenuItem> accountRows => m_List.contentContainer.Children().OfType<MenuItem>();

        /// <summary>
        /// Open the account menu.
        /// </summary>
        public void OpenMenu()
        {
            m_Popover = MenuBuilder.Build(m_Trigger, m_Menu);
            m_Popover.SetPlacement(m_Placement);
            m_Popover.SetCloseOnSelection(true);
            m_Popover.dismissed += OnMenuDismissed;
            m_Popover.Show();
        }

        /// <summary>
        /// Dismiss the account menu, if it is open.
        /// </summary>
        public void CloseMenu()
        {
            m_Popover?.Dismiss();
        }

        /// <summary>
        /// Rebuild the trigger and the menu from the current accounts and profile.
        /// </summary>
        /// <remarks>
        /// Assigning <see cref="accounts"/> or <see cref="profile"/> already does this. Call it by hand
        /// after mutating an existing <see cref="UserAccount"/> in place.
        /// </remarks>
        public void Refresh()
        {
            RefreshTrigger();
            RefreshMenu();
        }

        void OnMenuDismissed(MenuBuilder builder, DismissType reason)
        {
            // Reopening should be clean: the expander closes and the filter clears.
            m_Expanded = false;
            m_SearchBar.SetValueWithoutNotify(string.Empty);
            RefreshMenu();
            m_Popover = null;
        }

        void ToggleExpanded()
        {
            m_Expanded = !m_Expanded;
            RefreshMenu();
        }

        void OnSearchChanged(ChangeEvent<string> evt)
        {
            RefreshRows();
        }

        void OnRowProbeResized(GeometryChangedEvent evt)
        {
            var height = m_RowProbe.resolvedStyle.height;
            if (Mathf.Approximately(height, m_RowHeight) || height <= 0)
                return;

            m_RowHeight = height;
            ApplyListHeight();
        }

        void ApplyListHeight()
        {
            if (m_RowHeight <= 0)
                return;

            m_List.style.maxHeight = m_RowHeight * m_MaxVisibleAccounts;
        }

        void ApplyMenuWidth()
        {
            m_Menu.style.width = m_MenuWidth.IsSet
                ? m_MenuWidth.Value
                : m_Profile != null ? defaultProfileMenuWidth : defaultMenuWidth;
        }

        void RefreshTrigger()
        {
            var account = activeAccount;
            var name = m_Profile?.name ?? account?.name;

            m_TriggerAvatar.identity = m_Profile != null
                ? new UserIdentity
                {
                    id = m_Profile.name,
                    name = m_Profile.name,
                    portrait = m_Profile.portrait,
                    color = m_Profile.color,
                }
                : account != null
                    ? new UserIdentity
                    {
                        id = account.id,
                        name = account.name,
                        portrait = account.portrait,
                        color = account.color,
                    }
                    : default;

            m_TriggerLabel.text = m_TriggerLabelOverride ?? name;
            m_Trigger.tooltip = string.IsNullOrEmpty(name) ? "Switch account" : $"Account: {name}";
        }

        void RefreshMenu()
        {
            m_Menu.Clear();
            m_AccountsCard.Clear();
            ApplyMenuWidth();

            if (m_Profile != null)
            {
                m_ProfileAvatar.identity = new UserIdentity
                {
                    id = m_Profile.name,
                    name = m_Profile.name,
                    portrait = m_Profile.portrait,
                    color = m_Profile.color,
                };
                m_ProfileName.text = m_Profile.name;
                m_ProfileEmail.text = m_Profile.email;
                m_ProfileEmail.style.display = string.IsNullOrEmpty(m_Profile.email)
                    ? DisplayStyle.None : DisplayStyle.Flex;
                m_ProfileCaption.text = m_Profile.caption;
                m_ProfileCaption.style.display = string.IsNullOrEmpty(m_Profile.caption)
                    ? DisplayStyle.None : DisplayStyle.Flex;
                m_Menu.Add(m_ProfileCard);
            }

            // With no accounts there is nothing to switch between, so the card is dropped entirely and
            // only the profile header and the extra rows remain — a "profile and actions" surface.
            if (m_Accounts.Count > 0)
            {
                var listHost = new VisualElement { pickingMode = PickingMode.Ignore };
                listHost.hierarchy.Add(m_SearchWrapper);
                listHost.hierarchy.Add(m_List);

                m_SearchWrapper.style.display = searchVisible ? DisplayStyle.Flex : DisplayStyle.None;

                if (m_Collapsible)
                {
                    m_AccountsCard.Add(m_Expander);
                    if (m_Expanded)
                        m_AccountsCard.Add(listHost);
                }
                else
                {
                    m_Section.Clear();
                    m_Section.Add(listHost);
                    m_AccountsCard.Add(m_Section);
                }

                m_Menu.Add(m_AccountsCard);
                // A collapsed list is not merely hidden: its rows are dropped, so a stale row cannot
                // be found, focused or clicked while the expander says the list is closed.
                if (expanded)
                    RefreshRows();
                else
                    m_List.contentContainer.Clear();
            }

            m_Menu.Add(m_Extra);
        }

        void RefreshRows()
        {
            m_List.contentContainer.Clear();

            var query = m_SearchBar.value?.Trim();
            var rows = string.IsNullOrEmpty(query)
                ? m_Accounts
                : m_Accounts.Where(a =>
                    !string.IsNullOrEmpty(a.name) &&
                    a.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            if (rows.Count == 0)
            {
                m_List.contentContainer.Add(m_Empty);
                return;
            }

            var active = activeAccount;
            foreach (var account in rows)
                m_List.contentContainer.Add(BuildRow(account, account == active));
        }

        MenuItem BuildRow(UserAccount account, bool isActive)
        {
            // A real selectable row, so the checkmark, its alignment and its disabled treatment are the
            // menu's own rather than a glyph bolted on.
            var item = new MenuItem { label = account.name, selectable = true };
            item.SetValueWithoutNotify(isActive);

            if (m_ShowAccountAvatars)
            {
                var avatar = new UserAvatar
                {
                    size = Size.Sm,
                    pickingMode = PickingMode.Ignore,
                    identity = new UserIdentity
                    {
                        id = account.id,
                        name = account.name,
                        portrait = account.portrait,
                        color = account.color,
                    },
                };
                avatar.AddToClassList(accountAvatarUssClassName);
                // In front of the label, where MenuItem puts its own icon. Located rather than
                // hard-coded, so a change to MenuItem's own hierarchy cannot silently misplace it.
                item.hierarchy.Insert(LabelIndexOf(item), avatar);
            }

            if (account.disabled)
            {
                item.SetEnabled(false);
                // The tooltip explains the whole row, not just the marker, which is where a user
                // wondering why they cannot click will point.
                item.tooltip = account.warning;

                var warning = new Icon
                {
                    iconName = "warning",
                    variant = IconVariant.Fill,
                    name = accountWarningUssClassName,
                    pickingMode = PickingMode.Ignore,
                };
                warning.AddToClassList(accountWarningUssClassName);
                item.hierarchy.Add(warning);
            }
            else
            {
                item.clickable.clicked += () =>
                {
                    activeAccountId = account.id;
                    accountSelected?.Invoke(account.id);
                    CloseMenu();
                };
            }

            return item;
        }

        static int LabelIndexOf(MenuItem item)
        {
            for (var i = 0; i < item.hierarchy.childCount; i++)
            {
                if (item.hierarchy.ElementAt(i).ClassListContains(MenuItem.labelUssClassName))
                    return i;
            }
            return item.hierarchy.childCount;
        }
    }
}
