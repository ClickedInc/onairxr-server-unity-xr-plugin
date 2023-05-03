using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Server {
    [RequireComponent(typeof(MeshFilter))]
    public class AXRVolume : MonoBehaviour {
        public static void ClearConfiguration() {
            AXRServerPlugin.ConfigureVolumeMesh(null);
        }

        private Transform _thisTransform;
        private MeshFilter _meshFilter;
        private MeshRenderer _renderer;
        private bool _configured;

        public bool Configure() {
            var mesh = _meshFilter?.sharedMesh;
            if (mesh != null) {
                AXRServerPlugin.ConfigureVolumeMesh(mesh);
            }

            _configured = mesh != null;
            return _configured;
        }

        private void Awake() {
            AXRServer.instance.currentVolume = this;

            _thisTransform = transform;
            _meshFilter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();

            var mesh = _meshFilter?.sharedMesh;
            if (mesh == null) {
                throw new UnityException($"[ERROR] AXRVolume ({name}) requires a MeshFilter with a valid mesh.");
            }
            else if (mesh.subMeshCount == 0 || mesh.vertexCount == 0) {
                throw new UnityException("[ERROR] AXRVolume ({name}) requires a valid mesh with vertices.");
            }
            else if (mesh.GetTopology(0) != MeshTopology.Triangles) {
                throw new UnityException("[ERROR] AXRVolume ({name}) requires a valid mesh of triangle topology.");
            }
        }

        private void Start() {
            if (_renderer != null) {
                _renderer.enabled = false;
            }

            Configure();
        }

        private void Update() {
            if (_configured == false) { return; }

            AXRServerPlugin.UpdateVolumeInfo(_thisTransform.position.x, _thisTransform.position.y, _thisTransform.position.z,
                                             _thisTransform.rotation.x, _thisTransform.rotation.y, _thisTransform.rotation.z, _thisTransform.rotation.w,
                                             _thisTransform.lossyScale.x, _thisTransform.lossyScale.y, _thisTransform.lossyScale.z);
        }
    }
}
