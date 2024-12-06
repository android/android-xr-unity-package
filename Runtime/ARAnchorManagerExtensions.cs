// <copyright file="ARAnchorManagerExtensions.cs" company="Google LLC">
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

namespace Google.XR.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;

    /// <summary>
    /// Extensions to AR Foundation's <c><see cref="ARAnchorManager"/></c> class.
    /// </summary>
    public static class ARAnchorManagerExtensions
    {
        /// <summary>
        /// Gets the persistent id associated with the given anchor, it can be used in
        /// <see cref="TryLoad(ARAnchorManager, Guid)"/> to retrieve the persisted anchor across
        /// multiple sessions.
        /// If <paramref name="anchor"/> is not a persisted anchor, returns <c>Guid.Empty</c>.
        /// </summary>
        /// <param name="anchorManager">The <see cref="ARAnchorManager"/> instance.</param>
        /// <param name="anchor">The anchor to get persistent id.</param>
        /// <returns>The persistent id of the given anchor.</returns>
        public static Guid GetPersistentId(this ARAnchorManager anchorManager, ARAnchor anchor)
        {
            var subsystem = GetSubsystem(anchorManager);
            if (subsystem == null || anchor == null)
            {
                return Guid.Empty;
            }

            return subsystem.GetPersistentId(anchor.trackableId);
        }

        /// <summary>
        /// Gets all persisted ids that are currently saved on the device's local storage. It can be
        /// used in <see cref="TryLoad(ARAnchorManager, Guid)"/> to retrieve the persisted anchors
        /// across multiple sessions or removed by <see cref="Unpersist(ARAnchorManager, Guid)"/>.
        /// </summary>
        /// <param name="anchorManager">The <see cref="ARAnchorManager"/> instance.</param>
        /// <param name="guids">A list of all persisted ids.</param>
        /// <returns>Returns a boolean to indicate whether all stored ids were retrieved
        /// successfully.
        /// </returns>
        public static bool TryGetAllPersistedIds(
            this ARAnchorManager anchorManager, ref List<Guid> guids)
        {
            var subsystem = GetSubsystem(anchorManager);
            if (subsystem == null)
            {
                return false;
            }

            return subsystem.TryGetAllPersistedIds(ref guids);
        }

        /// <summary>
        /// Try to load a persisted anchor from the given <paramref name="guid"/>.
        /// If it succeeds, a new <see cref="ARAnchor"/> will be returned through
        /// <see cref="ARAnchorsChangedEventArgs.added"/> list.
        /// Use either <see cref="ARAnchorExtensions.GetPersistentId(ARAnchor)"/> or
        /// <see cref="GetPersistentId(ARAnchorManager, ARAnchor)"/> to confirm if it's the
        /// requested anchor.
        /// </summary>
        /// <param name="anchorManager">The <see cref="ARAnchorManager"/> instance.</param>
        /// <param name="guid">The <see cref="Guid"/> that represents a persisted anchor's id.
        /// </param>
        /// <returns>Returns a boolean to indicate whether successfully loaded the given
        /// <paramref name="guid"/>.</returns>
        public static bool TryLoad(this ARAnchorManager anchorManager, Guid guid)
        {
            var subsystem = GetSubsystem(anchorManager);
            if (subsystem == null || guid == Guid.Empty)
            {
                return false;
            }

            return subsystem.TryLoad(guid);
        }

        /// <summary>
        /// Try to save the <paramref name="anchor"/> on device's local storage.
        /// If it succeeds, use either <see cref="ARAnchorExtensions.GetPersistentId(ARAnchor)"/> or
        /// <see cref="GetPersistentId(ARAnchorManager, ARAnchor)"/> to get the associated
        /// persistent id, then use <see cref="TryLoad(ARAnchorManager, Guid)"/> to retrieve the
        /// anchor from different sessions.
        /// </summary>
        /// <param name="anchorManager">The <see cref="ARAnchorManager"/> instance.</param>
        /// <param name="anchor">The anchor to save on device's local storage.</param>
        /// <returns>Returns a boolean to indicate whether the anchor was successfully saved on the
        /// device's local storage.</returns>

        public static bool Persist(
            this ARAnchorManager anchorManager, ARAnchor anchor)
        {
            var subsystem = GetSubsystem(anchorManager);
            if (subsystem == null || anchor == null)
            {
                return false;
            }

            return subsystem.Persist(anchor.trackableId);
        }

        /// <summary>
        /// Try to remove a persisted anchor from device's local storage.
        /// </summary>
        /// <param name="anchorManager">The <see cref="ARAnchorManager"/> instance.</param>
        /// <param name="guid">The persistent id associated with the anchor to remove.</param>
        /// <returns>Returns a boolean to indicate whether the anchor was removed successfully from
        /// the device's local storage.</returns>
        public static bool Unpersist(this ARAnchorManager anchorManager, Guid guid)
        {
            var subsystem = GetSubsystem(anchorManager);
            if (subsystem == null || guid == Guid.Empty)
            {
                return false;
            }

            return subsystem.Unpersist(guid);
        }

        /// <summary>
        /// Try to remove a persisted anchor from device's local storage.
        /// </summary>
        /// <param name="anchorManager">The <see cref="ARAnchorManager"/> instance.</param>
        /// <param name="anchor">The persisted anchor to remove.</param>
        /// <returns>Returns a boolean to indicate whether the anchor was removed successfully from
        /// the device's local storage.</returns>
        public static bool Unpersist(this ARAnchorManager anchorManager, ARAnchor anchor)
        {
            var subsystem = GetSubsystem(anchorManager);
            if (subsystem == null || anchor == null)
            {
                return false;
            }

            return subsystem.Unpersist(anchor.trackableId);
        }

        /// <summary>
        /// Get persistence state of the given anchor.
        /// </summary>
        /// <param name="anchorManager">The <see cref="ARAnchorManager"/> instance.</param>
        /// <param name="anchor">The persisted anchor to get state from.</param>
        /// <param name="state">The state of the anchor.</param>
        /// <returns>Returns a boolean to indicate whether successfully got persistence state.
        /// </returns>
        public static bool GetPersistState(
            this ARAnchorManager anchorManager, ARAnchor anchor,
            out XRAnchorPersistStates state)
        {
            var subsystem = GetSubsystem(anchorManager);
            if (subsystem == null || anchor == null)
            {
                state = XRAnchorPersistStates.NotRequested;
                return false;
            }

            return subsystem.GetPersistState(anchor.trackableId, out state);
        }

        /// <summary>
        /// Get persistence state of the given anchor by GUID.
        /// </summary>
        /// <param name="anchorManager">The <see cref="ARAnchorManager"/> instance.</param>
        /// <param name="guid">The GUID associated with the anchor to get state from.</param>
        /// <param name="state">The state of the anchor.</param>
        /// <returns>Returns a boolean to indicate whether successfully got persistence state.
        /// </returns>
        public static bool GetPersistState(
            this ARAnchorManager anchorManager, Guid guid, out XRAnchorPersistStates state)
        {
            var subsystem = GetSubsystem(anchorManager);
            if (subsystem == null || guid == Guid.Empty)
            {
                state = XRAnchorPersistStates.NotRequested;
                return false;
            }

            return subsystem.GetPersistState(guid, out state);
        }

        /// <summary>
        /// Wait until an anchor has a given persistence state.
        /// </summary>
        /// <param name="anchorManager">The <see cref="ARAnchorManager"/> instance.</param>
        /// <param name="guid">The GUID associated with the anchor to get state from.</param>
        /// <param name="targetState">The target state of the anchor.</param>
        /// <param name="onComplete">Action called with "true" if anchor reaches target state.
        /// Called with false if the timeout is reached.</param>
        /// <param name="timeout">Optional timeout in seconds. A value of zero indicates no
        /// timeout.</param>
        /// <returns>A coroutine that waits until the anchor has the target state or the timeout is
        /// reached.</returns>
        public static IEnumerator WaitUntilPersistState(
            this ARAnchorManager anchorManager, Guid guid,
            XRAnchorPersistStates targetState, Action<bool> onComplete,
            int timeout = 0)
        {
            var subsystem = GetSubsystem(anchorManager);
            if (subsystem == null || guid == Guid.Empty)
            {
                onComplete(false);
                yield break;
            }

            int duration = 0;
            while (true)
            {
                if (anchorManager.GetPersistState(guid, out var state))
                {
                    if (state == targetState)
                    {
                        onComplete(true);
                        yield break;
                    }
                }

                if (timeout != 0)
                {
                    if (duration >= timeout)
                    {
                        onComplete(false);
                        yield break;
                    }

                    duration += 1;
                }

                yield return new WaitForSeconds(1.0f);
            }
        }

        private static AndroidXRAnchorSubsystem GetSubsystem(ARAnchorManager anchorManager)
        {
            if (anchorManager.subsystem == null)
            {
                Debug.LogError("ARAnchorManagerExtensions: subsystem not ready.");
                return null;
            }

            AndroidXRAnchorSubsystem subsystem =
                anchorManager.subsystem as AndroidXRAnchorSubsystem;
            if (subsystem == null)
            {
                Debug.LogErrorFormat("ARAnchorManagerExtensions: invalid subsystem {0}.",
                    anchorManager.subsystem.GetType());
            }

            return subsystem;
        }
    }
}
