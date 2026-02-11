using UnityEngine;

[CreateAssetMenu(fileName = "SoundEffect_", menuName = "Scriptable Objects/Sounds/SoundEffect")] 
public class SoundEffectSO : ScriptableObject
{
    #region Header SOUND EFFECT DETAILS
    [Space(10)]
    [Header("音效详情")]
    #endregion
    #region Tooltip
    [Tooltip("音效名称")]
    #endregion
    public string soundEffectName;
    #region Tooltip
    [Tooltip("音效预制体")]
    #endregion
    public GameObject soundPrefab;
    #region Tooltip
    [Tooltip("音效音频片段")]
    #endregion
    public AudioClip soundEffectClip;
    #region Tooltip
    [Tooltip("音效的最小音调偏差值。音调偏差值会在最小值与最大值之间随机生成，随机的音调偏差能让音效听起来更自然。")]
    #endregion
    [Range(0.1f, 1.5f)]
    public float soundEffectPitchRandomVariationMin = 0.8f;
    #region Tooltip
    [Tooltip("音效的最大音调偏差值。音调偏差值会在最小值与最大值之间随机生成，随机的音调偏差能让音效听起来更自然。")]
    #endregion
    [Range(0.1f, 1.5f)]
    public float soundEffectPitchRandomVariationMax = 1.2f;
    #region Tooltip
    [Tooltip("音效音量")]
    #endregion
    [Range(0f, 1f)]
    public float soundEffectVolume = 1f;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(soundEffectName), soundEffectName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundPrefab), soundPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundEffectClip), soundEffectClip);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(soundEffectPitchRandomVariationMin), soundEffectPitchRandomVariationMin, nameof(soundEffectPitchRandomVariationMax), soundEffectPitchRandomVariationMax, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(soundEffectVolume), soundEffectVolume, true);
    }
#endif
    #endregion

}
