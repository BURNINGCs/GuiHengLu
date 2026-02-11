using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Audio;

public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if(instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }

    #region Header DUNGEON
    [Space(10)]
    [Header("地牢")]
    #endregion
    #region Tooltip
    [Tooltip("填充地牢的内容房间节点类型列表的可编写对象")]
    #endregion

    public RoomNodeTypeListSO roomNodeTypeList;

    #region PLAYER SELECTION
    [Space(10)]
    [Header("玩家选择")]
    #endregion PLAYER SELECTION

    #region Tooltip
    [Tooltip("玩家选择预制体")]
    #endregion Tooltip
    public GameObject playerSelectionPrefab;

    #region Header PLAYER
    [Space(10)]
    [Header("玩家")]
    #endregion Header PLAYER

    #region Tooltip
    [Tooltip("玩家详情列表 - 使用玩家详情可脚本化对象填充此列表")]
    #endregion Tooltip
    public List<PlayerDetailsSO> playerDetailsList;

    #region Tooltip
    [Tooltip("当前玩家可脚本化对象 —— 用于在场景之间引用当前玩家")]
    #endregion Tooltip
    public CurrentPlayerSO currentPlayer;

    #region Header MUSIC
    [Space(10)]
    [Header("音乐设置")]
    #endregion Header MUSIC

    #region Tooltip
    [Tooltip("使用音乐主混音器组填充")]
    #endregion
    public AudioMixerGroup musicMasterMixerGroup;

    #region Tooltip
    [Tooltip("主菜单音乐可脚本化对象")]
    #endregion Tooltip
    public MusicTrackSO mainMenuMusic;

    #region Tooltip
    [Tooltip("音乐全音量快照")]
    #endregion Tooltip
    public AudioMixerSnapshot musicOnFullSnapshot;

    #region Tooltip
    [Tooltip("音乐低音量快照")]
    #endregion Tooltip
    public AudioMixerSnapshot musicLowSnapshot;

    #region Tooltip
    [Tooltip("音乐关闭快照")]
    #endregion Tooltip
    public AudioMixerSnapshot musicOffSnapshot;

    #region Header SOUNDS
    [Space(10)]
    [Header("声音相关")]
    #endregion Header
    #region Tooltip
    [Tooltip("用主混音组填充此处")]
    #endregion
    public AudioMixerGroup soundsMasterMixerGroup;
    #region Tooltip
    [Tooltip("门开关音效")]
    #endregion Tooltip
    public SoundEffectSO doorOpenCloseSoundEffect;

    #region Tooltip
    [Tooltip("填充箱子打开音效")]
    public SoundEffectSO chestOpen;
    #endregion
    #region Tooltip
    [Tooltip("填充生命值拾取音效")]
    public SoundEffectSO healthPickup;
    #endregion
    #region Tooltip
    [Tooltip("填充武器拾取音效")]
    public SoundEffectSO weaponPickup;
    #endregion
    #region Tooltip
    [Tooltip("填充弹药拾取音效")]
    public SoundEffectSO ammoPickup;
    #endregion

    #region Header MATERIALS
    [Space(10)]
    [Header("材质")]
    #endregion
    #region Tooltip
    [Tooltip("暗化材质")]
    #endregion
    public Material dimmedMaterial;

    #region Tooltip
    [Tooltip("Sprite-Lit-Default 材质")]
    #endregion
    public Material litMaterial;

    #region Tooltip
    [Tooltip("用可变光照着色器填充")]
    #endregion
    public Shader variableLitShader;
    #region Tooltip
    [Tooltip("填充Materialize Shader")]
    #endregion

    public Shader materializeShader;

    #region Header SPECIAL TILEMAP TILES
    [Space(10)]
    [Header("特殊瓦片地图瓦片")]
    #endregion Header SPECIAL TILEMAP TILES
    #region Tooltip
    [Tooltip("敌人可通行的碰撞瓦片")]
    #endregion Tooltip
    public TileBase[] enemyUnwalkableCollisionTilesArray;
    #region Tooltip
    [Tooltip("供敌人导航使用的优先路径瓦片")]
    #endregion Tooltip
    public TileBase preferredEnemyPathTile;

    #region Header UI
    [Space(10)]
    [Header("UI")]
    #endregion
    #region Tooltip
    [Tooltip("填充心形生命值图像预制体")]
    #endregion
    public GameObject heartPrefab;
    #region Tooltip
    [Tooltip("用弹药图标预制体填充此处")]
    #endregion
    public GameObject ammoIconPrefab;
    #region Tooltip
    [Tooltip("分数预制体")]
    #endregion
    public GameObject scorePrefab;

    #region Header CHESTS
    [Space(10)]
    [Header("宝箱")]
    #endregion
    #region Tooltip
    [Tooltip("宝箱物品预制体")]
    public GameObject chestItemPrefab;
    #region Tooltip
    #endregion
    [Tooltip("填充血条sprite")]
    public Sprite heartIcon;
    #region Tooltip
    #endregion
    [Tooltip("填充子弹sprite")]
    public Sprite bulletIcon;
    #endregion

    #region Header MINIMAP
    [Space(10)]
    [Header("小地图")]
    #endregion
    #region Tooltip
    [Tooltip("小地图Boss预制体")]
    #endregion
    public GameObject minimapBossPrefab;

    #region Validation
#if UNITY_EDITOR
    //验证输入的可脚本化对象详情
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(roomNodeTypeList), roomNodeTypeList);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerSelectionPrefab), playerSelectionPrefab);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(playerDetailsList), playerDetailsList);
        HelperUtilities.ValidateCheckNullValue(this, nameof(currentPlayer), currentPlayer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(mainMenuMusic), mainMenuMusic);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundsMasterMixerGroup), soundsMasterMixerGroup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorOpenCloseSoundEffect), doorOpenCloseSoundEffect);
        HelperUtilities.ValidateCheckNullValue(this, nameof(chestOpen), chestOpen);
        HelperUtilities.ValidateCheckNullValue(this, nameof(healthPickup), healthPickup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoPickup), ammoPickup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponPickup), weaponPickup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(litMaterial), litMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(dimmedMaterial), dimmedMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(variableLitShader), variableLitShader);
        HelperUtilities.ValidateCheckNullValue(this, nameof(materializeShader), materializeShader);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(enemyUnwalkableCollisionTilesArray), enemyUnwalkableCollisionTilesArray);
        HelperUtilities.ValidateCheckNullValue(this, nameof(preferredEnemyPathTile), preferredEnemyPathTile);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicMasterMixerGroup), musicMasterMixerGroup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicOnFullSnapshot), musicOnFullSnapshot);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicLowSnapshot), musicLowSnapshot);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicOffSnapshot), musicOffSnapshot);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoIconPrefab), ammoIconPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(chestItemPrefab), chestItemPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(heartIcon), heartIcon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(bulletIcon), bulletIcon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(scorePrefab), scorePrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(minimapBossPrefab), minimapBossPrefab);
    }
#endif
    #endregion
}
