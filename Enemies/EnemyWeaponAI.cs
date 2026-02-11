using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyWeaponAI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("选择敌人子弹将命中的图层（Layer）")]
    #endregion Tooltip
    [SerializeField] private LayerMask layerMask;
    #region Tooltip
    [Tooltip("用 “武器射击位置” 子游戏对象的变换组件（Transform）填充此处")]
    #endregion Tooltip
    [SerializeField] private Transform weaponShootPosition;
    private Enemy enemy;
    private EnemyDetailsSO enemyDetails;
    private float firingIntervalTimer;
    private float firingDurationTimer;

    private void Awake()
    {
        //加载组件
        enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        enemyDetails = enemy.enemyDetails;

        firingIntervalTimer = WeaponShootInterval();
        firingDurationTimer = WeaponShootDuration();
    }

    private void Update()
    {
        //更新计时器
        firingIntervalTimer -= Time.deltaTime;

        //间隔计时器
        if (firingIntervalTimer < 0f)
        {
            if (firingDurationTimer >= 0)
            {
                firingDurationTimer -= Time.deltaTime;

                FireWeapon();
            }
            else
            {
                //重置计时器
                firingIntervalTimer = WeaponShootInterval();
                firingDurationTimer = WeaponShootDuration();
            }
        }
    }

    //计算随机的武器射击持续时间（在最小与最大值之间取值）
    private float WeaponShootDuration()
    {
        // Calculate a random weapon shoot duration
        return Random.Range(enemyDetails.firingDurationMin, enemyDetails.firingDurationMax);
    }

    //计算随机的武器射击持续时间
    private float WeaponShootInterval()
    {
        //计算随机的武器射击间隔（在最小与最大值之间取值）
        return Random.Range(enemyDetails.firingIntervalMin, enemyDetails.firingIntervalMax);
    }

    //开火（触发武器射击）
    private void FireWeapon()
    {
        //与玩家的距离
        Vector3 playerDirectionVector = GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position;

        //计算从武器射击位置指向玩家的方向向量
        Vector3 weaponDirection = (GameManager.Instance.GetPlayer().GetPlayerPosition() - weaponShootPosition.position);

        //获取武器到玩家的角度
        float weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        //获取敌人到玩家的角度
        float enemyAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirectionVector);

        //设置敌人瞄准方向
        AimDirection enemyAimDirection = HelperUtilities.GetAimDirection(enemyAngleDegrees);

        //触发武器瞄准事件
        enemy.aimWeaponEvent.CallAimWeaponEvent(enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection);

        //仅当敌人持有武器时才开火
        if (enemyDetails.enemyWeapon != null)
        {
            //获取弹药射程
            float enemyAmmoRange = enemyDetails.enemyWeapon.weaponCurrentAmmo.ammoRange;

            //玩家是否在射程范围内
            if (playerDirectionVector.magnitude <= enemyAmmoRange)
            {
                //该敌人开火前是否需要与玩家保持视线通畅（无遮挡）？
                if (enemyDetails.firingLineOfSightRequired && !IsPlayerInLineOfSight(weaponDirection, enemyAmmoRange)) return;

                //触发武器射击事件
                enemy.fireWeaponEvent.CallFireWeaponEvent(true, true, enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection);
            }
        }
    }

    private bool IsPlayerInLineOfSight(Vector3 weaponDirection, float enemyAmmoRange)
    {
        RaycastHit2D raycastHit2D = Physics2D.Raycast(weaponShootPosition.position, (Vector2)weaponDirection, enemyAmmoRange, layerMask);

        if (raycastHit2D && raycastHit2D.transform.CompareTag(Settings.playerTag))
        {
            return true;
        }

        return false;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponShootPosition), weaponShootPosition);
    }

#endif
    #endregion Validation
}
