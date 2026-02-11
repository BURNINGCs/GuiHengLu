using UnityEngine;

[DisallowMultipleComponent]
public class Ammo : MonoBehaviour, IFireable
{
    #region Tooltip
    [Tooltip("用子物体的 TrailRenderer 组件填充")]
    #endregion Tooltip
    [SerializeField] private TrailRenderer trailRenderer;

    private float ammoRange = 0f; //每发弹药的射程
    private float ammoSpeed;
    private Vector3 fireDirectionVector;
    private float fireDirectionAngle;
    private SpriteRenderer spriteRenderer;
    [HideInInspector] public AmmoDetailsSO ammoDetails;
    private float ammoChargeTimer;
    private bool isAmmoMaterialSet = false;
    private bool overrideAmmoMovement;
    private bool isColliding = false;

    private void Awake()
    {
        //缓存精灵渲染器
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        //弹药充能效果
        if (ammoChargeTimer > 0f)
        {
            ammoChargeTimer -= Time.deltaTime;
            return;
        }
        else if (!isAmmoMaterialSet) 
        {
            SetAmmoMaterial(ammoDetails.ammoMaterial);
            isAmmoMaterialSet = true;
        }

        // Don't move ammo if movement has been overriden - e.g. this ammo is part of an ammo pattern
        if (!overrideAmmoMovement)
        {
            //计算弹药移动的距离向量
            Vector3 distanceVector = fireDirectionVector * ammoSpeed * Time.deltaTime;

            transform.position += distanceVector;

            //达到最大射程后禁用
            ammoRange -= distanceVector.magnitude;

            if (ammoRange < 0f)
            {
                DisableAmmo();
            }
        }
        
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //若已与其他物体发生碰撞，则直接返回
        if (isColliding) return;

        //对碰撞对象造成伤害
        DealDamage(collision);

        //显示弹药命中效果
        AmmoHitEffect();

        DisableAmmo();
    }

    private void DealDamage(Collider2D collision)
    {
        Health health = collision.GetComponent<Health>();
        if (health != null)
        {
            int baseDamage = ammoDetails.ammoDamage;
            float elementMultiplier = 1f;

            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemy.enemyDetails != null &&
                enemy.enemyDetails.elementProperty != null)
            {
                // 根据弹药的元素类型获取对应的抗性
                float resistance = GetElementResistance(enemy, ammoDetails.elementType);
                elementMultiplier = CalculateElementMultiplier(ammoDetails.elementType,
                                                             enemy.enemyDetails.elementProperty.mainElement,
                                                             resistance);
            }

            int finalDamage = Mathf.RoundToInt(baseDamage * elementMultiplier);
            health.TakeDamage(finalDamage);

            if (enemy != null && ammoDetails.elementType != ElementType.None)
            {
                ApplyElementEffect(enemy, ammoDetails.elementType);
            }
        }
    }

    private void ApplyElementEffect(Enemy enemy, ElementType elementType)
    {
        if (enemy == null) return;

        switch (elementType)
        {
            case ElementType.Fire:
                ApplyFireEffect(enemy);
                break;

            case ElementType.Water:
                ApplyIceEffect(enemy);
                break;

                // 其他元素...
        }
    }

    private void ApplyFireEffect(Enemy enemy)
    {
        // 普通火焰攻击的灼烧效果（较弱）
        BurningEffect existing = enemy.GetComponent<BurningEffect>();
        if (existing != null)
        {
            existing.currentStacks = Mathf.Min(existing.currentStacks + 1, 3);
            existing.duration = Mathf.Max(existing.duration, 2f);
        }
        else
        {
            BurningEffect effect = enemy.gameObject.AddComponent<BurningEffect>();
            effect.Initialize(enemy, 2f, 1); // 持续2秒，1层
        }
    }

    private void ApplyIceEffect(Enemy enemy)
    {
        // 普通冰冻攻击的减速效果（较弱）
        FrozenEffect existing = enemy.GetComponent<FrozenEffect>();
        if (existing != null)
        {
            existing.currentStacks = Mathf.Min(existing.currentStacks + 1, 2);
            existing.duration = Mathf.Max(existing.duration, 1.5f);
        }
        else
        {
            FrozenEffect effect = enemy.gameObject.AddComponent<FrozenEffect>();
            effect.Initialize(enemy, 1.5f, 1); // 持续1.5秒，1层
        }
    }

    // 获取敌人的具体抗性值
    private float GetElementResistance(Enemy enemy, ElementType attackElement)
    {
        switch (attackElement)
        {
            case ElementType.Fire:
                return enemy.enemyDetails.elementProperty.fireResistance;
            case ElementType.Water:
                return enemy.enemyDetails.elementProperty.waterResistance;
            case ElementType.Wood:
                return enemy.enemyDetails.elementProperty.woodResistance;
            case ElementType.Metal:
                return enemy.enemyDetails.elementProperty.metalResistance;
            default:
                return 0f;
        }
    }

    // 计算元素倍率（考虑五行相克）
    private float CalculateElementMultiplier(ElementType attackElement,
                                            ElementType defenseElement,
                                            float resistance)
    {
        float multiplier = 1f;

        // 先考虑五行相克关系
        if (IsElementStrongAgainst(attackElement, defenseElement))
        {
            multiplier *= 1.5f; // 克制时150%伤害
        }
        else if (IsElementWeakAgainst(attackElement, defenseElement))
        {
            multiplier *= 0.5f; // 被克时50%伤害
        }

        // 再考虑抗性（免疫、易伤等）
        multiplier *= (1f - resistance);

        return multiplier;
    }

    // 判断是否克制（火克金，金克木，木克土，土克水，水克火）
    private bool IsElementStrongAgainst(ElementType attack, ElementType defense)
    {
        return (attack == ElementType.Fire && defense == ElementType.Metal) ||
               (attack == ElementType.Metal && defense == ElementType.Wood) ||
               (attack == ElementType.Wood && defense == ElementType.Water) ||
               (attack == ElementType.Water && defense == ElementType.Fire);
    }

    // 判断是否被克
    private bool IsElementWeakAgainst(ElementType attack, ElementType defense)
    {
        return IsElementStrongAgainst(defense, attack); // 克制关系反过来
    }

    //初始化发射的弹药 —— 使用弹药详情、瞄准角度、武器角度和武器瞄准方向向量。
    //若该弹药属于某一模式，可通过将 overrideAmmoMovement 设为 true 来覆盖弹药移动逻辑
    public void InitialiseAmmo(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, float ammoSpeed, Vector3 weaponAimDirectionVector, bool overrideAmmoMovement = false)
    {
        #region Ammo

        this.ammoDetails = ammoDetails;

        //初始化碰撞标记（isColliding）
        isColliding = false;

        //设置发射方向
        SetFireDirection(ammoDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector);

        //设置弹药精灵图
        spriteRenderer.sprite = ammoDetails.ammoSprite;

        //根据是否存在弹药充能时间设置初始弹药材质
        if (ammoDetails.ammoChargeTime > 0f)
        {
            //设置弹药充能计时器
            ammoChargeTimer = ammoDetails.ammoChargeTime;
            SetAmmoMaterial(ammoDetails.ammoChargeMaterial);
            isAmmoMaterialSet = false;
        }
        else
        {
            ammoChargeTimer = 0f;
            SetAmmoMaterial(ammoDetails.ammoMaterial);
            isAmmoMaterialSet = true;
        }

        //设置弹药射程
        ammoRange = ammoDetails.ammoRange;

        //设置弹药速度
        this.ammoSpeed = ammoSpeed;

        //覆盖弹药移动逻辑
        this.overrideAmmoMovement = overrideAmmoMovement;

        //激活弹药游戏对象
        gameObject.SetActive(true);

        #endregion Ammo

        #region Trail

        if (ammoDetails.isAmmoTrail)
        {
            trailRenderer.gameObject.SetActive(true);
            trailRenderer.emitting = true;
            trailRenderer.material = ammoDetails.ammoTrailMaterial;
            trailRenderer.startWidth = ammoDetails.ammoTrailStartWidth;
            trailRenderer.endWidth = ammoDetails.ammoTrailEndWidth;
            trailRenderer.time = ammoDetails.ammoTrailTime;
        }
        else
        {
            trailRenderer.emitting = false;
            trailRenderer.gameObject.SetActive(false);
        }

        #endregion Trail
    }

    //基于输入角度和经随机散布调整后的方向，设置弹药发射方向和角度
    private void SetFireDirection(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        //计算最小值与最大值之间的随机散布角度
        float randomSpread = Random.Range(ammoDetails.ammoSpreadMin, ammoDetails.ammoSpreadMax);

        //获取 1 或 - 1 的随机散布切换值
        int spreadToggle = Random.Range(0, 2) * 2 - 1;

        if (weaponAimDirectionVector.magnitude < Settings.useAimAngleDistance)
        {
            fireDirectionAngle = aimAngle;
        }
        else
        {
            fireDirectionAngle = weaponAimAngle;
        }

        //通过随机散布调整弹药发射角度
        fireDirectionAngle += spreadToggle * randomSpread;

        //设置弹药旋转角度
        transform.eulerAngles = new Vector3(0f, 0f, fireDirectionAngle);

        //设置弹药发射方向
        fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);
    }

    //禁用弹药 —— 使其返回对象池
    private void DisableAmmo()
    {
        gameObject.SetActive(false);
    }

    //展示弹药命中效果
    private void AmmoHitEffect()
    {
        //若已指定命中效果则执行处理逻辑
        if (ammoDetails.ammoHitEffect != null && ammoDetails.ammoHitEffect.ammoHitEffectPrefab != null)
        {
            //从对象池获取弹药命中效果游戏对象（含粒子系统组件）
            AmmoHitEffect ammoHitEffect = (AmmoHitEffect)PoolManager.Instance.ReuseComponent
            (ammoDetails.ammoHitEffect.ammoHitEffectPrefab, transform.position, Quaternion.identity);

            //设置命中效果
            ammoHitEffect.SetHitEffect(ammoDetails.ammoHitEffect);

            //激活该游戏对象（粒子系统已设置为播放完成后自动禁用该游戏对象）
            ammoHitEffect.gameObject.SetActive(true);
        }
    }

    public void SetAmmoMaterial(Material material)
    {
        spriteRenderer.material = material;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(trailRenderer), trailRenderer);
    }
#endif
    #endregion Validation
}
