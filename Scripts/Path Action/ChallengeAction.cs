using System;
using UnityEngine.AddressableAssets;
using Utils;

public class ChallengeAction : ActionBase
{
    [Header("Challenge Action")]
    public AssetReference battleSceneReference;
    public List<CharacterDefinitionSO> npcTeamMembers;

    [Header("阵型")] 
    public EnemyLayoutFormation enemyLayoutFormation = EnemyLayoutFormation.Line;
    public CharacterDefinitionSO CurrentCharacter;
    
    public int lastDifficulty { get; private set; }
    
    /* ----------------------------------------------------------------------------------------- */

    private void Awake()
    {
        CurrentCharacter = GetComponent<CharacterIdentity>().characterDefinition;
        npcTeamMembers ??= new();
        if (!npcTeamMembers.Contains(CurrentCharacter))
            npcTeamMembers.Insert(0, CurrentCharacter);
    }

    public override void TriggerAction(AllyDefinitionSO interaction)
    {
        lastDifficulty = EvaluateDifficulty(EvaluatePlayerTeamPower(), EvaluateEnemyTeamPower());
        EventBus.Publish(new PanelRequestEvent(this));
    }

    public override void Execute()
    {
        // 挑战确认后，直接交给BattleService 组装开始数据并切场景
        BattleService.Instance.StartBattleFromAction(this);
    }

    #region 计算战斗力

    private int EvaluatePlayerTeamPower()
    {
        var partyMember = PartyManager.Instance.PartyMembers;
        var totalPower = 0;
        foreach (var member in partyMember)
        {
            totalPower += CharacterRuntimeData.EvaluatePowerFromStats(member.GetTotalStats());
        }
        return totalPower;
    }

    private int EvaluateEnemyTeamPower()
    {
        var totalPower = 0;
        foreach (var member in npcTeamMembers)
        {
            totalPower += CharacterRuntimeData.EvaluatePowerFromStats(member.BaseStats);
        }
        return totalPower;
    }

    private int EvaluateDifficulty(int playerPower, int enemyPower)
    {
        float ratio = enemyPower / (float)playerPower;

        if (ratio < 0.55f) return 1;    // 碾压
        if (ratio < 0.65f) return 2;
        if (ratio < 0.75f) return 3;
        if (ratio < 0.85f) return 4;
        if (ratio < 0.95f) return 5;    // 势均力敌
        if (ratio < 1.05f) return 6;
        if (ratio < 1.2f) return 7;
        if (ratio < 1.4f) return 8;
        if (ratio < 1.95f) return 9;
        return 10;                      // 危险
    }

    #endregion
}
