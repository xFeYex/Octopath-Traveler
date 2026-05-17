using System;
using Framework.Event;
    
public static class EventBus
{
    private static readonly Dictionary<Type, List<object>> EventDict = new();

    public static void Subscribe<TEvent>(IEventReceiver<TEvent> receiver) where TEvent : IEvent
    {
        Type eventType = typeof(TEvent);
        // 查字典有没有这个事件
        if (!EventDict.TryGetValue(eventType, out var receivers))
        {
            receivers = new List<object>();
            EventDict[eventType] = receivers;
        }
        
        // 如果字典没有该对象，则添加
        if (!receivers.Contains(receiver))
        {
            receivers.Add(receiver);
        }
    }

    public static void Unsubscribe<TEvent>(IEventReceiver<TEvent> receiver) where TEvent : IEvent
    {
        Type eventType = typeof(TEvent);
        
        // 字典存在，则删除
        if (EventDict.TryGetValue(eventType, out var receivers))
        {
            receivers.Remove(receiver);

            if (receivers.Count == 0)
                EventDict.Remove(eventType);
        }
    }

    public static void Publish<TEvent>(TEvent e) where TEvent : IEvent
    {
        Type eventType = typeof(TEvent);

        if (EventDict.TryGetValue(eventType, out var receivers))
        {
            for (int i = 0; i < receivers.Count; i++)
            {
                var obj = receivers[i];
                if (obj is UnityEngine.Object unityObject && unityObject == null)
                {
                    receivers.RemoveAt(i);
                    continue;
                }
                ((IEventReceiver<TEvent>)receivers[i]).OnEvent(e);
            }
        }
    }
} 