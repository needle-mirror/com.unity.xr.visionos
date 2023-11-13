    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.InputSystem.Layouts;
    using UnityEngine.InputSystem.LowLevel;
    using UnityEngine.InputSystem.Utilities;

    namespace UnityEngine.XR.VisionOS.InputDevices
    {
        [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        public struct VisionOSSpatialPointerState : IInputStateTypeInfo
        {
            public const string LayoutName = "VisionOSSpatialPointer";
            public const int SizeInBytes = 81; // 77+4

            public static FourCC Format => new('V', 'O', 'P', 'S');

            [InputControl(displayName = "Interaction ID", layout = "Integer", synthetic = true, dontReset = true)]
            [FieldOffset(0)]
            public int interactionId;

            [InputControl(displayName = "Start Ray Origin", noisy = true, dontReset = true)]
            [FieldOffset(4)] //0+4
            public Vector3 startRayOrigin;

            [InputControl(displayName = "Start Ray Direction", noisy = true, dontReset = true)]
            [FieldOffset(16)] //4+12
            public Vector3 startRayDirection;

            [InputControl(displayName = "Start Ray Rotation", noisy = true, dontReset = true)]
            [FieldOffset(28)] //16+12
            public Quaternion startRayRotation;

            [InputControl(displayName = "Device Position", noisy = true, dontReset = true)]
            [FieldOffset(44)] //28+16
            public Vector3 devicePosition;

            [InputControl(displayName = "Device Rotation", noisy = true, dontReset = true)]
            [FieldOffset(56)] //44+12
            public Quaternion deviceRotation;

            [InputControl(displayName = "Modifier Keys", layout = "Integer", synthetic = true)]
            [FieldOffset(72)] //56+16
            public ushort modifierKeys;

            [InputControl(name = "kind", displayName = "Kind", layout = "Integer", synthetic = true)]
            [FieldOffset(74)] //72+2
            public byte kindId;

            [InputControl(name = "phase", displayName = "Phase", layout = "TouchPhase", synthetic = true)]
            [FieldOffset(75)] //74+1
            public byte phaseId;

            [InputControl(name = "isTracked", displayName = "IsTracked", layout = "Button", synthetic = true)]
            [FieldOffset(76)] //75+1
            public bool isTracked;

            [InputControl(name = "trackingState", displayName = "TrackingState", layout = "Integer", synthetic = true)]
            [FieldOffset(77)] //76+1
            public InputTrackingState trackingState;

            public VisionOSSpatialPointerKind Kind
            {
                get => (VisionOSSpatialPointerKind)kindId;
                set => kindId = (byte)value;
            }

            public bool IsModifierKeyPressed(VisionOSSpatialPointerModifierKeys key)
            {
                return (modifierKeys & (ushort)key) != 0;
            }

            public FourCC format => Format;

            public void SetModifierKey(bool state, ushort modifierKey)
            {
                if (state)
                    modifierKeys |= modifierKey;
                else
                    modifierKeys &= (ushort)~modifierKey;
            }

            public VisionOSSpatialPointerPhase phase
            {
                get => (VisionOSSpatialPointerPhase)phaseId;
                set => phaseId = (byte)value;
            }

            public bool isNoneEndedOrCanceled
            {
                get
                {
                    switch (phase)
                    {
                        case VisionOSSpatialPointerPhase.None:
                        case VisionOSSpatialPointerPhase.Ended:
                        case VisionOSSpatialPointerPhase.Cancelled:
                            return true;
                        default:
                            return false;
                    }
                }
            }
        }
    }
