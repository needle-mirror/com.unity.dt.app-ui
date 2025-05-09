using System;
using Unity.AppUI.Core;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The tooltip popup type.
    /// </summary>
    public sealed class Tooltip : AnchorPopup<Tooltip>
    {
        /// <summary>
        /// The default placement of the tooltip.
        /// </summary>
        public const PopoverPlacement defaultPlacement = PopoverPlacement.Bottom;

        const int k_TooltipFadeInDurationMs = 250;

        readonly ValueAnimation<float> m_Animation;

        IVisualElementScheduledItem m_ScheduledAnimateViewIn;

        IVisualElementScheduledItem m_DeferredAnimateViewIn;

        IVisualElementScheduledItem m_ScheduledAnimationStart;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parentView">The popup container.</param>
        /// <param name="context">The application context attached to this popup.</param>
        /// <param name="contentView">The content to display inside the popup.</param>
        Tooltip(VisualElement parentView, ApplicationContext context, VisualElement contentView)
            : base(parentView, context, contentView)
        {
            m_Animation = contentView.experimental.animation.Start(0, 1, k_TooltipFadeInDurationMs, (element, f) => element.style.opacity = f).OnCompleted(InvokeShownEventHandlers).KeepAlive();
            m_Animation.Stop();

            contentView.style.position = Position.Absolute; // force to absolute.
            keyboardDismissEnabled = false;
        }

        TooltipVisualElement tooltip => (TooltipVisualElement)view;

        /// <summary>
        /// The text to display inside the popup.
        /// </summary>
        public string text => tooltip.text;

        /// <summary>
        /// Set a new value for the <see cref="text"/> property.
        /// </summary>
        /// <param name="value"> The new value (will be localized). </param>
        /// <returns>The Tooltip.</returns>
        public Tooltip SetText(string value)
        {
            if (!string.IsNullOrEmpty(value))
                tooltip.contentContainer.Clear();
            tooltip.text = value;
            return this;
        }
        
        /// <summary>
        /// The template to display inside the popup.
        /// </summary>
        public VisualElement template => tooltip.contentContainer.childCount > 0 ? tooltip.contentContainer[0] : null;

        /// <summary>
        /// Set the content of the tooltip.
        /// </summary>
        /// <remarks>
        /// Passing null will clear the content of the tooltip.
        /// </remarks>
        /// <param name="content"> The content to display inside the tooltip. </param>
        /// <returns> The Tooltip. </returns>
        public Tooltip SetContent(VisualElement content)
        {
            tooltip.contentContainer.Clear();
            tooltip.text = null;
            if (content != null)
                tooltip.contentContainer.Add(content);
            return this;
        }

        /// <inheritdoc cref="Popup.ShouldAnimate"/>
        protected override bool ShouldAnimate()
        {
            return true;
        }

        /// <inheritdoc cref="AnchorPopup{T}.AnimateViewIn"/>
        protected override void AnimateViewIn()
        {
            // delay the animation of the notification to be sure the layout has been updated with UI Toolkit.
            m_ScheduledAnimateViewIn?.Pause();
            m_ScheduledAnimateViewIn = view.schedule.Execute(ScheduledAnimateViewIn);
        }

        void ScheduledAnimateViewIn()
        {
            m_DeferredAnimateViewIn?.Pause();
            m_DeferredAnimateViewIn = view.schedule.Execute(DeferredAnimateViewIn);
        }

        void DeferredAnimateViewIn()
        {
            if (view.parent != null)
            {
                m_ScheduledAnimationStart?.Pause();
                tooltip.visible = true;
                tooltip.style.opacity = 0.0001f;
                RefreshPosition();
                m_ScheduledAnimationStart = view.schedule.Execute(m_Animation.Start);
            }
        }

        /// <inheritdoc cref="AnchorPopup{T}.AnimateViewOut"/>
        protected override void AnimateViewOut(DismissType reason)
        {
            m_Animation.Stop();
            m_ScheduledAnimationStart?.Pause();
            m_DeferredAnimateViewIn?.Pause();
            m_ScheduledAnimateViewIn?.Pause();
            tooltip.visible = false; // no out animation
            tooltip.style.opacity = 0;
            InvokeDismissedEventHandlers(reason);
        }

        /// <inheritdoc cref="Popup.FindSuitableParent"/>
        protected override VisualElement FindSuitableParent(VisualElement element)
        {
            return Panel.FindTooltipLayer(element);
        }

        /// <summary>
        /// Build a new Tooltip.
        /// <remarks>
        /// In the Application element, only one Tooltip is create and moved at the right place when hovering others UI
        /// elements. The Tooltip is handled by the <see cref="TooltipManipulator"/>.
        /// </remarks>
        /// </summary>
        /// <param name="referenceView">An arbitrary UI element used as reference for the application
        /// context to attach to the popup.</param>
        /// <returns>A Tooltip instance.</returns>
        public static Tooltip Build(VisualElement referenceView)
        {
            var context = referenceView.GetContext();
            var parentView = context.panel.tooltipContainer;
            var tooltipElement = new Tooltip(parentView, context, new TooltipVisualElement())
                .SetPlacement(defaultPlacement);

            return tooltipElement;
        }

        /// <summary>
        /// The Tooltip UI Element.
        /// </summary>
        sealed class TooltipVisualElement : VisualElement, IPlaceableElement
        {
            public static readonly string ussClassName = "appui-tooltip";

            public static readonly string containerUssClassName = ussClassName + "__container";

            public static readonly string contentUssClassName = ussClassName + "__content";

            public static readonly string tipUssClassName = ussClassName + "__tip";

            public static readonly string upDirectionUssClassName = ussClassName + "--up";

            public static readonly string downDirectionUssClassName = ussClassName + "--down";

            public static readonly string leftDirectionUssClassName = ussClassName + "--left";

            public static readonly string rightDirectionUssClassName = ussClassName + "--right";

            readonly ExVisualElement m_Container;

            PopoverPlacement m_Placement;

            readonly LocalizedTextElement m_Content;

            /// <summary>
            /// Default constructor.
            /// </summary>
            public TooltipVisualElement()
            {
                AddToClassList(ussClassName);

                m_Container = new ExVisualElement
                {
                    name = containerUssClassName,
                    usageHints = UsageHints.DynamicTransform,
                    pickingMode = PickingMode.Ignore,
                    passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.OutsetShadows
                };
                m_Container.AddToClassList(containerUssClassName);
                hierarchy.Add(m_Container);

                tipElement = new VisualElement { name = tipUssClassName, pickingMode = PickingMode.Ignore };
                tipElement.AddToClassList(tipUssClassName);
                hierarchy.Add(tipElement);

                m_Content = new LocalizedTextElement { name = contentUssClassName, pickingMode = PickingMode.Ignore };
                m_Content.AddToClassList(contentUssClassName);
                m_Container.hierarchy.Add(m_Content);

                placement = defaultPlacement;
            }

            public override VisualElement contentContainer => m_Content;


            public VisualElement tipElement { get; }

            /// <summary>
            /// The text to display inside the Tooltip.
            /// </summary>
            public string text
            {
                get => m_Content.text;
                set => m_Content.text = value;
            }

            /// <summary>
            /// The popup placement, used to display the arrow at the right place.
            /// </summary>
            public PopoverPlacement placement
            {
                get => m_Placement;

                set
                {
                    m_Placement = value;
                    RefreshPlacement();
                }
            }

            void RefreshPlacement()
            {
                bool up = false, down = false, left = false, right = false;

                switch (m_Placement)
                {
                    case PopoverPlacement.Bottom:
                    case PopoverPlacement.BottomLeft:
                    case PopoverPlacement.BottomRight:
                    case PopoverPlacement.BottomStart:
                    case PopoverPlacement.BottomEnd:
                        up = true;
                        break;
                    case PopoverPlacement.Top:
                    case PopoverPlacement.TopLeft:
                    case PopoverPlacement.TopRight:
                    case PopoverPlacement.TopStart:
                    case PopoverPlacement.TopEnd:
                        down = true;
                        break;
                    case PopoverPlacement.Left:
                    case PopoverPlacement.LeftTop:
                    case PopoverPlacement.LeftBottom:
                    case PopoverPlacement.Start:
                    case PopoverPlacement.StartTop:
                    case PopoverPlacement.StartBottom:
                        right = true;
                        break;
                    case PopoverPlacement.Right:
                    case PopoverPlacement.RightTop:
                    case PopoverPlacement.RightBottom:
                    case PopoverPlacement.End:
                    case PopoverPlacement.EndTop:
                    case PopoverPlacement.EndBottom:
                        left = true;
                        break;
                    default:
                        throw new ValueOutOfRangeException(nameof(m_Placement), m_Placement);
                }

                EnableInClassList(upDirectionUssClassName, up);
                EnableInClassList(downDirectionUssClassName, down);
                EnableInClassList(leftDirectionUssClassName, left);
                EnableInClassList(rightDirectionUssClassName, right);
            }
        }
    }
}
