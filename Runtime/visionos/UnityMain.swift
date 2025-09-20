//
//  UnityMain.swift
//  Unity-RealityOS
//
//  Created by Peter Kuhn on 6/30/23.
//

import SwiftUI
import CompositorServices

@main
struct MyApp: App {
    @UIApplicationDelegateAdaptor
    var swiftUIdelegate: UnitySwiftUIAppDelegate

    struct UnityContentConfiguration: CompositorLayerConfiguration {
        let singlePass: Bool
        let enableFoveation: Bool
        
        func makeConfiguration(
                capabilities: LayerRenderer.Capabilities,
                configuration: inout LayerRenderer.Configuration
            ) {
                let supportsFoveation = capabilities.supportsFoveation
                let supportedLayouts = capabilities.supportedLayouts(options: supportsFoveation ?
                                                        [.foveationEnabled] : [])


                configuration.layout = supportedLayouts.contains(.layered) ? .layered : .dedicated
                configuration.isFoveationEnabled = supportsFoveation


                configuration.colorFormat = .rgba8Unorm_srgb
                configuration.depthFormat = .depth32Float_stencil8
                if singlePass == true
                {
                    configuration.layout = .layered
                } else
                {
                    configuration.layout = .dedicated
                }
                
                configuration.isFoveationEnabled = enableFoveation
            }
    }
    
    @Environment(\.openImmersiveSpace) private var openImmersiveSpace
    @Environment(\.dismiss) private var dismiss
    
    @State private var immersionStyle: ImmersionStyle = .full
    
    var body: some Scene {
        WindowGroup {
            Text("Loading").onAppear() {
                Task { @MainActor in
                    await openImmersiveSpace(id: "CompositorSpace")
                }
                // TODO: doesn't work?
                self.dismiss()
            }
        }
        
        ImmersiveSpace(id: "CompositorSpace") {
            let _ = UnityLibrary.GetInstance()
            let unityClass = NSClassFromString("UnityVisionOS") as? NSObject.Type
            let singlePass = Bool(truncating: unityClass?.perform(Selector(("getSinglePass"))).takeRetainedValue() as! NSNumber)
            let configuration = UnityContentConfiguration(singlePass: singlePass, enableFoveation: false)
            CompositorLayer(configuration:configuration) { layerRenderer in
                unityClass?.perform(Selector(("setLayerRenderer:")), with: layerRenderer)
            }
        }.immersionStyle(selection: $immersionStyle, in: .full)
    }
}

class UnitySwiftUIAppDelegate: NSObject, UIApplicationDelegate
{
    var unity: UnityLibrary
    
    override init() {
        unity = UnityLibrary.GetInstance()!

        super.init()

//        let api = QuantumRealityKitAccess.getApiData()
//        let size = QuantumRealityKitAccess.getApiSize()
//        SetQuantumNativeAPIImplementation(api, size)
//
//        QuantumRealityKitAccess.register()
    }
    
    func application(_ application: UIApplication, didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey : Any]? = nil) -> Bool {
        
        var args = CommandLine.arguments
//        args.append("-batchmode")

        unity.run(args: args)
        
        return true
    }
}
