using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HealthUI : MonoBehaviour
{
    [Tooltip("拖入你的血条预制体")]
    [SerializeField] private GameObject healthBarPrefab;

    private GameObject healthBarInstance;
    private Image healthFillImage;

    private void Start()
    {
        InitializeHealthBar();
    }

    private void OnEnable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void InitializeHealthBar()
    {
        // 如果已有血条实例，先销毁
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }

        // 只创建一个血条实例
        healthBarInstance = Instantiate(healthBarPrefab, transform);

        // 假设你的填充Image叫做"Fill"或者有特定名称
        Transform fillTransform = healthBarInstance.transform.Find("Fill");

        if (fillTransform != null)
        {
            healthFillImage = fillTransform.GetComponent<Image>();
        }
        else
        {
            // 如果没有找到叫"Fill"的子对象，尝试查找第一个Image组件
            Image[] images = healthBarInstance.GetComponentsInChildren<Image>();
            foreach (Image img in images)
            {
                if (img.gameObject != healthBarInstance)
                {
                    healthFillImage = img;
                    break;
                }
            }
        }

        if (healthFillImage != null)
        {
            // 确保填充Image的设置正确
            healthFillImage.type = Image.Type.Filled;
            healthFillImage.fillMethod = Image.FillMethod.Horizontal;
            healthFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

            // 初始设置为满血
            healthFillImage.fillAmount = 1f;
        }
        else
        {
            Debug.LogError("在血条预制体中找不到填充Image组件！");
        }
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        if (healthFillImage != null)
        {
            // 直接更新填充量
            healthFillImage.fillAmount = healthEventArgs.healthPercent;
            Debug.Log($"血量更新: {healthEventArgs.healthPercent * 100:F1}%");
        }
    }
}