using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.AppUI.Navigation;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_LOCALIZATION_PRESENT
using UnityEngine.Localization.Settings;
#endif

namespace Unity.AppUI.Tests
{
    static class Utils
    {
        static readonly MethodInfo k_ImportXmlFromString;

        static readonly object k_UxmlImporterImplInstance;

        static bool s_ConditionalIgnoresRegistered;

        /// <summary>
        /// The current members of an enum, in declaration order, without its [Obsolete] aliases.
        /// </summary>
        /// <remarks>
        /// An obsolete alias such as <c>Size.S = Sm</c> shares its value with a current member, so
        /// <see cref="Enum.GetValues(Type)"/> lists that value twice and <c>ToString()</c> may return
        /// either name. Read the name from here instead.
        /// </remarks>
        internal static IEnumerable<(T value, string name)> CurrentEnumMembers<T>() where T : struct, Enum
        {
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!field.IsDefined(typeof(ObsoleteAttribute), false))
                    yield return ((T)field.GetValue(null), field.Name);
            }
        }

        /// <summary>
        /// The current (non-obsolete) name of an enum value. See <see cref="CurrentEnumMembers{T}"/>.
        /// </summary>
        internal static string CurrentEnumName<T>(T value) where T : struct, Enum =>
            CurrentEnumMembers<T>().First(m => EqualityComparer<T>.Default.Equals(m.value, value)).name;

        /// <summary>
        /// Registers the <c>IgnoreInEditor</c> / <c>IgnoreInPlayer</c> conditions that
        /// <see cref="ConditionalIgnoreAttribute"/> resolves against.
        /// </summary>
        /// <remarks>
        /// This cannot live in the static constructor alone. NUnit resolves a
        /// <c>[ConditionalIgnore]</c> key while it *builds the test tree*, before any test body
        /// runs, and a static constructor only fires when something first touches the type. In the
        /// Editor some other code path happens to touch <see cref="Utils"/> in time; in a player
        /// build nothing does, so every key is still unregistered when the tree is built and every
        /// <c>[ConditionalIgnore("IgnoreInPlayer", ...)]</c> silently fails to ignore. The tests
        /// then run in the player anyway and fail on Editor-only expectations, such as reading USS
        /// source out of <c>Packages/</c> or instantiating a VisualElement from UXML.
        ///
        /// <see cref="RuntimeInitializeOnLoadMethodAttribute"/> runs before the TestRunner builds
        /// its tree in a player, which is the guarantee the static constructor never had.
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        internal static void RegisterConditionalIgnores()
        {
            // AddConditionalIgnoreMapping throws on a duplicate key, and this runs from both the
            // runtime hook and the static constructor.
            if (s_ConditionalIgnoresRegistered)
                return;
            s_ConditionalIgnoresRegistered = true;

            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping("IgnoreInEditor", Application.isEditor);
            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping("IgnoreInPlayer", !Application.isEditor);
        }

        static Utils()
        {
            RegisterConditionalIgnores();
#if UNITY_EDITOR
            // UXMLImporterImpl is internal, so it is looked up in the assembly of a public type
            // that ships beside it (UnityEditor.UIElementsModule) instead of by scanning every
            // loaded assembly, which AppDomain-based enumeration cannot do under CoreCLR.
            var importerType = typeof(UnityEditor.UIElements.UxmlAttributeConverter<>).Assembly
                .GetType("UnityEditor.UIElements.UXMLImporterImpl", false);

            if (importerType == null)
            {
                Debug.LogError("Could not find UXMLImporterImpl type");
            }
            else
            {
                k_UxmlImporterImplInstance = importerType.GetConstructors(
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    .First(c => !c.GetParameters().Any())
                    .Invoke(Array.Empty<object>());

                k_ImportXmlFromString = importerType.GetMethod("ImportXmlFromString",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (k_ImportXmlFromString == null)
                    Debug.LogError("Could not find ImportXmlFromString method");
            }
#endif
        }

        internal static readonly string snapshotsOutputDir =
            Environment.GetEnvironmentVariable("SNAPSHOTS_OUTPUT_DIR") is {Length:>0} p ?
            Path.GetFullPath(p) : null;

        /// <summary>
        /// Resolves the capture path for one story, under a subdirectory named for the editor
        /// running the test.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The folder is <see cref="Application.unityVersion"/>, so a set of captures is labelled
        /// by the editor that actually produced it rather than by whichever harness took it. Story
        /// file names are identical across editors and harnesses, so two editors' folders can be
        /// compared image for image for the same set of tests.
        /// </para>
        /// <para>
        /// That only holds because exactly one harness captures a given component on a given
        /// editor. Where the com.unity.ui.test-framework path is live it takes over from
        /// <c>VisualElementTests</c> per component, via that fixture's
        /// <c>capturesSnapshotsElsewhere</c>.
        /// </para>
        /// </remarks>
        /// <param name="fileName">The story's file name.</param>
        /// <returns>An absolute path, with the directory created.</returns>
        internal static string SnapshotOutputPath(string fileName)
        {
            var dir = Path.Combine(snapshotsOutputDir ?? Application.dataPath, Application.unityVersion);
            Directory.CreateDirectory(dir);
            return Path.GetFullPath(Path.Combine(dir, fileName));
        }

        internal static IEnumerable<string> densities
        {
            get
            {
                yield return "normal";
                yield return "compact";
            }
        }

        internal static IEnumerable<string> themes
        {
            get
            {
                yield return "dark";
                yield return "light";
            }
        }

        internal static bool FileAvailable(string path)
        {
            if (!File.Exists(path))
                return false;

            var file = new FileInfo(path);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                // Can be either:
                // - file is processed by another thread
                // - file is still being written to
                // - file does not really exist yet
                return false;
            }
            finally
            {
                stream?.Close();
            }

            return true;
        }

        static ThemeStyleSheet s_AppUiThemeStyleSheet;

        /// <summary>
        /// The App UI theme style sheet, resolved from the package in the Editor and from
        /// Resources in a player. Shared by <see cref="panelSettingsInstance"/> and by the
        /// UI Test Framework fixtures, which take a <c>ThemeStyleSheet</c> directly.
        /// </summary>
        internal static ThemeStyleSheet appUiThemeStyleSheet
        {
            get
            {
                if (!s_AppUiThemeStyleSheet)
                {
#if UNITY_EDITOR
                    s_AppUiThemeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                        "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss");
#else
                    s_AppUiThemeStyleSheet = Resources.Load<ThemeStyleSheet>("Themes/App UI");
#endif
                }

                return s_AppUiThemeStyleSheet;
            }
        }

        static PanelSettings s_PanelSettingsInstance;

        internal static PanelSettings panelSettingsInstance
        {
            get
            {
                if (!s_PanelSettingsInstance)
                {
                    s_PanelSettingsInstance = ScriptableObject.CreateInstance<PanelSettings>();
                    s_PanelSettingsInstance.scaleMode = PanelScaleMode.ConstantPhysicalSize;
                    s_PanelSettingsInstance.themeStyleSheet = appUiThemeStyleSheet;
                }

                return s_PanelSettingsInstance;
            }
        }

        static NavGraphViewAsset s_NavGraphTestAsset;

        internal static NavGraphViewAsset navGraphTestAsset
        {
            get
            {
                if (!s_NavGraphTestAsset)
                {
#if UNITY_EDITOR
                    s_NavGraphTestAsset = AssetDatabase.LoadAssetAtPath<NavGraphViewAsset>(
                        "Packages/com.unity.dt.app-ui/Tests/Runtime/Navigation/NavGraphTestAsset.asset");
#endif
                }

                return s_NavGraphTestAsset;
            }
        }

        internal static IEnumerator WaitForLocalizationPreloaded()
        {
#if UNITY_LOCALIZATION_PRESENT
            var localizationSettings = LocalizationSettings.GetInstanceDontCreateDefault();
            if (localizationSettings)
            {
                yield return localizationSettings.GetInitializationOperation();
                yield return localizationSettings.GetSelectedLocaleAsync();
                Assert.IsTrue(localizationSettings.GetSelectedLocale());
            }
#else
            yield break;
#endif
        }

        internal static UIDocument ConstructTestUI()
        {
            var obj = new GameObject("TestUI");
            obj.AddComponent<Camera>();
            var doc = obj.AddComponent<UIDocument>();
            doc.panelSettings = panelSettingsInstance;

            return doc;
        }

#if ENABLE_PANEL_RENDERER
        internal static PanelRenderer ConstructTestUIPanelRenderer()
        {
            var obj = new GameObject("TestUIPanelRenderer");
            obj.AddComponent<Camera>();
            var pr = obj.AddComponent<PanelRenderer>();
            pr.panelSettings = panelSettingsInstance;

            return pr;
        }
#endif

        internal static VisualTreeAsset LoadUxmlTemplateFromString(string contents)
        {
            // ReSharper disable once RedundantAssignment
            VisualTreeAsset vta = null;
#if UNITY_EDITOR
            var args = new object[] {contents, null};
            k_ImportXmlFromString.Invoke(k_UxmlImporterImplInstance, args);
            vta = args[1] as VisualTreeAsset;
#endif
            return vta;
        }

        // ReSharper disable StringLiteralTypo
        internal static string loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod " +
            "tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation " +
            "ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in " +
            "voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non " +
            "proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        // ReSharper restore StringLiteralTypo
    }
}
