#import <Foundation/Foundation.h>
#include "IUnityInterface.h"
#import <CompositorServices/CompositorServices.h>
#include "visionos_config.h"

#ifdef __cplusplus
extern "C" {
#endif

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces);

void ObjC_SetCPLayer(cp_layer_renderer_t layer);
    
#ifdef __cplusplus
} // extern "C"
#endif

@interface UnityVisionOS : NSObject

+ (void)loadPlugin;

+ (void)setLayerRenderer:(cp_layer_renderer_t)layerRenderer;

+ (NSNumber*)getSinglePass;

@end

@implementation UnityVisionOS

+ (void)loadPlugin
{
    UnityRegisterRenderingPluginV5(UnityPluginLoad, NULL);
}

+ (void)setLayerRenderer:(cp_layer_renderer_t)layerRenderer
{
    ObjC_SetCPLayer(layerRenderer);
}

+ (NSNumber*)getSinglePass
{
    return @(VISIONOS_SINGLE_PASS);
}
                         
@end
