using System.Collections;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.TestTools;
using UnityEngine.XR.VisionOS.InputDevices;

namespace UnityEngine.XR.VisionOSTests
{
    /// <summary>
    /// Asserts that Swift's <c>MemoryLayout&lt;SpatialPointerEvent&gt;.stride</c> equals the marshalled size of
    /// the C# <see cref="VisionOSSpatialPointerEvent"/>. This bit us once — see the
    /// comment on <see cref="VisionOSSpatialPointerEvent"/>.
    /// </summary>
    /// <remarks>
    /// Complements the EditMode <c>SpatialPointerEventLayoutTests</c>, which is pure managed math and so runs
    /// in CI: this one reads Swift's ACTUAL stride from the loaded plugin, catching drift the C#-side rule
    /// can't predict (a Swift field reorder, an alignment or compiler layout change). The native symbol is
    /// only linked into the visionOS player, so this is ignored in the Editor (which links the mock).
    ///
    /// The bridge reads the stride from Swift on demand, so there is nothing to wait for: an unavailable
    /// result means the Swift trampoline was not built into the player at all.
    /// </remarks>
    class SpatialPointerEventNativeLayoutTests
    {
        [UnityTest]
        public IEnumerator NativeSwiftStride_MatchesManagedSize()
        {
#if UNITY_VISIONOS && !UNITY_EDITOR
            var nativeStride = VisionOSSpatialPointerEventNative.GetNativeStride();
            Assert.AreNotEqual(VisionOSSpatialPointerEventNative.StrideUnavailable, nativeStride,
                "Could not read the Swift SpatialPointerEvent stride, so the Swift trampoline was not built " +
                "into the player. That is expected in Windowed app mode, where spatial pointer events do not " +
                "exist, but nowhere else.");

            var managedSize = UnsafeUtility.SizeOf<VisionOSSpatialPointerEvent>();
            Assert.AreEqual(nativeStride, managedSize,
                $"Swift MemoryLayout<SpatialPointerEvent>.stride ({nativeStride}) must equal C# " +
                $"UnsafeUtility.SizeOf<VisionOSSpatialPointerEvent>() ({managedSize}). They are marshalled as " +
                "an array; a mismatch corrupts the second concurrent spatial pointer (two-handed input). " +
                "Update the C# [StructLayout(Size)] to match Swift's stride.");
#else
            Assert.Ignore("The native SpatialPointerEvent stride bridge is only linked into the visionOS " +
                          "player (device/sim); the Editor links the symbol-less mock. The C# side is guarded " +
                          "in CI by the EditMode SpatialPointerEventLayoutTests.");
#endif
            yield break;
        }
    }
}
