using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace onAirXR.Server {
    [RequireComponent(typeof(MeshFilter))]
    public class AXRVolume : MonoBehaviour {
        private Transform _thisTransform;
        private MeshFilter _meshFilter;
        private MeshRenderer _renderer;
        private Transform _cameraSpace;
        private bool _configured;

        [SerializeField] private Camera _camera = null;

        private void Awake() {
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
        }

        private void OnEnable() {
            configure();
        }

        private void OnDisable() {
            AXRServerPlugin.ConfigureVolumeMesh(null);
            _configured = false;
        }

        private void Update() {
            if (_configured == false) { return; }

            _cameraSpace = findCameraSpace();

            var position = _cameraSpace != null ? _cameraSpace.InverseTransformPoint(_thisTransform.position) : _thisTransform.position;
            var rotation = _cameraSpace != null ? Quaternion.Inverse(_cameraSpace.rotation) * _thisTransform.rotation : _thisTransform.rotation;

            AXRServerPlugin.UpdateVolumeInfo(position.x, position.y, position.z,
                                             rotation.x, rotation.y, rotation.z, rotation.w,
                                             _thisTransform.lossyScale.x, _thisTransform.lossyScale.y, _thisTransform.lossyScale.z);
        }

        private void configure() {
            var mesh = _meshFilter?.sharedMesh;
            if (mesh != null) {
                AXRServerPlugin.ConfigureVolumeMesh(mesh);
            }

            _configured = mesh != null;
        }

        private Transform findCameraSpace() {
            if (_cameraSpace != null && _cameraSpace.gameObject.activeInHierarchy) {
                return _cameraSpace;
            }

            if (_camera != null && _camera.gameObject.activeInHierarchy) {
                return _camera.transform.parent;
            }

            var camera = Camera.main;
            if (camera != null && camera.gameObject.activeInHierarchy) {
                return camera.transform.parent;
            }

            return null;
        }
    }
}
