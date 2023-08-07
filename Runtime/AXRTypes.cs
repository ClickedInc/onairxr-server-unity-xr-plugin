/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Runtime.InteropServices;
using UnityEngine;

namespace onAirXR.Server {
    public enum AXRRecordFormat {
        MP4 = 0,
        H264_HEVC,
        Lossless,
        Raw
    }

    public enum AXRCodec { 
        H264 = 0x1,
        H265 = 0x2,
        All = 0x3
    }

    public enum AXREncodingPreset { 
        LowLatency = 1,
        UltraLowLatency = 2
    }

    public enum AXREncodingQuality { 
        VeryLow = 0,
        Low,
        Moderate,
        High,
        VeryHigh
    }
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct AXRVector2D {
        public float x;
        public float y;

        public AXRVector2D(Vector2 value) {
            x = value.x;
            y = value.y;
        }

        public Vector3 toVector2() {
            return new Vector2(x, y);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AXRVector3D {
        public float x;
        public float y;
        public float z;

        public AXRVector3D(Vector3 value) {
            x = value.x;
            y = value.y;
            z = value.z;
        }

        public Vector3 toVector3() {
            return new Vector3(x, y, z);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AXRVector4D {
        public float x;
        public float y;
        public float z;
        public float w;

        public AXRVector4D(Quaternion value) {
            x = value.x;
            y = value.y;
            z = value.z;
            w = value.w;
        }

        public Quaternion toQuaternion() {
            return new Quaternion(x, y, z, w);
        }
    }
}
