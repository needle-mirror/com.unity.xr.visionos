using UnityEngine;
// ReSharper disable InconsistentNaming

namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// Status of an authorization.
    /// </summary>
    enum AR_Authorization_Status
    {
        // The user has not yet granted permission.
        Not_Determined,

        // The user has explicitly granted permission.
        Allowed,

        // The user has explicitly denied permission.
        Status_Denied
    }
}
