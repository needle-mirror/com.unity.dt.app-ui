#if UNITY_ANDROID
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEditor.Android;

class AppUIAndroidBuildProcessor : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 0;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        // The 'path' variable points to the root of the generated Gradle project.
        // We need to find the AndroidManifest.xml within the 'unityLibrary' module.
        var manifestPath = Path.Combine(path, "src/main/AndroidManifest.xml");

        if (!File.Exists(manifestPath))
        {
            Debug.LogError($"[AppUI] Could not find AndroidManifest.xml at {manifestPath}");
            return;
        }

        var doc = new XmlDocument();
        doc.Load(manifestPath);

        var androidNamespace = "http://schemas.android.com/apk/res/android";

        // Ensure the VIBRATE permission is added for haptics
        AddPermission(doc, androidNamespace, "android.permission.VIBRATE");

        doc.Save(manifestPath);
        Debug.Log("[AppUI] Manifest updated with required haptic permissions.");
    }

    void AddPermission(XmlDocument doc, string ns, string permissionName)
    {
        // Check if permission already exists to avoid duplicates
        var permissions = doc.SelectNodes($"/manifest/uses-permission[@android:name='{permissionName}']", GetNamespaceManager(doc, ns));

        if (permissions.Count == 0)
        {
            var element = doc.CreateElement("uses-permission");
            element.SetAttribute("name", ns, permissionName);
            doc.DocumentElement.PrependChild(element);
        }
    }

    XmlNamespaceManager GetNamespaceManager(XmlDocument doc, string ns)
    {
        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("android", ns);
        return nsmgr;
    }
}
#endif
