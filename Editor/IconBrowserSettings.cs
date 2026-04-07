using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Editor
{
    [FilePath("ProjectSettings/IconBrowser.asset", FilePathAttribute.Location.ProjectFolder)]
    class IconBrowserSettings : ScriptableSingleton<IconBrowserSettings>
    {
        [SerializeField]
        StyleSheet lastUsedStyleSheet;

        public StyleSheet LastUsedStyleSheet
        {
            get => lastUsedStyleSheet;
            set
            {
                if (lastUsedStyleSheet != value)
                {
                    lastUsedStyleSheet = value;
                    Save(true);
                }
            }
        }
    }
}
