namespace vFrame.Core.EventDispatchers
{
    public interface IInteractionMessage
    {
        object GetContext();
        object GetTarget();
    }
}
