
using Utils;

public class BattleStartPayload
{
    public List<CharacterRuntimeData> Allies { get; }
    public List<CharacterRuntimeData> Enemies { get; }
    public EnemyLayoutFormation Formation { get; }

    public BattleStartPayload(List<CharacterRuntimeData> allies, List<CharacterRuntimeData> enemies,
        EnemyLayoutFormation formation)
    {
        Allies = allies;
        Enemies = enemies;
        Formation = formation;
    }
}