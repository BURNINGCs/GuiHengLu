using UnityEngine;

public class FrozenEffect : StatusEffectBase
{
    [Header("冰冻设置")]
    public float speedMultiplier = 0.5f;    // 移动速度乘数
    public float freezeChance = 0.2f;       // 完全冻结概率
    public bool isFrozen = false;           // 是否完全冻结

    private float originalMoveSpeed = 0f;
    private float originalTurnSpeed = 0f;
    private EnemyMovementAI movementAI;
    private EnemyWeaponAI weaponAI;

    protected override void StartEffect()
    {
        base.StartEffect();

        // 设置冰冻颜色
        effectColor = new Color(0.5f, 0.8f, 1f, 0.8f); // 淡蓝色
        if (effectMaterial != null)
            effectMaterial.color = effectColor;

        // 获取组件
        movementAI = targetEnemy.GetComponent<EnemyMovementAI>();
        weaponAI = targetEnemy.GetComponent<EnemyWeaponAI>();

        if (movementAI != null)
        {
            originalMoveSpeed = movementAI.moveSpeed;

            // 概率完全冻结
            if (Random.value < freezeChance * currentStacks)
            {
                isFrozen = true;
                movementAI.moveSpeed = 0f; // 完全停止
                movementAI.enabled = false; // 禁用AI

                // 添加冰冻粒子特效
                AddIceBlockEffect();
            }
            else
            {
                // 减速
                movementAI.moveSpeed *= speedMultiplier;
            }
        }

        // 降低攻击速度
        if (weaponAI != null)
        {
            weaponAI.enabled = !isFrozen; // 如果冻结则完全禁用武器AI
        }
    }

    protected override void OnTick()
    {
        // 冰冻效果不需要每帧触发伤害，只需持续效果
        // 可以添加一些视觉反馈
        if (isFrozen && Random.value < 0.3f)
        {
            CreateIceParticles();
        }

        // 检查附近有火属性攻击时，提前解冻
        CheckForFireNearby();
    }

    protected override void EndEffect()
    {
        // 恢复移动速度
        if (movementAI != null)
        {
            if (isFrozen)
            {
                movementAI.enabled = true;
                movementAI.moveSpeed = originalMoveSpeed;
            }
            else
            {
                movementAI.moveSpeed = originalMoveSpeed;
            }
        }

        // 恢复武器AI
        if (weaponAI != null)
        {
            weaponAI.enabled = true;
        }

        // 移除冰封特效
        RemoveIceBlockEffect();

        base.EndEffect();
    }

    private void AddIceBlockEffect()
    {
        // 创建冰封特效（可以是一个子对象）
        GameObject iceBlock = new GameObject("IceBlock");
        iceBlock.transform.SetParent(targetEnemy.transform);
        iceBlock.transform.localPosition = Vector3.zero;
        iceBlock.transform.localScale = Vector3.one * 1.1f; // 稍微比敌人大一点

        SpriteRenderer iceRenderer = iceBlock.AddComponent<SpriteRenderer>();
        iceRenderer.sprite = targetEnemy.spriteRendererArray[0].sprite; // 复制敌人精灵
        iceRenderer.color = new Color(0.5f, 0.8f, 1f, 0.5f);
        iceRenderer.sortingOrder = targetEnemy.spriteRendererArray[0].sortingOrder - 1;

        // 标记为冰封特效，便于移除
        iceBlock.tag = "Effect";
    }

    private void RemoveIceBlockEffect()
    {
        // 查找并移除冰封特效
        Transform iceBlock = targetEnemy.transform.Find("IceBlock");
        if (iceBlock != null)
        {
            Destroy(iceBlock.gameObject);
        }
    }

    private void CreateIceParticles()
    {
        // 创建冰晶粒子
        if (effectParticles == null)
        {
            GameObject particles = new GameObject("IceParticles");
            particles.transform.SetParent(targetEnemy.transform);
            particles.transform.localPosition = Vector3.zero;

            ParticleSystem ps = particles.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSize = 0.1f;
            main.startColor = new Color(0.5f, 0.8f, 1f, 0.5f);
            main.startLifetime = 0.5f;

            effectParticles = ps;
        }

        effectParticles.Emit(5); // 发射5个粒子
    }

    private void CheckForFireNearby()
    {
        // 如果附近有火属性攻击，提前解冻
        Collider2D[] fireAttacks = Physics2D.OverlapCircleAll(
            transform.position,
            2f,
            LayerMask.GetMask("Ammo") // 假设子弹在"Ammo"层
        );

        foreach (var attack in fireAttacks)
        {
            Ammo ammo = attack.GetComponent<Ammo>();
            if (ammo != null && ammo.ammoDetails != null &&
                ammo.ammoDetails.elementType == ElementType.Fire)
            {
                // 立即解冻，并造成额外伤害
                duration = 0.1f; // 几乎立即结束
                if (targetEnemy.health != null)
                {
                    targetEnemy.health.TakeDamage(5); // 额外伤害
                }
                break;
            }
        }
    }

    // 外部调用：打破冰冻
    public void BreakFreeze()
    {
        duration = 0.1f; // 立即结束
    }
}