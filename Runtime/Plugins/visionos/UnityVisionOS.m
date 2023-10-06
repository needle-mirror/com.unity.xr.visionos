#import <Foundation/Foundation.h>
#include "IUnityInterface.h"
#import <CompositorServices/CompositorServices.h>

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
#if TARGET_OS_SIMULATOR
    return @(0);
#else
    return @(1);
#endif
}

@end
