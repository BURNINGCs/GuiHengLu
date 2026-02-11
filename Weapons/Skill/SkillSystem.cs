// SkillSystem.cs
using UnityEngine;
using System.Collections;

public class SkillSystem : MonoBehaviour
{
    [Header("组件引用")]
    private BlueBarUI blueBarUI;
    [SerializeField] private ActiveWeapon activeWeapon;
    [SerializeField] private Transform skillShootPoint; // 技能发射点

    [Header("技能设置")]
    public GameObject freezeSkillPrefab;  // 冰冻技能子弹预制体
    public GameObject burnSkillPrefab;    // 灼烧技能子弹预制体

    [Header("技能冷却")]
    public float skillCooldown = 1f;      // 技能冷却时间
    private float currentCooldown = 0f;

    [Header("音效")]
    public AudioClip freezeSkillSound;
    public AudioClip burnSkillSound;

    private void Start()
    {
        // 通过标签或路径查找BlueBarUI
        GameObject blueBarObject = GameObject.FindWithTag("BlueBarUI");
        if (blueBarObject != null)
        {
            blueBarUI = blueBarObject.GetComponent<BlueBarUI>();
        }

        // 或者直接查找类型（性能稍差但简单）
        if (blueBarUI == null)
        {
            blueBarUI = FindObjectOfType<BlueBarUI>();
        }

        if (blueBarUI == null)
        {
            Debug.LogError("找不到BlueBarUI组件！");
        }
    }

    private void Update()
    {
        // 冷却计时
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
        }

        // 检测技能按键
        if (Input.GetKeyDown(KeyCode.Q) && currentCooldown <= 0f)
        {
            TryCastSkill();
        }

        // 显示冷却指示器（可选）
        UpdateSkillIndicator();
    }

    // 尝试释放技能
    private void TryCastSkill()
    {
        // 获取当前武器
        Weapon currentWeapon = activeWeapon.GetCurrentWeapon();
        if (currentWeapon == null || currentWeapon.weaponDetails == null)
            return;

        // 获取当前武器弹药
        AmmoDetailsSO currentAmmo = activeWeapon.GetCurrentAmmo();
        if (currentAmmo == null)
            return;

        // 根据武器元素类型决定技能
        if (currentAmmo.elementType == ElementType.Water)
        {
            CastFreezeSkill();
        }
        else if (currentAmmo.elementType == ElementType.Fire)
        {
            CastBurnSkill();
        }
        else
        {
            Debug.Log("当前武器不支持技能释放");
        }
    }

    // 释放冰冻技能
    private void CastFreezeSkill()
    {
        if (!blueBarUI.ConsumeBlue(blueBarUI.freezeSkillCost))
        {
            Debug.Log("蓝量不足，无法释放冰冻技能");
            return;
        }

        //// 播放音效
        //if (freezeSkillSound != null)
        //{
        //    SoundEffectManager.Instance.PlaySoundEffect(freezeSkillSound);
        //}

        // 创建技能子弹
        if (freezeSkillPrefab != null)
        {
            // 获取鼠标方向
            Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();
            Vector3 shootDirection = (mouseWorldPosition - skillShootPoint.position).normalized;

            // 创建技能子弹
            GameObject skillBullet = Instantiate(freezeSkillPrefab,
                skillShootPoint.position,
                Quaternion.identity);

            // 设置子弹方向和速度
            skillBullet.GetComponent<Rigidbody2D>().velocity = shootDirection * 15f;

            // 设置旋转朝向
            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            skillBullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // 设置技能子弹的属性
            FreezeSkillBullet freezeBullet = skillBullet.GetComponent<FreezeSkillBullet>();
            if (freezeBullet != null)
            {
                freezeBullet.Initialize(skillShootPoint.position, shootDirection);
            }

            // 启动冷却
            currentCooldown = skillCooldown;
        }
    }

    // 释放灼烧技能
    private void CastBurnSkill()
    {
        if (!blueBarUI.ConsumeBlue(blueBarUI.burnSkillCost))
        {
            Debug.Log("蓝量不足，无法释放灼烧技能");
            return;
        }

        //// 播放音效
        //if (burnSkillSound != null)
        //{
        //    SoundEffectManager.Instance.PlaySoundEffect(burnSkillSound);
        //}

        // 创建技能子弹
        if (burnSkillPrefab != null)
        {
            // 获取鼠标方向
            Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();
            Vector3 shootDirection = (mouseWorldPosition - skillShootPoint.position).normalized;

            // 创建技能子弹
            GameObject skillBullet = Instantiate(burnSkillPrefab,
                skillShootPoint.position,
                Quaternion.identity);

            // 设置子弹方向和速度
            skillBullet.GetComponent<Rigidbody2D>().velocity = shootDirection * 12f;

            // 设置旋转朝向
            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            skillBullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // 设置技能子弹的属性
            BurnSkillBullet burnBullet = skillBullet.GetComponent<BurnSkillBullet>();
            if (burnBullet != null)
            {
                burnBullet.Initialize(skillShootPoint.position, shootDirection);
            }

            // 启动冷却
            currentCooldown = skillCooldown;
        }
    }

    // 更新技能指示器（可选）
    private void UpdateSkillIndicator()
    {
        // 可以在UI上显示技能冷却状态
    }

    // 获取技能冷却进度
    public float GetSkillCooldownProgress()
    {
        return Mathf.Clamp01(currentCooldown / skillCooldown);
    }

    // 是否可以释放技能
    public bool CanCastSkill()
    {
        return currentCooldown <= 0f;
    }
}