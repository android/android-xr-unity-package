// <copyright file="XRInstanceManagerApi.cs" company="Google LLC">
//
// Copyright 2024 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.Extensions.Internal
{
    using System;
    using System.Runtime.InteropServices;

    internal enum ApiXrFeature
    {
        Trackable = 0,
        BlendMode = 1,
        FaceTracking = 2,
        PassthroughCameraState = 3,
        Passthrough = 9,
        SceneMeshing = 14,
        HandMesh = 15,
        UnboundedRefSpace = 16,
        Reserved3 = 18,
        Reserved4 = 19,
        BodyTracking = 20,
        SystemState = 21,
        Reserved7 = 22,
        Reserved8 = 23,
    }

    internal class XRInstanceManagerApi
    {
        private static IntPtr _instanceManager = IntPtr.Zero;

        public static IntPtr GetIntPtr() => _instanceManager;

        public static IntPtr Intercept(IntPtr func)
        {
            return ExternalApi.intercept_xrGetInstanceProcAddr(func);
        }

        public static bool Create(ulong instance, IntPtr procAddr)
        {
            // Only needs to initialize instance manager one time.
            if (_instanceManager != IntPtr.Zero)
            {
                return true;
            }

            ExternalApi.XrInstanceManager_create(instance, procAddr, ref _instanceManager);
            return _instanceManager != IntPtr.Zero;
        }

        public static void Destroy()
        {
            if (_instanceManager == IntPtr.Zero)
            {
                return;
            }

            ExternalApi.XrInstanceManager_destroy(_instanceManager);
            _instanceManager = IntPtr.Zero;
        }

        public static bool Register(ApiXrFeature feature)
        {
            return ExternalApi.XrInstanceManager_registerFeature((uint)feature);
        }

        public static void Unregister(ApiXrFeature feature)
        {
            ExternalApi.XrInstanceManager_unregisterFeature((uint)feature);
        }

        public static void OnSystemChange(ulong system)
        {
            ExternalApi.XrInstanceManager_onSystemChange(system);
        }

        public static void OnSessionCreate(ulong session)
        {
            ExternalApi.XrInstanceManager_onSessionCreate(session);
        }

        public static void OnSessionBegin(ulong session)
        {
            ExternalApi.XrInstanceManager_onSessionBegin(session);
        }

        public static void OnSessionEnd(ulong session)
        {
            ExternalApi.XrInstanceManager_onSessionEnd(session);
        }

        public static void OnSessionDestroy(ulong session)
        {
            ExternalApi.XrInstanceManager_onSessionDestroy(session);
        }

        public static void OnSessionStateChange(int oldState, int newState)
        {
            ExternalApi.XrInstanceManager_onSessionStateChange(oldState, newState);
        }

        public static void OnAppSpaceChange(ulong space)
        {
            ExternalApi.XrInstanceManager_onAppSpaceChange(space);
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern IntPtr intercept_xrGetInstanceProcAddr(IntPtr func);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrInstanceManager_create(
                ulong instance, IntPtr proc_addr, ref IntPtr out_manager);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrInstanceManager_destroy(IntPtr manager);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern bool XrInstanceManager_registerFeature(uint feature);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrInstanceManager_unregisterFeature(uint feature);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrInstanceManager_onSystemChange(ulong system);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrInstanceManager_onSessionCreate(ulong session);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrInstanceManager_onSessionBegin(ulong session);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrInstanceManager_onSessionEnd(ulong session);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrInstanceManager_onSessionDestroy(ulong session);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrInstanceManager_onSessionStateChange(
                int old_state, int new_state);

            [DllImport(ApiConstants.OpenXRAndroidApi)]
            public static extern void XrInstanceManager_onAppSpaceChange(ulong space);
        }
    }
}
