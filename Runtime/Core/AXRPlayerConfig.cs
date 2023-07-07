﻿using System;
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
        public AXRPlayerConfig() {
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
        public int videoWidth => VideoWidth;
        public int videoHeight => VideoHeight;
        public float framerate => FrameRate;
        public Vector3 eyeCenterPosition => EyeCenterPosition;
        public float ipd => InterpupillaryDistance;
        public string userID => UserID;
        public string place => Place;
        public ulong videoStartBitrate => VideoStartBitrate;
        public ulong videoMaxBitrate => VideoMaxBitrate;
    }
}