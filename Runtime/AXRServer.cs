﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace onAirXR.Server {
    public class AXRServer : MonoBehaviour {
        private const int InvalidPlayerID = -1;
        private const float MaxFramerate = 120f;

        internal interface InternalEventHandler {
            void OnProfileDataReceived(string path);
            void OnProfileReportReceived(string report);
            void OnQueryResponseReceived(string statement, string body);
        }

        public interface EventHandler {
            void OnConnect(AXRPlayerConfig config);
            void OnActivate();
            void OnDeactivate();
            void OnDisconnect();
            void OnUserdataReceived(byte[] data);
        }

        public static AXRServer instance { get; private set; }

        internal static async void LoadOnce() {
            if (instance != null) { return; }

            // NOTE: wait until the first scene is loaded to avoid AXRServer from being destroyed.
            if (Application.isEditor == false &&  SceneManager.GetActiveScene().isLoaded == false) {
                await Task.Yield();
            }

            var go = new GameObject("AXRServer");
            go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            DontDestroyOnLoad(go);

            instance = go.AddComponent<AXRServer>();
        }

        private List<EventHandler> _eventHandlers = new List<EventHandler>();
        private List<InternalEventHandler> _internalEventHandlers = new List<InternalEventHandler>();
        private AXRAudioListener _currentAudioListener;
        private int _playerID = -1;

        internal bool isProfiling => connected && AXRServerPlugin.IsProfiling(_playerID);
        internal bool isRecording => connected && AXRServerPlugin.IsRecording(_playerID);

        public AXRServerInput input { get; private set; }
        public AXRPlayerConfig config { get; private set; }

        public bool connected => _playerID > InvalidPlayerID;
        public bool isOnStreaming => connected && AXRServerPlugin.IsOnStreaming(_playerID);

        public void RegisterEventHandler(EventHandler handler) {
            if (_eventHandlers.Contains(handler)) { return; }

            _eventHandlers.Add(handler);

            if (connected) {
                handler.OnConnect(config);
            }
            if (isOnStreaming) {
                handler.OnActivate();
            }
        }

        public void UnregisterEventHandler(EventHandler handler) {
            if (_eventHandlers.Contains(handler) == false) { return; }

            _eventHandlers.Remove(handler);
        }

        internal void RegisterInternalEventHandler(InternalEventHandler handler) {
            if (_internalEventHandlers.Contains(handler)) { return; }

            _internalEventHandlers.Add(handler);
        }

        internal void UnregisterInternalEventHandler(InternalEventHandler handler) {
            if (_internalEventHandlers.Contains(handler) == false) { return; }

            _internalEventHandlers.Remove(handler);
        }

        internal void Reconfigure(AXRServerSettings settings) {
            configure(settings);
        }

        internal void RequestConfigureSession(ulong minBitrate, ulong startBitrate, ulong maxBitrate) {
            if (connected == false) { return; }

            AXRServerPlugin.RequestConfigureSession(_playerID, minBitrate, startBitrate, maxBitrate);
        }

        internal void RequestImportSessionData(string path) {
            if (connected == false) { return; }

            AXRServerPlugin.RequestImportSessionData(_playerID, path);
        }

        internal void RequestRecordSession(string path) {
            if (connected == false) { return; }

            AXRServerPlugin.RequestRecordSession(_playerID, path);
        }

        internal void StartRecordVideo(string outputPathWithoutExt, AXRRecordFormat outputFormat, string sessionDataName = null) {
            if (connected == false) { return; }

            AXRServerPlugin.StartRecordVideo(_playerID, outputPathWithoutExt, (int)outputFormat, string.IsNullOrEmpty(sessionDataName) == false ? sessionDataName : null);
        }

        internal void StopRecordVideo() {
            if (connected == false) { return; }

            AXRServerPlugin.StopRecordVideo(_playerID);
        }

        internal void RequestPlay(string sessionDataName = null) {
            if (connected == false) { return; }

            AXRServerPlugin.RequestPlay(_playerID, string.IsNullOrEmpty(sessionDataName) == false ? sessionDataName : null);
        }

        internal void RequestStop() {
            if (connected == false) { return; }

            AXRServerPlugin.RequestStop(_playerID);
        }

        internal void RequestStartProfile(string directory, string filename, string sessionDataName = null) {
            if (connected == false) { return; }

            AXRServerPlugin.RequestStartProfile(_playerID, directory, filename, string.IsNullOrEmpty(sessionDataName) == false ? sessionDataName : null);
        }

        internal void RequestStopProfile() {
            if (connected == false) { return; }

            AXRServerPlugin.RequestStopProfile(_playerID);
        }

        internal void RequestQuery(string statement) {
            if (connected == false) { return; }

            AXRServerPlugin.RequestQuery(_playerID, statement);
        }

        private void Awake() {
            input = new AXRServerInput();
        }

        private void Start() {
            var settings = AXRServerSettings.instance;

            configure(settings);
            var ret = AXRServerPlugin.Startup(settings.propLicense, settings.propStapPort, settings.propAmpPort, settings.propLoopbackOnly);
            if (ret != 0) {
                var reason = ret == -1 ? "license not verified yet" :
                             ret == -2 ? "license file not found" :
                             ret == -3 ? "invalid license" :
                             ret == -4 ? "license expired" :
                                         $"system error (code = {ret})";

                Debug.LogError($"[onAirXR] failed to start up server: {reason}");
                return;
            }

            Debug.Log($"[onAirXR] server started up at port {settings.propStapPort}");
        }

        private void Update() {
            processServerMessages();
            ensureAudioListenerConfigured();
        }

        private void OnApplicationQuit() {
            if (instance != null) {
                Destroy(instance.gameObject);
            }
        }

        private void OnDestroy() {
            AXRServerPlugin.Shutdown();
            instance = null;
        }

        private void configure(AXRServerSettings settings) {
            AXRServerPlugin.Configure(settings.propMinFrameRate,
                                      MaxFramerate,
                                      AudioSettings.outputSampleRate,
                                      (int)settings.propDisplayTextureColorSpaceHint,
                                      settings.propCpuReadableEncodeBuffer,
                                      (int)settings.propCodecs,
                                      (int)settings.propEncodingPreset,
                                      (int)settings.propEncodingQuality);
        }

        private void processServerMessages() {
            while (AXRServerPlugin.GetNextServerMessage(out AXRServerMessage message)) {
                var playerID = message.source.ToInt32();

                if (message.IsSessionEvent()) {
                    if (message.Name.Equals(AXRServerMessage.NameConnected)) {
                        _playerID = playerID;
                    }
                    else if (message.Name.Equals(AXRServerMessage.NameDisconnected)) {
                        _playerID = InvalidPlayerID;
                    }
                    else if (message.Name.Equals(AXRServerMessage.NameProfileData)) {
                        onProfileDataReceived(message);
                    }
                    else if (message.Name.Equals(AXRServerMessage.NameProfileReport)) {
                        onProfileReportReceived(message);
                    }
                    else if (message.Name.Equals(AXRServerMessage.NameQueryResponse)) {
                        onQueryResponseReceived(message);
                    }
                }
                if (_playerID == InvalidPlayerID || playerID != _playerID) { continue; }

                if (message.IsPlayerEvent()) {
                    if (message.Name.Equals(AXRServerMessage.NameCreated)) {
                        config = AXRServerPlugin.GetConfig(_playerID);

                        notifyConnected(config);
                    }
                    else if (message.Name.Equals(AXRServerMessage.NameActivated)) {
                        notifyActivated();
                    }
                    else if (message.Name.Equals(AXRServerMessage.NameDeactivated)) {
                        notifyDeactivated();
                    }
                    else if (message.Name.Equals(AXRServerMessage.NameDestroyed)) {
                        config = null;

                        notifyDisconnected();
                    }
                }
                else if (message.Type.Equals(AXRMessage.TypeUserData)) {
                    onUserdataReceived(message);
                }
            }
        }

        private void notifyConnected(AXRPlayerConfig config) {
            foreach (var handler in _eventHandlers) {
                handler.OnConnect(config);
            }
        }

        private void notifyActivated() {
            foreach (var handler in _eventHandlers) {
                handler.OnActivate();
            }
        }

        private void notifyDeactivated() {
            foreach (var handler in _eventHandlers) {
                handler.OnDeactivate();
            }
        }

        private void notifyDisconnected() {
            foreach (var handler in _eventHandlers) {
                handler.OnDisconnect();
            }
        }

        private void onUserdataReceived(AXRServerMessage message) {
            foreach (var handler in _eventHandlers) {
                handler.OnUserdataReceived(message.Data_Decoded);
            }
        }

        private void onProfileDataReceived(AXRServerMessage message) {
            foreach (var handler in _internalEventHandlers) {
                handler.OnProfileDataReceived(message.DataFilePath);
            }
        }

        private void onProfileReportReceived(AXRServerMessage message) {
            Debug.Log($"[onairxr] profile report : {message.Body}");
            
            foreach (var handler in _internalEventHandlers) {
                handler.OnProfileReportReceived(message.Body);
            }
        }

        private void onQueryResponseReceived(AXRServerMessage message) {
            foreach (var handler in _internalEventHandlers) {
                handler.OnQueryResponseReceived(message.Statement, message.Body);
            }
        }

        private void ensureAudioListenerConfigured() {
            if (_currentAudioListener != null) { return; }

            var audioListener = Object.FindObjectOfType<AudioListener>(true);
            if (audioListener == null) { return; }

            _currentAudioListener = audioListener.GetComponent<AXRAudioListener>();
            if (_currentAudioListener == null) {
                _currentAudioListener = audioListener.gameObject.AddComponent<AXRAudioListener>();
            }
        }
    }
}
