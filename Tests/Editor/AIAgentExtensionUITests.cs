using System.IO;
using NUnit.Framework;
using Unity.AppUI.Editor;

namespace Unity.AppUI.Tests.Editor
{
    [TestFixture]
    class AIAgentExtensionUITests
    {
        string m_Root;

        [SetUp]
        public void SetUp()
        {
            m_Root = Path.Combine(Path.GetTempPath(), "appui-skills-" + Path.GetRandomFileName());
            Directory.CreateDirectory(m_Root);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(m_Root))
                Directory.Delete(m_Root, true);
        }

        [Test]
        public void FindClosestClaudeDirectory_finds_dir_at_start()
        {
            var project = Path.Combine(m_Root, "project");
            var claude = Path.Combine(project, ".claude");
            Directory.CreateDirectory(claude);

            var found = AIAgentExtensionUI.FindClosestClaudeDirectory(project, project);

            Assert.AreEqual(Path.GetFullPath(claude), Path.GetFullPath(found));
        }

        [Test]
        public void FindClosestClaudeDirectory_returns_null_when_absent()
        {
            var project = Path.Combine(m_Root, "project");
            Directory.CreateDirectory(project);

            Assert.IsNull(AIAgentExtensionUI.FindClosestClaudeDirectory(project, project));
        }

        [Test]
        public void FindClosestClaudeDirectory_does_not_walk_above_stop_dir()
        {
            // Regression guard: the walk used to continue to the filesystem root, so a
            // .claude directory anywhere above the project (typically ~/.claude) was
            // adopted as the install target, putting writes and recursive deletes
            // outside the project.
            var outside = Path.Combine(m_Root, ".claude");
            Directory.CreateDirectory(outside);

            var project = Path.Combine(m_Root, "project");
            Directory.CreateDirectory(project);

            var found = AIAgentExtensionUI.FindClosestClaudeDirectory(project, project);

            Assert.IsNull(found, "search escaped the project boundary");
        }

        [Test]
        public void FindClosestClaudeDirectory_rejects_the_home_claude_directory()
        {
            // Regression guard: the home check used to run *after* an existing candidate
            // was returned, so a project rooted directly in $HOME still selected the
            // shared ~/.claude — which install/remove then wrote to and deleted from.
            var home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(home) || !Directory.Exists(Path.Combine(home, ".claude")))
                Assert.Ignore("requires an existing ~/.claude to be meaningful");

            var found = AIAgentExtensionUI.FindClosestClaudeDirectory(home, home);

            Assert.IsNull(found, "resolved to the shared home .claude directory");
        }

        [Test]
        public void FindClosestClaudeDirectory_handles_null_and_empty()
        {
            Assert.IsNull(AIAgentExtensionUI.FindClosestClaudeDirectory(null, null));
            Assert.IsNull(AIAgentExtensionUI.FindClosestClaudeDirectory(string.Empty, string.Empty));
        }
    }
}
