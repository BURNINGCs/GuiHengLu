using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    #region Tooltip

    [Tooltip("包含移动详情（如速度）的 MovementDetailsSO 可脚本化对象")]

    #endregion Tooltip

    [SerializeField] private MovementDetailsSO movementDetails;

    private Player player;
    private bool leftMouseDownPreviousFrame = false;
    private int currentWeaponIndex = 1;
    private float moveSpeed;
    private bool isPlayerMovementDisabled = false;

    private void Awake()
    {
        //加载组件
        player = GetComponent<Player>();

        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        //设置初始武器
        SetStartingWeapon();

        //设置玩家动画速度
        SetPlayerAnimationSpeed();
    }

    //设置玩家初始武器
    private void SetStartingWeapon()
    {
        int index = 1;

        foreach (Weapon weapon in player.weaponList)
        {
            if (weapon.weaponDetails == player.playerDetails.startingWeapon)
            {
                SetWeaponByIndex(index);
                break;
            }
            index++;
        }
    }

    //将玩家动画器速度设置为与移动速度匹配
    private void SetPlayerAnimationSpeed()
    {
        //将动画器速度设置为与移动速度匹配
        player.animator.speed = moveSpeed / Settings.baseSpeedForPlayerAnimations;
    }

    private void Update()
    {
        // if player movement disabled then return
        if (isPlayerMovementDisabled)
            return;

        //处理玩家移动输入
        MovementInput();

        //处理玩家武器输入
        WeaponInput();

        //处理玩家使用物品输入
        UseItemInput();
    }

    //玩家移动输入
    private void MovementInput()
    {
        //获取移动输入
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");

        //根据输入创建方向向量
        Vector2 direction = new Vector2(horizontalMovement, verticalMovement);

        //调整对角线移动的距离（采用勾股定理近似值）
        if (horizontalMovement != 0f && verticalMovement != 0f) 
        {
            direction *= 0.7f;
        }

        //若存在移动
        if (direction != Vector2.zero) 
        {
            //触发移动事件
            player.movementByVelocityEvent.CallMovementByVelocityEvent(direction, moveSpeed);
        }
        //否则触发闲置事件
        else
        {
            player.idleEvent.CallIdleEvent();
        }

    }

    //武器输入
    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        //武器瞄准输入
        AimWeaponInput(out  weaponDirection, out weaponAngleDegrees,out playerAngleDegrees, out playerAimDirection);

        //武器开火输入
        FireWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        //切换武器输入
        SwitchWeaponInput();

        //重新装填武器输入
        ReloadWeaponInput();
    }

    private void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, 
        out float playerAngleDegrees, out AimDirection playerAimDirection)
    {
        //获取鼠标世界位置
        Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();

        //计算从武器射击位置到鼠标光标的方向向量
        weaponDirection = (mouseWorldPosition - player.activeWeapon.GetShootPosition());

        //计算从玩家变换位置到鼠标光标的方向向量
        Vector3 playerDirection = (mouseWorldPosition - transform.position);

        //获取武器到光标的角度
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        //获取玩家到光标的角度
        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        //设置玩家瞄准方向
        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        //触发武器瞄准事件
        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
    }

    private void FireWeaponInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        //鼠标左键点击时开火
        if (Input.GetMouseButton(0))
        {
            //触发武器开火事件
            player.fireWeaponEvent.CallFireWeaponEvent(true, leftMouseDownPreviousFrame, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
            leftMouseDownPreviousFrame = true;
        }
        else
        {
            leftMouseDownPreviousFrame = false;
        }
    }

    private void SwitchWeaponInput()
    {
        //若选中鼠标滚轮，则切换武器
        if (Input.mouseScrollDelta.y < 0f)
        {
            PreviousWeapon();
        }

        if (Input.mouseScrollDelta.y > 0f)
        {
            NextWeapon();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWeaponByIndex(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetWeaponByIndex(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetWeaponByIndex(3);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetWeaponByIndex(4);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetWeaponByIndex(5);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SetWeaponByIndex(6);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SetWeaponByIndex(7);
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SetWeaponByIndex(8);
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SetWeaponByIndex(9);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SetWeaponByIndex(10);
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            SetCurrentWeaponToFirstInTheList();
        }

    }

    private void SetWeaponByIndex(int weaponIndex)
    {
        if (weaponIndex - 1 < player.weaponList.Count)
        {
            currentWeaponIndex = weaponIndex;
            player.setActiveWeaponEvent.CallSetActiveWeaponEvent(player.weaponList[weaponIndex - 1]);
        }
    }

    private void NextWeapon()
    {
        currentWeaponIndex++;

        if (currentWeaponIndex > player.weaponList.Count)
        {
            currentWeaponIndex = 1;
        }

        SetWeaponByIndex(currentWeaponIndex);
    }

    private void PreviousWeapon()
    {
        currentWeaponIndex--;

        if (currentWeaponIndex < 1)
        {
            currentWeaponIndex = player.weaponList.Count;
        }

        SetWeaponByIndex(currentWeaponIndex);
    }

    private void ReloadWeaponInput()
    {
        Weapon currentWeapon = player.activeWeapon.GetCurrentWeapon();

        //若当前武器正在装填，则返回（不执行后续操作）
        if (currentWeapon.isWeaponReloading) return;

        //若剩余弹药量小于弹夹容量且非无限弹药，则返回（不执行后续操作）
        if (currentWeapon.weaponRemainingAmmo < currentWeapon.weaponDetails.weaponClipAmmoCapacity && !currentWeapon.weaponDetails.hasInfiniteAmmo)
            return;

        //若弹夹弹药量等于弹夹容量，则返回（不执行后续操作）
        if (currentWeapon.weaponClipRemainingAmmo == currentWeapon.weaponDetails.weaponClipAmmoCapacity) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            //调用重新装填武器事件
            player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentWeapon(), 0);
        }

    }

    //使用距离玩家2单位内的最近物品
    private void UseItemInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float useItemRadius = 2f;

            //获取玩家附近任何'可用'物品
            Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(player.GetPlayerPosition(), useItemRadius);

            //遍历检测到的物品，查看是否有'可用'物品
            foreach (Collider2D collider2D in collider2DArray)
            {
                IUseable iUseable = collider2D.GetComponent<IUseable>();

                if (iUseable != null)
                {
                    iUseable.UseItem();
                }
            }
        }
    }

    //Enable the player movement
    public void EnablePlayer()
    {
        isPlayerMovementDisabled = false;
    }

    //Disable the player movement
    public void DisablePlayer()
    {
        isPlayerMovementDisabled = true;
        player.idleEvent.CallIdleEvent();
    }

    //将当前武器设为玩家武器列表的首位
    private void SetCurrentWeaponToFirstInTheList()
    {
        //创建新的临时列表
        List<Weapon> tempWeaponList = new List<Weapon>();

        //将当前武器添加到临时列表的首位
        Weapon currentWeapon = player.weaponList[currentWeaponIndex - 1];
        currentWeapon.weaponListPosition = 1;
        tempWeaponList.Add(currentWeapon);

        //遍历现有武器列表并添加（跳过当前武器）
        int index = 2;

        foreach (Weapon weapon in player.weaponList)
        {
            if (weapon == currentWeapon) continue;

            tempWeaponList.Add(weapon);
            weapon.weaponListPosition = index;
            index++;
        }

        //赋值新列表（覆盖原武器列表）
        player.weaponList = tempWeaponList;

        currentWeaponIndex = 1;

        //设置当前武器
        SetWeaponByIndex(currentWeaponIndex);
    }

    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }

#endif

    #endregion Validation
}
