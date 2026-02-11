using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName ="Room_", menuName ="Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector] public string guid;

    #region Header ROOM PREFAB

    [Space(10)]
    [Header("房间预制体")]

    #endregion Header ROOM PREFAB

    #region Tooltip

    [Tooltip("房间的游戏对象预制体（该预制体将包含房间的所有瓦片地图以及环境游戏对象）")]

    #endregion Tooltip

    public GameObject prefab;

    [HideInInspector] public GameObject previousPrefab;

    #region Header ROOM MUSIC
    [Space(10)]
    [Header("房间音乐")]
    #endregion Header ROOM MUSIC

    #region Tooltip
    [Tooltip("房间尚未清除敌人时的战斗音乐SO")]
    #endregion Tooltip

    public MusicTrackSO battleMusic;

    #region Tooltip
    [Tooltip("房间已清除敌人时的环境音乐SO")]
    #endregion Tooltip

    public MusicTrackSO ambientMusic;

    #region Header ROOM CONFIGURATION

    [Space(10)]
    [Header("房间配置")]

    #endregion Header ROOM CONFIGURATION

    #region Tooltip

    [Tooltip("房间节点类型 SO（可脚本化对象）。房间节点类型与房间节点图中使用的房间节点相对应，其中走廊是个例外。" +
        "在房间节点图中，只有一种走廊类型--“走廊（Corridor）”；" +
        "而对于房间模板，存在两种走廊节点类型--南北向走廊（CorridorNS）和东西向走廊（CorridorEW）。")]

    #endregion Tooltip

    public RoomNodeTypeSO roomNodeType;

    #region Tooltip

    [Tooltip("如果你想象用一个矩形将房间的瓦片地图完全包围起来，那么房间的下界（lower bounds）就代表这个矩形的左下角。" +
        "这一位置需要根据房间的瓦片地图来确定 —— 使用坐标笔刷指针获取该左下角的瓦片地图网格位置" +
        "（注意：这是瓦片地图的本地位置，而非世界位置）")]

    #endregion Tooltip

    public Vector2Int lowerBounds;

    #region Tooltip

    [Tooltip("如果你想象用一个矩形将房间的瓦片地图完全包围起来，那么房间的上界（upper bounds）就代表这个矩形的右上角。" +
        "这一位置需要根据房间的瓦片地图来确定 —— 使用坐标笔刷指针获取该右上角的瓦片地图网格位置" +
        "（注意：这是瓦片地图的本地位置，而非世界位置）")]

    #endregion Tooltip

    public Vector2Int upperBounds;

    #region Tooltip

    [Tooltip("一个房间最多应有四个出入口，分别对应四个方位（东、南、西、北）。" +
        "这些出入口应统一为3格的开口大小，其中中间一格的位置即为该出入口坐标的“位置点”。")]

    #endregion Tooltip

    [SerializeField] public List<Doorway> doorwayList;

    #region Tooltip

    [Tooltip("房间中每个可能的生成位置（用于生成敌人和宝箱）的瓦片地图坐标，都应添加到这个数组中。")]

    #endregion Tooltip

    public Vector2Int[] spawnPositionArray;

    #region Header ENEMY DETAILS

    [Space(10)]
    [Header("敌人详情")]

    #endregion Header ENEMY DETAILS

    #region Tooltip

    [Tooltip("按地下城关卡等级，将所有可在该房间生成的敌人填充到列表中，包括该类型敌人将被生成的（随机）比例")]

    #endregion Tooltip

    public List<SpawnableObjectsByLevel<EnemyDetailsSO>> enemiesByLevelList;

    #region Tooltip

    [Tooltip("将敌人的生成参数填充到列表中")]

    #endregion Tooltip

    public List<RoomEnemySpawnParameters> roomEnemySpawnParametersList;

    //返回该房间模板的入口列表
    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region Validation

#if UNITY_EDITOR
    // Validate SO fields
    private void OnValidate()
    {
        //若 GUID 为空或预制体发生变更，则设置唯一 GUID
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }
        HelperUtilities.ValidateCheckNullValue(this, nameof(prefab), prefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(battleMusic), battleMusic);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ambientMusic), ambientMusic);
        HelperUtilities.ValidateCheckNullValue(this, nameof(roomNodeType), roomNodeType);

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);

        //检查敌人配置与房间生成参数是否匹配对应关卡
        if (enemiesByLevelList.Count > 0 || roomEnemySpawnParametersList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(enemiesByLevelList), enemiesByLevelList);
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomEnemySpawnParametersList), roomEnemySpawnParametersList);

            foreach (RoomEnemySpawnParameters roomEnemySpawnParameters in roomEnemySpawnParametersList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(roomEnemySpawnParameters.dungeonLevel), roomEnemySpawnParameters.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(roomEnemySpawnParameters.minTotalEnemiesToSpawn),
                    roomEnemySpawnParameters.minTotalEnemiesToSpawn, nameof(roomEnemySpawnParameters.maxTotalEnemiesToSpawn),
                    roomEnemySpawnParameters.maxTotalEnemiesToSpawn, true);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(roomEnemySpawnParameters.minSpawnInterval),
                    roomEnemySpawnParameters.minSpawnInterval, nameof(roomEnemySpawnParameters.maxSpawnInterval),
                    roomEnemySpawnParameters.maxSpawnInterval, true);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(roomEnemySpawnParameters.minConcurrentEnemies),
                    roomEnemySpawnParameters.minConcurrentEnemies, nameof(roomEnemySpawnParameters.maxConcurrentEnemies),
                    roomEnemySpawnParameters.maxConcurrentEnemies, false);
                bool isEnemyTypesListForDungeonLevel = false;

                //验证敌人类型列表的有效性
                foreach (SpawnableObjectsByLevel<EnemyDetailsSO> dungeonObjectsByLevel in enemiesByLevelList)
                {
                    if (dungeonObjectsByLevel.dungeonLevel == roomEnemySpawnParameters.dungeonLevel && dungeonObjectsByLevel.spawnableObjectRatioList.Count > 0)
                    {
                        isEnemyTypesListForDungeonLevel = true;
                    }

                    HelperUtilities.ValidateCheckNullValue(this, nameof(dungeonObjectsByLevel.dungeonLevel), dungeonObjectsByLevel.dungeonLevel);

                    foreach (SpawnableObjectRatio<EnemyDetailsSO> dungeonObjectRatio in dungeonObjectsByLevel.spawnableObjectRatioList)
                    {
                        HelperUtilities.ValidateCheckNullValue(this, nameof(dungeonObjectRatio.dungeonObject), dungeonObjectRatio.dungeonObject);

                        HelperUtilities.ValidateCheckPositiveValue(this, nameof(dungeonObjectRatio.ratio), dungeonObjectRatio.ratio, false);
                    }

                }

                if (isEnemyTypesListForDungeonLevel == false && roomEnemySpawnParameters.dungeonLevel != null)
                {
                    Debug.Log("No enemy types specified in for dungeon level " + roomEnemySpawnParameters.dungeonLevel.levelName + " in gameobject" + this.name.ToString());
                }
            }
        }

        //检查生成位置是否已填充完整
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);
    }
#endif
    #endregion Validation
}
