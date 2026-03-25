using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Editor
{
    /// <summary>
    /// Package Manager extension that shows an "AI Agent" section
    /// for packages containing a <c>Plugins~/skills</c> folder.
    /// </summary>
    [InitializeOnLoad]
    class AIAgentExtension : IPackageManagerExtension
    {
        static AIAgentExtension()
        {
            PackageManagerExtensions.RegisterExtension(new AIAgentExtension());
        }

        AIAgentExtensionUI m_UI;

        /// <inheritdoc/>
        public VisualElement CreateExtensionUI()
        {
            m_UI = new AIAgentExtensionUI();
            return m_UI;
        }

        /// <inheritdoc/>
        public void OnPackageSelectionChange(UnityEditor.PackageManager.PackageInfo packageInfo)
        {
            m_UI?.OnPackageSelectionChange(packageInfo);
        }

        /// <inheritdoc/>
        public void OnPackageAddedOrUpdated(UnityEditor.PackageManager.PackageInfo packageInfo) { }

        /// <inheritdoc/>
        public void OnPackageRemoved(UnityEditor.PackageManager.PackageInfo packageInfo) { }
    }
}
