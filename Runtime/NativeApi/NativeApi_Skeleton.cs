using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// Native API wrappers for skeleton.
    /// signatures should match skeleton.h
    /// </summary>
    static class NativeApi_Skeleton
    {
        // TODO: Bring over missing summary comments
        // TODO: Clean up naming

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_skeleton_definition_get_joint_count")]
        public static extern ulong ar_skeleton_definition_get_joint_count(IntPtr definition);

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_skeleton_get_skeleton_root_transform_for_joint")]
        public static extern IntPtr ar_skeleton_get_skeleton_root_transform_for_joint(IntPtr skeleton, string joint);

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_skeleton_is_joint_tracked")]
        public static extern bool ar_skeleton_is_joint_tracked(IntPtr skeleton, string joint);

    }
}
