using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A resizable container element designed to be used as a child of SplitView.
    /// </summary>
    /// <remarks>
    /// The Pane component is a specialized container designed exclusively for use within a SplitView. It
    /// represents an individual resizable panel that users can adjust by dragging splitters between adjacent
    /// panes.
    ///
    /// Key features:
    /// - Flexible sizing with stretch factor control
    /// - Automatic compact mode when resized below threshold
    /// - Persistent state saving and restoration
    /// - Minimum size constraints via USS
    /// - Non-interactive by default (picking mode set to ignore)
    ///
    /// Panes can be configured to either maintain a fixed size or stretch to fill available space. The compact
    /// mode feature allows panes to automatically collapse when they become too narrow, providing a better user
    /// experience for complex layouts.
    ///
    /// NOTE: Panes must be direct children of a SplitView component. Using Pane outside of a SplitView is not
    /// supported.
    /// </remarks>
    /// <example>
    /// <para>Basic pane in a split view — creating a split view with fixed and flexible panes.</para>
    /// <code lang="xml"><![CDATA[
    /// <SplitView>
    ///     <Pane style="min-width: 200px;">
    ///         <Text text="Fixed width pane" />
    ///     </Pane>
    ///     <Pane stretch-factor="1">
    ///         <Text text="Flexible pane that fills remaining space" />
    ///     </Pane>
    /// </SplitView>
    /// ]]></code>
    /// <para>Using compact mode — creating a collapsible sidebar that enters compact mode when small.</para>
    /// <code lang="xml"><![CDATA[
    /// <SplitView>
    ///     <Pane compact-threshold="50" style="min-width: 50px;">
    ///         <Icon name="menu" />
    ///     </Pane>
    ///     <Pane stretch-factor="1">
    ///         <Text text="Main content" />
    ///     </Pane>
    /// </SplitView>
    /// ]]></code>
    /// <para>Proportional sizing with stretch factors — creating panes with proportional sizing.</para>
    /// <code lang="xml"><![CDATA[
    /// <SplitView>
    ///     <Pane stretch-factor="1">
    ///         <Text text="Takes 1/3 of space" />
    ///     </Pane>
    ///     <Pane stretch-factor="2">
    ///         <Text text="Takes 2/3 of space" />
    ///     </Pane>
    /// </SplitView>
    /// ]]></code>
    /// <para>Saving and restoring pane state — persisting pane state across sessions.</para>
    /// <code lang="csharp"><![CDATA[
    /// // Save pane state
    /// var state = pane.SaveState();
    /// PlayerPrefs.SetString("paneState", JsonUtility.ToJson(state));
    ///
    /// // Restore pane state later
    /// var stateJson = PlayerPrefs.GetString("paneState");
    /// var restoredState = JsonUtility.FromJson<Pane.State>(stateJson);
    /// pane.RestoreState(restoredState);
    /// ]]></code>
    /// <para>Reacting to compact mode changes — handling compact mode state changes.</para>
    /// <code lang="csharp"><![CDATA[
    /// var pane = new Pane();
    /// pane.compactChanged += (p) =>
    /// {
    ///     if (p.compact)
    ///     {
    ///         // Pane entered compact mode - show icon-only view
    ///         Debug.Log("Pane collapsed");
    ///     }
    ///     else
    ///     {
    ///         // Pane expanded - show full content
    ///         Debug.Log("Pane expanded");
    ///     }
    /// };
    ///
    /// splitView.AddPane(pane);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class Pane : BaseVisualElement
    {
        internal static readonly BindingId compactThresholdProperty = nameof(compactThreshold);

        internal static readonly BindingId compactProperty = nameof(compact);

        internal static readonly BindingId stretchFactorProperty = nameof(stretchFactor);

        internal static readonly BindingId stretchProperty = nameof(stretch);

        static readonly EventCallback<GeometryChangedEvent> k_OnGeometryChanged = OnGeometryChanged;

        /// <summary>
        /// Event that is triggered when the pane is toggled between compact and expanded mode.
        /// </summary>
        public event Action<Pane> compactChanged;

        /// <summary>
        /// The default threshold used to snap to compact mode when the pane is resized.
        /// </summary>
        internal const float defaultCompactThreshold = 16f;

        /// <summary>
        /// The USS class name of a <see cref="Pane"/>.
        /// </summary>
        public const string ussClassName = "appui-pane";

        /// <summary>
        /// The USS class name of a <see cref="Pane"/> in compact mode.
        /// </summary>
        public const string compactUssClassName = ussClassName + "--compact";

        bool m_Compact;

        float m_CompactThreshold = defaultCompactThreshold;

        /// <summary>
        /// A threshold used to snap to compact mode when the pane is resized.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float compactThreshold
        {
            get => m_CompactThreshold;
            set
            {
                var changed = Mathf.Approximately(m_CompactThreshold, value);
                m_CompactThreshold = value;

                if (changed)
                    NotifyPropertyChanged(in compactThresholdProperty);
            }
        }

        /// <summary>
        /// Whether the pane is in compact mode or not.
        /// </summary>
        [CreateProperty]
        internal bool compact
        {
            get => m_Compact;
            set
            {
                var changed = m_Compact != value;
                SetCompact(value);

                if (changed)
                {
                    compactChanged?.Invoke(this);
                    NotifyPropertyChanged(in compactProperty);
                }
            }
        }

        /// <summary>
        /// The stretch factor of the pane.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float stretchFactor
        {
            get => resolvedStyle.flexGrow;
            set
            {
                var changed = Mathf.Approximately(resolvedStyle.flexGrow, value);
                var stretchChanged = stretch != (value > 0);

                style.flexGrow = value;
                style.flexShrink = value > 0 ? 1 : 0;

                if (changed)
                    NotifyPropertyChanged(in stretchFactorProperty);
                if (stretchChanged)
                    NotifyPropertyChanged(in stretchProperty);
            }
        }

        /// <summary>
        /// Whether the pane can be stretched or not.
        /// </summary>
        [CreateProperty]
        public bool stretch
        {
            get => stretchFactor > 0;
            set => stretchFactor = value ? 1 : 0;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Pane()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            compactThreshold = defaultCompactThreshold;
            SetCompact(false);
            stretchFactor = 0;

            RegisterCallback(k_OnGeometryChanged);
        }

        static void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.target is Pane { parent: SplitView splitView } pane)
                splitView.RefreshSplitterPosition(splitView.IndexOf(pane));
        }

        /// <summary>
        /// Toggles the pane between compact and expanded mode.
        /// </summary>
        public void ToggleCompact()
        {
            compact = !compact;
        }

        void SetCompact(bool value)
        {
            m_Compact = value;
            EnableInClassList(compactUssClassName, m_Compact);
        }

        /// <summary>
        /// Saves the state of the <see cref="Pane"/>.
        /// </summary>
        /// <returns> The state of the <see cref="Pane"/>.</returns>
        public State SaveState()
        {
            return new State
            {
                compact = compact,
                compactThreshold = compactThreshold
            };
        }

        /// <summary>
        /// Restores the state of the <see cref="Pane"/>.
        /// </summary>
        /// <param name="state"> The state of the <see cref="Pane"/>.</param>
        public void RestoreState(State state)
        {
            compactThreshold = state.compactThreshold;
            SetCompact(state.compact);
        }

        /// <summary>
        /// The state of a <see cref="Pane"/>.
        /// </summary>
        [Serializable]
        public struct State
        {
            /// <summary>
            /// Whether the pane is in compact mode or not.
            /// </summary>
            public bool compact;

            /// <summary>
            /// A threshold used to snap to compact mode when the pane is resized.
            /// </summary>
            public float compactThreshold;
        }

    }
}
