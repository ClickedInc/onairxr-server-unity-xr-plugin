using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace onAirXR.Server {
    internal static class AXRServerPlugin {
        private const string LibName = "axr";

        [DllImport(LibName, EntryPoint = "axr_configure")] 
        public extern static void Configure(string license, int portSTAP, int portAMP, bool loopbackOnlyForSTAP, float minFrameRate, float maxFrameRate, int audioSampleRate, int renderPass, int framebufferTextureColorSpaceHint);

        [DllImport(LibName, EntryPoint = "axr_setRecordSettings")]
        public extern static void SetRecordSettings(bool enable, string outputPathWithoutExtension, int outputFormat);

        [DllImport(LibName)] private extern static bool axr_peekMessage(out IntPtr source, out IntPtr data, out int length);
        [DllImport(LibName)] private extern static void axr_popMessage();

        public static bool GetNextServerMessage(out AXRServerMessage message) {
            message = null;

            IntPtr source, data;
            int length;

            if (axr_peekMessage(out source, out data, out length) == false) { return false; }

            var array = new byte[length];
            Marshal.Copy(data, array, 0, length);
            axr_popMessage();

            message = AXRServerMessage.Parse(source, System.Text.Encoding.UTF8.GetString(array, 0, length));
            return true;
        }

        [DllImport(LibName, EntryPoint = "axr_sendAudioFrame")]
        public extern static void SendAudioFrame(float[] data, int sampleCount, int channels, double timestamp);
    }
}
