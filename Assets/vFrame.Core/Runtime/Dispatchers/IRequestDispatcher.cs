using System;
using vFrame.Core.Base;

namespace vFrame.Core.Dispatchers
{
    public interface IRequestDispatcher
    {
        TResponse Request<TRequest, TResponse>(in TRequest payload)
            where TRequest : IRequest<TResponse>;

        bool TryRequest<TRequest, TResponse>(in TRequest payload, out TResponse response)
            where TRequest : IRequest<TResponse>;

        ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler)
            where TRequest : IRequest<TResponse>;

        ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler, BaseObject owner)
            where TRequest : IRequest<TResponse>;

        ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler, ILifetime lifetime)
            where TRequest : IRequest<TResponse>;

        void UnhandleRequest(ISubscription subscription);

        int GetRequestSubscriptionCount();
    }
}
