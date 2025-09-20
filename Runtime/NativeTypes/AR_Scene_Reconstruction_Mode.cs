using System;
// ReSharper disable InconsistentNaming

namespace UnityEngine.XR.VisionOS
{
    [Flags]
    enum AR_Scene_Reconstruction_Mode : long
    {
        /** Scene reconstruction default mode. Generates a mesh of the world. */
        Default = 0,
        
        /** Scene reconstruction classification mode. It generates a mesh of the world with an additional classification for each face. */
        Classification = 1 << 0
    }
}
