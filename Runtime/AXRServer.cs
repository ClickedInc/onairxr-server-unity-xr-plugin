using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Server {
    internal class AXRServer : AXRServerEventLoop.Listener {
        private static AXRServer _instance;

        public static AXRServer instance {
            get {
                if (_instance == null) {
                    _instance = new AXRServer();
                }
                return _instance;
            }
        }

        private AXRAudioListener _currentAudioListener;

        public void Init() {
            AXRServerEventLoop.instance.RegisterListener(this);
        }

        public void Cleanup() {
            AXRServerEventLoop.instance.UnregisterListener(this);
        }

        // implements AXRServerEventLoop.Listener
        void AXRServerEventLoop.Listener.OnMessageReceived(AXRServerMessage message) {
            var playerID = message.source.ToInt32();

            // nothing to do for now
        }

        void AXRServerEventLoop.Listener.OnUpdate() {
            if (_currentAudioListener != null) { return; }

            var audioListener = Object.FindObjectOfType<AudioListener>(true);
            if (audioListener == null) { return; }

            _currentAudioListener = audioListener.GetComponent<AXRAudioListener>();
            if (_currentAudioListener == null) {
                _currentAudioListener = audioListener.gameObject.AddComponent<AXRAudioListener>();
            }
        }

        void AXRServerEventLoop.Listener.OnLateUpdate() {}
    }
}
