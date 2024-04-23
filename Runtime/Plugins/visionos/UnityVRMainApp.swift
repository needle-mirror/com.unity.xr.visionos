import CompositorServices
import SwiftUI
import UnityFramework

@main
struct UnityVRMain: App {
    @UIApplicationDelegateAdaptor
    var swiftUIdelegate: UnitySwiftUIAppDelegate

    var body: some Scene {
        unityVisionOSCompositorSpace
    }
}
