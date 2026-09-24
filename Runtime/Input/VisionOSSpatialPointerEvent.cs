using System.Runtime.InteropServices;

namespace UnityEngine.XR.VisionOS.InputDevices
{
    /// <summary>
    /// Struct to represent data for a visionOS spatial pointer event.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 60)]
    public struct VisionOSSpatialPointerEvent
    {
        /// <summary>
        /// The interaction Id of the spatial pointer event.
        /// </summary>
        [FieldOffset(0)]
        public int interactionId;

        /// <summary>
        /// The ray origin of the spatial pointer event.
        /// </summary>
        [FieldOffset(4)] // 0+4
        public Vector3 rayOrigin;

        /// <summary>
        /// The ray direction of the spatial pointer event.
        /// </summary>
        [FieldOffset(16)] // 4+12
        public Vector3 rayDirection;

        /// <summary>
        /// The input device position of the spatial pointer event.
        /// </summary>
        [FieldOffset(28)] // 16+12
        public Vector3 inputDevicePosition;

        /// <summary>
        /// The input device rotation of the spatial pointer event.
        /// </summary>
        [FieldOffset(40)] // 28+12
        public Quaternion inputDeviceRotation;

        /// <summary>
        /// The modifier key states of the spatial pointer event.
        /// </summary>
        [FieldOffset(56)] // 40+16
        public VisionOSSpatialPointerModifierKeys modifierKeys;

        /// <summary>
        /// The spatial pointer kind of the spatial pointer event.
        /// </summary>
        [FieldOffset(58)] // 56+2
        public VisionOSSpatialPointerKind kind;

        /// <summary>
        /// The pointer phase of the spatial pointer event.
        /// </summary>
        [FieldOffset(59)] // 58+1
        public VisionOSSpatialPointerPhase phase;
    }

    /// <summary>
    /// Native binding for the ABI guard on <see cref="VisionOSSpatialPointerEvent"/>.
    /// </summary>
    /// <remarks>
    /// The native input callback marshals an array of these structs, so the C# struct size must equal Swift's
    /// <c>MemoryLayout&lt;SpatialPointerEvent&gt;.stride</c>. If they differ, element 0 reads fine but every
    /// element after it is read at the wrong offset, silently corrupting the second concurrent spatial pointer
    /// (two-handed input). The bridge in VisionOSNativeBridge.mm reads Swift's stride so a device/sim test can
    /// compare the two. Routed through the bridge rather than binding the Swift symbol directly because the
    /// bridge compiles into UnityFramework alongside the IL2CPP caller, whereas SpatialPointerEvent.swift
    /// compiles into the app target and so cannot be linked against from here.
    /// </remarks>
    static class VisionOSSpatialPointerEventNative
    {
        /// <summary>Sentinel returned by <see cref="GetNativeStride"/> when the Swift symbol is absent.</summary>
        internal const int StrideUnavailable = -1;

        /// <summary>
        /// Returns Swift's <c>MemoryLayout&lt;SpatialPointerEvent&gt;.stride</c>, or
        /// <see cref="StrideUnavailable"/> if the Swift trampoline was not built into the player. Only
        /// resolvable in the visionOS player, where <see cref="NativeApi.Constants.LibraryName"/> is
        /// <c>__Internal</c>; in the Editor it points at the mock, which has no such symbol.
        /// </summary>
        [DllImport(NativeApi.Constants.LibraryName, EntryPoint = "UnityVisionOS_GetSpatialPointerEventStride")]
        internal static extern int GetNativeStride();
    }
}
