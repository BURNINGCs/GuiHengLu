using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDetails_", menuName = "Scriptable Objects/Enemy/EnemyDetails")]
public class EnemyDetailsSO : ScriptableObject
{
    #region Header BASE ENEMY DETAILS
    [Space(10)]
    [Header("敌人基础详情")]
    #endregion

    #region Tooltip
    [Tooltip("敌人名称")]
    #endregion
    public string enemyName;

    #region Tooltip
    [Tooltip("敌人预制体")]
    #endregion
    public GameObject enemyPrefab;

    #region Tooltip
    [Tooltip("敌人开始追击玩家的触发距离")]
    #endregion
    public float chaseDistance = 50f;

    #region Header ENEMY MATERIAL
    [Space(10)]
    [Header("敌人材质")]
    #endregion
    #region Tooltip
    [Tooltip("敌人使用的标准光照着色器材质（在敌人显形完成后启用）")]
    #endregion
    public Material enemyStandardMaterial;

    #region Header ENEMY MATERIALIZE SETTINGS
    [Space(10)]
    [Header("敌人显形设置")]
    #endregion
    #region Tooltip
    [Tooltip("敌人完成显形所需的时长（单位：秒）")]
    #endregion
    public float enemyMaterializeTime;
    #region Tooltip
    [Tooltip("敌人显形过程中使用的着色器")]
    #endregion
    public Shader enemyMaterializeShader;
    [ColorUsage(true, true)]
    #region Tooltip
    [Tooltip("敌人显形时的颜色 —— 该颜色为高动态范围（HDR）颜色，可通过调整亮度参数实现发光 / 泛光效果")]
    #endregion
    public Color enemyMaterializeColor;

    #region Header ENEMY WEAPON SETTINGS
    [Space(10)]
    [Header("敌人武器设置")]
    #endregion
    #region Tooltip
    [Tooltip("敌人配备的武器 —— 若敌人无武器则设为无")]
    #endregion
    public WeaponDetailsSO enemyWeapon;
    #region Tooltip
    [Tooltip("敌人每次射击连射间隔的最小延迟时间（单位：秒），该值需大于 0，实际延迟时间为最小值与最大值之间的随机数")]
    #endregion
    public float firingIntervalMin = 0.1f;
    #region Tooltip
    [Tooltip("敌人每次射击连射间隔的最大延迟时间（单位：秒），实际延迟时间为最小值与最大值之间的随机数")]
    #endregion
    public float firingIntervalMax = 1f;
    #region Tooltip
    [Tooltip("敌人单次连射的最短持续时长（单位：秒），该值需大于 0，实际持续时长为最小值与最大值之间的随机数")]
    #endregion
    public float firingDurationMin = 1f;
    #region Tooltip
    [Tooltip("敌人单次连射的最长持续时长（单位：秒），实际持续时长为最小值与最大值之间的随机数")]
    #endregion
    public float firingDurationMax = 2f;
    #region Tooltip
    [Tooltip("勾选此选项，则敌人开火前需与玩家保持无遮挡的视线；若未勾选，则只要玩家处于射程范围内，敌人无视障碍物都会开火")]
    #endregion
    public bool firingLineOfSightRequired;

    #region Header ENEMY HEALTH
    [Space(10)]
    [Header("敌人生命值")]
    #endregion
    #region Tooltip
    [Tooltip("各等级敌人对应的生命值")]
    #endregion
    public EnemyHealthDetails[] enemyHealthDetailsArray;
    #region Tooltip
    [Tooltip("勾选此项，可设置敌人在被击中后立即进入无敌时间；若勾选，请在另一字段中指定无敌时间（单位：秒）")]
    #endregion
    public bool isImmuneAfterHit = false;
    #region Tooltip
    [Tooltip("敌人被击中后的无敌时间（单位：秒）")]
    #endregion
    public float hitImmunityTime;
    #region Tooltip
    [Tooltip("Select to display a health bar for the enemy")]
    #endregion
    public bool isHealthBarDisplayed = false;

    // 在EnemyDetailsSO.cs中
    [System.Serializable]
    public class ElementProperty
    {
        public ElementType mainElement; // 敌人的主要元素属性
        public float fireResistance = 0f;     // 火抗性
        public float waterResistance = 0f;    // 水抗性
        public float woodResistance = 0f;     // 木抗性
        public float metalResistance = 0f;    // 金抗性
    }

    public ElementProperty elementProperty;
    #region Validation
#if UNITY_EDITOR
    //验证已输入的可脚本化对象详情
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(enemyName), enemyName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyPrefab), enemyPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(chaseDistance), chaseDistance, false);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyStandardMaterial), enemyStandardMaterial);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(enemyMaterializeTime), enemyMaterializeTime, true);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyMaterializeShader), enemyMaterializeShader);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(firingIntervalMin), firingIntervalMin, nameof(firingIntervalMax), firingIntervalMax, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(firingDurationMin), firingDurationMin, nameof(firingDurationMax), firingDurationMax, false);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(enemyHealthDetailsArray), enemyHealthDetailsArray);
        if (isImmuneAfterHit)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(hitImmunityTime), hitImmunityTime, false);
        }
    }
#endif
    #endregion
}
