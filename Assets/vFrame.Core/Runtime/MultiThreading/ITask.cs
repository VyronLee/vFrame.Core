namespace vFrame.Core.MultiThreading
{
    public interface ITask : IAsync { }

    public interface ITask<out TRet> : ITask
    {
        TRet Value { get; }
    }
}