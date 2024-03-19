using System;

namespace vFrame.Core.Unity.Asynchronous
{
    public class AsyncRequestException : Exception { }

    public class AsyncRequestNotFinishedException : AsyncRequestException { }

    public class AsyncRequestTypeErrorException : AsyncRequestException { }
}