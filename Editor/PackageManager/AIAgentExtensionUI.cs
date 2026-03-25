using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Editor
{
    /// <summary>
    /// UI component displayed in the Package Manager detail pane.
    /// Shows skill installation controls when the selected package
    /// contains a <c>Plugins~/skills</c> folder.
    /// </summary>
    class AIAgentExtensionUI : VisualElement
    {
        const string k_SkillsSubPath = "Plugins~/skills";
        const string k_SkillsManifest = "Plugins~/skills.json";
        const string k_ClaudeDirName = ".claude";
        const string k_SkillsDirName = "skills";
        const string k_SkillManifest = "SKILL.md";

        readonly HelpBox m_Warning;
        readonly Button m_InstallAllButton;
        readonly Button m_RemoveAllButton;
        readonly VisualElement m_SkillList;

        string m_SourceSkillsPath;
        SkillDefinition[] m_SkillDefinitions;
        readonly List<SkillRow> m_SkillRows = new List<SkillRow>();
        readonly Label m_PathLabel;

        [Serializable]
        struct SkillDefinition
        {
            public string id;
            public string displayName;
            public string description;
        }

        [Serializable]
        struct SkillDefinitionArray
        {
            public SkillDefinition[] items;
        }

        /// <summary>
        /// Creates the AI Agent extension UI.
        /// </summary>
        public AIAgentExtensionUI()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                AssetDatabase.GUIDToAssetPath("d2fdab81e8e544fa926069812c829664")); // AIAgentExtensionUI.uss

            if (styleSheet)
                styleSheets.Add(styleSheet);

            AddToClassList("ai-agent-extension");

            var foldout = new Foldout { text = "AI Agent Skills" };
            foldout.AddToClassList("ai-agent-extension__foldout");
            Add(foldout);

            m_PathLabel = new Label();
            m_PathLabel.AddToClassList("ai-agent-extension__path-label");
            foldout.Add(m_PathLabel);

            m_Warning = new HelpBox(
                "Skills will be installed in your home directory (~/.claude/skills). " +
                "Consider creating a .claude folder in your project to keep skills project-scoped.",
                HelpBoxMessageType.Warning);
            m_Warning.AddToClassList("ai-agent-extension__warning");
            foldout.Add(m_Warning);

            m_InstallAllButton = new Button(OnInstallAllClicked)
            {
                text = "Install All",
            };
            m_InstallAllButton.AddToClassList("ai-agent-extension__install-all-button");

            m_RemoveAllButton = new Button(OnRemoveAllClicked)
            {
                text = "Remove All",
            };
            m_RemoveAllButton.AddToClassList("ai-agent-extension__remove-all-button");
            m_RemoveAllButton.style.display = DisplayStyle.None;

            foldout.Add(m_InstallAllButton);
            foldout.Add(m_RemoveAllButton);

            m_SkillList = new VisualElement { name = "samplesContainer" };
            m_SkillList.AddToClassList("ai-agent-extension__list");
            foldout.Add(m_SkillList);
        }

        /// <summary>
        /// Called when the user selects a different package in the Package Manager.
        /// </summary>
        /// <param name="packageInfo">The selected package info, or <c>null</c> if none selected.</param>
        public void OnPackageSelectionChange(UnityEditor.PackageManager.PackageInfo packageInfo)
        {
            m_SourceSkillsPath = null;
            m_SkillDefinitions = null;
            m_SkillRows.Clear();
            m_SkillList.Clear();

            if (packageInfo == null || string.IsNullOrEmpty(packageInfo.resolvedPath))
            {
                style.display = DisplayStyle.None;
                return;
            }

            var manifestPath = Path.Combine(packageInfo.resolvedPath, k_SkillsManifest);
            if (!File.Exists(manifestPath))
            {
                style.display = DisplayStyle.None;
                return;
            }

            var skillsDir = Path.Combine(packageInfo.resolvedPath, k_SkillsSubPath);
            if (!Directory.Exists(skillsDir))
            {
                style.display = DisplayStyle.None;
                return;
            }

            var definitions = ParseSkillsManifest(manifestPath);
            if (definitions == null || definitions.Length == 0)
            {
                style.display = DisplayStyle.None;
                return;
            }

            // Only keep definitions whose directory actually exists
            definitions = definitions.Where(d =>
                Directory.Exists(Path.Combine(skillsDir, d.id))).ToArray();

            if (definitions.Length == 0)
            {
                style.display = DisplayStyle.None;
                return;
            }

            m_SourceSkillsPath = skillsDir;
            m_SkillDefinitions = definitions;

            style.display = DisplayStyle.Flex;
            BuildSkillRows();
            UpdateWarning();
        }

        static SkillDefinition[] ParseSkillsManifest(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                // JsonUtility needs a wrapper object for arrays
                var wrapped = "{\"items\":" + json + "}";
                return JsonUtility.FromJson<SkillDefinitionArray>(wrapped).items;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AI Agent Skills] Failed to parse skills manifest at '{path}': {ex.Message}");
                return null;
            }
        }

        void BuildSkillRows()
        {
            var targetDir = GetTargetSkillsDirectory();

            foreach (var def in m_SkillDefinitions)
            {
                var id = def.id;
                var row = new SkillRow(def, () => InstallSkill(id), () => RemoveSkill(id));
                UpdateRowStatus(row, targetDir);
                m_SkillRows.Add(row);
                m_SkillList.Add(row);
            }

            UpdateInstallAllButton();
        }

        void UpdateWarning()
        {
            var targetDir = GetTargetSkillsDirectory();
            var claudeDir = Path.GetDirectoryName(targetDir); // .../.claude
            var isHome = IsHomeDirectory(Path.GetDirectoryName(claudeDir)); // .../
            m_PathLabel.text = $"Current Path: {targetDir}";
            m_Warning.style.display = isHome ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void UpdateRowStatus(SkillRow row, string targetDir)
        {
            var targetSkillDir = Path.Combine(targetDir, row.SkillId);

            if (!Directory.Exists(targetSkillDir))
            {
                row.SetStatus(SkillStatus.NotInstalled);
                return;
            }

            var sourceManifest = Path.Combine(m_SourceSkillsPath, row.SkillId, k_SkillManifest);
            var targetManifest = Path.Combine(targetSkillDir, k_SkillManifest);

            if (File.Exists(sourceManifest) && File.Exists(targetManifest))
            {
                var sourceTime = File.GetLastWriteTimeUtc(sourceManifest);
                var targetTime = File.GetLastWriteTimeUtc(targetManifest);
                if (sourceTime > targetTime)
                {
                    row.SetStatus(SkillStatus.Outdated);
                    return;
                }
            }

            row.SetStatus(SkillStatus.Installed);
        }

        void UpdateInstallAllButton()
        {
            var anyInstalled = m_SkillRows.Any(r => r.Status != SkillStatus.NotInstalled);

            if (m_SkillRows.All(r => r.Status == SkillStatus.Installed))
                m_InstallAllButton.text = "Reinstall All";
            else if (m_SkillRows.Any(r => r.Status == SkillStatus.Outdated))
                m_InstallAllButton.text = "Update All";
            else
                m_InstallAllButton.text = "Install All";

            m_RemoveAllButton.style.display = anyInstalled ? DisplayStyle.Flex : DisplayStyle.None;
        }

        static bool TryGetSafeChildDirectory(string baseDir, string childName, out string fullPath)
        {
            fullPath = null;
            if (string.IsNullOrEmpty(baseDir) || string.IsNullOrEmpty(childName))
                return false;

            // Disallow traversal/separators and invalid filename chars
            if (childName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                childName.Contains(Path.DirectorySeparatorChar) ||
                childName.Contains(Path.AltDirectorySeparatorChar) ||
                childName.Contains(".."))
                return false;

            var baseFull = Path.GetFullPath(baseDir)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            var candidateFull = Path.GetFullPath(Path.Combine(baseDir, childName));
            if (!candidateFull.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase))
                return false;

            fullPath = candidateFull;
            return true;
        }

        void InstallSkill(string skillName)
        {
            if (m_SourceSkillsPath == null)
                return;

            var targetDir = GetTargetSkillsDirectory();

            try
            {
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                if (!TryGetSafeChildDirectory(m_SourceSkillsPath, skillName, out var sourceDir) ||
                    !TryGetSafeChildDirectory(targetDir, skillName, out var targetSkillDir) ||
                    !Directory.Exists(sourceDir))
                    return;

                if (Directory.Exists(targetSkillDir))
                {
                    if (!DisplayDialog(
                        "AI Agent Skills",
                        $"The skill '{skillName}' is already installed. Overwriting will delete the existing directory and any local modifications.\n\nDo you want to continue?",
                        "Overwrite", "Cancel"))
                        return;

                    Directory.Delete(targetSkillDir, true);
                }

                CopyDirectoryRecursive(sourceDir, targetSkillDir);

                var row = m_SkillRows.FirstOrDefault(r => r.SkillId == skillName);
                if (row != null)
                    UpdateRowStatus(row, targetDir);

                UpdateInstallAllButton();
                UpdateWarning();
            }
            catch (Exception ex)
            {
                DisplayDialog(
                    "AI Agent Skills",
                    $"Failed to install skill '{skillName}':\n{ex.Message}",
                    "OK");
            }
        }

        void RemoveSkill(string skillId)
        {
            var targetDir = GetTargetSkillsDirectory();

            if (!TryGetSafeChildDirectory(targetDir, skillId, out var targetSkillDir) ||
                !Directory.Exists(targetSkillDir))
                return;

            if (!DisplayDialog(
                "AI Agent Skills",
                $"Are you sure you want to remove the skill '{skillId}'? This will delete the entire skill directory and any local modifications.",
                "Remove", "Cancel"))
                return;

            try
            {
                Directory.Delete(targetSkillDir, true);

                var row = m_SkillRows.FirstOrDefault(r => r.SkillId == skillId);
                if (row != null)
                    UpdateRowStatus(row, targetDir);

                UpdateInstallAllButton();
                UpdateWarning();
            }
            catch (Exception ex)
            {
                DisplayDialog(
                    "AI Agent Skills",
                    $"Failed to remove skill '{skillId}':\n{ex.Message}",
                    "OK");
            }
        }

        void OnInstallAllClicked()
        {
            if (m_SourceSkillsPath == null || m_SkillDefinitions == null)
                return;

            var targetDir = GetTargetSkillsDirectory();

            var existingSkills = m_SkillDefinitions
                .Where(def => TryGetSafeChildDirectory(targetDir, def.id, out var p) && Directory.Exists(p))
                .Select(def => def.id)
                .ToArray();

            if (existingSkills.Length > 0)
            {
                if (!DisplayDialog(
                    "AI Agent Skills",
                    $"The following skills are already installed and will be overwritten:\n{string.Join(", ", existingSkills)}\n\nAny local modifications will be lost. Do you want to continue?",
                    "Overwrite All", "Cancel"))
                    return;
            }

            try
            {
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                foreach (var def in m_SkillDefinitions)
                {
                    if (!TryGetSafeChildDirectory(m_SourceSkillsPath, def.id, out var sourceDir) ||
                        !TryGetSafeChildDirectory(targetDir, def.id, out var targetSkillDir) ||
                        !Directory.Exists(sourceDir))
                        continue;

                    if (Directory.Exists(targetSkillDir))
                        Directory.Delete(targetSkillDir, true);

                    CopyDirectoryRecursive(sourceDir, targetSkillDir);
                }

                foreach (var row in m_SkillRows)
                    UpdateRowStatus(row, targetDir);

                UpdateInstallAllButton();
                UpdateWarning();
            }
            catch (Exception ex)
            {
                DisplayDialog(
                    "AI Agent Skills",
                    $"Failed to install skills:\n{ex.Message}",
                    "OK");
            }
        }

        void OnRemoveAllClicked()
        {
            if (m_SkillDefinitions == null)
                return;

            var targetDir = GetTargetSkillsDirectory();

            var installedSkills = m_SkillDefinitions
                .Where(def => TryGetSafeChildDirectory(targetDir, def.id, out var p) && Directory.Exists(p))
                .Select(def => def.id)
                .ToArray();

            if (installedSkills.Length == 0)
                return;

            if (!DisplayDialog(
                "AI Agent Skills",
                $"Are you sure you want to remove all installed skills?\n{string.Join(", ", installedSkills)}\n\nThis will delete the skill directories and any local modifications.",
                "Remove All", "Cancel"))
                return;

            try
            {
                foreach (var def in m_SkillDefinitions)
                {
                    if (!TryGetSafeChildDirectory(targetDir, def.id, out var targetSkillDir))
                        continue;

                    if (Directory.Exists(targetSkillDir))
                        Directory.Delete(targetSkillDir, true);
                }

                foreach (var row in m_SkillRows)
                    UpdateRowStatus(row, targetDir);

                UpdateInstallAllButton();
                UpdateWarning();
            }
            catch (Exception ex)
            {
                DisplayDialog(
                    "AI Agent Skills",
                    $"Failed to remove skills:\n{ex.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// Returns the target skills directory path.
        /// Walks up from the project root looking for an existing <c>.claude</c> folder.
        /// If none found, defaults to <c>&lt;project-root&gt;/.claude/skills</c>.
        /// </summary>
        string GetTargetSkillsDirectory()
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var claudeDir = FindClosestClaudeDirectory(projectRoot);

            if (claudeDir == null)
                claudeDir = Path.Combine(projectRoot, k_ClaudeDirName);

            return Path.Combine(claudeDir, k_SkillsDirName);
        }

        static string FindClosestClaudeDirectory(string startDir)
        {
            var current = startDir;
            while (!string.IsNullOrEmpty(current))
            {
                var candidate = Path.Combine(current, k_ClaudeDirName);
                if (Directory.Exists(candidate))
                    return candidate;

                var parent = Path.GetDirectoryName(current);
                if (parent == current)
                    break;
                current = parent;
            }

            return null;
        }

        static bool IsHomeDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(home))
                return false;

            return string.Equals(
                Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                Path.GetFullPath(home).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }

        static void CopyDirectoryRecursive(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var destDir = Path.Combine(targetDir, Path.GetFileName(dir));
                CopyDirectoryRecursive(dir, destDir);
            }
        }

        static void DisplayDialog(string title, string message, string ok)
        {
#if ENABLE_ENTITY_ID
            EditorDialog.DisplayAlertDialog(title, message, ok, DialogIconType.Error);
#else
            EditorUtility.DisplayDialog(title, message, ok);
#endif
        }

        static bool DisplayDialog(string title, string message, string ok, string cancel)
        {
#if ENABLE_ENTITY_ID
            return EditorDialog.DisplayDecisionDialog(title, message, ok, cancel, DialogIconType.Warning);
#else
            return EditorUtility.DisplayDialog(title, message, ok, cancel);
#endif
        }

        enum SkillStatus
        {
            NotInstalled,
            Installed,
            Outdated,
        }

        /// <summary>
        /// A single row representing one skill: display name, description, status, and action button.
        /// </summary>
        class SkillRow : VisualElement
        {
            readonly Label m_NameLabel;
            readonly Label m_DescriptionLabel;
            readonly Label m_StatusLabel;
            readonly Button m_ActionButton;
            readonly Button m_RemoveButton;

            public string SkillId { get; }

            public SkillStatus Status { get; private set; }

            public SkillRow(SkillDefinition definition, Action onInstall, Action onRemove)
            {
                SkillId = definition.id;

                AddToClassList("ai-agent-extension__row");

                var infoContainer = new VisualElement();
                infoContainer.AddToClassList("ai-agent-extension__skill-info");

                var headerContainer = new VisualElement();
                headerContainer.AddToClassList("ai-agent-extension__skill-info-header");
                infoContainer.Add(headerContainer);

                m_NameLabel = new Label(definition.displayName);
                m_NameLabel.AddToClassList("ai-agent-extension__skill-name");
                headerContainer.Add(m_NameLabel);

                // Status is conveyed via USS class toggling ("imported") rather than text content.
                m_StatusLabel = new Label();
                m_StatusLabel.AddToClassList("ai-agent-extension__skill-status");
                m_StatusLabel.AddToClassList("importStatus");
                headerContainer.Add(m_StatusLabel);

                if (!string.IsNullOrEmpty(definition.description))
                {
                    m_DescriptionLabel = new Label(definition.description);
                    m_DescriptionLabel.AddToClassList("ai-agent-extension__skill-description");
                    infoContainer.Add(m_DescriptionLabel);
                }

                Add(infoContainer);

                var expander = new VisualElement();
                expander.AddToClassList("ai-agent-extension__row-expander");
                Add(expander);

                m_ActionButton = new Button(() => onInstall?.Invoke());
                m_ActionButton.AddToClassList("ai-agent-extension__skill-action-button");
                Add(m_ActionButton);

                m_RemoveButton = new Button(() => onRemove?.Invoke())
                {
                    text = "Remove",
                };
                m_RemoveButton.AddToClassList("ai-agent-extension__skill-remove-button");
                Add(m_RemoveButton);
            }

            public void SetStatus(SkillStatus status)
            {
                Status = status;
                var installed = status != SkillStatus.NotInstalled;
                m_StatusLabel.EnableInClassList("imported", installed);
                m_RemoveButton.SetEnabled(installed);
                m_RemoveButton.style.display = installed ? DisplayStyle.Flex : DisplayStyle.None;

                switch (status)
                {
                    case SkillStatus.NotInstalled:
                        m_ActionButton.text = "Install";
                        break;
                    case SkillStatus.Installed:
                        m_ActionButton.text = "Reinstall";
                        break;
                    case SkillStatus.Outdated:
                        m_ActionButton.text = "Update";
                        break;
                }
            }
        }
    }
}
