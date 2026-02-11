using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SetActiveWeaponEvent))]
[DisallowMultipleComponent]
public class ActiveWeapon : MonoBehaviour
{
    #region Tooltip
    [Tooltip("用子物体 “武器” 游戏对象上的 SpriteRenderer 组件填充此处")]
    #endregion
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    #region Tooltip
    [Tooltip("用子物体 “武器” 游戏对象上的 PolygonCollider2D 组件填充此处")]
    #endregion
    [SerializeField] private PolygonCollider2D weaponPolygonCollider2D;
    #region Tooltip
    [Tooltip("用 “WeaponShootPosition” 游戏对象上的 Transform 组件填充此处")]
    #endregion
    [SerializeField] private Transform weaponShootPositionTransform;
    #region Tooltip
    [Tooltip("用 “WeaponEffectPosition” 游戏对象上的 Transform 组件填充此处")]
    #endregion
    [SerializeField] private Transform weaponEffectPositionTransform;

    private SetActiveWeaponEvent setWeaponEvent;
    private Weapon currentWeapon;

    private void Awake()
    {
        //加载组件
        setWeaponEvent = GetComponent<SetActiveWeaponEvent>();
    }

    private void OnEnable()
    {
        setWeaponEvent.OnSetActiveWeapon += SetWeaponEvent_OnSetActiveWeapon;
    }

    private void OnDisable()
    {
        setWeaponEvent.OnSetActiveWeapon -= SetWeaponEvent_OnSetActiveWeapon;
    }

    private void SetWeaponEvent_OnSetActiveWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetWeapon(setActiveWeaponEventArgs.weapon);
    }

    private void SetWeapon(Weapon weapon)
    {
        currentWeapon = weapon;

        //设置当前武器精灵图
        weaponSpriteRenderer.sprite = currentWeapon.weaponDetails.weaponSprite;

        //若武器拥有多边形碰撞体和精灵图，则将其设置为武器精灵图的物理形状
        if (weaponPolygonCollider2D != null && weaponSpriteRenderer.sprite != null)
        {
            //获取精灵图物理形状 —— 返回的是精灵图物理形状的点，以 Vector2 列表形式呈现
            List<Vector2> spritePhysicsShapePointsList = new List<Vector2>();
            weaponSpriteRenderer.sprite.GetPhysicsShape(0, spritePhysicsShapePointsList);

            //为武器设置多边形碰撞体，使其适配精灵图的物理形状 —— 将碰撞体的点设为精灵图物理形状的点
            weaponPolygonCollider2D.points = spritePhysicsShapePointsList.ToArray();
        }

        //设置武器射击位置
        weaponShootPositionTransform.localPosition = currentWeapon.weaponDetails.weaponShootPosition;
    }

    public AmmoDetailsSO GetCurrentAmmo()
    {
        return currentWeapon.weaponDetails.weaponCurrentAmmo;
    }

    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public Vector3 GetShootPosition()
    {
        return weaponShootPositionTransform.position;
    }

    public Vector3 GetShootEffectPosition()
    {
        return weaponEffectPositionTransform.position;
    }

    public void RemoveCurrentWeapon()
    {
        currentWeapon = null;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponSpriteRenderer), weaponSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponPolygonCollider2D), weaponPolygonCollider2D);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponShootPositionTransform), weaponShootPositionTransform);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponEffectPositionTransform), weaponEffectPositionTransform);
    }
#endif
    #endregion
}
