using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>
    /// Marks a static parameterless method returning a <see cref="VisualElement"/> as a live
    /// example for a Visual Documentation page. The element is instantiated when the page is
    /// built and displayed inside the section named by <see cref="section"/>; if the page does
    /// not author that section in its XML documentation, a dedicated section is appended so the
    /// demo stays reachable from the table of contents.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    sealed class VisualDocDemoAttribute : Attribute
    {
        /// <summary>The registry id of the target page (e.g. "button").</summary>
        public string pageId { get; }

        /// <summary>The title of the section the demo is displayed in.</summary>
        public string section { get; }

        /// <summary>Relative order among the demos of the same page.</summary>
        public int order { get; set; }

        /// <param name="pageId">The registry id of the target page (e.g. "button").</param>
        /// <param name="section">The title of the section the demo is displayed in.</param>
        public VisualDocDemoAttribute(string pageId, string section = "Overview")
        {
            this.pageId = pageId;
            this.section = section;
        }
    }

    /// <summary>
    /// A live example registered for a documentation page.
    /// </summary>
    class VisualDocDemo
    {
        /// <summary>The title of the section the demo is displayed in.</summary>
        public string section { get; }

        /// <summary>Relative order among the demos of the same page.</summary>
        public int order { get; }

        /// <summary>Creates the demo element. Invoked each time the page is built.</summary>
        public Func<VisualElement> factory { get; }

        public VisualDocDemo(string section, int order, Func<VisualElement> factory)
        {
            this.section = section;
            this.order = order;
            this.factory = factory;
        }
    }

    /// <summary>
    /// Collects the live examples declared with <see cref="VisualDocDemoAttribute"/> in this
    /// assembly, indexed by page id.
    /// </summary>
    static class VisualDocDemoRegistry
    {
        static readonly List<VisualDocDemo> k_NoDemos = new List<VisualDocDemo>();

        static Dictionary<string, List<VisualDocDemo>> s_DemosByPage;

        /// <summary>
        /// Returns the demos registered for a page, ordered, or an empty list.
        /// </summary>
        public static IReadOnlyList<VisualDocDemo> GetDemos(string pageId)
        {
            EnsureScanned();
            return s_DemosByPage.TryGetValue(pageId, out var demos) ? demos : k_NoDemos;
        }

        /// <summary>
        /// The page ids that have at least one registered demo.
        /// </summary>
        public static IEnumerable<string> pageIds
        {
            get
            {
                EnsureScanned();
                return s_DemosByPage.Keys;
            }
        }

        static void EnsureScanned()
        {
            if (s_DemosByPage != null)
                return;

            s_DemosByPage = new Dictionary<string, List<VisualDocDemo>>();
            foreach (var type in typeof(VisualDocDemoRegistry).Assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attribute = method.GetCustomAttribute<VisualDocDemoAttribute>();
                    if (attribute == null)
                        continue;

                    if (!typeof(VisualElement).IsAssignableFrom(method.ReturnType) || method.GetParameters().Length != 0)
                    {
                        Debug.LogWarning($"[VisualDoc] Demo method {type.Name}.{method.Name} must be a " +
                            "static parameterless method returning a VisualElement.");
                        continue;
                    }

                    if (!s_DemosByPage.TryGetValue(attribute.pageId, out var demos))
                    {
                        demos = new List<VisualDocDemo>();
                        s_DemosByPage[attribute.pageId] = demos;
                    }

                    var m = method;
                    demos.Add(new VisualDocDemo(attribute.section, attribute.order,
                        () => (VisualElement)m.Invoke(null, null)));
                }
            }

            foreach (var demos in s_DemosByPage.Values)
                demos.Sort((a, b) => a.order.CompareTo(b.order));
        }
    }
}
