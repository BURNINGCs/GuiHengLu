using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class AnimatePlayer : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        //加载组件
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        //订阅按速度移动事件
        player.movementByVelocityEvent.OnMovementByVelocity += MovementByVelocityEvent_OnMovementByVelocity;

        //订阅 “向指定位置移动” 事件
        player.movementToPositionEvent.OnMovementToPosition += MovementToPositionEvent_OnMovementToPosition;

        //订阅闲置事件
        player.idleEvent.OnIdle += IdleEvent_OnIdle;

        //订阅武器瞄准事件
        player.aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeaponAim;
    }

    private void OnDisable()
    {
        //取消订阅按速度移动事件
        player.movementByVelocityEvent.OnMovementByVelocity -= MovementByVelocityEvent_OnMovementByVelocity;

        //取消订阅 “向指定位置移动” 事件
        player.movementToPositionEvent.OnMovementToPosition -= MovementToPositionEvent_OnMovementToPosition;

        //取消订阅闲置事件
        player.idleEvent.OnIdle -= IdleEvent_OnIdle;

        //取消订阅武器瞄准事件
        player.aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeaponAim;
    }

    //按速度移动事件处理器
    private void MovementByVelocityEvent_OnMovementByVelocity(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityArgs movementByVelocityArgs)
    {
        SetMovementAnimationParameters();
    }

    private void MovementToPositionEvent_OnMovementToPosition(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs movementToPositionArgs)
    {
        InitializeAimAnimationParameters();
    }

    //闲置事件处理器
    private void IdleEvent_OnIdle(IdleEvent idleEvent)
    {
        SetIdleAnimationParameters();
    }

    //武器瞄准事件处理器
    private void AimWeaponEvent_OnWeaponAim(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs aimWeaponEventArgs)
    {
        InitializeAimAnimationParameters();

        SetAimWeaponAnimationParameters(aimWeaponEventArgs.aimDirection);
    }

    //初始化瞄准动画参数
    private void InitializeAimAnimationParameters()
    {
        player.animator.SetBool(Settings.aimRight, false);
        player.animator.SetBool(Settings.aimLeft, false);
    }

    //设置移动动画参数
    private void SetMovementAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, true);
        player.animator.SetBool(Settings.isIdle, false);
    }

    //设置移动动画参数
    private void SetIdleAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, true);
    }

    //设置瞄准动画参数
    private void SetAimWeaponAnimationParameters(AimDirection aimDirection)
    {
        //设置瞄准方向
        switch (aimDirection)
        {
            case AimDirection.Right:
                player.animator.SetBool(Settings.aimRight,true); 
                break;

            case AimDirection.Left:
                player.animator.SetBool(Settings.aimLeft,true); 
                break;
        }
    }
}
