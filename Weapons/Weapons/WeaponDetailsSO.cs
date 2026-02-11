using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDetails_", menuName = "Scriptable Objects/Weapons/Weapon Details")] 
public class WeaponDetailsSO : ScriptableObject
{
    #region Header WEAPON BASE DETAILS
    [Space(10)]
    [Header("武器基础详情")]
    #endregion Header WEAPON BASE DETAILS
    #region Tooltip
    [Tooltip("武器名称")]
    #endregion Tooltip
    public string weaponName;
    #region Tooltip
    [Tooltip("武器精灵图 —— 该精灵图应勾选 “生成物理形状” 选项")]
    #endregion Tooltip
    public Sprite weaponSprite;

    #region Header WEAPON CONFIGURATION
    [Space(10)]
    [Header("武器配置")]
    #endregion Header WEAPON CONFIGURATION
    #region Tooltip
    [Tooltip("武器射击位置 —— 武器末端相对于精灵轴心点的偏移位置")]
    #endregion Tooltip
    public Vector3 weaponShootPosition;
    #region Tooltip
    [Tooltip("武器当前弹药量")]
    #endregion Tooltip
    public AmmoDetailsSO weaponCurrentAmmo;
    #region Tooltip
    [Tooltip("武器射击特效可脚本化对象 —— 包含需与武器射击特效预制体配合使用的粒子特效参数")]
    #endregion Tooltip
    public WeaponShootEffectSO weaponShootEffect;

    #region Tooltip
    [Tooltip("武器的开火音效可脚本化对象")]
    #endregion Tooltip
    public SoundEffectSO weaponFiringSoundEffect;
    #region Tooltip
    [Tooltip("武器的装填音效可脚本化对象")]
    #endregion Tooltip
    public SoundEffectSO weaponReloadingSoundEffect;

    #region Header WEAPON OPERATING VALUES
    [Space(10)]
    [Header("武器操作参数")]
    #endregion Header WEAPON OPERATING VALUES
    #region Tooltip
    [Tooltip("选择武器是否拥有无限弹药")]
    #endregion Tooltip
    public bool hasInfiniteAmmo = false;
    #region Tooltip
    [Tooltip("选择武器是否拥有无限弹夹容量")]
    #endregion Tooltip
    public bool hasInfiniteClipCapacity = false;
    #region Tooltip
    [Tooltip("武器弹夹容量 —— 重新装填前可射击的次数")]
    #endregion Tooltip
    public int weaponClipAmmoCapacity = 6;
    #region Tooltip
    [Tooltip("武器弹药总容量 —— 该武器可携带的最大弹药数量")]
    #endregion Tooltip
    public int weaponAmmoCapacity = 100;
    #region Tooltip
    [Tooltip("武器射速 ——0.2 意味着每秒 5 发")]
    #endregion Tooltip
    public float weaponFireRate = 0.2f;
    #region Tooltip
    [Tooltip("武器预充能时间 —— 按住开火按钮到开火前的秒数")]
    #endregion Tooltip
    public float weaponPrechargeTime = 0f;
    #region Tooltip
    [Tooltip("武器装填时间（秒）")]
    #endregion Tooltip
    public float weaponReloadTime = 0f;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(weaponName), weaponName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponCurrentAmmo), weaponCurrentAmmo);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponFireRate), weaponFireRate, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponPrechargeTime), weaponPrechargeTime, true);

        if (!hasInfiniteAmmo)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponAmmoCapacity), weaponAmmoCapacity, false);
        }

        if (!hasInfiniteClipCapacity)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponClipAmmoCapacity), weaponClipAmmoCapacity, false);
        }
    }

#endif
    #endregion Validation
}
