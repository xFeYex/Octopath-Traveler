

using System;
using Framework.Event;
using Utils;

public class BattleContoller : MonoBehaviour,
    IEventReceiver<GameModeChangedEvent>
{
    [Header("场景管理")]
    [SerializeField] private BattleFieldManager fieldManager;
    public BattleFieldManager FieldManager => fieldManager;
    
    [Header("UI 引用")]
    [SerializeField] private BattleCommandUI commandUI;
    [SerializeField] private BattleTimelineUI timelineUI;
    public BattleTimelineUI TimelineUI => timelineUI;
    
    private readonly List<BattleEntity> _allEntities = new();
    public List<BattleEntity> AllEntities => _allEntities;
    
    private BattleState _currentState;
    private bool _battleRunning;
    public bool IsBattleRunning => _battleRunning;
    private Coroutine _battleLoopRoutine;

    private BattleRoundScheduler _roundScheduler = new();
    
    // 全局共享游标
    public BattleEntity CurrentEntity { get; set; }
    public BattleCommandRequest CurrentCommand { get; set; }
    
    /* -------------------------------------------------------------------------------------- */

    private void OnEnable()
    {
        EventBus.Subscribe(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(this);
    }

    /* -------------------------------------------------------------------------------------- */

    #region 回合调度桥接入口

    /// <summary>
    /// 开战完成后，正式初始化第轮 CTB 顺序
    /// BattleController 不排顺序，只负责把参战列表交给调度器。
    /// </summary>
    public void StartNewRound()
    {
        _roundScheduler.Initialize(_allEntities);
    }
    
    /// <summary>
    /// 向正式调度器请求“下一位真正要行动的人”。
    /// </summary>
    public BattleEntity GetNextActorByRound() => _roundScheduler.GetNextActor(_allEntities);

    #endregion
    
    public void SetState (BattleState nextState) => _currentState = nextState;

    public void StartBattleIfReady()
    {
        if (_battleRunning) return;
        
        if (GameModeManager.Instance.CurrentGameMode != GameMode.Battle) return;

        if (!BattleService.Instance.HasPendingPayload) return;

        var payload = BattleService.Instance.ConsumeStartPayLoad();
        // 进入战斗第一个阶段，准备阶段
        SetState(new BattleSetupState(this, payload));

        _battleRunning = true;
        _battleLoopRoutine = StartCoroutine(BattleLoopRoutine());
    }

    ///<summary>
    /// 标准状态机
    /// (Enter 资源准备 -> Execute 为触发-> Exit 资源回收)
    ///</summary>
    private IEnumerator BattleLoopRoutine()
    {
        while (_battleRunning && _currentState != null)
        {
            // 先记住这一轮开始时的状态，避免Enter里切状态后还继续跑错流程。
            var stateSnapshot =  _currentState;
            
            yield return StartCoroutine(stateSnapshot.Enter());
            
            // 如果Enter期间已经切到别的状态了，这一轮就不要继续往下执行
            if (stateSnapshot != _currentState)
            {
                yield return StartCoroutine(stateSnapshot.Exit());
                continue;
            }
            
            // 当前状态真正执行业务。
            yield return StartCoroutine(stateSnapshot.Execute());
            
            // 当前状态自己的收尾清理。
            yield return StartCoroutine(stateSnapshot.Exit());
        }
        _battleLoopRoutine = null;
    }

    public void StopBattle()
    {
        _battleRunning = false;
        _currentState = null;

        if (_battleLoopRoutine != null)
        {
            StopCoroutine(_battleLoopRoutine);
            _battleLoopRoutine = null;
        }
    }

    #region 事件监听

    public void OnEvent(GameModeChangedEvent e)
    {
        if (e.newMode == GameMode.Battle)
        {
            StartBattleIfReady();
            return;
        }
        
        // 停止战斗
        if (_battleRunning)
            StopBattle();
    }

    #endregion

    #region 目标选择高亮桥接

    /// <summary>
    /// 单体自标高亮：只让当前这个自标显示选中光标
    /// </summary>
    public void SetSelectedTarget(BattleEntity target)
    {
        foreach (var entity in _allEntities)
        {
            entity.Unit.SetTargetSelection(entity == target);
        }
    }

    /// <summary>
    /// 群体目标高亮：让候选集合里的所有目标一起显示选中光标。
    /// </summary>
    public void SetSelectedTargets(List<BattleEntity> targets)
    {
        foreach (var entity in _allEntities)
        {
            entity.Unit.SetTargetSelection(targets.Contains(entity));
        }
    }

    /// <summary>
    /// 退出目标选择时，统一关闭全部目标亮。
    /// </summary>
    public void ClearTargetSelection()
    {
        foreach (var entity in _allEntities)
        {
            entity.Unit.SetTargetSelection(false);
        }
    }

    #endregion

    #region 时间轴预测与破盾队列同步

    public void UpdateTimelinePrediction()
    {
        timelineUI.UpdateTimeline(_roundScheduler.BuildTimeLinePrediction());
    }

    #endregion
}
