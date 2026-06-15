using Utils;

/// <summary>
/// 目标选择状态。
/// 
/// 这个状态主要负责：Unity脚本
/// 1）根据当前命令收集可选目标；
/// 2）处理单体目标时的左右切换；
/// 3）负责群体目标时的整体高亮显示；Unity消息
/// 4） 确认后把真正选中的目标写回 BattleCommandRequest；
/// 5）取消则回到玩家输入状态。
/// 
/// 可以把它理解成：
/// “命令已经确定，正在决定打谁 / 奶谁的状态”。
/// </summary>
public class TargetSelectionState : BattleState
{
    private List<BattleEntity> _targets;
    private int _currentIndex;
    private TargetType _targetType = TargetType.SingleEnemy;
    
    private bool _ignoreConfirmThisFrame;
    private float _navigateCooldown; // 左右切换冷却时间
    private const float InputCooldownTime = 0.15f;
    
    public TargetSelectionState(BattleController controller) : base(controller)
    {
    }

    public override IEnumerator Enter()
    {
        // 1. 技能命令直接读SkillDataS0，道具命令按当前项目固定为单体友军。
        _targetType = _controller.CurrentCommand.Skill != null
            ? _controller.CurrentCommand.Skill.targetType
            : TargetType.SingleAlly;
        
        // 2. 再按这个目标类型收集当前所有可选目标。
        _targets = BattleTargeting.GetAliveTargetsByType(_controller.CurrentEntity, 
            _targetType,
            _controller.AllEntities);
        
        // 3. 重置这一轮目标选择的运行时状态。
        _currentIndex = 0;
        _navigateCooldown = 0;
        _ignoreConfirmThisFrame = true;
        
        // 4. 如果当前没有可选目标，就直接跳到执行阶段，让执行层自己做最终兜底。
        if (_targets.Count == 0)
        {
            _controller.SetState(new PerformActionState(_controller));
            yield break;
        }
        
        // 5. 群体目标不需要逐个切换，进来时直接整体高亮即可。
        if (_targetType == TargetType.AllEnemies || _targetType == TargetType.AllAllies)
        {
            _controller.SetSelectedTargets(_targets);
            yield break;
        }
        
        // 6. 单体目标则默认先选第一个。
        SelectTarget(_currentIndex);
        yield break;
    }

    public override IEnumerator Execute()
    {
        while (true)
        {
            // 1. 刚进状态的这一帧先忽略确认键，避免沿用上一层的提交输入
            if (_ignoreConfirmThisFrame)
            {
                _ignoreConfirmThisFrame = false;
                yield return null;
                continue;
            }
            
            // 2. 单体目标时才需要处理左右切换。
            HandleInput();
            
            // 3. 提交则尝试确认目标，并结束当前状态。
            if (InputSystemController.Instance.GetUISubmitPressed())
            {
                ConfirmSelection();
                yield break;
            }
                

            // 4. 取消则返回玩家输入状态。
            if (InputSystemController.Instance.GetUICancelPressed())
            {
                _controller.SetState(new PlayerInputState(_controller));
                yield break;
            }
            
            yield return null;
        }
    }

    public override IEnumerator Exit()
    {
        _controller.ClearTargetSelection();
        yield break;
    }

    private void HandleInput()
    {
        Vector2 navigate = InputSystemController.Instance.GetNavigateInput();
        if (Mathf.Abs(navigate.x) <= 0.5f)
            return;
        
        int step = navigate.x > 0 ? 1 : -1; // 向右是1，向左是-1
        int nextIndex =  (_currentIndex + step + _targets.Count) % _targets.Count;
        SelectTarget(nextIndex);
    }
    
    private void SelectTarget(int index)
    {
        // 避免方向键连续跳太快
        if (_navigateCooldown > 0)
        {
            _navigateCooldown -= Time.deltaTime;
            return;
        }
        
        if (index < 0 || index >= _targets.Count) return;
        
        _currentIndex = index;
        _controller.SetSelectedTarget(_targets[_currentIndex]);
        _navigateCooldown = InputCooldownTime;
    }

    #region 目标写回

    private void ConfirmSelection()
    {
        if (_targetType == TargetType.AllEnemies || _targetType == TargetType.AllAllies)
        {
            _controller.CurrentCommand.TargetEntityID = null;
        }
        else
        {
            _controller.CurrentCommand.TargetEntityID = _targets[_currentIndex].ID;
        }
        
        _controller.SetState(new PerformActionState(_controller));
    }

    #endregion
}