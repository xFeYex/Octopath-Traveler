
/// <summary>
/// 战斗开场状态。
/// 
/// 这个状态主要负责：
/// 1）读取BattleStartPayload，在战斗场景里生成敌我双方单位；
/// 2）把RuntimeData、BattleUnit、BattleEntity 三层正式绑起来；
/// 3）播放最小开场入场表现；
/// 4）初始化轮时间轴，然后切到“选择下位动者”状态。
/// 
/// 可以把它理解成：
/// “战斗开始后的总装配状态”。
/// </summary>
public class BattleSetupState : BattleState
{
    private readonly BattleStartPayload _startPayload;
    
    public BattleSetupState(BattleContoller controller, BattleStartPayload startPayload) : base(controller)
    {
        _startPayload = startPayload;
    }

    public override IEnumerator Execute()
    {
        // 1.先按payload在战斗场景里生成敌我双方的BattleUnit。
        _controller.FieldManager.SpawnAll(_startPayload);
        
        // 2.再把 RuntimeData、BattleUnit 和 BattleEntity 正式绑起来。
        _controller.AllEntities.Clear();
        var allyEntities = CreateEntities(_startPayload.Allies, _controller.FieldManager.SpawnedAllyUnits, true);
        _controller.AllEntities.AddRange(allyEntities);
        _controller.AllEntities.AddRange(CreateEntities(_startPayload.Enemies, _controller.FieldManager.SpawnEnemyUnits, false));

        // 3.友军先从入场点跑到各自站位，敌军默认直接站好。
        float runtime = 2f; // todo: 以后统一管理
        if (runtime > 0f)
        {
            yield return new WaitForSeconds(runtime);

            foreach (var entity in allyEntities)
            {
                Vector3 homePos = _controller.FieldManager.GetHomePos(entity.Unit);
                _controller.StartCoroutine(entity.Unit.MoveToPosition(homePos, runtime));
            }
            yield return new WaitForSeconds(runtime);
        }
        
        // 4.战斗正式开始，通知HUD建显示，并初始化轮时间轴。
        EventBus.Publish(new BattleStartedEvent()); // HUD会在收到这个事件后建UI，建完UI会再发个事件告诉Controller它准备好了，可以继续了。
        _controller.StartNewRound();                // CTB排顺序，准备好第一轮的行动者了。
        _controller.UpdateTimelinePrediction();     // 更新时间轴预测
        
        yield return new WaitForSeconds(1f);        // 等HUD的入场表现播完
        
        _controller.SetState(new SelectNextEntityState(_controller));
        yield break;
    }

    private List<BattleEntity> CreateEntities(List<CharacterRuntimeData> runtimeList, IReadOnlyList<BattleUnit> units,
        bool isPlayer)
    {
        var entities = new List<BattleEntity>(runtimeList.Count);
        string side = isPlayer ? "P" : "E";

        for (var i = 0; i < runtimeList.Count; i++)
        {
            var runtimeData = runtimeList[i];
            BattleUnit unit = units[i];
            
            var entity = new BattleEntity(runtimeData, unit, isPlayer, $"{side}_{i:D2}_{runtimeData.Definition.ID}");
            unit.Bind(entity);
            entities.Add(entity);
        }
        
        return entities;
    } 
}