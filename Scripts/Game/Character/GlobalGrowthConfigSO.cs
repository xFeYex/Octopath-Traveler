
using Utils;

[CreateAssetMenu(menuName = "Game/GlobalGrowthConfig")]
public class GlobalGrowthConfigSO : ScriptableObject
{
    [Header("成长曲线(x: Level 1-99, Y: 幅度)")] 
    public AnimationCurve RankS = AnimationCurve.Linear(1, 1, 99, 4f);
    public AnimationCurve RankA = AnimationCurve.Linear(1, 1, 99, 3.5f);
    public AnimationCurve RankB = AnimationCurve.Linear(1, 1, 99, 3f);
    public AnimationCurve RankC = AnimationCurve.Linear(1, 1, 99, 2.5f);
    public AnimationCurve RankD = AnimationCurve.Linear(1, 1, 99, 2f);

    public AnimationCurve GetCurveByRank(GrowthRank rank)
    {
        return rank switch
        {
            GrowthRank.S => RankS,
            GrowthRank.A => RankA,
            GrowthRank.B => RankB,
            GrowthRank.C => RankC,
            GrowthRank.D => RankD,
            _ => RankB
        };
    }
}