using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace onAirXR.Server {
    public class AXRServerInput {
        private List<InputDevice> _inputMain = new List<InputDevice>();
        private List<InputDevice> _inputLeftController = new List<InputDevice>();
        private List<InputDevice> _inputRightController = new List<InputDevice>();

        private InputDeviceCharacteristics mainCharacteristics => InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.TrackedDevice;
        private InputDeviceCharacteristics leftControllerCharacteristics => InputDeviceCharacteristics.Left | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller;
        private InputDeviceCharacteristics rightControllerCharacteristics => InputDeviceCharacteristics.Right | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller;

        public AXRServerInput() {
            InputDevices.GetDevicesWithCharacteristics(mainCharacteristics, _inputMain);
            InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, _inputLeftController);
            InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, _inputRightController);

            InputDevices.deviceConnected += onInputDeviceConnected;
            InputDevices.deviceDisconnected += onInputDeviceDisconnected;
        }

        public void Cleanup() {
            InputDevices.deviceConnected -= onInputDeviceConnected;
            InputDevices.deviceDisconnected -= onInputDeviceDisconnected;
        }

        public bool IsDeviceConnected(AXRInputDeviceID device) {
            switch (device) {
                case AXRInputDeviceID.HeadTracker:
                    return _inputMain.Count > 0;
                case AXRInputDeviceID.LeftHandTracker:
                    return _inputLeftController.Count > 0;
                case AXRInputDeviceID.RightHandTracker:
                    return _inputRightController.Count > 0;
                case AXRInputDeviceID.Controller:
                    return _inputLeftController.Count > 0 || _inputRightController.Count > 0;
                default:
                    return false;
            }
        }

        public bool TryGetFeatureValue(AXRInputDeviceID device, InputFeatureUsage<float> usage, ref float value) {
            switch (device) {
                case AXRInputDeviceID.HeadTracker:
                    return tryGetFeatureValue(_inputMain, usage, ref value);
                case AXRInputDeviceID.LeftHandTracker:
                    return tryGetFeatureValue(_inputLeftController, usage, ref value);
                case AXRInputDeviceID.RightHandTracker:
                    return tryGetFeatureValue(_inputRightController, usage, ref value);
                case AXRInputDeviceID.Controller:
                    if (tryGetFeatureValue(_inputLeftController, usage, ref value)) { return true; }
                    return tryGetFeatureValue(_inputRightController, usage, ref value);
                default:
                    return false;
            }
        }

        private void onInputDeviceConnected(InputDevice device) {
            if (device.characteristics.HasFlag(mainCharacteristics)) {
                addInputDevice(_inputMain, device);
            }
            else if (device.characteristics.HasFlag(leftControllerCharacteristics)) {
                addInputDevice(_inputLeftController, device);
            }
            else if (device.characteristics.HasFlag(rightControllerCharacteristics)) {
                addInputDevice(_inputRightController, device);
            }
        }

        private void onInputDeviceDisconnected(InputDevice device) {
            if (_inputMain.Contains(device)) {
                _inputMain.Remove(device);
            }
            if (_inputLeftController.Contains(device)) {
                _inputLeftController.Remove(device);
            }
            if (_inputRightController.Contains(device)) {
                _inputRightController.Remove(device);
            }
        }

        private void addInputDevice(List<InputDevice> devices, InputDevice device) {
            if (devices.Contains(device)) { return; }

            devices.Add(device);
        }

        private bool tryGetFeatureValue(List<InputDevice> devices, InputFeatureUsage<float> usage, ref float value) {
            if (devices.Count == 0) { return false; }

            return devices[0].TryGetFeatureValue(usage, out value);
        }
    }
}
