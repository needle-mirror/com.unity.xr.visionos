using System;
using System.Runtime.InteropServices;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace UnityEngine.XR.VisionOS
{
    // Signatures and types should match plane_detection.h
    static class NativeApi_Hand_Tracking
    {
        /// <summary>
        /// Handler triggered when there are hand tracking updates.
        /// </summary>
        /// <param name="hand_anchor_left">The latest left hand anchor.</param>
        /// <param name="hand_anchor_right">The latest right hand anchor.</param>
        public delegate void AR_Hand_Tracking_Update_Handler(IntPtr hand_anchor_left, IntPtr hand_anchor_right);

        /// <summary>
        /// Create a hand anchor.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <returns>Returns an allocated `ar_hand_anchor_t` object.</returns>
        [DllImport("__Internal", EntryPoint = "ar_hand_anchor_create")]
        public static extern IntPtr ar_hand_anchor_create();

        /// <summary>
        /// Get the skeleton of hand anchor.
        /// </summary>
        /// <param name="hand_anchor">Hand anchor.</param>
        /// <returns>The skeleton.</returns>
        [DllImport("__Internal", EntryPoint = "ar_hand_anchor_get_skeleton")]
        public static extern IntPtr ar_hand_anchor_get_skeleton(IntPtr hand_anchor);

        /// <summary>
        /// Get the chirality of the hand tracked by the hand anchor.
        /// </summary>
        /// <param name="hand_anchor">Hand anchor.</param>
        /// <returns>The chirality of the hand.</returns>
        [DllImport("__Internal", EntryPoint = "ar_hand_anchor_get_chirality")]
        public static extern AR_Hand_Chirality ar_hand_anchor_get_chirality(IntPtr hand_anchor);

        /// <summary>
        /// Create hand tracking configuration.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <returns>An instance of `ar_hand_tracking_configuration_t`.</returns>
        [DllImport("__Internal", EntryPoint = "ar_hand_tracking_configuration_create")]
        public static extern IntPtr ar_hand_tracking_configuration_create();

        /// <summary>
        /// Create a hand tracking provider.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <param name="hand_tracking_configuration">Hand tracking configuration.</param>
        /// <returns>An instance of `ar_hand_tracking_provider`.</returns>
        [DllImport("__Internal", EntryPoint = "ar_hand_tracking_provider_create")]
        public static extern IntPtr ar_hand_tracking_provider_create(IntPtr hand_tracking_configuration);

        /// <summary>
        /// Set the handler for receiving hand tracking updates.
        /// </summary>
        /// <remarks>
        /// Native API includes hand_anchor_updates_queue parameter which we are not using
        /// </remarks>
        /// <param name="hand_tracking_provider">Hand tracking provider.</param>
        /// <param name="hand_tracking_update_handler">Handler to be called when new data arrives.</param>
        /// <returns></returns>
        [DllImport("__Internal", EntryPoint = "UnityVisionOS_impl_ar_hand_tracking_provider_set_update_handler")]
        public static extern IntPtr UnityVisionOS_impl_ar_hand_tracking_provider_set_update_handler(IntPtr hand_tracking_provider,
            AR_Hand_Tracking_Update_Handler hand_tracking_update_handler);

        /// <summary>
        /// Fill the given ar_hand_anchor_t instances with latest hand anchor tracking data, if at least one of the hands has been tracked since the last call
        /// to this function. Subsequent calls to this function will not update the instances and return false until updated tracking data has arrived.
        /// </summary>
        /// <param name="hand_tracking_provider">Hand tracking provider.</param>
        /// <param name="hand_anchor_left">`ar_hand_anchor_t` instance for the left hand to be updated</param>
        /// <param name="hand_anchor_right">`ar_hand_anchor_t` instance for the right hand to be updated</param>
        /// <returns>True on success and false if there is no update for either of the hands.</returns>
        [DllImport("__Internal", EntryPoint = "ar_hand_tracking_provider_get_latest_anchors")]
        public static extern bool ar_hand_tracking_provider_get_latest_anchors(IntPtr hand_tracking_provider,
            IntPtr hand_anchor_left, IntPtr hand_anchor_right);

        /// <summary>
        /// Get the authorization type required by the hand tracking provider.
        /// </summary>
        /// <returns>Authorization type.</returns>
        [DllImport("__Internal", EntryPoint = "ar_hand_tracking_provider_get_required_authorization_type")]
        public static extern AR_Authorization_Type ar_hand_tracking_provider_get_required_authorization_type();


        /// <summary>
        /// Get the list of joint names supported by ARKit. Assume there are 27 of them, and they align with XRJointID
        /// </summary>
        /// <returns>Pointer to a list of C strings containing the joint names.</returns>
        [DllImport("__Internal", EntryPoint = "UnityVisionOS_impl_get_joint_names")]
        public static extern unsafe byte** UnityVisionOS_impl_get_joint_names();
    }
}
