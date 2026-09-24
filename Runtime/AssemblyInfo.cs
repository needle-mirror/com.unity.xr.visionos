using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
// Unconditional, because the SpatialPointerEvent native-stride test runs in the visionOS player, not just
// the Editor, and needs the internal native binding there too.
[assembly: InternalsVisibleTo("Unity.XR.VisionOS.Tests")]
#if UNITY_EDITOR
[assembly: InternalsVisibleTo("Unity.XR.VisionOS.Editor.Tests")]
[assembly: InternalsVisibleTo("Unity.XR.VisionOS.Editor")]
#endif
