using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace onAirXR.Server {
    internal static class AXRServerPlugin {
        private const string LibName = "axr";

        [DllImport(LibName, EntryPoint = "axr_configure")] 
        public extern static bool Configure(float minFrameRate, 
                                            float maxFrameRate, 
                                            int audioSampleRate, 
                                            int framebufferTextureColorSpaceHint, 
                                            bool cpuReadableEncodeBuffer,
                                            int codecs,
                                            int encodingPreset,
                                            int encodingPerformance);

        public static void ConfigureVolumeMesh(Mesh mesh) {
            if (mesh == null || mesh.subMeshCount == 0 || mesh.vertexCount == 0 || mesh.GetTopology(0) != MeshTopology.Triangles) { 
                axr_configureVolumeMesh(null, 0, null, 0);
                return; 
            }

            var vertices = new float[mesh.vertexCount * 3];
            for (var index = 0; index < mesh.vertexCount; index++) {
                var vertex = mesh.vertices[index];

                vertices[index * 3]     = vertex.x;
                vertices[index * 3 + 1] = vertex.y;
                vertices[index * 3 + 2] = vertex.z;
            }

            var indices = mesh.GetIndices(0);
            axr_configureVolumeMesh(vertices, vertices.Length, indices, indices.Length);
        }

        [DllImport(LibName)] private extern static void axr_configureVolumeMesh(float[] vertices, int vertexCount, int[] indices, int indexCount);

        [DllImport(LibName, EntryPoint = "axr_updateChromaKeyProps")]
        public extern static void UpdateChromaKeyProps(float r, float g, float b, float similarity, float smoothness, float spill);

        [DllImport(LibName, EntryPoint = "axr_updateVolumeInfo")]
        public extern static void UpdateVolumeInfo(float posx, float posy, float posz, float rotx, float roty, float rotz, float rotw, float scalex, float scaley, float scalez);

        [DllImport(LibName, EntryPoint = "axr_startupServer")]
        public extern static int Startup(string licenseFile, int portSTAP, int portAMP, bool loopbackOnlyForSTAP);

        public static void Shutdown() {
            GL.IssuePluginEvent(axr_shutdownServer_RenderThread_Func(), 0);
            axr_shutdownServer();
        }

        [DllImport(LibName)] private extern static IntPtr axr_shutdownServer_RenderThread_Func();
        [DllImport(LibName)] private extern static void axr_shutdownServer();
        [DllImport(LibName)] private extern static bool axr_peekMessage(out IntPtr source, out IntPtr data, out int length);
        [DllImport(LibName)] private extern static void axr_popMessage();

        public static bool GetNextServerMessage(out AXRServerMessage message) {
            message = null;

            if (axr_peekMessage(out IntPtr source, out IntPtr data, out int length) == false) { return false; }

            var array = new byte[length];
            Marshal.Copy(data, array, 0, length);
            axr_popMessage();

            message = AXRServerMessage.Parse(source, System.Text.Encoding.UTF8.GetString(array, 0, length));
            return true;
        }

        [DllImport(LibName, EntryPoint = "axr_sendAudioFrame")]
        public extern static void SendAudioFrame(float[] data, int sampleCount, int channels, double timestamp);

        [DllImport(LibName, EntryPoint = "axr_isOnStreaming")]
        public extern static bool IsOnStreaming(int playerID);

        [DllImport(LibName)] private extern static bool axr_getConfig(int playerID, out IntPtr data, out int length);

        public static AXRPlayerConfig GetConfig(int playerID) {
            if (axr_getConfig(playerID, out IntPtr data, out int length) == false) { return null; }

            var array = new byte[length];
            Marshal.Copy(data, array, 0, length);
            var json = System.Text.Encoding.UTF8.GetString(array, 0, length);
            return JsonUtility.FromJson<AXRPlayerConfig>(json);
        }

        [DllImport(LibName, EntryPoint = "axr_isProfiling")]
        public extern static bool IsProfiling(int playerID);

        [DllImport(LibName, EntryPoint = "axr_isRecording")]
        public extern static bool IsRecording(int playerID);

        [DllImport(LibName, EntryPoint = "axr_requestConfigureSession")]
        public extern static void RequestConfigureSession(int playerID, ulong minBitrate, ulong startBitrate, ulong maxBitrate);

        [DllImport(LibName, EntryPoint = "axr_requestImportSessionData")]
        public extern static void RequestImportSessionData(int playerID, string path);

        [DllImport(LibName, EntryPoint = "axr_requestRecordSession")]
        public extern static void RequestRecordSession(int playerID, string targetPath);

        [DllImport(LibName, EntryPoint = "axr_startRecordVideo")]
        public extern static void StartRecordVideo(int playerID, string outputPathWithoutExtension, int outputFormat, string sessionDataName);

        [DllImport(LibName, EntryPoint = "axr_stopRecordVideo")]
        public extern static void StopRecordVideo(int playerID);

        [DllImport(LibName, EntryPoint = "axr_requestPlay")] 
        public extern static void RequestPlay(int playerID, string sessionDataName);

        [DllImport(LibName, EntryPoint = "axr_requestStop")] 
        public extern static void RequestStop(int playerID);

        [DllImport(LibName, EntryPoint = "axr_requestStartProfile")]
        public extern static void RequestStartProfile(int playerID, string directory, string filename, string sessionDataName);

        [DllImport(LibName, EntryPoint = "axr_requestStopProfile")]
        public extern static void RequestStopProfile(int playerID);

        [DllImport(LibName, EntryPoint = "axr_requestQuery")]
        public extern static void RequestQuery(int playerID, string statement);
    }
}
