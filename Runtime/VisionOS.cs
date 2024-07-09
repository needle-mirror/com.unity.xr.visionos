using System;
using UnityEngine;

#if UNITY_VISIONOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// Runtime scripting API for VisionOS.
    /// </summary>
    public static class VisionOS
    {
#if UNITY_VISIONOS && !UNITY_EDITOR
        const string k_LibraryName = "__Internal";

        /// <summary>
        /// Set the range of values used for depth sorting.
        /// These values should match Camera.nearClipPlane and Camera.farClipPlane
        /// </summary>
        /// <param name="near">The value for the near clipping plane.</param>
        /// <param name="far">The value for the far clipping plane.</param>
        [DllImport(k_LibraryName, EntryPoint = "SetDepthRange")]
        public static extern void SetDepthRange(float near, float far);

        /// <summary>
        /// Determine whether the immersive space for the app is ready.
        /// </summary>
        /// <returns><see langword="true"/> if the immersive space is ready. Otherwise, <see langword="false"/>.</returns>
        [DllImport(k_LibraryName, EntryPoint = "UnityVisionOS_IsImmersiveSpaceReady")]
        public static extern bool IsImmersiveSpaceReady();
#else
        /// <summary>
        /// Set the range of values used for depth sorting.
        /// These values should match Camera.nearClipPlane and Camera.farClipPlane
        /// </summary>
        /// <param name="near">The value for the near clipping plane.</param>
        /// <param name="far">The value for the far clipping plane.</param>
        public static void SetDepthRange(float near, float far) { }

        /// <summary>
        /// Determine whether the immersive space for the app is ready.
        /// Return true in the Editor for testing purposes.
        /// </summary>
        /// <returns><see langword="true"/> if the immersive space is ready (or in the Editor when targeting visionOS).
        /// Otherwise, <see langword="false"/>.</returns>
        public static bool IsImmersiveSpaceReady()
        {
#if UNITY_VISIONOS
            return true;
#else
            return false;
#endif
        }
#endif

        /// <summary>
        /// Determine whether the app is running in the visionOS simulator.
        /// Treat the Editor targeting visionOS as running in simulator.
        /// </summary>
        /// <returns><see langword="true"/> if the app is running in the visionOS Simulator.
        /// Otherwise, <see langword="false"/>.</returns>
        public static bool IsSimulator()
        {
#if UNITY_VISIONOS
#if UNITY_EDITOR
            return true;
#else
            return Environment.GetEnvironmentVariable("SIMULATOR_ROOT") != null;
#endif
#else
            return false;
#endif
        }

        /// <summary>
        /// Called when an ARKit authorization like hand tracking or world tracking changes status. Some examples of when this occurs:
        /// - On app start when initially querying the authorization status, if it was already requested
        /// - After querying authorization status, if it was not requested and the user allowed or declined
        /// - When the user changes authorizations in Settings and returns to the app
        /// </summary>
        public static event Action<VisionOSAuthorizationEventArgs> AuthorizationChanged;

        /// <summary>
        /// Query the current authorization status for a given authorization type.
        /// </summary>
        /// <param name="type">The authorization type to query.</param>
        /// <returns>The status of the queried authorization type.</returns>
        public static VisionOSAuthorizationStatus QueryAuthorizationStatus(VisionOSAuthorizationType type)
        {
            return VisionOSSessionSubsystem.VisionOSSessionProvider.QueryAuthorizationStatus(type);
        }

        internal static void OnAuthorizationChanged(VisionOSAuthorizationType type, VisionOSAuthorizationStatus status)
        {
            AuthorizationChanged?.Invoke(new VisionOSAuthorizationEventArgs { type = type, status = status });
        }
    }
}
