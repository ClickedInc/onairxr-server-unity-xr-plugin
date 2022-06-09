using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Server {
    [RequireComponent(typeof(AudioListener))]
    public class AXRAudioListener : MonoBehaviour {
        private static AXRAudioListener _instance;

        private void Awake() {
            if (_instance != null) {
                throw new UnityException("[ERROR] there must be only one instance of AXRAudioListener at a time.");
            }
            _instance = this;

            hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
        }

        private void OnAudioFilterRead(float[] data, int channels) {
            AXRServerPlugin.SendAudioFrame(data, data.Length / channels, channels, AudioSettings.dspTime);
        }
    }
}
