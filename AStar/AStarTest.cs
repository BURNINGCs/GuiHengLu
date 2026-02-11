using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarTest : MonoBehaviour
{
    private InstantiatedRoom instantiatedRoom;
    private Grid grid;
    private Tilemap frontTilemap;
    private Tilemap pathTilemap;
    private Vector3Int startGridPosition;
    private Vector3Int endGridPosition;
    private TileBase startPathTile;
    private TileBase finishPathTile;

    private Vector3Int noValue = new Vector3Int(9999, 9999, 9999);
    private Stack<Vector3> pathStack;

    private void OnEnable()
    {
        //订阅房间变更事件（onRoomChanged）
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        //取消订阅房间变更事件（onRoomChanged）
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void Start()
    {
        startPathTile = GameResources.Instance.preferredEnemyPathTile;
        finishPathTile = GameResources.Instance.enemyUnwalkableCollisionTilesArray[0];
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        pathStack = null;
        instantiatedRoom = roomChangedEventArgs.room.instantiatedRoom;
        frontTilemap = instantiatedRoom.transform.Find("Grid/Tilemap4_Front").GetComponent<Tilemap>();
        grid = instantiatedRoom.transform.GetComponentInChildren<Grid>();
        startGridPosition = noValue;
        endGridPosition = noValue;

        SetUpPathTilemap();
    }

    //使用前景瓦片地图（front tilemap）的克隆作为路径瓦片地图。若尚未创建克隆，则新建一个；否则直接使用已有克隆
    private void SetUpPathTilemap()
    {
        Transform tilemapCloneTransform = instantiatedRoom.transform.Find("Grid/Tilemap4_Front(Clone)");

        //若前景瓦片地图尚未被克隆，则执行克隆操作
        if (tilemapCloneTransform == null)
        {
            pathTilemap = Instantiate(frontTilemap, grid.transform);
            pathTilemap.GetComponent<TilemapRenderer>().sortingOrder = 2;
            pathTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
            pathTilemap.gameObject.tag = "Untagged";
        }
        //否则直接使用该克隆地图
        else
        {
            pathTilemap = instantiatedRoom.transform.Find("Grid/Tilemap4_Front(Clone)").GetComponent<Tilemap>();
            pathTilemap.ClearAllTiles();
        }
    }

    //每帧调用一次 Update 方法
    private void Update()
    {
        if (instantiatedRoom == null || startPathTile == null || finishPathTile == null || grid == null || pathTilemap == null) return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            ClearPath();
            SetStartPosition();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            ClearPath();
            SetEndPosition();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            DisplayPath();
        }
    }

    //在前景瓦片地图上设置起始位置与起始瓦片
    private void SetStartPosition()
    {
        if (startGridPosition == noValue)
        {
            startGridPosition = grid.WorldToCell(HelperUtilities.GetMouseWorldPosition());

            if (!IsPositionWithinBounds(startGridPosition))
            {
                startGridPosition = noValue;
                return;
            }

            pathTilemap.SetTile(startGridPosition, startPathTile);
        }
        else
        {
            pathTilemap.SetTile(startGridPosition, null);
            startGridPosition = noValue;
        }
    }

    //在前景瓦片地图上设置结束位置与结束瓦片
    private void SetEndPosition()
    {
        if (endGridPosition == noValue)
        {
            endGridPosition = grid.WorldToCell(HelperUtilities.GetMouseWorldPosition());

            if (!IsPositionWithinBounds(endGridPosition))
            {
                endGridPosition = noValue;
                return;
            }

            pathTilemap.SetTile(endGridPosition, finishPathTile);
        }
        else
        {
            pathTilemap.SetTile(endGridPosition, null);
            endGridPosition = noValue;
        }
    }

    //检查该位置是否在房间的上下边界范围内
    private bool IsPositionWithinBounds(Vector3Int position)
    {
        //若位置超出网格范围，则返回 false
        if (position.x < instantiatedRoom.room.templateLowerBounds.x || position.x > instantiatedRoom.room.templateUpperBounds.x
            || position.y < instantiatedRoom.room.templateLowerBounds.y || position.y > instantiatedRoom.room.templateUpperBounds.y)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //清空路径并重置起始与结束位置
    private void ClearPath()
    {
        //清空路径
        if (pathStack == null) return;

        foreach (Vector3 worldPosition in pathStack)
        {
            pathTilemap.SetTile(grid.WorldToCell(worldPosition), null);
        }

        pathStack = null;

        //清空起始和结束方格
        endGridPosition = noValue;
        startGridPosition = noValue;
    }

    //生成并显示起始位置与结束位置之间的 A 星路径
    private void DisplayPath()
    {
        if (startGridPosition == noValue || endGridPosition == noValue) return;

        pathStack = AStar.BuildPath(instantiatedRoom.room, startGridPosition, endGridPosition);

        if (pathStack == null) return;

        foreach (Vector3 worldPosition in pathStack)
        {
            pathTilemap.SetTile(grid.WorldToCell(worldPosition), startPathTile);
        }
    }
}
