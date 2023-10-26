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
            public const int SizeInBytes = 60;

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

            [InputControl(displayName = "Device Position", noisy = true, dontReset = true)]
            [FieldOffset(28)] //16+12
            public Vector3 devicePosition;

            [InputControl(displayName = "Device Rotation", noisy = true, dontReset = true)]
            [FieldOffset(40)] //28+12
            public Quaternion deviceRotation;

            [InputControl(displayName = "Modifier Keys", layout = "Integer", synthetic = true)]
            [FieldOffset(56)] //40+16
            public ushort modifierKeys;

            [InputControl(name = "kind", displayName = "Kind", layout = "Integer", synthetic = true)]
            [FieldOffset(58)] //56+2
            public byte kindId;

            [InputControl(name = "phase", displayName = "Phase", layout = "TouchPhase", synthetic = true)]
            [FieldOffset(59)] //58+1
            public byte phaseId;

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
