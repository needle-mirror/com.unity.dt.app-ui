using System;
using System.Reflection;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Editor.Theming
{
    /// <summary>
    /// Compiles USS text into a <see cref="StyleSheet"/> or <see cref="ThemeStyleSheet"/> during an
    /// asset import.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unity exposes no public "parse this USS string" API, so this drives the same internal
    /// implementation its own importers use. <c>StyleSheetImporter.OnImportAsset</c> does nothing
    /// more than read the file, create a <see cref="StyleSheet"/>, and hand both to a
    /// <c>StyleSheetImporterImpl</c> built around the current
    /// <see cref="AssetImportContext"/> — which is exactly what happens below. The type is
    /// internal, but its constructor and its <c>Import</c> method are public members of it, so no
    /// private reflection is involved beyond locating the type.
    /// </para>
    /// <para>
    /// This is deliberately NOT the "write a temporary .uss and let the AssetDatabase import it"
    /// approach. Calling <c>AssetDatabase.ImportAsset</c> from inside
    /// <c>OnImportAsset</c> re-enters the import pipeline, and the scratch file has to live under
    /// <c>Assets/</c>, where a crash mid-import strands it. Driving the parser directly touches the
    /// AssetDatabase not at all.
    /// </para>
    /// <para>
    /// A further benefit: the impl reports USS syntax errors onto the very
    /// <see cref="AssetImportContext"/> it is given, so a malformed generated stylesheet surfaces
    /// against the theme asset without any error plumbing here.
    /// </para>
    /// <para>
    /// If a future Unity renames or reshapes the internal type, <see cref="available"/> turns false
    /// and callers are expected to degrade rather than throw. `StyleSheetCompilerTests` asserts
    /// <see cref="available"/>, so the breakage surfaces as a failing test rather than as themes
    /// that silently stop compiling.
    /// </para>
    /// </remarks>
    static class StyleSheetCompiler
    {
        const string k_ImplTypeName = "UnityEditor.UIElements.StyleSheets.StyleSheetImporterImpl";

        static readonly ConstructorInfo k_Ctor;
        static readonly MethodInfo k_Import;

        static StyleSheetCompiler()
        {
            // Resolved from the assembly that defines AssetImportContext, the type the impl's
            // constructor takes, rather than by naming an assembly: both live in
            // UnityEditor.CoreModule from 6000.3 through the CoreCLR editors. Scanning AppDomain
            // assemblies instead is unsafe in Unity and unavailable under CoreCLR.
            var implType = typeof(AssetImportContext).Assembly.GetType(k_ImplTypeName, false);

            if (implType == null)
                return;

            k_Ctor = implType.GetConstructor(new[] { typeof(AssetImportContext) });
            k_Import = implType.GetMethod("Import", new[] { typeof(StyleSheet), typeof(string) });
        }

        /// <summary>
        /// Whether USS can be compiled in-process. False means the internal API moved; callers
        /// should fall back rather than fail.
        /// </summary>
        public static bool available => k_Ctor != null && k_Import != null;

        /// <summary>
        /// Parses <paramref name="ussText"/> into a new stylesheet.
        /// </summary>
        /// <param name="ctx">The import context; parse errors are reported against it.</param>
        /// <param name="ussText">The USS source.</param>
        /// <param name="name">The name to give the created object.</param>
        /// <typeparam name="T">
        /// <see cref="StyleSheet"/> for a sheet to add to an element, or
        /// <see cref="ThemeStyleSheet"/> for one assignable to <c>PanelSettings.themeStyleSheet</c>.
        /// </typeparam>
        /// <returns>The compiled stylesheet, or null when <see cref="available"/> is false.</returns>
        public static T Compile<T>(AssetImportContext ctx, string ussText, string name)
            where T : StyleSheet
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));
            if (!available)
                return null;

            var sheet = ScriptableObject.CreateInstance<T>();
            sheet.name = name;
            // Matches what Unity's own importer does: the asset is generated, so editing it by hand
            // in the inspector would be edits that the next reimport throws away.
            sheet.hideFlags = HideFlags.NotEditable;

            var impl = k_Ctor.Invoke(new object[] { ctx });
            k_Import.Invoke(impl, new object[] { sheet, ussText });
            return sheet;
        }
    }
}
