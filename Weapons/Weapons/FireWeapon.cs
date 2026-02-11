using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(WeaponFiredEvent))]

[DisallowMultipleComponent]
public class FireWeapon : MonoBehaviour
{
    private float firePreChargeTimer = 0f;
    private float fireRateCoolDownTimer = 0f;
    private ActiveWeapon activeWeapon;
    private FireWeaponEvent fireWeaponEvent;
    private ReloadWeaponEvent reloadWeaponEvent;
    private WeaponFiredEvent weaponFiredEvent;

    private void Awake()
    {
        //加载组件
        activeWeapon = GetComponent<ActiveWeapon>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
    }

    private void OnEnable()
    {
        //订阅武器开火事件
        fireWeaponEvent.OnFireWeapon += FireWeaponEvent_OnFireWeapon;
    }

    private void OnDisable()
    {
        //取消订阅武器开火事件
        fireWeaponEvent.OnFireWeapon -= FireWeaponEvent_OnFireWeapon;
    }

    private void Update()
    {
        //减少冷却计时器
        fireRateCoolDownTimer -= Time.deltaTime;
    }

    //处理武器开火事件
    private void FireWeaponEvent_OnFireWeapon(FireWeaponEvent fireWeaponEvent, FireWeaponEventArgs fireWeaponEventArgs)
    {
        WeaponFire(fireWeaponEventArgs);
    }

    //开火
    private void WeaponFire(FireWeaponEventArgs fireWeaponEventArgs)
    {
        //处理武器预充能计时器
        WeaponPreCharge(fireWeaponEventArgs);

        //武器射击
        if (fireWeaponEventArgs.fire)
        {
            //测试武器是否准备好开火
            if (IsWeaponReadyToFire())
            {
                FireAmmo(fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector);

                ResetCoolDownTimer();

                ResetPrechargeTimer();
            }
        }
    }

    //处理武器预充能
    private void WeaponPreCharge(FireWeaponEventArgs fireWeaponEventArgs)
    {
        //武器预充能
        if (fireWeaponEventArgs.firePreviousFrame)
        {
            //若上一帧按住开火按钮，则减少预充能计时器数值
            firePreChargeTimer -= Time.deltaTime;
        }
        else
        {
            //否则重置预充能计时器
            ResetPrechargeTimer();
        }
    }

    //若武器准备好开火则返回 true，否则返回 false
    private bool IsWeaponReadyToFire()
    {
        //若无弹药且武器无无限弹药则返回 false
        if (activeWeapon.GetCurrentWeapon().weaponRemainingAmmo <= 0 && !activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteAmmo)
            return false;

        //若武器正在装填则返回 false
        if (activeWeapon.GetCurrentWeapon().isWeaponReloading)
            return false;

        //若武器未完成预充能或处于冷却中，则返回 false
        if (firePreChargeTimer > 0f || fireRateCoolDownTimer > 0f)
            return false;

        //若弹夹无弹药且武器无无限弹夹容量则返回 false
        if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity && activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo <= 0)
        {
            //触发重新装填武器事件
            reloadWeaponEvent.CallReloadWeaponEvent(activeWeapon.GetCurrentWeapon(), 0);

            return false;
        }
            
        //武器准备好开火 —— 返回 true
        return true;
    }

    //使用对象池中的弹药游戏对象和组件设置弹药
    private void FireAmmo(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        AmmoDetailsSO currentAmmo = activeWeapon.GetCurrentAmmo();

        if (currentAmmo != null)
        {
            //发射弹药协程
            StartCoroutine(FireAmmoRoutine(currentAmmo, aimAngle, weaponAimAngle, weaponAimDirectionVector));
        }
    }

    //若弹药详情中指定了每次射击生成多发弹药，则通过此协程实现该逻辑
    private IEnumerator FireAmmoRoutine(AmmoDetailsSO currentAmmo, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        int ammoCounter = 0;

        //获取每次射击的随机弹药数量
        int ammoPerShot = Random.Range(currentAmmo.ammoSpawnAmountMin, currentAmmo.ammoSpawnAmountMax + 1);

        //获取弹药之间的随机生成间隔
        float ammoSpawnInterval;

        if (ammoPerShot > 1)
        {
            ammoSpawnInterval = Random.Range(currentAmmo.ammoSpawnIntervalMin, currentAmmo.ammoSpawnIntervalMax);
        }
        else
        {
            ammoSpawnInterval = 0f;
        }

        //按每次射击的弹药数量执行循环
        while (ammoCounter < ammoPerShot)
        {
            ammoCounter++;

            //从数组获取弹药预制体
            GameObject ammoPrefab = currentAmmo.ammoPrefabArray[Random.Range(0, currentAmmo.ammoPrefabArray.Length)];

            //获取随机速度值
            float ammoSpeed = Random.Range(currentAmmo.ammoSpeedMin, currentAmmo.ammoSpeedMax);

            //获取带有 IFireable 组件的游戏对象
            IFireable ammo = (IFireable)PoolManager.Instance.ReuseComponent(ammoPrefab, activeWeapon.GetShootPosition(), Quaternion.identity);

            //初始化弹药
            ammo.InitialiseAmmo(currentAmmo, aimAngle, weaponAimAngle, ammoSpeed, weaponAimDirectionVector);

            //等待弹药生成的时间间隔
            yield return new WaitForSeconds(ammoSpawnInterval);
        }       

        //若非无限弹夹容量则减少弹夹弹药数
        if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity)
        {
            activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo--;
            activeWeapon.GetCurrentWeapon().weaponRemainingAmmo--;
        }

        //调用武器开火事件
        weaponFiredEvent.CallWeaponFiredEvent(activeWeapon.GetCurrentWeapon());

        //显示武器射击特效
        WeaponShootEffect(aimAngle);

        //武器开火音效
        WeaponSoundEffect();
    }

    //重置冷却计时器
    private void ResetCoolDownTimer()
    {
        //重置冷却计时器
        fireRateCoolDownTimer = activeWeapon.GetCurrentWeapon().weaponDetails.weaponFireRate;
    }

    //重置预充能计时器
    private void ResetPrechargeTimer()
    {
        //重置预充能计时器
        firePreChargeTimer = activeWeapon.GetCurrentWeapon().weaponDetails.weaponPrechargeTime;
    }

    //展示武器射击特效
    private void WeaponShootEffect(float aimAngle)
    {
        //若存在射击特效及预制体则执行处理逻辑
        if (activeWeapon.GetCurrentWeapon().weaponDetails.weaponShootEffect != null && 
            activeWeapon.GetCurrentWeapon().weaponDetails.weaponShootEffect.weaponShootEffectPrefab != null)
        {
            //从对象池中获取带有粒子系统组件的武器射击特效游戏对象
            WeaponShootEffect weaponShootEffect = (WeaponShootEffect)PoolManager.Instance.ReuseComponent
                (activeWeapon.GetCurrentWeapon().weaponDetails.weaponShootEffect.weaponShootEffectPrefab,
                activeWeapon.GetShootEffectPosition(), Quaternion.identity);

            //设置射击特效
            weaponShootEffect.SetShootEffect(activeWeapon.GetCurrentWeapon().weaponDetails.weaponShootEffect, aimAngle);

            //激活该游戏对象（粒子系统已设置为播放完毕后自动禁用该游戏对象）
            weaponShootEffect.gameObject.SetActive(true);
        }
    }

    //播放武器射击音效
    private void WeaponSoundEffect()
    {
        if (activeWeapon.GetCurrentWeapon().weaponDetails.weaponFiringSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(activeWeapon.GetCurrentWeapon().weaponDetails.weaponFiringSoundEffect);
        }
    }
}
