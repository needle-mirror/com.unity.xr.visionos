using UnityEngine;
// ReSharper disable InconsistentNaming

namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// Types of authorization for ARKit data.
    /// </summary>
    enum AR_Authorization_Type : long
    {
        None = 0,

        //Authorization type used when requesting hand tracking.
        Hand_Tracking = 1 << 0,
        /** Authorization type used when requesting:
                - Plane detection
                - Scene reconstruction
            */
        World_Sensing = 1 << 1
    }
}
