// BurnSkillBullet.cs
using UnityEngine;

public class BurnSkillBullet : MonoBehaviour
{
    [Header("子弹设置")]
    public float bulletSpeed = 12f;
    public float bulletLifeTime = 2.5f;
    public int damage = 15;

    [Header("灼烧效果")]
    public float burnRadius = 2.5f;
    public float burnDuration = 4f;
    public int burnDamagePerSecond = 3;

    [Header("特效预制体")]
    public GameObject burnAreaEffectPrefab;  // 在Inspector中拖入预制体
    public GameObject hitEffectPrefab;

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

            // 创建灼烧区域
            if (burnAreaEffectPrefab != null)
            {
                GameObject effect = Instantiate(burnAreaEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, burnDuration);
            }

            // 创建命中特效
            if (hitEffectPrefab != null)
            {
                GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(hitEffect, 1f);
            }

            // 查找并灼烧范围内的敌人
            ApplyBurnToNearbyEnemies();

            Destroy(gameObject);
        }
    }

    private void ApplyBurnToNearbyEnemies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, burnRadius);

        foreach (var collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && enemy.gameObject != gameObject)
            {
                // 这里调用你之前实现的ApplyElementEffect方法
                ApplyElementEffect(enemy, ElementType.Fire);
            }
        }
    }

    private void ApplyElementEffect(Enemy enemy, ElementType elementType)
    {
        if (elementType == ElementType.Fire)
        {
            BurningEffect existing = enemy.GetComponent<BurningEffect>();
            if (existing != null)
            {
                existing.currentStacks = Mathf.Min(existing.currentStacks + 1, 5);
                existing.duration = Mathf.Max(existing.duration, burnDuration);
            }
            else
            {
                BurningEffect effect = enemy.gameObject.AddComponent<BurningEffect>();
                effect.Initialize(enemy, burnDuration, 1);
            }
        }
    }
}