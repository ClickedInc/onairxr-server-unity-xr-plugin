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

        public void Init() {
            AXRServerEventLoop.instance.RegisterListener(this);
        }

        public void Cleanup() {
            AXRServerEventLoop.instance.UnregisterListener(this);
        }

        // implements AXRServerEventLoop.Listener
        void AXRServerEventLoop.Listener.OnMessageReceived(AXRServerMessage message) {
            var playerID = message.source.ToInt32();

            // TODO
        }

        void AXRServerEventLoop.Listener.OnUpdate() {}
        void AXRServerEventLoop.Listener.OnLateUpdate() {}
    }
}
