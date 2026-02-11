using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerUI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("用于显示玩家头像的Image组件")]
    #endregion
    [SerializeField] private Image playerHeadImage;

    private void Start()
    {
        // 确保有Image组件
        if (playerHeadImage == null)
        {
            playerHeadImage = GetComponent<Image>();
            if (playerHeadImage == null)
            {
                Debug.LogError("PlayerHeadUI: 未找到Image组件！");
                return;
            }
        }

        // 获取玩家头像并设置
        SetupPlayerHeadIcon();
    }

    private void SetupPlayerHeadIcon()
    {
        // 从GameManager获取玩家头像
        if (GameManager.Instance != null)
        {
            Sprite headIcon = GameManager.Instance.GetPlayerHeadIcon();

            if (headIcon != null)
            {
                playerHeadImage.sprite = headIcon;

                // 确保图像显示正确
                playerHeadImage.preserveAspect = true;
                playerHeadImage.SetNativeSize();
            }
            else
            {
                Debug.LogWarning("PlayerHeadUI: 未找到玩家头像精灵！");
            }
        }
        else
        {
            Debug.LogError("PlayerHeadUI: GameManager实例为空！");
        }
    }
}
