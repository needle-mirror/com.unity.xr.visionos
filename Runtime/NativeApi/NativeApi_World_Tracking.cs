using System;
using System.Runtime.InteropServices;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace UnityEngine.XR.VisionOS
{
    // Signatures and types should match world_tracking.h
    static class NativeApi_World_Tracking
    {
#if UNITY_VISIONOS && !UNITY_EDITOR
        const string k_LibraryName = "__Internal";
#else
        const string k_LibraryName = "arkit_stub";
#endif
        
        // TODO: Bring over missing summary comments
        // TODO: Clean up naming
        
        /// <summary>
        /// Handler triggered when there are updates to world anchors.
        /// <param name="added_anchors">Collection of anchors that are added.</param>
        /// <param name="updated_anchors">Collection of anchors that are updated.</param>
        /// <param name="removed_anchors">Collection of anchors that are removed.</param>
        /// </summary>
        public unsafe delegate void AR_World_Tracking_Update_Handler(void* added_anchors, int added_anchor_count,
            void* updated_anchors, int updated_anchor_count, void* removed_anchors, int removed_anchor_count);
        
        public delegate void AR_World_Tracking_Add_Anchor_Completion_Handler(IntPtr world_anchor, bool successful, IntPtr error);
        public delegate void AR_World_Tracking_Remove_Anchor_Completion_Handler(IntPtr world_anchor, bool successful, IntPtr error);

        [DllImport(k_LibraryName, EntryPoint = "UnityVisionOS_impl_ar_world_anchor_create_with_transform_float_array")]
        public static extern IntPtr UnityVisionOS_impl_ar_world_anchor_create_with_transform_float_array(IntPtr transform);
        
        [DllImport(k_LibraryName, EntryPoint = "UnityVisionOS_impl_ar_world_tracking_provider_add_anchor")]
        public static extern IntPtr UnityVisionOS_impl_ar_world_tracking_provider_add_anchor(IntPtr world_tracking_provider,
            IntPtr world_anchor, AR_World_Tracking_Add_Anchor_Completion_Handler add_anchor_completion_handler);
        
        [DllImport(k_LibraryName, EntryPoint = "UnityVisionOS_impl_ar_world_tracking_provider_remove_anchor_with_identifier")]
        public static extern IntPtr UnityVisionOS_impl_ar_world_tracking_provider_remove_anchor_with_identifier(IntPtr world_tracking_provider,
            IntPtr anchor_identifier, AR_World_Tracking_Remove_Anchor_Completion_Handler add_anchor_completion_handler);

        [DllImport(k_LibraryName, EntryPoint = "ar_world_anchors_get_count")]
        public static extern int ar_world_anchors_get_count(IntPtr world_anchors);

        // TODO: Wrapper function for enumerating anchors

        /// <summary>
        /// Set the handler for receiving world tracking anchor updates.
        /// </summary>
        /// <param name="world_tracking_provider">World tracking provider.</param>
        /// <param name="world_tracking_update_handler">The world tracking update handler.</param>
        [DllImport(k_LibraryName, EntryPoint = "UnityVisionOS_impl_ar_world_tracking_provider_set_anchor_update_handler")]
        public static extern void UnityVisionOS_impl_ar_world_tracking_provider_set_anchor_update_handler(IntPtr world_tracking_provider,
            AR_World_Tracking_Update_Handler world_tracking_update_handler);
    }
}
