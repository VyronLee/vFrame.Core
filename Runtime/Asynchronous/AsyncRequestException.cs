using System;

namespace vFrame.Core.Unity.Asynchronous
{
    public class AsyncRequestException : Exception { }

    public class AsyncRequestAlreadySetupException : AsyncRequestException { }
}