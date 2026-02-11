using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "AmmoDetails_", menuName = "Scriptable Objects/Weapons/Ammo Details")] 
public class AmmoDetailsSO : ScriptableObject
{
    #region Header BASIC AMMO DETAILS
    [Space(10)]
    [Header("弹药基础详情")]
    #endregion
    #region Tooltip
    [Tooltip("弹药名称")]
    #endregion
    public string ammoName;
    public bool isPlayerAmmo;

    #region Header AMMO SPRITE, PREFAB & MATERIALS
    [Space(10)]
    [Header("弹药精灵图、预制体及材质")]
    #endregion
    #region Tooltip
    [Tooltip("弹药使用的精灵图")]
    #endregion
    public Sprite ammoSprite;
    #region Tooltip
    [Tooltip("用弹药使用的预制体填充此处。若指定多个预制体，则会从数组中随机选择一个预制体。该预制体可以是弹药模式 —— 只要它符合 IFireable 接口规范。")]
    #endregion
    public GameObject[] ammoPrefabArray;
    #region Tooltip
    [Tooltip("弹药使用的材质")]
    #endregion
    public Material ammoMaterial;
    #region Tooltip
    [Tooltip("若弹药需在移动前短暂 “充能”，则设置开火后释放前弹药保持充能状态的时间（秒）")]
    #endregion
    public float ammoChargeTime = 0.1f;
    #region Tooltip
    [Tooltip("若弹药有充能时间，则指定充能期间渲染弹药所用的材质")]
    #endregion
    public Material ammoChargeMaterial;

    #region Header AMMO HIT EFFECT
    [Space(10)]
    [Header("弹药命中效果")]
    #endregion
    #region Tooltip
    [Tooltip("定义命中效果预制体参数的可脚本化对象")]
    #endregion
    public AmmoHitEffectSO ammoHitEffect;

    #region Header AMMO BASE PARAMETERS
    [Space(10)]
    [Header("弹药基础参数")]
    #endregion
    #region Tooltip
    [Tooltip("每发弹药造成的伤害")]
    #endregion
    public int ammoDamage = 1;
    #region Tooltip
    [Tooltip("弹药的最小速度 —— 速度将是最小值与最大值之间的随机值")]
    #endregion
    public float ammoSpeedMin = 20f;
    #region Tooltip
    [Tooltip("弹药的最大速度 —— 速度将是最小值与最大值之间的随机值")]
    #endregion
    public float ammoSpeedMax = 20f;
    #region Tooltip
    [Tooltip("弹药（或弹药模式）的射程（单位：Unity 单位）")]
    #endregion
    public float ammoRange = 20f;
    #region Tooltip
    [Tooltip("弹药模式的旋转速度（单位：度 / 秒）")]
    #endregion
    public float ammoRotationSpeed = 1f;

    #region Header AMMO SPREAD DETAILS
    [Space(10)]
    [Header("弹药散布详情")]
    #endregion
    #region Tooltip
    [Tooltip("弹药的最小散布角度。散布值越高，精准度越低。散布角度会在最小值与最大值之间随机计算。")]
    #endregion
    public float ammoSpreadMin = 0f;
    #region Tooltip
    [Tooltip("弹药的最大散布角度。散布值越高，精准度越低。散布角度会在最小值与最大值之间随机计算。")]
    #endregion
    public float ammoSpreadMax = 0f;

    #region Header AMMO SPAWN DETAILS
    [Space(10)]
    [Header("弹药生成详情")]
    #endregion
    #region Tooltip
    [Tooltip("每次射击生成的最小弹药数量。生成的弹药数量会在最小值与最大值之间随机。")]
    #endregion
    public int ammoSpawnAmountMin = 1;
    #region Tooltip
    [Tooltip("每次射击生成的最大弹药数量。生成的弹药数量会在最小值与最大值之间随机。")]
    #endregion
    public int ammoSpawnAmountMax = 1;
    #region Tooltip
    [Tooltip("最小生成间隔时间。生成弹药的时间间隔（秒）为指定的最小值与最大值之间的随机值。")]
    #endregion
    public float ammoSpawnIntervalMin = 0f;
    #region Tooltip
    [Tooltip("最大生成间隔时间。生成弹药的时间间隔（秒）为指定的最小值与最大值之间的随机值。")]
    #endregion
    public float ammoSpawnIntervalMax = 0f;

    #region Header AMMO TRAIL DETAILS
    [Space(10)]
    [Header("弹药拖尾详情")]
    #endregion
    #region Tooltip
    [Tooltip("若需要弹药拖尾则勾选，否则取消勾选。若勾选，则应填充其余弹药拖尾参数。")]
    #endregion
    public bool isAmmoTrail = false;
    #region Tooltip
    [Tooltip("弹药拖尾的生命周期（秒）。")]
    #endregion
    public float ammoTrailTime = 3f;
    #region Tooltip
    [Tooltip("弹药拖尾材质。")]
    #endregion
    public Material ammoTrailMaterial;
    #region Tooltip
    [Tooltip("弹药拖尾的起始宽度。")]
    #endregion
    [Range(0f, 1f)] public float ammoTrailStartWidth;
    #region Tooltip
    [Tooltip("弹药拖尾的结束宽度。")]
    #endregion
    [Range(0f, 1f)] public float ammoTrailEndWidth;

    [Header("技能设置")]
    public bool canCastSkill = false;                  // 是否可以释放技能
    [Header("元素属性")]
    public ElementType elementType = ElementType.None;
    public float elementDamageMultiplier = 1f; // 元素伤害倍率

    [Header("技能消耗")]
    public float skillBlueCost = 30f;                  // 技能蓝耗

    #region Validation
#if UNITY_EDITOR
    //验证输入的可脚本化对象详情
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(ammoName), ammoName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoSprite), ammoSprite);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(ammoPrefabArray), ammoPrefabArray);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoMaterial), ammoMaterial);
        if (ammoChargeTime > 0)
            HelperUtilities.ValidateCheckNullValue(this, nameof(ammoChargeMaterial), ammoChargeMaterial);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoDamage), ammoDamage, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpeedMin), ammoSpeedMin, nameof(ammoSpeedMax), ammoSpeedMax, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoRange), ammoRange, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpreadMin), ammoSpreadMin, nameof(ammoSpreadMax), ammoSpreadMax, true);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpawnAmountMin), ammoSpawnAmountMin, nameof(ammoSpawnAmountMax),
        ammoSpawnAmountMax, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpawnIntervalMin), ammoSpawnIntervalMin, nameof(ammoSpawnIntervalMax),
        ammoSpawnIntervalMax, true);
        if (isAmmoTrail)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailTime), ammoTrailTime, false);
            HelperUtilities.ValidateCheckNullValue(this, nameof(ammoTrailMaterial), ammoTrailMaterial);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailStartWidth), ammoTrailStartWidth, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailEndWidth), ammoTrailEndWidth, false);
        }
    }
#endif
    #endregion
}
