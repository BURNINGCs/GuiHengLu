using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ScrollUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Image scrollBackground;      //卷轴背景图片
    [SerializeField] private TextMeshProUGUI chapterTitleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private Button sealButton;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("卷轴配置")]
    [SerializeField] private List<ScrollConfig> scrollConfigs;

    [Header("音频")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip writingSound;

    [Header("动画设置")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float titleDisplayDuration = 1.5f;
    [SerializeField] private float charDisplayInterval = 0.05f;
    [SerializeField] private float sealDisplayDelay = 0.5f;
    [SerializeField] private float sealFadeDuration = 0.5f;

    private System.Action onScrollComplete;
    private Coroutine displayRoutine;
    private Image buttonImage;

    private bool isShowingScroll = false;
    public bool IsShowingScroll => isShowingScroll;

    public enum ScrollType
    {
        Opening,  //开场
        Victory,  //胜利
        Defeat    //失败
    }

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (sealButton != null)
        {
            buttonImage = sealButton.GetComponent<Image>();
            sealButton.onClick.AddListener(OnSealClicked);
        }

        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }

    public void ShowScroll(ScrollType type, int levelIndex, System.Action onComplete)
    {
        currentScrollType = type;
        currentLevelIndex = Mathf.Clamp(levelIndex, 0, scrollConfigs.Count - 1);
        onScrollComplete = onComplete;

        //设置为正在显示卷轴
        isShowingScroll = true;

        gameObject.SetActive(true);

        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        displayRoutine = StartCoroutine(DisplayScrollRoutine());
    }

    private ScrollType currentScrollType;
    private int currentLevelIndex;

    private IEnumerator DisplayScrollRoutine()
    {
        //获取当前配置
        ScrollConfig config = scrollConfigs[currentLevelIndex];
        string content;
        string title;
        Sprite sealSprite;
        Sprite scrollSprite;

        //根据类型选择内容
        switch (currentScrollType)
        {
            case ScrollType.Opening:
                title = config.openingTitle;
                content = config.openingContent;
                sealSprite = config.openingSealSprite;
                scrollSprite = config.openingScrollSprite;
                break;
            case ScrollType.Victory:
                title = config.victoryTitle;
                content = config.victoryContent;
                sealSprite = config.victorySealSprite;
                scrollSprite = config.victoryScrollSprite;
                break;
            case ScrollType.Defeat:
                title = config.defeatTitle;
                content = config.defeatContent;
                sealSprite = config.defeatSealSprite;
                scrollSprite = config.defeatScrollSprite;
                break;
            default:
                yield break;
        }

        //设置卷轴背景图片
        if (scrollBackground != null && scrollSprite != null)
        {
            scrollBackground.sprite = scrollSprite;
        }

        //设置印章图片
        if (buttonImage != null && sealSprite != null)
        {
            buttonImage.sprite = sealSprite;
        }

        //重置UI状态
        ResetUIState();

        //卷轴整体淡入
        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeInDuration));

        //显示标题
        chapterTitleText.gameObject.SetActive(true);
        chapterTitleText.text = FormatVerticalTitle(title);
        chapterTitleText.alpha = 0;

        yield return StartCoroutine(FadeText(chapterTitleText, 0f, 1f, 0.5f));
        yield return new WaitForSeconds(titleDisplayDuration);

        //显示正文内容
        contentText.gameObject.SetActive(true);
        contentText.text = "";

        //如果内容不为空，显示逐字效果
        if (!string.IsNullOrEmpty(content))
        {
            yield return StartCoroutine(TypewriterEffect(content));
        }

        //显示印章
        yield return new WaitForSeconds(sealDisplayDelay);

        if (buttonImage != null)
        {
            yield return StartCoroutine(FadeImage(buttonImage, 0f, 1f, sealFadeDuration));
        }

        //激活印章按钮
        sealButton.interactable = true;
    }

    //标题格式化为竖排（每个字符换行）
    private string FormatVerticalTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            return "";

        string verticalTitle = "";
        for (int i = 0; i < title.Length; i++)
        {
            verticalTitle += title[i];
            if (i < title.Length - 1)
            {
                verticalTitle += "\n";
            }
        }
        return verticalTitle;
    }

    private IEnumerator TypewriterEffect(string text)
    {
        //清除之前的文本
        contentText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            contentText.text += text[i];

            if (i % 2 == 0 && writingSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(writingSound, 0.2f);
            }

            yield return new WaitForSeconds(charDisplayInterval);
        }
    }

    private void OnSealClicked()
    {
        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        StartCoroutine(HideScrollRoutine());
    }

    private IEnumerator HideScrollRoutine()
    {
        //禁用按钮防止重复点击
        if (sealButton != null)
        {
            sealButton.interactable = false;
        }

        //卷轴淡出
        yield return StartCoroutine(FadeCanvas(1f, 0f, 0.8f));

        //隐藏所有UI元素
        if (chapterTitleText != null)
            chapterTitleText.gameObject.SetActive(false);

        if (contentText != null)
            contentText.gameObject.SetActive(false);

        if (sealButton != null)
            sealButton.gameObject.SetActive(false);

        gameObject.SetActive(false);

        //设置为不在显示卷轴
        isShowingScroll = false;

        //执行回调
        onScrollComplete?.Invoke();
    }

    private void ResetUIState()
    {
        if (chapterTitleText != null)
        {
            chapterTitleText.gameObject.SetActive(false);
            chapterTitleText.alpha = 0f;
        }

        if (contentText != null)
        {
            contentText.gameObject.SetActive(false);
            contentText.text = "";
            contentText.alpha = 1f;
        }

        if (sealButton != null)
        {
            sealButton.gameObject.SetActive(true);
            sealButton.interactable = false;

            if (buttonImage != null)
            {
                buttonImage.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private IEnumerator FadeCanvas(float startAlpha, float targetAlpha, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float startAlpha, float targetAlpha, float duration)
    {
        float time = 0f;
        Color startColor = text.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        while (time < duration)
        {
            text.color = Color.Lerp(
                new Color(startColor.r, startColor.g, startColor.b, startAlpha),
                targetColor,
                time / duration
            );
            time += Time.deltaTime;
            yield return null;
        }
        text.color = targetColor;
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float targetAlpha, float duration)
    {
        float time = 0f;
        Color startColor = image.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        while (time < duration)
        {
            image.color = Color.Lerp(
                new Color(startColor.r, startColor.g, startColor.b, startAlpha),
                targetColor,
                time / duration
            );
            time += Time.deltaTime;
            yield return null;
        }
        image.color = targetColor;
    }

    public void ForceHide()
    {
        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        canvasGroup.alpha = 0;
        gameObject.SetActive(false);

        //确保在强制隐藏时也更新状态
        isShowingScroll = false;
    }
}