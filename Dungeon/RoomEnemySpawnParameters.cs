using UnityEngine;

[System.Serializable]
public class RoomEnemySpawnParameters
{
    #region Tooltip
    [Tooltip("定义该房间对应的地下城关卡等级，以及该等级下房间内应生成的敌人总数")]
    #endregion Tooltip
    public DungeonLevelSO dungeonLevel;
    #region Tooltip
    [Tooltip("该地下城等级下，房间内生成敌人的最小数量。实际生成数量为最小值与最大值之间的随机数")]
    #endregion Tooltip
    public int minTotalEnemiesToSpawn;
    #region Tooltip
    [Tooltip("该地下城等级下，房间内生成敌人的最大数量。实际生成数量为最小值与最大值之间的随机数")]
    #endregion Tooltip
    public int maxTotalEnemiesToSpawn;
    #region Tooltip
    [Tooltip("该地下城等级下，房间内同时生成敌人的最小数量。实际同时生成数量为最小值与最大值之间的随机数")]
    #endregion Tooltip
    public int minConcurrentEnemies;
    #region Tooltip
    [Tooltip("该地下城等级下，房间内同时生成敌人的最大数量。实际同时生成数量为最小值与最大值之间的随机数")]
    #endregion Tooltip
    public int maxConcurrentEnemies;
    #region Tooltip
    [Tooltip("该地下城等级下，该房间内敌人的最小生成间隔（以秒为单位）。实际生成间隔为最小值与最大值之间的随机数")]
    #endregion Tooltip
    public int minSpawnInterval;
    #region Tooltip
    [Tooltip("该地下城等级下，该房间内敌人的最大生成间隔（以秒为单位）。实际生成间隔为最小值与最大值之间的随机数")]
    #endregion Tooltip
    public int maxSpawnInterval;

}
