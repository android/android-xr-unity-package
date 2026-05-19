// <copyright file="XRFutureHandler.cs" company="Google LLC">
//
// Copyright 2026 Google LLC
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
    using System.Threading;
    using UnityEngine;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR.NativeTypes;

    using StatusCode = UnityEngine.XR.ARSubsystems.XRResultStatus.StatusCode;
    using XrFutureEXT = System.UInt64;

    /// <summary>
    /// Supports XrFutureEXT with polling-based asynchronous operations.
    /// </summary>
    /// <typeparam name="T">The type returned by OpenXR completion calls.
    /// </typeparam>
    /// <remarks>
    /// Possible inheritances: SpatialContext, SpatialDiscoverySnapshot,
    /// SpatialPersistenceContext, SpatialAnchors, PersistSpatialEntity,
    /// UnpersistSpatialEntity, etc.
    /// </remarks>
    internal abstract class XRFutureHandler<T> : IDisposable where T : struct
    {
        private XrFutureEXT _xrfuture = 0;
        private bool _isCancelled = false;
        private CancellationTokenSource _cancelToken = new CancellationTokenSource();
        private Result<T>? _result = null;

        ~XRFutureHandler()
        {
            Dispose();
        }

        /// <summary>
        /// Attempts to get the result from the XrFutureEXT handle.
        /// </summary>
        /// <param name="token">
        /// The <see cref="CancellationToken"/> used by this call.</param>
        /// <returns>
        /// The future completion result if it's not cancelled.
        /// When the underlying OpenXR calls succeed,
        /// use <see cref="Result.value"/> to retrieve the completion value.
        /// Otherwise, check <see cref="Result.status"/> for the failure reason.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Async operation has been cancelled by the Unity main thread.
        /// </exception>
        public async Awaitable<Result<T>> TryGetResultAsync(CancellationToken token = default)
        {
            if (_result.HasValue)
            {
                return _result.Value;
            }

            var linkedToken = token.CanBeCanceled ?
                CancellationTokenSource.CreateLinkedTokenSource(token, _cancelToken.Token) :
                _cancelToken;

            while (true)
            {
                if (_result.HasValue)
                {
                    return _result.Value;
                }

                if (_isCancelled)
                {
                    throw new OperationCanceledException();
                }

                await Awaitable.NextFrameAsync(linkedToken.Token);
            }
        }

        public void Dispose()
        {
            CancelFuture();
            CancelToken();
        }

        protected abstract Result<T> CompleteFuture(XrFutureEXT xrFuture);

        protected void StartPolling(XrFutureEXT xrFuture)
        {
            _xrfuture = xrFuture;
            PollingFuture();
        }

        private async void PollingFuture()
        {
            try
            {
                var pollInfo = new XrFuturePollInfoEXT(_xrfuture);
                var polling = true;
                while (polling)
                {
                    var result = OpenXRNativeApi.xrPollFutureEXT(pollInfo, out var pollResult);
                    if (result.IsError())
                    {
                        Debug.LogError($"xrPollFutureEXT failed with {result}.");
                        _result = new Result<T>(result.ToXRResultStatus(), new T());
                        _xrfuture = 0;
                        return;
                    }

                    switch (pollResult.state)
                    {
                        case XrFutureStateEXT.Pending:
                            await Awaitable.NextFrameAsync(_cancelToken.Token);
                            break;
                        case XrFutureStateEXT.Ready:
                            polling = false;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(XrFutureStateEXT));
                    }
                }

                _result = CompleteFuture(_xrfuture);
                _xrfuture = 0;
            }
            catch (OperationCanceledException)
            {
                _isCancelled = true;
                _xrfuture = 0;
            }
            catch (Exception e)
            {
                _result = new Result<T>(new XRResultStatus(StatusCode.UnknownError), new T());
                _xrfuture = 0;
                Debug.LogException(e);
            }
        }

        private void CancelFuture()
        {
            if (_xrfuture == 0)
            {
                return;
            }

            var cancelInfo = new XrFutureCancelInfoEXT(_xrfuture);
            var result = OpenXRNativeApi.xrCancelFutureEXT(cancelInfo);
            if (result.IsError())
            {
                Debug.LogError($"xrCancelFutureEXT failed with {result}.");
            }

            _xrfuture = 0;
        }

        private void CancelToken()
        {
            if (_cancelToken != null && !_cancelToken.IsCancellationRequested)
            {
                _cancelToken.Cancel();
                _cancelToken.Dispose();
                _cancelToken = null;
            }
        }
    }
}
