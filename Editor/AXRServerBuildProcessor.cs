using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace onAirXR.Server.Editor {
    public class AXRServerBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport {
        int IOrderedCallback.callbackOrder => 0;

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report) {
            cleanOldSettings();

            EditorBuildSettings.TryGetConfigObject(AXRServerSettings.SettingsKey, out AXRServerSettings settings);
            if (settings == null) { return; }

            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            if (preloadedAssets.Contains(settings) == false) {
                var assets = preloadedAssets.ToList();
                assets.Add(settings);
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report) {
            cleanOldSettings();
        }

        private void cleanOldSettings() {
            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            if (preloadedAssets == null) { return; }

            var oldSettings = preloadedAssets.Where((asset) => asset?.GetType() == typeof(AXRServerSettings));
            if (oldSettings?.Any() ?? false) {
                var assets = preloadedAssets.ToList();
                foreach (var setting in oldSettings) {
                    assets.Remove(setting);
                }

                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
        }
    }
}
