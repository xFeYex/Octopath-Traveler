
public class BattleEntity
{
    public string ID {get;}
    public BattleUnit Unit {get;}
    public bool IsPlayer {get;}
    public CharacterRuntimeData RuntimeData { get;}
    
    public CharacterDefinitionSO Definition => RuntimeData.Definition;
    public bool IsAlive => RuntimeData.CurrentHP > 0;
    public int CurrentHP => RuntimeData.CurrentHP;
    public int CurrentSP => RuntimeData.CurrentSP;
    public int CurrentBP => RuntimeData.CurrentBP;
    public StatBlock TotalStats => RuntimeData.GetTotalStats();

    public BattleEntity(CharacterRuntimeData runtimeData, BattleUnit unit, bool isPlayer, string id)
    {
        RuntimeData = runtimeData;
        ID = id;
        Unit = unit;
        IsPlayer = isPlayer;
    }

    public int GetCurrentSpeed()
    {
        // 目前只取基础属性，未来在这里加上GetBuff（StatType.Speed）的加成
        return TotalStats.Speed;
    }
}