using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyMovementAI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("存储移动详情（如移动速度）的移动详情可脚本化对象（MovementDetailsSO）")]
    #endregion Tooltip
    [SerializeField] private MovementDetailsSO movementDetails;
    private Enemy enemy;
    private Stack<Vector3> movementSteps = new Stack<Vector3>();
    private Vector3 playerReferencePosition;
    private Coroutine moveEnemyRoutine;
    private float currentEnemyPathRebuildCooldown;
    private WaitForFixedUpdate waitForFixedUpdate;
    [HideInInspector] public float moveSpeed;
    private bool chasePlayer = false;
    [HideInInspector] public int updateFrameNumber = 1; //默认值 —— 该值由敌人生成器（enemy spawner）设置
    private List<Vector2Int> surroundingPositionList = new List<Vector2Int>();

    private void Awake()
    {
        //加载组件
        enemy = GetComponent<Enemy>();

        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        //创建用于协程的 “等待固定更新”（WaitForFixedUpdate）对象
        waitForFixedUpdate = new WaitForFixedUpdate();

        //重置玩家参考位置
        playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();
    }

    private void Update()
    {
        MoveEnemy();
    }

    //使用 A 星路径寻路算法生成一条通往玩家的路径，随后将敌人移动到该路径上的每一个网格位置
    private void MoveEnemy()
    {
        //移动冷却计时器
        currentEnemyPathRebuildCooldown -= Time.deltaTime;

        //检查与玩家的距离，判断敌人是否应开始追击玩家
        if (!chasePlayer && Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition()) < enemy.enemyDetails.chaseDistance)
        {
            chasePlayer = true;
        }

        //若与玩家距离未达到追击阈值，则直接返回（不执行后续移动逻辑）
        if (!chasePlayer)
            return;

        //仅在特定帧处理 A 星路径重建，分摊多个敌人的性能负载
        if (Time.frameCount % Settings.targetFrameRateToSpreadPathfindingOver != updateFrameNumber) return;

        //若移动冷却计时器已结束，或玩家移动距离超过设定值，则重新生成敌人的移动路径并移动敌人
        if (currentEnemyPathRebuildCooldown <= 0f || (Vector3.Distance(playerReferencePosition, GameManager.Instance.GetPlayer().GetPlayerPosition()) >
            Settings.playerMoveDistanceToRebuildPath))
        {
            //重置路径重建冷却计时器
            currentEnemyPathRebuildCooldown = Settings.enemyPathRebuildCooldown;

            //重置玩家参考位置
            playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

            //使用 A 星路径寻路移动敌人 —— 触发通往玩家的路径重建
            CreatePath();

            //若成功找到路径，则移动敌人
            if (movementSteps != null)
            {
                if (moveEnemyRoutine != null)
                {
                    //触发闲置事件
                    enemy.idleEvent.CallIdleEvent();
                    StopCoroutine(moveEnemyRoutine);
                }

                //通过协程让敌人沿路径移动
                moveEnemyRoutine = StartCoroutine(MoveEnemyRoutine(movementSteps));
            }
        }
    }

    //用于将敌人移动到路径上下一个位置的协程
    private IEnumerator MoveEnemyRoutine(Stack<Vector3> movementSteps)
    {
        while (movementSteps.Count > 0)
        {
            Vector3 nextPosition = movementSteps.Pop();

            //在未足够接近目标时持续移动 —— 当距离足够近时，切换到路径的下一个节点
            while (Vector3.Distance(nextPosition, transform.position) > 0.2f)
            {
                //触发移动事件
                enemy.movementToPositionEvent.CallMovementToPositionEvent(nextPosition, transform.position, moveSpeed, (nextPosition -
                    transform.position).normalized);

                yield return waitForFixedUpdate; //使用 2D 物理系统移动敌人，因此需等待下一次固定更新（FixedUpdate）
            }

            yield return waitForFixedUpdate;
        }

        //路径节点全部走完 —— 触发敌人闲置事件
        enemy.idleEvent.CallIdleEvent();
    }

    //使用 A 星静态类为敌人生成移动路径
    private void CreatePath()
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        Grid grid = currentRoom.instantiatedRoom.grid;

        //获取玩家在网格上的位置
        Vector3Int playerGridPosition = GetNearestNonObstaclePlayerPosition(currentRoom);

        //获取敌人在网格上的位置
        Vector3Int enemyGridPosition = grid.WorldToCell(transform.position);

        //为敌人生成可供移动的路径
        movementSteps = AStar.BuildPath(currentRoom, enemyGridPosition, playerGridPosition);

        //移除路径的第一个节点 —— 该节点是敌人当前已处于的网格方格
        if (movementSteps != null)
        {
            movementSteps.Pop();
        }
        else
        {
            //触发闲置事件 —— 未找到有效路径
            enemy.idleEvent.CallIdleEvent();
        }
    }

    //设置敌人路径重新计算的帧编号 —— 避免出现性能峰值
    public void SetUpdateFrameNumber(int updateFrameNumber)
    {
        this.updateFrameNumber = updateFrameNumber;
    }

    //获取距离玩家最近且非障碍物的网格位置
    private Vector3Int GetNearestNonObstaclePlayerPosition(Room currentRoom)
    {
        Vector3 playerPosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

        Vector3Int playerCellPosition = currentRoom.instantiatedRoom.grid.WorldToCell(playerPosition);

        Vector2Int adjustedPlayerCellPositon = new Vector2Int(playerCellPosition.x - currentRoom.templateLowerBounds.x, playerCellPosition.y - 
            currentRoom.templateLowerBounds.y);


        //int obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPositon.x, adjustedPlayerCellPositon.y];

        int obstacle = Mathf.Min(currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPositon.x, adjustedPlayerCellPositon.y],
        currentRoom.instantiatedRoom.aStarItemObstacles[adjustedPlayerCellPositon.x, adjustedPlayerCellPositon.y]);

        //若玩家所在的网格方格未被标记为障碍物，则直接返回该位置
        if (obstacle != 0)
        {
            return playerCellPosition;
        }
        //寻找玩家周围非障碍物的相邻网格 —— 此步骤是必要的，因为在 “半碰撞瓦片” 机制下，玩家可能处于被标记为障碍物的网格方格上
        else
        {
            //清空周围位置列表
            surroundingPositionList.Clear();

            //填充周围位置列表 - 会保存围绕(0,0)网格方格的8个可能向量位置
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (j == 0 && i == 0) continue;

                    surroundingPositionList.Add(new Vector2Int(i, j));
                }
            }

            // 循环遍历所有位置
            for (int i = 0; i < 8; i++)
            {
                // 为列表生成一个随机索引
                int index = Random.Range(0, surroundingPositionList.Count);

                // 检查所选周围位置是否存在障碍物
                try
                {
                    obstacle = Mathf.Min(currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPositon.x + surroundingPositionList[index].x,
                        adjustedPlayerCellPositon.y + surroundingPositionList[index].y],
                        currentRoom.instantiatedRoom.aStarItemObstacles[adjustedPlayerCellPositon.x + surroundingPositionList[index].x,
                        adjustedPlayerCellPositon.y + surroundingPositionList[index].y]);

                    // 如果没有障碍物，返回要导航到的单元格位置
                    if (obstacle != 0)
                    {
                        return new Vector3Int(playerCellPosition.x + surroundingPositionList[index].x, playerCellPosition.y + surroundingPositionList[index].y, 0);
                    }
                }

                // 捕获周围位置超出网格范围的错误
                catch
                {

                }

                // 移除有障碍物的周围位置，以便我们可以重试
                surroundingPositionList.RemoveAt(index);
            }

            // 如果在玩家周围未找到无障碍物的单元格 - 将敌人送往一个敌人生成位置的方向
            return (Vector3Int)currentRoom.spawnPositionArray[Random.Range(0, currentRoom.spawnPositionArray.Length)];

            //for (int i = -1; i <= 1; i++)
            //{
            //    for (int j = -1; j <= 1; j++)
            //    {
            //        if (j == 0 && i == 0) continue;

            //        try
            //        {
            //            obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPositon.x + i, adjustedPlayerCellPositon.y + j];
            //            if (obstacle != 0) return new Vector3Int(playerCellPosition.x + i, playerCellPosition.y + j, 0);
            //        }
            //        catch
            //        {
            //            continue;
            //        }
            //    }
            //}

            ////若玩家周围没有非障碍物网格，则直接返回玩家当前位置
            //return playerCellPosition;
        }
    }


    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
    #endregion Validation
}
