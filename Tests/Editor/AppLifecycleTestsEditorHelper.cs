#if ENABLE_APP_LIFECYCLE_TESTS
#if UNITY_VISIONOS || UNITY_IOS || UNITY_TVOS
#define APPLE_NON_DESKTOP
#endif

using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR.VisionOSTests;
using Debug = UnityEngine.Debug;

#if APPLE_NON_DESKTOP
using UnityEditor.TestTools.TestRunner.Api;
#endif

// ReSharper disable MergeIntoPattern

namespace UnityEditor.XR.VisionOSTests
{
    /// <summary>
    /// Automatically run app-lifecycle-test-helper.py when running <see cref="AppLifecycleTests"/> in visionOS, iOS, and tvOS players.
    /// </summary>
    /// <remarks>
    /// We rely on Xcode's `simctl` tool to periodically try to wake (launch) the app while the test is running in the visionOS simulator. There is no way to
    /// implement this test on a physical device. This is far from ideal, but so far the approach has been pretty stable, now that it's working. Just be careful
    /// about modifying the durations of the various delays and timeouts here and in the python script. We want to make sure that Unity isn't waiting too long
    /// for the python script, but we also don't want to spam the simulator with launch commands.
    /// Note that this doesn't work for standalone UTR runs, which means we can't take advantage of this helper on CI. Instead, we use an extra command to start
    /// up the script before calling UTR to run tests. This is why the tests are guarded behind #if ENABLE_APP_LIFECYCLE_TESTS. They won't work in isolation, and
    /// should be skipped outside our package CI pipeline. This utility is still helpful for manual test runs when iterating with the Test Runner.
    /// The Python script in the Tests/CLI~ folder contains more detailed documentation, but here's what we're doing at a high level:
    /// - Run the script using python3 with the app's bundle identifier as its only argument.
    /// - Every 10 seconds the script will do the following:
    ///   - Check if the app is running by executing `ps aux | pgrep -fl  [app_name]`; if not, early-out and wait another 10 seconds.
    ///   - If the app is running, execute `xcrun simctl launch booted [bundle_id]` to bring it to the foreground.
    ///   - If the app is not running, and we've done at least one cycle of foregrounding (HAS_LAUNCHED = true), terminate the python script.
    /// </remarks>
#if APPLE_NON_DESKTOP
    [InitializeOnLoad]
#endif
    // ReSharper disable once ClassNeverInstantiated.Global
    class AppLifecycleTestsEditorHelper
    {
#if APPLE_NON_DESKTOP
        /// <summary>
        /// Handler for test callbacks. Calls <see cref="AppLifecycleTestsEditorHelper.StartPythonScript"/> when the <see cref="AppLifecycleTests"/> suite is
        /// started, and <see cref="AppLifecycleTestsEditorHelper.StopPythonScript"/> when it finishes.
        /// </summary>
        class TestRunCallback : ICallbacks
        {
            static readonly string k_TargetedTestSuite = typeof(AppLifecycleTests).FullName;
            bool m_IsPlayerRun;

            public void RunStarted(ITestAdaptor testsToRun)
            {
                // When TestMode is PlayMode, but we're not in play mode, it's a Player test run.
                if (Application.isPlaying || testsToRun.TestMode == TestMode.EditMode)
                    return;

                m_IsPlayerRun = true;
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                // Do nothing; we should have stopped the script when the suite is finished.
            }

            public void TestStarted(ITestAdaptor test)
            {
                if (!m_IsPlayerRun)
                    return;

                if (test.FullName != k_TargetedTestSuite)
                    return;

                StartPythonScript();
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (!m_IsPlayerRun)
                    return;

                if (result.FullName != k_TargetedTestSuite)
                    return;

                StopPythonScript();
            }
        }

        /// <summary>
        /// Register the callback receiver using <see cref="InitializeOnLoadAttribute"/>
        /// </summary>
        static AppLifecycleTestsEditorHelper()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            testRunnerApi.RegisterCallbacks(new TestRunCallback());
        }
#endif

        // Store the process handle so it can be forcibly closed if needed.
        static Process s_HelperProcess;

        /// <summary>
        /// Start the app-lifecycle-test-helper.py python script.
        /// </summary>
        [MenuItem("Tests/Run App Lifecycle Helper")]
        static void StartPythonScript()
        {
            if (s_HelperProcess != null && !s_HelperProcess.HasExited)
            {
                Debug.Log("App lifecycle helper script is already running.");
                return;
            }

            // Create and start the process (similar to the previous example)
            var path = FileUtil.GetPhysicalPath("Packages/com.unity.xr.visionos/Tests/CLI~/app-lifecycle-test-helper.py");
            var bundleId = PlayerSettings.applicationIdentifier;
            var appName = $"{PlayerSettings.productName}.app";
            var command = "/usr/bin/python3";
            var arguments = $"{path} {appName} {bundleId}";

            Debug.Log($"Running test helper with the following command:\n{command} {arguments}");
            s_HelperProcess = new Process();
            s_HelperProcess.StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false,
            };

            s_HelperProcess.OutputDataReceived += LogToUnity;
            s_HelperProcess.ErrorDataReceived += LogErrorToUnity;

            s_HelperProcess.Start();
            s_HelperProcess.BeginOutputReadLine();
            s_HelperProcess.BeginErrorReadLine();
        }

        /// <summary>
        /// Stop the app-lifecycle-test-helper.py script, if it's running.
        /// </summary>
        [MenuItem("Tests/Stop App Lifecycle Helper")]
        static void StopPythonScript()
        {
            if (s_HelperProcess != null && !s_HelperProcess.HasExited)
            {
                s_HelperProcess.Kill();
                s_HelperProcess.Dispose();
                s_HelperProcess = null;
                Debug.Log("App lifecycle helper script stopped.");
            }
        }

        static void LogToUnity(object _, DataReceivedEventArgs dataReceivedEventArgs)
        {
            var message = dataReceivedEventArgs.Data;
            if (!string.IsNullOrEmpty(message))
            {
                Debug.Log(message);
            }
        }

        static void LogErrorToUnity(object _, DataReceivedEventArgs dataReceivedEventArgs)
        {
            var message = dataReceivedEventArgs.Data;
            if (!string.IsNullOrEmpty(message))
            {
                Debug.LogError(message);
            }
        }
    }
}
#endif
