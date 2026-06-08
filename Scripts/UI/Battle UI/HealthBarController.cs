
using System;
using Framework.Event;

public class HealthBarController : MonoBehaviour,
    IEventReceiver<BattleStartedEvent>
{
    
    [SerializeField] private HealthBar healthBarPrefab;
    
    /* ---------------------------------------------------------------------------------- */

    private void OnEnable()
    {
        EventBus.Subscribe<BattleStartedEvent>(this);
        RebuildHealthBars();
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BattleStartedEvent>(this);
    }

    /* ---------------------------------------------------------------------------------- */

    private void RebuildHealthBars()
    {
        // 1.清理旧血条（防止多次进入战斗残留）
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        // 2.查找场景中所有的BattleUnit。
        // 这里我们只需要把已经绑定BattleEntity的友军单位转成血条即可。
        var allUnits = FindObjectsOfType<BattleUnit>();
        for (int i = 0; i < allUnits.Length; i++)
        {
            var unit = allUnits[i];
            if (unit.Entity == null || !unit.Entity.IsPlayer) continue;
            
            HealthBar bar = Instantiate(healthBarPrefab, transform);
            bar.Setup(unit.Entity);
        }
    }
    
    public void OnEvent(BattleStartedEvent e)
    {
        RebuildHealthBars(); // 每次战斗开始都重建血条，确保正确显示当前战斗单位的血量
    }
}
