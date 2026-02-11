// StatusEffectBase.cs
using UnityEngine;

public abstract class StatusEffectBase : MonoBehaviour
{
    [System.Serializable]
    public enum EffectType
    {
        Burning,    // 灼烧 - 持续掉血
        Frozen,     // 冰冻 - 减速+概率冻结
    }

    [Header("基础设置")]
    public EffectType effectType;
    public float duration = 3f;          // 效果持续时间
    public int maxStacks = 1;            // 最大叠加层数
    public int currentStacks = 1;        // 当前层数
    public float tickInterval = 1f;      // 触发间隔（秒）

    [Header("显示效果")]
    public Color effectColor = Color.white;  // 效果颜色（用于材质或粒子）
    public ParticleSystem effectParticles;   // 特效粒子

    protected Enemy targetEnemy;
    protected SpriteRenderer[] enemySprites;
    protected Material originalMaterial;
    protected Material effectMaterial;

    protected float timer = 0f;
    protected float tickTimer = 0f;

    protected bool isActive = false;

    public void Initialize(Enemy enemy, float customDuration = -1f, int stacks = 1)
    {
        this.targetEnemy = enemy;

        if (customDuration > 0)
            duration = customDuration;

        currentStacks = Mathf.Min(stacks, maxStacks);

        // 获取敌人的精灵渲染器
        enemySprites = enemy.GetComponentsInChildren<SpriteRenderer>();
        if (enemySprites.Length > 0)
        {
            originalMaterial = enemySprites[0].material;

            // 创建材质实例（避免修改原始材质）
            effectMaterial = new Material(originalMaterial);
            effectMaterial.color = effectColor;
        }

        // 启动效果
        StartEffect();
        isActive = true;

        // 自动销毁组件
        Invoke("EndEffect", duration);
    }

    protected virtual void StartEffect()
    {
        // 应用视觉反馈
        if (enemySprites != null && effectMaterial != null)
        {
            foreach (var sprite in enemySprites)
            {
                sprite.material = effectMaterial;
            }
        }

        // 播放粒子特效
        if (effectParticles != null)
        {
            effectParticles.Play();
        }

        Debug.Log($"{effectType}效果应用于{targetEnemy.name}，持续{duration}秒");
    }

    protected virtual void UpdateEffect()
    {
        if (!isActive || targetEnemy == null) return;

        timer += Time.deltaTime;
        tickTimer += Time.deltaTime;

        // 间隔触发
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            OnTick();
        }
    }

    protected abstract void OnTick();  // 每次触发间隔执行

    protected virtual void EndEffect()
    {
        // 恢复原始材质
        if (enemySprites != null && originalMaterial != null)
        {
            foreach (var sprite in enemySprites)
            {
                sprite.material = originalMaterial;
            }
        }

        // 停止粒子特效
        if (effectParticles != null)
        {
            effectParticles.Stop();
        }

        isActive = false;
        Destroy(this);
    }

    void Update()
    {
        if (!isActive || targetEnemy == null) return;
        UpdateEffect();
    }

    void OnDestroy()
    {
        if (isActive)
        {
            EndEffect();
        }
    }
}