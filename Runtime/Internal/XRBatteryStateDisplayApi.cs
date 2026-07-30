// <copyright file="XRBatteryStateDisplayApi.cs" company="Google LLC">
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
    using UnityEngine;

    internal static class XRBatteryStateDisplayApi
    {
        /// <summary>
        /// Matches XrBatteryStateDisplayStateFlagsEXT.
        /// </summary>
        [Flags]
        internal enum XRBatteryStateDisplayStates
        {
            /// <summary>
            /// The battery state is valid.
            /// </summary>
            Valid = 1 << 0,

            /// <summary>
            /// The battery is charging.
            /// </summary>
            Charging = 1 << 1,

            /// <summary>
            /// The battery is plugged in to a power source.
            /// </summary>
            PluggedIn = 1 << 2,

            /// <summary>
            /// The device does not have a battery.
            /// </summary>
            NoBattery = 1 << 3,
        }

        public static bool TryGetBatteryStateDisplay(
            string topLevelUserPath, out BatteryStatus status,
            out float level)
        {
            XRBatteryStateDisplay batteryState = default;
            if (!ExternalApi.OpenXRAndroid_getBatteryStateDisplay(
                XRInstanceManagerApi.GetIntPtr(), topLevelUserPath, ref batteryState))
            {
                status = BatteryStatus.Unknown;
                level = 1.0f;
                return false;
            }

            if (!batteryState.State.HasFlag(XRBatteryStateDisplayStates.Valid))
            {
                status = BatteryStatus.Unknown;
                level = 1.0f;
                return true;
            }

            level = batteryState.Level;
            if (batteryState.State.HasFlag(XRBatteryStateDisplayStates.Charging))
            {
                status = BatteryStatus.Charging;
            }
            else if (batteryState.State.HasFlag(XRBatteryStateDisplayStates.PluggedIn))
            {
                status = BatteryStatus.NotCharging;
            }
            else
            {
                status = BatteryStatus.Discharging;
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XRBatteryStateDisplay
        {
            /// <summary>
            /// The current battery state.
            /// </summary>
            public XRBatteryStateDisplayStates State;

            /// <summary>
            /// The battery level as a float between 0 and 1 (If current state is valid).
            /// </summary>
            public float Level;
        }

        private struct ExternalApi
        {
            [DllImport(ApiConstants.OpenXRAndroidApi, CharSet = CharSet.Ansi)]
            public static extern bool OpenXRAndroid_getBatteryStateDisplay(
                IntPtr manager, string topLevelUserPath,
                ref XRBatteryStateDisplay batteryState);
        }
    }
}
