using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Server {
    public class AXRServer : AXRServerEventLoop.Listener {
        private const int InvalidPlayerID = -1;

        public interface EventHandler {
            void OnConnect(AXRPlayerConfig config);
            void OnActivate();
            void OnDeactivate();
            void OnDisconnect();
            void OnUserdataReceived(byte[] data);
        }

        private static AXRServer _instance;

        public static AXRServer instance {
            get {
                if (_instance == null) {
                    _instance = new AXRServer();
                }
                return _instance;
            }
        }

        private List<EventHandler> _eventHandlers = new List<EventHandler>();
        private AXRAudioListener _currentAudioListener;
        private int _playerID = -1;

        public AXRServerInput input { get; private set; }
        public AXRPlayerConfig config { get; private set; }
        public bool isOnStreaming => _playerID >= 0 && AXRServerPlugin.IsOnStreaming(_playerID);

        public void Init() {
            input = new AXRServerInput();

            AXRServerEventLoop.instance.RegisterListener(this);
        }

        public void Cleanup() {
            AXRServerEventLoop.instance.UnregisterListener(this);

            input = null;
        }

        public void RegisterEventHandler(EventHandler handler) {
            if (_eventHandlers.Contains(handler)) { return; }

            _eventHandlers.Add(handler);
        }

        public void UnregisterEventHandler(EventHandler handler) {
            if (_eventHandlers.Contains(handler) == false) { return; }

            _eventHandlers.Remove(handler);
        }

        // implements AXRServerEventLoop.Listener
        void AXRServerEventLoop.Listener.OnMessageReceived(AXRServerMessage message) {
            var playerID = message.source.ToInt32();

            if (message.IsSessionEvent()) {
                if (message.Name.Equals(AXRServerMessage.NameConnected)) {
                    _playerID = playerID;
                }
                else if (message.Name.Equals(AXRServerMessage.NameDisconnected)) {
                    _playerID = InvalidPlayerID;
                }
            }
            if (_playerID == InvalidPlayerID || playerID != _playerID) { return; }

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
                else if (message.Name.Equals(AXRServerMessage.NameDisconnected)) {
                    notifyDisconnected();

                    config = null;
                }
            }
            else if (message.Type.Equals(AXRMessage.TypeUserData)) {
                onUserdataReceived(message);
            }
        }

        void AXRServerEventLoop.Listener.OnUpdate() {
            ensureAudioListenerConfigured();
        }

        void AXRServerEventLoop.Listener.OnLateUpdate() {}

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
