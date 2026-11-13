using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The rubber-band rectangle drawn while drag-selecting on a <see cref="Canvas"/>.
    /// </summary>
    /// <remarks>
    /// Purely presentational, and deliberately so: the caller owns the drag and the hit testing, and
    /// feeds the result in through <see cref="rect"/>. Keeping the two apart is what lets the same
    /// marquee serve a node graph, a timeline or a tile map without knowing what it is selecting.
    ///
    /// A drag that runs right-to-left or bottom-to-top produces a negative extent, which no layout
    /// system draws. <see cref="Normalize"/> turns two drag corners into the positive rect this
    /// element expects, so every caller does not have to rediscover that.
    ///
    /// The element ignores the pointer, so a marquee never eats the release that ends the drag it is
    /// drawing.
    /// </remarks>
    /// <example>
    /// <para>Driving the marquee from a drag.</para>
    /// <code lang="csharp">
    /// var marquee = new SelectionRect { visible = false };
    /// canvas.Add(marquee);
    ///
    /// canvas.RegisterCallback&lt;PointerMoveEvent&gt;(evt =>
    /// {
    ///     marquee.visible = true;
    ///     marquee.rect = SelectionRect.Normalize(dragOrigin, canvas.WorldToLocal(evt.position));
    /// });
    /// </code>
    /// <para>In UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:SelectionRect /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class SelectionRect : BaseVisualElement
    {
        internal static readonly BindingId rectProperty = nameof(rect);

        /// <summary>
        /// The SelectionRect main styling class.
        /// </summary>
        public const string ussClassName = "appui-selection-rect";

        Rect m_Rect;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SelectionRect()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;
            rect = Rect.zero;
        }

        /// <summary>
        /// The marquee's rectangle, in the coordinate space of whatever the marquee is a child of —
        /// the same space the elements it selects are positioned in.
        /// </summary>
        /// <remarks>
        /// A negative width or height is clamped to zero rather than silently drawing nothing: pass a
        /// rect through <see cref="Normalize"/> first.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public Rect rect
        {
            get => m_Rect;
            set
            {
                var changed = m_Rect != value;
                m_Rect = value;
                style.left = value.x;
                style.top = value.y;
                style.width = Mathf.Max(0f, value.width);
                style.height = Mathf.Max(0f, value.height);

                if (changed)
                    NotifyPropertyChanged(in rectProperty);
            }
        }

        /// <summary>
        /// Turns the two corners of a drag into a rectangle with a positive extent.
        /// </summary>
        /// <param name="origin"> Where the drag started. </param>
        /// <param name="current"> Where the pointer is now. </param>
        /// <returns> The rectangle spanned by the two points. </returns>
        public static Rect Normalize(Vector2 origin, Vector2 current)
        {
            var min = Vector2.Min(origin, current);
            var max = Vector2.Max(origin, current);
            return new Rect(min, max - min);
        }
    }
}
