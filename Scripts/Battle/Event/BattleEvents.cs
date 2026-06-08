using Framework.Event;

/// <summary>
/// 战初始化完成，UI可以开始显示，音乐开始播放
///</summary>
public readonly struct BattleStartedEvent : IEvent
{
    // （可以携带战斗类型（BoSS战/普通战）用于切换BGM
}

public readonly struct ActiveEntityChangedEvent : IEvent
{
    public readonly BattleEntity Entity;

    public ActiveEntityChangedEvent(BattleEntity entity)
    {
        Entity = entity;
    }
}