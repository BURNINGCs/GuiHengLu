using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSelectionUI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("填充子游戏对象 WeaponAnchorPosition/WeaponRotationPoint/Hand 上的 Sprite Renderer")]
    #endregion
    public SpriteRenderer playerHandSpriteRenderer;
    #region Tooltip
    [Tooltip("填充子游戏对象 HandNoWeapon 上的 Sprite Renderer")]
    #endregion
    public SpriteRenderer playerHandNoWeaponSpriteRenderer;
    #region Tooltip
    [Tooltip("填充子游戏对象 WeaponAnchorPosition/WeaponRotationPoint/Weapon 上的 Sprite Renderer")]
    #endregion
    public SpriteRenderer playerWeaponSpriteRenderer;
    #region Tooltip
    [Tooltip("填充 Animator 组件")]
    #endregion
    public Animator animator;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerHandSpriteRenderer), playerHandSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerHandNoWeaponSpriteRenderer), playerHandNoWeaponSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerWeaponSpriteRenderer), playerWeaponSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(animator), animator);
    }
#endif
    #endregion
}
