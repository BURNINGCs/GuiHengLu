using System.Collections.Generic;
using UnityEngine;

public class Room 
{
    public string id;
    public string templateID;
    public GameObject prefab;
    public RoomNodeTypeSO roomNodeType;
    public MusicTrackSO battleMusic;
    public MusicTrackSO ambientMusic;
    public Vector2Int lowerBounds;
    public Vector2Int upperBounds;
    public Vector2Int templateLowerBounds;
    public Vector2Int templateUpperBounds;
    public Vector2Int[] spawnPositionArray;
    public List<SpawnableObjectsByLevel<EnemyDetailsSO>> enemiesByLevelList;
    public List<RoomEnemySpawnParameters> roomLevelEnemySpawnParametersList;
    public List<string> childRoomIDList;
    public string parentRoomID;
    public List<Doorway> doorWayList;
    public bool isPositioned = false;
    public InstantiatedRoom instantiatedRoom;
    public bool isLit = false;
    public bool isClearedOfEnemies = false;
    public bool isPreviouslyVisited = false;

    public Room()
    {
        childRoomIDList = new List<string>();
        doorWayList = new List<Doorway>();
    }

    //获取当前地下城等级下，该房间应生成的敌人数量
    public int GetNumberOfEnemiesToSpawn(DungeonLevelSO dungeonLevel)
    {
        foreach (RoomEnemySpawnParameters roomEnemySpawnParameters in roomLevelEnemySpawnParametersList)
        {
            if (roomEnemySpawnParameters.dungeonLevel == dungeonLevel)
            {
                return Random.Range(roomEnemySpawnParameters.minTotalEnemiesToSpawn, roomEnemySpawnParameters.maxTotalEnemiesToSpawn);
            }
        }

        return 0;
    }

    //获取当前地下城等级对应的房间敌人生成参数 —— 若未找到对应参数，则返回空值
    public RoomEnemySpawnParameters GetRoomEnemySpawnParameters(DungeonLevelSO dungeonLevel)
    {
        foreach (RoomEnemySpawnParameters roomEnemySpawnParameters in roomLevelEnemySpawnParametersList)
        {
            if (roomEnemySpawnParameters.dungeonLevel == dungeonLevel)
            {
                return roomEnemySpawnParameters;
            }
        }
        return null;
    }

}
