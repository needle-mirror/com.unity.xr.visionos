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
#else
        /// <summary>
        /// Set the range of values used for depth sorting.
        /// These values should match Camera.nearClipPlane and Camera.farClipPlane 
        /// </summary>
        /// <param name="near">The value for the near clipping plane.</param>
        /// <param name="far">The value for the far clipping plane.</param>
        public static void SetDepthRange(float near, float far) { }
#endif
    }
}