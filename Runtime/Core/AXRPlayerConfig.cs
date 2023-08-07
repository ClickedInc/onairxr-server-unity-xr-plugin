using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Server {
    public enum AXRPlayerType {
        Monoscopic,
        Stereoscopic
    }

    [Serializable]
    public class AXRPlayerConfig {
        internal AXRPlayerConfig() {
            CameraProjection = new float[4];
        }

        [SerializeField] private string UserID;
        [SerializeField] private string Place;
        [SerializeField] private bool Stereoscopy;
        [SerializeField] private int VideoWidth;
        [SerializeField] private int VideoHeight;
        [SerializeField] private float[] CameraProjection;
        [SerializeField] private float FrameRate;
        [SerializeField] private float InterpupillaryDistance;
        [SerializeField] private Vector3 EyeCenterPosition;
        [SerializeField] private ulong VideoStartBitrate;
        [SerializeField] private ulong VideoMaxBitrate;

        public AXRPlayerType type => Stereoscopy ? AXRPlayerType.Stereoscopic : AXRPlayerType.Monoscopic;
        public string userID => UserID;
        public string place => Place;

        internal int videoWidth => VideoWidth;
        internal int videoHeight => VideoHeight;
        internal float framerate => FrameRate;
        internal ulong videoStartBitrate => VideoStartBitrate;
        internal ulong videoMaxBitrate => VideoMaxBitrate;
        internal Vector3 eyeCenterPosition => EyeCenterPosition;
        internal float ipd => InterpupillaryDistance;
    }
}