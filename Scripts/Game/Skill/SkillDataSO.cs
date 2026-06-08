using Utils;

[CreateAssetMenu(menuName = "Battle/Skill")]
public class SkillDataSO : ScriptableObject
{
    [Header("Special Logic Strategy")]
    public SkillLogicSO specialLogic;
    
    [Header("Identify")] 
    public string skillID;
    
    public string skillName;
    
    [TextArea]
    public string description;
    public Sprite icon;
    
    [Header("Cost")]
    [Min(0)] public int spCost;
    
    [Header("Targeting")]
    public TargetType targetType = TargetType.SingleEnemy;
    
    [Header("Type")]
    public SkillType skillType = SkillType.Damage;
    public DamageKind  damageKind = DamageKind.Physical;
    public ElementType elementType = ElementType.None;
    public WeaponType weaponType = WeaponType.None;

    [Header("Effect (Prototype)")] 
    [Min(0)] public int basePower;
    [Min(1)] public int hitCount;
    [Min(0)] public int healAmount;
}