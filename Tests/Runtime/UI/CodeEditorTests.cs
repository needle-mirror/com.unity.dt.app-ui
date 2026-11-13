using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    // Unity's IDE integration packages declare a `Unity.CodeEditor` namespace. Namespace members
    // of an enclosing namespace outrank a file-level using directive, so from inside
    // `Unity.AppUI.Tests.UI` the bare name resolves to that namespace unless an alias declared in
    // this namespace body reclaims it.
    using CodeEditor = Unity.AppUI.UI.CodeEditor;

    [TestFixture]
    [TestOf(typeof(CodeEditor))]
    class CodeEditorTests : VisualElementTests<CodeEditor>
    {
        const string k_Uss = "CodeEditor.uss";

        protected override string mainUssClassName => CodeEditor.ussClassName;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Empty", (ctx) => new CodeEditor
                {
                    placeholder = "-- your script here",
                    style = { width = 320, height = 120 },
                });
                yield return new Story("Lua", (ctx) => new CodeEditor
                {
                    language = "lua",
                    value = "function update(dt)\n  print(dt)\nend\n",
                    style = { width = 320, height = 120 },
                });
                yield return new Story("ReadOnly", (ctx) => new CodeEditor
                {
                    language = "csharp",
                    value = "void Start() { }",
                    isReadOnly = true,
                    style = { width = 320, height = 120 },
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:CodeEditor />",
            @"<appui:CodeEditor language=""lua"" placeholder=""-- code"" />",
            @"<appui:CodeEditor is-read-only=""true"" indent-size=""4"" />",
            @"<appui:CodeEditor value=""print(1)"" />",
        };

        // -------------------------------------------------------------------
        // Value
        // -------------------------------------------------------------------

        [Test]
        public void Default_IsAnEmptyEditableEditorWithNoHighlighting()
        {
            var editor = new CodeEditor();
            Assert.AreEqual(string.Empty, editor.value);
            Assert.IsFalse(editor.isReadOnly);
            Assert.IsFalse(editor.isHighlighted,
                "Nothing is highlighted until a grammar and theme resolve; the input shows its own text.");
            Assert.AreEqual(CodeEditor.defaultIndentSize, editor.indentSize);
        }

        [Test]
        public void Value_RoundTripsThroughTheInnerTextArea()
        {
            var editor = new CodeEditor { value = "print(1)" };
            Assert.AreEqual("print(1)", editor.value);
            Assert.AreEqual("print(1)", editor.textArea.value,
                "The editor does not keep its own copy of the source.");
        }

        /// <summary>
        /// Events only travel through a panel, so this one has to be mounted — asserting on an unmounted
        /// element would pass against a component that sends nothing at all.
        /// </summary>
        [UnityTest]
        public IEnumerator Value_SendsOneChangeEventTargetedAtTheEditor()
        {
            var editor = new CodeEditor();
            var count = 0;
            IEventHandler target = null;
            string previous = null, current = null;
            editor.RegisterValueChangedCallback(evt =>
            {
                count++;
                target = evt.target;
                previous = evt.previousValue;
                current = evt.newValue;
            });

            yield return ChatTestUtils.Mount(m_TestUI, editor);

            editor.value = "a";
            Assert.AreEqual(1, count);
            Assert.AreSame(editor, target,
                "A caller registers on the editor and must not have to know about the inner TextArea.");
            Assert.AreEqual(string.Empty, previous);
            Assert.AreEqual("a", current);
        }

        [UnityTest]
        public IEnumerator SettingTheSameValue_SendsNothing()
        {
            var editor = new CodeEditor { value = "a" };
            var seen = 0;
            editor.RegisterValueChangedCallback(evt => seen++);

            yield return ChatTestUtils.Mount(m_TestUI, editor);

            editor.value = "a";
            Assert.AreEqual(0, seen);
        }

        [UnityTest]
        public IEnumerator SetValueWithoutNotify_Notifies_Nobody()
        {
            var editor = new CodeEditor();
            var seen = 0;
            editor.RegisterValueChangedCallback(evt => seen++);

            yield return ChatTestUtils.Mount(m_TestUI, editor);

            editor.SetValueWithoutNotify("a");
            Assert.AreEqual("a", editor.value);
            Assert.AreEqual(0, seen);
        }

        [Test]
        public void NullValue_BecomesEmptyRatherThanNull()
        {
            var editor = new CodeEditor { value = "a" };
            editor.SetValueWithoutNotify(null);
            Assert.AreEqual(string.Empty, editor.value);
        }

        // -------------------------------------------------------------------
        // Language
        // -------------------------------------------------------------------

        [Test]
        public void Language_AddsTheModifierClassAndRemovesTheStaleOne()
        {
            var editor = new CodeEditor { language = "lua" };
            Assert.IsTrue(editor.ClassListContains("appui-code-editor--lang-lua"));

            editor.language = "csharp";
            Assert.IsFalse(editor.ClassListContains("appui-code-editor--lang-lua"),
                "The previous language's class must be removed, not accumulated.");
            Assert.IsTrue(editor.ClassListContains("appui-code-editor--lang-csharp"));

            editor.language = null;
            Assert.IsFalse(editor.ClassListContains("appui-code-editor--lang-csharp"));
        }

        /// <summary>
        /// The class the component emits and the class the stylesheet maps to a grammar are two
        /// independent strings. If they drift, the language silently resolves no grammar and the editor
        /// degrades to plain text with no error at all.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void EveryLanguageTheStylesheetMaps_MatchesTheClassTheComponentEmits()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            foreach (var language in new[] { "csharp", "lua", "css", "json", "xml", "html", "hlsl", "wgsl" })
            {
                var editor = new CodeEditor { language = language };
                var emitted = CodeEditor.languageVariantUssClassNamePrefix + language;
                Assert.IsTrue(editor.ClassListContains(emitted));
                StringAssert.Contains("." + emitted, uss,
                    $"CodeEditor.uss maps no grammar onto '{emitted}'.");
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void TheStylesheetResolvesTheSameCustomPropertiesCodeBlockDoes()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            StringAssert.Contains("--codeblock-grammar", uss);
            StringAssert.Contains("--codeblock-theme", uss);
        }

        // -------------------------------------------------------------------
        // Delegation
        // -------------------------------------------------------------------

        [Test]
        public void PlaceholderAndReadOnly_ReachTheInnerTextArea()
        {
            var editor = new CodeEditor { placeholder = "code", isReadOnly = true };
            Assert.AreEqual("code", editor.textArea.placeholder);
            Assert.IsTrue(editor.textArea.isReadOnly);
        }

        [Test]
        public void TheHighlightLayerSitsOverTheInput()
        {
            var editor = new CodeEditor();
            var children = editor.hierarchy.Children();
            var order = new List<string>();
            foreach (var child in children)
                order.Add(child is TextArea ? "input" : "highlight");

            CollectionAssert.AreEqual(new[] { "input", "highlight" }, order,
                "The coloured copy has to paint over the input, or the TextArea's own field background hides it.");
        }

        [Test]
        public void TheHighlightLayerIgnoresPickingSoTheInputKeepsTheCaret()
        {
            var editor = new CodeEditor();
            var highlight = editor.Q(className: CodeEditor.highlightUssClassName);
            Assert.IsNotNull(highlight);
            Assert.AreEqual(PickingMode.Ignore, highlight.pickingMode);
        }

        [Test]
        public void WithNoHighlighting_TheOverlayIsHiddenAndCarriesNoText()
        {
            var editor = new CodeEditor { value = "print(1)" };
            var highlight = (TextElement)editor.Q(className: CodeEditor.highlightUssClassName);
            Assert.AreEqual(string.Empty, highlight.text);
            Assert.IsTrue(highlight.ClassListContains(Styles.hiddenUssClassName),
                "An empty overlay must not take part in layout.");
        }

        // -------------------------------------------------------------------
        // Indenting
        // -------------------------------------------------------------------

        [Test]
        public void InsertIndent_InsertsIndentSizeSpaces()
        {
            var editor = new CodeEditor { value = "ab" };
            editor.InsertIndent();
            StringAssert.Contains("  ", editor.value);
            Assert.AreEqual("ab".Length + CodeEditor.defaultIndentSize, editor.value.Length);
        }

        [Test]
        public void InsertIndent_HonoursIndentSize()
        {
            var editor = new CodeEditor { indentSize = 4, value = string.Empty };
            editor.InsertIndent();
            Assert.AreEqual("    ", editor.value);
        }

        [Test]
        public void InsertIndent_DoesNothingWhenIndentingIsTurnedOff()
        {
            var editor = new CodeEditor { indentSize = 0, value = "ab" };
            editor.InsertIndent();
            Assert.AreEqual("ab", editor.value);
        }

        /// <summary>
        /// A read-only editor must never rewrite its own source. The registry hit the same thing: its
        /// `setRangeText` fallback happily edits a read-only textarea, so the guard is on the edit, not
        /// on the key.
        /// </summary>
        [Test]
        public void InsertIndent_RefusesToEditAReadOnlyEditor()
        {
            var editor = new CodeEditor { isReadOnly = true, value = "ab" };
            editor.InsertIndent();
            Assert.AreEqual("ab", editor.value);
        }

        [UnityTest]
        public IEnumerator InsertIndent_ReportsTheEditThroughOneChangeEvent()
        {
            var editor = new CodeEditor { value = "ab" };
            var seen = 0;
            editor.RegisterValueChangedCallback(evt => seen++);

            yield return ChatTestUtils.Mount(m_TestUI, editor);

            editor.InsertIndent();
            Assert.AreEqual(1, seen);
        }

        static void Key(VisualElement target, KeyCode code)
        {
            using var evt = KeyDownEvent.GetPooled('\0', code, EventModifiers.None);
            evt.target = target;
            target.SendEvent(evt);
        }

        /// <summary>
        /// The whole point of the key handler. TextArea claims Tab for focus navigation, so the editor
        /// has to intercept it on the way down; asserting only on <see cref="CodeEditor.InsertIndent"/>
        /// would leave that interception untested and a user unable to indent.
        /// </summary>
        [UnityTest]
        public IEnumerator Tab_IndentsInsteadOfMovingFocus()
        {
            var editor = new CodeEditor { value = "ab" };
            yield return ChatTestUtils.Mount(m_TestUI, editor);

            Key(editor.textArea, KeyCode.Tab);
            yield return null;

            Assert.AreEqual("ab".Length + CodeEditor.defaultIndentSize, editor.value.Length,
                "Tab must insert an indent rather than moving focus out of the editor.");
            StringAssert.Contains("  ", editor.value);
        }

        [UnityTest]
        public IEnumerator Tab_IsLeftAloneInAReadOnlyEditor()
        {
            var editor = new CodeEditor { isReadOnly = true, value = "ab" };
            yield return ChatTestUtils.Mount(m_TestUI, editor);

            Key(editor.textArea, KeyCode.Tab);
            yield return null;

            Assert.AreEqual("ab", editor.value,
                "A read-only viewer must not trap Tab, and must not rewrite its own source.");
        }

        // -------------------------------------------------------------------
        // Token contract and the alignment rule
        // -------------------------------------------------------------------

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Uss_CarriesNoLiteralColour()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Uss_CarriesNoMagicPixelValue()
        {
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        /// <summary>
        /// The overlay is only correct while both layers lay text out identically, and nothing at runtime
        /// can notice that they stopped. Pinning the shared rule at the source is the only defence:
        /// splitting it, or dropping a layer from its selector list, fails here.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void OneRuleStylesBothLayersSoTheirMetricsCannotDrift()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            var rule = Regex.Match(
                uss,
                @"^(?<selectors>\.appui-code-editor__highlight,[^{]*)\{(?<body>[^}]*)\}",
                RegexOptions.Multiline);
            Assert.IsTrue(rule.Success, "No shared rule starting at .appui-code-editor__highlight.");

            var selectors = rule.Groups["selectors"].Value;
            StringAssert.Contains(".appui-code-editor__highlight", selectors);
            StringAssert.Contains(".appui-textarea__input", selectors);
            StringAssert.Contains(".appui-textarea__placeholder", selectors);

            var body = rule.Groups["body"].Value;
            foreach (var declaration in new[] { "font-size", "-unity-font-definition", "padding", "white-space" })
                StringAssert.Contains(declaration, body,
                    $"Every metric that can misalign the two layers belongs in the shared rule; '{declaration}' is missing.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void TheSharedRule_UsesTokensForTheMetricsItPins()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            var body = ChatTestUtils.RuleBody(uss, @"^\.appui-code-editor__highlight,[^{]*");
            StringAssert.Contains("var(--appui-font-sizes-", body);
            StringAssert.Contains("var(--appui-static-sizing-", body);
        }
    }
}
