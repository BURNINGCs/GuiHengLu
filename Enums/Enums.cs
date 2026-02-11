public enum Orientation //方向
{
    north,
    east,
    south,
    west,
    none
}
public enum AimDirection
{
    Right,
    Left,
}
public enum ElementType
{
    None,       // 无元素
    Fire,       // 火
    Water,      // 水
    Wood,       // 木
    Metal,      // 金
}
public enum ChestSpawnEvent
{
    onRoomEntry,
    onEnemiesDefeated
}

public enum ChestSpawnPosition
{
    atSpawnerPosition,
    atPlayerPosition
}
public enum ChestState
{
    closed,
    healthItem,
    ammoItem,
    weaponItem,
    empty
}

public enum GameState //游戏状态
{
    gameStarted,
    playingLevel,
    engagingEnemies,
    bossStage,
    engagingBoss,   //与boss交战状态
    levelCompleted,
    gameWon,
    gameLost,
    gamePaused,
    dungeonOverviewMap,     //地牢概览地图
    restartGame
}
