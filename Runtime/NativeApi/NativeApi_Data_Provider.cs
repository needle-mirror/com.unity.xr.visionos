using System;
using System.Runtime.InteropServices;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace UnityEngine.XR.VisionOS
{
    // Signatures and types should match data_provider.h
    static class NativeApi_Data_Provider
    {
        /// <summary>
        /// State of the data provider.
        /// </summary>
        public enum AR_Data_Provider_State
        {
            Initialized,
            Running,
            Stopped
        }

        // TODO: Bring over missing summary comments
        // TODO: Clean up naming

        /// <summary>
        /// Creates an empty collection of data providers.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <returns>An instance of `ar_data_providers_t`.</returns>
        [DllImport("__Internal", EntryPoint = "ar_data_providers_create")]
        public static extern IntPtr ar_data_providers_create();

        // TODO: How to handle variable args
        /// <summary>
        /// Creates an empty collection of data providers.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <param name="provider">Variable number of data providers to add to newly created collection.</param>
        /// <returns>An instance of `ar_data_providers_t`.</returns>
        [DllImport("__Internal", EntryPoint = "ar_data_providers_create_with_providers")]
        public static extern IntPtr ar_data_providers_create_with_providers(IntPtr provider);

        /// <summary>
        /// Add a data provider to collection.
        /// </summary>
        /// <param name="data_providers">Collection to expand.</param>
        /// <param name="data_provider_to_add">Data provider to add.</param>
        [DllImport("__Internal", EntryPoint = "ar_data_providers_add_data_provider")]
        public static extern void ar_data_providers_add_data_provider(IntPtr data_providers, IntPtr data_provider_to_add);

        /// <summary>
        /// Add data providers to collection.
        /// </summary>
        /// <param name="data_providers">Collection to expand.</param>
        /// <param name="data_providers_to_add">Collection of data providers to add.</param>
        [DllImport("__Internal", EntryPoint = "ar_data_providers_add_data_providers")]
        public static extern void ar_data_providers_add_data_providers(IntPtr data_providers, IntPtr data_providers_to_add);

        /// <summary>
        /// Remove data provider from collection.
        /// </summary>
        /// <param name="data_providers">Collection from which to remove.</param>
        /// <param name="data_provider_to_remove">Data provider to remove.</param>
        [DllImport("__Internal", EntryPoint = "ar_data_providers_remove_data_provider")]
        public static extern void ar_data_providers_remove_data_provider(IntPtr data_providers, IntPtr data_provider_to_remove);

        /// <summary>
        /// Remove data providers from collection.
        /// </summary>
        /// <param name="data_providers">Collection from which to remove.</param>
        /// <param name="data_providers_to_remove">Collection of data providers to remove.</param>
        [DllImport("__Internal", EntryPoint = "ar_data_providers_remove_data_providers")]
        public static extern void ar_data_providers_remove_data_providers(IntPtr data_providers, IntPtr data_providers_to_remove);

        /// <summary>
        /// Get the count of data providers in the collection.
        /// </summary>
        /// <param name="data_providers">The collection of data providers.</param>
        /// <returns>The number of data providers in the collection.</returns>
        [DllImport("__Internal", EntryPoint = "ar_data_providers_get_count")]
        public static extern int ar_data_providers_get_count(IntPtr data_providers);

        // TODO: Implement remaining function signatures
    }
}
