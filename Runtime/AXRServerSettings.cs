using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace onAirXR.Server {
    public enum AXRMirrorBlitMode { 
        None = 0,
        Left = -1,
        Right = -2,
        SideBySide = -3
    }

    public enum AXRRenderPass {
        SinglePassInstanced = 0,
        MultiPass
    }

    public enum AXRTextureColorSpaceHint {
        None = 0,
        Gamma,
        Linear
    }

    [Serializable]
    [XRConfigurationData("onAirXR Server", SettingsKey)]
    public class AXRServerSettings : ScriptableObject {
        public const string SettingsKey = "com.onairxr.server.settings";

        public static bool IsUniveralRenderPipeline() => UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline?.GetType()?.Name?.Equals("UniversalRenderPipelineAsset") ?? false;
        public static bool IsHDRenderPipeline() => UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline?.GetType()?.Name?.Equals("HDRenderPipelineAsset") ?? false;

#if UNITY_EDITOR
        public static AXRServerSettings instance {
            get {
                UnityEngine.Object obj;
                UnityEditor.EditorBuildSettings.TryGetConfigObject(SettingsKey, out obj);
                if (obj == null || (obj is AXRServerSettings) == false) { return null; }

                var settings = obj as AXRServerSettings;
                settings.ParseCommandLine();

                return settings;
            }
        }
#else
        public static AXRServerSettings runtimeInstance { get; private set; } = null;
        public static AXRServerSettings instance => runtimeInstance;

        public void Awake() {
            if (runtimeInstance != null) { return; }

            ParseCommandLine();
            runtimeInstance = this;
        }
#endif

        [SerializeField] private string license = "noncommercial.license";
        [SerializeField] private int stapPort = 9090;
        [SerializeField] [Range(10, 120)] private int minFrameRate = 10;
        [SerializeField] private AXRMirrorBlitMode defaultMirrorBlitMode = AXRMirrorBlitMode.Left;

        // overridable by command line args only
        [SerializeField] private int ampPort;
        [SerializeField] private bool loopbackOnly;

        [SerializeField] private bool advancedSettingsEnabled = false;
        [SerializeField] private AXRRenderPass desiredRenderPass = AXRRenderPass.SinglePassInstanced;
        [SerializeField] private AXRTextureColorSpaceHint displayTextureColorSpaceHint = AXRTextureColorSpaceHint.None;

        public string propLicense {
            get {
                if (Application.isEditor) {
                    return Path.GetFullPath("Packages/com.onairxr.server/Resources/noncommercial.license");
                }

                return string.IsNullOrEmpty(license) == false ? license : "noncommercial.license";
            }
        }

        public int propMinFrameRate => minFrameRate;
        public int propDefaultMirrorBlitMode => (int)defaultMirrorBlitMode;
        public int propStapPort => stapPort;
        public int propAmpPort => ampPort;
        public bool propLoopbackOnly => loopbackOnly;
        public AXRRenderPass propDesiredRenderPass => advancedSettingsEnabled ? desiredRenderPass : AXRRenderPass.SinglePassInstanced;

        public AXRTextureColorSpaceHint propDisplayTextureColorSpaceHint {
            get {
                var value = advancedSettingsEnabled ? displayTextureColorSpaceHint : AXRTextureColorSpaceHint.None;

                if (value == AXRTextureColorSpaceHint.None) {
                    if (IsUniveralRenderPipeline()) {
                        // workaround: URP uses always non-sRGB texture even if color space is set to linear. (but xr plugin misleads as if it were sRGB.)
                        value = AXRTextureColorSpaceHint.Gamma;
                    }
                    else if (IsHDRenderPipeline() && QualitySettings.activeColorSpace == ColorSpace.Gamma) {
                        // workaround: On HDRP, xr plugin misleads as if texture were sRGB even when color space is set to gamma.
                        value = AXRTextureColorSpaceHint.Gamma;
                    }
                }
                return value;
            }
        }

        public AXRServerSettings ParseCommandLine() {
            var pairs = AXRUtils.ParseCommandLine(Environment.GetCommandLineArgs());
            if (pairs == null) { return this; }

            const string keyConfigFile = "config";
            if (pairs.ContainsKey(keyConfigFile)) {
                if (File.Exists(pairs[keyConfigFile])) {
                    try {
                        var reader = new AXRServerSettingsReader();
                        reader.ReadSettings(pairs[keyConfigFile], this);
                    }
                    catch (Exception e) {
                        Debug.LogWarning("[onAirXR] WARNING: failed to parse " + pairs[keyConfigFile] + " : " + e.ToString());
                    }
                }
                pairs.Remove("config");
            }

            foreach (string key in pairs.Keys) {
                if (key.Equals("onairvr_stap_port") || 
                    key.Equals("onairxr_stap_port")) {
                    stapPort = AXRUtils.ParseInt(pairs[key], propStapPort,
                        (parsed) => {
                            return 0 <= parsed && parsed <= 65535;
                        },
                        (val) => {
                            Debug.LogWarning("[onAirXR] WARNING: STAP Port number of the command line argument is invalid : " + val);
                        });
                }
                else if (key.Equals("onairvr_amp_port") || 
                         key.Equals("onairxr_amp_port")) {
                    ampPort = AXRUtils.ParseInt(pairs[key], propAmpPort,
                        (parsed) => {
                            return 0 <= parsed && parsed <= 65535;
                        },
                        (val) => {
                            Debug.LogWarning("[onAirXR] WARNING: AMP Port number of the command line argument is invalid : " + val);
                        });
                }
                else if (key.Equals("onairvr_loopback_only") ||
                         key.Equals("onairxr_loopback_only")) {
                    loopbackOnly = pairs[key].Equals("true");
                }
                else if (key.Equals("onairvr_license") ||
                         key.Equals("onairxr_license")) {
                    license = pairs[key];
                }
                else if (key.Equals("onairvr_min_frame_rate") ||
                         key.Equals("onairxr_min_frame_rate")) {
                    minFrameRate = AXRUtils.ParseInt(pairs[key], propMinFrameRate,
                        (parsed) => {
                            return parsed >= 0;
                        });
                }
            }

            return this;
        }

        [Serializable]
        private class AXRServerSettingsReader {
            [SerializeField] private AXRServerSettings onairxr;

            public void ReadSettings(string fileFrom, AXRServerSettings to) {
                onairxr = to;
                JsonUtility.FromJsonOverwrite(File.ReadAllText(fileFrom), this);
            }
        }

        // experimental features
        #pragma warning disable 0414

        [SerializeField] private bool cpuReadableEncodeBuffer = false;
        [SerializeField] private AXRCodec codecs = AXRCodec.All;
        [SerializeField] private AXREncodingPreset encodingPreset = AXREncodingPreset.LowLatency;
        [SerializeField] private AXREncodingQuality encodingQuality = AXREncodingQuality.VeryHigh;
        
        #pragma warning restore 0414

#if ONAIRXR_EXPERIMENTAL
        public bool propCpuReadableEncodeBuffer => advancedSettingsEnabled ? cpuReadableEncodeBuffer : false;
        public AXRCodec propCodecs => advancedSettingsEnabled ? codecs : AXRCodec.All;
        public AXREncodingPreset propEncodingPreset => advancedSettingsEnabled ? encodingPreset : AXREncodingPreset.LowLatency;
        public AXREncodingQuality propEncodingQuality => advancedSettingsEnabled ? encodingQuality : AXREncodingQuality.VeryHigh;
#else
        public bool propCpuReadableEncodeBuffer => false;
        public AXRCodec propCodecs => AXRCodec.All;
        public AXREncodingPreset propEncodingPreset => AXREncodingPreset.LowLatency;
        public AXREncodingQuality propEncodingQuality => AXREncodingQuality.VeryHigh;
#endif
    }
}
