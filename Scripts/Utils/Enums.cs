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

public enum PanelType
{
    Item,
    Sell,
    Buy,
    Equipment
}

public enum EquipmentCategory
{
    Weapon,
    Shield,         // 盾
    Head,
    Body,
    Accessory,      // 配件
}

public enum EquipSlot
{
    Sword,
    Spear,          // 矛
    Dagger,         // 短剑
    Axe,            // 斧
    Bow,            // 弓
    Staff,          // 法杖
    Shield,         // 盾
    Head,
    Body,
    Accessory1,
    Accessory2,
}

public enum WeaponType
{
    None = 0,
    Sword,
    Spear,
    Dagger,
    Axe,
    Bow,
    Staff
}

public enum StatType
{
    MaxHp = 0,
    MaxSp = 1,
    
    PAtk = 2,
    PDef = 3,

    MAtk = 4,
    MDef = 5,
    
    Speed = 6,
    Accuracy = 7, // 命中
    Evasion = 8,  // 闪避
    
    // 当前的数值
    CurrentHP = 100,
    CurrentSP = 101,
    CureentBP = 102
}
