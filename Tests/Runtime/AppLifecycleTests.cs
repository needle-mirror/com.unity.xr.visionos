#if ENABLE_APP_LIFECYCLE_TESTS
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
#define MACOS
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#define WINDOWS
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObject = UnityEngine.Object;

namespace UnityEngine.XR.VisionOSTests
{
    /// <summary>
    /// Ensure that OnApplicationFocus and OnApplicationPause are called in the right order depending on the platform/version. See
    /// <see cref="AppLifecycleTestsHelper"/> and AppLifecycleEditorTestsHelper for more details.
    /// </summary>
    class AppLifecycleTests
    {
        /// <summary>
        /// The type of callback received.
        /// </summary>
        enum CallbackType
        {
            Focus,
            Pause
        }

        /// <summary>
        /// Container struct for logging app lifecycle events.
        /// </summary>
        struct CallbackEvent
        {
            public CallbackType CallbackType;
            public bool Argument;
        }

        // Don't wait more than a minute for Application.isFocused to be true.
        const float k_WaitForFocusTimeout = 60f;

        // Try to re-run the test in case we missed a message due to bad timing.
        const int k_RerunCount = 3;

#if UNITY_EDITOR
        // Grab the UnityEditor.GameView type so we can show the Game View programmatically.
        static readonly Type k_GameViewType = Type.GetType("UnityEditor.GameView, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
#endif

        // List of events intercepted by AppLifecycleTestBehaviour
        readonly List<CallbackEvent> m_CallbackEvents = new();

        // Stores the state of Application.runInBackground before running tests so that it can be restored after.
        bool m_RunInBackgroundWasEnabled;

        // A MonoBehaviour that will forward OnApplicationFocus and OnApplicationPause messages to the test.
        AppLifecycleTestBehaviour m_TestBehaviour;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (!ExpectFocusMessages() && !ExpectPauseMessages())
                return;

            m_TestBehaviour = new GameObject("AppLifecycleTest Object").AddComponent<AppLifecycleTestBehaviour>();
            m_TestBehaviour.ApplicationFocusReceived += OnApplicationFocusReceived;
            m_TestBehaviour.ApplicationPauseReceived += ApplicationPauseReceived;

            m_RunInBackgroundWasEnabled = Application.runInBackground;
            Application.runInBackground = false;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (!ExpectFocusMessages() && !ExpectPauseMessages())
                return;

#if MACOS || WINDOWS
            AppLifecycleTestsHelper.StopForegroundHelperThread();
#endif

            Application.runInBackground = m_RunInBackgroundWasEnabled;

            if (m_TestBehaviour)
                UnityObject.Destroy(m_TestBehaviour.gameObject);
        }

        void ApplicationPauseReceived(bool paused)
        {
            m_CallbackEvents.Add(new CallbackEvent{ CallbackType = CallbackType.Pause, Argument = paused });
        }

        void OnApplicationFocusReceived(bool focused)
        {
            m_CallbackEvents.Add(new CallbackEvent{ CallbackType = CallbackType.Focus, Argument = focused });
        }

        /// <summary>
        /// Ensure that OnApplicationFocus messages are sent, if expected, when the app is sent to the background and brought back to the foreground.
        /// </summary>
        /// <returns>Coroutine enumerator.</returns>
        [UnityTest]
        public IEnumerator OnApplicationFocusCalledOnBackgroundAndForeground()
        {
            if (!ExpectFocusMessages())
                Assert.Ignore("We don't expect to see OnApplicationFocus messages in this version/context.");

            var expectPauseMessages = ExpectPauseMessages();
            var expectedEventCount = 2;
            if (expectPauseMessages)
                expectedEventCount += 2;

            // Run the test
            const string testName = nameof(OnApplicationFocusCalledOnBackgroundAndForeground);
            foreach (var (abortMessage, yieldInstruction) in CommonTestMethodWithRerun(testName, expectedEventCount))
            {
                if (!string.IsNullOrEmpty(abortMessage))
                    Assert.Ignore(abortMessage);

                yield return yieldInstruction;
            }

            try
            {
                // Interpret the results
                Assert.AreEqual(expectedEventCount, m_CallbackEvents.Count, $"Did not receive all {expectedEventCount} events.");
                Assert.AreEqual(CallbackType.Focus, m_CallbackEvents[0].CallbackType, "First event type was not Focus.");
                Assert.IsFalse(m_CallbackEvents[0].Argument, "First event was not focused = false.");

                // Standalone and editor events come in a different order from iOS and visionOS
#if UNITY_EDITOR || UNITY_STANDALONE

                // Expected order: Focus(false), Pause(true), Pause(false), Focus(true)
                var secondFocusEventIndex = 3;
#else
                // Expected order: Focus(false), Pause(true), Focus(true), Pause(false)
                var secondFocusEventIndex = 2;
#endif

                if (!expectPauseMessages)
                    secondFocusEventIndex = 1;

                var secondFocusEvent = m_CallbackEvents[secondFocusEventIndex];
                Assert.AreEqual(CallbackType.Focus, secondFocusEvent.CallbackType, $"Event type at index {secondFocusEventIndex} should be Focus.");
                Assert.IsTrue(secondFocusEvent.Argument, $"Event at index {secondFocusEventIndex} should be focused = true.");
            }
            catch (Exception)
            {
                PrintCapturedEvents(testName);
                throw;
            }
        }

        /// <summary>
        /// Ensure that OnApplicationPause messages are sent, if expected, when the app is sent to the background and brought back to the foreground.
        /// </summary>
        /// <returns>Coroutine enumerator.</returns>
        [UnityTest]
        public IEnumerator OnApplicationPausedCalledOnBackgroundAndForeground()
        {
            if (!ExpectPauseMessages())
                Assert.Ignore("We don't expect to see OnApplicationPause messages in this version/context.");

            var expectFocusMessages = ExpectFocusMessages();
            var expectedEventCount = 2;
            if (expectFocusMessages)
                expectedEventCount += 2;

            // Run the test
            const string testName = nameof(OnApplicationPausedCalledOnBackgroundAndForeground);
            foreach (var (abortMessage, yieldInstruction) in CommonTestMethodWithRerun(testName, expectedEventCount))
            {
                if (!string.IsNullOrEmpty(abortMessage))
                    Assert.Ignore(abortMessage);

                yield return yieldInstruction;
            }

            try
            {
                // Interpret the results
                Assert.AreEqual(expectedEventCount, m_CallbackEvents.Count, $"Did not receive all {expectedEventCount} events.");

                var firstPauseIndex = 0;
                if (expectFocusMessages)
                    firstPauseIndex = 1;

                Assert.AreEqual(CallbackType.Pause, m_CallbackEvents[firstPauseIndex].CallbackType, $"Event type at index {firstPauseIndex} should be Pause.");
                Assert.IsTrue(m_CallbackEvents[firstPauseIndex].Argument, $"Event at index {firstPauseIndex} should be paused = true.");

                // Standalone and editor events come in a different order from iOS and visionOS
#if UNITY_EDITOR || UNITY_STANDALONE

                // Expected order: Focus(false), Pause(true), Pause(false), Focus(true)
                var secondPauseEventIndex = 2;
#else
                // Expected order: Focus(false), Pause(true), Focus(true), Pause(false)
                var secondPauseEventIndex = 3;
#endif

                if (!expectFocusMessages)
                    secondPauseEventIndex = 1;

                Assert.AreEqual(CallbackType.Pause, m_CallbackEvents[secondPauseEventIndex].CallbackType,
                    $"Event type at index {secondPauseEventIndex} should be Pause.");
                Assert.IsFalse(m_CallbackEvents[secondPauseEventIndex].Argument, $"Event at index {secondPauseEventIndex} should be paused = false.");
            }
            catch (Exception)
            {
                PrintCapturedEvents(testName);
                throw;
            }
        }

        IEnumerable<(string, YieldInstruction)> CommonTestMethodWithRerun(string testName, int expectedEventCount)
        {
            // Rerun the test a few times in case we missed a message due to bad timing.
            for (var i = 0; i < k_RerunCount; i++)
            {
                // Run the test
                foreach (var tuple in CommonTestMethod(nameof(OnApplicationFocusCalledOnBackgroundAndForeground)))
                {
                    yield return tuple;
                }

                // If we have the correct number of events, we're done
                if (m_CallbackEvents.Count == expectedEventCount)
                    break;

                Debug.Log($"Run {i} of {testName} failed. Event count was {m_CallbackEvents.Count}. Expected {expectedEventCount}. Retrying.");
            }
        }

        IEnumerable<(string, YieldInstruction)> CommonTestMethod(string testName)
        {
#if MACOS || WINDOWS
            AppLifecycleTestsHelper.BackgroundCurrentApplication();
            AppLifecycleTestsHelper.StartForegroundHelperThread();
#endif

#if UNITY_EDITOR
            // For some reason isPaused keeps getting set in between test runs which interferes with these events in the Editor
            EditorApplication.isPaused = false;

            // Show the GameView window if we're in batch mode so that foreground/background takes effect
            if (Application.isBatchMode)
            {
                if (k_GameViewType == null)
                    yield return ("Failed to show game view in batch mode", null);

                EditorWindow.GetWindow(k_GameViewType);
            }
#endif

            // Let things settle for a few seconds (one second isn't enough)
            yield return (null, new WaitForSeconds(5));

#if MACOS || WINDOWS
            // Start in background to ensure app is unpaused when brought to the foreground; there was a mysterious bug where the app was paused even though it
            // was in the foreground. The delay between SetUp and the actual test can be too long for this to work properly
            AppLifecycleTestsHelper.BackgroundCurrentApplication();
            yield return (null, new WaitForSeconds(3));
#endif

            var startTime = Time.realtimeSinceStartup;
            while (!Application.isFocused)
            {
                yield return (null, null);
                FailIfWaitTimeExceeded(startTime, testName);
            }

            m_CallbackEvents.Clear();
            Assert.AreEqual(0, m_CallbackEvents.Count, "Callback list was not cleared.");

            AppLifecycleTestsHelper.BackgroundCurrentApplication();

            // In most cases, the test won't be ticked, but we need to wait a few seconds for the app to background.
            yield return (null, new WaitForSeconds(3));

#if UNITY_EDITOR
            // Editor continues to tick the test the background even when runInBackground is false
            // We need to wait for focus for the test to pass properly
            startTime = Time.realtimeSinceStartup;
            while (!Application.isFocused)
            {
                yield return (null, null);
                FailIfWaitTimeExceeded(startTime, testName);
            }
#endif

#if MACOS || WINDOWS
            AppLifecycleTestsHelper.StopForegroundHelperThread();
#endif
        }

        void PrintCapturedEvents(string testName)
        {
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0} failed (see assert message above). Captured events were:", testName);
            for(var i = 0; i < m_CallbackEvents.Count; i++)
            {
                var callbackEvent = m_CallbackEvents[i];
                const string formatString = "Event {0} is {1}({2})";
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, formatString, i, callbackEvent.CallbackType, callbackEvent.Argument);
            }
        }

        /// <summary>
        /// Call <see cref="Assert.Fail(string)"/> if the difference between <see cref="Time.realtimeSinceStartup"/> and <see cref="startTime"/> has exceeded 60 seconds.
        /// </summary>
        /// <param name="startTime">The value of <see cref="Time.realtimeSinceStartup"/> when we started waiting.</param>
        /// <param name="testName">The name of the test which is calling this method. This is used in the failure message.</param>
        static void FailIfWaitTimeExceeded(float startTime, string testName)
        {
            if (Time.realtimeSinceStartup - startTime > k_WaitForFocusTimeout)
                Assert.Fail($"{testName} timed out waiting for focus after {k_WaitForFocusTimeout} seconds.");
        }

#if UNITY_VISIONOS && !UNITY_EDITOR
        /// <summary>
        /// Returns true if the app is currently rendering with CompositorServices (Metal or Hybrid <see cref="VisionOSSettings.AppMode"/>)
        /// </summary>
        /// <returns>True if the app is currently rendering with CompositorServices</returns>
        static bool IsMetalMode()
        {
            return VisionOS.VisionOS.GetLayerRenderer() != IntPtr.Zero;
        }
#endif

        /// <summary>
        /// Returns true if we expect to receive OnApplicationFocus messages.
        /// </summary>
        /// <remarks>
        /// This will depend on the Unity version and app mode. Some earlier versions do not provide any lifecycle messages, and some only provide one type or the other.
        /// </remarks>
        /// <returns>True if we expect to receive OnApplicationFocus messages.</returns>
        static bool ExpectFocusMessages()
        {
#if (MACOS || WINDOWS) && UNITY_6000_2_OR_NEWER
            // TODO: LXR-4674 fix lifecycle tests in CI in 6000.2
            // Return true here to test locally--the issue appears to be related to permissions dialogs which you can dismiss manually
            return false;
#elif UNITY_STANDALONE_WIN
            // TODO: LXR-4682 fix app lifecycle tests in CI on Windows Standalone in 6000.0
            // Return true here to test locally--the issue appears to be related to permissions dialogs which you can dismiss manually
            return false;
#else
#if UNITY_VISIONOS && !UNITY_EDITOR
            // RealityKit and Hybrid modes require versions that support ShouldRunInBackground
#if UNITY_VISIONOS_RUN_IN_BACKGROUND_SUPPORT
            return true;
#elif UNITY_VISIONOS_METAL_PAUSE_MESSAGE_SUPPORT
            return IsMetalMode();
#else
            return false;
#endif
#else
            return true;
#endif
#endif
        }

        /// <summary>
        /// Returns true if we expect to receive OnApplicationPause messages.
        /// </summary>
        /// <remarks>
        /// This will depend on the Unity version and app mode. Some earlier versions do not provide any lifecycle messages, and some only provide one type or the other.
        /// </remarks>
        /// <returns>True if we expect to receive OnApplicationPause messages.</returns>
        static bool ExpectPauseMessages()
        {
#if (MACOS || WINDOWS) && UNITY_6000_2_OR_NEWER
            // TODO: LXR-4674 fix lifecycle tests in CI in 6000.2
            // Return true here to test locally--the issue appears to be related to permissions dialogs which you can dismiss manually
            return false;
#elif UNITY_STANDALONE_WIN
            // TODO: LXR-4682 fix app lifecycle tests in CI on Windows Standalone in 6000.0
            // Return true here to test locally--the issue appears to be related to permissions dialogs which you can dismiss manually
            return false;
#else
#if UNITY_VISIONOS && !UNITY_EDITOR
            // RealityKit and Hybrid modes require versions that support ShouldRunInBackground
#if UNITY_VISIONOS_RUN_IN_BACKGROUND_SUPPORT
            return true;
#elif UNITY_VISIONOS_METAL_PAUSE_MESSAGE_SUPPORT
            return IsMetalMode();
#else
            return IsMetalMode();
#endif
#else
            return true;
#endif
#endif
        }
    }
}
#endif
