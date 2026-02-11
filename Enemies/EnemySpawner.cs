using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawner : SingletonMonobehaviour<EnemySpawner>
{
    private int enemiesToSpawn;
    private int currentEnemyCount;
    private int enemiesSpawnedSoFar;
    private int enemyMaxConcurrentSpawnNumber;
    private Room currentRoom;
    private RoomEnemySpawnParameters roomEnemySpawnParameters;

    private void OnEnable()
    {
        //订阅房间变更事件
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        //取消订阅房间变更事件
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }


    //处理房间变更逻辑
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        enemiesSpawnedSoFar = 0;
        currentEnemyCount = 0;

        currentRoom = roomChangedEventArgs.room;

        //更新房间音乐（环境音乐）
        MusicManager.Instance.PlayMusic(currentRoom.ambientMusic, 0.2f, 2f);

        //若当前房间是走廊或入口，则直接返回（不执行敌人生成逻辑）
        if (currentRoom.roomNodeType.isCorridorEW || currentRoom.roomNodeType.isCorridorNS || currentRoom.roomNodeType.isEntrance)
            return;

        //若该房间已被攻克，则直接返回（不重复生成敌人）
        if (currentRoom.isClearedOfEnemies) return;

        //获取随机的敌人生成总数
        enemiesToSpawn = currentRoom.GetNumberOfEnemiesToSpawn(GameManager.Instance.GetCurrentDungeonLevel());

        //获取该房间的敌人生成参数
        roomEnemySpawnParameters = currentRoom.GetRoomEnemySpawnParameters(GameManager.Instance.GetCurrentDungeonLevel());

        //若无需生成敌人，则返回
        if (enemiesToSpawn == 0)
        {
            //将该房间标记为 “已清理”
            currentRoom.isClearedOfEnemies = true;

            return;
        }

        //获取随机的敌人同时生成数量
        enemyMaxConcurrentSpawnNumber = GetConcurrentEnemies();

        //更新房间音乐（战斗音乐）
        MusicManager.Instance.PlayMusic(currentRoom.battleMusic, 0.2f, 0.5f);

        //锁定房间门（防止玩家中途退出）
        currentRoom.instantiatedRoom.LockDoors();

        //生成敌人
        SpawnEnemies();
    }

    //生成敌人
    private void SpawnEnemies()
    {
        // Set gamestate engaging boss
        if (GameManager. Instance.gameState == GameState.bossStage)
        {
            GameManager.Instance.previousGameState = GameState.bossStage;
            GameManager.Instance.gameState = GameState.engagingBoss;
        }

        //将游戏状态设置为 “与敌人交战中”
        else if (GameManager.Instance.gameState == GameState.playingLevel)
        {
            GameManager.Instance.previousGameState = GameState.playingLevel;
            GameManager.Instance.gameState = GameState.engagingEnemies;
        }

        StartCoroutine(SpawnEnemiesRoutine());
    }

    //生成敌人的协程
    private IEnumerator SpawnEnemiesRoutine()
    {
        Grid grid = currentRoom.instantiatedRoom.grid;

        //创建用于随机选取敌人类型的辅助类实例
        RandomSpawnableObject<EnemyDetailsSO> randomEnemyHelperClass = new RandomSpawnableObject<EnemyDetailsSO>(currentRoom.enemiesByLevelList);

        //检查是否存在合法的敌人生成位置
        if (currentRoom.spawnPositionArray.Length > 0)
        {
            //循环创建所有敌人
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                //等待当前敌人数量低于最大同时生成数量
                while (currentEnemyCount >= enemyMaxConcurrentSpawnNumber)
                {
                    yield return null;
                }

                Vector3Int cellPosition = (Vector3Int)currentRoom.spawnPositionArray[Random.Range(0, currentRoom.spawnPositionArray.Length)];

                //创建敌人 —— 获取下一个要生成的敌人类型
                CreateEnemy(randomEnemyHelperClass.GetItem(), grid.CellToWorld(cellPosition));

                yield return new WaitForSeconds(GetEnemySpawnInterval());
            }
        }
    }

    //获取随机的敌人生成间隔（在最小值与最大值之间取值）
    private float GetEnemySpawnInterval()
    {
        return (Random.Range(roomEnemySpawnParameters.minSpawnInterval, roomEnemySpawnParameters.maxSpawnInterval));
    }

    //获取随机的敌人同时生成数量（在最小值与最大值之间取值）
    private int GetConcurrentEnemies()
    {
        return (Random.Range(roomEnemySpawnParameters.minConcurrentEnemies, roomEnemySpawnParameters.maxConcurrentEnemies));
    }

    //在指定位置创建敌人
    private void CreateEnemy(EnemyDetailsSO enemyDetails, Vector3 position)
    {
        //记录当前已生成的敌人数量
        enemiesSpawnedSoFar++;

        //将当前敌人数量加 1—— 该数值会在敌人被销毁时减少
        currentEnemyCount++;

        //获取当前地下城关卡等级
        DungeonLevelSO dungeonLevel = GameManager.Instance.GetCurrentDungeonLevel();

        //实例化敌人
        GameObject enemy = Instantiate(enemyDetails.enemyPrefab, position, Quaternion.identity, transform);

        //初始化敌人
        enemy.GetComponent<Enemy>().EnemyInitialization(enemyDetails, enemiesSpawnedSoFar, dungeonLevel);

        //订阅敌人销毁事件
        enemy.GetComponent<DestroyedEvent>().OnDestroyed += Enemy_OnDestroyed;
    }

    //处理敌人被销毁的逻辑
    private void Enemy_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        //取消订阅事件
        destroyedEvent.OnDestroyed -= Enemy_OnDestroyed;

        //减少当前敌人数量
        currentEnemyCount--;

        // Score points - call points scored event
        StaticEventHandler.CallPointsScoredEvent(destroyedEventArgs.points);

        if (currentEnemyCount <= 0 && enemiesSpawnedSoFar == enemiesToSpawn)
        {
            currentRoom.isClearedOfEnemies = true;

            //设置游戏状态
            if (GameManager.Instance.gameState == GameState.engagingEnemies)
            {
                GameManager.Instance.gameState = GameState.playingLevel;
                GameManager.Instance.previousGameState = GameState.engagingEnemies;
            }

            else if (GameManager.Instance.gameState == GameState.engagingBoss)
            {
                GameManager.Instance.gameState = GameState.bossStage;
                GameManager.Instance.previousGameState = GameState.engagingBoss;
            }

            //解锁房门
            currentRoom.instantiatedRoom.UnlockDoors(Settings.doorUnlockDelay);

            //更新房间音乐（环境音乐）
            MusicManager.Instance.PlayMusic(currentRoom.ambientMusic, 0.2f, 2f);

            //触发 “房间敌人已被击败” 事件
            StaticEventHandler.CallRoomEnemiesDefeatedEvent(currentRoom);
        }
    }
}
