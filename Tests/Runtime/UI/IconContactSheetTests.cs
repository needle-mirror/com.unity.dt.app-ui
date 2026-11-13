using System;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Captures every icon in the library as a handful of grid "contact sheets".
    /// </summary>
    /// <remarks>
    /// <para>
    /// Icons ship as SVG imported to <c>VectorImage</c> rather than as prebaked PNGs, so Unity
    /// tessellates them at runtime. Antialiasing therefore differs from the source SVG, which is
    /// expected and harmless. What is not harmless is the other failure mode: the importer and
    /// tessellator can drop a subpath, invert a fill, or blank an icon outright on complex
    /// geometry, silently. This fixture is what makes that visible.
    /// </para>
    /// <para>
    /// One capture per icon would mean thousands of tests and thousands of PNGs, so icons are laid
    /// out in a fixed grid instead — around 300 per sheet, ten sheets per theme. Per-icon
    /// diagnostics survive because the layout is deterministic: a cell index is an icon name.
    /// </para>
    /// <para>
    /// The layout is <b>not</b> computed here. It is read from <c>icon-sheets.json</c>, generated
    /// by <c>scripts/gen-icon-sheets.mjs</c>, because the oracle rasterizer
    /// (<c>build-contact-sheets.mjs</c>) has to place the same icon in the same cell. If the two
    /// sides ever disagreed, every diff would be noise.
    /// </para>
    /// <para>
    /// Captures land alongside the component snapshots via <see cref="Utils.SnapshotOutputPath"/>,
    /// so the existing <c>snapshots</c> artifact carries them with no CI change, and the external
    /// VRT tool compares them against previously approved sheets. The separate comparison against
    /// the source-SVG oracle happens in its own Yamato job.
    /// </para>
    /// </remarks>
    [TestFixture]
    [TestOf(typeof(Icon))]
    class IconContactSheetTests
    {
        /// <summary>
        /// Sheets render white-on-black in both themes, rather than on the theme's own surface
        /// colour.
        /// </summary>
        /// <remarks>
        /// The oracle has to reproduce the background exactly or every pixel of it reads as a
        /// diff. A fixed background also keeps the dark and light sheets comparable to each other:
        /// the only thing that then varies between them is which artwork an icon resolves to.
        /// </remarks>
        static readonly Color k_Background = Color.black;

        /// <summary>Carries the pinned tint from <c>IconSheetCatalogue.uss</c>.</summary>
        const string k_CellUssClassName = "appui-icon-sheet-cell";

        UIDocument m_TestUI;

        PanelSettings m_PanelSettings;

        StyleSheet m_Catalogue;

        SheetManifest m_Manifest;

        bool m_SetupDone;

        /// <summary>
        /// Panel settings pinned to one panel point per screen pixel.
        /// </summary>
        /// <remarks>
        /// The shared <see cref="Utils.panelSettingsInstance"/> uses
        /// <see cref="PanelScaleMode.ConstantPhysicalSize"/>, which scales the panel by
        /// <c>Screen.dpi / referenceDpi</c>. A headless CI agent reports no usable DPI and falls
        /// back to 1.0, which is the 1:1 the oracle rasterizer assumes; a Retina workstation
        /// reports roughly twice that and lays the grid out at 2x, so half the columns fall off
        /// the capture and every cell holds a different icon than the oracle's. The sheets stay
        /// the right size either way, so nothing fails -- the comparison just reports several
        /// hundred icons broken. The grid is a pixel contract with the oracle, so pin the scale
        /// here rather than inherit the agent's display.
        /// </remarks>
        static PanelSettings CreatePinnedPanelSettings()
        {
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.themeStyleSheet = Utils.appUiThemeStyleSheet;
            settings.scaleMode = PanelScaleMode.ConstantPixelSize;
            settings.scale = 1f;
            return settings;
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (!m_SetupDone)
            {
                var scene = SceneManager.CreateScene("IconContactSheetScene-" + Random.Range(1, 1000000));
                while (!SceneManager.SetActiveScene(scene))
                {
                    yield return null;
                }

                m_TestUI = Utils.ConstructTestUI();
                m_PanelSettings = CreatePinnedPanelSettings();
                m_TestUI.panelSettings = m_PanelSettings;
                Screen.SetResolution(1200, 600, FullScreenMode.Windowed);
                TimeUtils.timeOverride = 1f;
            }

            m_TestUI.rootVisualElement.Clear();
            m_SetupDone = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TimeUtils.timeOverride = null;
            if (m_TestUI)
                Object.Destroy(m_TestUI.gameObject);

            if (m_PanelSettings)
                Object.Destroy(m_PanelSettings);

            m_PanelSettings = null;
            m_TestUI = null;
            m_SetupDone = false;
#pragma warning disable CS0618
            SceneManager.UnloadScene(SceneManager.GetActiveScene());
#pragma warning restore CS0618
        }

        [UnityTest]
        public IEnumerator CreateSnapshot()
        {
            // Same gate the component harness uses: playmode-in-editor never writes a PNG, because
            // ScreenCapture grabs the framebuffer and there isn't a meaningful one.
            var capture = !Application.isEditor && !string.IsNullOrEmpty(Utils.snapshotsOutputDir);

            // Bail before touching the assets, not after. They live in the dev projects' Resources
            // (DevProject6 and DevProject7), so they exist only where a capture can actually happen:
            // the Yamato jobs that build a StandaloneOSX player from those projects. Every other run reaching this fixture is
            // playmode-in-editor -- including the throwaway project `upm-pvp create-test-project`
            // builds from the package alone, which has no such Resources and would otherwise fail
            // here rather than skip. Loading late also spares those runs from laying out twenty
            // grids of three hundred icons only to write nothing.
            if (!capture)
            {
                Assert.Ignore(
                    "Icon contact sheets are captured only in a player run with SNAPSHOTS_OUTPUT_DIR set.");
                yield break;
            }

            var manifestAsset = Resources.Load<TextAsset>("IconSheets/icon-sheets");
            Assert.IsNotNull(manifestAsset,
                "icon-sheets.json is missing from the test project's Resources. Run 'pnpm symbols:sync'.");
            m_Manifest = JsonUtility.FromJson<SheetManifest>(manifestAsset.text);

            // Icon.uss imports "Icons - Minimal.uss", which declares a couple of hundred icons.
            // Icon resolves artwork purely through USS custom properties, so without the full
            // catalogue every icon outside that subset renders as nothing and the sheets come out
            // blank.
            //
            // Both assets live in the test project's Resources, not the package's: a package-side
            // Resources directory fails PVP-38-2, and would ship every icon it references into
            // consumer builds. Same mechanism, and the same single-job blast radius, as the
            // Themes/App UI.tss the harness already loads this way.
            m_Catalogue = Resources.Load<StyleSheet>("IconSheets/IconSheetCatalogue");
            Assert.IsNotNull(m_Catalogue,
                "IconSheetCatalogue.uss is missing from the test project's Resources.");

            foreach (var theme in Utils.themes)
            {
                foreach (var sheet in m_Manifest.sheets)
                {
                    yield return CaptureSheet(sheet, theme);
                }
            }
        }

        IEnumerator CaptureSheet(Sheet sheet, string theme)
        {
#if !UNITY_EDITOR
            Screen.SetResolution(1200, 600, FullScreenMode.Windowed);
            m_TestUI.rootVisualElement.styleSheets.Add(Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif

            // Panel adds the dark/light context class itself, suffix-matched on the theme name.
            // That is what selects the `d_*` artwork for the icons that ship a distinct dark
            // drawing, so the theme loop is not merely a tint change.
            var panel = new Panel
            {
                theme = theme,
                density = "normal",
                layoutDirection = Dir.Ltr,
                lang = "en",
            };
            panel.styleSheets.Add(m_Catalogue);
            panel.style.backgroundColor = k_Background;

            m_TestUI.rootVisualElement.Clear();
            m_TestUI.rootVisualElement.style.backgroundColor = k_Background;
            m_TestUI.rootVisualElement.Add(panel);

            // No padded container here, unlike the component harness: the grid has to start at the
            // panel's origin or the cells no longer line up with the oracle's.
            var inset = (m_Manifest.cell - m_Manifest.glyph) / 2f;
            for (var i = 0; i < sheet.icons.Length; i++)
            {
                var icon = new Icon
                {
                    iconName = sheet.icons[i],
                    variant = IconVariant.Regular,
                };
                // Pins the tint to white, through IconSheetCatalogue.uss. It has to go through USS
                // rather than assigning Image.tintColor here: Image re-reads the tint from the
                // --unity-image-tint-color custom property on every custom-style resolution, so a
                // value set in C# is clobbered as soon as styles resolve.
                icon.AddToClassList(k_CellUssClassName);
                icon.style.position = Position.Absolute;
                icon.style.left = i % m_Manifest.cols * m_Manifest.cell + inset;
                icon.style.top = i / m_Manifest.cols * m_Manifest.cell + inset;
                icon.style.width = m_Manifest.glyph;
                icon.style.height = m_Manifest.glyph;
                panel.Add(icon);
            }

            yield return null;
            const int waitFrames = 3;
            for (var i = 0; i < waitFrames; i++)
            {
                yield return null;
            }

            yield return new WaitForEndOfFrame();
            var outputFilePath = Utils.SnapshotOutputPath($"IconSheet.{sheet.index:D2}.{theme}.png");
            ScreenCapture.CaptureScreenshot(outputFilePath);
            yield return null;
            yield return new WaitUntilOrTimeOut(
                () => Utils.FileAvailable(outputFilePath),
                false,
                TimeSpan.FromSeconds(10));
            Assert.IsTrue(Utils.FileAvailable(outputFilePath), $"No capture written for {outputFilePath}");
        }

        /// <summary>The layout contract shared with the oracle rasterizer.</summary>
        [Serializable]
        class SheetManifest
        {
#pragma warning disable CS0649
            public int cell;
            public int glyph;
            public int cols;
            public int rows;
            public Sheet[] sheets;
#pragma warning restore CS0649
        }

        [Serializable]
        class Sheet
        {
#pragma warning disable CS0649
            public int index;
            public string[] icons;
#pragma warning restore CS0649
        }
    }
}
