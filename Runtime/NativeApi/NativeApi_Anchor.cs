using System;
using System.Runtime.InteropServices;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace UnityEngine.XR.VisionOS
{
    // Signatures and types should match anchor.h
    static class NativeApi_Anchor
    {
        // TODO: Bring over missing summary comments
        // TODO: Clean up naming
        [DllImport("__Internal", EntryPoint = "ar_anchor_get_origin_from_anchor_transform")]
        public static extern IntPtr ar_anchor_get_origin_from_anchor_transform(IntPtr anchor);

        [DllImport("__Internal", EntryPoint = "UnityVisionOS_impl_ar_anchor_get_origin_from_anchor_transform_to_float_array")]
        public static extern IntPtr UnityVisionOS_impl_ar_anchor_get_origin_from_anchor_transform_to_float_array(IntPtr anchor);

        [DllImport("__Internal", EntryPoint = "ar_anchor_get_identifier")]
        public static extern void ar_anchor_get_identifier(IntPtr anchor, IntPtr out_identifier);

        [DllImport("__Internal", EntryPoint = "ar_anchor_get_timestamp")]
        public static extern IntPtr ar_anchor_get_timestamp(IntPtr anchor);

        [DllImport("__Internal", EntryPoint = "ar_trackable_anchor_is_tracked")]
        public static extern bool ar_trackable_anchor_is_tracked(IntPtr anchor);
    }
}
