using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ONAIRXR_EXPERIMENTAL

namespace onAirXR.Server {
    public class AXRChromaKeyCamera : MonoBehaviour {
        internal static void ClearConfiguration() {
            AXRServerPlugin.UpdateChromaKeyProps(0, 0, 0, 0, 0, 0);
        }

        [SerializeField] private Color _keyColor = Color.green;
        [SerializeField] private float _similarity = 0.42f;
        [SerializeField] private float _smoothness = 0.08f;
        [SerializeField] private float _spill = 0.1f;

        private void Awake() {
            AXRServer.instance.currentChromaKey = this;
        }

        private void Update() {
            AXRServerPlugin.UpdateChromaKeyProps(_keyColor.r, _keyColor.g, _keyColor.b, _similarity, _smoothness, _spill);
        }
    }
}

#endif
