using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class AnimateEnemy : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        //加载组件
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        //订阅移动事件
        enemy.movementToPositionEvent.OnMovementToPosition += MovementToPositionEvent_OnMovementToPosition;

        //订阅闲置事件
        enemy.idleEvent.OnIdle += IdleEvent_OnIdle;

        //订阅武器瞄准事件
        enemy.aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeaponAim;
    }

    private void OnDisable()
    {
        //取消订阅移动事件
        enemy.movementToPositionEvent.OnMovementToPosition -= MovementToPositionEvent_OnMovementToPosition;

        //取消订阅闲置事件
        enemy.idleEvent.OnIdle -= IdleEvent_OnIdle;

        //取消订阅武器瞄准事件
        enemy.aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeaponAim;
    }

    //武器瞄准事件处理器
    private void AimWeaponEvent_OnWeaponAim(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs aimWeaponEventArgs)
    {
        InitialiseAimAnimationParameters();
        SetAimWeaponAnimationParameters(aimWeaponEventArgs.aimDirection);
    }


    //移动事件处理器
    private void MovementToPositionEvent_OnMovementToPosition(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs movementToPositionArgs)
    {
        SetMovementAnimationParameters();
    }

    //闲置事件处理器
    private void IdleEvent_OnIdle(IdleEvent idleEvent)
    {
        SetIdleAnimationParameters();
    }

    //初始化瞄准动画参数
    private void InitialiseAimAnimationParameters()
    {
        enemy.animator.SetBool(Settings.aimRight, false);
        enemy.animator.SetBool(Settings.aimLeft, false);
    }

    //设置移动动画参数
    private void SetMovementAnimationParameters()
    {
        //设置为 “移动中” 状态
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.isMoving, true);
    }

    //设置闲置动画参数
    private void SetIdleAnimationParameters()
    {
        //设置为 “闲置” 状态
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, true);
    }

    //设置瞄准动画参数
    private void SetAimWeaponAnimationParameters(AimDirection aimDirection)
    {
        //设置瞄准方向
        switch (aimDirection)
        {
            case AimDirection.Right:
                enemy.animator.SetBool(Settings.aimRight, true);
                break;

            case AimDirection.Left:
                enemy.animator.SetBool(Settings.aimLeft, true);
                break;
        }
    }
}
