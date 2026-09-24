using System;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine.XR.VisionOS.InputDevices;

namespace UnityEditor.XR.VisionOSTests
{
    /// <summary>
    /// Guards the C# <see cref="VisionOSSpatialPointerEvent"/>'s declared <c>[StructLayout(Size = N)]</c>
    /// against the C-ABI stride rule that the Swift <c>SpatialPointerEvent</c> also follows, so the two stay
    /// marshal-compatible without needing a device. See the comment on
    /// <see cref="VisionOSSpatialPointerEvent"/> for why the strides must match.
    /// </summary>
    /// <remarks>
    /// The regression this guards against: adding a <c>UInt64</c> field took the fields to 68 bytes and
    /// forced 8-byte struct alignment, so Swift rounded the stride up to 72 while the C# struct was left at
    /// <c>Size = 68</c>. Every element after the first was then read 4 bytes short, so two-handed grab broke
    /// while single-handed, which only ever reads element 0, stayed fine. This struct has no 8-byte member:
    /// it spans 60 bytes and aligns to 4, hence <c>Size = 60</c>.
    /// </remarks>
    [TestFixture]
    class SpatialPointerEventLayoutTests
    {
        // The rule, for a struct of trivial fields: lay the fields out at their offsets, then round the total
        // size up to the struct's alignment (the max field alignment).
        [Test]
        public void DeclaredSize_MatchesCAbiStride_SoItAgreesWithSwiftStride()
        {
            var type = typeof(VisionOSSpatialPointerEvent);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(fields, Is.Not.Empty, "Expected VisionOSSpatialPointerEvent to have marshalled fields.");

            var maxFieldEnd = 0;
            var structAlignment = 1;
            foreach (var field in fields)
            {
                var offset = Marshal.OffsetOf(type, field.Name).ToInt32();
                var size = SizeOf(field.FieldType);
                maxFieldEnd = Math.Max(maxFieldEnd, offset + size);
                structAlignment = Math.Max(structAlignment, AlignmentOf(field.FieldType));
            }

            var expectedStride = RoundUp(maxFieldEnd, structAlignment);

            // Read Size off the attribute rather than calling Marshal.SizeOf, which can trip Mono's "cannot
            // be marshaled" path on a struct with enum fields.
            var layout = type.StructLayoutAttribute;
            Assert.That(layout, Is.Not.Null, "VisionOSSpatialPointerEvent must declare an explicit [StructLayout].");
            var declaredSize = layout.Size;

            Assert.AreEqual(expectedStride, declaredSize,
                $"VisionOSSpatialPointerEvent [StructLayout(Size = {declaredSize})] must be {expectedStride} " +
                $"(fields span {maxFieldEnd} bytes, rounded up to the struct's {structAlignment}-byte alignment). " +
                "This Size is the array-marshalling stride and must equal the Swift SpatialPointerEvent's " +
                "MemoryLayout.stride; if you changed the struct, update Size to match.");
        }

        // Marshalled size of a blittable type. Marshal.SizeOf throws on enum types ("cannot be marshaled")
        // under Mono, so resolve enums to their underlying primitive first.
        static int SizeOf(Type type)
        {
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);
            return Marshal.SizeOf(type);
        }

        // Natural C-ABI alignment of a blittable type: primitives align to their own size; an enum aligns
        // like its underlying primitive; a composite struct aligns to the max alignment of its fields.
        static int AlignmentOf(Type type)
        {
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);

            if (type.IsPrimitive)
                return Marshal.SizeOf(type);

            var alignment = 1;
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                alignment = Math.Max(alignment, AlignmentOf(field.FieldType));
            return alignment;
        }

        static int RoundUp(int value, int alignment) => (value + alignment - 1) / alignment * alignment;
    }
}
