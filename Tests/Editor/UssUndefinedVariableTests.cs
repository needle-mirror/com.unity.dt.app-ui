using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Unity.AppUI.Editor.Tests
{
    /// <summary>
    /// Fails when a package USS file reads <c>var(--appui-x)</c> with no fallback while no package USS
    /// defines <c>--appui-x</c>. Such a read resolves to nothing at runtime and only shows up as a
    /// console warning, or as an element that silently stops painting.
    /// </summary>
    [TestFixture]
    class UssUndefinedVariableTests
    {
        const string k_PackagePath = "Packages/com.unity.dt.app-ui";

        static readonly Regex k_Comment = new Regex(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex k_Definition = new Regex(@"(--appui-[\w-]+)\s*:", RegexOptions.Compiled);
        static readonly Regex k_ReadWithoutFallback = new Regex(@"var\(\s*(--appui-[\w-]+)\s*\)", RegexOptions.Compiled);

        // Plugins~ holds agent-skill examples of a consumer theme, which define their own palette.
        static bool IsExampleFile(string path) =>
            path.Replace('\\', '/').Contains("/Plugins~/");

        [Test]
        public void EveryAppUIVariableReadWithoutFallback_IsDefinedInThePackage()
        {
            // Resolve through the Package Manager: projects such as DevProject6 reference the
            // package with a file: dependency, so Packages/<name> is not a physical directory.
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(k_PackagePath);
            Assert.IsNotNull(info, $"Package Manager does not know {k_PackagePath}.");
            var root = info.resolvedPath;
            Assert.IsTrue(Directory.Exists(root), $"Package not found at {root}");

            var files = Directory.GetFiles(root, "*.uss", SearchOption.AllDirectories)
                .Where(f => !IsExampleFile(f));
            var defined = new HashSet<string>(StringComparer.Ordinal);
            var reads = new List<(string file, string name)>();

            foreach (var file in files)
            {
                var text = k_Comment.Replace(File.ReadAllText(file), string.Empty);
                foreach (Match m in k_Definition.Matches(text))
                    defined.Add(m.Groups[1].Value);

                foreach (Match m in k_ReadWithoutFallback.Matches(text))
                    reads.Add((Path.GetRelativePath(root, file), m.Groups[1].Value));
            }

            var offenders = reads
                .Where(r => !defined.Contains(r.name))
                .GroupBy(r => r.name)
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => $"{g.Key}: {string.Join(", ", g.Select(r => r.file).Distinct().OrderBy(f => f, StringComparer.Ordinal))}")
                .ToList();

            Assert.IsEmpty(offenders,
                $"{offenders.Count} USS variable(s) are read without a fallback but defined nowhere in the package:\n"
                + string.Join("\n", offenders));
        }
    }
}
