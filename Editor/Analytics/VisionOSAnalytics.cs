#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER

namespace UnityEditor.XR.VisionOS.Analytics
{
    /// <summary>
    /// The entry point class to send XR VisionOS analytics data.
    /// </summary>
#if !UNITY_2023_2_OR_NEWER
    [InitializeOnLoad]
#endif
    static class VisionOSAnalytics
    {
        internal const string VendorKey = "unity.xr.visionos";

#if UNITY_2023_2_OR_NEWER
        internal const string PackageName = "com.unity.xr.visionos";
        internal static readonly string PackageVersion = PackageManager.PackageInfo.FindForPackageName(PackageName).version;
#endif

        internal static VisionOSPlayModeUsageEvent PlayModeUsageEvent { get; } = new();

#if !UNITY_2023_2_OR_NEWER
        static VisionOSAnalytics()
        {
            PlayModeUsageEvent.Register();
        }
#endif
    }
}
#endif //ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
