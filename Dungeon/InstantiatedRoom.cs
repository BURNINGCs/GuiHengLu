using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public int[,] aStarMovementPenalty; //使用这个二维数组存储来自瓦片地图的移动惩罚值，供 A 星路径寻路算法使用
    [HideInInspector] public int[,] aStarItemObstacles; //用于存储可作为障碍物的可移动物品的位置
    [HideInInspector] public Bounds roomColliderBounds;
    [HideInInspector] public List<MoveItem> moveableItemsList = new List<MoveItem>();

    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        //保存房间碰撞体边界
        roomColliderBounds = boxCollider2D.bounds;
    }

    private void Start()
    {
        //更新可移动物品障碍物数组
        CreateItemObstaclesArray();
    }

    //当玩家进入房间时触发房间变更事件
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //若玩家触发了碰撞体
        if (collision.tag == Settings.playerTag && room != GameManager.Instance.GetCurrentRoom())
        {
            //将房间标记为已访问
            this.room.isPreviouslyVisited = true;

            //调用房间变更事件
            StaticEventHandler.CallRoomChangedEvent(room);
        }
    }

    //初始化实例化的房间
    public void Initialise(GameObject roomGameobject)
    {
        PopulateTilemapMemberVariables(roomGameobject);

        BlockOffUnusedDoorWays();

        AddObstaclesAndPreferredPaths();

        CreateItemObstaclesArray();

        AddDoorsToRooms();

        DisableCollisionTilemapRenderer();
    }

    //填充瓦片地图和网格成员变量
    private void PopulateTilemapMemberVariables(GameObject roomGameobject)
    {
        //获取网格组件
        grid = roomGameobject.GetComponentInChildren<Grid>();

        //获取子物体中的瓦片地图
        Tilemap[] tilemaps = roomGameobject.GetComponentsInChildren<Tilemap>();

        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.tag == "groundTilemap")
            {
                groundTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration1Tilemap")
            {
                decoration1Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration2Tilemap")
            {
                decoration2Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "frontTilemap")
            {
                frontTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "collisionTilemap")
            {
                collisionTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "minimapTilemap")
            {
                minimapTilemap = tilemap;
            }
        }
    }

    //封锁房间中未使用的门口
    private void BlockOffUnusedDoorWays()
    {
        //遍历所有门口
        foreach (Doorway doorway in room.doorWayList)
        {
            if (doorway.isConnected)
                continue;

            //在瓦片地图上用瓦片封锁未连接的门口
            if (collisionTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(collisionTilemap, doorway);
            }

            if (minimapTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(minimapTilemap, doorway);
            }

            if (groundTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(groundTilemap, doorway);
            }

            if (decoration1Tilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration1Tilemap, doorway);
            }

            if (decoration2Tilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration2Tilemap, doorway);
            }

            if (frontTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(frontTilemap, doorway);
            }
        }
    }

    //在瓦片地图层上封锁门口
    private void BlockADoorwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch (doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayHorizontally(tilemap, doorway);
                break;

            case Orientation.east:
            case Orientation.west:
                BlockDoorwayVertically(tilemap, doorway);
                break;

            case Orientation.none:
                break;
        }
    }

    //水平封锁门口 —— 用于南北方向的门口
    private void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        //遍历所有需要复制的瓦片
        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                //获取被复制瓦片的旋转角度
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                //复制瓦片
                tilemap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x +
                    xPos, startPosition.y - yPos, 0)));

                //设置复制后瓦片的旋转角度
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);
            }
        }
    }

    //垂直封锁门口 —— 用于东西方向的门口
    private void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        //遍历所有需要复制的瓦片
        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {

            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
            {
                //获取被复制瓦片的旋转角度
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                //复制瓦片
                tilemap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x +
                    xPos, startPosition.y - yPos, 0)));

                //设置复制后瓦片的旋转角度
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);
            }
        }
    }

    //更新 A 星路径寻路所使用的障碍物信息
    private void AddObstaclesAndPreferredPaths()
    {
        //该数组将填充墙体障碍物数据
        aStarMovementPenalty = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y - room.templateLowerBounds.y + 1];

        //遍历所有网格方格
        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                //为网格方格设置默认移动惩罚值
                aStarMovementPenalty[x, y] = Settings.defaultAStarMovementPenalty;

                //为敌人无法行走的碰撞瓦片添加障碍物
                TileBase tile = collisionTilemap.GetTile(new Vector3Int(x + room.templateLowerBounds.x, y + room.templateLowerBounds.y, 0));

                foreach (TileBase collisionTile in GameResources.Instance.enemyUnwalkableCollisionTilesArray)
                {
                    if (tile == collisionTile)
                    {
                        aStarMovementPenalty[x, y] = 0;
                        break;
                    }
                }

                //为敌人设置优先路径（优先路径值为 1，网格位置的默认值在 “设置（Settings）” 中指定）
                if (tile == GameResources.Instance.preferredEnemyPathTile)
                {
                    aStarMovementPenalty[x, y] = Settings.preferredPathAStarMovementPenalty;
                }

            }
        }
    }

    // 若当前房间不是走廊房间，则添加可开启的门
    private void AddDoorsToRooms()
    {
        //如果房间是走廊，则返回
        if (room.roomNodeType.isCorridorEW || room.roomNodeType.isCorridorNS) return;

        //在门口位置实例化门预制体
        foreach (Doorway doorway in room.doorWayList)
        {
            //如果门口预制体不为空且门口处于连接状态
            if (doorway.doorPrefab != null && doorway.isConnected)
            {
                float tileDistance = Settings.tileSizePixels / Settings.pixelsPerUnit;

                GameObject door = null;

                if (doorway.orientation == Orientation.north)
                {
                    //创建门，其父对象设为当前房间
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y + tileDistance, 0f);
                }
                else if (doorway.orientation == Orientation.south)
                {
                    //创建门，其父对象设为当前房间
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y, 0f);
                }
                else if (doorway.orientation == Orientation.east)
                {
                    //创建门，其父对象设为当前房间
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance, doorway.position.y + tileDistance * 1.25f, 0f);
                }
                else if (doorway.orientation == Orientation.west)
                {
                    //创建门，其父对象设为当前房间
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x, doorway.position.y + tileDistance * 1.25f, 0f);
                }

                //获取门组件
                Door doorComponent = door.GetComponent<Door>();

                //设置门是否属于 Boss 房间
                if (room.roomNodeType.isBossRoom)
                {
                    doorComponent.isBossRoomDoor = true;

                    //锁定门以阻止进入房间
                    doorComponent.LockDoor();

                    //在门的位置为小地图实例化boss图标
                    GameObject bossIcon = Instantiate(GameResources.Instance.minimapBossPrefab, gameObject.transform);
                    bossIcon.transform.localPosition = door.transform.localPosition;
                }
            }
        }
    }

    //禁用碰撞瓦片地图渲染器
    private void DisableCollisionTilemapRenderer()
    {
        //禁用碰撞瓦片地图渲染器
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }

    //禁用用于检测玩家进入房间的房间触发碰撞体
    public void DisableRoomCollider()
    {
        boxCollider2D.enabled = false;
    }

    //启用用于检测玩家进入房间的房间触发碰撞体
    public void EnableRoomCollider()
    {
        boxCollider2D.enabled = true;
    }

    //锁定房间的门
    public void LockDoors()
    {
        Door[] doorArray = GetComponentsInChildren<Door>();

        //触发房门锁定逻辑
        foreach (Door door in doorArray)
        {
            door.LockDoor();    
        }

        //禁用房间触发碰撞体
        DisableRoomCollider();
    }

    //解锁房间的门
    public void UnlockDoors(float doorUnlockDelay)
    {
        StartCoroutine(UnlockDoorsRoutine(doorUnlockDelay));
    }

    //执行解锁房间门的处理流程
    private IEnumerator UnlockDoorsRoutine(float doorUnlockDelay)
    {
        if (doorUnlockDelay > 0f)
            yield return new WaitForSeconds(doorUnlockDelay);

        Door[] doorArray = GetComponentsInChildren<Door>();

        //触发房门开启逻辑
        foreach (Door door in doorArray)
        {
            door.UnlockDoor();
        }

        //启用房间触发碰撞体
        EnableRoomCollider();
    }

    //创建物品障碍物数组
    //private void CreateItemObstaclesArray()
    //{
    //    //这个数组将在游戏过程中填充所有可移动障碍物信息
    //    aStarItemObstacles = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y - room.templateLowerBounds.y + 1];
    //}

    //创建物品障碍物数组
    private void CreateItemObstaclesArray()
    {
        int width = room.templateUpperBounds.x - room.templateLowerBounds.x + 1;
        int height = room.templateUpperBounds.y - room.templateLowerBounds.y + 1;

        //创建数组并立即初始化
        aStarItemObstacles = new int[width, height];

        //初始化所有单元格为默认值
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                aStarItemObstacles[x, y] = Settings.defaultAStarMovementPenalty;
            }
        }
    }

    //用默认A*移动惩罚值初始化物品障碍物数组
    private void InitializeItemObstaclesArray()
    {
        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                //为网格方块设置默认移动惩罚值
                aStarItemObstacles[x, y] = Settings.defaultAStarMovementPenalty;
            }
        }
    }

    //更新可移动障碍物数组
    public void UpdateMoveableObstacles()
    {
        InitializeItemObstaclesArray();

        foreach (MoveItem moveItem in moveableItemsList)
        {
            Vector3Int colliderBoundsMin = grid.WorldToCell(moveItem.boxCollider2D.bounds.min);
            Vector3Int colliderBoundsMax = grid.WorldToCell(moveItem.boxCollider2D.bounds.max);

            //循环遍历并将可移动物品碰撞体边界添加到障碍物数组
            for (int i = colliderBoundsMin.x; i <= colliderBoundsMax.x; i++)
            {
                for (int j = colliderBoundsMin.y; j <= colliderBoundsMax.y; j++)
                {
                    aStarItemObstacles[i - room.templateLowerBounds.x, j - room.templateLowerBounds.y] = 0;
                }
            }
        }
    }
}
