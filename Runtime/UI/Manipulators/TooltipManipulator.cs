using System;
using System.Runtime.CompilerServices;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Manipulator used to display a Tooltip UI component anchored to the currently hovered UI element.
    /// </summary>
    class TooltipManipulator : Manipulator
    {
        internal const int defaultDelayMs = 750;

        VisualElement m_AnchorElement;

        IVisualElementScheduledItem m_ScheduledItem;

        Tooltip m_Tooltip;
        
        /// <summary>
        /// If true, this manipulator will force the picked tooltip to be displayed, no matter the context.
        /// </summary>
        /// <remarks>
        /// This is useful in an environment where default UI-Toolkit tooltips are disabled,
        /// but you still want to display tooltips.
        /// </remarks>
        public bool force { get; set; }
        
        protected override void RegisterCallbacksOnTarget()
        {
            m_Tooltip = Tooltip.Build(target);
            m_ScheduledItem = target.schedule.Execute(StartFadeIn);
            m_ScheduledItem.Pause();
            target.RegisterCallback<PointerMoveEvent>(OnPointerMoved);
            target.panel.visualTree.RegisterCallback<PointerDownEvent>(OnClick, TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMoved);
            target.panel?.visualTree.UnregisterCallback<PointerDownEvent>(OnClick, TrickleDown.TrickleDown);
        }
        
        void OnClick(PointerDownEvent evt)
        {
            HideTooltip();
        }
        
        void OnPointerMoved(PointerMoveEvent evt)
        {
            // 0 - If the pointer has been captured, nothing to do
            if (target.panel?.GetCapturingElement(evt.pointerId) != null)
            {
                HideTooltip();
                return;
            }
            
            // 1 - pick tooltip below cursor
            var pickedElement = target.panel?.Pick(evt.position);
            if (pickedElement == null)
            {
                HideTooltip();
                return;
            }

            // 2 - If the picked tooltip is same as currently displayed, nothing to do
            var tooltipElement = GetTooltipElement(pickedElement);
            if (m_AnchorElement == tooltipElement)
                return;
            m_AnchorElement = tooltipElement;

            // 3 - New tooltip to display, hide the visual tooltip first
            HideTooltip();
            
            var hasTooltip = false;
            m_Tooltip.SetContent(null);
            m_Tooltip.SetText(null);
            if (m_AnchorElement?.GetTooltipTemplate() is { } template)
            {
                m_Tooltip.SetContent(template);
                hasTooltip = true;
            }
            else if (CanDisplayTooltipInCurrentContext(m_AnchorElement) && !string.IsNullOrEmpty(m_AnchorElement?.tooltip))
            {
                m_Tooltip.SetText(m_AnchorElement.tooltip);
                hasTooltip = true;
            }

            // 4 - If the tne new tooltip is not null, start delay
            if (hasTooltip)
                ShowTooltip();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool CanDisplayTooltipInCurrentContext(VisualElement refElement)
        {
            return force || target.panel.contextType == ContextType.Player;
        }

        void ShowTooltip()
        {
            var delay = m_AnchorElement?.GetContext<TooltipDelayContext>()?.tooltipDelayMs ?? defaultDelayMs;
            m_ScheduledItem?.ExecuteLater(delay);
        }

        void StartFadeIn()
        {
            m_Tooltip?.SetAnchor(m_AnchorElement);
            m_Tooltip?.SetPlacement(m_AnchorElement.GetPreferredTooltipPlacement());
            m_Tooltip?.Show();
        }

        void HideTooltip()
        {
            m_ScheduledItem?.Pause();
            m_Tooltip?.Dismiss();
        }

        static VisualElement GetTooltipElement(VisualElement element)
        {
            var ret = element;
            var parent = element.parent;

            while (HasInvalidTooltip(ret) && parent != null)
            {
                ret = parent;
                parent = parent.parent;
            }

            return HasInvalidTooltip(ret) ? null : ret;

            bool HasInvalidTooltip(VisualElement ve)
            {
                return (ve.GetTooltipTemplate() == null && string.IsNullOrEmpty(ve.tooltip)) || ve.ClassListContains(Styles.openUssClassName);
            }
        }
    }
}
