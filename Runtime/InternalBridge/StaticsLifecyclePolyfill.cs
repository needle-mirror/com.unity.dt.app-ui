#if !ENABLE_STATICS_CLEANUP_ATTRIBUTES
// From 6000.5.0a8, Unity ships Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanupAttribute
// publicly in Unity.Scripting.dll, and its statics analyzer (UAL0010, UAL0013) asks for it on every
// type that holds static state when domain reload is off. Earlier editors either lack it or keep it
// internal (6000.3 and 6000.4 carry an internal copy), so this stand-in lets
// App UI write [NoAutoStaticsCleanup] once and compile on every supported version. The
// ENABLE_STATICS_CLEANUP_ATTRIBUTES version define in this assembly's asmdef turns it off where
// Unity's own attribute is public. [Conditional] keeps it out of compiled metadata.
namespace Unity.Scripting.LifecycleManagement
{
    [System.Diagnostics.Conditional("APPUI_NEVER_DEFINED")]
    [System.AttributeUsage(
        System.AttributeTargets.Class |
        System.AttributeTargets.Struct |
        System.AttributeTargets.Field |
        System.AttributeTargets.Property |
        System.AttributeTargets.Event)]
    sealed class NoAutoStaticsCleanupAttribute : System.Attribute { }
}
#endif
