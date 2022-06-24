using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace onAirXR.Server {
    public class AXRServerLoader : XRLoaderHelper {
        private static List<XRDisplaySubsystemDescriptor> _displaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();
        private static List<XRInputSubsystemDescriptor> _inputSubsystemDescriptors = new List<XRInputSubsystemDescriptor>();

        public override bool Initialize() {
            AXRServer.instance.Init();
            configure(AXRServerSettings.instance);

            CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(_displaySubsystemDescriptors, "onAirXR Display");
            CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(_inputSubsystemDescriptors, "onAirXR Input");
            return true;
        }

        public override bool Start() {
            var settings = AXRServerSettings.instance;
            configure(settings);

            AXRServerPlugin.SetRecordSettings(settings.propRecordVideo, settings.GenerateRecordVideoOutPathWithoutExtension(), (int)settings.propRecordFormat);

            StartSubsystem<XRDisplaySubsystem>();
            StartSubsystem<XRInputSubsystem>();

            var display = GetLoadedSubsystem<XRDisplaySubsystem>();
            display.SetPreferredMirrorBlitMode(settings.propDefaultMirrorBlitMode);

            switch (settings.propDesiredRenderPass) {
                case AXRRenderPass.SinglePassInstanced:
                    if ((display.textureLayout & XRDisplaySubsystem.TextureLayout.Texture2DArray) != 0) {
                        display.textureLayout = XRDisplaySubsystem.TextureLayout.Texture2DArray;
                    }
                    break;
                case AXRRenderPass.MultiPass:
                    if ((display.textureLayout & XRDisplaySubsystem.TextureLayout.SeparateTexture2Ds) != 0) {
                        display.textureLayout = XRDisplaySubsystem.TextureLayout.SeparateTexture2Ds;
                    }
                    break;
                case AXRRenderPass.SinglePassSideBySide:
                    if ((display.textureLayout & XRDisplaySubsystem.TextureLayout.SingleTexture2D) != 0) {
                        display.textureLayout = XRDisplaySubsystem.TextureLayout.SingleTexture2D;
                    }
                    break;
            }
            return true;
        }

        public override bool Stop() {
            StopSubsystem<XRDisplaySubsystem>();
            StopSubsystem<XRInputSubsystem>();
            return true;
        }

        public override bool Deinitialize() {
            DestroySubsystem<XRDisplaySubsystem>();
            DestroySubsystem<XRInputSubsystem>();

            AXRServer.instance.Cleanup();
            return true;
        }

        private void configure(AXRServerSettings settings) {
            AXRServerPlugin.Configure(settings.propLicense,
                                      settings.propStapPort,
                                      settings.propAmpPort,
                                      settings.propLoopbackOnly,
                                      settings.propMinFrameRate,
                                      120,
                                      AudioSettings.outputSampleRate,
                                      (int)settings.propDesiredRenderPass,
                                      (int)settings.propDisplayTextureColorSpaceHint);
        }
    }
}
