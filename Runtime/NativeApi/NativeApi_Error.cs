using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// Native API wrappers for error.
    /// Signatures should match error.h.
    /// </summary>
    static class NativeApi_Error
    {
        /// <summary>
        /// Get the error code associated with an error.
        /// </summary>
        /// <param name="error">An instance of `ar_error_t`.</param>
        /// <returns>The error code.</returns>
        [DllImport(NativeApi_Constants.LibraryName, EntryPoint = "ar_error_get_error_code")]
        public static extern int ar_error_get_error_code(IntPtr error);
    }
}
