using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMap : SingletonMonobehaviour<DungeonMap>
{
    #region Header GameObject References
    [Space(10)]
    [Header("游戏对象的引用")]
    #endregion
    #region Tooltip
    [Tooltip("请填充 MinimapUI 游戏对象")]
    #endregion
    [SerializeField] private GameObject minimapUI;
    private Camera dungeonMapCamera;
    private Camera cameraMain;

    private void Start()
    {
        //缓存主摄像机
        cameraMain = Camera.main;

        //获取玩家变换组件
        Transform playerTransform = GameManager.Instance.GetPlayer().transform;

        //将玩家设为Cinemachine的跟随目标
        CinemachineVirtualCamera cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = playerTransform;
        
        //获取地下城地图摄像机并禁用它
        dungeonMapCamera = GetComponentInChildren<Camera>();
        dungeonMapCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        //鼠标左键按下 且 游戏状态为地下城总览地图，则获取点击的房间
        if (Input.GetMouseButtonDown(0) && GameManager.Instance.gameState == GameState.dungeonOverviewMap)
        {
            GetRoomClicked();
        }
    }

    //获取地图上点击的房间
    private void GetRoomClicked()
    {
        //将屏幕位置转换为世界位置
        Vector3 worldPosition = dungeonMapCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPosition = new Vector3(worldPosition.x, worldPosition.y, 0f);

        //检查光标位置的碰撞体
        Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), 1f);

        //检查是否有碰撞体是房间
        foreach (Collider2D collider2D in collider2DArray)
        {
            if (collider2D.GetComponent<InstantiatedRoom>() != null)
            {
                InstantiatedRoom instantiatedRoom = collider2D.GetComponent<InstantiatedRoom>();

                //如果点击的房间已清除敌人且之前访问过，则将玩家移动到该房间
                if (instantiatedRoom.room.isClearedOfEnemies && instantiatedRoom.room.isPreviouslyVisited)
                {
                    //将玩家移动到房间
                    StartCoroutine(MovePlayerToRoom(worldPosition, instantiatedRoom.room));
                }
            }
        }
    }

    //将玩家移动到选定的房间
    private IEnumerator MovePlayerToRoom(Vector3 worldPosition, Room room)
    {
        //调用房间变更事件
        StaticEventHandler.CallRoomChangedEvent(room);

        //屏幕淡出为黑色
        yield return StartCoroutine(GameManager.Instance.Fade(0f, 1f, 0f, Color.black));

        //关闭地下城总览地图
        ClearDungeonOverViewMap();

        //在淡出期间禁用玩家控制
        GameManager.Instance.GetPlayer().playerControl.DisablePlayer();

        //获取距离玩家最近的房间中的最近生成点
        Vector3 spawnPosition = HelperUtilities.GetSpawnPositionNearestToPlayer(worldPosition);

        //将玩家移动到新位置 - 在最近的生成点生成他们
        GameManager.Instance.GetPlayer().transform.position = spawnPosition;

        //屏幕淡入
        yield return StartCoroutine(GameManager.Instance.Fade(1f, 0f, 1f, Color.black));

        //启用玩家控制
        GameManager.Instance.GetPlayer().playerControl.EnablePlayer();
    }

    //显示地下城总览地图
    public void DisplayDungeonOverViewMap()
    {
        //设置游戏状态
        GameManager.Instance.previousGameState = GameManager.Instance.gameState;
        GameManager.Instance.gameState = GameState.dungeonOverviewMap;

        //禁用玩家
        GameManager.Instance.GetPlayer().playerControl.DisablePlayer();

        //禁用主摄像机 + 开启地下城总览摄像机
        cameraMain.gameObject.SetActive(false);
        dungeonMapCamera.gameObject.SetActive(true);

        //确保所有房间都处于活动状态以便显示
        ActivateRoomsForDisplay();
        //禁用MinimapUI
        minimapUI.SetActive(false);
    }

    //关闭地下城总览地图
    public void ClearDungeonOverViewMap()
    {
        //设置游戏状态
        GameManager.Instance.gameState = GameManager.Instance.previousGameState;
        GameManager.Instance.previousGameState = GameState.dungeonOverviewMap;

        //启用玩家
        GameManager.Instance.GetPlayer().playerControl.EnablePlayer();

        //启用主摄像机 + 禁用地下城总览摄像机
        cameraMain.gameObject.SetActive(true);
        dungeonMapCamera.gameObject.SetActive(false);

        //启用MinimapUI
        minimapUI.SetActive(true);
    }

    //确保所有房间都处于活动状态以便显示
    private void ActivateRoomsForDisplay()
    {
        //遍历地下城房间
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;
            room.instantiatedRoom.gameObject.SetActive(true);
        }
    }
}
