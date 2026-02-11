using UnityEngine;

[DisallowMultipleComponent]
public class WeaponShootEffect : MonoBehaviour
{
    private ParticleSystem shootEffectParticleSystem;

    private void Awake()
    {
        //加载组件
        shootEffectParticleSystem = GetComponent<ParticleSystem>();
    }

    //根据传入的武器射击特效可脚本化对象与瞄准角度设置射击特效
    public void SetShootEffect(WeaponShootEffectSO shootEffect, float aimAngle)
    {
        //设置射击特效的颜色渐变
        SetShootEffectColorGradient(shootEffect.colorGradient);

        //设置射击特效粒子系统的初始参数
        SetShootEffectParticleStartingValues(shootEffect.duration, shootEffect.startParticleSize, shootEffect.startParticleSpeed,
            shootEffect.startLifetime, shootEffect.effectGravity, shootEffect.maxParticleNumber);

        //设置射击特效粒子系统的粒子爆发数量
        SetShootEffectParticleEmission(shootEffect.emissionRate, shootEffect.burstParticleNumber);

        //设置发射器旋转角度
        SetEmmitterRotation(aimAngle);

        //设置射击特效的粒子精灵图
        SetShootEffectParticleSprite(shootEffect.sprite);

        //设置射击特效生命周期内的最小与最大速度
        SetShootEffectVelocityOverLifeTime(shootEffect.velocityOverLifetimeMin, shootEffect.velocityOverLifetimeMax);
    }

    //设置射击特效粒子系统的颜色渐变
    private void SetShootEffectColorGradient(Gradient gradient)
    {
        //设置颜色渐变
        ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = shootEffectParticleSystem.colorOverLifetime;
        colorOverLifetimeModule.color = gradient;
    }

    //设置射击特效粒子系统的初始参数
    private void SetShootEffectParticleStartingValues(float duration, float startParticleSize, float startParticleSpeed,
        float startLifetime, float effectGravity, int maxParticles)
    {
        ParticleSystem.MainModule mainModule = shootEffectParticleSystem.main;

        //设置粒子系统持续时间
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

    //设置射击特效粒子系统的粒子爆发数量
    private void SetShootEffectParticleEmission(int emissionRate, float burstParticleNumber)
    {
        ParticleSystem.EmissionModule emissionModule = shootEffectParticleSystem.emission;

        //设置粒子爆发数量
        ParticleSystem.Burst burst = new ParticleSystem.Burst(0f, burstParticleNumber);
        emissionModule.SetBurst(0, burst);

        //设置粒子发射速率
        emissionModule.rateOverTime = emissionRate;
    }

    //设置射击特效粒子系统的精灵图
    private void SetShootEffectParticleSprite(Sprite sprite)
    {
        //设置粒子爆发数量
        ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = shootEffectParticleSystem.textureSheetAnimation;

        textureSheetAnimationModule.SetSprite(0, sprite);
    }

    //将发射器旋转角度调整至与瞄准角度匹配
    private void SetEmmitterRotation(float aimAngle)
    {
        transform.eulerAngles = new Vector3(0f, 0f, aimAngle);
    }

    //设置射击特效生命周期内的速度
    private void SetShootEffectVelocityOverLifeTime(Vector3 minVelocity, Vector3 maxVelocity)
    {
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = shootEffectParticleSystem.velocityOverLifetime;

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
