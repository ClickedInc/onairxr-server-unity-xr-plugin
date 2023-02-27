using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Server {
    public class AXRServer : MonoBehaviour {
        private const int InvalidPlayerID = -1;
        private const float MaxFramerate = 120f;

        public interface EventHandler {
            void OnConnect(AXRPlayerConfig config);
            void OnActivate();
            void OnDeactivate();
            void OnDisconnect();
            void OnUserdataReceived(byte[] data);
            void OnProfileDataReceived(string path);
            void OnProfileReportReceived(string report);
            void OnQueryResponseReceived(string statement, string body);
        }

        private static AXRServer _instance;

        public static AXRServer instance {
            get {
                if (_instance == null) {
                    var go = new GameObject("AXRServer");
                    go.hideFlags = HideFlags.HideAndDontSave;
                    DontDestroyOnLoad(go);

                    _instance = go.AddComponent<AXRServer>();
                }
                return _instance;
            }
        }

        private List<EventHandler> _eventHandlers = new List<EventHandler>();
        private AXRAudioListener _currentAudioListener;
        private int _playerID = -1;

        public AXRServerInput input { get; private set; }
        public AXRPlayerConfig config { get; private set; }
        public bool connected => _playerID > InvalidPlayerID;
        public bool isOnStreaming => connected && AXRServerPlugin.IsOnStreaming(_playerID);
        public bool isProfiling => connected && AXRServerPlugin.IsProfiling(_playerID);
        public bool isRecording => connected && AXRServerPlugin.IsRecording(_playerID);

        public void LoadOnce() {
            // do nothing. just to instantiate
        }

        public void Reconfigure(AXRServerSettings settings) {
            configure(settings);
        }

        public void RegisterEventHandler(EventHandler handler) {
            if (_eventHandlers.Contains(handler)) { return; }

            _eventHandlers.Add(handler);
        }

        public void UnregisterEventHandler(EventHandler handler) {
            if (_eventHandlers.Contains(handler) == false) { return; }

            _eventHandlers.Remove(handler);
        }

        public void RequestConfigureSession(ulong minBitrate, ulong startBitrate, ulong maxBitrate) {
            if (connected == false) { return; }

            AXRServerPlugin.RequestConfigureSession(_playerID, minBitrate, startBitrate, maxBitrate);
        }

        public void RequestImportSessionData(string path) {
            if (connected == false) { return; }

            AXRServerPlugin.RequestImportSessionData(_playerID, path);
        }

        public void RequestRecordSession(string path) {
            if (connected == false) { return; }

            AXRServerPlugin.RequestRecordSession(_playerID, path);
        }

        public void StartRecordVideo(string outputPathWithoutExt, AXRRecordFormat outputFormat, string sessionDataName = null) {
            if (connected == false) { return; }

            AXRServerPlugin.StartRecordVideo(_playerID, outputPathWithoutExt, (int)outputFormat, string.IsNullOrEmpty(sessionDataName) == false ? sessionDataName : null);
        }

        public void StopRecordVideo() {
            if (connected == false) { return; }

            AXRServerPlugin.StopRecordVideo(_playerID);
        }

        public void RequestPlay(string sessionDataName = null) {
            if (connected == false) { return; }

            AXRServerPlugin.RequestPlay(_playerID, string.IsNullOrEmpty(sessionDataName) == false ? sessionDataName : null);
        }

        public void RequestStop() {
            if (connected == false) { return; }

            AXRServerPlugin.RequestStop(_playerID);
        }

        public void RequestStartProfile(string directory, string filename, string sessionDataName = null) {
            if (connected == false) { return; }

            AXRServerPlugin.RequestStartProfile(_playerID, directory, filename, sessionDataName);
        }

        public void RequestStopProfile() {
            if (connected == false) { return; }

            AXRServerPlugin.RequestStopProfile(_playerID);
        }

        public void RequestQuery(string statement) {
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
                var reason = ret == -1 ? "not verified yet" :
                             ret == -2 ? "file not found" :
                             ret == -3 ? "invalid license" :
                             ret == -4 ? "license expired" :
                                         $"system error (code = {ret})";

                Debug.LogError($"[onAirXR Server] failed to start up server: code = {reason}");
            }
        }

        private void Update() {
            processServerMessages();
            ensureAudioListenerConfigured();
        }

        private void OnApplicationQuit() {
            if (_instance != null) {
                Destroy(_instance.gameObject);
            }
        }

        private void OnDestroy() {
            AXRServerPlugin.Shutdown();
        }

        private void configure(AXRServerSettings settings) {
            AXRServerPlugin.Configure(settings.propMinFrameRate,
                                      MaxFramerate,
                                      AudioSettings.outputSampleRate,
                                      (int)settings.propDesiredRenderPass,
                                      (int)settings.propDisplayTextureColorSpaceHint,
                                      settings.propCpuReadableEncodeBuffer,
                                      (int)settings.propCodecs,
                                      (int)settings.propEncodingPreset,
                                      (int)settings.propEncodingPerformance);
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
            foreach (var handler in _eventHandlers) {
                handler.OnProfileDataReceived(message.DataFilePath);
            }
        }

        private void onProfileReportReceived(AXRServerMessage message) {
            foreach (var handler in _eventHandlers) {
                handler.OnProfileReportReceived(message.Body);
            }
        }

        private void onQueryResponseReceived(AXRServerMessage message) {
            foreach (var handler in _eventHandlers) {
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
