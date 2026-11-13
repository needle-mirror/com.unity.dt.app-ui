using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Unity's gradient stop-bar editor: alpha stops above a live preview strip, color stops below,
    /// and an inspector for whichever stop is selected.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The editor is the counterpart <see cref="ColorGradientField"/> has been waiting for. It implements
    /// <see cref="ICustomColorGradientFieldPicker"/>, so assigning one to
    /// <see cref="ColorGradientField.customPicker"/> is all it takes to turn the field from a read-only
    /// gradient well into an editable one — exactly how <see cref="ColorField"/> and
    /// <see cref="ColorPicker"/> pair up.
    /// </para>
    ///
    /// <para>
    /// Everything visual is reused rather than reinvented. The preview strip is a
    /// <see cref="ColorSwatch"/>, the same shader path <see cref="ColorGradientField"/>'s own preview
    /// uses, so <see cref="GradientMode.Fixed"/> and the transparency checkerboard are rendered
    /// once and identically in both places. The selected color stop is edited with a
    /// <see cref="ColorField"/>, which brings the whole <see cref="ColorPicker"/> with it, and the
    /// selected alpha stop with a <see cref="ColorSlider"/> whose track is the sampled stop color
    /// fading to transparent.
    /// </para>
    ///
    /// <para>
    /// Stops are held sorted by time. Dragging one past a neighbour therefore reorders the list,
    /// and the selection follows the stop rather than the index, so a drag never silently starts
    /// editing a different stop. Both rails are capped at <see cref="maxKeys"/>, which is
    /// <see cref="Gradient"/>'s own limit: a ninth key would be dropped by
    /// <see cref="Gradient.SetKeys"/> without an error.
    /// </para>
    ///
    /// <para>Interaction</para>
    /// <para>
    /// <list type="bullet">
    /// <item><description>Click a stop to select it, drag to move it.</description></item>
    /// <item><description>Double-click an empty rail to add a stop there, sampled from the current
    /// gradient so inserting one does not change how the gradient looks.</description></item>
    /// <item><description><c>Delete</c> or <c>Backspace</c> removes the selected stop, never the
    /// last one on a rail.</description></item>
    /// </list>
    /// </para>
    ///
    /// <para>Example</para>
    /// <code>
    /// var field = new ColorGradientField { customPicker = new GradientEditor() };
    /// field.RegisterValueChangedCallback(evt => Debug.Log(evt.newValue));
    /// </code>
    /// </remarks>
    public class GradientEditor : VisualElement, INotifyValueChanged<Gradient>, ICustomColorGradientFieldPicker
    {
        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId modeProperty = nameof(mode);

        internal static readonly BindingId hdrProperty = nameof(hdr);

        /// <summary>
        /// The most keys a <see cref="Gradient"/> can hold on either rail. A ninth key is silently
        /// dropped by <see cref="Gradient.SetKeys"/>, so the editor refuses to add one instead.
        /// </summary>
        public const int maxKeys = 8;

        /// <summary>
        /// The fewest keys a <see cref="Gradient"/> can hold on either rail.
        /// </summary>
        /// <remarks>
        /// <see cref="Gradient.SetKeys"/> silently pads anything shorter back up to two, so a rail
        /// that let the user delete down to one key would appear to work and then hand back a
        /// gradient with a duplicated key. The editor refuses the deletion instead.
        /// </remarks>
        public const int minKeys = 2;

        /// <summary>
        /// The GradientEditor main styling class.
        /// </summary>
        public const string ussClassName = "appui-gradienteditor";

        /// <summary>
        /// The alpha stop rail styling class.
        /// </summary>
        public const string alphaRailUssClassName = ussClassName + "__alpha-rail";

        /// <summary>
        /// The color stop rail styling class.
        /// </summary>
        public const string colorRailUssClassName = ussClassName + "__color-rail";

        /// <summary>
        /// The preview strip styling class.
        /// </summary>
        public const string stripUssClassName = ussClassName + "__strip";

        /// <summary>
        /// The stop marker styling class.
        /// </summary>
        public const string markerUssClassName = ussClassName + "__marker";

        /// <summary>
        /// The stop marker chip styling class. This is the part tinted with the stop's own value.
        /// </summary>
        public const string markerChipUssClassName = ussClassName + "__marker-chip";

        /// <summary>
        /// The selected stop marker styling class.
        /// </summary>
        public const string selectedMarkerUssClassName = markerUssClassName + "--selected";

        /// <summary>
        /// The inspector styling class.
        /// </summary>
        public const string inspectorUssClassName = ussClassName + "__inspector";

        /// <summary>
        /// The inspector row styling class.
        /// </summary>
        public const string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The inspector label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The gradient mode dropdown styling class.
        /// </summary>
        public const string modeDropdownUssClassName = ussClassName + "__mode-dropdown";

        /// <summary>
        /// The stop location field styling class.
        /// </summary>
        public const string locationFieldUssClassName = ussClassName + "__location-field";

        /// <summary>
        /// The stop color field styling class.
        /// </summary>
        public const string colorFieldUssClassName = ussClassName + "__color-field";

        /// <summary>
        /// The stop alpha slider styling class.
        /// </summary>
        public const string alphaSliderUssClassName = ussClassName + "__alpha-slider";

        /// <summary>
        /// The HDR styling class.
        /// </summary>
        public const string hdrUssClassName = ussClassName + "--hdr";

        /// <summary>
        /// Which rail a stop lives on.
        /// </summary>
        public enum KeyKind
        {
            /// <summary>
            /// A <see cref="GradientColorKey"/>, on the rail below the strip.
            /// </summary>
            Color,

            /// <summary>
            /// A <see cref="GradientAlphaKey"/>, on the rail above the strip.
            /// </summary>
            Alpha
        }

        // Stops are held as mutable reference objects rather than as the engine's value-type keys.
        // The list is kept sorted, so a drag past a neighbour reorders it, and only a reference
        // survives that reorder — an index does not. This is what keeps the selection on the stop
        // the user grabbed.
        class ColorStop
        {
            public Color color;
            public float time;
        }

        class AlphaStop
        {
            public float alpha;
            public float time;
        }

        readonly List<ColorStop> m_ColorStops = new List<ColorStop>();

        readonly List<AlphaStop> m_AlphaStops = new List<AlphaStop>();

        readonly List<VisualElement> m_ColorMarkers = new List<VisualElement>();

        readonly List<VisualElement> m_AlphaMarkers = new List<VisualElement>();

        readonly VisualElement m_AlphaRail;

        readonly VisualElement m_ColorRail;

        readonly ColorSwatch m_Strip;

        readonly Dropdown m_ModeDropdown;

        readonly IntField m_LocationField;

        readonly ColorField m_ColorField;

        readonly ColorSlider m_AlphaSlider;

        readonly LocalizedTextElement m_StopLabel;

        readonly VisualElement m_StopRow;

        GradientMode m_Mode = GradientMode.Blend;

        bool m_Hdr;

        object m_Selected;

        KeyKind m_SelectedKind = KeyKind.Color;

        bool m_Dragging;

        bool m_Syncing;

        Gradient m_LastNotified;

        Popover m_Popover;

        Action<Gradient> m_OnValueChanging;

        /// <summary>
        /// Default constructor. Starts on an opaque black-to-white blend.
        /// </summary>
        public GradientEditor()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;

            m_AlphaRail = MakeRail(alphaRailUssClassName, KeyKind.Alpha);
            hierarchy.Add(m_AlphaRail);

            m_Strip = new ColorSwatch
            {
                name = stripUssClassName,
                pickingMode = PickingMode.Position,
                round = false,
            };
            m_Strip.AddToClassList(stripUssClassName);
            m_Strip.RegisterCallback<PointerDownEvent>(OnStripPointerDown);
            hierarchy.Add(m_Strip);

            m_ColorRail = MakeRail(colorRailUssClassName, KeyKind.Color);
            hierarchy.Add(m_ColorRail);

            var inspector = new VisualElement { name = inspectorUssClassName };
            inspector.AddToClassList(inspectorUssClassName);
            hierarchy.Add(inspector);

            var modeRow = new VisualElement();
            modeRow.AddToClassList(rowUssClassName);
            inspector.Add(modeRow);

            var modeLabel = new LocalizedTextElement("Mode");
            modeLabel.AddToClassList(labelUssClassName);
            modeRow.Add(modeLabel);

            var modeItems = new List<string> { "Blend", "Fixed" };
            m_ModeDropdown = new Dropdown(modeItems, (item, i) => item.label = modeItems[i])
            {
                name = modeDropdownUssClassName,
            };
            m_ModeDropdown.AddToClassList(modeDropdownUssClassName);
            m_ModeDropdown.selectedIndex = 0;
            m_ModeDropdown.RegisterValueChangedCallback(OnModeChanged);
            modeRow.Add(m_ModeDropdown);

            var locationLabel = new LocalizedTextElement("Location");
            locationLabel.AddToClassList(labelUssClassName);
            modeRow.Add(locationLabel);

            m_LocationField = new IntField
            {
                name = locationFieldUssClassName,
                lowValue = 0,
                highValue = 100,
                unit = "%",
            };
            m_LocationField.AddToClassList(locationFieldUssClassName);
            m_LocationField.RegisterValueChangedCallback(OnLocationChanged);
            m_LocationField.RegisterValueChangingCallback(OnLocationChanging);
            modeRow.Add(m_LocationField);

            m_StopRow = new VisualElement();
            m_StopRow.AddToClassList(rowUssClassName);
            inspector.Add(m_StopRow);

            m_StopLabel = new LocalizedTextElement("Color");
            m_StopLabel.AddToClassList(labelUssClassName);
            m_StopRow.Add(m_StopLabel);

            m_ColorField = new ColorField
            {
                name = colorFieldUssClassName,
                showAlpha = false,
                showText = false,
            };
            m_ColorField.AddToClassList(colorFieldUssClassName);
            m_ColorField.RegisterValueChangedCallback(OnStopColorChanged);
            m_StopRow.Add(m_ColorField);

            m_AlphaSlider = new ColorSlider
            {
                name = alphaSliderUssClassName,
                lowValue = 0f,
                highValue = 1f,
            };
            m_AlphaSlider.AddToClassList(alphaSliderUssClassName);
            m_AlphaSlider.RegisterValueChangedCallback(OnStopAlphaChanged);
            m_AlphaSlider.RegisterValueChangingCallback(OnStopAlphaChanging);
            m_StopRow.Add(m_AlphaSlider);

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<ChangeEvent<Gradient>>(OnAnyGradientChange);

            SetValueWithoutNotify(DefaultGradient());
            SelectKey(KeyKind.Color, 0);
        }

        /// <inheritdoc/>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The rail carrying the alpha stops, above the strip.
        /// </summary>
        public VisualElement alphaRailElement => m_AlphaRail;

        /// <summary>
        /// The rail carrying the color stops, below the strip.
        /// </summary>
        public VisualElement colorRailElement => m_ColorRail;

        /// <summary>
        /// The <see cref="ColorSwatch"/> rendering the composited preview.
        /// </summary>
        public ColorSwatch stripElement => m_Strip;

        /// <summary>
        /// The <see cref="ColorField"/> editing the selected color stop.
        /// </summary>
        public ColorField colorElement => m_ColorField;

        /// <summary>
        /// The <see cref="ColorSlider"/> editing the selected alpha stop.
        /// </summary>
        public ColorSlider alphaElement => m_AlphaSlider;

        /// <summary>
        /// The gradient mode dropdown.
        /// </summary>
        public Dropdown modeElement => m_ModeDropdown;

        /// <summary>
        /// The stop location field, in percent.
        /// </summary>
        public IntField locationElement => m_LocationField;

        /// <summary>
        /// The color stop markers, in time order.
        /// </summary>
        public IReadOnlyList<VisualElement> colorMarkers => m_ColorMarkers;

        /// <summary>
        /// The alpha stop markers, in time order.
        /// </summary>
        public IReadOnlyList<VisualElement> alphaMarkers => m_AlphaMarkers;

        /// <summary>
        /// Which rail the selected stop lives on.
        /// </summary>
        public KeyKind selectedKeyKind => m_SelectedKind;

        /// <summary>
        /// The index of the selected stop on its rail, or -1 when nothing is selected.
        /// </summary>
        public int selectedKeyIndex
        {
            get
            {
                if (m_Selected is ColorStop c)
                    return m_ColorStops.IndexOf(c);
                if (m_Selected is AlphaStop a)
                    return m_AlphaStops.IndexOf(a);
                return -1;
            }
        }

        /// <summary>
        /// Whether the color stop editor offers HDR colors. Forwarded from
        /// <see cref="ColorGradientField.hdr"/> when the editor is opened as its picker.
        /// </summary>
        [CreateProperty]
        public bool hdr
        {
            get => m_Hdr;
            set
            {
                var changed = m_Hdr != value;
                m_Hdr = value;
                m_ColorField.hdr = value;
                EnableInClassList(hdrUssClassName, value);
                if (changed)
                    NotifyPropertyChanged(in hdrProperty);
            }
        }

        /// <summary>
        /// How the gradient interpolates between its color stops.
        /// </summary>
        /// <remarks>
        /// The dropdown offers <see cref="GradientMode.Blend"/> and <see cref="GradientMode.Fixed"/>,
        /// matching the registry. <see cref="GradientMode.PerceptualBlend"/> can still be set from
        /// code; it renders as a blend.
        /// </remarks>
        [CreateProperty]
        public GradientMode mode
        {
            get => m_Mode;
            set
            {
                if (m_Mode == value)
                    return;
                m_Mode = value;
                NotifyPropertyChanged(in modeProperty);
                Commit();
            }
        }

        /// <summary>
        /// The gradient being edited.
        /// </summary>
        /// <remarks>
        /// The getter builds a fresh <see cref="Gradient"/> on every read, so a consumer holding an
        /// earlier value never sees it mutate underneath them.
        /// </remarks>
        [CreateProperty]
        public Gradient value
        {
            get => BuildGradient();
            set
            {
                var previous = m_LastNotified ?? BuildGradient();
                SetValueWithoutNotify(value);
                var next = BuildGradient();
                m_LastNotified = next;
                using var evt = ChangeEvent<Gradient>.GetPooled(previous, next);
                evt.target = this;
                SendEvent(evt);
                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// Sets the gradient without sending a change event.
        /// </summary>
        /// <param name="newValue"> The gradient to edit. Null resets to the default. </param>
        public void SetValueWithoutNotify(Gradient newValue)
        {
            var source = newValue ?? DefaultGradient();

            m_Mode = source.mode;
            m_ColorStops.Clear();
            foreach (var key in source.colorKeys)
                m_ColorStops.Add(new ColorStop { color = key.color, time = Mathf.Clamp01(key.time) });
            m_AlphaStops.Clear();
            foreach (var key in source.alphaKeys)
                m_AlphaStops.Add(new AlphaStop { alpha = Mathf.Clamp01(key.alpha), time = Mathf.Clamp01(key.time) });

            // Gradient guarantees at least two keys per rail. Pad rather than trust the input, so
            // the rails and the value can never disagree about how many stops there are.
            while (m_ColorStops.Count < minKeys)
                m_ColorStops.Add(new ColorStop { color = Color.white, time = m_ColorStops.Count });
            while (m_AlphaStops.Count < minKeys)
                m_AlphaStops.Add(new AlphaStop { alpha = 1f, time = m_AlphaStops.Count });

            SortStops();

            // The reloaded stops are new objects, so the old selection cannot survive.
            m_Selected = m_ColorStops.Count > 0 ? m_ColorStops[0] : null;
            m_SelectedKind = KeyKind.Color;

            // Snapshot the loaded state as the baseline for the next change event. Left null, the
            // first interactive edit would report a null previousValue rather than the gradient the
            // caller just supplied — including on the normal Show(..., initialValue) path.
            m_LastNotified = BuildGradient();
            Refresh();
        }

        /// <summary>
        /// Selects a stop.
        /// </summary>
        /// <param name="kind"> Which rail the stop lives on. </param>
        /// <param name="index"> Index of the stop on that rail. Out-of-range clears the selection. </param>
        public void SelectKey(KeyKind kind, int index)
        {
            m_SelectedKind = kind;
            if (kind == KeyKind.Color)
                m_Selected = index >= 0 && index < m_ColorStops.Count ? m_ColorStops[index] : null;
            else
                m_Selected = index >= 0 && index < m_AlphaStops.Count ? m_AlphaStops[index] : null;

            Refresh();
        }

        /// <summary>
        /// Adds a color stop, sampling the current gradient at that time so the gradient looks
        /// unchanged, then selects it.
        /// </summary>
        /// <param name="time"> Normalized time, clamped to 0..1. </param>
        /// <returns> False when the rail already holds <see cref="maxKeys"/> stops. </returns>
        public bool AddColorKey(float time)
        {
            if (m_ColorStops.Count >= maxKeys)
                return false;

            var t = Mathf.Clamp01(time);
            var stop = new ColorStop { color = SampleColor(t), time = t };
            m_ColorStops.Add(stop);
            SortStops();
            m_Selected = stop;
            m_SelectedKind = KeyKind.Color;
            Commit();
            return true;
        }

        /// <summary>
        /// Adds an alpha stop, sampling the current alpha ramp at that time, then selects it.
        /// </summary>
        /// <param name="time"> Normalized time, clamped to 0..1. </param>
        /// <returns> False when the rail already holds <see cref="maxKeys"/> stops. </returns>
        public bool AddAlphaKey(float time)
        {
            if (m_AlphaStops.Count >= maxKeys)
                return false;

            var t = Mathf.Clamp01(time);
            var stop = new AlphaStop { alpha = SampleAlpha(t), time = t };
            m_AlphaStops.Add(stop);
            SortStops();
            m_Selected = stop;
            m_SelectedKind = KeyKind.Alpha;
            Commit();
            return true;
        }

        /// <summary>
        /// Removes the selected stop.
        /// </summary>
        /// <returns>
        /// False when nothing is selected, or when its rail is already down to
        /// <see cref="minKeys"/> stops.
        /// </returns>
        public bool RemoveSelectedKey()
        {
            if (m_Selected is ColorStop c)
            {
                if (m_ColorStops.Count <= minKeys)
                    return false;
                m_ColorStops.Remove(c);
            }
            else if (m_Selected is AlphaStop a)
            {
                if (m_AlphaStops.Count <= minKeys)
                    return false;
                m_AlphaStops.Remove(a);
            }
            else
            {
                return false;
            }

            m_Selected = null;
            Commit();
            return true;
        }

        /// <summary>
        /// Moves the selected stop in time. The selection follows the stop through the reorder that
        /// may cause.
        /// </summary>
        /// <param name="time"> Normalized time, clamped to 0..1. </param>
        public void SetSelectedKeyTime(float time)
        {
            var t = Mathf.Clamp01(time);
            switch (m_Selected)
            {
                case ColorStop c when !Mathf.Approximately(c.time, t):
                    c.time = t;
                    break;
                case AlphaStop a when !Mathf.Approximately(a.time, t):
                    a.time = t;
                    break;
                default:
                    return;
            }

            SortStops();
            Commit();
        }

        /// <summary>
        /// Sets the color of the selected color stop. Does nothing when an alpha stop is selected.
        /// </summary>
        /// <param name="color"> The new stop color. Alpha is carried by the alpha rail, not here. </param>
        public void SetSelectedKeyColor(Color color)
        {
            if (m_Selected is not ColorStop c)
                return;
            c.color = new Color(color.r, color.g, color.b, 1f);
            Commit();
        }

        /// <summary>
        /// Sets the alpha of the selected alpha stop. Does nothing when a color stop is selected.
        /// </summary>
        /// <param name="alpha"> The new alpha, clamped to 0..1. </param>
        public void SetSelectedKeyAlpha(float alpha)
        {
            if (m_Selected is not AlphaStop a)
                return;
            a.alpha = Mathf.Clamp01(alpha);
            Commit();
        }

        // ------------------------------------------------------------------ ICustomColorGradientFieldPicker

        /// <summary>
        /// Opens the editor in a <see cref="Popover"/> anchored to the field, as
        /// <see cref="ColorField"/> does with <see cref="ColorPicker"/>.
        /// </summary>
        /// <param name="owner"> The field that requested the editor. </param>
        /// <param name="onValueChanging"> Invoked as the user edits. </param>
        /// <param name="initialValue"> The gradient assigned when the editor opened. </param>
        /// <param name="showHdr"> Whether the editor should allow HDR color stops. </param>
        public void Show(ColorGradientField owner, Action<Gradient> onValueChanging, Gradient initialValue, bool showHdr)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            owner.AddToClassList(Styles.focusedUssClassName);

            m_OnValueChanging = onValueChanging;
            hdr = showHdr;
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

        // ------------------------------------------------------------------ internals

        static Gradient DefaultGradient()
        {
            // Black to white, fully opaque. The registry's default carries a mid blue stop, but
            // that is a demo color rather than a token, and inventing one here would put a literal
            // in the design system's own default.
            var gradient = new Gradient { mode = GradientMode.Blend };
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.black, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }

        VisualElement MakeRail(string className, KeyKind kind)
        {
            var rail = new VisualElement { name = className, pickingMode = PickingMode.Position };
            rail.AddToClassList(className);
            rail.RegisterCallback<PointerDownEvent>(evt => OnRailPointerDown(evt, kind));
            rail.RegisterCallback<PointerMoveEvent>(evt => OnRailPointerMove(evt, kind));
            rail.RegisterCallback<PointerUpEvent>(OnRailPointerUp);
            rail.RegisterCallback<PointerCaptureOutEvent>(_ => m_Dragging = false);
            return rail;
        }

        void SortStops()
        {
            m_ColorStops.Sort((a, b) => a.time.CompareTo(b.time));
            m_AlphaStops.Sort((a, b) => a.time.CompareTo(b.time));
        }

        Gradient BuildGradient()
        {
            var gradient = new Gradient { mode = m_Mode };

            var colors = new GradientColorKey[Mathf.Min(m_ColorStops.Count, maxKeys)];
            for (var i = 0; i < colors.Length; i++)
                colors[i] = new GradientColorKey(m_ColorStops[i].color, m_ColorStops[i].time);

            var alphas = new GradientAlphaKey[Mathf.Min(m_AlphaStops.Count, maxKeys)];
            for (var i = 0; i < alphas.Length; i++)
                alphas[i] = new GradientAlphaKey(m_AlphaStops[i].alpha, m_AlphaStops[i].time);

            gradient.SetKeys(colors, alphas);
            return gradient;
        }

        Color SampleColor(float t)
        {
            if (m_ColorStops.Count == 0)
                return Color.white;
            if (t <= m_ColorStops[0].time)
                return m_ColorStops[0].color;
            var last = m_ColorStops[m_ColorStops.Count - 1];
            if (t >= last.time)
                return last.color;

            for (var i = 0; i < m_ColorStops.Count - 1; i++)
            {
                var a = m_ColorStops[i];
                var b = m_ColorStops[i + 1];
                if (t < a.time || t > b.time)
                    continue;
                if (m_Mode == GradientMode.Fixed)
                    return a.color;
                var span = b.time - a.time;
                var f = span > Mathf.Epsilon ? (t - a.time) / span : 0f;
                return Color.Lerp(a.color, b.color, f);
            }

            return m_ColorStops[0].color;
        }

        float SampleAlpha(float t)
        {
            if (m_AlphaStops.Count == 0)
                return 1f;
            if (t <= m_AlphaStops[0].time)
                return m_AlphaStops[0].alpha;
            var last = m_AlphaStops[m_AlphaStops.Count - 1];
            if (t >= last.time)
                return last.alpha;

            for (var i = 0; i < m_AlphaStops.Count - 1; i++)
            {
                var a = m_AlphaStops[i];
                var b = m_AlphaStops[i + 1];
                if (t < a.time || t > b.time)
                    continue;
                if (m_Mode == GradientMode.Fixed)
                    return a.alpha;
                var span = b.time - a.time;
                var f = span > Mathf.Epsilon ? (t - a.time) / span : 0f;
                return Mathf.Lerp(a.alpha, b.alpha, f);
            }

            return m_AlphaStops[0].alpha;
        }

        /// <summary>
        /// Applies the current stops everywhere and reports the change. Every mutating entry point
        /// funnels through here, so no path can update the model and forget the view.
        /// </summary>
        void Commit()
        {
            var previous = m_LastNotified;
            Refresh();
            var next = BuildGradient();
            m_LastNotified = next;

            using (var evt = ChangeEvent<Gradient>.GetPooled(previous, next))
            {
                evt.target = this;
                SendEvent(evt);
            }

            NotifyPropertyChanged(in valueProperty);
            m_OnValueChanging?.Invoke(BuildGradient());
        }

        void Refresh()
        {
            m_Strip.SetValueWithoutNotify(BuildGradient());
            RefreshRail(m_ColorRail, m_ColorMarkers, m_ColorStops.Count, KeyKind.Color);
            RefreshRail(m_AlphaRail, m_AlphaMarkers, m_AlphaStops.Count, KeyKind.Alpha);
            RefreshInspector();
        }

        void RefreshRail(VisualElement rail, List<VisualElement> markers, int count, KeyKind kind)
        {
            while (markers.Count > count)
            {
                var last = markers[markers.Count - 1];
                rail.Remove(last);
                markers.RemoveAt(markers.Count - 1);
            }

            while (markers.Count < count)
            {
                var marker = new VisualElement { pickingMode = PickingMode.Position };
                marker.AddToClassList(markerUssClassName);
                var chip = new VisualElement { pickingMode = PickingMode.Ignore };
                chip.AddToClassList(markerChipUssClassName);
                marker.Add(chip);
                rail.Add(marker);
                markers.Add(marker);
            }

            for (var i = 0; i < count; i++)
            {
                var marker = markers[i];
                var chip = marker[0];
                float time;

                if (kind == KeyKind.Color)
                {
                    var stop = m_ColorStops[i];
                    time = stop.time;
                    marker.userData = stop;
                    // The chip shows the stop's own value. That is data the user chose, not a
                    // design color, which is why it is set inline rather than tokenized.
                    chip.style.backgroundColor = new Color(stop.color.r, stop.color.g, stop.color.b, 1f);
                }
                else
                {
                    var stop = m_AlphaStops[i];
                    time = stop.time;
                    marker.userData = stop;
                    chip.style.backgroundColor = new Color(stop.alpha, stop.alpha, stop.alpha, 1f);
                }

                // Percent so the rail can be resized, minus half a marker so the chip is centred on
                // the time it edits rather than starting at it.
                marker.style.left = Length.Percent(time * 100f);
                marker.style.translate = new Translate(Length.Percent(-50f), 0f);
                marker.EnableInClassList(selectedMarkerUssClassName, ReferenceEquals(marker.userData, m_Selected));
            }
        }

        void RefreshInspector()
        {
            m_Syncing = true;
            try
            {
                m_ModeDropdown.SetValueWithoutNotify(new[] { m_Mode == GradientMode.Fixed ? 1 : 0 });

                var isColor = m_Selected is ColorStop;
                var isAlpha = m_Selected is AlphaStop;

                m_ColorField.style.display = isColor ? DisplayStyle.Flex : DisplayStyle.None;
                m_AlphaSlider.style.display = isAlpha ? DisplayStyle.Flex : DisplayStyle.None;
                m_StopRow.style.display = m_Selected != null ? DisplayStyle.Flex : DisplayStyle.None;
                m_LocationField.SetEnabled(m_Selected != null);

                switch (m_Selected)
                {
                    case ColorStop c:
                        m_StopLabel.text = "Color";
                        m_LocationField.SetValueWithoutNotify(Mathf.RoundToInt(c.time * 100f));
                        m_ColorField.SetValueWithoutNotify(c.color);
                        break;
                    case AlphaStop a:
                        m_StopLabel.text = "Alpha";
                        m_LocationField.SetValueWithoutNotify(Mathf.RoundToInt(a.time * 100f));
                        m_AlphaSlider.colorRange = AlphaTrack(SampleColor(a.time));
                        m_AlphaSlider.SetValueWithoutNotify(a.alpha);
                        break;
                }
            }
            finally
            {
                m_Syncing = false;
            }
        }

        static Gradient AlphaTrack(Color stopColor)
        {
            var opaque = new Color(stopColor.r, stopColor.g, stopColor.b, 1f);
            var track = new Gradient { mode = GradientMode.Blend };
            track.SetKeys(
                new[] { new GradientColorKey(opaque, 0f), new GradientColorKey(opaque, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 1f) });
            return track;
        }

        /// <summary>
        /// Maps a pointer position onto a normalized time.
        /// </summary>
        /// <remarks>
        /// Measured against the rail's content rect, which is also what the markers' percentage
        /// offsets resolve against, so a drag lands on exactly the time its marker will be drawn
        /// at. The two coordinate systems only agree while the rail carries no padding and no
        /// border, which <c>GradientEditor.uss</c> enforces and a test pins.
        /// </remarks>
        static float TimeAt(VisualElement rail, Vector2 worldPosition)
        {
            var rect = rail.contentRect;
            if (rect.width <= Mathf.Epsilon)
                return 0f;
            var local = rail.WorldToLocal(worldPosition);
            return Mathf.Clamp01((local.x - rect.x) / rect.width);
        }

        void OnRailPointerDown(PointerDownEvent evt, KeyKind kind)
        {
            var rail = kind == KeyKind.Color ? m_ColorRail : m_AlphaRail;
            var marker = FindMarker(evt.target as VisualElement, rail);

            if (marker != null)
            {
                m_SelectedKind = kind;
                m_Selected = marker.userData;
                Refresh();
                m_Dragging = true;
                rail.CapturePointer(evt.pointerId);
                evt.StopPropagation();
                return;
            }

            if (evt.clickCount >= 2)
            {
                var time = TimeAt(rail, evt.position);
                if (kind == KeyKind.Color)
                    AddColorKey(time);
                else
                    AddAlphaKey(time);
                evt.StopPropagation();
            }
        }

        void OnRailPointerMove(PointerMoveEvent evt, KeyKind kind)
        {
            if (!m_Dragging)
                return;
            var rail = kind == KeyKind.Color ? m_ColorRail : m_AlphaRail;
            SetSelectedKeyTime(TimeAt(rail, evt.position));
            evt.StopPropagation();
        }

        void OnRailPointerUp(PointerUpEvent evt)
        {
            if (!m_Dragging)
                return;
            m_Dragging = false;
            if (evt.currentTarget is VisualElement rail && rail.HasPointerCapture(evt.pointerId))
                rail.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }

        static VisualElement FindMarker(VisualElement from, VisualElement rail)
        {
            while (from != null && from != rail)
            {
                if (from.ClassListContains(markerUssClassName))
                    return from;
                from = from.parent;
            }
            return null;
        }

        /// <summary>
        /// Double-clicking the strip adds a stop at that time — an alpha stop in the upper half, a
        /// color stop in the lower half, matching which rail is on which side.
        /// </summary>
        void OnStripPointerDown(PointerDownEvent evt)
        {
            if (evt.clickCount < 2)
                return;

            var rect = m_Strip.contentRect;
            if (rect.height <= Mathf.Epsilon)
                return;

            var local = m_Strip.WorldToLocal((Vector2)evt.position);
            var time = Mathf.Clamp01((local.x - rect.x) / rect.width);

            if (local.y - rect.y < rect.height * 0.5f)
                AddAlphaKey(time);
            else
                AddColorKey(time);

            evt.StopPropagation();
        }

        /// <summary>
        /// Swallows the ChangeEvent&lt;Gradient&gt; a descendant sends, so it cannot masquerade as
        /// the editor's own value change.
        /// </summary>
        /// <remarks>
        /// Refreshing the inspector writes to the <see cref="ColorField"/> and to the alpha
        /// slider's track, and both are built on <see cref="ColorSwatch"/>, whose setter sends a
        /// bubbling ChangeEvent&lt;Gradient&gt;. Without this guard a single edit reaches a
        /// consumer's RegisterValueChangedCallback three times, and merely opening the editor
        /// reaches it once — with the swatch's gradient, not the edited one.
        /// </remarks>
        void OnAnyGradientChange(ChangeEvent<Gradient> evt)
        {
            if (evt.target == this)
                return;
            // Immediate: StopPropagation alone still runs the consumer's callbacks on this element.
            evt.StopImmediatePropagation();
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode is not (KeyCode.Delete or KeyCode.Backspace))
                return;
            if (RemoveSelectedKey())
                evt.StopPropagation();
        }

        void OnModeChanged(ChangeEvent<IEnumerable<int>> evt)
        {
            if (m_Syncing)
                return;
            var index = m_ModeDropdown.selectedIndex;
            mode = index == 1 ? GradientMode.Fixed : GradientMode.Blend;
        }

        void OnLocationChanged(ChangeEvent<int> evt)
        {
            if (m_Syncing)
                return;
            SetSelectedKeyTime(evt.newValue / 100f);
        }

        void OnLocationChanging(ChangingEvent<int> evt)
        {
            if (m_Syncing)
                return;
            SetSelectedKeyTime(evt.newValue / 100f);
        }

        void OnStopColorChanged(ChangeEvent<Color> evt)
        {
            if (m_Syncing)
                return;
            SetSelectedKeyColor(evt.newValue);
        }

        void OnStopAlphaChanged(ChangeEvent<float> evt)
        {
            if (m_Syncing)
                return;
            SetSelectedKeyAlpha(evt.newValue);
        }

        void OnStopAlphaChanging(ChangingEvent<float> evt)
        {
            if (m_Syncing)
                return;
            SetSelectedKeyAlpha(evt.newValue);
        }
    }
}
