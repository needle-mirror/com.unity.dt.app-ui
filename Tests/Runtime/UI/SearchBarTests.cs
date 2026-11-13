using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SearchBar))]
    class SearchBarTests : VisualElementTests<SearchBar>
    {
        protected override string mainUssClassName => SearchBar.ussClassName;

        // Handed over to SearchBarSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        /// <summary>
        /// The registry's <c>search-bar</c> input names <c>text-base</c> itself, so it keeps the base
        /// step at <c>sm</c>. UITK deliberately diverges there: a <c>text-base</c> line overruns the
        /// compact <c>Sm</c> box and the input container clips it, so an <c>Sm</c> SearchBar steps down
        /// to <c>text-sm</c> with the TextField it is.
        /// </summary>
        [UnityTest]
        public IEnumerator Text_RendersAtTheTextFieldStep_AndFits_AtEverySizeAndDensity()
        {
            foreach (var density in FieldTextSize.densities)
            {
                var bars = new List<(Size size, SearchBar filled, SearchBar empty)>();
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    foreach (var (size, _) in FieldTextSize.sizes)
                    {
                        var filled = new SearchBar { size = size, value = "Ag" };
                        var empty = new SearchBar { size = size, placeholder = "Ag" };
                        panel.Add(filled);
                        panel.Add(empty);
                        bars.Add((size, filled, empty));
                    }
                }, s => stage = s);

                foreach (var (size, filled, empty) in bars)
                {
                    var value = filled.Q<UnityEngine.UIElements.TextField>(className: TextField.inputUssClassName)
                        .Q<TextElement>();
                    var tier = FieldTextSize.TextFieldTier(size);
                    FieldTextSize.AssertTier(stage, value, tier, $"a {size} SearchBar's value");
                    FieldTextSize.AssertTier(stage, empty.Q(className: TextField.placeholderUssClassName), tier,
                        $"a {size} SearchBar's placeholder");
                    FieldTextSize.AssertFits(stage, value, filled, $"a {size} SearchBar's value");
                }
            }
        }

        /// <summary>
        /// An <c>Sm</c> SearchBar takes TextField's numbered <c>sm</c> step. A SearchBar rule in
        /// TextField.uss would outrank it, which is how the registry's <c>text-base</c> at <c>sm</c>
        /// was once mirrored, and would clip at compact density again.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Text_ReadsTheNumberedSmStep_AtSm()
        {
            FieldTextSize.AssertFontSize("TextField.uss", ".appui-textfield--size-sm .appui-textfield__input", 100);
            StringAssert.DoesNotMatch(@"(?m)^\s*\.appui-searchbar[^{]*\{[^}]*font-size", UssSource.Read("TextField.uss"),
                "No SearchBar rule may override the TextField text steps.");
        }
    }
}
