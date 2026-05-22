namespace Utils;

public enum GameMode
{
    Explore,
    Battle,
    InteractionMenu, // 交互菜单
    Pause
}

// 输入总线控制类型
public enum ActionMap
{
    Player,
    UI,
    Battle,
    None
}

public enum CameraView
{
    Explore,
    Battle,
    BattleResult
}

public enum PlayerJob
{
    Any,
    
    // base Job
    Warrior,    // 剑士
    Apothecary, // 药师
    Cleric,     // 神官
    Dancer,     // 舞者
    Hunter,     // 猎人
    Merchant,   // 商人
    Scholar,    // 学者
    Thief       // 盗贼
}

public enum GrowthRank{ S, A, B, C, D }

public enum ItemType
{
    Equipment,  // 可装备物品
    Consumable, // 可消耗物品
}

public enum ItemIconKey
{
    // 物品类型枚举
    // 定义游戏中各自物品的分类
    Weapon,         // 武器 
    Armor,          // 防具
    Accessory,      // 饰品
    Healing,        // 治疗
    SP,             // SP恢复
    Revive,         // 复活
    Cure,           // 解除异常
    KeyItem         // 任务道具
}