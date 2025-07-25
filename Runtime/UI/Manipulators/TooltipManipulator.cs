using System;
using System.Runtime.CompilerServices;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using PointerType = UnityEngine.UIElements.PointerType;

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

        VisualElement rootElement => target.GetExclusiveRootElement();

        int tooltipDelayMs => m_AnchorElement?.GetContext<TooltipDelayContext>()?.tooltipDelayMs ?? defaultDelayMs;

        protected override void RegisterCallbacksOnTarget()
        {
            m_Tooltip = Tooltip.Build(target);
            m_ScheduledItem = target.schedule.Execute(StartFadeIn);
            m_ScheduledItem.Pause();

            rootElement?.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target?.RegisterCallback<PointerMoveEvent>(OnPointerMoved);
            rootElement?.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            rootElement?.RegisterCallback<PointerCancelEvent>(OnPointerCanceled, TrickleDown.TrickleDown);
            rootElement?.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            rootElement?.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target?.UnregisterCallback<PointerMoveEvent>(OnPointerMoved);
            rootElement?.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            rootElement?.UnregisterCallback<PointerCancelEvent>(OnPointerCanceled, TrickleDown.TrickleDown);
            rootElement?.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            HideTooltip();
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            HideTooltip();
            if (evt.pointerType == PointerType.touch)
            {
                var pickedElement = target.panel?.Pick(evt.position);
                if (pickedElement == null)
                    return;
                var tooltipElement = GetTooltipElement(pickedElement);
                if (m_AnchorElement == tooltipElement)
                    return;
                m_AnchorElement = tooltipElement;
                var hasTooltip = false;
                m_Tooltip.SetContent(null);
                m_Tooltip.SetText(null);
                if (m_AnchorElement?.GetTooltipTemplate() is { } template)
                {
                    m_Tooltip.SetContent(template);
                    m_AnchorElement.RegisterTooltipTemplateChangedCallback(OnTooltipTemplateOrContentChanged);
                    m_AnchorElement.RegisterTooltipContentChangedCallback(OnTooltipTemplateOrContentChanged);
                    OnTooltipTemplateOrContentChanged();
                    hasTooltip = true;
                }
                else if (CanDisplayTooltipInCurrentContext(m_AnchorElement) && GetTooltipText(m_AnchorElement) is {} text)
                {
                    m_Tooltip.SetText(text);
                    hasTooltip = true;
                }
                if (hasTooltip)
                    ShowTooltip(tooltipDelayMs);
            }
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerType == PointerType.touch && m_ScheduledItem is {isActive: true})
                HideTooltip();
        }

        void OnPointerCanceled(PointerCancelEvent evt)
        {
            if (evt.pointerType == PointerType.touch && m_ScheduledItem is {isActive: true})
                HideTooltip();
        }

        void OnPointerMoved(PointerMoveEvent evt)
        {
            if (evt.pointerType == PointerType.touch)
                return;

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
                m_AnchorElement.RegisterTooltipTemplateChangedCallback(OnTooltipTemplateOrContentChanged);
                m_AnchorElement.RegisterTooltipContentChangedCallback(OnTooltipTemplateOrContentChanged);
                OnTooltipTemplateOrContentChanged();
                hasTooltip = true;
            }
            else if (CanDisplayTooltipInCurrentContext(m_AnchorElement) && GetTooltipText(m_AnchorElement) is {} text)
            {
                m_Tooltip.SetText(text);
                hasTooltip = true;
            }

            // 4 - If the tne new tooltip is not null, start delay
            if (hasTooltip)
                ShowTooltip(tooltipDelayMs);
        }

        void OnTooltipTemplateOrContentChanged()
        {
            if (m_AnchorElement?.GetTooltipTemplate() is { } template)
            {
                m_Tooltip.SetContent(template);
                var callback = m_AnchorElement.GetTooltipContent();
                callback?.Invoke(template);
            }
        }

        static string GetTooltipText(VisualElement element)
        {
            if (element is TextElement {displayTooltipWhenElided: true, isElided: true} textElement && string.IsNullOrEmpty(textElement.tooltip))
                return string.IsNullOrEmpty(textElement.text) ? null : textElement.text;

            return string.IsNullOrEmpty(element?.tooltip) ? null : element.tooltip;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool CanDisplayTooltipInCurrentContext(VisualElement refElement)
        {
            if (target?.panel == null)
                return false;
            return force || target.panel.contextType == ContextType.Player;
        }

        void ShowTooltip(int delayMs)
        {
            m_ScheduledItem?.ExecuteLater(delayMs);
        }

        void StartFadeIn()
        {
            if (m_AnchorElement == null || m_AnchorElement.IsInvisible())
            {
                HideTooltip();
                return;
            }
            m_Tooltip?.SetAnchor(m_AnchorElement);
            m_Tooltip?.SetPlacement(m_AnchorElement.GetPreferredTooltipPlacement());
            m_Tooltip?.Show();
        }

        void HideTooltip()
        {
            m_ScheduledItem?.Pause();
            m_AnchorElement?.UnregisterTooltipContentChangedCallback(OnTooltipTemplateOrContentChanged);
            m_AnchorElement?.UnregisterTooltipTemplateChangedCallback(OnTooltipTemplateOrContentChanged);
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
