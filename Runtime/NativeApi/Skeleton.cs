using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// Native API wrappers for skeleton.
    /// signatures should match skeleton.h
    /// </summary>
    static partial class NativeApi
    {
        internal static class Skeleton
        {
            // TODO: Bring over missing summary comments
            // TODO: Clean up naming

            [DllImport(Constants.LibraryName, EntryPoint = "ar_skeleton_get_anchor_from_joint_transform_for_joint")]
            public static extern IntPtr ar_skeleton_get_anchor_from_joint_transform_for_joint(IntPtr skeleton, string joint);

            [DllImport(Constants.LibraryName, EntryPoint = "ar_skeleton_is_joint_tracked")]
            public static extern bool ar_skeleton_is_joint_tracked(IntPtr skeleton, string joint);

        }
    }
}
