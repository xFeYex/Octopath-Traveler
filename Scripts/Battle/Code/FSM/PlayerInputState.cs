
using Utils;

public class PlayerInputState : BattleState
{
    private bool _inputReceived;    // 标记玩家是否已经完成输入，等待输入时这个值为false，收到输入后设为true，进入下一状态
    private int _pendingBoostSpend;
    
    /* ---------------------------------------------------------------------------------- */
    
    public PlayerInputState(BattleContoller controller) : base(controller) { }

    public override IEnumerator Enter()
    {
        // 1.先让当前行动者跑到行动点。
        yield return MoveCurrentEntityToActionPosition();
        
        // 2.重置这一回合的输入暂存状态。
        _inputReceived = false;
        _pendingBoostSpend = 0;
        _controller.CurrentCommand = null;
        
        // todo : 3.把场景里的 Boost 表现先归零
        
        // 4. 最后正式打开命令UI，等待玩家输入。
        BattleCommandUI.Instance.RequestInput(_controller.CurrentEntity, OnCommandSelected, OnSkillSelected, null);
        yield break;
    }

    public override IEnumerator Execute()
    {
        // 等待玩家输入
        while (!_inputReceived)
        {
            // todo: 1. 在等待玩家输入期间，持续监听BPBoost的增减。
            yield return null;
        }
        
        // 2. 输入一旦完成，就按命令里的目标规则决定下一个状态。
        MoveToNextStateByTargetRule();
    }
    
    private IEnumerator MoveCurrentEntityToActionPosition()
    {
        var actionPos = _controller.FieldManager.GetActionPos();
        var distance = Vector3.Distance(_controller.CurrentEntity.Unit.transform.position, actionPos);
        
        // 如果距离较远，才让单位先跑到行动点，距离近的话就直接在原地选指令
        if (distance > 0.1f)
            yield return _controller.StartCoroutine(_controller.CurrentEntity.Unit.MoveToPosition(actionPos));
    }

    private void MoveToNextStateByTargetRule()
    {
        var command = _controller.CurrentCommand;
        
        _controller.SetState(command.Type switch
        {
            BattleCommandType.Item => new TargetSelectionState(_controller),
            BattleCommandType.Attack or BattleCommandType.Skill => new TargetSelectionState(_controller),
            _=> new PerformActionState(_controller)
        });
    }
    
    #region UI回调与命令构建

    private void OnCommandSelected(BattleCommandType type)
    {
        switch (type)
        {
            case BattleCommandType.Attack: //todo: bp 消耗
                ConfirmInput(BattleCommandRequest.CreateAttack(_controller.CurrentEntity));
                break;
            case BattleCommandType.Defend:
                ConfirmInput(BattleCommandRequest.CreateDefend());
                break;
            case BattleCommandType.Escape:
                ConfirmInput(BattleCommandRequest.CreateEscape());
                break;
        }
    }
    
    /// <summary>
    /// 处理技能选择事件
    /// </summary>
    /// <param name="skill">玩家选择的技能数据</param>
    private void OnSkillSelected(SkillDataSO skill)
    {
        // 确认输入，创建技能战斗请求
        // 使用预设目标解析方法确定目标
        // 传入当前实体和技能的目标类型
        //todo: bp 消耗
        ConfirmInput(BattleCommandRequest.CreateSkill(skill));
    }

    private void ConfirmInput(BattleCommandRequest command)
    {
        _controller.CurrentCommand = command;
        _inputReceived = true;
    }

    #endregion
    
    
}