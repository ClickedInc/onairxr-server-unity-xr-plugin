using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace onAirXR.Server {
    internal class AXRServerEventLoop : MonoBehaviour {
        public interface Listener {
            void OnMessageReceived(AXRServerMessage message);
            void OnUpdate();
            void OnLateUpdate();
        }

        private static AXRServerEventLoop _instance;

        public static AXRServerEventLoop instance {
            get {
                if (_instance == null) {
                    var go = new GameObject("AXRServerEventLoop");
                    go.hideFlags = HideFlags.HideAndDontSave;
                    DontDestroyOnLoad(go);
                    
                    _instance = go.AddComponent<AXRServerEventLoop>();
                }
                return _instance;
            }
        }

        private HashSet<Listener> _listeners = new HashSet<Listener>();

        public void RegisterListener(Listener listener) {
            if (_listeners.Contains(listener)) { return; }

            _listeners.Add(listener);
        }

        public void UnregisterListener(Listener listener) {
            if (_listeners.Contains(listener) == false) { return; }

            _listeners.Remove(listener);
        }

        private void Update() {
            processServerMessages();

            foreach (var listener in _listeners) {
                listener?.OnUpdate();
            }
        }

        private void LateUpdate() {
            foreach (var listener in _listeners) {
                listener?.OnLateUpdate();
            }
        }

        private void processServerMessages() {
            AXRServerMessage message;
            while (AXRServerPlugin.GetNextServerMessage(out message)) {
                notifyMessageReceived(message);
            }
        }

        private void notifyMessageReceived(AXRServerMessage message) {
            foreach (var listener in _listeners) {
                listener?.OnMessageReceived(message);
            }
        }
    }
}
