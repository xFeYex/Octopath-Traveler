
using Utils;

[CreateAssetMenu(menuName = "Inventory/Equipment Item")]
public class EquipmentItemSO : ItemDefinitionSO
{
    [Header("Equipment Config")]
    public EquipmentCategory category = EquipmentCategory.Weapon;

    public WeaponType WeaponType = WeaponType.Sword;

    [Header("Stats Bonus")] 
    public StatBlock statBonus = StatBlock.zero;
    
    /* ---------------------------------------------------------------------- */

    private void OnValidate()
    {
        if (category != EquipmentCategory.Weapon)
            WeaponType = WeaponType.None;
    }
}