using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// How a <see cref="MaskField"/> describes a value with several named bits set.
    /// </summary>
    public enum MaskFieldSummary
    {
        /// <summary>
        /// Show the mixed label.
        /// </summary>
        Mixed,

        /// <summary>
        /// List the display names of the set bits, separated by commas.
        /// </summary>
        List,
    }

    /// <summary>
    /// MaskField UI element.
    /// </summary>
    [UxmlElement]
    public partial class MaskField : Dropdown, IInputElement<int>
    {
        internal static readonly BindingId maskValueProperty = nameof(value);

        internal static readonly BindingId getMaskValueProperty = nameof(getMaskValue);

        internal static readonly BindingId getDisplayNameProperty = nameof(getDisplayName);

        internal static readonly BindingId everythingValueProperty = nameof(everythingValue);

        internal static readonly BindingId summaryProperty = nameof(summary);

        internal static readonly BindingId nothingLabelProperty = nameof(nothingLabel);

        internal static readonly BindingId everythingLabelProperty = nameof(everythingLabel);

        internal static readonly BindingId mixedLabelProperty = nameof(mixedLabel);

        int m_RawValue;

        // The item bits the current selection was made against.
        int m_KnownBits;

        int m_EverythingValue;

        MaskFieldSummary m_Summary = MaskFieldSummary.Mixed;

        string m_NothingLabel;

        string m_EverythingLabel;

        string m_MixedLabel;

        /// <summary>
        /// The main styling class for the MaskField.
        /// </summary>
        public new const string ussClassName = "appui-mask-field";

        int m_DefaultMaskValue;

        GetDisplayNameDelegate m_GetDisplayName;

        GetMaskValueDelegate m_GetMaskValue;

        const string k_NothingMessage =
#if UNITY_LOCALIZATION_PRESENT
            "@AppUI:nothing";
#else
            "Nothing";
#endif

        const string k_EverythingMessage =
#if UNITY_LOCALIZATION_PRESENT
            "@AppUI:everything";
#else
            "Everything";
#endif

        const string k_MixedMessage =
#if UNITY_LOCALIZATION_PRESENT
            "@AppUI:mixed";
#else
            "Mixed...";
#endif

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MaskField() : this(null) { }

        /// <summary>
        /// Constructor with a default mask value.
        /// </summary>
        /// <param name="sourceItems"> The source items collection. </param>
        /// <param name="defaultMaskValue"> The default value for the field. </param>
        /// <param name="getDisplayNameFunc"> An optional function to get the display name. </param>
        /// <param name="getMaskValueFunc"> An optional function to get the mask value. </param>
        /// <param name="bindItemFunc"> An optional function to bind the item. </param>
        /// <param name="bindTitleFunc"> An optional function to bind the title. </param>
        public MaskField(
            IList sourceItems,
            int defaultMaskValue = 0,
            GetDisplayNameDelegate getDisplayNameFunc = null,
            GetMaskValueDelegate getMaskValueFunc = null,
            BindItemFunc bindItemFunc = null,
            BindTitleFunc bindTitleFunc = null)
            : base(sourceItems, bindItemFunc, bindTitleFunc)
        {
            AddToClassList(ussClassName);

            selectionType = PickerSelectionType.Multiple;

            makeItem = MakeItem;
            makeTitle = MakeTitle;
            getDisplayName = getDisplayNameFunc;
            getMaskValue = getMaskValueFunc;
            bindItem = bindItemFunc ?? OnBindMaskItem;
            bindTitle = bindTitleFunc ?? OnBindTitle;

            Init(defaultMaskValue);
        }

        void Init(int newMaskValue)
        {
            if (value != newMaskValue)
                SetValueWithoutNotify(newMaskValue);
            else
                Refresh();
        }

        /// <summary>
        /// Set the value of the field without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value to set. </param>
        public void SetValueWithoutNotify(int newValue)
        {
            m_RawValue = newValue;
            var indices = ConvertMaskValueToIndices(newValue);
            base.SetValueWithoutNotify(indices);
        }

        /// <inheritdoc />
        protected override void OnValueSet()
        {
            base.OnValueSet();
            // Keep the full mask in step with the selection, so it survives a change of items.
            m_RawValue = value;
            if (validateValue != null)
                invalid = !validateValue(value);
        }

        /// <summary>
        /// The current value of the field.
        /// </summary>
        public new int value
        {
            get
            {
                var indices = base.value;
                if (indices == null || !HasValidDataBindings())
                    return 0;
                // Bits no item names are kept from the last value written; the named bits follow the selection.
                var result = m_RawValue & ~m_KnownBits;
                foreach (var index in indices)
                {
                    var itemValue = GetItemValue(index);
                    result |= itemValue;
                }
                return result;
            }
            set
            {
                var previousValue = this.value;
                if (value == previousValue)
                    return;
                using var evt = ChangeEvent<int>.GetPooled(previousValue, value);
                evt.target = this;
                m_RawValue = value;
                base.value = ConvertMaskValueToIndices(value);
                // The selection does not change when only unnamed bits do, so refresh and validate here.
                RefreshTitleUI();
                if (validateValue != null)
                    invalid = !validateValue(value);
                SendEvent(evt);

                NotifyPropertyChanged(in maskValueProperty);
            }
        }

        /// <summary>
        /// An optional function to validate the value.
        /// </summary>
        public new Func<int, bool> validateValue { get; set; }

        [UxmlAttribute("mask-value")]
        int defaultMaskValue
        {
            get => m_DefaultMaskValue;
            set
            {
                m_DefaultMaskValue = value;
                Init(m_DefaultMaskValue);
            }
        }

        /// <summary>
        /// Method to get the mask value for a specific index in the source items collection.
        /// </summary>
        /// <param name="index"> The index of the item in the source items collection. </param>
        /// <returns> The mask value for the item at the specified index. </returns>
        public delegate int GetMaskValueDelegate(int index);

        /// <summary>
        /// Get the mask value for a specific index in the source items collection.
        /// </summary>
        [CreateProperty]
        public GetMaskValueDelegate getMaskValue
        {
            get => m_GetMaskValue;
            set
            {
                if (m_GetMaskValue == value)
                    return;

                m_GetMaskValue = value;
                Refresh();

                NotifyPropertyChanged(in getMaskValueProperty);
            }
        }

        /// <summary>
        /// Method to get the display name for a specific index in the source items collection.
        /// </summary>
        /// <param name="index"> The index of the item in the source items collection. </param>
        /// <returns> The display name for the item at the specified index. </returns>
        public delegate string GetDisplayNameDelegate(int index);

        /// <summary>
        /// Get the display name for a specific index in the source items collection.
        /// </summary>
        [CreateProperty]
        public GetDisplayNameDelegate getDisplayName
        {
            get => m_GetDisplayName;
            set
            {
                if (m_GetDisplayName == value)
                    return;

                m_GetDisplayName = value;
                Refresh();

                NotifyPropertyChanged(in getDisplayNameProperty);
            }
        }

        /// <summary>
        /// The value that means every bit. The Everything row writes it and the field reads it back as Everything.
        /// When 0 (the default), it is the OR of the item values. Set it when the mask has a sentinel wider than its
        /// items, such as -1 for a layer mask whose every bit is set, including bits no item names yet.
        /// </summary>
        [UxmlAttribute("everything-value")]
        [CreateProperty]
        public int everythingValue
        {
            get => m_EverythingValue;
            set
            {
                if (m_EverythingValue == value)
                    return;
                m_EverythingValue = value;
                Refresh();
                NotifyPropertyChanged(in everythingValueProperty);
            }
        }

        /// <summary>
        /// How the field describes several set bits: <see cref="MaskFieldSummary.Mixed"/> (the default) shows the
        /// mixed label, <see cref="MaskFieldSummary.List"/> lists the display names. Either way, a set bit no item
        /// names reads as mixed.
        /// </summary>
        [UxmlAttribute]
        [CreateProperty]
        public MaskFieldSummary summary
        {
            get => m_Summary;
            set
            {
                if (m_Summary == value)
                    return;
                m_Summary = value;
                Refresh();
                NotifyPropertyChanged(in summaryProperty);
            }
        }

        /// <summary>
        /// The text for a value with no bits set, on the field and its Nothing row. Null uses the default "Nothing".
        /// </summary>
        [UxmlAttribute]
        [CreateProperty]
        public string nothingLabel
        {
            get => m_NothingLabel;
            set
            {
                if (m_NothingLabel == value)
                    return;
                m_NothingLabel = value;
                Refresh();
                NotifyPropertyChanged(in nothingLabelProperty);
            }
        }

        /// <summary>
        /// The text for a value with every bit set, on the field and its Everything row. Null uses the default
        /// "Everything".
        /// </summary>
        [UxmlAttribute]
        [CreateProperty]
        public string everythingLabel
        {
            get => m_EverythingLabel;
            set
            {
                if (m_EverythingLabel == value)
                    return;
                m_EverythingLabel = value;
                Refresh();
                NotifyPropertyChanged(in everythingLabelProperty);
            }
        }

        /// <summary>
        /// The text for a value with several bits set when <see cref="summary"/> is
        /// <see cref="MaskFieldSummary.Mixed"/>, or with a bit no item names. Null uses the default "Mixed...".
        /// </summary>
        [UxmlAttribute]
        [CreateProperty]
        public string mixedLabel
        {
            get => m_MixedLabel;
            set
            {
                if (m_MixedLabel == value)
                    return;
                m_MixedLabel = value;
                Refresh();
                NotifyPropertyChanged(in mixedLabelProperty);
            }
        }

        string nothingText => m_NothingLabel ?? k_NothingMessage;

        string everythingText => m_EverythingLabel ?? k_EverythingMessage;

        string mixedText => m_MixedLabel ?? k_MixedMessage;

        int GetAllItemBits()
        {
            var result = 0;
            if (sourceItems == null)
                return result;
            for (var i = 0; i < sourceItems.Count; i++)
                result |= GetItemValue(i);
            return result;
        }

        int GetEverythingValue() => m_EverythingValue != 0 ? m_EverythingValue : GetAllItemBits();

        /// <inheritdoc />
        protected override void OnPickerItemsCreated()
        {
            base.OnPickerItemsCreated();

            // Reselect against the new items, so a bit an added item now names stays set.
            var fullValue = m_RawValue;
            m_KnownBits = HasValidDataBindings() ? GetAllItemBits() : 0;
            if (HasValidDataBindings())
            {
                m_RawValue = fullValue;
                base.SetValueWithoutNotify(ConvertMaskValueToIndices(fullValue));
            }

            var everythingContent = new DropdownItem { label = everythingText };
            var everythingItem = new PickerItem {index = -2};
            everythingItem.clickable.clickedWithEventInfo += OnEverythingClicked;
            everythingItem.Add(everythingContent);
            m_Items.Insert(0, everythingItem);

            var nothingContent = new DropdownItem { label = nothingText };
            var nothingItem = new PickerItem {index = -1};
            nothingItem.clickable.clickedWithEventInfo += OnNothingClicked;
            nothingItem.Add(nothingContent);
            m_Items.Insert(1, nothingItem);
        }

        /// <inheritdoc />
        protected override void OnPickerItemRemoved(PickerItem item, int index)
        {
            base.OnPickerItemRemoved(item, index);
            switch (index)
            {
                case -2:
                    item.clickable.clickedWithEventInfo -= OnEverythingClicked;
                    break;
                case -1:
                    item.clickable.clickedWithEventInfo -= OnNothingClicked;
                    break;
            }
        }

        void OnEverythingClicked(EventBase evt)
        {
            value = GetEverythingValue();
        }

        void OnNothingClicked(EventBase evt)
        {
            value = 0;
        }

        bool HasValidDataBindings()
        {
            if (sourceItems == null)
                return false;

            var isIntegerCollection = sourceItems is IList<int> or IList<Enum>;
            return isIntegerCollection || getMaskValue != null;
        }

        IEnumerable<int> ConvertMaskValueToIndices(int newValue)
        {
            var result = new List<int>();
            if (!HasValidDataBindings())
                return result;
            for (var i = 0; i < sourceItems.Count; i++)
            {
                var itemValue = GetItemValue(i);
                if ((newValue & itemValue) == itemValue)
                    result.Add(i);
            }
            return result;
        }

        void OnBindMaskItem(DropdownItem item, int index)
        {
            item.label = GetDisplayName(index);
        }

        void OnBindTitle(DropdownItem item, IEnumerable<int> indices)
        {
            var names = new List<string>();
            var mask = HasValidDataBindings() ? m_RawValue & ~m_KnownBits : 0;
            var hasUnnamedBits = mask != 0;
            if (indices != null)
            {
                foreach (var index in indices)
                {
                    mask |= GetItemValue(index);
                    names.Add(GetDisplayName(index));
                }
            }

            var everything = GetEverythingValue();
            if (mask == 0)
                item.label = nothingText;
            else if (everything != 0 && mask == everything)
                item.label = everythingText;
            else if (hasUnnamedBits)
                item.label = mixedText;
            else if (names.Count == 1)
                item.label = names[0];
            else if (summary == MaskFieldSummary.List && names.Count > 0)
                item.label = string.Join(", ", names);
            else
                item.label = mixedText;
        }

        string GetDisplayName(int index)
        {
            if (sourceItems == null)
                return string.Empty;

            if (getDisplayName != null)
                return getDisplayName(index);

            return sourceItems switch
            {
                IList<string> stringList => stringList[index],
                IList<Enum> enumList => Enum.GetNames(enumList[index].GetType())[index],
                _ => index.ToString()
            };
        }

        int GetItemValue(int index)
        {
            if (sourceItems == null)
                return 0;

            if (getMaskValue != null)
                return getMaskValue(index);

            return sourceItems switch
            {
                IList<int> intList => intList[index],
                IList<Enum> enumList => Convert.ToInt32(enumList[index]),
                _ => 0
            };
        }

    }
}
