using System;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// A value describing the classification of a plane anchor.
    /// </summary>
    enum AR_Plane_Classification
    {
        /** Plane classification is currently unavailable. */
        Status_not_available = 0,

        /** Tha classification of the plane has not yet been determined.  */
        Status_undetermined,

        /** The plane classification is not any of the known classes. */
        Status_unknown,

        /** The classification is of type wall. */
        Wall,

        /** The classification is of type floor. */
        Floor,

        /** The classification is of type ceiling. */
        Ceiling,

        /** The classification is of type table. */
        Table,

        /** The classification is of type seat. */
        Seat,

        /** The classification is of type window. */
        Window,

        /** The classification is of type door. */
        Door
    }
}
