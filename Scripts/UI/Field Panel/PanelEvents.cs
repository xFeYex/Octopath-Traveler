using Framework;
using Framework.Event;

public class PanelRequestEvent : IEvent
{
    public readonly ActionBase actionBase;

    public PanelRequestEvent(ActionBase actionBase)
    {
        this.actionBase = actionBase;
    }
}