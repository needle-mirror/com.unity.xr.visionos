#if ENABLE_APP_LIFECYCLE_TESTS
using System;
using UnityEngine;

namespace UnityEngine.XR.VisionOSTests
{
    /// <summary>
    /// Intercept OnApplicationFocus and OnApplicationPause messages and forward them to <see cref="AppLifecycleTests"/>.
    /// </summary>
    class AppLifecycleTestBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Called whenever an OnApplicationFocus message is received.
        /// </summary>
        public Action<bool> ApplicationFocusReceived;

        /// <summary>
        /// Called whenever an OnApplicationPause message is received.
        /// </summary>
        public Action<bool> ApplicationPauseReceived;

        void OnApplicationFocus(bool focus)
        {
            ApplicationFocusReceived?.Invoke(focus);
        }

        void OnApplicationPause(bool pause)
        {
            ApplicationPauseReceived?.Invoke(pause);
        }
    }
}
#endif
