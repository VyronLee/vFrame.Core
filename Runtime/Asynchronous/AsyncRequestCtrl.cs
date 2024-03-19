// ------------------------------------------------------------
//         File: AsyncRequestCtrl.cs
//        Brief: AsyncRequestCtrl.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-19 20:42
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;
using vFrame.Core.Base;

namespace vFrame.Core.Unity.Asynchronous
{
    public class AsyncRequestCtrl : CreateAbility<AsyncRequestCtrl>, IAsyncRequestCtrl
    {
        private List<IAsyncRequest> _requests;

        protected override void OnCreate() {
            _requests = new List<IAsyncRequest>(64);
        }

        protected override void OnDestroy() {
            _requests?.Clear();
            _requests = null;
        }

        public void Update() {
            for (var i = _requests.Count - 1; i >= 0; i--) {
                var request = _requests[i];
                switch (request.State) {
                    case AsyncState.NotStarted:
                        request.Start();
                        break;
                    case AsyncState.Processing:
                        request.Update();
                        break;
                    case AsyncState.Finished:
                        _requests.RemoveAt(i);
                        OnRequestError?.Invoke(request);
                        break;
                    case AsyncState.Error:
                        _requests.RemoveAt(i);
                        OnRequestFinish?.Invoke(request);
                        break;
                }
            }
        }

        public T CreateRequest<T>() where T : IAsyncRequest {
            return (T) CreateRequest(typeof(T));
        }

        public IAsyncRequest CreateRequest(Type type) {
            if (!(Activator.CreateInstance(type) is AsyncRequest request)) {
                throw new AsyncRequestTypeErrorException();
            }
            request.Create();
            AddRequest(request);
            return request;
        }

        public void AddRequest(IAsyncRequest request) {
            _requests.Add(request);
        }

        public event Action<IAsyncRequest> OnRequestFinish;
        public event Action<IAsyncRequest> OnRequestError;
    }
}