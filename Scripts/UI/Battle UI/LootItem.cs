
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 结算面板里的单个掉落条目。
/// 负责把掉落物品名称、数量和图标绑定到UI上。
/// </summary>
public class LootItem : MonoBehaviour
{
    #region 掉落条目组件引用

    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemAmount;

    #endregion

    /* ------------------------------------------------------------------------------ */
    
    #region 掉落数据绑定

    public void SetLootItem(InventoryItem inventoryItem)
    {
        // 1.先显示物品名称。
        itemName.text = inventoryItem.ItemDefinition.ItemName;
        // 2.再显示数量
        itemAmount.text = $"x{inventoryItem.Quantity}";
        // 3.最后从图标表里取出对应的Sprite。
        itemIcon.sprite = InventoryManager.Instance.IconSet.GetIcon(inventoryItem.ItemDefinition.itemIconKey);
    }

    #endregion
}