using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Server {
    [RequireComponent(typeof(MeshFilter))]
    public class AXRVolume : MonoBehaviour {
        private Transform _thisTransform;
        private MeshFilter _meshFilter;
        private MeshRenderer _renderer;

        private void Awake() {
            _thisTransform = transform;
            _meshFilter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();

            if (_meshFilter?.sharedMesh == null) {
                throw new UnityException("[ERROR] AXRVolume requires a MeshFilter with a valid mesh.");
            }
        }

        private void Start() {
            if (_renderer != null) {
                _renderer.enabled = false;
            }

            AXRServerPlugin.ConfigureVolumeMesh(_meshFilter?.sharedMesh);
        }

        private void Update() {
            AXRServerPlugin.UpdateVolumeInfo(_thisTransform.position.x, _thisTransform.position.y, _thisTransform.position.z,
                                             _thisTransform.rotation.x, _thisTransform.rotation.y, _thisTransform.rotation.z, _thisTransform.rotation.w,
                                             _thisTransform.lossyScale.x, _thisTransform.lossyScale.y, _thisTransform.lossyScale.z);
        }
    }
}
