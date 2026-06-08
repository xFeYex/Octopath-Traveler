
using UnityEngine.AddressableAssets;
using Utils;

public class BattleService : Singleton<BattleService>
{
    private AssetReference _returnSceneAfterBattle;
    
    // 战斗启动缓存
    private BattleStartPayload _pendingPayload;
    public bool HasPendingPayload => _pendingPayload != null;
    
    /* ---------------------------------------------------------------------------------- */

    public BattleStartPayload ConsumeStartPayLoad()
    {
        var payload = _pendingPayload;
        _pendingPayload = null;
        return payload;
    }
    
    
    public void StartBattleFromAction(ChallengeAction action)
    {
        List<CharacterRuntimeData> allies = new(PartyManager.Instance.PartyMembers);
        List<CharacterRuntimeData> enemies = new();
        foreach (var definition in action.npcTeamMembers)
        {
            if (definition is not null)
                enemies.Add(new CharacterRuntimeData(definition));
        }
        
        StartBattle(allies, enemies, action.battleSceneReference, action.enemyLayoutFormation);
    }
    
    public void StartBattle(List<CharacterRuntimeData> allies, List<CharacterRuntimeData> enemies, AssetReference battleScene,
        EnemyLayoutFormation formation)
    {
        SceneLoadManager sceneLoadManager = SceneLoadManager.Instance;
        _returnSceneAfterBattle = sceneLoadManager.activeScene; 
        
        // 开战前先把友军的战斗会话状态整理好：
        // 1）BP归零，保证每场战斗都从头开始积攒。
        // 2）如果角色当前已经倒地，就抬到 1HP，避免直接以死亡状态进战斗。
        NormalizeBattleSessionState(PartyManager.Instance.PartyMembers);
        
        // 启动载荷
        _pendingPayload = new(new List<CharacterRuntimeData>(allies),
            new List<CharacterRuntimeData>(enemies),
            formation);
        
        // 加载场景
        sceneLoadManager.RequestLoad(new SceneLoadRequest(
            battleScene,
            FadeStyle.PanelFade,
            GameMode.Battle
            ));
    }

    private void NormalizeBattleSessionState(List<CharacterRuntimeData> members)
    {
        foreach (var member in members)
        {
            member.ResetBattleBp();
            if (member.CurrentHP < 0)
                member.CurrentHP = 1;
        }
    }
}