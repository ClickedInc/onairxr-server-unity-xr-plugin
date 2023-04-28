using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Server {
    public class AXRVolume : MonoBehaviour {
        private Transform _thisTransform;

        private void Awake() {
            _thisTransform = transform;
        }

        private void Update() {
            AXRServerPlugin.UpdateVolumeInfo(_thisTransform.position.x, _thisTransform.position.y, _thisTransform.position.z,
                                             _thisTransform.rotation.x, _thisTransform.rotation.y, _thisTransform.rotation.z, _thisTransform.rotation.w,
                                             _thisTransform.lossyScale.x, _thisTransform.lossyScale.y, _thisTransform.lossyScale.z);
        }
    }
}
