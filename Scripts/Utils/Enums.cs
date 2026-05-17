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