using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A small popup that displays contextual information when hovering over a target element.
    /// </summary>
    /// <remarks>
    /// A tooltip is a small popup that appears when hovering over or focusing on a UI element, providing
    /// additional context, descriptions, or helpful information without cluttering the interface.
    ///
    /// Tooltips are perfect for explaining the purpose of buttons, providing definitions for technical terms,
    /// or offering additional details about interface elements. They appear on demand and automatically
    /// dismiss when the user moves away.
    ///
    /// The tooltip component automatically positions itself relative to the target element and includes a
    /// small arrow pointing to the source. It supports both text content and custom visual elements for more
    /// complex information displays.
    ///
    /// Use tooltips for supplementary information that enhances understanding but isn't essential for basic
    /// functionality. Avoid putting critical information in tooltips as they may not be discoverable on touch
    /// interfaces.
    ///
    /// ## Anatomy
    /// Tooltip Examples: a button with a tooltip on hover.
    /// ```xml
    /// &lt;appui:Button title="Hover me" size="M" /&gt;
    /// &lt;appui:Tooltip text="This is a simple tooltip" anchor="Top" /&gt;
    /// ```
    ///
    /// Different Positions:
    /// ```xml
    /// &lt;appui:IconButton icon="info" size="M" /&gt;
    /// &lt;appui:Tooltip text="Tooltip above" anchor="Top" /&gt;
    ///
    /// &lt;appui:IconButton icon="help" size="M" /&gt;
    /// &lt;appui:Tooltip text="Tooltip below" anchor="Bottom" /&gt;
    ///
    /// &lt;appui:IconButton icon="settings" size="M" /&gt;
    /// &lt;appui:Tooltip text="Tooltip on left" anchor="Left" /&gt;
    ///
    /// &lt;appui:IconButton icon="edit" size="M" /&gt;
    /// &lt;appui:Tooltip text="Tooltip on right" anchor="Right" /&gt;
    /// ```
    ///
    /// Rich Content: a tooltip with rich content.
    /// ```xml
    /// &lt;appui:TextField placeholder-text="Username" size="M" /&gt;
    /// &lt;appui:Tooltip&gt;
    ///     &lt;appui:Text text="Username Requirements:" size="S" /&gt;
    ///     &lt;appui:Text text="&#8226; 3-20 characters" size="XS" /&gt;
    ///     &lt;appui:Text text="&#8226; Letters and numbers only" size="XS" /&gt;
    /// &lt;/appui:Tooltip&gt;
    /// ```
    /// </remarks>
    /// <example>
    /// <para>Basic text tooltip. Adding simple text tooltips to UI elements.</para>
    /// <code lang="csharp"><![CDATA[
    /// var saveButton = new Button { title = "Save", leadingIcon = "save" };
    ///
    /// var tooltip = Tooltip.Build(saveButton)
    ///     .SetText("Save your changes to the current document")
    ///     .SetPlacement(PopoverPlacement.Top);
    ///
    /// // Tooltip will automatically show on hover
    /// // and hide when the user moves away
    ///
    /// toolbar.Add(saveButton);
    ///
    /// // For icon-only buttons, tooltips are especially helpful
    /// var settingsButton = new IconButton { icon = "settings" };
    /// var settingsTooltip = Tooltip.Build(settingsButton)
    ///     .SetText("Open application settings");
    ///
    /// toolbar.Add(settingsButton);
    /// ]]></code>
    /// <para>Custom content tooltip. Creating tooltips with rich visual content.</para>
    /// <code lang="csharp"><![CDATA[
    /// var profileButton = new Button { title = "User Profile" };
    ///
    /// // Create custom tooltip content
    /// var tooltipContent = new VisualElement();
    /// tooltipContent.AddToClassList("profile-tooltip");
    ///
    /// var avatar = new Avatar { src = "user-avatar.png", size = Size.S };
    /// var userName = new Text("John Doe") { size = TextSize.S };
    /// var userRole = new Text("Administrator") { size = TextSize.XS };
    /// userRole.AddToClassList("text-muted");
    ///
    /// var info = new VisualElement();
    /// info.Add(userName);
    /// info.Add(userRole);
    ///
    /// var container = new VisualElement { style = { flexDirection = FlexDirection.Row } };
    /// container.Add(avatar);
    /// container.Add(info);
    /// tooltipContent.Add(container);
    ///
    /// var profileTooltip = Tooltip.Build(profileButton)
    ///     .SetContent(tooltipContent)
    ///     .SetPlacement(PopoverPlacement.BottomStart);
    ///
    /// header.Add(profileButton);
    /// ]]></code>
    /// <para>Tooltip positioning and configuration. Different placement options and tooltip behavior.</para>
    /// <code lang="csharp"><![CDATA[
    /// // Top placement for bottom toolbar buttons
    /// var bottomButton = new IconButton { icon = "add" };
    /// var topTooltip = Tooltip.Build(bottomButton)
    ///     .SetText("Add new item")
    ///     .SetPlacement(PopoverPlacement.Top);
    ///
    /// // Left placement for right-side buttons
    /// var rightButton = new IconButton { icon = "help" };
    /// var leftTooltip = Tooltip.Build(rightButton)
    ///     .SetText("Get help and documentation")
    ///     .SetPlacement(PopoverPlacement.Left);
    ///
    /// // Tooltip with keyboard dismissal enabled
    /// var complexButton = new Button { title = "Advanced Options" };
    /// var complexTooltip = Tooltip.Build(complexButton)
    ///     .SetText("Access advanced configuration options\nPress ESC to close this tooltip")
    ///     .SetPlacement(PopoverPlacement.Right)
    ///     .SetKeyboardDismissEnabled(true);
    ///
    /// // Programmatically show/hide tooltips
    /// complexButton.clicked += () => {
    ///     if (complexTooltip.isShown)
    ///         complexTooltip.Dismiss();
    ///     else
    ///         complexTooltip.Show();
    /// };
    /// ]]></code>
    /// </example>
    [VisualDocPage("popups")]
    public sealed class Tooltip : AnchorPopup<Tooltip>
    {
        /// <summary>
        /// The default placement of the tooltip.
        /// </summary>
        public const PopoverPlacement defaultPlacement = PopoverPlacement.Bottom;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="referenceView"> The element used as context provider for the tooltip. </param>
        /// <param name="contentView">The content to display inside the popup.</param>
        Tooltip(VisualElement referenceView, VisualElement contentView)
            : base(referenceView, contentView)
        {
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
        /// The content Visual Element of the tooltip (if any).
        /// </summary>
        public VisualElement content => tooltip.contentContainer.childCount > 0 ? tooltip.contentContainer[0] : null;

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
            if (content?.parent == tooltip.contentContainer)
                return this;

            tooltip.contentContainer.Clear();
            tooltip.text = null;
            if (content != null)
                tooltip.contentContainer.Add(content);
            return this;
        }

        /// <inheritdoc />
        protected override bool ShouldAnimate() => base.ShouldAnimate();

        /// <inheritdoc />
        protected override void AnimateViewOut(DismissType reason)
        {
            InvokeDismissedEventHandlers(reason);
        }

        /// <inheritdoc cref="Popup.FindSuitableParent"/>
        protected override VisualElement FindSuitableParent(VisualElement element)
        {
            return Panel.FindTooltipLayer(element);
        }

        /// <summary>
        /// Build a new Tooltip.
        /// </summary>
        /// <param name="referenceView">An arbitrary UI element used as reference for the application
        /// context to attach to the popup.</param>
        /// <returns>A Tooltip instance.</returns>
        /// <remarks>
        /// In the Application element, only one Tooltip is create and moved at the right place when hovering others UI
        /// elements. The Tooltip is handled by the <see cref="TooltipManipulator"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">If <paramref name="referenceView"/> is null.</exception>
        public static Tooltip Build(VisualElement referenceView)
        {
            if (referenceView == null)
                throw new ArgumentNullException(nameof(referenceView));

            var tooltipElement = new Tooltip(referenceView, new TooltipVisualElement())
                .SetPlacement(defaultPlacement);

            return tooltipElement;
        }

        /// <summary>
        /// The Tooltip UI Element.
        /// </summary>
        sealed class TooltipVisualElement : VisualElement, IPlaceableElement
        {
            public const string ussClassName = "appui-tooltip";

            public const string containerUssClassName = ussClassName + "__container";

            public const string contentUssClassName = ussClassName + "__content";

            public const string tipUssClassName = ussClassName + "__tip";

            public const string upDirectionUssClassName = ussClassName + "--up";

            public const string downDirectionUssClassName = ussClassName + "--down";

            public const string leftDirectionUssClassName = ussClassName + "--left";

            public const string rightDirectionUssClassName = ussClassName + "--right";

            readonly ExVisualElement m_Container;

            PopoverPlacement m_Placement;

            readonly LocalizedTextElement m_Content;

            /// <summary>
            /// Default constructor.
            /// </summary>
            public TooltipVisualElement()
            {
                AddToClassList(ussClassName);
                pickingMode = PickingMode.Ignore;

                m_Container = new ExVisualElement
                {
                    name = containerUssClassName,
                    pickingMode = PickingMode.Ignore,
                    passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.OutsetShadows
                };
                m_Container.EnableDynamicTransform(true);
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
