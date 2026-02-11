using UnityEngine;

[CreateAssetMenu(fileName = "AmmoHitEffect_", menuName = "Scriptable Objects/Weapons/Ammo Hit Effect")]
public class AmmoHitEffectSO : ScriptableObject
{
    #region Header AMMO HIT EFFECT DETAILS
    [Space(10)]
    [Header("弹药命中效果详情")]
    #endregion Header AMMO HIT EFFECT DETAILS

    #region Tooltip
    [Tooltip("命中效果的颜色渐变。该渐变用于定义粒子在其生命周期内的颜色变化 —— 从左至右对应粒子从生成到消失的颜色过渡")]
    #endregion Tooltip
    public Gradient colorGradient;

    #region Tooltip
    [Tooltip("粒子系统的粒子发射时长")]
    #endregion Tooltip
    public float duration = 0.50f;

    #region Tooltip
    [Tooltip("粒子效果的粒子初始尺寸")]
    #endregion Tooltip
    public float startParticleSize = 0.25f;

    #region Tooltip
    [Tooltip("粒子效果的粒子初始速度")]
    #endregion Tooltip
    public float startParticleSpeed = 3f;

    #region Tooltip
    [Tooltip("粒子效果的粒子生命周期")]
    #endregion Tooltip
    public float startLifetime = 0.5f;

    #region Tooltip
    [Tooltip("粒子的最大发射数量")]
    #endregion Tooltip
    public int maxParticleNumber = 100;

    #region Tooltip
    [Tooltip("粒子的每秒发射数量。若该值设为 0，则仅发射爆发数量的粒子")]
    #endregion Tooltip
    public int emissionRate = 100;

    #region Tooltip
    [Tooltip("粒子效果爆发时应发射的粒子数量")]
    #endregion Tooltip
    public int burstParticleNumber = 20;

    #region Tooltip
    [Tooltip("作用于粒子的重力值 —— 设置一个较小的负数可使粒子向上漂浮")]
    #endregion
    public float effectGravity = -0.01f;

    #region Tooltip
    [Tooltip("粒子效果使用的精灵图。若未指定，则使用默认粒子精灵图")]
    #endregion Tooltip
    public Sprite sprite;

    #region Tooltip
    [Tooltip("粒子生命周期内的最小速度。粒子速度会在最小值与最大值之间随机生成")]
    #endregion Tooltip
    public Vector3 velocityOverLifetimeMin;

    #region Tooltip
    [Tooltip("粒子生命周期内的最大速度。粒子速度会在最小值与最大值之间随机生成")]
    #endregion Tooltip
    public Vector3 velocityOverLifetimeMax;

    #region Tooltip
    [Tooltip("包含命中效果粒子系统的预制体 —— 需预先定义对应的弹药命中效果可脚本化对象（ammoHitEffectSO）")]
    #endregion
    public GameObject ammoHitEffectPrefab;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(duration), duration, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(startParticleSize), startParticleSize, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(startParticleSpeed), startParticleSpeed, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(startLifetime), startLifetime, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(maxParticleNumber), maxParticleNumber, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(emissionRate), emissionRate, true);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(burstParticleNumber), burstParticleNumber, true);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoHitEffectPrefab), ammoHitEffectPrefab);
    }
#endif
    #endregion Validation
}
