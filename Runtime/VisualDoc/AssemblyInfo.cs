using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

[assembly: InternalsVisibleTo("Unity.AppUI.Editor")]
[assembly: InternalsVisibleTo("Unity.AppUI.Tests")]
#if UNITY_EDITOR
[assembly: UxmlNamespacePrefix("Unity.AppUI.VisualDoc", "visualdoc")]
#endif
