#if ENABLE_APP_LIFECYCLE_TESTS
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
#define MACOS
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#define WINDOWS
#endif

#if WINDOWS
using System;
using System.Diagnostics;
#endif

#if MACOS || WINDOWS
using System.Threading;
using System.Runtime.InteropServices;
#else
using UnityEngine;
#endif

namespace UnityEngine.XR.VisionOSTests
{
    /// <summary>
    /// Utility methods for foregrounding and backgrounding the app to facilitate app lifecycle tests.
    /// </summary>
    /// <remarks>
    /// Each platform has a simple way for the app to background itself. Either there is a system call like `ShowWindow` or we can just use `Application.OpenURL`.
    /// Bringing the app back to the foreground is a little more complicated. We use a helper thread for desktop platforms and a python script for iOS and
    /// visionOS. The reason we need this is that we can't count on the test itself to bring the app to the foreground. In most cases the app will be paused
    /// (except in the Editor) after we send it to the background, so the test simply doesn't move on to the next step. This is why we need to use a thread
    /// on desktop platforms. For iOS and visionOS, there is no API or mechanism for the app to bring itself to the foreground. We use an external python script
    /// to bring the app back to the foreground. Refer to AppLifecycleTestsEditorHelper for more information.
    /// </remarks>
    static class AppLifecycleTestsHelper
    {
#if MACOS || WINDOWS
        const int k_ForegroundDelayInMilliseconds = 5000;
        static Thread s_ForegroundThread;

#if MACOS
        /// <summary>
        /// Hides the current application.
        /// </summary>
        /// <code>
        /// [[NSApplication sharedApplication] hide:nil]
        /// </code>
        [DllImport("libUnityLifecycleTestsHelper")]
        public static extern void BackgroundCurrentApplication();

        /// <summary>
        /// Activates the current application.
        /// </summary>
        /// <code>
        /// dispatch_async(dispatch_get_main_queue(), ^{
        ///     [[NSApplication sharedApplication] activateIgnoringOtherApps:YES];
        /// });
        /// </code>
        [DllImport("libUnityLifecycleTestsHelper")]
        static extern void ForegroundCurrentApplication();
#elif WINDOWS
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // ReSharper disable once InconsistentNaming
        const int SW_MINIMIZE = 6;
        // ReSharper disable once InconsistentNaming
        const int SW_RESTORE = 9;

        /// <summary>
        /// Minimizes the main window of the current application.
        /// </summary>
        public static void BackgroundCurrentApplication()
        {
            var currentProcess = Process.GetCurrentProcess();
            var handle = currentProcess.MainWindowHandle;

            if (handle == IntPtr.Zero)
            {
                Debug.LogError("VisionOS App Lifecycle Tests failed to get current window handle");
                return;
            }

            ShowWindow(handle, SW_MINIMIZE);
        }

        /// <summary>
        /// Restores (maximizes) the main window of the current application.
        /// </summary>
        static void ForegroundCurrentApplication()
        {
            var currentProcess = Process.GetCurrentProcess();
            var handle = currentProcess.MainWindowHandle;

            if (handle == IntPtr.Zero)
            {
                Debug.LogError("VisionOS App Lifecycle Tests failed to get current window handle");
                return;
            }

            ShowWindow(handle, SW_RESTORE);
        }
#endif

        /// <summary>
        /// Start a helper thread which will call <see cref="ForegroundCurrentApplication"/> every 5 seconds.
        /// </summary>
        public static void StartForegroundHelperThread()
        {
            if (s_ForegroundThread != null)
                return;

            s_ForegroundThread = new Thread(() =>
            {
                while (s_ForegroundThread != null)
                {
                    Thread.Sleep(k_ForegroundDelayInMilliseconds);
                    ForegroundCurrentApplication();
                }

            });

            s_ForegroundThread.Start();
        }

        /// <summary>
        /// Stop the thread started in <see cref="StartForegroundHelperThread"/> if it is still running.
        /// </summary>
        public static void StopForegroundHelperThread()
        {
            if (s_ForegroundThread == null)
                return;

            // Call Abort, just in case
            s_ForegroundThread.Abort();
            s_ForegroundThread = null;
        }
#else
        /// <summary>
        /// Send the current application to the background by opening a browser (<see cref="Application.OpenURL"/>) pointed at http://localhost/.
        /// </summary>
        public static void BackgroundCurrentApplication()
        {
            Application.OpenURL("https://localhost/");
        }
#endif
    }
}
#endif
