#if XR_MGMT_320

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;

namespace onAirXR.Server.Editor {
    internal class AXRServerMetadata : IXRPackage {
        private static IXRPackageMetadata _metadata = new AXRServerPackageMetadata();

        public IXRPackageMetadata metadata => _metadata;

        public bool PopulateNewSettingsInstance(ScriptableObject obj) {
            var settings = obj as AXRServerSettings;

            return settings != null;
        }

        private class AXRServerPackageMetadata : IXRPackageMetadata {
            public string packageName => "onAirXR";
            public string packageId => "com.onairxr.server";
            public string settingsType => "onAirXR.Server.AXRServerSettings";

            private static readonly List<IXRLoaderMetadata> _loaderMetadata = new List<IXRLoaderMetadata>() { new AXRServerLoaderMetadata() };
            public List<IXRLoaderMetadata> loaderMetadata => _loaderMetadata;
        }

        private class AXRServerLoaderMetadata : IXRLoaderMetadata {
            public string loaderName => "onAirXR";
            public string loaderType => "onAirXR.Server.AXRServerLoader";

            private static readonly List<BuildTargetGroup> _supportedBuildTargets = new List<BuildTargetGroup>() { BuildTargetGroup.Standalone };
            public List<BuildTargetGroup> supportedBuildTargets => _supportedBuildTargets;
        }
    }
}

#endif