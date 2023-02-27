using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

#if UNITY_INPUT_SYSTEM && ENABLE_VR
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Layouts;
    using UnityEngine.InputSystem.XR;
#endif

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace onAirXR.Server {
#if UNITY_INPUT_SYSTEM && ENABLE_VR
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    static class InputLayoutLoader {
        static InputLayoutLoader() {
            RegisterInputLayouts();
        }

        public static void RegisterInputLayouts() {
            InputSystem.RegisterLayout<AXRInputDeviceLayoutHMD>(
                matches: new InputDeviceMatcher()
                .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                .WithProduct("^(onAirXR Main Device)")
            );

            InputSystem.RegisterLayout<AXRInputDeviceLayoutController>(
                matches: new InputDeviceMatcher()
                .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                .WithProduct("^(onAirXR Controller)")
            );
        }
    }
#endif

    public class AXRServerLoader : XRLoaderHelper {
        private static List<XRDisplaySubsystemDescriptor> _displaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();
        private static List<XRInputSubsystemDescriptor> _inputSubsystemDescriptors = new List<XRInputSubsystemDescriptor>();

        public override bool Initialize() {
#if UNITY_INPUT_SYSTEM && ENABLE_VR
            InputLayoutLoader.RegisterInputLayouts();
#endif

            AXRServer.instance.LoadOnce();

            CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(_displaySubsystemDescriptors, "onAirXR Display");
            CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(_inputSubsystemDescriptors, "onAirXR Input");
            return true;
        }

        public override bool Start() {
            var settings = AXRServerSettings.instance;
            AXRServer.instance.Reconfigure(settings);

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

            return true;
        }
    }
}
