// ------------------------------------------------------------
//         File: IAsyncRequestCtrl.cs
//        Brief: IAsyncRequestCtrl.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-19 20:44
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using vFrame.Core.Base;

namespace vFrame.Core.Unity.Asynchronous
{
    public interface IAsyncRequestCtrl : IBaseObject
    {
        void Update();
        void SelfDrive();
        T CreateRequest<T>() where T : IAsyncRequest;
        IAsyncRequest CreateRequest(Type type);
        void AddRequest(IAsyncRequest request);
        event Action<IAsyncRequest> OnRequestFinish;
        event Action<IAsyncRequest> OnRequestError;
    }
}