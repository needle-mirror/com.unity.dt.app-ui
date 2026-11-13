using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// How a keyframe's tangents are derived.
    /// </summary>
    /// <remarks>
    /// <see cref="UnityEngine.Keyframe"/> stores tangent <em>values</em> but not the intent behind
    /// them, and Unity's own <c>AnimationUtility.TangentMode</c> lives in <c>UnityEditor</c>, so it
    /// cannot be referenced from a runtime component. The editor therefore keeps its own mode per
    /// key, alongside the curve, and re-derives the tangent values from it whenever the geometry
    /// the mode depends on moves.
    /// </remarks>
    public enum TangentMode
    {
        /// <summary>
        /// Smooth across the neighbours, flattened at a local minimum or maximum so the
        /// interpolation cannot overshoot past the key.
        /// </summary>
        ClampedAuto,

        /// <summary>
        /// Smooth across the neighbours, with no overshoot protection.
        /// </summary>
        Auto,

        /// <summary>
        /// Both tangents are authored by hand and stay equal, so the curve remains smooth
        /// through the key.
        /// </summary>
        Free,

        /// <summary>
        /// Both tangents are zero: the curve arrives and leaves horizontally.
        /// </summary>
        Flat,

        /// <summary>
        /// The two tangents are authored independently, putting a corner at the key.
        /// </summary>
        Broken,

        /// <summary>
        /// Each tangent points straight at the neighbour on that side.
        /// </summary>
        Linear
    }

    /// <summary>
    /// An editable animation-curve graph: drag keys, drag the selected key's tangent handles,
    /// double-click to add a key, and pick a preset.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The editor is the counterpart <see cref="AnimationCurveField"/> has been waiting for. It implements
    /// <see cref="ICustomAnimationCurveFieldPicker"/>, so assigning one to
    /// <see cref="AnimationCurveField.customPicker"/> turns the field from a read-only curve thumbnail into
    /// an editable one — the same way <see cref="ColorField"/> and <see cref="ColorPicker"/> pair
    /// up.
    /// </para>
    ///
    /// <para>
    /// The curve and the grid are stroked with <see cref="Painter2D"/>, exactly as
    /// <see cref="AnimationCurveField"/> strokes its thumbnail, so no shader and no render texture are
    /// involved. Keys and tangent handles are real elements rather than painted marks, which is
    /// what lets them be hit-tested and styled from USS.
    /// </para>
    ///
    /// <para>
    /// The plotted range is taken from the curve on load: time spans the curve's own first and last
    /// key, and value spans <c>[0, 1]</c> widened to fit the curve — the same rule
    /// <see cref="AnimationCurveField"/> uses, so the thumbnail and the editor frame a curve identically.
    /// Editing clamps into that range rather than growing it, so the graph never moves under the
    /// pointer mid-drag.
    /// </para>
    ///
    /// <para>Interaction</para>
    /// <para>
    /// <list type="bullet">
    /// <item><description>Click a key to select it, drag to move it. The first and last key keep
    /// their time and move only in value, because they anchor the plotted range.</description></item>
    /// <item><description>Drag a tangent handle to author the slope. Hold <c>Alt</c> to break the
    /// handle so the two sides move independently.</description></item>
    /// <item><description>Double-click the graph to add a key there.</description></item>
    /// <item><description><c>Delete</c> or <c>Backspace</c> removes the selected key, never the
    /// last one. <c>F</c> flattens it.</description></item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// Two deliberate divergences from the registry. Tangent modes are chosen from a dropdown in
    /// the toolbar rather than from a right-click context menu, because UI Toolkit has no
    /// equivalent of the registry's absolutely positioned menu div and a dropdown is the
    /// package's own idiom for a mode list. And selection is single-key: there is no marquee
    /// box-select, so no group move, group delete or group flatten.
    /// </para>
    ///
    /// <para>Example</para>
    /// <code>
    /// var field = new AnimationCurveField { customPicker = new CurveEditor() };
    /// field.RegisterValueChangedCallback(evt => Debug.Log(evt.newValue));
    /// </code>
    /// </remarks>
    public class CurveEditor : VisualElement, INotifyValueChanged<AnimationCurve>, ICustomAnimationCurveFieldPicker
    {
        internal static readonly BindingId valueProperty = nameof(value);

        /// <summary>
        /// How far from its key a tangent handle sits, in pixels. Fixed rather than proportional so
        /// the handle stays grabbable whatever the graph's aspect ratio.
        /// </summary>
        public const float handleRadius = 32f;

        /// <summary>
        /// Number of samples used to stroke the curve. Matches <see cref="AnimationCurveField"/>'s density
        /// scaled up for the editor's larger surface.
        /// </summary>
        const int k_SampleCount = 128;

        /// <summary>
        /// Smallest time gap kept between a key and its neighbour, so two keys never land on the
        /// same time and make the segment between them degenerate.
        /// </summary>
        const float k_MinTimeGap = 0.001f;

        static readonly CustomStyleProperty<Color> k_UssCurveColor = new CustomStyleProperty<Color>("--curve-color");

        static readonly CustomStyleProperty<float> k_UssCurveWidth = new CustomStyleProperty<float>("--curve-width");

        static readonly CustomStyleProperty<Color> k_UssGridColor = new CustomStyleProperty<Color>("--grid-color");

        static readonly CustomStyleProperty<float> k_UssGridWidth = new CustomStyleProperty<float>("--grid-width");

        /// <summary>
        /// The CurveEditor main styling class.
        /// </summary>
        public const string ussClassName = "appui-curveeditor";

        /// <summary>
        /// The graph styling class.
        /// </summary>
        public const string graphUssClassName = ussClassName + "__graph";

        /// <summary>
        /// The keyframe marker styling class.
        /// </summary>
        public const string keyUssClassName = ussClassName + "__key";

        /// <summary>
        /// The selected keyframe marker styling class.
        /// </summary>
        public const string selectedKeyUssClassName = keyUssClassName + "--selected";

        /// <summary>
        /// The tangent handle styling class.
        /// </summary>
        public const string handleUssClassName = ussClassName + "__handle";

        /// <summary>
        /// The toolbar styling class.
        /// </summary>
        public const string toolbarUssClassName = ussClassName + "__toolbar";

        /// <summary>
        /// The tangent mode dropdown styling class.
        /// </summary>
        public const string tangentDropdownUssClassName = ussClassName + "__tangent-dropdown";

        /// <summary>
        /// The presets row styling class.
        /// </summary>
        public const string presetsUssClassName = ussClassName + "__presets";

        /// <summary>
        /// The preset button styling class.
        /// </summary>
        public const string presetUssClassName = ussClassName + "__preset";

        /// <summary>
        /// The preset glyph styling class.
        /// </summary>
        public const string presetGlyphUssClassName = ussClassName + "__preset-glyph";

        /// <summary>
        /// Which side of a key a tangent belongs to.
        /// </summary>
        public enum TangentSide
        {
            /// <summary>
            /// The incoming tangent, on the earlier side of the key.
            /// </summary>
            In,

            /// <summary>
            /// The outgoing tangent, on the later side of the key.
            /// </summary>
            Out
        }

        /// <summary>
        /// A named starting shape offered in the presets row.
        /// </summary>
        public class CurvePreset
        {
            internal KeyState[] states { get; }

            /// <summary>
            /// Stable identifier, e.g. <c>ease-in-out</c>.
            /// </summary>
            public string id { get; }

            /// <summary>
            /// Human-readable label.
            /// </summary>
            public string label { get; }

            /// <summary>
            /// The preset's shape, as a fresh <see cref="AnimationCurve"/> per read so a caller
            /// cannot mutate the shared preset.
            /// </summary>
            public AnimationCurve curve => BuildCurve(states);

            internal CurvePreset(string id, string label, KeyState[] states)
            {
                this.id = id;
                this.label = label;
                this.states = states;
            }
        }

        // A key plus the intent behind its tangents. Kept as a class rather than a struct so the
        // selection can hold a reference and survive the reorder a time change causes.
        internal class KeyState
        {
            public float time;
            public float value;
            public float inTangent;
            public float outTangent;
            public TangentMode mode;

            public KeyState Copy() => new KeyState
            {
                time = time,
                value = value,
                inTangent = inTangent,
                outTangent = outTangent,
                mode = mode,
            };
        }

        static KeyState K(float time, float value, float inTangent, float outTangent, TangentMode mode)
        {
            return new KeyState
            {
                time = time,
                value = value,
                inTangent = inTangent,
                outTangent = outTangent,
                mode = mode,
            };
        }

        static readonly CurvePreset[] k_Presets =
        {
            new CurvePreset("constant", "Constant", new[]
            {
                K(0f, 0f, 0f, 0f, TangentMode.Flat),
                K(1f, 0f, 0f, 0f, TangentMode.Flat),
            }),
            new CurvePreset("linear", "Linear", new[]
            {
                K(0f, 0f, 1f, 1f, TangentMode.Linear),
                K(1f, 1f, 1f, 1f, TangentMode.Linear),
            }),
            new CurvePreset("ease-in", "Ease In", new[]
            {
                K(0f, 0f, 0f, 0f, TangentMode.Flat),
                K(1f, 1f, 2f, 2f, TangentMode.Free),
            }),
            new CurvePreset("ease-out", "Ease Out", new[]
            {
                K(0f, 0f, 2f, 2f, TangentMode.Free),
                K(1f, 1f, 0f, 0f, TangentMode.Flat),
            }),
            new CurvePreset("ease-in-out", "Ease In-Out", new[]
            {
                K(0f, 0f, 0f, 0f, TangentMode.Flat),
                K(1f, 1f, 0f, 0f, TangentMode.Flat),
            }),
            new CurvePreset("linear-down", "Linear Down", new[]
            {
                K(0f, 1f, -1f, -1f, TangentMode.Linear),
                K(1f, 0f, -1f, -1f, TangentMode.Linear),
            }),
        };

        static readonly TangentMode[] k_ModeOrder =
        {
            TangentMode.ClampedAuto,
            TangentMode.Auto,
            TangentMode.Free,
            TangentMode.Flat,
            TangentMode.Broken,
            TangentMode.Linear,
        };

        readonly List<KeyState> m_Keys = new List<KeyState>();

        readonly List<VisualElement> m_KeyMarkers = new List<VisualElement>();

        readonly VisualElement m_Graph;

        readonly VisualElement m_InHandle;

        readonly VisualElement m_OutHandle;

        readonly VisualElement m_Presets;

        readonly Dropdown m_TangentDropdown;

        KeyState m_Selected;

        float m_TimeMin;

        float m_TimeMax = 1f;

        float m_ValueMin;

        float m_ValueMax = 1f;

        Color m_CurveColor = Color.white;

        float m_CurveWidth = 2f;

        Color m_GridColor = Color.gray;

        float m_GridWidth = 1f;

        bool m_Syncing;

        enum DragKind
        {
            None,
            Key,
            Handle
        }

        DragKind m_DragKind;

        TangentSide m_DragSide;

        AnimationCurve m_LastNotified;

        Popover m_Popover;

        Action<AnimationCurve> m_OnValueChanging;

        /// <summary>
        /// Default constructor. Starts on the Ease In-Out preset.
        /// </summary>
        public CurveEditor()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;

            m_Graph = new VisualElement { name = graphUssClassName, pickingMode = PickingMode.Position };
            m_Graph.AddToClassList(graphUssClassName);
            m_Graph.generateVisualContent += OnGenerateVisualContent;
            m_Graph.RegisterCallback<PointerDownEvent>(OnGraphPointerDown);
            m_Graph.RegisterCallback<PointerMoveEvent>(OnGraphPointerMove);
            m_Graph.RegisterCallback<PointerUpEvent>(OnGraphPointerUp);
            m_Graph.RegisterCallback<PointerCaptureOutEvent>(_ => m_DragKind = DragKind.None);
            m_Graph.RegisterCallback<GeometryChangedEvent>(_ => Refresh());
            hierarchy.Add(m_Graph);

            m_InHandle = MakeHandle(TangentSide.In);
            m_Graph.Add(m_InHandle);
            m_OutHandle = MakeHandle(TangentSide.Out);
            m_Graph.Add(m_OutHandle);

            var toolbar = new VisualElement { name = toolbarUssClassName };
            toolbar.AddToClassList(toolbarUssClassName);
            hierarchy.Add(toolbar);

            var modeLabels = new List<string>(k_ModeOrder.Length);
            foreach (var m in k_ModeOrder)
                modeLabels.Add(ModeLabel(m));
            m_TangentDropdown = new Dropdown(modeLabels, (item, i) => item.label = modeLabels[i])
            {
                name = tangentDropdownUssClassName,
            };
            m_TangentDropdown.AddToClassList(tangentDropdownUssClassName);
            m_TangentDropdown.RegisterValueChangedCallback(OnTangentModeChanged);
            toolbar.Add(m_TangentDropdown);

            m_Presets = new VisualElement { name = presetsUssClassName };
            m_Presets.AddToClassList(presetsUssClassName);
            hierarchy.Add(m_Presets);
            for (var i = 0; i < k_Presets.Length; i++)
                m_Presets.Add(MakePresetButton(i));

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            SetValueWithoutNotify(k_Presets[4].curve);
        }

        /// <inheritdoc/>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The element the curve, the grid and the tangent arms are stroked into. Key markers and
        /// tangent handles are its children.
        /// </summary>
        public VisualElement graphElement => m_Graph;

        /// <summary>
        /// The presets row.
        /// </summary>
        public VisualElement presetsElement => m_Presets;

        /// <summary>
        /// The tangent mode dropdown.
        /// </summary>
        public Dropdown tangentElement => m_TangentDropdown;

        /// <summary>
        /// The key markers, in time order.
        /// </summary>
        public IReadOnlyList<VisualElement> keyMarkers => m_KeyMarkers;

        /// <summary>
        /// The incoming tangent handle of the selected key. Hidden when nothing is selected.
        /// </summary>
        public VisualElement inTangentHandle => m_InHandle;

        /// <summary>
        /// The outgoing tangent handle of the selected key. Hidden when nothing is selected.
        /// </summary>
        public VisualElement outTangentHandle => m_OutHandle;

        /// <summary>
        /// The available starting shapes.
        /// </summary>
        public static IReadOnlyList<CurvePreset> presets => k_Presets;

        /// <summary>
        /// The area of <see cref="graphElement"/> the curve is plotted into, in graph-local pixels.
        /// </summary>
        public Rect plotRect => m_Graph.contentRect;

        /// <summary>
        /// The index of the selected key, or -1 when nothing is selected.
        /// </summary>
        public int selectedKeyIndex => m_Selected == null ? -1 : m_Keys.IndexOf(m_Selected);

        /// <summary>
        /// The curve being edited.
        /// </summary>
        /// <remarks>
        /// The getter builds a fresh <see cref="AnimationCurve"/> on every read, so a consumer
        /// holding an earlier value never sees it mutate underneath them.
        /// </remarks>
        [CreateProperty]
        public AnimationCurve value
        {
            get => BuildCurve(m_Keys.ToArray());
            set
            {
                var previous = m_LastNotified ?? BuildCurve(m_Keys.ToArray());
                SetValueWithoutNotify(value);
                var next = BuildCurve(m_Keys.ToArray());
                m_LastNotified = next;
                using var evt = ChangeEvent<AnimationCurve>.GetPooled(previous, next);
                evt.target = this;
                SendEvent(evt);
                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// Sets the curve without sending a change event.
        /// </summary>
        /// <param name="newValue"> The curve to edit. Null resets to the default. </param>
        /// <remarks>
        /// Incoming keys land in <see cref="TangentMode.Free"/> so their authored tangents are
        /// preserved exactly. Defaulting them to a solved mode would silently re-derive — and
        /// therefore change — a curve the caller only asked to display.
        /// </remarks>
        public void SetValueWithoutNotify(AnimationCurve newValue)
        {
            var source = newValue ?? k_Presets[4].curve;

            m_Keys.Clear();
            foreach (var key in source.keys)
            {
                m_Keys.Add(new KeyState
                {
                    time = key.time,
                    value = key.value,
                    inTangent = key.inTangent,
                    outTangent = key.outTangent,
                    mode = TangentMode.Free,
                });
            }

            if (m_Keys.Count == 0)
                m_Keys.Add(K(0f, 0f, 0f, 0f, TangentMode.Flat));

            m_Keys.Sort((a, b) => a.time.CompareTo(b.time));
            RecomputeRange();

            m_Selected = null;
            // Snapshot the loaded state as the baseline for the next change event. Left null, the
            // first interactive edit would report a null previousValue rather than the curve the
            // caller just supplied — including on the normal Show(..., initialValue) path.
            m_LastNotified = BuildCurve(m_Keys.ToArray());
            Refresh();
        }

        /// <summary>
        /// Selects a key.
        /// </summary>
        /// <param name="index"> Index of the key. Out-of-range clears the selection. </param>
        public void SelectKey(int index)
        {
            m_Selected = index >= 0 && index < m_Keys.Count ? m_Keys[index] : null;
            Refresh();
        }

        /// <summary>
        /// Moves the selected key.
        /// </summary>
        /// <param name="time"> Target time. Clamped between the neighbouring keys; ignored for the
        /// first and last key, which anchor the plotted time range. </param>
        /// <param name="value"> Target value, clamped to the plotted value range. </param>
        public void MoveSelectedKey(float time, float value)
        {
            if (m_Selected == null)
                return;

            var index = m_Keys.IndexOf(m_Selected);
            var isEndpoint = index == 0 || index == m_Keys.Count - 1;

            if (!isEndpoint)
            {
                var low = m_Keys[index - 1].time + k_MinTimeGap;
                var high = m_Keys[index + 1].time - k_MinTimeGap;
                m_Selected.time = high < low ? low : Mathf.Clamp(time, low, high);
            }

            m_Selected.value = Mathf.Clamp(value, m_ValueMin, m_ValueMax);

            Solve();
            Commit();
        }

        /// <summary>
        /// Adds a key and selects it.
        /// </summary>
        /// <param name="time"> Time of the new key, clamped to the plotted time range. </param>
        /// <param name="value"> Value of the new key, clamped to the plotted value range. </param>
        /// <returns> False when a key already sits at that time. </returns>
        public bool AddKey(float time, float value)
        {
            var t = Mathf.Clamp(time, m_TimeMin, m_TimeMax);
            foreach (var key in m_Keys)
            {
                if (Mathf.Abs(key.time - t) < k_MinTimeGap)
                    return false;
            }

            var state = K(t, Mathf.Clamp(value, m_ValueMin, m_ValueMax), 0f, 0f, TangentMode.ClampedAuto);
            m_Keys.Add(state);
            m_Keys.Sort((a, b) => a.time.CompareTo(b.time));
            m_Selected = state;
            Solve();
            Commit();
            return true;
        }

        /// <summary>
        /// Removes the selected key.
        /// </summary>
        /// <returns> False when nothing is selected, or when it is the only key left. </returns>
        public bool RemoveSelectedKey()
        {
            if (m_Selected == null || m_Keys.Count <= 1)
                return false;

            m_Keys.Remove(m_Selected);
            m_Selected = null;
            Solve();
            Commit();
            return true;
        }

        /// <summary>
        /// Returns the tangent mode of a key.
        /// </summary>
        /// <param name="index"> Index of the key. </param>
        /// <returns> The key's mode, or <see cref="TangentMode.Free"/> for an out-of-range index. </returns>
        public TangentMode GetTangentMode(int index)
        {
            return index >= 0 && index < m_Keys.Count ? m_Keys[index].mode : TangentMode.Free;
        }

        /// <summary>
        /// Sets the tangent mode of a key and re-derives its tangents.
        /// </summary>
        /// <param name="index"> Index of the key. </param>
        /// <param name="mode"> The mode to apply. </param>
        public void SetTangentMode(int index, TangentMode mode)
        {
            if (index < 0 || index >= m_Keys.Count)
                return;

            m_Keys[index].mode = mode;
            Solve();
            Commit();
        }

        /// <summary>
        /// Authors one tangent by hand.
        /// </summary>
        /// <remarks>
        /// On a <see cref="TangentMode.Broken"/> key only the named side moves. On any other mode
        /// the key becomes <see cref="TangentMode.Free"/> and both sides take the slope, which
        /// keeps the curve smooth through the key.
        /// </remarks>
        /// <param name="index"> Index of the key. </param>
        /// <param name="side"> Which tangent to set. </param>
        /// <param name="slope"> The slope, in value units per time unit. </param>
        public void SetTangent(int index, TangentSide side, float slope)
        {
            if (index < 0 || index >= m_Keys.Count)
                return;

            var key = m_Keys[index];
            if (key.mode == TangentMode.Broken)
            {
                if (side == TangentSide.In)
                    key.inTangent = slope;
                else
                    key.outTangent = slope;
            }
            else
            {
                key.mode = TangentMode.Free;
                key.inTangent = slope;
                key.outTangent = slope;
            }

            Commit();
        }

        /// <summary>
        /// Replaces the curve with a preset.
        /// </summary>
        /// <param name="index"> Index into <see cref="presets"/>. </param>
        public void ApplyPreset(int index)
        {
            if (index < 0 || index >= k_Presets.Length)
                return;

            m_Keys.Clear();
            foreach (var state in k_Presets[index].states)
                m_Keys.Add(state.Copy());

            m_Selected = null;
            RecomputeRange();
            Solve();
            Commit();
        }

        // ------------------------------------------------------------------ ICustomAnimationCurveFieldPicker

        /// <summary>
        /// Opens the editor in a <see cref="Popover"/> anchored to the field, as
        /// <see cref="ColorField"/> does with <see cref="ColorPicker"/>.
        /// </summary>
        /// <param name="owner"> The field that requested the editor. </param>
        /// <param name="onValueChanging"> Invoked as the user edits. </param>
        /// <param name="initialValue"> The curve assigned when the editor opened. </param>
        public void Show(AnimationCurveField owner, Action<AnimationCurve> onValueChanging, AnimationCurve initialValue)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            owner.AddToClassList(Styles.focusedUssClassName);

            m_OnValueChanging = onValueChanging;
            SetValueWithoutNotify(initialValue);

            m_Popover = Popover.Build(owner, this);
            m_Popover.SetAnchor(owner);
            m_Popover.SetArrowVisible(false);
            m_Popover.SetMovable(true);
            m_Popover.SetLastFocusedElement(owner);
            m_Popover.SetPlacement(PopoverPlacement.End);
            m_Popover.SetShouldFlip(true);
            m_Popover.dismissed += (_, _) =>
            {
                m_OnValueChanging = null;
                owner.RemoveFromClassList(Styles.focusedUssClassName);
                m_Popover = null;
                owner.Focus();
            };
            m_Popover.shown += popup => popup.SetAnchor(null);
            m_Popover.Show();
        }

        /// <summary>
        /// Closes the popover this editor was opened in, if any.
        /// </summary>
        public void Dismiss()
        {
            m_Popover?.Dismiss(Unity.AppUI.Core.DismissType.Manual);
            m_Popover = null;
        }

        // ------------------------------------------------------------------ geometry

        /// <summary>
        /// Maps a point in curve space onto <see cref="graphElement"/>'s local pixels.
        /// </summary>
        /// <param name="time"> A time in the plotted range. </param>
        /// <param name="value"> A value in the plotted range. </param>
        /// <returns> The pixel position, with y growing downward. </returns>
        public Vector2 DomainToPlot(float time, float value)
        {
            var rect = plotRect;
            var timeSpan = Mathf.Max(m_TimeMax - m_TimeMin, Mathf.Epsilon);
            var valueSpan = Mathf.Max(m_ValueMax - m_ValueMin, Mathf.Epsilon);
            return new Vector2(
                rect.xMin + (time - m_TimeMin) / timeSpan * rect.width,
                rect.yMax - (value - m_ValueMin) / valueSpan * rect.height);
        }

        /// <summary>
        /// Maps a point in <see cref="graphElement"/>'s local pixels back into curve space.
        /// </summary>
        /// <param name="local"> A pixel position, with y growing downward. </param>
        /// <returns> The time in x and the value in y, both unclamped. </returns>
        public Vector2 PlotToDomain(Vector2 local)
        {
            var rect = plotRect;
            if (rect.width <= Mathf.Epsilon || rect.height <= Mathf.Epsilon)
                return Vector2.zero;
            var timeSpan = m_TimeMax - m_TimeMin;
            var valueSpan = m_ValueMax - m_ValueMin;
            return new Vector2(
                m_TimeMin + (local.x - rect.xMin) / rect.width * timeSpan,
                m_ValueMin + (rect.yMax - local.y) / rect.height * valueSpan);
        }

        // ------------------------------------------------------------------ internals

        static string ModeLabel(TangentMode mode)
        {
            return mode switch
            {
                TangentMode.ClampedAuto => "Clamped Auto",
                TangentMode.Auto => "Auto",
                TangentMode.Free => "Free Smooth",
                TangentMode.Flat => "Flat",
                TangentMode.Broken => "Broken",
                TangentMode.Linear => "Linear",
                _ => mode.ToString(),
            };
        }

        internal static AnimationCurve BuildCurve(KeyState[] states)
        {
            var frames = new Keyframe[states.Length];
            for (var i = 0; i < states.Length; i++)
            {
                frames[i] = new Keyframe(
                    states[i].time,
                    states[i].value,
                    states[i].inTangent,
                    states[i].outTangent);
            }
            return new AnimationCurve(frames);
        }

        /// <summary>
        /// The plotted band, taken from the curve exactly as <see cref="AnimationCurveField"/> takes its
        /// thumbnail's. Recomputed only on load and on a preset, never mid-edit, so the graph does
        /// not move under the pointer.
        /// </summary>
        void RecomputeRange()
        {
            m_TimeMin = m_Keys[0].time;
            m_TimeMax = m_Keys[m_Keys.Count - 1].time;
            if (m_TimeMax - m_TimeMin <= Mathf.Epsilon)
            {
                m_TimeMin = m_Keys[0].time;
                m_TimeMax = m_TimeMin + 1f;
            }

            m_ValueMin = 0f;
            m_ValueMax = 1f;
            foreach (var key in m_Keys)
            {
                if (key.value < m_ValueMin) m_ValueMin = key.value;
                if (key.value > m_ValueMax) m_ValueMax = key.value;
            }
        }

        /// <summary>
        /// Re-derives every solved key's tangents from the geometry around it. Free and Broken keys
        /// are authored by hand and are left alone.
        /// </summary>
        void Solve()
        {
            for (var i = 0; i < m_Keys.Count; i++)
            {
                var key = m_Keys[i];
                if (key.mode is TangentMode.Free or TangentMode.Broken)
                    continue;

                var prev = i > 0 ? m_Keys[i - 1] : null;
                var next = i < m_Keys.Count - 1 ? m_Keys[i + 1] : null;

                if (key.mode == TangentMode.Flat)
                {
                    key.inTangent = 0f;
                    key.outTangent = 0f;
                    continue;
                }

                if (key.mode == TangentMode.Linear)
                {
                    key.inTangent = prev != null ? Slope(prev, key) : 0f;
                    key.outTangent = next != null ? Slope(key, next) : 0f;
                    continue;
                }

                float slope;
                if (prev != null && next != null)
                    slope = Slope(prev, next);
                else if (next != null)
                    slope = Slope(key, next);
                else if (prev != null)
                    slope = Slope(prev, key);
                else
                    slope = 0f;

                if (key.mode == TangentMode.ClampedAuto && prev != null && next != null)
                {
                    var isMax = key.value >= prev.value && key.value >= next.value;
                    var isMin = key.value <= prev.value && key.value <= next.value;
                    if (isMax || isMin)
                        slope = 0f;
                }

                key.inTangent = slope;
                key.outTangent = slope;
            }
        }

        static float Slope(KeyState a, KeyState b)
        {
            var dt = b.time - a.time;
            return Mathf.Abs(dt) <= Mathf.Epsilon ? 0f : (b.value - a.value) / dt;
        }

        /// <summary>
        /// Applies the current keys everywhere and reports the change. Every mutating entry point
        /// funnels through here, so no path can update the model and forget the view.
        /// </summary>
        void Commit()
        {
            var previous = m_LastNotified;
            Refresh();
            var next = BuildCurve(m_Keys.ToArray());
            m_LastNotified = next;

            using (var evt = ChangeEvent<AnimationCurve>.GetPooled(previous, next))
            {
                evt.target = this;
                SendEvent(evt);
            }

            NotifyPropertyChanged(in valueProperty);
            m_OnValueChanging?.Invoke(BuildCurve(m_Keys.ToArray()));
        }

        void Refresh()
        {
            RefreshMarkers();
            RefreshHandles();
            RefreshToolbar();
            m_Graph.MarkDirtyRepaint();
        }

        void RefreshMarkers()
        {
            while (m_KeyMarkers.Count > m_Keys.Count)
            {
                var last = m_KeyMarkers[m_KeyMarkers.Count - 1];
                m_Graph.Remove(last);
                m_KeyMarkers.RemoveAt(m_KeyMarkers.Count - 1);
            }

            while (m_KeyMarkers.Count < m_Keys.Count)
            {
                var marker = new VisualElement { pickingMode = PickingMode.Position };
                marker.AddToClassList(keyUssClassName);
                m_Graph.Add(marker);
                m_KeyMarkers.Add(marker);
            }

            for (var i = 0; i < m_Keys.Count; i++)
            {
                var key = m_Keys[i];
                var marker = m_KeyMarkers[i];
                var point = DomainToPlot(key.time, key.value);
                marker.userData = key;
                marker.style.left = point.x;
                marker.style.top = point.y;
                marker.style.translate = new Translate(Length.Percent(-50f), Length.Percent(-50f));
                marker.EnableInClassList(selectedKeyUssClassName, ReferenceEquals(key, m_Selected));
            }
        }

        void RefreshHandles()
        {
            if (m_Selected == null)
            {
                m_InHandle.style.display = DisplayStyle.None;
                m_OutHandle.style.display = DisplayStyle.None;
                return;
            }

            m_InHandle.style.display = DisplayStyle.Flex;
            m_OutHandle.style.display = DisplayStyle.Flex;

            var keyPoint = DomainToPlot(m_Selected.time, m_Selected.value);
            Place(m_InHandle, HandlePoint(keyPoint, m_Selected.inTangent, TangentSide.In));
            Place(m_OutHandle, HandlePoint(keyPoint, m_Selected.outTangent, TangentSide.Out));

            static void Place(VisualElement handle, Vector2 point)
            {
                handle.style.left = point.x;
                handle.style.top = point.y;
                handle.style.translate = new Translate(Length.Percent(-50f), Length.Percent(-50f));
            }
        }

        /// <summary>
        /// Where a tangent handle sits: a fixed pixel radius from the key, along the tangent
        /// direction. A slope of +1 rises, and plot y grows downward, so the out handle goes up and
        /// to the right while the in handle goes down and to the left.
        /// </summary>
        Vector2 HandlePoint(Vector2 keyPoint, float slope, TangentSide side)
        {
            var rect = plotRect;
            var timeSpan = Mathf.Max(m_TimeMax - m_TimeMin, Mathf.Epsilon);
            var valueSpan = Mathf.Max(m_ValueMax - m_ValueMin, Mathf.Epsilon);

            // One time unit is this many pixels across, one value unit this many pixels up.
            var pxPerTime = rect.width / timeSpan;
            var pxPerValue = rect.height / valueSpan;

            var dx = side == TangentSide.Out ? pxPerTime : -pxPerTime;
            var dy = side == TangentSide.Out ? -slope * pxPerValue : slope * pxPerValue;
            var length = Mathf.Sqrt(dx * dx + dy * dy);
            if (length <= Mathf.Epsilon)
                return keyPoint;

            return new Vector2(
                keyPoint.x + dx / length * handleRadius,
                keyPoint.y + dy / length * handleRadius);
        }

        void RefreshToolbar()
        {
            m_Syncing = true;
            try
            {
                m_TangentDropdown.SetEnabled(m_Selected != null);
                if (m_Selected == null)
                {
                    m_TangentDropdown.SetValueWithoutNotify(Array.Empty<int>());
                    return;
                }

                var index = Array.IndexOf(k_ModeOrder, m_Selected.mode);
                m_TangentDropdown.SetValueWithoutNotify(new[] { Mathf.Max(index, 0) });
            }
            finally
            {
                m_Syncing = false;
            }
        }

        VisualElement MakeHandle(TangentSide side)
        {
            var handle = new VisualElement { pickingMode = PickingMode.Position };
            handle.AddToClassList(handleUssClassName);
            handle.style.display = DisplayStyle.None;
            handle.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (m_Selected == null)
                    return;
                m_DragKind = DragKind.Handle;
                m_DragSide = side;
                m_Graph.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });
            return handle;
        }

        VisualElement MakePresetButton(int index)
        {
            var button = new VisualElement { pickingMode = PickingMode.Position, focusable = true };
            button.AddToClassList(presetUssClassName);
            button.tooltip = k_Presets[index].label;

            var glyph = new PresetGlyph(k_Presets[index].states) { pickingMode = PickingMode.Ignore };
            glyph.AddToClassList(presetGlyphUssClassName);
            button.Add(glyph);

            button.AddManipulator(new Pressable(() => ApplyPreset(index)));
            return button;
        }

        void OnGraphPointerDown(PointerDownEvent evt)
        {
            var marker = FindMarker(evt.target as VisualElement);
            if (marker != null)
            {
                m_Selected = marker.userData as KeyState;
                Refresh();
                m_DragKind = DragKind.Key;
                m_Graph.CapturePointer(evt.pointerId);
                evt.StopPropagation();
                return;
            }

            if (evt.clickCount >= 2)
            {
                var domain = PlotToDomain(m_Graph.WorldToLocal(evt.position));
                AddKey(domain.x, domain.y);
                evt.StopPropagation();
                return;
            }

            // A press on empty graph clears the selection, which also hides the tangent handles.
            if (m_Selected != null)
            {
                m_Selected = null;
                Refresh();
            }
        }

        void OnGraphPointerMove(PointerMoveEvent evt)
        {
            if (m_DragKind == DragKind.None || m_Selected == null)
                return;

            var local = m_Graph.WorldToLocal(evt.position);

            if (m_DragKind == DragKind.Key)
            {
                var domain = PlotToDomain(local);
                MoveSelectedKey(domain.x, domain.y);
            }
            else
            {
                DragHandleTo(local, evt.altKey);
            }

            evt.StopPropagation();
        }

        /// <summary>
        /// Turns a pointer position into a slope for the handle being dragged.
        /// </summary>
        /// <remarks>
        /// The horizontal offset is forced to at least a few pixels on the handle's own side. Left
        /// unclamped it passes through zero as the pointer crosses the key, and the slope goes to
        /// infinity and flips sign — which reads as the handle snapping to vertical and then
        /// inverting.
        /// </remarks>
        void DragHandleTo(Vector2 local, bool breakTangent)
        {
            var index = m_Keys.IndexOf(m_Selected);
            if (index < 0)
                return;

            var rect = plotRect;
            if (rect.width <= Mathf.Epsilon || rect.height <= Mathf.Epsilon)
                return;

            var keyPoint = DomainToPlot(m_Selected.time, m_Selected.value);
            var dx = local.x - keyPoint.x;
            var dy = local.y - keyPoint.y;

            const float minPx = 4f;
            dx = m_DragSide == TangentSide.Out ? Mathf.Max(minPx, dx) : Mathf.Min(-minPx, dx);

            var timeSpan = Mathf.Max(m_TimeMax - m_TimeMin, Mathf.Epsilon);
            var valueSpan = Mathf.Max(m_ValueMax - m_ValueMin, Mathf.Epsilon);
            var dTime = dx / rect.width * timeSpan;
            var dValue = -dy / rect.height * valueSpan;
            var slope = dValue / dTime;

            if (breakTangent)
                m_Selected.mode = TangentMode.Broken;

            SetTangent(index, m_DragSide, slope);
        }

        void OnGraphPointerUp(PointerUpEvent evt)
        {
            if (m_DragKind == DragKind.None)
                return;
            m_DragKind = DragKind.None;
            if (m_Graph.HasPointerCapture(evt.pointerId))
                m_Graph.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }

        VisualElement FindMarker(VisualElement from)
        {
            while (from != null && from != m_Graph)
            {
                if (from.ClassListContains(keyUssClassName))
                    return from;
                from = from.parent;
            }
            return null;
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (m_Selected == null)
                return;

            if (evt.keyCode is KeyCode.Delete or KeyCode.Backspace)
            {
                if (RemoveSelectedKey())
                    evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.F)
            {
                SetTangentMode(m_Keys.IndexOf(m_Selected), TangentMode.Flat);
                evt.StopPropagation();
            }
        }

        void OnTangentModeChanged(ChangeEvent<IEnumerable<int>> evt)
        {
            if (m_Syncing || m_Selected == null)
                return;
            var index = m_TangentDropdown.selectedIndex;
            if (index < 0 || index >= k_ModeOrder.Length)
                return;
            SetTangentMode(m_Keys.IndexOf(m_Selected), k_ModeOrder[index]);
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            var changed = false;

            if (evt.customStyle.TryGetValue(k_UssCurveColor, out var curveColor) && curveColor != m_CurveColor)
            {
                m_CurveColor = curveColor;
                changed = true;
            }

            if (evt.customStyle.TryGetValue(k_UssCurveWidth, out var curveWidth) && !Mathf.Approximately(curveWidth, m_CurveWidth))
            {
                m_CurveWidth = curveWidth;
                changed = true;
            }

            if (evt.customStyle.TryGetValue(k_UssGridColor, out var gridColor) && gridColor != m_GridColor)
            {
                m_GridColor = gridColor;
                changed = true;
            }

            if (evt.customStyle.TryGetValue(k_UssGridWidth, out var gridWidth) && !Mathf.Approximately(gridWidth, m_GridWidth))
            {
                m_GridWidth = gridWidth;
                changed = true;
            }

            if (changed)
                m_Graph.MarkDirtyRepaint();
        }

        void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            var rect = plotRect;
            if (rect.width <= Mathf.Epsilon || rect.height <= Mathf.Epsilon || m_Keys.Count == 0)
                return;

            var painter = mgc.painter2D;

            // Grid.
            painter.lineWidth = m_GridWidth;
            painter.strokeColor = m_GridColor;
            painter.lineCap = LineCap.Butt;
            for (var i = 0; i <= 4; i++)
            {
                var f = i / 4f;
                var y = Mathf.Lerp(rect.yMax, rect.yMin, f);
                var x = Mathf.Lerp(rect.xMin, rect.xMax, f);

                painter.BeginPath();
                painter.MoveTo(new Vector2(rect.xMin, y));
                painter.LineTo(new Vector2(rect.xMax, y));
                painter.Stroke();

                painter.BeginPath();
                painter.MoveTo(new Vector2(x, rect.yMin));
                painter.LineTo(new Vector2(x, rect.yMax));
                painter.Stroke();
            }

            // Tangent arms of the selected key, drawn under the handles.
            if (m_Selected != null)
            {
                var keyPoint = DomainToPlot(m_Selected.time, m_Selected.value);
                painter.lineWidth = m_GridWidth;
                painter.strokeColor = m_CurveColor;
                foreach (var side in new[] { TangentSide.In, TangentSide.Out })
                {
                    var slope = side == TangentSide.In ? m_Selected.inTangent : m_Selected.outTangent;
                    painter.BeginPath();
                    painter.MoveTo(keyPoint);
                    painter.LineTo(HandlePoint(keyPoint, slope, side));
                    painter.Stroke();
                }
            }

            // Curve, sampled through the engine so what is drawn is what the value evaluates to.
            var curve = BuildCurve(m_Keys.ToArray());
            painter.lineWidth = m_CurveWidth;
            painter.strokeColor = m_CurveColor;
            painter.lineJoin = LineJoin.Round;
            painter.BeginPath();
            for (var i = 0; i < k_SampleCount; i++)
            {
                var f = i / (float)(k_SampleCount - 1);
                var time = Mathf.Lerp(m_TimeMin, m_TimeMax, f);
                var point = DomainToPlot(time, curve.Evaluate(time));
                if (i == 0)
                    painter.MoveTo(point);
                else
                    painter.LineTo(point);
            }
            painter.Stroke();
        }

        /// <summary>
        /// The little curve drawn inside a preset button. It reads the same custom properties the
        /// graph does, so the preset row is tinted from tokens rather than from a literal.
        /// </summary>
        class PresetGlyph : VisualElement
        {
            readonly KeyState[] m_States;

            Color m_Color = Color.white;

            float m_Width = 1.5f;

            public PresetGlyph(KeyState[] states)
            {
                m_States = states;
                generateVisualContent += OnGenerate;
                RegisterCallback<CustomStyleResolvedEvent>(evt =>
                {
                    if (evt.customStyle.TryGetValue(k_UssCurveColor, out var color))
                        m_Color = color;
                    if (evt.customStyle.TryGetValue(k_UssCurveWidth, out var width))
                        m_Width = width;
                    MarkDirtyRepaint();
                });
            }

            void OnGenerate(MeshGenerationContext mgc)
            {
                var rect = contentRect;
                if (rect.width <= Mathf.Epsilon || rect.height <= Mathf.Epsilon)
                    return;

                var curve = BuildCurve(m_States);
                var min = 0f;
                var max = 1f;
                foreach (var state in m_States)
                {
                    if (state.value < min) min = state.value;
                    if (state.value > max) max = state.value;
                }
                var span = Mathf.Max(max - min, Mathf.Epsilon);

                var painter = mgc.painter2D;
                painter.lineWidth = m_Width;
                painter.strokeColor = m_Color;
                painter.lineJoin = LineJoin.Round;
                painter.BeginPath();
                const int samples = 24;
                for (var i = 0; i < samples; i++)
                {
                    var f = i / (float)(samples - 1);
                    var v = curve.Evaluate(Mathf.Lerp(m_States[0].time, m_States[m_States.Length - 1].time, f));
                    var point = new Vector2(
                        rect.xMin + f * rect.width,
                        rect.yMax - (v - min) / span * rect.height);
                    if (i == 0)
                        painter.MoveTo(point);
                    else
                        painter.LineTo(point);
                }
                painter.Stroke();
            }
        }
    }
}
