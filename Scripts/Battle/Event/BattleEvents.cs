using Framework.Event;
using Utils;

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

public readonly struct EntityStatChangedEvent : IEvent
{
    public readonly BattleEntity Entity;
    public readonly StatType StatType;
    public readonly int NewValue;
    public readonly int MaxValue;

    public EntityStatChangedEvent(BattleEntity entity, StatType statType, int newValue, int maxValue)
    {
        Entity = entity;
        StatType = statType;
        NewValue = newValue;
        MaxValue = maxValue;
    }
}

public readonly struct SkillNameDisplayEvent : IEvent
{
    public readonly BattleEntity Actor;
    public readonly string SkillName;

    public SkillNameDisplayEvent(BattleEntity actor, string skillName)
    {
        Actor = actor;
        SkillName = skillName;
    }
}

public readonly struct BattleNotificationEvent : IEvent
{
    public readonly string Message;
    public readonly bool IsSuccess;

    public BattleNotificationEvent(string message, bool isSuccess = false)
    {
        Message = message;
        IsSuccess = isSuccess;
    }
}

public readonly struct EntityShieldChangedEvent : IEvent
{
    public readonly BattleEntity Target;
    public readonly int NewShield;

    public EntityShieldChangedEvent(BattleEntity target, int newShield)
    {
        Target = target;
        NewShield = newShield;
    }
}

public readonly struct EntityWeaknessChangedEvent : IEvent
{
    public readonly BattleEntity Target;

    public EntityWeaknessChangedEvent(BattleEntity target)
    {
        Target = target;
    }
}

// 恢复事件：把“谁刚刚从Break里恢复”同步给外部系统。
public readonly struct EntityRecoverFromBreakEvent : IEvent
{
    public readonly BattleEntity Target;
    
    public EntityRecoverFromBreakEvent(BattleEntity target)
    {
        Target = target;
    }
}

#region

/// <summary>
/// 请求播放击杀演出（慢动作/镜头/溶解）
/// </summary>
public readonly struct KillCinematicRequestedEvent : IEvent
{
    public readonly BattleEntity Target;
    
    public KillCinematicRequestedEvent(BattleEntity target)
    {
        Target = target;
    }
}

/// <summary>
/// 请求播放破盾演出(慢动作/镜头/BreakVoLume
/// </summary>
public readonly struct BreakCinematicRequestedEvent : IEvent
{

}

#endregion

#region 结算视角进入事件

public readonly struct BattleResultViewEnterEvent : IEvent
{
    public readonly int ExpReward;
    public readonly int MoneyReward;
    public readonly List<BattleDropReward> DropRewards;

    public BattleResultViewEnterEvent(int expReward, int moneyReward, List<BattleDropReward> dropRewards)
    {
        ExpReward = expReward;
        MoneyReward = moneyReward;
        DropRewards = dropRewards;
    }
}
          
public readonly struct BattleEndedEvent : IEvent
{
    public readonly bool IsWin;
    public readonly int ExpReward;
    public readonly int MoneyReward;
    public readonly List<BattleDropReward> DropRewards;

    public BattleEndedEvent(bool isWin, int expReward = 0, int moneyReward = 0, List<BattleDropReward> dropRewards = null)
    {
        IsWin = isWin;
        ExpReward = expReward;
        MoneyReward = moneyReward;
        DropRewards = dropRewards ?? new();
    }
}
public readonly struct BattleDropReward
{
  public readonly ItemDefinitionSO ItemDefinition;
  public readonly int Quantity;

  public BattleDropReward(ItemDefinitionSO itemDefinition, int quantity)
  {
      ItemDefinition = itemDefinition;
      Quantity = quantity;
  }
}

public readonly struct BattleLoseViewEvent : IEvent {}

/// <summary>
/// 玩家在结算面板点击"确认"
/// </summary>
public readonly struct BattleResultConfirmedEvent : IEvent {}
#endregion