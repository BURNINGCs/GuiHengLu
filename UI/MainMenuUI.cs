using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("对象引用")]
    #endregion Header OBJECT REFERENCES
    #region Tooltip
    [Tooltip("填充开始游戏按钮游戏对象")]
    #endregion Tooltip
    [SerializeField] private GameObject playButton;
    #region Tooltip
    [Tooltip("填充退出游戏按钮游戏对象")]
    #endregion
    [SerializeField] private GameObject quitButton;
    #region Tooltip
    [Tooltip("填充排行榜按钮游戏对象")]
    #endregion
    [SerializeField] private GameObject highScoresButton;
    #region Tooltip
    [Tooltip("填充游戏说明按钮游戏对象")]
    #endregion
    [SerializeField] private GameObject instructionsButton;
    #region Tooltip
    [Tooltip("填充返回主菜单按钮游戏对象")]
    #endregion
    [SerializeField] private GameObject returnToMainMenuButton;
    private bool isInstructionSceneLoaded = false;
    private bool isHighScoresSceneLoaded = false;

    private void Start()
    {
        //播放音乐
        MusicManager.Instance.PlayMusic(GameResources.Instance.mainMenuMusic, 0f, 2f);

        //叠加加载角色选择场景
        SceneManager.LoadScene("CharacterSelectorSceneNEW", LoadSceneMode.Additive);

        returnToMainMenuButton.SetActive(false);
    }

    //从 开始游戏 按钮调用
    public void LoadLoadingScene()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        returnToMainMenuButton.SetActive(false);

        SceneManager.UnloadSceneAsync("CharacterSelectorSceneNEW");

        //叠加加载加载中场景
        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);
    }

    //从 排行榜 按钮调用
    public void LoadHighScores()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        isHighScoresSceneLoaded = true;

        SceneManager.UnloadSceneAsync("CharacterSelectorSceneNEW");

        returnToMainMenuButton.SetActive(true);

        //叠加加载排行榜场景
        SceneManager.LoadScene("HighScoreScene", LoadSceneMode.Additive);
    }

    //从 返回主菜单 按钮调用
    public void LoadCharacterSelector()
    {
        returnToMainMenuButton.SetActive(false);

        if (isHighScoresSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("HighScoreScene");
            isHighScoresSceneLoaded = false;
        }
        else if (isInstructionSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("InstructionsScene");
            isInstructionSceneLoaded = false;
        }

        playButton.SetActive(true);
        quitButton.SetActive(true);
        highScoresButton.SetActive(true);
        instructionsButton.SetActive(true);

        //叠加加载角色选择场景
        SceneManager.LoadScene("CharacterSelectorSceneNEW", LoadSceneMode.Additive);
    }

    //从 游戏说明 按钮调用
    public void LoadInstructions()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        isInstructionSceneLoaded = true;

        SceneManager.UnloadSceneAsync("CharacterSelectorSceneNEW");

        returnToMainMenuButton.SetActive(true);

        //叠加加载游戏说明场景
        SceneManager.LoadScene("InstructionsScene", LoadSceneMode.Additive);
    }

    //退出游戏 - 通过检视器中设置的 onClick 事件调用
    public void QuitGame()
    {
        Application.Quit();
    }


    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(playButton), playButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(quitButton), quitButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(highScoresButton), highScoresButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(instructionsButton), instructionsButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(returnToMainMenuButton), returnToMainMenuButton);
    }
#endif
    #endregion
}
