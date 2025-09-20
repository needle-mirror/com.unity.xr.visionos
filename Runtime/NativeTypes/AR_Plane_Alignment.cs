using System;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// Option set describing possible general alignments of a detected plane.
    /// </summary>
    [Flags]
    enum AR_Plane_Alignment : long
    {
        /** No plane alignment. */
        None                   = 0,

        /** Planes orthogonal to the gravity vector. */
        Horizontal             = 1 << 0,

        /** Planes parallel to the gravity vector. */
        Vertical               = 1 << 1
    }
}
