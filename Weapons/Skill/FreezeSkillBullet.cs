// FreezeSkillBullet.cs
using UnityEngine;
using System.Collections;

public class FreezeSkillBullet : MonoBehaviour
{
    [Header("子弹设置")]
    public float bulletSpeed = 15f;
    public float bulletLifeTime = 3f;
    public int damage = 10;

    [Header("冰冻效果")]
    public float freezeRadius = 3f;
    public float freezeDuration = 3f;
    public float slowPercentage = 0.5f;  // 减速百分比（50%速度）

    [Header("特效预制体")]
    public GameObject freezeAreaEffectPrefab;  // 冰冻区域特效预制体
    public GameObject hitEffectPrefab;         // 命中特效

    private Vector3 shootDirection;

    public void Initialize(Vector3 startPos, Vector3 direction)
    {
        transform.position = startPos;
        shootDirection = direction.normalized;

        // 设置旋转朝向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, bulletLifeTime);
    }

    void Update()
    {
        transform.position += shootDirection * bulletSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 跳过自己
        if (collision.gameObject == gameObject) return;

        // 检查是否碰到敌人或墙壁
        bool hitEnemy = collision.GetComponent<Enemy>() != null;
        bool hitWall = collision.CompareTag("collisionTilemap") ||
                      collision.CompareTag("Door");

        if (hitEnemy || hitWall)
        {
            if (hitEnemy)
            {
                Enemy enemy = collision.GetComponent<Enemy>();
                enemy.health.TakeDamage(damage);
            }

            // 创建冰冻区域
            if (freezeAreaEffectPrefab != null)
            {
                GameObject effect = Instantiate(freezeAreaEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, freezeDuration);
            }

            // 创建命中特效
            if (hitEffectPrefab != null)
            {
                GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(hitEffect, 1f);
            }

            // 查找并冰冻范围内的敌人
            ApplyFreezeToNearbyEnemies();

            Destroy(gameObject);
        }
    }

    private void ApplyFreezeToNearbyEnemies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, freezeRadius);

        foreach (var collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && enemy.gameObject != gameObject)
            {
                // 调用冰冻效果方法
                ApplyElementEffect(enemy, ElementType.Water);
                
                // 应用减速效果
                ApplySlowEffect(enemy);
            }
        }
    }

    private void ApplyElementEffect(Enemy enemy, ElementType elementType)
    {
        if (elementType == ElementType.Water)
        {
            FrozenEffect existing = enemy.GetComponent<FrozenEffect>();
            if (existing != null)
            {
                existing.currentStacks = Mathf.Min(existing.currentStacks + 1, 3);
                existing.duration = Mathf.Max(existing.duration, freezeDuration);
            }
            else
            {
                FrozenEffect effect = enemy.gameObject.AddComponent<FrozenEffect>();
                effect.Initialize(enemy, freezeDuration, 1);
            }
        }
    }

    private void ApplySlowEffect(Enemy enemy)
    {
        EnemyMovementAI movementAI = enemy.GetComponent<EnemyMovementAI>();
        if (movementAI != null)
        {
            // 保存原始速度
            float originalSpeed = movementAI.moveSpeed;
            
            // 应用减速
            movementAI.moveSpeed = originalSpeed * (1f - slowPercentage);
            
            // 启动协程恢复速度
            StartCoroutine(RestoreSpeedAfterDelay(enemy, movementAI, originalSpeed, freezeDuration));
        }
    }

    private IEnumerator RestoreSpeedAfterDelay(Enemy enemy, EnemyMovementAI movementAI, float originalSpeed, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 检查敌人是否还存在
        if (enemy != null && movementAI != null)
        {
            // 恢复原始速度
            movementAI.moveSpeed = originalSpeed;
        }
    }

    // 绘制范围（调试用）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, freezeRadius);
    }
}