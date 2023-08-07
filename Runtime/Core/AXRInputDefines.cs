/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

namespace onAirXR.Server {
    public enum AXRInputDeviceID : byte {
        HeadTracker = 0,
        LeftHandTracker = 1,
        RightHandTracker = 2,
        Controller = 3,
        TouchScreen = 4
    }

    internal enum AXRDeviceStatus : byte {
        Unavailable = 0,
        Ready
    }
}
