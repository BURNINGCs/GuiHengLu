using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public static class Settings
{
    #region UNITS
    public const float pixelsPerUnit = 16f;
    public const float tileSizePixels = 16f;
    #endregion

    #region DUNGEON BUILD SETTINGS
    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion

    #region ROOM SETTINGS
    public const float fadeInTime = 0.5f; //房间淡入时间
    //一个房间可引出的子走廊最大数量。—— 最大值应为 3 个，不过不建议设置这么多，
    //因为这可能导致地牢生成失败，因为房间之间更可能无法合理拼接在一起。
    public const int maxChildCorridors = 3;
    public const float doorUnlockDelay = 1f;
    #endregion

    #region ANIMATOR PARAMETERS
    // Animator parameters - Player
    //public static int aimUp = Animator.StringToHash("aimUp");
    //public static int aimDown = Animator.StringToHash("aimDown");
    //public static int aimUpRight = Animator.StringToHash("aimUpRight");
    //public static int aimUpLeft = Animator.StringToHash("aimUpLeft");
    public static int aimRight = Animator.StringToHash("aimRight");
    public static int aimLeft = Animator.StringToHash("aimLeft");
    public static int isIdle = Animator.StringToHash("isIdle");
    public static int isMoving = Animator.StringToHash("isMoving");
    public static float baseSpeedForPlayerAnimations = 8f;
    public static int use = Animator.StringToHash("use");

    //动画控制器参数 - 敌人
    public static float baseSpeedForEnemyAnimations = 3f;

    //动画控制器参数 - 门（Door 相关）
    public static int open = Animator.StringToHash("open");

    #endregion

    #region GAMEOBJECT TAGS
    public const string playerTag = "Player";
    public const string playerWeapon = "playerWeapon";
    #endregion

    #region AUDIO
    public const float musicFadeOutTime = 0.5f;//音乐淡出时间
    public const float musicFadeInTime = 0.5f;//音乐淡入时间
    #endregion

    #region FIRING CONTROL
    //若目标距离小于此值，则使用瞄准角度（基于玩家计算），否则使用武器瞄准角度（基于武器计算）
    public const float useAimAngleDistance = 3.5f;
    #endregion

    #region ASTAR PATHFINDING PARAMETERS
    public const int defaultAStarMovementPenalty = 40;
    public const int preferredPathAStarMovementPenalty = 1;
    public const int targetFrameRateToSpreadPathfindingOver = 60;
    public const float playerMoveDistanceToRebuildPath = 3f;
    public const float enemyPathRebuildCooldown = 2f;
    #endregion

    #region ENEMY PARAMETERS
    public const int defaultEnemyHealth = 20;
    #endregion

    #region UI PARAMETERS
    public const float uiHeartSpacing = 16f;
    public const float uiAmmoIconSpacing = 4f;
    #endregion

    #region CONTACT DAMAGE PARAMETERS
    public const float contactDamageCollisionResetDelay = 0.5f;
    #endregion

    #region HIGHSCORES
    public const int numberOfHighScoresToSave = 100;
    #endregion
}
