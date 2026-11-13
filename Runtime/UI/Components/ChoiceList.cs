using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A single-select list of lettered options.
    /// </summary>
    /// <remarks>
    /// ChoiceList is the multiple-choice question surface: a vertical list of full-width rows, each
    /// carrying an auto-assigned letter badge (A, B, C…), of which exactly one can be selected.
    ///
    /// It derives from <see cref="RadioGroup"/> and its items from <see cref="Radio"/>, so the whole
    /// selection contract is the radio one and nothing about it is re-implemented: a
    /// <see cref="RadioGroup.value"/> holding the selected <see cref="Radio.key"/>, mutual
    /// exclusivity, <see cref="RadioGroup.validateValue"/>, change events and keyboard operation all
    /// come from the base types. ChoiceList adds only the lettering, and
    /// <see cref="ChoiceListItem"/> adds only the badge and the row presentation.
    ///
    /// Letters are assigned in document order to the direct <see cref="ChoiceListItem"/> children and
    /// kept up to date as items attach or detach. Set <see cref="ChoiceListItem.letter"/> explicitly
    /// to pin a badge, or to an empty string to hide it.
    /// </remarks>
    /// <example>
    /// <para>A lettered question.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:ChoiceList value="balanced">
    ///     <appui:ChoiceListItem key="fast" label="Favor speed" />
    ///     <appui:ChoiceListItem key="balanced" label="Balanced" />
    ///     <appui:ChoiceListItem key="quality" label="Favor quality" />
    /// </appui:ChoiceList>
    /// ]]></code>
    /// <para>Reading the answer.</para>
    /// <code lang="csharp"><![CDATA[
    /// var list = new ChoiceList();
    /// list.Add(new ChoiceListItem { key = "fast", label = "Favor speed" });
    /// list.Add(new ChoiceListItem { key = "quality", label = "Favor quality" });
    /// list.RegisterValueChangedCallback(evt => Debug.Log($"Chose: {evt.newValue}"));
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class ChoiceList : RadioGroup
    {
        /// <summary>
        /// The ChoiceList main styling class.
        /// </summary>
        public new const string ussClassName = "appui-choicelist";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChoiceList()
        {
            AddToClassList(ussClassName);
        }

        /// <summary>
        /// Re-assign the automatic letters (A, B, C…) to the direct <see cref="ChoiceListItem"/>
        /// children, in document order. Items whose letter was set explicitly are skipped and do
        /// not consume a letter.
        /// </summary>
        internal void RefreshLetters()
        {
            var index = 0;
            foreach (var child in Children())
            {
                if (child is not ChoiceListItem item)
                    continue;
                if (item.hasExplicitLetter)
                    continue;
                item.SetAutoLetter(GetLetter(index));
                index++;
            }
        }

        internal static string GetLetter(int index)
        {
            // A..Z, then AA, AB… so a long list never runs out of badges.
            var letters = new List<char>();
            var i = index;
            do
            {
                letters.Insert(0, (char)('A' + i % 26));
                i = i / 26 - 1;
            } while (i >= 0);
            return new string(letters.ToArray());
        }
    }

    /// <summary>
    /// A single lettered option inside a <see cref="ChoiceList"/>.
    /// </summary>
    /// <remarks>
    /// ChoiceListItem derives from <see cref="Radio"/>. Everything about being a radio — the
    /// <see cref="Radio.key"/>, <see cref="Radio.value"/>, group registration, the
    /// <see cref="Radio.clickable"/> manipulator and keyboard focus — is inherited unchanged. The
    /// item only adds the letter badge and styles the row as a full-width pressable surface with
    /// the radio circle hidden.
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class ChoiceListItem : Radio
    {
        internal static readonly BindingId letterProperty = new BindingId(nameof(letter));

        /// <summary>
        /// The ChoiceListItem main styling class.
        /// </summary>
        public new const string ussClassName = "appui-choicelistitem";

        /// <summary>
        /// The ChoiceListItem letter badge styling class.
        /// </summary>
        public const string badgeUssClassName = ussClassName + "__badge";

        readonly LocalizedTextElement m_Badge;

        string m_ExplicitLetter;

        ChoiceList m_List;

        internal bool hasExplicitLetter => m_ExplicitLetter != null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChoiceListItem()
        {
            AddToClassList(ussClassName);

            m_Badge = new LocalizedTextElement { name = badgeUssClassName, pickingMode = PickingMode.Ignore };
            m_Badge.AddToClassList(badgeUssClassName);
            hierarchy.Insert(0, m_Badge);

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        /// <summary>
        /// The letter badge shown at the start of the row.
        /// </summary>
        /// <remarks>
        /// Left unset, the parent <see cref="ChoiceList"/> assigns it automatically from the item's
        /// position (A, B, C…). Setting it pins the badge and takes the item out of the automatic
        /// sequence. Set it to an empty string to hide the badge.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string letter
        {
            get => m_Badge.text;
            set
            {
                var changed = m_Badge.text != value;
                m_ExplicitLetter = value;
                ApplyLetter(value);
                (m_List ?? GetFirstAncestorOfType<ChoiceList>())?.RefreshLetters();

                if (changed)
                    NotifyPropertyChanged(in letterProperty);
            }
        }

        internal void SetAutoLetter(string value)
        {
            var changed = m_Badge.text != value;
            ApplyLetter(value);

            if (changed)
                NotifyPropertyChanged(in letterProperty);
        }

        void ApplyLetter(string value)
        {
            m_Badge.text = value;
            m_Badge.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            m_List = GetFirstAncestorOfType<ChoiceList>();
            m_List?.RefreshLetters();
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            // The item is still in the hierarchy while the event propagates, so re-letter the list
            // it is leaving on the next frame, once the removal has actually happened.
            var list = m_List;
            m_List = null;
            list?.schedule.Execute(list.RefreshLetters);
        }
    }
}
