using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Keeps App UI from adding a public type whose name UI Toolkit already uses.
    /// </summary>
    /// <remarks>
    /// Nearly every consumer file imports both <c>UnityEngine.UIElements</c> and
    /// <c>Unity.AppUI.UI</c>, so a shared name such as <c>ListView</c> turns every unqualified use
    /// into CS0104 the day App UI ships it. The names listed in <see cref="k_PreExisting"/> shipped
    /// before this test existed and stay for compatibility. The list may only shrink.
    /// </remarks>
    [TestFixture]
    class UitkNameShadowingTests
    {
        static readonly HashSet<string> k_PreExisting = new HashSet<string>
        {
            "BaseSlider", "BoundsField", "BoundsIntField", "Button", "Clickable", "ColorField",
            "DoubleField", "EnumField", "FloatField", "HelpBox", "LongField", "MaskField", "RectField",
            "RectIntField", "Slider", "SliderInt", "TextField", "TextOverflow", "Toggle", "Toolbar",
            "Vector2Field", "Vector2IntField", "Vector3Field", "Vector3IntField", "Vector4Field",
            "VisualElementExtensions",
        };

        static IEnumerable<string> UitkPublicTypeNames()
        {
            var assemblies = new List<System.Reflection.Assembly> { typeof(VisualElement).Assembly };
#if UNITY_EDITOR
            assemblies.Add(typeof(UnityEditor.UIElements.ObjectField).Assembly);
#endif
            return assemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => !t.IsNested
                    && (t.Namespace == "UnityEngine.UIElements" || t.Namespace == "UnityEditor.UIElements"))
                .Select(StripArity);
        }

        static string StripArity(System.Type t)
        {
            var tick = t.Name.IndexOf('`');
            return tick < 0 ? t.Name : t.Name.Substring(0, tick);
        }

        [Test]
        public void AppUIPublicTypes_ShouldNotShadowUIToolkitTypes()
        {
            var uitk = new HashSet<string>(UitkPublicTypeNames());
            var shadowing = typeof(Unity.AppUI.UI.Button).Assembly
                .GetExportedTypes()
                .Where(t => !t.IsNested && t.Namespace != null && t.Namespace.StartsWith("Unity.AppUI"))
                .Select(StripArity)
                .Distinct()
                .Where(n => uitk.Contains(n) && !k_PreExisting.Contains(n))
                .OrderBy(n => n)
                .ToList();

            Assert.IsEmpty(shadowing,
                "These App UI types share a name with a public UI Toolkit type, so any file importing " +
                "both namespaces fails with CS0104. Rename them: " + string.Join(", ", shadowing));
        }
    }
}
