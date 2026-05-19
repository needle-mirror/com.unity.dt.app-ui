using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(CodeBlock))]
    class CodeBlockTests : VisualElementTests<CodeBlock>
    {
        protected override string mainUssClassName => CodeBlock.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:CodeBlock />",
            $@"<appui:CodeBlock class=""appui-code-block--lang-cs"" content=""var x = 1;"" show-line-numbers=""true"" />",
        };

        [Test]
        public void Code_DefaultIsEmpty()
        {
            var cb = new CodeBlock();
            Assert.AreEqual(string.Empty, cb.code);
        }

        [Test]
        public void Code_CanSetAndGet()
        {
            var cb = new CodeBlock();
            cb.code = "var x = 1;";
            Assert.AreEqual("var x = 1;", cb.code);
        }

        [Test]
        public void Code_NullBecomesEmpty()
        {
            var cb = new CodeBlock();
            cb.code = "something";
            cb.code = null;
            Assert.AreEqual(string.Empty, cb.code);
        }

        [Test]
        public void Language_DefaultIsNull()
        {
            var cb = new CodeBlock();
            Assert.IsNull(cb.language);
        }

        [Test]
        public void Language_CanSetAndGet()
        {
            var cb = new CodeBlock();
            cb.language = "csharp";
            Assert.AreEqual("csharp", cb.language);
        }

        [Test]
        public void Language_AddsVariantClass()
        {
            var cb = new CodeBlock();
            cb.language = "lua";
            Assert.IsTrue(cb.ClassListContains(CodeBlock.languageVariantUssClassNamePrefix + "lua"));
        }

        [Test]
        public void Language_RemovesPreviousVariantClass()
        {
            var cb = new CodeBlock();
            cb.language = "lua";
            cb.language = "css";
            Assert.IsFalse(cb.ClassListContains(CodeBlock.languageVariantUssClassNamePrefix + "lua"));
            Assert.IsTrue(cb.ClassListContains(CodeBlock.languageVariantUssClassNamePrefix + "css"));
        }

        [Test]
        public void Language_SetNullRemovesClass()
        {
            var cb = new CodeBlock();
            cb.language = "json";
            cb.language = null;
            Assert.IsFalse(cb.ClassListContains(CodeBlock.languageVariantUssClassNamePrefix + "json"));
        }

        [Test]
        public void ShowLineNumbers_DefaultIsFalse()
        {
            var cb = new CodeBlock();
            Assert.IsFalse(cb.showLineNumbers);
        }

        [Test]
        public void ShowLineNumbers_CanSetAndGet()
        {
            var cb = new CodeBlock();
            cb.showLineNumbers = true;
            Assert.IsTrue(cb.showLineNumbers);
        }

        [Test]
        public void ShowLineNumbers_TogglesUssClass()
        {
            var cb = new CodeBlock();
            cb.showLineNumbers = true;
            Assert.IsTrue(cb.ClassListContains(CodeBlock.withLineNumbersUssClassName));
            cb.showLineNumbers = false;
            Assert.IsFalse(cb.ClassListContains(CodeBlock.withLineNumbersUssClassName));
        }

        [Test]
        public void ContentContainer_IsNull()
        {
            var cb = new CodeBlock();
            Assert.IsNull(cb.contentContainer);
        }

        [Test]
        public void Anatomy_HasExpectedParts()
        {
            var cb = new CodeBlock();
            var header = cb.hierarchy.Children()
                .FirstOrDefault(c => c.ClassListContains(CodeBlock.headerUssClassName));
            Assert.IsNotNull(header, "header");

            var langLabel = header.Children()
                .FirstOrDefault(c => c.ClassListContains(CodeBlock.languageUssClassName));
            Assert.IsNotNull(langLabel, "language label");

            var spacer = header.Children()
                .FirstOrDefault(c => c.ClassListContains(CodeBlock.headerSpacerUssClassName));
            Assert.IsNotNull(spacer, "header spacer");

            var copyBtn = header.Children()
                .FirstOrDefault(c => c.ClassListContains(CodeBlock.copyButtonUssClassName));
            Assert.IsNotNull(copyBtn, "copy button");

            var scroll = cb.hierarchy.Children()
                .FirstOrDefault(c => c.ClassListContains(CodeBlock.scrollUssClassName));
            Assert.IsNotNull(scroll, "scroll view");
        }

        [Test]
        public void Language_LabelHiddenWhenNull()
        {
            var cb = new CodeBlock();
            var header = cb.hierarchy.Children()
                .First(c => c.ClassListContains(CodeBlock.headerUssClassName));
            var langLabel = header.Children()
                .First(c => c.ClassListContains(CodeBlock.languageUssClassName));

            Assert.AreEqual(DisplayStyle.None, langLabel.style.display.value);
        }

        [Test]
        public void Language_LabelShownWhenSet()
        {
            var cb = new CodeBlock();
            cb.language = "csharp";

            var header = cb.hierarchy.Children()
                .First(c => c.ClassListContains(CodeBlock.headerUssClassName));
            var langLabel = header.Children()
                .First(c => c.ClassListContains(CodeBlock.languageUssClassName));

            Assert.AreEqual(DisplayStyle.Flex, langLabel.style.display.value);
        }
    }
}
