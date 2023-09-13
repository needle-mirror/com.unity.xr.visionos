#import <Foundation/Foundation.h>
#include "IUnityInterface.h"
#import <CompositorServices/CompositorServices.h>

#if __has_include("visionos_config.h")
#include "visionos_config.h"
#else
#define VISIONOS_SINGLE_PASS 1
#define VISIONOS_SIMULATOR 0
#endif

#define EXPORT(RETURN_TYPE) RETURN_TYPE __attribute__ ((visibility("default")))  __attribute__((__used__))

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
