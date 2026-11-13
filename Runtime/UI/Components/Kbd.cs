using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A keyboard shortcut hint, rendered as one or more key caps.
    /// </summary>
    /// <remarks>
    /// Kbd renders a shortcut the way a keyboard does: each token sits in its own cap, a raised surface
    /// with a strong border and a thicker bottom edge that gives it the depth of a physical key.
    ///
    /// There are two forms, mirroring the web implementation. Set <see cref="keys"/> for a combo and
    /// every token gets its own cap. Set <see cref="text"/> for a single token and you get one cap.
    /// <see cref="keys"/> wins when both are set; clearing it falls back to <see cref="text"/>.
    ///
    /// Kbd is a hint, not a control. It ignores pointer events and is not focusable.
    /// </remarks>
    /// <example>
    /// <para>A single key.</para>
    /// <code lang="csharp">
    /// var kbd = new Kbd { text = "Esc" };
    /// parentElement.Add(kbd);
    /// </code>
    /// <para>A combo, one cap per token.</para>
    /// <code lang="csharp">
    /// var shortcut = new Kbd { keys = new List&lt;string&gt; { "Ctrl", "Shift", "K" } };
    /// parentElement.Add(shortcut);
    /// </code>
    /// <para>Both forms in UXML. The <c>keys</c> attribute takes a comma separated list.</para>
    /// <code lang="xml">
    /// &lt;appui:Kbd text="Esc" /&gt;
    /// &lt;appui:Kbd keys="Ctrl,K" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("typography")]
    public partial class Kbd : BaseVisualElement
    {
        internal static readonly BindingId textProperty = new BindingId(nameof(text));

        internal static readonly BindingId keysProperty = new BindingId(nameof(keys));

        /// <summary>
        /// The Kbd main styling class.
        /// </summary>
        public const string ussClassName = "appui-kbd";

        /// <summary>
        /// The styling class of a single key cap.
        /// </summary>
        public const string capUssClassName = ussClassName + "__cap";

        string m_Text = string.Empty;

        List<string> m_Keys;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Kbd()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
            focusable = false;

            RefreshCaps();
        }

        /// <summary>
        /// The single key token to display.
        /// </summary>
        /// <remarks>
        /// Used only while <see cref="keys"/> is null or empty. Defaults to an empty string, which still
        /// renders one empty cap so the element always has the shape of a key.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string text
        {
            get => m_Text;
            set
            {
                var newValue = value ?? string.Empty;
                var changed = m_Text != newValue;
                m_Text = newValue;
                RefreshCaps();

                if (changed)
                    NotifyPropertyChanged(in textProperty);
            }
        }

        /// <summary>
        /// The tokens of a key combo, one cap each.
        /// </summary>
        /// <remarks>
        /// Takes precedence over <see cref="text"/>. Set it to null or an empty list to fall back to the
        /// single cap form.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public List<string> keys
        {
            get => m_Keys;
            set
            {
                var changed = !SameKeys(m_Keys, value);
                m_Keys = value;
                RefreshCaps();

                if (changed)
                    NotifyPropertyChanged(in keysProperty);
            }
        }

        static bool SameKeys(List<string> a, List<string> b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            return a.SequenceEqual(b);
        }

        void RefreshCaps()
        {
            hierarchy.Clear();

            if (m_Keys != null && m_Keys.Count > 0)
            {
                foreach (var key in m_Keys)
                    hierarchy.Add(CreateCap(key));
            }
            else
            {
                hierarchy.Add(CreateCap(m_Text));
            }
        }

        static LocalizedTextElement CreateCap(string value)
        {
            var cap = new LocalizedTextElement(value)
            {
                name = capUssClassName,
                pickingMode = PickingMode.Ignore
            };
            cap.AddToClassList(capUssClassName);
            return cap;
        }
    }
}
