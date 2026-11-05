using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.XR.VisionOS.InputDevices;

namespace UnityEditor.XR.VisionOSTests
{
    /// <summary>
    /// Validates that the spatial pointer listener gets a new input device after each Play mode session.
    /// </summary>
    [TestFixture]
    class SpatialPointerEventListenerTests
    {
        [SetUp]
        public void SetUp()
        {
            // Ensure tests start with a clean slate
            VisionOSSpatialPointerEventListener.EditorApplicationOnplayModeStateChanged(PlayModeStateChange.ExitingPlayMode);
        }

        [TearDown]
        public void TearDown()
        {
            VisionOSSpatialPointerEventListener.EditorApplicationOnplayModeStateChanged(PlayModeStateChange.ExitingPlayMode);
        }

        [Test]
        public void OnInputEvent_AfterExitingPlayMode_AddsNewPointerDevice()
        {
            VisionOSSpatialPointerEventListener.OnInputEvent(default);
            Assert.AreEqual(1, CountPointerDevices());

            VisionOSSpatialPointerEventListener.EditorApplicationOnplayModeStateChanged(PlayModeStateChange.ExitingPlayMode);
            Assert.AreEqual(0, CountPointerDevices());

            VisionOSSpatialPointerEventListener.OnInputEvent(default);
            Assert.AreEqual(1, CountPointerDevices());
        }

        static int CountPointerDevices()
        {
            var count = 0;
            foreach (var device in InputSystem.devices)
            {
                if (device is VisionOSSpatialPointerDevice)
                    count++;
            }

            return count;
        }
    }
}
