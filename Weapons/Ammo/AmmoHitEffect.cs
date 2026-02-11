using UnityEngine;

[DisallowMultipleComponent]
public class AmmoHitEffect : MonoBehaviour
{
    private ParticleSystem ammoHitEffectParticleSystem;

    private void Awake()
    {
        ammoHitEffectParticleSystem = GetComponent<ParticleSystem>();
    }

    //根据传入的弹药命中效果可脚本化对象详情设置弹药命中效果
    public void SetHitEffect(AmmoHitEffectSO ammoHitEffect)
    {
        //设置命中效果的颜色渐变
        SetHitEffectColorGradient(ammoHitEffect.colorGradient);

        //设置命中效果粒子系统的初始参数
        SetHitEffectParticleStartingValues(ammoHitEffect.duration, ammoHitEffect.startParticleSize,
        ammoHitEffect.startParticleSpeed, ammoHitEffect.startLifetime, ammoHitEffect.effectGravity,
        ammoHitEffect.maxParticleNumber);

        //设置命中效果粒子系统的粒子爆发数量
        SetHitEffectParticleEmission(ammoHitEffect.emissionRate, ammoHitEffect.burstParticleNumber);

        //设置命中效果的粒子精灵图
        SetHitEffectParticleSprite(ammoHitEffect.sprite);

        //设置命中效果的生命周期最小与最大速度
        SetHitEffectVelocityOverLifeTime(ammoHitEffect.velocityOverLifetimeMin, ammoHitEffect.velocityOverLifetimeMax);
    }

    //设置命中效果粒子系统的颜色渐变
    private void SetHitEffectColorGradient(Gradient gradient)
    {
        //设置颜色渐变
        ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = ammoHitEffectParticleSystem.colorOverLifetime;
        colorOverLifetimeModule.color = gradient;
    }

    //设置命中效果粒子系统的初始参数
    private void SetHitEffectParticleStartingValues(float duration, float startParticleSize, float startParticleSpeed,
        float startLifetime, float effectGravity, int maxParticles)
    {
        ParticleSystem.MainModule mainModule = ammoHitEffectParticleSystem.main;

        //设置粒子系统的持续时间
        mainModule.duration = duration;

        //设置粒子初始尺寸
        mainModule.startSize = startParticleSize;

        //设置粒子初始速度
        mainModule.startSpeed = startParticleSpeed;

        //设置粒子初始生命周期
        mainModule.startLifetime = startLifetime;

        //设置粒子初始重力
        mainModule.gravityModifier = effectGravity;

        //设置最大粒子数量
        mainModule.maxParticles = maxParticles;
    }

    //设置命中效果粒子系统的粒子爆发数量
    private void SetHitEffectParticleEmission(int emissionRate, float burstParticleNumber)
    {
        ParticleSystem.EmissionModule emissionModule = ammoHitEffectParticleSystem.emission;

        //设置粒子爆发数量
        ParticleSystem.Burst burst = new ParticleSystem.Burst(0f, burstParticleNumber);
        emissionModule.SetBurst(0, burst);

        //设置粒子发射速率
        emissionModule.rateOverTime = emissionRate;
    }

    //设置命中效果粒子系统的精灵图
    private void SetHitEffectParticleSprite(Sprite sprite)
    {
        //设置粒子爆发数量
        ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = ammoHitEffectParticleSystem.textureSheetAnimation;

        textureSheetAnimationModule.SetSprite(0, sprite);
    }

    //设置命中效果粒子的生命周期速度
    private void SetHitEffectVelocityOverLifeTime(Vector3 minVelocity, Vector3 maxVelocity)
    {
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = ammoHitEffectParticleSystem.velocityOverLifetime;

        //定义 X 轴速度的最小值与最大值
        ParticleSystem.MinMaxCurve minMaxCurveX = new ParticleSystem.MinMaxCurve();
        minMaxCurveX.mode = ParticleSystemCurveMode.TwoConstants;
        minMaxCurveX.constantMin = minVelocity.x;
        minMaxCurveX.constantMax = maxVelocity.x;
        velocityOverLifetimeModule.x = minMaxCurveX;

        //定义 Y 轴速度的最小值与最大值
        ParticleSystem.MinMaxCurve minMaxCurveY = new ParticleSystem.MinMaxCurve();
        minMaxCurveY.mode = ParticleSystemCurveMode.TwoConstants;
        minMaxCurveY.constantMin = minVelocity.y;
        minMaxCurveY.constantMax = maxVelocity.y;
        velocityOverLifetimeModule.y = minMaxCurveY;

        //定义 Z 轴速度的最小值与最大值
        ParticleSystem.MinMaxCurve minMaxCurveZ = new ParticleSystem.MinMaxCurve();
        minMaxCurveZ.mode = ParticleSystemCurveMode.TwoConstants;
        minMaxCurveZ.constantMin = minVelocity.z;
        minMaxCurveZ.constantMax = maxVelocity.z;
        velocityOverLifetimeModule.z = minMaxCurveZ;
    }

}
