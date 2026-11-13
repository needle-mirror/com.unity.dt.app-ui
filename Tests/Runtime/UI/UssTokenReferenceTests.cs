using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Guards the stylesheets against <c>var(--appui-*)</c> references to tokens that no stylesheet
    /// defines.
    /// </summary>
    /// <remarks>
    /// An undefined custom property does not error: a <c>var()</c> without a fallback silently
    /// resolves to nothing. A border loses its
    /// colour, a tick loses its width, and the component reads like a design problem rather than a
    /// bug. Rendered output hides it, so only the source can catch it.
    /// </remarks>
    [TestFixture]
    class UssTokenReferenceTests
    {
        /// <summary>
        /// Undefined <c>--appui-*</c> references that already existed on <c>develop</c> and are
        /// tolerated as known debt. Every name here is a bug to fix, not a pass; remove an entry
        /// once its stylesheet is fixed, and never add one to make a new reference pass.
        /// </summary>
        /// <remarks>
        /// Empty on purpose. The thirteen names an earlier scan flagged on develop were all scanner
        /// artefacts: four were truncated at <c>__</c> (<c>--appui-checkbox__box-size-md</c> and
        /// the like, all defined), and nine only occur inside <c>/* ... */</c> comments that keep
        /// the original CSS next to its USS translation.
        /// </remarks>
        static readonly HashSet<string> k_KnownUndefinedTokenDebt = new HashSet<string>();

        // A custom property name may contain underscores (BEM element vars such as
        // --appui-checkbox__box-size-md), so the class must include `_`.
        static readonly Regex k_Definition = new Regex(@"(--appui-[A-Za-z0-9_-]+)\s*:");

        // A reference with a fallback, var(--x, ...), still resolves when --x is missing.
        static readonly Regex k_Reference = new Regex(@"var\(\s*(--appui-[A-Za-z0-9_-]+)\s*(,)?");

        static readonly Regex k_Comment = new Regex(@"/\*.*?\*/", RegexOptions.Singleline);

        static IEnumerable<string> Stylesheets(string root) =>
            Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                .Where(p => p.EndsWith(".uss") || p.EndsWith(".tss"));

        static string ReadWithoutComments(string path) => k_Comment.Replace(File.ReadAllText(path), "");

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads package source files, Editor only")]
        public void EveryAppUiVarReference_IsDefined_ExceptKnownDevelopDebt()
        {
            var packageResources = UssSource.PackageResourcesDirectory();

            // Definitions come from every stylesheet the package ships, Icons included: the
            // --appui-icon-* vars are declared in Icons/*.uss, not under Styles.
            var defined = new HashSet<string>();
            foreach (var path in Stylesheets(packageResources))
            {
                foreach (Match m in k_Definition.Matches(ReadWithoutComments(path)))
                    defined.Add(m.Groups[1].Value);
            }

            var stylesDir = Path.Combine(packageResources, "Styles");
            var undefined = new SortedDictionary<string, SortedSet<string>>();
            foreach (var path in Stylesheets(stylesDir))
            {
                foreach (Match m in k_Reference.Matches(ReadWithoutComments(path)))
                {
                    if (m.Groups[2].Success)
                        continue;
                    var name = m.Groups[1].Value;
                    if (defined.Contains(name) || k_KnownUndefinedTokenDebt.Contains(name))
                        continue;
                    if (!undefined.TryGetValue(name, out var files))
                        undefined[name] = files = new SortedSet<string>();
                    files.Add(Path.GetFileName(path));
                }
            }

            Assert.IsEmpty(undefined,
                "These var(--appui-*) references have no fallback and no USS/TSS defines them, so the "
                + "property silently resolves to nothing. Use a token that exists in the generated theme "
                + "USS:\n"
                + string.Join("\n", undefined.Select(kv => $"  {kv.Key} in {string.Join(", ", kv.Value)}")));

            var staleDebt = k_KnownUndefinedTokenDebt.Where(defined.Contains).ToList();
            Assert.IsEmpty(staleDebt,
                "These known-debt names are now defined; remove them from k_KnownUndefinedTokenDebt: "
                + string.Join(", ", staleDebt));
        }
    }
}
