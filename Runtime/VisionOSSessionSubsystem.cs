using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// VisionOS implementation of the <c>XRSessionSubsystem</c>. Do not create this directly. Use the <c>SubsystemManager</c> instead.
    /// </summary>
    [Preserve]
    public class VisionOSSessionSubsystem : XRSessionSubsystem
    {
        internal const string sessionSubsystemId = "VisionOS-Session";
        
#if UNITY_VISIONOS && !UNITY_EDITOR
        const string k_LibraryName = "__Internal";
#else
        const string k_LibraryName = "arkit_stub";
#endif

        internal class VisionOSProvider : Provider
        {
            // TODO: How to get back to instance from callbacks?
            // TODO: How to connect subsystems together without singleton?
            static readonly NativeApi_Session.AR_Session_Run_Providers_Completion_Handler k_SessionCompletionHandler = SessionCompletionHandler;

            public static VisionOSProvider Instance { get; private set; }

            readonly IntPtr m_Self;

            IntPtr m_ProviderCollection;
            IntPtr m_SceneReconstructionProvider = IntPtr.Zero;
            bool m_SessionRunning;

            AR_Scene_Reconstruction_Mode m_SceneReconstructionMode = AR_Scene_Reconstruction_Mode.Default;

            public IntPtr WorldTrackingProvider { get; }

            // ReSharper disable InconsistentNaming
            public VisionOSProvider()
            {
                Instance = this;
                m_Self = NativeApi_Session.ar_session_create();

                NativeApi_Session.UnityVisionOS_impl_ar_session_set_data_provider_state_change_handler(m_Self, k_SessionCompletionHandler);

                m_ProviderCollection = NativeApi_Data_Provider.ar_data_providers_create();

                WorldTrackingProvider = UnityVisionOSCreateWorldTrackingProvider();

                UpdateSceneReconstructionProvider();

                // AddDataProvider will start the session
                AddDataProvider(WorldTrackingProvider);
            }

            public void SetSceneReconstructionMode(AR_Scene_Reconstruction_Mode mode)
            {
                if (m_SceneReconstructionMode == mode)
                    return;

                m_SceneReconstructionMode = mode;
                UpdateSceneReconstructionProvider();
            }

            public AR_Scene_Reconstruction_Mode GetSceneReconstructionMode()
            {
                return m_SceneReconstructionMode;
            }

            void UpdateSceneReconstructionProvider()
            {
                if (m_SceneReconstructionProvider != IntPtr.Zero)
                    RemoveDataProvider(m_SceneReconstructionProvider);

                m_SceneReconstructionProvider = UnityVisionOSCreateSceneReconstructionProvider(m_SceneReconstructionMode);
                AddDataProvider(m_SceneReconstructionProvider);
            }

            public override void Start() { }

            public override void Stop() => NativeApi_Session.ar_session_stop_all_data_providers(m_Self);

            public override Promise<SessionAvailability> GetAvailabilityAsync() => Promise<SessionAvailability>.CreateResolvedPromise(SessionAvailability.Supported | SessionAvailability.Installed);

            public override TrackingState trackingState => m_SessionRunning ? TrackingState.Tracking : TrackingState.None;

            public void AddDataProvider(IntPtr dataProvider)
            {
                NativeApi_Data_Provider.ar_data_providers_add_data_provider(m_ProviderCollection, dataProvider);
                NativeApi_Session.ar_session_run(m_Self, m_ProviderCollection);
            }

            public void RemoveDataProvider(IntPtr dataProvider)
            {
                NativeApi_Data_Provider.ar_data_providers_remove_data_provider(m_ProviderCollection, dataProvider);
                NativeApi_Session.ar_session_run(m_Self, m_ProviderCollection);
            }

            [MonoPInvokeCallback(typeof(NativeApi_Session.AR_Session_Run_Providers_Completion_Handler))]
            static void SessionCompletionHandler(IntPtr data_providers, AR_Data_Provider_State new_state, IntPtr error, IntPtr failed_provider)
            {
                // TODO: read error code
                Instance.m_SessionRunning = true;
            }

            // ReSharper restore InconsistentNaming
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRSessionSubsystemDescriptor.RegisterDescriptor(new XRSessionSubsystemDescriptor.Cinfo
            {
                id = sessionSubsystemId,
                providerType = typeof(VisionOSProvider),
                subsystemTypeOverride = typeof(VisionOSSessionSubsystem),
                supportsInstall = false,
                supportsMatchFrameRate = false
            });
        }

        [DllImport(k_LibraryName, EntryPoint = "UnityVisionOSCreateWorldTrackingProvider")]
        static extern IntPtr UnityVisionOSCreateWorldTrackingProvider();

        [DllImport(k_LibraryName, EntryPoint = "UnityVisionOSCreateSceneReconstructionProvider")]
        static extern IntPtr UnityVisionOSCreateSceneReconstructionProvider(AR_Scene_Reconstruction_Mode mode);
    }
}
