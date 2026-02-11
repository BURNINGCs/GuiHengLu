using UnityEngine;
using TMPro;

public class ScorePrefab : MonoBehaviour
{
    public TextMeshProUGUI rankTMP;//排名
    public TextMeshProUGUI nameTMP;//名字
    public TextMeshProUGUI levelTMP;//关卡
    public TextMeshProUGUI scoreTMP;//分数

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(rankTMP), rankTMP);
        HelperUtilities.ValidateCheckNullValue(this, nameof(nameTMP), nameTMP);
        HelperUtilities.ValidateCheckNullValue(this, nameof(levelTMP), levelTMP);
        HelperUtilities.ValidateCheckNullValue(this, nameof(scoreTMP), scoreTMP);
    }


#endif
    #endregion
}
