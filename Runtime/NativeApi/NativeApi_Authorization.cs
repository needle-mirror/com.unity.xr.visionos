using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// Native API wrappers for authorization.
    /// Signatures should match authorization.h.
    /// </summary>
    static class NativeApi_Authorization
    {
        public delegate void Authorization_Results_Enumeration_Step_Callback(IntPtr authorization_result);

        public delegate void Authorization_Results_Enumeration_Completed_Callback();

        // TODO: Bring over missing summary comments
        // TODO: Clean up naming
        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_authorization_result_get_authorization_type")]
        public static extern AR_Authorization_Type ar_authorization_result_get_authorization_type(IntPtr authorization_result);

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_authorization_result_get_status")]
        public static extern AR_Authorization_Status ar_authorization_result_get_status(IntPtr authorization_result);

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_authorization_results_get_count")]
        public static extern int ar_authorization_results_get_count(IntPtr authorization_results);

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "UnityVisionOS_impl_ar_authorization_results_enumerate_results")]
        public static extern void UnityVisionOS_impl_ar_authorization_results_enumerate_results(IntPtr authorization_results,
            Authorization_Results_Enumeration_Step_Callback step, Authorization_Results_Enumeration_Completed_Callback completed);
    }
}
