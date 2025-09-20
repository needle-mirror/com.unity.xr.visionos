using System;
using System.Runtime.InteropServices;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace UnityEngine.XR.VisionOS
{
    // Signatures and types should match plane_detection.h
    static class NativeApi_Skeleton
    {
        //public delegate bool ar_skeleton_definition_joint_name_enumerator(string joint);

        [DllImport("__Internal", EntryPoint = "ar_skeleton_definition_get_joint_count")]
        public static extern ulong ar_skeleton_definition_get_joint_count(IntPtr definition);

        // [DllImport("__Internal", EntryPoint = "ar_skeleton_definition_enumerate_joints")]
        // public static extern ulong ar_skeleton_definition_enumerate_joints(IntPtr definition);

        [DllImport("__Internal", EntryPoint = "ar_skeleton_get_skeleton_root_transform_for_joint")]
        public static extern IntPtr ar_skeleton_get_skeleton_root_transform_for_joint(IntPtr skeleton, string joint);

        [DllImport("__Internal", EntryPoint = "ar_skeleton_is_joint_tracked")]
        public static extern bool ar_skeleton_is_joint_tracked(IntPtr skeleton, string joint);

    }
}
