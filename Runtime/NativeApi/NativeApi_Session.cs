using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// Native API wrappers for session.
    /// Signatures and types should match session.h.
    /// </summary>
    static class NativeApi_Session
    {
        // TODO: Bring over missing summary comments
        // TODO: Clean up naming

        public delegate void AR_Session_Data_Provider_State_Change_Handler(IntPtr data_providers, AR_Data_Provider_State new_state, IntPtr error, IntPtr failed_data_provider);

        public delegate void AR_Authorization_Update_Handler(IntPtr authorization_result);

        /// <summary>
        /// Create an augmented reality session.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <returns>An instance of `ar_session_t`.</returns>
        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_session_create")]
        public static extern IntPtr ar_session_create();

        /// <summary>
        /// Handler to be called upon completion of an authorization request.
        /// </summary>
        /// <param name="authorization_results">A collection of authorization results.</param>
        /// <param name="error">An error object describing the error encountered during the authorization request. NULL if there was no error during the authorization request.</param>
        public delegate void AR_Authorization_Results_Handler(IntPtr authorization_results, IntPtr error);

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "UnityVisionOS_impl_ar_session_set_data_provider_state_change_handler")]
        public static extern void UnityVisionOS_impl_ar_session_set_data_provider_state_change_handler(IntPtr session, AR_Session_Data_Provider_State_Change_Handler data_provider_state_change_handler);

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_session_run")]
        public static extern void ar_session_run(IntPtr session, IntPtr data_providers);

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_session_stop")]
        public static extern void ar_session_stop(IntPtr session);

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_authorization_result_get_authorization_type")]
        public static extern AR_Authorization_Type ar_authorization_result_get_authorization_type(IntPtr authorization_result);

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_authorization_result_get_status")]
        public static extern AR_Authorization_Status ar_authorization_result_get_status(IntPtr authorization_result);

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_authorization_results_get_count")]
        public static extern uint ar_authorization_results_get_count(IntPtr authorization_results);

        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_authorization_results_enumerate_results")]
        public static extern void AR_Authorization_Results_Enumerate_Results(IntPtr authorization_results, IntPtr authorization_results_enumerator);

        /// <summary>
        /// Set the handler for receiving authorization status updates.
        /// </summary>
        /// <param name="session">The `ar_session_t` instance.</param>
        /// <param name="authorization_update_handler">Handler to be called when there is an update to authorization status.</param>
        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "UnityVisionOS_impl_ar_session_set_authorization_update_handler")]
        public static extern void UnityVisionOS_impl_ar_session_set_authorization_update_handler(IntPtr session, AR_Authorization_Update_Handler authorization_update_handler);

        /// <summary>
        /// Request authorization for accessing ARKit data.
        /// <remarks>This will asynchronously prompt users for permission to access their data. If the authorization is already allowed or denied by the user,
        /// the handler will be executed without prompting the user for permission again.</remarks>
        /// </summary>
        /// <param name="session">An instance of `ar_session_t`.</param>
        /// <param name="authorization_types">The authorization types to request.</param>
        /// <param name="results_handler">The handler to be called upon completion of the request. This handler will be executed on an arbitrary queue.</param>
        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "UnityVisionOS_impl_ar_session_request_authorization")]
        public static extern void UnityVisionOS_impl_ar_session_request_authorization(IntPtr session,
            AR_Authorization_Type authorization_types, AR_Authorization_Results_Handler results_handler);

        /// <summary>
        /// Query the status of different authorization types.
        /// </summary>
        /// <param name="session">The `ar_session_t` instance.</param>
        /// <param name="authorization_types">The authorization types to query.</param>
        /// <param name="results_handler">The handler to be called upon completion of the request. This handler will be executed on an arbitrary queue.</param>
        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "UnityVisionOS_impl_ar_session_query_authorization_results")]
        public static extern void UnityVisionOS_impl_ar_session_query_authorization_results(IntPtr session,
            AR_Authorization_Type authorization_types, AR_Authorization_Results_Handler results_handler);
    }
}
