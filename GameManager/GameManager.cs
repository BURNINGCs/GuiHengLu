using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>
{
    #region Header GAMEOBJECT REFERENCES
    [Space(10)]
    [Header("游戏对象引用")]
    #endregion Header GAMEOBJECT REFERENCES

    #region Tooltip
    [Tooltip("填充卷轴ScrollUI")]
    #endregion Tooltip
    [SerializeField] private ScrollUI scrollUI;

    #region Tooltip
    [Tooltip("填充暂停菜单对象")]
    #endregion
    [SerializeField] private GameObject pauseMenu;

    #region Tooltip
    [Tooltip("填充FadeScreenUI（屏幕淡入淡出 UI）中的MessageText文本组件（TextMeshPro）")]
    #endregion Tooltip
    [SerializeField] private TextMeshProUGUI messageTextTMP;

    #region Tooltip
    [Tooltip("填充FadeScreenUI中的FadeImage画布组组件（CanvasGroup）")]
    #endregion Tooltip
    [SerializeField] private CanvasGroup canvasGroup;

    #region Header DUNGEON LEVELS

    [Space(10)]
    [Header("地牢关卡")]

    #endregion Header DUNGEON LEVELS

    #region Tooltip

    [Tooltip("用地牢关卡可脚本化对象填充")]

    #endregion Tooltip

    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    #region Tooltip

    [Tooltip("用于测试的起始地牢关卡填充（注：第一关编号为 0）")]

    #endregion Tooltip

    [SerializeField] private int currentDungeonLevelListIndex = 0;
    private Room currentRoom;
    private Room previousRoom;
    private PlayerDetailsSO playerDetails;
    private Player player;

    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;
    private long gameScore;
    private InstantiatedRoom bossRoom;
    private bool isFading = false;

    protected override void Awake()
    {
        //调用基类
        base.Awake();

        //设置玩家详情 —— 从主菜单保存到当前玩家可脚本化对象中
        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        //实例化玩家
        InstantiatePlayer();
    }

    //在场景中的指定位置创建玩家
    private void InstantiatePlayer()
    {
        //实例化玩家
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);

        //初始化玩家
        player = playerGameObject.GetComponent<Player>();

        player.Initialize(playerDetails);
    }

    private void OnEnable()
    {
        //订阅房间变更事件
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;

        //订阅房间敌人已被击败事件
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;

        //订阅得分事件
        StaticEventHandler.OnPointsScored += StaticEventHandler_OnPointsScored;

        //订阅玩家被销毁事件
        player.destroyedEvent.OnDestroyed += Player_OnDestroyed;
    }

    private void OnDisable()
    {
        //取消订阅房间变更事件
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;

        //取消订阅房间敌人已被击败事件
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;

        //取消订阅得分事件
        StaticEventHandler.OnPointsScored -= StaticEventHandler_OnPointsScored;

        //取消订阅玩家被销毁事件
        player.destroyedEvent.OnDestroyed -= Player_OnDestroyed;
    }

    //处理房间变更事件
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room);
    }

    //处理房间敌人已被击败事件
    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        RoomEnemiesDefeated();
    }

    //处理得分事件
    private void StaticEventHandler_OnPointsScored(PointsScoredArgs pointsScoredArgs)
    {
        //增加分数
        gameScore += pointsScoredArgs.points;

        //调用分数变更事件
        StaticEventHandler.CallScoreChangedEvent(gameScore);
    }

    //处理玩家被销毁事件
    private void Player_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        previousGameState = gameState;
        gameState = GameState.gameLost;
    }

    // Start is called before the first frame update
    private void Start()
    {
        previousGameState = GameState.gameStarted;
        gameState = GameState.gameStarted;

        //将分数重置为零
        gameScore = 0;

        //将屏幕设为全黑
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));
    }

    // Update is called once per frame
    private void Update()
    {
        HandleGameState();

        ////For testing,后面会删
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    gameState = GameState.gameStarted;
        //}
    }

    //处理游戏状态
    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:

                //运行第一关
                PlayDungeonLevel(currentDungeonLevelListIndex);

                gameState = GameState.playingLevel;

                //触发房间敌人已被击败事件（因游戏从无敌人的入口房间开始，以防出现仅包含首领房间的关卡）
                RoomEnemiesDefeated();

                break;

            //游戏进行中 → 按下 Tab 键 显示地下城总览地图
            case GameState.playingLevel:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    DisplayDungeonOverviewMap();
                }

                break;

            //在与敌人交战时，按下 Esc 键打开暂停菜单
            case GameState.engagingEnemies:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }

                break;

            //地图显示中 → 释放 Tab 键 关闭地下城总览地图
            case GameState.dungeonOverviewMap:

                if (Input.GetKeyUp(KeyCode.Tab))
                {
                    DungeonMap.Instance.ClearDungeonOverViewMap();
                }

                break;

            //当处于游玩关卡状态且未与Boss交战时 → 按下 Tab 键 显示地下城总览地图
            case GameState.bossStage:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    DisplayDungeonOverviewMap();
                }

                break;

            //在与Boss交战时，按下 Esc 键打开暂停菜单
            case GameState.engagingBoss:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }

                break;

            //处理关卡完成逻辑
            case GameState.levelCompleted:

                //显示 “关卡完成” 文本
                StartCoroutine(LevelCompleted());

                break;

            //处理游戏胜利逻辑（仅触发一次 —— 通过检测前一游戏状态实现）
            case GameState.gameWon:

                if (previousGameState != GameState.gameWon)
                    StartCoroutine(GameWon());

                break;

            //处理游戏失败逻辑（仅触发一次 —— 通过检测前一游戏状态实现）
            case GameState.gameLost:

                if (previousGameState != GameState.gameLost)
                {
                    StopAllCoroutines(); // 防止玩家在通关的同时阵亡时重复弹出提示信息
                    StartCoroutine(GameLost());
                }

                break;

            //重启游戏
            case GameState.restartGame:

                RestartGame();

                break;

            // 如果游戏已暂停且暂停菜单正在显示，再次按下 Esc 键将关闭暂停菜单
            case GameState.gamePaused:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }
                break;
        }
    }

    //暂停游戏菜单 - 也从暂停菜单的恢复游戏按钮调用
    public void PauseGameMenu()
    {
        if (gameState != GameState.gamePaused)
        {
            pauseMenu.SetActive(true);
            GetPlayer().playerControl.DisablePlayer();
            // 设置游戏状态
            previousGameState = gameState;
            gameState = GameState.gamePaused;
        }
        else if (gameState == GameState.gamePaused)
        {
            pauseMenu.SetActive(false);
            GetPlayer().playerControl.EnablePlayer();
            // 设置游戏状态
            gameState = previousGameState;
            previousGameState = GameState.gamePaused;
        }
    }

    //设置玩家当前所在的房间
    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;

        //调试
        //Debug.Log(room.prefab.name.ToString());
    }

    //房间敌人已被击败 —— 检测地牢所有房间是否已清空敌人，若已清空则加载下一个地牢关卡
    private void RoomEnemiesDefeated()
    {
        //将地牢初始状态设为 “已通关”，随后逐间校验房间状态
        bool isDungeonClearOfRegularEnemies = true;
        bossRoom = null;

        //遍历地牢所有房间，检查是否已清空敌人
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            //暂时跳过首领房间的校验
            if (keyValuePair.Value.roomNodeType.isBossRoom)
            {
                bossRoom = keyValuePair.Value.instantiatedRoom;
                continue;
            }

            //检查其他房间是否已清空敌人
            if (!keyValuePair.Value.isClearedOfEnemies)
            {
                isDungeonClearOfRegularEnemies = false;
                break;
            }
        }

        //设置游戏状态
        //若地牢关卡完全通关（即：除首领房间外其余房间均已通关且无首领房间 或 除首领房间外其余房间均已通关且首领房间也已清空）
        if ((isDungeonClearOfRegularEnemies && bossRoom == null) || (isDungeonClearOfRegularEnemies && bossRoom.room.isClearedOfEnemies))
        {
            //是否存在更多地牢关卡
            if (currentDungeonLevelListIndex < dungeonLevelList.Count - 1)
            {
                gameState = GameState.levelCompleted;
            }
            else
            {
                gameState = GameState.gameWon;
            }
        }
        //否则，若地牢关卡仅余首领房间未通关
        else if (isDungeonClearOfRegularEnemies)
        {
            gameState = GameState.bossStage;

            StartCoroutine(BossStage());
        }
    }

    //显示地下城总览地图
    private void DisplayDungeonOverviewMap()
    {
        //如果正在淡入淡出效果中，则返回（不执行）
        if (isFading)
            return;

        //如果卷轴UI不为空且正在显示卷轴，也不执行
        if (scrollUI != null && scrollUI.IsShowingScroll)
            return;

        //显示地下城总览地图
        DungeonMap.Instance.DisplayDungeonOverViewMap();
    }

    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        //为关卡生成地牢
        bool dungeonBuiltSucessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

        if (!dungeonBuiltSucessfully)
        {
            Debug.LogError("无法通过指定的房间与节点图生成地牢");
        }

        //调用房间已变更的静态事件
        StaticEventHandler.CallRoomChangedEvent(currentRoom);

        //将玩家大致放置在房间中央
        player.gameObject.transform.position = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f, (currentRoom.lowerBounds.y +
            currentRoom.upperBounds.y) / 2f, 0f);


        //在距离玩家最近的房间内，获取该房间中最近的生成点
        player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(player.gameObject.transform.position);

        //显示地牢关卡文本
        StartCoroutine(DisplayDungeonLevelText());

        //显示开场卷轴
        StartCoroutine(ShowOpeningScroll());

        //// ** Demo code
        //RoomEnemiesDefeated();
    }

    //显示开场卷轴
    private IEnumerator ShowOpeningScroll()
    {
        yield return new WaitForSeconds(1f);

        if (scrollUI != null)
        {
            yield return StartCoroutine(ShowScrollUI(ScrollUI.ScrollType.Opening));
        }
    }

    //显示战胜卷轴
    private IEnumerator ShowVictoryScroll()
    {
        if (scrollUI != null)
        {
            yield return StartCoroutine(ShowScrollUI(ScrollUI.ScrollType.Victory));
        }
    }
    
    //显示失败卷轴
    private IEnumerator ShowDefeatScroll()
    {
        if (scrollUI != null)
        {
            yield return StartCoroutine(ShowScrollUI(ScrollUI.ScrollType.Defeat));
        }
    }

    //显示卷轴
    private IEnumerator ShowScrollUI(ScrollUI.ScrollType scrollType)
    {
        if (scrollUI == null)
            yield break;

        //暂停玩家控制
        GetPlayer().playerControl.DisablePlayer();

        //暂停摄像头跟随鼠标
        DisableCameraMouseFollow();

        //显示卷轴
        bool scrollCompleted = false;
        scrollUI.ShowScroll(scrollType, currentDungeonLevelListIndex, () =>
        {
            scrollCompleted = true;
        });

        //等待卷轴完成
        while (!scrollCompleted)
        {
            yield return null;
        }

        //恢复摄像头跟随鼠标
        EnableCameraMouseFollow();

        //恢复玩家控制
        GetPlayer().playerControl.EnablePlayer();
    }

    //禁用摄像头跟随鼠标的方法
    private void DisableCameraMouseFollow()
    {
        CinemachineTargetGroup targetGroup = FindObjectOfType<CinemachineTargetGroup>();

        if (targetGroup != null && targetGroup.m_Targets.Length > 1)
        {
            targetGroup.m_Targets[1].weight = 0f;
            //Debug.Log("摄像头鼠标跟随已禁用");
        }
    }

    //启用摄像头跟随鼠标的方法
    private void EnableCameraMouseFollow()
    {
        CinemachineTargetGroup targetGroup = FindObjectOfType<CinemachineTargetGroup>();

        if (targetGroup != null && targetGroup.m_Targets.Length > 1)
        {
            targetGroup.m_Targets[1].weight = 1f;
            //Debug.Log("摄像头鼠标跟随已启用");
        }
    }

    //显示地牢关卡文本
    private IEnumerator DisplayDungeonLevelText()
    {
        //将屏幕设为全黑
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));

        //GetPlayer().playerControl.DisablePlayer();

        //string messageText = "第  " + (currentDungeonLevelListIndex + 1).ToString() + "  章" + "\n\n" + dungeonLevelList
        //    [currentDungeonLevelListIndex].levelName.ToUpper();

        //yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 2f));

        //GetPlayer().playerControl.EnablePlayer();

        //淡入画面
        yield return StartCoroutine(Fade(1f, 0f, 2f, Color.black));
    }

    //显示提示文本并持续指定时长（displaySeconds）—— 若该时长设为 0，则文本持续显示直至按下回车键
    private IEnumerator DisplayMessageRoutine(string text, Color textColor, float displaySeconds)
    {
        //设置文本内容
        messageTextTMP.SetText(text);
        messageTextTMP.color = textColor;

        //按设定时长显示提示文本
        if (displaySeconds > 0f)
        {
            float timer = displaySeconds;

            while (timer > 0f && !Input.GetKeyDown(KeyCode.Return))
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        else
        //否则，持续显示文本直至按下回车键
        {
            while (!Input.GetKeyDown(KeyCode.Return))
            {
                yield return null;
            }
        }

        yield return null;

        //清空文本内容
        messageTextTMP.SetText("");
    }

    //进入首领关卡阶段
    private IEnumerator BossStage()
    {
        //激活首领房间
        bossRoom.gameObject.SetActive(true);

        //解锁首领房间
        bossRoom.UnlockDoors(0f);

        //等待 2 秒
        yield return new WaitForSeconds(2f);

        //淡入画布以显示文本提示
        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        //显示首领关卡提示信息
        yield return StartCoroutine(DisplayMessageRoutine("干得漂亮 " + GameResources.Instance.currentPlayer.playerName + "！\n你活下来了……至少现在是！ \n现在去找到并击败BOSS！ \n祝你好运！！", Color.white, 5f));

        //淡出画布
        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));
    }

    //标记当前关卡为已完成 —— 加载下一关
    private IEnumerator LevelCompleted()
    {
        //进入下一关
        gameState = GameState.playingLevel;

        //等待 2 秒
        //yield return new WaitForSeconds(2f);

        //淡入画布以显示文本提示
        //yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        //显示 “关卡完成” 文本
        //yield return StartCoroutine(DisplayMessageRoutine("干得漂亮 " + GameResources.Instance.currentPlayer.playerName + "！ \n\n你成功闯过了这层关卡", Color.white, 5f));

        //yield return StartCoroutine(DisplayMessageRoutine("收集所有战利品,然后按下“回车键”\n\n继续深入下一层关卡", Color.white, 5f));

        //淡出画布
        //yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        yield return StartCoroutine(Fade(0f, 1f, 1f, new Color(0f, 0f, 0f, 0.8f)));//淡出画面，透明度为0.8

        yield return StartCoroutine(ShowVictoryScroll());

        ////玩家按下回车键后进入下一关
        //while (!Input.GetKeyDown(KeyCode.Return))
        //{
        //    yield return null;
        //}

        //yield return null; //避免回车键触发重复响应

        //将关卡索引递增至下一关
        currentDungeonLevelListIndex++;

        PlayDungeonLevel(currentDungeonLevelListIndex);
    }

    //画布组淡入淡出过渡
    public IEnumerator Fade(float startFadeAlpha, float targetFadeAlpha, float fadeSeconds, Color backgroundColor)
    {
        isFading = true;

        Image image = canvasGroup.GetComponent<Image>();
        image.color = backgroundColor;

        float time = 0;

        while (time <= fadeSeconds)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startFadeAlpha, targetFadeAlpha, time / fadeSeconds);
            yield return null;
        }

        isFading = false;
    }

    //游戏胜利
    private IEnumerator GameWon()
    {
        previousGameState = GameState.gameWon;

        //禁用玩家对象
        GetPlayer().playerControl.DisablePlayer();

        int rank = HighScoreManager.Instance.GetRank(gameScore);//获得玩家的分数排名
       
        string rankText;

        // 检查分数是否进入排行榜
        if (rank > 0 && rank <= Settings.numberOfHighScoresToSave)
        {
            rankText = "你的分数排名第 " + rank.ToString("#0") + " 位，进入前 " + Settings.numberOfHighScoresToSave.ToString("#0") + " 名";
            
            string name = GameResources.Instance.currentPlayer.playerName;

            if (name == "")
            {
                name = playerDetails.playerCharacterName.ToUpper();
            }
            // 更新分数记录
            HighScoreManager.Instance.AddScore(new Score()
            {
                playerName = name,
                levelDescription = "第 " + (currentDungeonLevelListIndex + 1).ToString() + " 关 - " + GetCurrentDungeonLevel().levelName.ToUpper(),
                playerScore = gameScore
            }, rank);
        }
        else
        {
            rankText = "你的分数未进入前 " + Settings.numberOfHighScoresToSave.ToString("#0") + " 名";
        }

        // 等待 1 秒
        yield return new WaitForSeconds(1f);

        //淡出画面
        //yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        //显示 “游戏胜利” 文本
        //yield return StartCoroutine(DisplayMessageRoutine("干得漂亮 " + GameResources.Instance.currentPlayer.playerName + "！\n你成功击败了整个关卡", Color.white, 3f));

        //yield return StartCoroutine(DisplayMessageRoutine("你的得分: " + gameScore.ToString("###,###0") + "\n\n" + ranText, Color.white, 4f));

        //yield return StartCoroutine(DisplayMessageRoutine("按下“回车键”重新开始游戏", Color.white, 0f));

        yield return StartCoroutine(Fade(0f, 1f, 1f, new Color(0f, 0f, 0f, 0.8f)));//淡出画面，透明度为0.8

        yield return StartCoroutine(ShowVictoryScroll());

        //将游戏状态设置为 “重启游戏”
        gameState = GameState.restartGame;
    }

    //游戏失败
    private IEnumerator GameLost()
    {
        previousGameState = GameState.gameLost;

        //禁用玩家对象
        GetPlayer().playerControl.DisablePlayer();
        int rank = HighScoreManager.Instance.GetRank(gameScore);//获得玩家的分数排名

        string rankText;

        // 检查分数是否进入排行榜
        if (rank > 0 && rank <= Settings.numberOfHighScoresToSave)
        {
            rankText = "你的分数排名第 " + rank.ToString("#0") + " 位，进入前 " + Settings.numberOfHighScoresToSave.ToString("#0") + " 名";

            string name = GameResources.Instance.currentPlayer.playerName;

            if (name == "")
            {
                name = playerDetails.playerCharacterName.ToUpper();
            }
            // 更新分数记录
            HighScoreManager.Instance.AddScore(new Score()
            {
                playerName = name,
                levelDescription = "第 " + (currentDungeonLevelListIndex + 1).ToString() + " 关 - " + GetCurrentDungeonLevel().levelName.ToUpper(),
                playerScore = gameScore
            }, rank);
        }
        else
        {
            rankText = "你的分数未进入前 " + Settings.numberOfHighScoresToSave.ToString("#0") + " 名";
        }

        //等待 1 秒
        yield return new WaitForSeconds(1f);

        //淡出画面
        //yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        //禁用所有敌人（FindObjectsOfType方法会消耗较多资源 —— 但在游戏结束的场景下使用是可行的）
        Enemy[] enemyArray = GameObject.FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemyArray)
        {
            enemy.gameObject.SetActive(false);
        }

        ////显示 “游戏失败” 文本
        //yield return StartCoroutine(DisplayMessageRoutine("运气不佳 " + GameResources.Instance.currentPlayer.playerName + "！你在这层关卡倒下了", Color.white, 2f));

        //yield return StartCoroutine(DisplayMessageRoutine("你的得分: " + gameScore.ToString("###,###0") + "\n\n" + rankText, Color.white, 4f));

        //yield return StartCoroutine(DisplayMessageRoutine("按下回车键重新开始游戏", Color.white, 0f));

        yield return StartCoroutine(Fade(0f, 1f, 1f, new Color(0f, 0f, 0f, 0.8f)));//淡出画面，透明度为0.8

        yield return StartCoroutine(ShowDefeatScroll());//显示失败卷轴

        //将游戏状态设置为 “重启游戏”
        gameState = GameState.restartGame;
    }

    //重启游戏
    private void RestartGame()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    //获取玩家
    public Player GetPlayer()
    {
        return player;
    }

    //获取玩家小地图图标
    public Sprite GetPlayerMiniMapIcon()
    {
        return playerDetails.playerMiniMapIcon;
    }

    //获取玩家头像
    public Sprite GetPlayerHeadIcon()
    {
        return playerDetails.playerHeadIcon;
    }

    //获取玩家当前所在的房间
    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    //获取当前地下城关卡等级
    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }

    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(pauseMenu), pauseMenu);
        HelperUtilities.ValidateCheckNullValue(this, nameof(messageTextTMP), messageTextTMP);
        HelperUtilities.ValidateCheckNullValue(this, nameof(canvasGroup), canvasGroup);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }

#endif

    #endregion Validation
}
