// BlueBarUI.cs
using UnityEngine;
using UnityEngine.UI;

public class BlueBarUI : MonoBehaviour
{
    [Header("蓝条组件")]
    [SerializeField] private Image blueBarFill;     // 填充条
    [SerializeField] private Image blueBarBg;       // 背景

    [Header("蓝条设置")]
    public float maxBlue = 100f;    // 最大蓝量
    [SerializeField] private float currentBlue = 100f; // 当前蓝量
    public float blueRegenRate = 5f; // 每秒回复量

    [Header("消耗设置")]
    public float freezeSkillCost = 30f;   // 冰冻技能消耗
    public float burnSkillCost = 25f;     // 灼烧技能消耗

    // 属性，便于外部访问
    public float CurrentBlue { get => currentBlue; private set => currentBlue = value; }

    private void Awake()
    {
        // 确保找到UI组件
        if (blueBarFill == null)
        {
            // 查找名为"bar"的直接子对象
            blueBarFill = GetComponentInChildren<Image>();

            // 如果有多个Image，需要更精确的查找
            Image[] images = GetComponentsInChildren<Image>();
            foreach (Image img in images)
            {
                if (img.gameObject.name.Contains("bar") ||
                    img.fillMethod == Image.FillMethod.Horizontal)
                {
                    blueBarFill = img;
                    break;
                }
            }
        }

        UpdateBlueBar();
    }

    private void Update()
    {
        // 自动回复蓝量
        if (currentBlue < maxBlue)
        {
            currentBlue += blueRegenRate * Time.deltaTime;
            currentBlue = Mathf.Min(currentBlue, maxBlue);
            UpdateBlueBar();
        }
    }

    // 消耗蓝量
    public bool ConsumeBlue(float amount)
    {
        if (currentBlue >= amount)
        {
            currentBlue -= amount;
            UpdateBlueBar();
            return true;
        }
        return false;
    }

    // 更新UI显示
    private void UpdateBlueBar()
    {
        if (blueBarFill != null)
        {
            blueBarFill.fillAmount = currentBlue / maxBlue;

            // 调试输出
            // Debug.Log($"蓝量: {currentBlue}/{maxBlue}, 填充: {blueBarFill.fillAmount}");
        }
        else
        {
            // 尝试重新查找
            blueBarFill = GetComponentInChildren<Image>();
            if (blueBarFill != null)
            {
                blueBarFill.fillAmount = currentBlue / maxBlue;
            }
        }
    }

    // 获取蓝量百分比
    public float GetBluePercentage()
    {
        return currentBlue / maxBlue;
    }

    // 获取蓝量数值
    public float GetCurrentBlue()
    {
        return currentBlue;
    }

    // 调试方法：测试UI更新
    public void TestUIUpdate()
    {
        Debug.Log("测试蓝条更新...");
        Debug.Log($"蓝条Fill组件: {(blueBarFill != null ? "找到" : "未找到")}");
        Debug.Log($"蓝条Fill填充方式: {(blueBarFill != null ? blueBarFill.fillMethod.ToString() : "null")}");
        Debug.Log($"当前蓝量: {currentBlue}/{maxBlue}");

        // 强制测试一次
        currentBlue = 50f;
        UpdateBlueBar();
    }
}