using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if ONAIRXR_EXPERIMENTAL

namespace onAirXR.Server {
    public class AXRSessionControlWindow {
        [MenuItem("onAirXR/Server/Open Session Controls...", false, 100)]
        public static void ShowWindow() {
            var window = EditorWindow.GetWindow<SessionControlWindow>("onAirXR Session Controls");
            window.Show();
        }

        private class SessionControlWindow : EditorWindow {
            private const float RepaintInterval = 0.1f;
            private const float LabelWidth = 160;
            private const string SessionFileExtension = "session";
            
            private const string PrefKeyMinBitrate = "onairxr.session-control.min-bitrate";
            private const string PrefKeyStartBitrate = "onairxr.session-control.start-bitrate";
            private const string PrefKeyMaxBitrate = "onairxr.session-control.max-bitrate";
            private const string PrefKeySessionDataPath = "onairxr.session-control.session-data-path";
            private const string PrefKeyRecordVideoDirectory = "onairxr.session-control.record-video-directory";
            private const string PrefKeyRecordVideoName = "onairxr.session-contorl.record-video-name";
            private const string PrefKeyRecordVideoFormat = "onairxr.session-control.record-video-format";
            private const string PrefKeySessionDirectory = "onairxr.session-control.session-directory";
            private const string PrefKeySessionName = "onairxr.session-control.session-name";

            private float _remainingToRepaint;
            private float _minBitrate;
            private float _startBitrate;
            private float _maxBitrate;
            private string _sessionDataPath;
            private float _sessionRecordTime;
            private string _recordVideoDirectory;
            private string _recordVideoName;
            private AXRRecordFormat _recordVideoFormat;
            private string _profileSessionDirectory;
            private string _profileSessionName;
            private float _profileSessionTime;

            private void OnEnable() {
                _minBitrate = PlayerPrefs.GetFloat(PrefKeyMinBitrate, 16f);
                _startBitrate = PlayerPrefs.GetFloat(PrefKeyStartBitrate, 20f);
                _maxBitrate = PlayerPrefs.GetFloat(PrefKeyMaxBitrate, 24f);
                _sessionDataPath = PlayerPrefs.GetString(PrefKeySessionDataPath, "");
                _recordVideoDirectory = PlayerPrefs.GetString(PrefKeyRecordVideoDirectory, Application.persistentDataPath);
                _recordVideoName = PlayerPrefs.GetString(PrefKeyRecordVideoName, "video");
                _recordVideoFormat = (AXRRecordFormat)PlayerPrefs.GetInt(PrefKeyRecordVideoFormat, (int)AXRRecordFormat.MP4);
                _profileSessionDirectory = PlayerPrefs.GetString(PrefKeySessionDirectory, Application.persistentDataPath);
                _profileSessionName = PlayerPrefs.GetString(PrefKeySessionName, "session");
            }

            private void OnDisable() {
                if (Application.isPlaying && AXRServer.instance.isProfiling) {
                    AXRServer.instance.RequestStopProfile();
                }

                PlayerPrefs.SetFloat(PrefKeyMinBitrate, _minBitrate);
                PlayerPrefs.SetFloat(PrefKeyStartBitrate, _startBitrate);
                PlayerPrefs.SetFloat(PrefKeyMaxBitrate, _maxBitrate);
                PlayerPrefs.SetString(PrefKeySessionDataPath, _sessionDataPath);
                PlayerPrefs.SetString(PrefKeyRecordVideoDirectory, _recordVideoDirectory);
                PlayerPrefs.SetString(PrefKeyRecordVideoName, _recordVideoName);
                PlayerPrefs.SetInt(PrefKeyRecordVideoFormat, (int)_recordVideoFormat);
                PlayerPrefs.SetString(PrefKeySessionDirectory, _profileSessionDirectory);
                PlayerPrefs.SetString(PrefKeySessionName, _profileSessionName);
            }

            private void OnGUI() {
                if (Application.isPlaying == false || AXRServer.instance.connected == false) {
                    renderUnavailable();
                    return;
                }

                renderConfigure();
                renderSessionData();
                renderPlaybackControls();
                renderRecordVideoControls();
                renderProfilerControls();
            }

            private void Update() {
                if (Application.isPlaying == false || AXRServer.instance.connected == false) { return; }

                _remainingToRepaint -= Time.unscaledDeltaTime;
                if (_remainingToRepaint <= 0) {
                    Repaint();

                    _remainingToRepaint = RepaintInterval;
                }
            }

            private void renderUnavailable() {
                EditorGUILayout.LabelField(Styles.descUnavailable);
            }

            private void renderSection(GUIContent label, Action render) {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                render();

                EditorGUILayout.Space();
            }

            private void renderConfigure() {
                renderSection(Styles.labelRenderConfigure, () => {
                    _minBitrate = renderFloatField(Styles.labelMinBitrate, _minBitrate);
                    _startBitrate = renderFloatField(Styles.labelStartBitrate, _startBitrate);
                    _maxBitrate = renderFloatField(Styles.labelMaxBitrate, _maxBitrate);
                    var apply = GUILayout.Button(Styles.labelApply);

                    if (apply) {
                        AXRServer.instance.RequestConfigureSession(mbpsToBps(_minBitrate), mbpsToBps(_startBitrate), mbpsToBps(_maxBitrate));
                    }
                });
            }

            private void renderSessionData() {
                renderSection(Styles.labelSessionData, () => {
                    _sessionDataPath = renderSaveFileField(Styles.labelSessionDataPath, Styles.titleEnterSessionDataPath, _sessionDataPath, SessionFileExtension);

                    var playing = AXRServer.instance.isOnStreaming;
                    var sessionDataExists = string.IsNullOrEmpty(_sessionDataPath) == false && File.Exists(_sessionDataPath);

                    EditorGUILayout.BeginHorizontal();
                    {
                        var record = renderEnabled(!playing, () => GUILayout.Button(Styles.labelRecord));
                        var import = renderEnabled(sessionDataExists, () => GUILayout.Button(Styles.labelImportToClient));

                        if (record) {
                            AXRServer.instance.RequestRecordSession(_sessionDataPath);
                            _sessionRecordTime = Time.realtimeSinceStartup;
                        }
                        if (import) {
                            AXRServer.instance.RequestImportSessionData(_sessionDataPath);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (_sessionRecordTime > 0) {
                        EditorGUILayout.LabelField($"{Time.realtimeSinceStartup - _sessionRecordTime:f1} secs elapsed.");
                    }
                });
            }

            private void renderPlaybackControls() {
                renderSection(Styles.labelPlaybackControls, () => {
                    EditorGUILayout.BeginHorizontal();
                    {
                        var playing = AXRServer.instance.isOnStreaming;

                        var play = renderEnabled(!playing, () => GUILayout.Button(Styles.labelPlay));
                        var playSessionData = renderEnabled(!playing && string.IsNullOrEmpty(_sessionDataPath) == false, () => GUILayout.Button(Styles.labelPlaySessionData));
                        var stop = renderEnabled(playing, () => GUILayout.Button(Styles.labelStop));

                        if (play || playSessionData) {
                            AXRServer.instance.RequestPlay(playSessionData ? Path.GetFileName(_sessionDataPath) : null);
                        }
                        else if (stop) {
                            AXRServer.instance.RequestStop();
                            _sessionRecordTime = -1f;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                });
            }

            private void renderRecordVideoControls() {
                renderSection(Styles.labelRecordVideo, () => {
                    _recordVideoDirectory = renderOpenFolderField(Styles.labelVideoDirectory, Styles.titleBrowseVideoDirectory, _recordVideoDirectory);
                    _recordVideoName = renderTextField(Styles.labelVideoName, _recordVideoName);
                    _recordVideoFormat = (AXRRecordFormat)renderEnumField(Styles.labelVideoFormat, _recordVideoFormat);

                    EditorGUILayout.BeginHorizontal();
                    {
                        var recordable = string.IsNullOrEmpty(_recordVideoDirectory) == false && string.IsNullOrEmpty(_recordVideoName) == false;
                        var playing = AXRServer.instance.isOnStreaming;

                        var record = renderEnabled(recordable && !playing, () => GUILayout.Button(Styles.labelRecord));
                        var recordSessionData = renderEnabled(recordable && !playing && string.IsNullOrEmpty(_sessionDataPath) == false, () => GUILayout.Button(Styles.labelRecordSessionData));
                        var stop = renderEnabled(recordable && playing, () => GUILayout.Button(Styles.labelStop));

                        if (record || recordSessionData) {
                            var recordPathWithoutExt = Path.Combine(_recordVideoDirectory, $"{_recordVideoName}_{DateTime.Now:yyMMddHHmmss}");
                            AXRServer.instance.StartRecordVideo(recordPathWithoutExt, _recordVideoFormat, recordSessionData ? Path.GetFileName(_sessionDataPath) : null);
                        }
                        else if (stop) {
                            AXRServer.instance.StopRecordVideo();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                });
            }

            private void renderProfilerControls() {
                renderSection(Styles.labelProfilerControls, () => {
                    var playing = AXRServer.instance.isOnStreaming;
                    var profiling = AXRServer.instance.isProfiling;

                    _profileSessionDirectory = renderEnabled(!profiling, () => renderOpenFolderField(Styles.labelDataDirectory, Styles.titleBrowseDataDirectory, _profileSessionDirectory));
                    _profileSessionName = renderEnabled(!profiling, () => renderTextField(Styles.labelSessionName, _profileSessionName));

                    EditorGUILayout.BeginHorizontal();
                    {
                        var start = renderEnabled(!profiling, () => GUILayout.Button(Styles.labelStartProfile));
                        var startSessionData = renderEnabled(!playing && string.IsNullOrEmpty(_sessionDataPath) == false, () => GUILayout.Button(Styles.labelStartProfileSessionData));
                        var stop = renderEnabled(profiling, () => GUILayout.Button(Styles.labelStopProfile));

                        if (start || startSessionData) {
                            AXRServer.instance.RequestStartProfile(_profileSessionDirectory, 
                                                                   $"{_profileSessionName}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.frames",
                                                                   startSessionData ? Path.GetFileName(_sessionDataPath) : null);
                            _profileSessionTime = Time.realtimeSinceStartup;
                        }
                        else if (stop) {
                            AXRServer.instance.RequestStopProfile();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (profiling) {
                        EditorGUILayout.LabelField($"{Time.realtimeSinceStartup - _profileSessionTime:f1} secs elapsed.");
                    }
                });
            }

            private T renderEnabled<T>(bool enabled, Func<T> render) {
                GUI.enabled = enabled;
                var result = render();
                GUI.enabled = true;

                return result;
            }

            private float renderFloatField(GUIContent label, float value, Action<float> onChange = null) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.MaxWidth(LabelWidth));
                var next = EditorGUILayout.FloatField(value);
                EditorGUILayout.EndHorizontal();

                if (next != value) {
                    onChange?.Invoke(next);
                }
                return next;
            }

            private Enum renderEnumField(GUIContent label, Enum value, Action<Enum> onChange = null) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.MaxWidth(LabelWidth));
                var next = EditorGUILayout.EnumPopup(value);
                EditorGUILayout.EndHorizontal();

                if (next != value) {
                    onChange?.Invoke(next);
                }
                return next;
            }

            private string renderTextField(GUIContent label, string value, Action<string> onChange = null) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.MaxWidth(LabelWidth));
                var next = EditorGUILayout.TextField(value);
                EditorGUILayout.EndHorizontal();

                if (next != value) {
                    onChange?.Invoke(next);
                }
                return next;
            }

            private string renderOpenFolderField(GUIContent label, string dialogTitle, string value) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.MaxWidth(LabelWidth));

                var enabled = GUI.enabled;
                GUI.enabled = false;
                {
                    EditorGUILayout.TextField(value);
                }
                GUI.enabled = enabled;

                var pressed = GUILayout.Button("..", GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();

                if (pressed == false) { return value; }

                var selected = EditorUtility.OpenFolderPanel(dialogTitle, value, "");
                return string.IsNullOrEmpty(selected) == false ? selected : value;
            }

            private string renderOpenFileField(GUIContent label, string dialogTitle, string value, string extension) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.MaxWidth(LabelWidth));

                var enabled = GUI.enabled;
                GUI.enabled = false;
                {
                    EditorGUILayout.TextField(value);
                }
                GUI.enabled = enabled;

                var pressed = GUILayout.Button("..", GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();

                if (pressed == false) { return value; }

                var selected = EditorUtility.OpenFilePanel(dialogTitle, string.IsNullOrEmpty(value) == false ? getDirectoryName(value) : value, extension);
                return string.IsNullOrEmpty(selected) == false ? selected : value;
            }

            private string renderSaveFileField(GUIContent label, string dialogTitle, string value, string extension) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.MaxWidth(LabelWidth));

                var enabled = GUI.enabled;
                GUI.enabled = false;
                {
                    EditorGUILayout.TextField(value);
                }
                GUI.enabled = enabled;

                var pressed = GUILayout.Button("..", GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();

                if (pressed == false) { return value; }

                var selected = EditorUtility.SaveFilePanel(dialogTitle, getDirectoryName(value), getFilenameWithoutExtension(value), extension);
                return string.IsNullOrEmpty(selected) == false ? selected : value;
            }
            
            private ulong mbpsToBps(float mbps) => (ulong)(mbps * 1000UL * 1000UL);

            private string getDirectoryName(string value) {
                try {
                    return Path.GetDirectoryName(value);
                }
                catch {
                    return "";
                }
            }

            private string getFilenameWithoutExtension(string value) {
                try {
                    return Path.GetFileNameWithoutExtension(value);
                }
                catch {
                    return "";
                }
            }
        }

        private class Styles {
            public static GUIContent descUnavailable = new GUIContent("There is no connected session.");

            public static GUIContent labelRenderConfigure = new GUIContent("Configure");
            public static GUIContent labelMinBitrate = new GUIContent("Min Bitrate (Mbps)");
            public static GUIContent labelStartBitrate = new GUIContent("Start Bitrate (Mbps)");
            public static GUIContent labelMaxBitrate = new GUIContent("Max Bitrate (Mbps)");
            public static GUIContent labelApply = new GUIContent("Apply");

            public static GUIContent labelSessionData = new GUIContent("Session Data");
            public static GUIContent labelSessionDataPath = new GUIContent("Session Data File");
            public const string titleEnterSessionDataPath = "Enter a session data path...";
            public static GUIContent labelRecord = new GUIContent("Record");
            public static GUIContent labelImportToClient = new GUIContent("Import");

            public static GUIContent labelPlaybackControls = new GUIContent("Playback");
            public static GUIContent labelPlay = new GUIContent("Play");
            public static GUIContent labelPlaySessionData = new GUIContent("Play (Session Data)");
            public static GUIContent labelStop = new GUIContent("Stop");

            public static GUIContent labelRecordVideo = new GUIContent("Record Video");
            public static GUIContent labelVideoDirectory = new GUIContent("Video Directory");
            public const string titleBrowseVideoDirectory = "Select a directory to save recorded videos...";
            public static GUIContent labelVideoName = new GUIContent("Video Name");
            public static GUIContent labelVideoFormat = new GUIContent("Video Format");
            public static GUIContent labelRecordSessionData = new GUIContent("Record (Session Data)");

            public static GUIContent labelProfilerControls = new GUIContent("Profiler");
            public static GUIContent labelDataDirectory = new GUIContent("Data Directory");
            public const string titleBrowseDataDirectory = "Select a directory to save profiled data files...";
            public static GUIContent labelSessionName = new GUIContent("Session Name");
            public static GUIContent labelStartProfile = new GUIContent("Start");
            public static GUIContent labelStartProfileSessionData = new GUIContent("Start (Session Data)");
            public static GUIContent labelStopProfile = new GUIContent("Stop");
        }
    }
}

#endif //ONAIRXR_EXPERIMENTAL
