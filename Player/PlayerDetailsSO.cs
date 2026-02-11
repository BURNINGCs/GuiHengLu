using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDetails_", menuName = "Scriptable Objects/Player/Player Details")] 
public class PlayerDetailsSO : ScriptableObject
{
    #region Header PLAYER BASE DETAILS
    [Space(10)]
    [Header("玩家基础详情")]
    #endregion
    #region Tooltip
    [Tooltip("玩家角色名称")]
    #endregion
    public string playerCharacterName;

    #region Tooltip
    [Tooltip("玩家的预制游戏对象")]
    #endregion
    public GameObject playerPrefab;

    #region Tooltip
    [Tooltip("玩家运行时动画控制器")]
    #endregion
    public RuntimeAnimatorController runtimeAnimatorController;

    #region Header HEALTH
    [Space(10)]
    [Header("生命值")]
    #endregion
    #region Tooltip
    [Tooltip("玩家初始生命值")]
    #endregion
    public int playerHealthAmount;
    #region Tooltip
    [Tooltip("Select if has immunity period immediately after being hit. If so specify the immunity time in seconds in the other field")]
    #endregion
    public bool isImmuneAfterHit = false;
    #region Tooltip
    [Tooltip("Immunity time in seconds after being hit")]
    #endregion
    public float hitImmunityTime;

    #region Header WEAPON
    [Space(10)]
    [Header("武器")]
    #endregion
    #region Tooltip
    [Tooltip("玩家初始武器")]
    #endregion
    public WeaponDetailsSO startingWeapon;
    #region Tooltip
    [Tooltip("用初始武器列表填充此处")]
    #endregion
    public List<WeaponDetailsSO> startingWeaponList;

    #region Header OTHER
    [Space(10)]
    [Header("其他")]
    #endregion
    #region Tooltip
    [Tooltip("用于小地图的玩家图标精灵")]
    #endregion
    public Sprite playerMiniMapIcon;
    #region Tooltip
    [Tooltip("用于玩家图标精灵")]
    #endregion
    public Sprite playerHeadIcon;

    #region Tooltip
    [Tooltip("玩家手部精灵")]
    #endregion
    public Sprite playerHandSprite;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(playerCharacterName), playerCharacterName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerPrefab), playerPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(playerHealthAmount), playerHealthAmount, false);
        HelperUtilities.ValidateCheckNullValue(this, nameof(startingWeapon), startingWeapon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerMiniMapIcon), playerMiniMapIcon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerHandSprite), playerHandSprite);
        HelperUtilities.ValidateCheckNullValue(this, nameof(runtimeAnimatorController), runtimeAnimatorController);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(startingWeaponList), startingWeaponList);

        if (isImmuneAfterHit)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(hitImmunityTime), hitImmunityTime, false);
        }
    }
#endif
    #endregion
}
