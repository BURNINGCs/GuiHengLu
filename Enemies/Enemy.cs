using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(HealthEvent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(DealContactDamage))]
[RequireComponent(typeof(DestroyedEvent))]
[RequireComponent(typeof(Destroyed))]
[RequireComponent(typeof(EnemyWeaponAI))]
[RequireComponent(typeof(AimWeaponEvent))]
[RequireComponent(typeof(AimWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(FireWeapon))]
[RequireComponent(typeof(SetActiveWeaponEvent))]
[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(WeaponFiredEvent))]
[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(ReloadWeapon))]
[RequireComponent(typeof(WeaponReloadedEvent))]
[RequireComponent(typeof(EnemyMovementAI))]
[RequireComponent(typeof(MovementToPositionEvent))]
[RequireComponent(typeof(MovementToPosition))]
[RequireComponent(typeof(IdleEvent))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(AnimateEnemy))]
[RequireComponent(typeof(MaterializeEffect))]
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
#endregion REQUIRE COMPONENTS

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    [HideInInspector] public EnemyDetailsSO enemyDetails;
    private HealthEvent healthEvent;
    [HideInInspector] public Health health;
    [HideInInspector] public AimWeaponEvent aimWeaponEvent;
    [HideInInspector] public FireWeaponEvent fireWeaponEvent;
    private FireWeapon fireWeapon;
    private SetActiveWeaponEvent setActiveWeaponEvent;
    private EnemyMovementAI enemyMovementAI;
    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;
    [HideInInspector] public IdleEvent idleEvent;
    private MaterializeEffect materializeEffect;
    private CircleCollider2D circleCollider2D;
    private PolygonCollider2D polygonCollider2D;
    [HideInInspector] public SpriteRenderer[] spriteRendererArray;
    [HideInInspector] public Animator animator;

    private void Awake()
    {
        //加载组件
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        fireWeapon = GetComponent<FireWeapon>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        enemyMovementAI = GetComponent<EnemyMovementAI>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        idleEvent = GetComponent<IdleEvent>();
        materializeEffect = GetComponent<MaterializeEffect>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        spriteRendererArray = GetComponentsInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        //订阅生命值事件
        healthEvent.OnHealthChanged += HealthEvent_OnHealthLost;
    }

    private void OnDisable()
    {
        //取消订阅生命值事件
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthLost;
    }

    //处理生命值损失事件
    private void HealthEvent_OnHealthLost(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        if (healthEventArgs.healthAmount <= 0)
        {
            EnemyDestroyed();
        }
    }

    //敌人被销毁
    private void EnemyDestroyed()
    {
        DestroyedEvent destroyedEvent = GetComponent<DestroyedEvent>();
        destroyedEvent.CallDestroyedEvent(false, health.GetStartingHealth());
    }

    //初始化敌人
    public void EnemyInitialization(EnemyDetailsSO enemyDetails, int enemySpawnNumber, DungeonLevelSO dungeonLevel)
    {
        this.enemyDetails = enemyDetails;

        SetEnemyMovementUpdateFrame(enemySpawnNumber);

        SetEnemyStartingHealth(dungeonLevel);

        SetEnemyStartingWeapon();

        SetEnemyAnimationSpeed();

        //使敌人显形
        StartCoroutine(MaterializeEnemy());
    }

    //设置敌人移动更新帧
    private void SetEnemyMovementUpdateFrame(int enemySpawnNumber)
    {
        //设定敌人应执行自身更新逻辑的帧编号
        enemyMovementAI.SetUpdateFrameNumber(enemySpawnNumber % Settings.targetFrameRateToSpreadPathfindingOver);
    }

    //设置敌人初始生命值
    private void SetEnemyStartingHealth(DungeonLevelSO dungeonLevel)
    {
        //获取当前地下城等级对应的敌人生命值
        foreach (EnemyHealthDetails enemyHealthDetails in enemyDetails.enemyHealthDetailsArray)
        {
            if (enemyHealthDetails.dungeonLevel == dungeonLevel)
            {
                health.SetStartingHealth(enemyHealthDetails.enemyHealthAmount);
                return;
            }
        }

        health.SetStartingHealth(Settings.defaultEnemyHealth);
    }

    //根据武器详情可脚本化对象（WeaponDetailsSO）配置敌人初始武器
    private void SetEnemyStartingWeapon()
    {
        //若敌人持有武器则执行对应处理逻辑
        if (enemyDetails.enemyWeapon != null)
        {
            Weapon weapon = new Weapon()
            {
                weaponDetails = enemyDetails.enemyWeapon,
                weaponReloadTimer = 0f,
                weaponClipRemainingAmmo = enemyDetails.enemyWeapon.weaponClipAmmoCapacity,
                weaponRemainingAmmo = enemyDetails.enemyWeapon.weaponAmmoCapacity,
                isWeaponReloading = false
            };

            //为敌人配备武器
            setActiveWeaponEvent.CallSetActiveWeaponEvent(weapon);
        }
    }

    //将敌人动画控制器的速度与移动速度匹配
    private void SetEnemyAnimationSpeed()
    {
        //将动画控制器速度与移动速度匹配
        animator.speed = enemyMovementAI.moveSpeed / Settings.baseSpeedForEnemyAnimations;
    }

    private IEnumerator MaterializeEnemy()
    {
        //禁用碰撞体、移动 AI 及武器 AI
        EnemyEnable(false);

        yield return StartCoroutine(materializeEffect.MaterializeRoutine(enemyDetails.enemyMaterializeShader, enemyDetails.enemyMaterializeColor,
            enemyDetails.enemyMaterializeTime, spriteRendererArray, enemyDetails.enemyStandardMaterial));

        //启用碰撞体、移动 AI 及武器 AI
        EnemyEnable(true);
    }

    private void EnemyEnable(bool isEnabled)
    {
        //启用 / 禁用碰撞体
        circleCollider2D.enabled = isEnabled;
        polygonCollider2D.enabled = isEnabled;

        //启用 / 禁用移动 AI
        enemyMovementAI.enabled = isEnabled;

        //启用 / 禁用武器开火功能
        fireWeapon.enabled = isEnabled;
    }
}
