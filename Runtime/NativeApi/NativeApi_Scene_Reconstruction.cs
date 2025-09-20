using System;
using System.Runtime.InteropServices;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace UnityEngine.XR.VisionOS
{
    // Signatures and types should match scene_reconstruction.h
    static class NativeApi_Scene_Reconstruction
    {
#if UNITY_VISIONOS && !UNITY_EDITOR
        const string k_LibraryName = "__Internal";
#else
        const string k_LibraryName = "arkit_stub";
#endif
        
        /// <summary>
        /// Get a Metal buffer containing index data that defines the geometry.
        /// </summary>
        /// <param name="geometry_element">The geometry element.</param>
        /// <returns>The Metal buffer.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_geometry_element_get_buffer")]
        public static extern IntPtr ar_geometry_element_get_buffer(IntPtr geometry_element);
        
        /// <summary>
        /// Get the number of primitives in the buffer.
        /// </summary>
        /// <param name="geometry_element">The geometry element.</param>
        /// <returns>The number of primitives.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_geometry_element_get_count")]
        public static extern int ar_geometry_element_get_count(IntPtr geometry_element);
        
        /// <summary>
        /// Get the number of bytes that represent an index value.
        /// </summary>
        /// <param name="geometry_element">The geometry element.</param>
        /// <returns>The number of bytes that represent an index value.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_geometry_element_get_bytes_per_index")]
        public static extern int ar_geometry_element_get_bytes_per_index(IntPtr geometry_element);
        
        /// <summary>
        /// Get the number of indices for each primitive.
        /// </summary>
        /// <remarks>
        /// This is based on the primitiveType. For ARGeometryPrimitiveTypeTriangle the value is 3.
        /// </remarks>
        /// <param name="geometry_element">The geometry element.</param>
        /// <returns>The number of indices for each primitive.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_geometry_element_get_index_count_per_primitive")]
        public static extern int ar_geometry_element_get_index_count_per_primitive(IntPtr geometry_element);

        /// <summary>
        /// Get the primitive type of the geometry element.
        /// </summary>
        /// <param name="geometry_element">The geometry element.</param>
        /// <returns>The primitive type of the geometry element.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_geometry_element_get_primitive_type")]
        public static extern AR_Geometry_Primitive_Type ar_geometry_element_get_primitive_type(IntPtr geometry_element);

        /// <summary>
        /// Get a Metal buffer containing per-vector data for the source.
        /// </summary>
        /// <param name="geometry_source">The geometry source.</param>
        /// <returns>The Metal buffer.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_geometry_source_get_buffer")]
        public static extern IntPtr ar_geometry_source_get_buffer(IntPtr geometry_source);
        
        /// <summary>
        /// Get a Metal buffer containing per-vector data for the source, and return its contents as a void*
        /// </summary>
        /// <param name="geometry_source">The geometry source.</param>
        /// <returns>The contents of the Metal buffer.</returns>
        [DllImport(k_LibraryName, EntryPoint = "UnityVisionOS_impl_ar_geometry_source_get_buffer")]
        public static extern IntPtr UnityVisionOS_impl_ar_geometry_source_get_buffer(IntPtr geometry_source);
        
        /// <summary>
        /// Get the number of vectors in the source.
        /// </summary>
        /// <param name="geometry_source">The geometry source.</param>
        /// <returns>The number of vectors in the source.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_geometry_source_get_count")]
        public static extern int ar_geometry_source_get_count(IntPtr geometry_source);
        
        /// <summary>
        /// Get the type of per-vector data in the buffer.
        /// </summary>
        /// <param name="geometry_source">The geometry source.</param>
        /// <returns>The type of per-vector data in the buffer.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_geometry_source_get_format")]
        public static extern MTLVertexFormat ar_geometry_source_get_format(IntPtr geometry_source);
        
        /// <summary>
        /// Get the number of scalar components in each vector.
        /// </summary>
        /// <param name="geometry_source">The geometry source.</param>
        /// <returns>The number of scalar components in each vector.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_geometry_source_get_components_per_vector")]
        public static extern int ar_geometry_source_get_components_per_vector(IntPtr geometry_source);
        
        /// <summary>
        /// Get the offset (in bytes) from the beginning of the buffer.
        /// </summary>
        /// <param name="geometry_source">The geometry source.</param>
        /// <returns>The offset (in bytes) from the beginning of the buffer.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_geometry_source_get_offset")]
        public static extern int ar_geometry_source_get_offset(IntPtr geometry_source);
        
        /// <summary>
        /// Get the number of bytes from a vector to the next one in the buffer.
        /// </summary>
        /// <param name="geometry_source">The geometry source.</param>
        /// <returns>The number of bytes from a vector to the next one in the buffer.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_geometry_source_get_stride")]
        public static extern int ar_geometry_source_get_stride(IntPtr geometry_source);

        /// <summary>
        /// Get the vertices of the mesh.
        /// </summary>
        /// <param name="mesh_geometry">The mesh geometry.</param>
        /// <returns>An instance of `ar_geometry_source_t`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_mesh_geometry_get_vertices")]
        public static extern IntPtr ar_mesh_geometry_get_vertices(IntPtr mesh_geometry);
        
        /// <summary>
        /// Get the normals of the mesh.
        /// </summary>
        /// <param name="mesh_geometry">The mesh geometry.</param>
        /// <returns>An instance of `ar_geometry_source_t`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_mesh_geometry_get_normals")]
        public static extern IntPtr ar_mesh_geometry_get_normals(IntPtr mesh_geometry);
        
        /// <summary>
        /// Get the faces of the mesh.
        /// </summary>
        /// <param name="mesh_geometry">The mesh geometry.</param>
        /// <returns>An instance of `ar_geometry_element_t`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_mesh_geometry_get_faces")]
        public static extern IntPtr ar_mesh_geometry_get_faces(IntPtr mesh_geometry);
        
        /// <summary>
        /// Get the classification for each face of the mesh.
        /// </summary>
        /// <param name="mesh_geometry">The mesh geometry.</param>
        /// <returns>An instance of `ar_geometry_source_t`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_mesh_geometry_get_classification")]
        public static extern IntPtr ar_mesh_geometry_get_classification(IntPtr mesh_geometry);
        
        /// <summary>
        /// Get the geometry of the mesh in the anchor's coordinate system.
        /// </summary>
        /// <param name="mesh_anchor">The mesh anchor.</param>
        /// <returns>An instance of `ar_mesh_geometry_t`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_mesh_anchor_get_geometry")]
        public static extern IntPtr ar_mesh_anchor_get_geometry(IntPtr mesh_anchor);
        
        /// <summary>
        /// Get the count of mesh anchors in the collection.
        /// </summary>
        /// <param name="mesh_anchors">The collection of mesh anchors.</param>
        /// <returns>The number of number of mesh anchors in the collection.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_mesh_anchors_get_count")]
        public static extern int ar_mesh_anchors_get_count(IntPtr mesh_anchors);
        
        //TODO: Mesh anchor enumeration

        /// <summary>
        /// Create a scene reconstruction configuration.
        /// </summary>
        /// <returns>An instance of `ar_scene_reconstruction_configuration_t`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_scene_reconstruction_configuration_create")]
        public static extern IntPtr ar_scene_reconstruction_configuration_create();
        
        /// <summary>
        /// Get the scene reconstruction mode.
        /// </summary>
        /// <param name="scene_reconstruction_configuration">Scene reconstruction configuration.</param>
        /// <returns>Type of scene reconstruction mode associated with the setting.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_scene_reconstruction_configuration_get_scene_reconstruction_mode")]
        public static extern AR_Scene_Reconstruction_Mode ar_scene_reconstruction_configuration_get_scene_reconstruction_mode(IntPtr scene_reconstruction_configuration);
        
        /// <summary>
        /// Set the scene reconstruction mode.
        /// </summary>
        /// <param name="scene_reconstruction_configuration">Scene reconstruction configuration.</param>
        /// <param name="scene_reconstruction_mode">Scene reconstruction mode to set.</param>
        [DllImport(k_LibraryName, EntryPoint = "ar_scene_reconstruction_configuration_set_scene_reconstruction_mode")]
        public static extern void ar_scene_reconstruction_configuration_set_scene_reconstruction_mode(IntPtr scene_reconstruction_configuration, AR_Scene_Reconstruction_Mode scene_reconstruction_mode);
        
        /// <summary>
        /// Create a scene reconstruction provider.
        /// </summary>
        /// <param name="scene_reconstruction_configuration">Scene reconstruction configuration.</param>
        /// <returns>An instance of `ar_scene_reconstruction_provider`.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_scene_reconstruction_provider_create")]
        public static extern IntPtr ar_scene_reconstruction_provider_create(IntPtr scene_reconstruction_configuration);
        
        //TODO: Scene reconstruction update handler
        
        /// <summary>
        /// Get the authorization type required by the scene reconstruction provider.
        /// </summary>
        /// <returns>Authorization type.</returns>
        [DllImport(k_LibraryName, EntryPoint = "ar_scene_reconstruction_provider_get_required_authorization_type")]
        public static extern AR_Authorization_Type ar_scene_reconstruction_provider_get_required_authorization_type();
    }
}
