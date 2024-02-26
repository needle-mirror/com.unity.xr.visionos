#if INCLUDE_UNITY_XR_HANDS || PACKAGE_DOCS_GENERATION

using System;
using UnityEngine.XR.Hands;

namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// Continues <c>XRHandJointID</c> with new, platform-specific
    /// values. Pass this to to <c>xrHand.GetVisionOSJoint()</c>
    /// with an <c>XRHand</c> retrieved from an <c>XRHandSubsystem</c>.
    /// </summary>
    public enum VisionOSHandJointID
    {
        /// <summary>
        /// visionOS-specific joint for forearm-wrist.
        /// </summary>
        ForearmWrist = XRHandJointID.EndMarker,

        /// <summary>
        /// visionOS-specific joint for forearm-arm.
        /// </summary>
        ForearmArm,
    }

    /// <summary>
    /// Defines extension methods for platform-specific hand data.
    /// </summary>
    public static class VisionOSHandExtensions
    {
        internal class VisionOSHand
        {
            internal XRHandJoint GetJoint(VisionOSHandJointID jointID) => m_Joints[jointID.ToIndex() - XRHandJointID.EndMarker.ToIndex()];
            internal void SetJoint(XRHandJoint joint) => m_Joints[joint.id.ToIndex() - XRHandJointID.EndMarker.ToIndex()] = joint;

            XRHandJoint[] m_Joints = new XRHandJoint[k_NumVisionOSJoints];
        }

        /// <summary>
        /// Call <c>.ToIndex()</c> on a <see cref="VisionOSHandJointID"/> to get its
        /// corresponding index into an array of joint data.
        /// </summary>
        /// <param name="jointId">ID of the joint to convert to an index.</param>
        /// <returns>
        /// The index matching the ID passed in.
        /// </returns>
        public static int ToIndex(this VisionOSHandJointID jointId) => (int)jointId - 1;

        /// <summary>
        /// Call this to get the corresponding <see cref="VisionOSHandJointID"/> from
        /// an index into an array of associated data.
        /// </summary>
        /// <param name="index">Index to convert to an ID.</param>
        /// <returns>
        /// The ID matching the index passed in.
        /// </returns>
        public static VisionOSHandJointID FromIndex(int index) => (VisionOSHandJointID)(index + 1);

        /// <summary>
        /// Gets the pose of the joint, if available, but without the
        /// Unity-defined change to the rotation to make the reported
        /// rotation cross-platform.
        /// </summary>
        /// <param name="joint">
        /// The joint this extension method extends. To call this extension
        /// method, write it like
        /// <c>myJoint.TryGetVisionOSRotation(out var rotation)</c>.
        /// </param>
        /// <param name="rotation">
        /// If this method returns <see langword="true"/>, this will be
        /// populated with the Apple-defined rotation for the given joint, but
        /// still converted to Unity space.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if successful and the rotation was available,
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryGetVisionOSRotation(this XRHandJoint joint, out Quaternion rotation)
        {
            var rotations = joint.handedness == Handedness.Left ? s_LeftHandRotations : s_RightHandRotations;
            var nullableRotation = rotations[joint.id.ToIndex()];
            rotation = nullableRotation ?? Quaternion.identity;
            return nullableRotation.HasValue;
        }

        /// <summary>
        /// Retrieves a visionOS-specific hand joint by its ID.
        /// </summary>
        /// <param name="hand">
        /// The hand this extension method extends. To call this extension
        /// method, write it like
        /// <c>myHand.GetVisionOSJoint(VisionOSHandJointID.ForearmArm)</c>.
        /// </param>
        /// <param name="jointID">
        /// ID of the required joint.
        /// </param>
        /// <returns>
        /// The <c>XRHandJoint</c> corresponding the ID passed in.
        /// </returns>
        /// <exception>
        /// Will throw an exception if <paramref name="jointID"/> is not a named
        /// value of <see cref="VisionOSHandJointID"/>. Will also throw an exception
        /// if <paramref name="hand"/> was not retrieved from an <c>XRHandSubsystem</c>.
        /// </exception>
        public static XRHandJoint GetVisionOSJoint(this XRHand hand, VisionOSHandJointID jointID)
        {
            return hand.handedness switch
            {
                Handedness.Left => leftHand.GetJoint(jointID),
                Handedness.Right => rightHand.GetJoint(jointID),
                _ => throw new ArgumentException(
                    "Invalid hand - only use XRHands that are retrieved from either the leftHand or rightHand property of an XRHandSubsystem!")
            };
        }

        internal static VisionOSHand leftHand { get; } = new();
        internal static VisionOSHand rightHand { get; } = new();

        internal const int k_NumVisionOSJoints = 2;

        internal static Quaternion?[] GetVisionOSRotations(Handedness handedness) => handedness == Handedness.Left ? s_LeftHandRotations : s_RightHandRotations;

        // extra +2 for both for the visionOS-specific joints (forearm-wrist and forearm-arm)
        static Quaternion?[] s_LeftHandRotations = new Quaternion?[XRHandJointID.EndMarker.ToIndex() + k_NumVisionOSJoints];
        static Quaternion?[] s_RightHandRotations = new Quaternion?[XRHandJointID.EndMarker.ToIndex() + k_NumVisionOSJoints];
    }
}
#endif // INCLUDE_UNITY_XR_HANDS || PACKAGE_DOCS_GENERATION
