using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Server {
    [RequireComponent(typeof(Camera))]
    public class AXRChromaKeyCamera : MonoBehaviour {
        public enum Preset {
            Green,
            Blue,
            Custom
        }

        private Camera _camera;

        [SerializeField] private Preset _preset = Preset.Green;
        [SerializeField] private Color _clearColor = new Color(0, 0.6431f, 0, 0);
        [SerializeField] private Color _keyColor = Color.green;
        [SerializeField] [Range(0, 1)] private float _similarity = 0.42f;
        [SerializeField] [Range(0, 1)] private float _smoothness = 0.08f;
        [SerializeField] [Range(0, 1)] private float _spill = 0.1f;

        private Color clearColor => _preset switch {
            Preset.Green => new Color(0, 0.6431f, 0, 0),
            Preset.Blue => new Color(0, 0, 0.6431f, 0),
            _ => _clearColor
        };

        private Color keyColor => _preset switch {
            Preset.Green => Color.green,
            Preset.Blue => Color.blue,
            _ => _keyColor
        };

        private float similarity => _preset switch {
            Preset.Green => 0.42f,
            Preset.Blue => 0.42f,
            _ => _similarity
        };

        private float smoothness => _preset switch {
            Preset.Green => 0.08f,
            Preset.Blue => 0.08f,
            _ => _smoothness
        };

        private float spill => _preset switch {
            Preset.Green => 0.1f,
            Preset.Blue => 0.1f,
            _ => _spill
        };

        private void Awake() {
            _camera = GetComponent<Camera>();
        }

        private void Start() {
            _camera.clearFlags = CameraClearFlags.SolidColor;
        }

        private void Update() {
            _camera.backgroundColor = clearColor;

            AXRServerPlugin.UpdateChromaKeyProps(keyColor.r, keyColor.g, keyColor.b, similarity, smoothness, spill);
        }

        private void OnDestroy() {
            AXRServerPlugin.UpdateChromaKeyProps(0, 0, 0, 0, 0, 0);
        }
    }
}
