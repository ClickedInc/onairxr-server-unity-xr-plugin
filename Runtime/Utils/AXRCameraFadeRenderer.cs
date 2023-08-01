using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_RENDER_PIPELINES_UNIVERSAL
using UnityEngine.Rendering.Universal;

namespace onAirXR.Server {
    public class AXRCameraFadeRenderer : ScriptableRendererFeature {
        private Dictionary<AXRCameraFade, AXRCameraFadeRenderPass> _renderPasses;

        public override void Create() {
            _renderPasses = new Dictionary<AXRCameraFade, AXRCameraFadeRenderPass>();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            if (renderingData.cameraData.cameraType != CameraType.Game) { return; }

            var fade = renderingData.cameraData.camera.GetComponent<AXRCameraFade>();
            if (fade == null) { return; }

            if (_renderPasses.ContainsKey(fade) == false) {
                _renderPasses.Add(fade, new AXRCameraFadeRenderPass(fade));
            }

            var renderPass = _renderPasses[fade];
            renderPass.ConfigureInput(ScriptableRenderPassInput.Color);

            renderer.EnqueuePass(renderPass);
        }

        protected override void Dispose(bool disposing) {
            _renderPasses.Clear();
        }
    }

    public class AXRCameraFadeRenderPass : ScriptableRenderPass {
        private AXRCameraFade _fade;

        public AXRCameraFadeRenderPass(AXRCameraFade fade) {
            _fade = fade;
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            _fade.Render(context);
        }
    }
}

#endif
