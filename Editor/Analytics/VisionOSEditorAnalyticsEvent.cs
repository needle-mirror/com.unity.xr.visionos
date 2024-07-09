// Add DEBUG_VISIONOS_ANALYTICS to the scripting defines to debug analytics events, you can also use the Analytics Debugger window (Unity 2023.1+)
#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
using Unity.XR.CoreUtils.Editor.Analytics;
using UnityEngine.Analytics;

#if DEBUG_VISIONOS_ANALYTICS
using UnityEngine;
#endif

namespace UnityEditor.XR.VisionOS.Analytics
{
    abstract class VisionOSEditorAnalyticsEvent<T> : EditorAnalyticsEvent<T> where T : struct
#if UNITY_2023_2_OR_NEWER
        , IAnalytic.IData
#endif
    {
        protected const int k_MaxEventPerHour = 1000;
        protected const int k_MaxItems = 1000;

        protected override AnalyticsResult SendToAnalyticsServer(T parameter)
        {
#if UNITY_2023_2_OR_NEWER
            var result = EditorAnalytics.SendAnalytic(this);
#else
            var result = EditorAnalytics.SendEventWithLimit(EventName, parameter, EventVersion);
#endif

#if DEBUG_VISIONOS_ANALYTICS
            Debug.Log($"[{GetType().Name}] parameter {JsonUtility.ToJson(parameter)} sent with status {result}.");
#endif
            return result;
        }

        protected override AnalyticsResult RegisterWithAnalyticsServer() =>
#if UNITY_2023_2_OR_NEWER
            AnalyticsResult.Ok;
#else
            EditorAnalytics.RegisterEventWithLimit(EventName, k_MaxEventPerHour, k_MaxItems, VisionOSAnalytics.VendorKey, EventVersion);
#endif

#if !UNITY_2023_2_OR_NEWER
        protected VisionOSEditorAnalyticsEvent(string eventName, int eventVersion) : base(eventName, eventVersion)
        {
        }
#endif
    }
}
#endif //ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
