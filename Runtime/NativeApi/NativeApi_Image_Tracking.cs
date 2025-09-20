using System;
using System.Runtime.InteropServices;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace UnityEngine.XR.VisionOS
{
    // Signatures and types should match image_tracking.h
    static class NativeApi_Image_Tracking
    {
#if UNITY_VISIONOS && !UNITY_EDITOR
        const string k_LibraryName = "__Internal";
#else
        const string k_LibraryName = "arkit_stub";
#endif

        /// <summary>
        /// Handler triggered when there are updates to image anchors.
        /// <param name="added_anchors">Collection of anchors that are added.</param>
        /// <param name="updated_anchors">Collection of anchors that are updated.</param>
        /// <param name="removed_anchors">Collection of anchors that are removed.</param>
        /// </summary>
        public unsafe delegate void AR_Image_Tracking_Update_Handler(void* added_anchors, int added_anchor_count,
            void* updated_anchors, int updated_anchor_count, void* removed_anchors, int removed_anchor_count);

        /// <summary>
        /// Get the name of the image anchor.
        /// </summary>
        /// <param name="image_anchor">An instance of `ar_image_anchor_t`.</param>
        /// <returns>An optional name used to associate with the anchor. Returns NULL if no name is associated with this anchor.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_image_anchor_get_name")]
        public static extern IntPtr ar_image_anchor_get_name(IntPtr image_anchor);

        /// <summary>
        /// Get the estimated scale factor.
        /// </summary>
        /// <param name="image_anchor">An instance of `ar_image_anchor_t`.</param>
        /// <returns>The factor between estimated physical size and provided size.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_image_anchor_get_estimated_scale_factor")]
        public static extern float ar_image_anchor_get_estimated_scale_factor(IntPtr image_anchor);

        /// <summary>
        /// Get the underlying tracked reference image from an image anchor.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <param name="image_anchor">The image anchor to get the reference image from.</param>
        /// <returns>An instance of `ar_reference_image_t`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_image_anchor_get_reference_image")]
        public static extern IntPtr ar_image_anchor_get_reference_image(IntPtr image_anchor);

        /// <summary>
        /// Get the count of image anchors in the collection.
        /// </summary>
        /// <param name="image_anchors">The collection of image anchors.</param>
        /// <returns>The number of image anchors in the collection.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_image_anchors_get_count")]
        public static extern int ar_image_anchors_get_count(IntPtr image_anchors);

        // TODO: Wrapper function for enumerating anchors

        // TODO: Managed type for CGImagePropertyOrientation
        /// <summary>
        /// Create a reference image from a `CVPixelBufferRef`.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <param name="pixelBuffer">The reference image as  `CVPixelBuffer`.</param>
        /// <param name="orientation">The image orientation.</param>
        /// <param name="physicalWidth">The width in meters of the physical object.</param>
        /// <returns>An instance of `ar_reference_image_t`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_reference_image_create_from_pixel_buffer")]
        public static extern IntPtr ar_reference_image_create_from_pixel_buffer(IntPtr pixelBuffer, int orientation, float physicalWidth);

        /// <summary>
        /// Create a reference image from a `CGImageRef`.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <param name="image">The reference image as `CGImageRef`.</param>
        /// <param name="orientation">The image orientation.</param>
        /// <param name="physicalWidth">The width in meters of the physical object.</param>
        /// <returns>An instance of `ar_reference_image_t`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_reference_image_create_from_cgimage")]
        public static extern IntPtr ar_reference_image_create_from_cgimage(IntPtr image, int orientation, float physicalWidth);

        /// <summary>
        /// Set the reference image name.
        /// </summary>
        /// <param name="reference_image">Reference Image.</param>
        /// <param name="name">A name used to identify the image.</param>
        [DllImport(k_LibraryName, EntryPoint = "ar_reference_image_set_name")]
        public static extern void ar_reference_image_set_name(IntPtr reference_image, IntPtr name);

        /// <summary>
        /// Get the reference image name.
        /// </summary>
        /// <param name="reference_image">Reference Image.</param>
        /// <returns>The name of the image, might be NULL if it hasn't been set before.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_reference_image_get_name")]
        public static extern IntPtr ar_reference_image_get_name(IntPtr reference_image);

        /// <summary>
        /// Get the width in meters of the reference image.
        /// </summary>
        /// <param name="reference_image">Reference Image.</param>
        /// <returns>The physical width of the image in meters.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_reference_image_get_physical_width")]
        public static extern float ar_reference_image_get_physical_width(IntPtr reference_image);

        /// <summary>
        /// Get the height in meters of the reference image.
        /// </summary>
        /// <param name="reference_image">Reference Image.</param>
        /// <returns>The physical width of the image in meters.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_reference_image_get_physical_height")]
        public static extern float ar_reference_image_get_physical_height(IntPtr reference_image);

        /// <summary>
        /// Create a collection of reference images initialized with an empty set.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <returns>An instance of `ar_reference_images_t`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_reference_images_create")]
        public static extern IntPtr ar_reference_images_create();

        /// <summary>
        /// Load reference images from a bundle into a new collection.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <param name="group_name">Group to load images from.</param>
        /// <param name="bundle">If nil, this will load the main bundle</param>
        /// <returns>New collection of reference images.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_reference_images_load_reference_images_in_group")]
        public static extern IntPtr ar_reference_images_load_reference_images_in_group(IntPtr group_name, IntPtr bundle);

        /// <summary>
        /// Add a reference image to collection. The image must be unique; any duplicates of existing images will not be added.
        /// </summary>
        /// <param name="reference_images">Collection to expand.</param>
        /// <param name="image_to_add">Image to add.</param>
        [DllImport(k_LibraryName, EntryPoint = "ar_reference_images_add_image")]
        public static extern void ar_reference_images_add_image(IntPtr reference_images, IntPtr image_to_add);

        /// <summary>
        /// Add reference images to collection. The images must be unique; any duplicates of existing images will not be added.
        /// </summary>
        /// <param name="reference_images">Collection to expand.</param>
        /// <param name="images_to_add">Collection of images to add.</param>
        [DllImport(k_LibraryName, EntryPoint = "ar_reference_images_add_images")]
        public static extern void ar_reference_images_add_images(IntPtr reference_images, IntPtr images_to_add);

        /// <summary>
        /// Get the count of reference images in the collection.
        /// </summary>
        /// <param name="reference_images">The collection of reference images.</param>
        /// <returns>The number of reference images in the collection.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_reference_images_get_count")]
        public static extern int ar_reference_images_get_count(IntPtr reference_images);

        // TODO: wrapper function for enumerating reference images

        /// <summary>
        /// Create image tracking configuration.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <returns>An instance of `ar_image_tracking_configuration_t`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_image_tracking_configuration_create")]
        public static extern IntPtr ar_image_tracking_configuration_create();

        /// <summary>
        /// Add reference images to the set to be tracked. The image tracking configuration can run without any reference images, but will not detect anything.
        /// </summary>
        /// <param name="image_tracking_configuration">Image tracking configuration.</param>
        /// <param name="reference_images">Reference images to add.</param>
        [DllImport(k_LibraryName, EntryPoint = "ar_image_tracking_configuration_add_reference_images")]
        public static extern void ar_image_tracking_configuration_add_reference_images(IntPtr image_tracking_configuration, IntPtr reference_images);

        // TODO: Wrapper function for add images completion handler

        /// <summary>
        /// Create an image tracking provider.
        /// </summary>
        /// <remarks>
        /// This type supports ARC. In non-ARC files, use `ar_retain()` and `ar_release()` to retain and release the object.
        /// </remarks>
        /// <param name="image_tracking_configuration">Image Tracking configuration.</param>
        /// <returns>An instance of `ar_image_tracking_provider`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_image_tracking_provider_create")]
        public static extern IntPtr ar_image_tracking_provider_create(IntPtr image_tracking_configuration);

        /// <summary>
        /// Set the image tracking update handler.
        /// </summary>
        /// <param name="image_tracking_provider">The image provider.</param>
        /// <param name="image_tracking_update_handler">The image tracking update handler.</param>
        [DllImport(k_LibraryName, EntryPoint = "UnityVisionOS_impl_ar_image_tracking_provider_set_update_handler")]
        public static extern void UnityVisionOS_impl_ar_image_tracking_provider_set_update_handler(IntPtr image_tracking_provider,
            AR_Image_Tracking_Update_Handler image_tracking_update_handler);

        /// <summary>
        /// Get the authorization type required by the image tracking provider.
        /// </summary>
        /// <returns>Authorization type.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_image_tracking_provider_get_required_authorization_type")]
        public static extern AR_Authorization_Type ar_image_tracking_provider_get_required_authorization_type();
        
        /// <summary>
        /// Get a reference image from a collection of reference images.
        /// </summary>
        /// <param name="reference_images">The collection of reference images.</param>
        /// <param name="index">The index of the image to get.</param>
        /// <returns></returns>
        [DllImport(k_LibraryName, EntryPoint = "UnityVisionOS_impl_get_reference_image_at_index")]
        public static extern IntPtr UnityVisionOS_impl_get_reference_image_at_index(IntPtr reference_images, int index);
    }
}
