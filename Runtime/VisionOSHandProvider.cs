#if INCLUDE_UNITY_XR_HANDS
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.VisionOS
{
    public class VisionOSHandProvider : XRHandSubsystemProvider
    {
        internal const string handSubsystemId = "VisionOS-Hands";
        const int k_JointNameCount = 28;

        string[] m_JointNames;
        IntPtr m_HandTrackingConfiguration;
        IntPtr m_HandTrackingProvider;

        XRHandSubsystem.UpdateSuccessFlags m_LastSuccessFlags = XRHandSubsystem.UpdateSuccessFlags.None;

        public override unsafe void Start()
        {
            if (m_JointNames == null)
            {
                var jointNames = NativeApi_Hand_Tracking.UnityVisionOS_impl_get_joint_names();
                m_JointNames = new string[k_JointNameCount];
                for (var i = 0; i < k_JointNameCount; i++)
                {
                    m_JointNames[i] = Marshal.PtrToStringAnsi((IntPtr)jointNames[i]);
                }
            }

            m_HandTrackingConfiguration = NativeApi_Hand_Tracking.ar_hand_tracking_configuration_create();
            m_HandTrackingProvider = NativeApi_Hand_Tracking.ar_hand_tracking_provider_create(m_HandTrackingConfiguration);
            VisionOSSessionSubsystem.VisionOSProvider.Instance.AddDataProvider(m_HandTrackingProvider);
        }

        public override void Stop()
        {
            VisionOSSessionSubsystem.VisionOSProvider.Instance.RemoveDataProvider(m_HandTrackingProvider);
        }

        public override void Destroy()
        {
        }

        public override void GetHandLayout(NativeArray<bool> handJointsInLayout)
        {
            // All joints except palm are supported
            for (var i = 0; i < handJointsInLayout.Length; i++)
            {
                handJointsInLayout[i] = (XRHandJointID)i != XRHandJointID.Palm;
            }
        }

        /// <inheritdoc/>
        public override XRHandSubsystem.UpdateSuccessFlags TryUpdateHands(
            XRHandSubsystem.UpdateType updateType,
            ref Pose leftHandRootPose,
            NativeArray<XRHandJoint> leftHandJoints,
            ref Pose rightHandRootPose,
            NativeArray<XRHandJoint> rightHandJoints)
        {
            // TODO: Can we re-use these anchors? We should confirm that these are freed by ARC
            var leftHandAnchor = NativeApi_Hand_Tracking.ar_hand_anchor_create();
            var rightHandAnchor = NativeApi_Hand_Tracking.ar_hand_anchor_create();
            var success = NativeApi_Hand_Tracking.ar_hand_tracking_provider_get_latest_anchors(m_HandTrackingProvider, leftHandAnchor, rightHandAnchor);

            // get_latest_anchors will return false if we poll too quickly--in that case, return the last valid result
            if (!success)
                return m_LastSuccessFlags;

            m_LastSuccessFlags = XRHandSubsystem.UpdateSuccessFlags.None;
            GetHandData(ref leftHandRootPose, ref m_LastSuccessFlags, leftHandJoints, leftHandAnchor, Handedness.Left);
            GetHandData(ref rightHandRootPose, ref m_LastSuccessFlags, rightHandJoints, rightHandAnchor, Handedness.Right);
            return m_LastSuccessFlags;
        }

        void GetHandData(ref Pose rootPose, ref XRHandSubsystem.UpdateSuccessFlags successFlags, NativeArray<XRHandJoint> jointArray, IntPtr handAnchor, Handedness handedness)
        {
            var isTracked = NativeApi_Anchor.ar_trackable_anchor_is_tracked(handAnchor);
            if (!isTracked)
                return;

            var worldTransform = NativeApi_Anchor.ar_anchor_get_origin_from_anchor_transform(handAnchor);
            var convertedMatrix = NativeApi_Types.UnityVisionOS_impl_simd_float4x4_to_float_array(worldTransform);
            var worldMatrix = Marshal.PtrToStructure<FloatArrayToMatrix4x4>(convertedMatrix);
            var wristPosition = worldMatrix.GetPosition();
            var wristRotation = worldMatrix.GetRotation();

            rootPose = new Pose(wristPosition, wristRotation);
            successFlags |= handedness == Handedness.Left
                ? XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose
                : XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose;

            var skeleton = NativeApi_Hand_Tracking.ar_hand_anchor_get_skeleton(handAnchor);
            for (var jointID = XRHandJointID.BeginMarker; jointID < XRHandJointID.EndMarker; jointID++)
            {
                var index = jointID.ToIndex();
                var name = m_JointNames[index];
                var jointIsTracked = NativeApi_Skeleton.ar_skeleton_is_joint_tracked(skeleton, name);
                var trackingState = jointIsTracked ? XRHandJointTrackingState.Pose : XRHandJointTrackingState.None;
                if (jointID == XRHandJointID.Palm)
                    trackingState = XRHandJointTrackingState.WillNeverBeValid;

                var pose = Pose.identity;
                if (jointIsTracked)
                {
                    var jointTransformPtr = NativeApi_Skeleton.ar_skeleton_get_skeleton_root_transform_for_joint(skeleton, name);
                    convertedMatrix = NativeApi_Types.UnityVisionOS_impl_simd_float4x4_to_float_array(jointTransformPtr);
                    var jointMatrix = Marshal.PtrToStructure<FloatArrayToMatrix4x4>(convertedMatrix);
                    var jointPosition = wristPosition + wristRotation * jointMatrix.GetPosition();
                    var jointRotation = wristRotation * jointMatrix.GetRotation();
                    pose = new Pose(jointPosition, jointRotation);

                    successFlags |= handedness == Handedness.Left
                        ? XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints
                        : XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;
                }

#if INCLUDE_UNITY_XR_HANDS_1_1
                var joint = XRHandProviderUtility.CreateJoint(handedness, trackingState, jointID, pose);
#else
                var joint = XRHandProviderUtility.CreateJoint(trackingState, jointID, pose);
#endif
                jointArray[index] = joint;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Register()
        {
            var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
            {
                id = handSubsystemId,
                providerType = typeof(VisionOSHandProvider)
            };

            XRHandSubsystemDescriptor.Register(handsSubsystemCinfo);
        }
    }
}
#endif
