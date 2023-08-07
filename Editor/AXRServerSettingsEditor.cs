using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace onAirXR.Server.Editor {
    [CustomEditor(typeof(AXRServerSettings))]
    public class AXRServerSettingsEditor : UnityEditor.Editor {
        private SerializedProperty _propLicense;
        private SerializedProperty _propStapPort;
        private SerializedProperty _propMinFrameRate;
        private SerializedProperty _propDefaultMirrorBlitMode;
        private SerializedProperty _propAdvancedSettingsEnabled;
        private SerializedProperty _propDesiredRenderPass;
        private SerializedProperty _propDisplayTextureColorSpaceHint;

        private void OnEnable() {
            _propLicense = serializedObject.FindProperty("license");
            _propStapPort = serializedObject.FindProperty("stapPort");
            _propMinFrameRate = serializedObject.FindProperty("minFrameRate");
            _propDefaultMirrorBlitMode = serializedObject.FindProperty("defaultMirrorBlitMode");
            _propAdvancedSettingsEnabled = serializedObject.FindProperty("advancedSettingsEnabled");
            _propDesiredRenderPass = serializedObject.FindProperty("desiredRenderPass");
            _propDisplayTextureColorSpaceHint = serializedObject.FindProperty("displayTextureColorSpaceHint");

            Experimental_OnEnable();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 240;

            EditorGUILayout.PropertyField(_propLicense, Styles.labelLicense);
            EditorGUILayout.PropertyField(_propStapPort, Styles.labelStapPort);
            EditorGUILayout.PropertyField(_propMinFrameRate, Styles.labelMinFrameRate);
            EditorGUILayout.PropertyField(_propDefaultMirrorBlitMode, Styles.labelMirrorBlitMode);

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.PropertyField(_propAdvancedSettingsEnabled, Styles.labelAdvancedSettingsEnabled);
                if (_propAdvancedSettingsEnabled.boolValue) {
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(_propDesiredRenderPass, Styles.labelDesiredRenderPass);
                    EditorGUILayout.PropertyField(_propDisplayTextureColorSpaceHint, Styles.labelDisplayTextureColorSpaceHint);

                    Experimental_renderAdvancedSettings();
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUIUtility.labelWidth = prevLabelWidth;

            serializedObject.ApplyModifiedProperties();
        }

        private static class Styles {
            public static GUIContent labelLicense = new GUIContent("License File");
            public static GUIContent labelStapPort = new GUIContent("Port");
            public static GUIContent labelMinFrameRate = new GUIContent("Minimum Frame Rate");
            public static GUIContent labelMirrorBlitMode = new GUIContent("Mirror View Mode");

            public static GUIContent labelAdvancedSettingsEnabled = new GUIContent("Advanced Settings");
            public static GUIContent labelDesiredRenderPass = new GUIContent("Render Pass");
            public static GUIContent labelDisplayTextureColorSpaceHint = new GUIContent("Display Texture Color Space Hint");
        }

        // experimental features
#if ONAIRXR_EXPERIMENTAL
        private SerializedProperty _propCpuReadableEncodeBuffer;
        private SerializedProperty _propCodecs;
        private SerializedProperty _propEncodingPreset;
        private SerializedProperty _propEncodingQuality;

        private void Experimental_OnEnable() {
            _propCpuReadableEncodeBuffer = serializedObject.FindProperty("cpuReadableEncodeBuffer");
            _propCodecs = serializedObject.FindProperty("codecs");
            _propEncodingPreset = serializedObject.FindProperty("encodingPreset");
            _propEncodingQuality = serializedObject.FindProperty("encodingQuality");
        }

        private void Experimental_renderAdvancedSettings() {
            EditorGUILayout.PropertyField(_propCodecs, Experimental_Styles.labelCodecs);
            EditorGUILayout.PropertyField(_propEncodingPreset, Experimental_Styles.labelEncodingPreset);
            EditorGUILayout.PropertyField(_propEncodingQuality, Experimental_Styles.labelEncodingQuality);
            EditorGUILayout.PropertyField(_propCpuReadableEncodeBuffer, Experimental_Styles.labelCpuReadableEncodeBuffer);
        }

        private static class Experimental_Styles {
            public static GUIContent labelCodecs = new GUIContent("Codecs");
            public static GUIContent labelEncodingPreset = new GUIContent("Encoding Preset");
            public static GUIContent labelEncodingQuality = new GUIContent("Encoding Quality");
            public static GUIContent labelCpuReadableEncodeBuffer = new GUIContent("CPU Readable Encode Buffer", "Must be set if you want to record a session as a video file.");
        }
#else
        private void Experimental_OnEnable() { }
        private void Experimental_renderAdvancedSettings() { }
#endif
    }
}
