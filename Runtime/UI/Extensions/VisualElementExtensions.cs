using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Extensions for <see cref="VisualElement"/> class.
    /// </summary>
    public static class VisualElementExtensions
    {
        static readonly WeakReferenceTable<VisualElement, AdditionalData> k_AdditionalDataCache =
            new WeakReferenceTable<VisualElement, AdditionalData>();

        /// <summary>
        /// Get the current application context associated with the current <see cref="VisualElement"/> object.
        /// </summary>
        /// <param name="ve">The <see cref="VisualElement"/> object.</param>
        /// <returns>The application context for this element.</returns>
        /// <exception cref="ArgumentNullException">The provided <see cref="VisualElement"/> object must be not null.</exception>
        public static ApplicationContext GetContext(this VisualElement ve)
        {
            if (ve == null)
                throw new ArgumentNullException(nameof(ve));

            var p = ve;
            // ReSharper disable once UseNegatedPatternInIsExpression
            while (p != null && !(p is ContextProvider)) p = p.parent;

            if (p is ContextProvider contextProvider)
                return contextProvider.context;

            return default;
        }

        /// <summary>
        /// Get child elements of a given type.
        /// </summary>
        /// <param name="element">The parent element.</param>
        /// <param name="recursive">If true, the search will be recursive.</param>
        /// <typeparam name="T">The type of the child elements to search for.</typeparam>
        /// <returns> A list of child elements of the given type.</returns>
        /// <exception cref="ArgumentNullException"> The <see cref="VisualElement"/> object can't be null.</exception>
        public static IEnumerable<T> GetChildren<T>(this VisualElement element, bool recursive)
            where T : VisualElement
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var res = new List<T>();

            foreach (var child in element.Children())
            {
                if (child is T c)
                {
                    res.Add(c);
                    if (recursive)
                        res.AddRange(c.GetChildren<T>(true));
                }
            }

            return res;
        }

        /// <summary>
        /// Get the preferred placement for a <see cref="VisualElement"/>'s <see cref="Tooltip"/>.
        /// </summary>
        /// <param name="element">The <see cref="VisualElement"/> which contains a tooltip.</param>
        /// <returns>The preferred placement, previously set using <see cref="SetPreferredTooltipPlacement"/>
        /// or the closest value set on a parent <see cref="ContextProvider"/> element.</returns>
        /// <exception cref="ArgumentNullException">The <see cref="VisualElement"/> object can't be null.</exception>
        public static PopoverPlacement GetPreferredTooltipPlacement(this VisualElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (element is ContextProvider provider && provider.preferredTooltipPlacement.HasValue)
                return provider.preferredTooltipPlacement.Value;

            if (k_AdditionalDataCache.TryGetValue(element, out var data))
                return data.preferredTooltipPlacement;

            var context = element.GetContext();
            return context.preferredTooltipPlacement.GetValueOrDefault(Tooltip.defaultPlacement);
        }

        /// <summary>
        /// Set a preferred <see cref="Tooltip"/> placement.
        /// </summary>
        /// <param name="element">The target visual element.</param>
        /// <param name="placement">The placement value.</param>
        /// <exception cref="ArgumentNullException">The <see cref="VisualElement"/> object can't be null.</exception>
        public static void SetPreferredTooltipPlacement(this VisualElement element, PopoverPlacement placement)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (element is ContextProvider provider)
            {
                provider.preferredTooltipPlacement = placement;
                return;
            }

            var data = k_AdditionalDataCache.GetOrCreateValue(element);
            data.preferredTooltipPlacement = placement;
        }
        
        /// <summary>
        /// Get the tooltip template for a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element"> The target visual element.</param>
        /// <returns> The tooltip template.</returns>
        /// <exception cref="ArgumentNullException"> The <see cref="VisualElement"/> object can't be null.</exception>
        public static VisualElement GetTooltipTemplate(this VisualElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return k_AdditionalDataCache.TryGetValue(element, out var data) ? data.tooltipTemplate : null;
        }
        
        /// <summary>
        /// Set the tooltip template for a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element"> The target visual element.</param>
        /// <param name="template"> The tooltip template.</param>
        /// <exception cref="ArgumentNullException"> The <see cref="VisualElement"/> object can't be null.</exception>
        public static void SetTooltipTemplate(this VisualElement element, VisualElement template)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            
            var data = k_AdditionalDataCache.GetOrCreateValue(element);
            data.tooltipTemplate = template;
        }

        /// <summary>
        /// Additional Data that should be stored on any <see cref="VisualElement"/> object.
        /// </summary>
        // ReSharper disable once ClassNeverInstantiated.Local
        class AdditionalData
        {
            /// <summary>
            /// Callbacks to invoke when the element is attached to a panel to send the context changed event.
            /// </summary>
            internal Dictionary<Type, EventCallback<AttachToPanelEvent>>
                sendContextChangedOnAttachedToPanelCallbacksPerType { get; }  = new();
            
            /// <summary>
            /// Callbacks to invoke when the context changes.
            /// </summary>
            internal Dictionary<Type, List<object>> contextChangedCallbacksPerType { get; } = new ();
            
            /// <summary>
            /// Callbacks to invoke when the element is attached to a panel to change the context.
            /// </summary>
            internal Dictionary<Type, List<EventCallback<AttachToPanelEvent>>> 
                contextChangedOnAttachedToPanelCallbacksPerType { get; } = new ();

            /// <summary>
            /// The Contexts collection.
            /// </summary>
            internal Dictionary<Type, IContext> contexts { get; } = new ();
                
            /// <summary>
            /// The preferred placement for a tooltip.
            /// </summary>
            public PopoverPlacement preferredTooltipPlacement { get; set; } = Tooltip.defaultPlacement;

            /// <summary>
            /// The tooltip template to use for this element.
            /// </summary>
            public VisualElement tooltipTemplate { get; set; } = null;
        }

        /// <summary>
        /// Find the closest context provider in the hierarchy of a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element"> The <see cref="VisualElement"/> object.</param>
        /// <typeparam name="T"> The type of the context.</typeparam>
        /// <returns> The context provider.</returns>
        /// <exception cref="ArgumentNullException"> The <see cref="VisualElement"/> object can't be null.</exception>
        public static VisualElement GetContextProvider<T>(this VisualElement element)
            where T : IContext
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            
            var el = element;
            
            while (el != null)
            {
                if (k_AdditionalDataCache.TryGetValue(el, out var data) && data.contexts.ContainsKey(typeof(T))) 
                    return el;

                el = el.parent;
            }
            
            return null;
        }

        /// <summary>
        /// Get the context of a given type in a <see cref="VisualElement"/>.
        /// </summary>
        /// <remarks>
        /// This method will look for the context in the current element and its parents without checking
        /// if the element is part of the visual tree.
        /// </remarks>
        /// <param name="element"> The <see cref="VisualElement"/> object.</param>
        /// <typeparam name="T"> The type of the context.</typeparam>
        /// <returns> The context.</returns>
        /// <exception cref="ArgumentNullException"> The <see cref="VisualElement"/> object can't be null.</exception>
        public static T GetContext<T>(this VisualElement element) 
            where T : IContext
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (element.GetContextProvider<T>() is { } provider)
            {
                k_AdditionalDataCache.TryGetValue(provider, out var data);
                return (T)data.contexts[typeof(T)];
            }

            return default;
        }

        /// <summary>
        /// Get the context of a given type in a <see cref="VisualElement"/> if this element is provider of this context.
        /// </summary>
        /// <param name="element"> The <see cref="VisualElement"/> object.</param>
        /// <typeparam name="T"> The type of the context.</typeparam>
        /// <returns> The context if the element is provider of this context, null otherwise.</returns>
        /// <exception cref="ArgumentNullException"> The <see cref="VisualElement"/> object can't be null.</exception>
        public static T GetSelfContext<T>(this VisualElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            
            if (k_AdditionalDataCache.TryGetValue(element, out var data) && data.contexts.ContainsKey(typeof(T)))
                return (T)data.contexts[typeof(T)];
            
            return default;
        }
        
        /// <summary>
        /// Make the element provide a context of a given type in a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element"> The <see cref="VisualElement"/> object.</param>
        /// <param name="context"> The context.</param>
        /// <typeparam name="T"> The type of the context.</typeparam>
        /// <exception cref="ArgumentNullException"> The <see cref="VisualElement"/> object can't be null.</exception>
        public static void ProvideContext<T>(this VisualElement element, T context) 
            where T : IContext
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var data = k_AdditionalDataCache.GetOrCreateValue(element);
            
            void OnAttached(AttachToPanelEvent evt)
            {
                if (evt.destinationPanel != null)
                    element.SendContextChangedEvent(context);
            }

            if (context == null)
            {
                if (data.contexts.ContainsKey(typeof(T)))
                {
                    data.contexts.Remove(typeof(T));
                    var ancestorContext = element.GetContext<T>();
                    element.SendContextChangedEvent(ancestorContext);
                }
                if (data.sendContextChangedOnAttachedToPanelCallbacksPerType.TryGetValue(typeof(T), out var callback))
                {
                    data.sendContextChangedOnAttachedToPanelCallbacksPerType.Remove(typeof(T));
                    element.UnregisterCallback(callback);
                }
            }
            else
            {
                data.contexts[typeof(T)] = context;
                if (!data.sendContextChangedOnAttachedToPanelCallbacksPerType.ContainsKey(typeof(T)))
                {
                    var callback = new EventCallback<AttachToPanelEvent>(OnAttached);
                    data.sendContextChangedOnAttachedToPanelCallbacksPerType[typeof(T)] = callback;
                    element.RegisterCallback(callback);
                }
                element.SendContextChangedEvent(context);
            }
        }
        
        /// <summary>
        /// Check if a <see cref="VisualElement"/> provides a context of a given type.
        /// </summary>
        /// <param name="element"> The <see cref="VisualElement"/> object.</param>
        /// <typeparam name="T"> The type of the context.</typeparam>
        /// <returns> True if the element provides the context, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"> The <see cref="VisualElement"/> object can't be null.</exception>
        public static bool IsContextProvider<T>(this VisualElement element) 
            where T : IContext
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return k_AdditionalDataCache.TryGetValue(element, out var data) && data.contexts.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Register a callback to be invoked when the context of a given type changes in a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element"> The <see cref="VisualElement"/> object.</param>
        /// <param name="callback"> The callback.</param>
        /// <typeparam name="T"> The type of the context.</typeparam>
        /// <exception cref="ArgumentNullException"> The <see cref="VisualElement"/> object and the callback can't be null.</exception>
        public static void RegisterContextChangedCallback<T>(this VisualElement element, EventCallback<ContextChangedEvent<T>> callback) 
            where T : IContext
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            
            void SendContextChangedEventLocal()
            {
                var context = element.GetContext<T>();
                using var evt = ContextChangedEvent<T>.GetPooled(context);
                evt.target = element;
                callback(evt);
            }

            void OnAttached(AttachToPanelEvent attachToPanelEvent)
            {
                if (attachToPanelEvent.destinationPanel != null)
                    SendContextChangedEventLocal();
            }
            
            var data = k_AdditionalDataCache.GetOrCreateValue(element);
            if (data.contextChangedCallbacksPerType.TryGetValue(typeof(T), out var callbacks))
            {
                if (!callbacks.Contains(callback))
                {
                    callbacks.Add(callback);
                    var attachCallback = new EventCallback<AttachToPanelEvent>(OnAttached);
                    data.contextChangedOnAttachedToPanelCallbacksPerType[typeof(T)].Add(attachCallback);
                    element.RegisterCallback(attachCallback);
                    if (element.panel != null)
                        SendContextChangedEventLocal();
                }
            }
            else
            {
                callbacks = new List<object> { callback };
                data.contextChangedCallbacksPerType[typeof(T)] = callbacks;
                var attachCallback = new EventCallback<AttachToPanelEvent>(OnAttached);
                data.contextChangedOnAttachedToPanelCallbacksPerType[typeof(T)] = new List<EventCallback<AttachToPanelEvent>> { attachCallback };
                element.RegisterCallback(attachCallback);
                if (element.panel != null)
                    SendContextChangedEventLocal();
            }
        }
        
        /// <summary>
        /// Unregister a callback to be invoked when the context of a given type changes in a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element"> The <see cref="VisualElement"/> object.</param>
        /// <param name="callback"> The callback.</param>
        /// <typeparam name="T"> The type of the context.</typeparam>
        /// <exception cref="ArgumentNullException"> The <see cref="VisualElement"/> object and the callback can't be null.</exception>
        public static void UnregisterContextChangedCallback<T>(this VisualElement element, EventCallback<ContextChangedEvent<T>> callback) 
            where T : IContext
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            
            if (k_AdditionalDataCache.TryGetValue(element, out var data) && 
                data.contextChangedCallbacksPerType.TryGetValue(typeof(T), out var callbacks))
            {
                var index = callbacks.IndexOf(callback);
                
                if (index >= 0)
                {
                    callbacks.RemoveAt(index);
                    var attachCallback = data.contextChangedOnAttachedToPanelCallbacksPerType[typeof(T)][index];
                    data.contextChangedOnAttachedToPanelCallbacksPerType[typeof(T)].RemoveAt(index);
                    element.UnregisterCallback(attachCallback);
                }
            }
        }
        
        internal static void SendContextChangedEvent<T>(this VisualElement element, T context) 
            where T : IContext
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            
            if (element.panel == null)
                return;
            
            using var evt = ContextChangedEvent<T>.GetPooled(context);
            evt.target = element;
            
            void SendContextChangedEventToChildren(VisualElement parent, ContextChangedEvent<T> evt)
            {
                if (parent.IsContextProvider<T>())
                    return;

                if (k_AdditionalDataCache.TryGetValue(parent, out var data)
                    && data.contextChangedCallbacksPerType.TryGetValue(typeof(T), out var callbacks))
                {
                    foreach (var cb in callbacks)
                    {
                        ((EventCallback<ContextChangedEvent<T>>)cb).Invoke(evt);
                    }
                }
                
                foreach (var c in parent.Children())
                {
                    SendContextChangedEventToChildren(c, evt);
                }
            }

            foreach (var child in element.Children())
            {
                SendContextChangedEventToChildren(child, evt);
            }
        }
    }
}
