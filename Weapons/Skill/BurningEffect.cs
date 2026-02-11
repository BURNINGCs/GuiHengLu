using UnityEngine;

public class BurningEffect : StatusEffectBase
{
    [Header("灼烧设置")]
    public int damagePerTick = 2;           // 每次触发伤害
    public float spreadChance = 0.1f;       // 传播给附近敌人的概率

    private float originalMoveSpeed = 0f;
    private bool hasSlowed = false;

    protected override void StartEffect()
    {
        base.StartEffect();

        // 设置灼烧颜色
        effectColor = new Color(1f, 0.3f, 0f, 1f); // 橙红色
        if (effectMaterial != null)
            effectMaterial.color = effectColor;

        // 轻微减速
        EnemyMovementAI movementAI = targetEnemy.GetComponent<EnemyMovementAI>();
        if (movementAI != null && !hasSlowed)
        {
            originalMoveSpeed = movementAI.moveSpeed;
            movementAI.moveSpeed *= 0.9f; // 轻微减速10%
            hasSlowed = true;
        }

        // 随机传播给附近敌人
        TrySpreadEffect();
    }

    protected override void OnTick()
    {
        if (targetEnemy == null || targetEnemy.health == null) return;

        // 造成伤害
        int totalDamage = damagePerTick * currentStacks;
        targetEnemy.health.TakeDamage(totalDamage);

        // 播放伤害数字（可选）
        ShowDamageNumber(totalDamage);

        // 概率点燃周围敌人
        if (Random.value < spreadChance * currentStacks)
        {
            SpreadToNearbyEnemies();
        }
    }

    protected override void EndEffect()
    {
        // 恢复移动速度
        if (hasSlowed && targetEnemy != null)
        {
            EnemyMovementAI movementAI = targetEnemy.GetComponent<EnemyMovementAI>();
            if (movementAI != null)
            {
                movementAI.moveSpeed = originalMoveSpeed;
            }
        }

        base.EndEffect();
    }

    private void TrySpreadEffect()
    {
        if (Random.value < 0.3f) // 30%概率初始就传播
        {
            SpreadToNearbyEnemies();
        }
    }

    private void SpreadToNearbyEnemies()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(
            transform.position,
            3f,  // 3米范围
            LayerMask.GetMask("Enemy")
        );

        foreach (var collider in nearbyColliders)
        {
            if (collider.gameObject != targetEnemy.gameObject)
            {
                Enemy nearbyEnemy = collider.GetComponent<Enemy>();
                if (nearbyEnemy != null)
                {
                    // 检查是否已有灼烧效果
                    BurningEffect existing = nearbyEnemy.GetComponent<BurningEffect>();
                    if (existing != null)
                    {
                        // 增加层数（最多5层）
                        existing.currentStacks = Mathf.Min(existing.currentStacks + 1, 5);
                        existing.duration = Mathf.Max(existing.duration, duration); // 刷新持续时间
                    }
                    else
                    {
                        // 添加新效果
                        BurningEffect newEffect = nearbyEnemy.gameObject.AddComponent<BurningEffect>();
                        newEffect.Initialize(nearbyEnemy, duration * 0.7f, 1); // 持续70%时间
                    }
                }
            }
        }
    }

    private void ShowDamageNumber(int damage)
    {
        // 简单实现：在敌人上方显示伤害数字
        Vector3 position = targetEnemy.transform.position + Vector3.up * 0.5f;

        // 如果有对象池，可以用对象池生成文字
        // 这里简单用Debug.Log替代
        Debug.Log($"灼烧伤害: {damage}");
    }
}