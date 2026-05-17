namespace Framework.Event;
public interface IEventReceiver<TEvent> where TEvent : IEvent
{
    // active evt
    void OnEvent(TEvent e);
}