namespace UnityEngine.XR.VisionOS
{
    /// <summary>
    /// Stereo rendering mode.
    /// </summary>
    public enum RenderMode
    {
        /// <summary>
        /// Submit separate draw calls for each eye.
        /// </summary>
        MultiPass,

        /// <summary>
        /// Submit one draw call for both eyes.
        /// </summary>
        SinglePassInstanced
    }
}