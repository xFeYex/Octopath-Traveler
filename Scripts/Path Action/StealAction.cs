
using System;
using Random = UnityEngine.Random;

public class StealAction : ActionBase
{
    [Header("Steal Action")]
    public List<InventoryItem> itemsToSteal;
    
    /* ----------------------------------------------------------------- */

    public override void TriggerAction(AllyDefinitionSO interaction)
    {
        EventBus.Publish(new PanelRequestEvent(this));
    }

    public bool TrySteal(ItemDefinitionSO itemDefinitionSO)
    {
        bool success = Random.value < Mathf.Clamp01(itemDefinitionSO.RarityWeight / 100f); // 成功率基于物品稀有度权重

        if (success)
        {
            InventoryManager.Instance.AddItem(itemDefinitionSO, 1); // 成功偷取，添加物品到玩家库存

            for (int i = 0; i < itemsToSteal.Count; i++)
            {
                if (itemsToSteal[i].ItemDefinition != itemDefinitionSO)
                    continue;
                itemsToSteal.RemoveAt(i); // 从可偷取列表中移除已偷取的物品
            }
        }
        
        return success;
    }
}