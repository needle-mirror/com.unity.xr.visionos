import CompositorServices
import SwiftUI

struct UnityCompositorServicesConfiguration: CompositorLayerConfiguration {
    let singlePass: Bool
    let enableFoveation: Bool

    func makeConfiguration(capabilities: LayerRenderer.Capabilities, configuration: inout LayerRenderer.Configuration) {
        configuration.colorFormat = .rgba8Unorm_srgb
        configuration.depthFormat = .depth32Float_stencil8
        if singlePass == true {
            configuration.layout = .layered
        } else {
            configuration.layout = .dedicated
        }

        configuration.isFoveationEnabled = enableFoveation
        configuration.generateFlippedRasterizationRateMaps = enableFoveation
    }
}
