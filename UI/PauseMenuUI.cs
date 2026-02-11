//using TMPro;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections;

//public class PauseMenuUI : MonoBehaviour
//{
//    #region Tooltip
//    [Tooltip("填充音乐音量大小")]
//    #endregion Tooltip
//    [SerializeField] private TextMeshProUGUI musicLevelText;
//    #region Tooltip
//    [Tooltip("填充音效音量大小")]
//    #endregion Tooltip
//    [SerializeField] private TextMeshProUGUI soundsLevelText;

//    private void Start()
//    {
//        //初始隐藏暂停菜单
//        gameObject.SetActive(false);
//    }

//    //初始化UI文本
//    private IEnumerator InitializeUI()
//    {
//        //等待一帧以确保之前的音乐和音效级别已设置
//        yield return null;

//        //初始化UI文本
//        soundsLevelText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
//        musicLevelText.SetText(MusicManager.Instance.musicVolume.ToString());
//    }

//    private void OnEnable()
//    {
//        Time.timeScale = 0f;

//        //初始化UI文本
//        StartCoroutine(InitializeUI());
//    }

//    private void OnDisable()
//    {
//        Time.timeScale = 1f;
//    }

//    //退出并加载主菜单 - 从暂停菜单UI按钮链接调用
//    public void LoadMainMenu()
//    {
//        SceneManager.LoadScene("MainMenuScene");
//    }

//    //增加音乐音量 - 从UI中的音乐音量增加按钮链接调用
//    public void IncreaseMusicVolume()
//    {
//        MusicManager.Instance.IncreaseMusicVolume();
//        musicLevelText.SetText(MusicManager.Instance.musicVolume.ToString());
//    }

//    //减少音乐音量 - 从UI中的音乐音量减少按钮链接调用
//    public void DecreaseMusicVolume()
//    {
//        MusicManager.Instance.DecreaseMusicVolume();
//        musicLevelText.SetText(MusicManager.Instance.musicVolume.ToString());
//    }

//    //增加音效音量 - 从UI中的音效音量增加按钮链接调用
//    public void IncreaseSoundsVolume()
//    {
//        SoundEffectManager.Instance.IncreaseSoundsVolume();
//        soundsLevelText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
//    }

//    //减少音效音量 - 从UI中的音效音量减少按钮链接调用
//    public void DecreaseSoundsVolume()
//    {
//        SoundEffectManager.Instance.DecreaseSoundsVolume();
//        soundsLevelText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
//    }

//    #region Validation
//#if UNITY_EDITOR

//    private void OnValidate()
//    {
//        HelperUtilities.ValidateCheckNullValue(this, nameof(musicLevelText), musicLevelText);
//        HelperUtilities.ValidateCheckNullValue(this, nameof(soundsLevelText), soundsLevelText);
//    }

//#endif
//    #endregion Validation
//}


using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PauseMenuUI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("填充音乐音量Slider")]
    #endregion Tooltip
    [SerializeField] private Slider musicSlider;
    #region Tooltip
    [Tooltip("填充音效音量Slider")]
    #endregion Tooltip
    [SerializeField] private Slider soundsSlider;

    private void Start()
    {
        //初始隐藏暂停菜单
        gameObject.SetActive(false);
    }

    //初始化UI文本
    private IEnumerator InitializeUI()
    {
        //等待一帧以确保之前的音乐和音效级别已设置
        yield return null;

        //初始化Slider的值
        musicSlider.value = MusicManager.Instance.musicVolume;
        soundsSlider.value = SoundEffectManager.Instance.soundsVolume;
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;

        //初始化UI
        StartCoroutine(InitializeUI());
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    //退出并加载主菜单 - 从暂停菜单UI按钮链接调用
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    // 音乐音量变化时调用 - 从UI中的音乐Slider链接调用
    public void OnMusicVolumeChanged()
    {
        int volume = Mathf.RoundToInt(musicSlider.value);
        MusicManager.Instance.SetMusicVolume(volume);
    }

    // 音效音量变化时调用 - 从UI中的音效Slider链接调用
    public void OnSoundsVolumeChanged()
    {
        int volume = Mathf.RoundToInt(soundsSlider.value);
        SoundEffectManager.Instance.SetSoundsVolume(volume);
    }

    #region Validation
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicSlider), musicSlider);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundsSlider), soundsSlider);
    }

#endif
    #endregion Validation
}