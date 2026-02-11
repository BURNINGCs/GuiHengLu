using Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public class Minimap : MonoBehaviour
{
    #region Tooltip
    [Tooltip("用子物体 “MinimapPlayer” 游戏对象填充此处")]
    #endregion Tooltip

    [SerializeField] private GameObject miniMapPlayer;

    private Transform playerTransform;

    private void Start()
    {
        playerTransform = GameManager.Instance.GetPlayer().transform;

        //将玩家设为 Cinemachine 相机的目标
        CinemachineVirtualCamera cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = playerTransform;

        //设置小地图玩家图标
        SpriteRenderer spriteRenderer = miniMapPlayer.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = GameManager.Instance.GetPlayerMiniMapIcon();
        }
    }

    private void Update()
    {
        //移动小地图玩家图标以跟随玩家
        if (playerTransform != null && miniMapPlayer != null)
        {
            miniMapPlayer.transform.position = playerTransform.position;
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(miniMapPlayer), miniMapPlayer);
    }
#endif
    #endregion Validation
}
