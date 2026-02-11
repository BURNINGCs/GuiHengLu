using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class characterSelector : MonoBehaviour
{
    #region Tooltip
    [Tooltip("填充Boy角色的Image组件")]
    #endregion
    [SerializeField] private Image boyImage;

    #region Tooltip
    [Tooltip("填充Girl角色的Image组件")]
    #endregion
    [SerializeField] private Image girlImage;

    #region Tooltip
    [Tooltip("填充Boy的输入框")]
    #endregion
    [SerializeField] private TMP_InputField boyNameInput;

    #region Tooltip
    [Tooltip("填充Girl的输入框")]
    #endregion
    [SerializeField] private TMP_InputField girlNameInput;


    //角色数据
    private List<PlayerDetailsSO> playerDetailsList;
    private CurrentPlayerSO currentPlayer;
    private int selectedPlayerIndex = -1; //初始化为-1，为没有选中
    private Coroutine scaleCoroutine;
    private float hoverScale = 1.05f;
    private float selectedScale = 1.15f;
    private float scaleDuration = 0.15f;

    //鼠标悬停状态
    private bool isBoyHovered = false;
    private bool isGirlHovered = false;

    private void Awake()
    {
        //加载资源
        playerDetailsList = GameResources.Instance.playerDetailsList;
        currentPlayer = GameResources.Instance.currentPlayer;
    }

    private void Start()
    {
        //为Image添加点击和鼠标事件
        SetupEvents();

        //设置默认名字
        if (boyNameInput != null) boyNameInput.text = "墨衡";
        if (girlNameInput != null) girlNameInput.text = "晏清";
    }

    //设置所有事件
    private void SetupEvents()
    {
        //为Boy添加事件
        if (boyImage != null)
        {
            AddEventsToImage(boyImage, 0);
        }

        //为Girl添加事件
        if (girlImage != null)
        {
            AddEventsToImage(girlImage, 1);
        }
    }

    //为Image添加点击和鼠标事件
    private void AddEventsToImage(Image image, int characterIndex)
    {
        if (image == null) return;

        //添加Button组件用于点击
        Button button = image.gameObject.GetComponent<Button>();
        if (button == null)
        {
            button = image.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
        }

        //设置点击事件
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SelectCharacter(characterIndex));

        //添加EventTrigger用于鼠标悬停事件
        EventTrigger eventTrigger = image.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = image.gameObject.AddComponent<EventTrigger>();
        }

        //清除旧的事件
        eventTrigger.triggers.Clear();

        //鼠标经过事件
        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
        pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
        pointerEnterEntry.callback.AddListener((data) => OnPointerEnterImage(characterIndex));
        eventTrigger.triggers.Add(pointerEnterEntry);

        //鼠标离开事件
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback.AddListener((data) => OnPointerExitImage(characterIndex));
        eventTrigger.triggers.Add(pointerExitEntry);
    }

    //鼠标经过图片
    private void OnPointerEnterImage(int characterIndex)
    {
        if (characterIndex == 0)
        {
            isBoyHovered = true;
            UpdateScaleEffect();
        }
        else if (characterIndex == 1)
        {
            isGirlHovered = true;
            UpdateScaleEffect();
        }
    }

    //鼠标离开图片
    private void OnPointerExitImage(int characterIndex)
    {
        if (characterIndex == 0)
        {
            isBoyHovered = false;
            UpdateScaleEffect();
        }
        else if (characterIndex == 1)
        {
            isGirlHovered = false;
            UpdateScaleEffect();
        }
    }

    //Boy被点击
    public void OnBoyClicked()
    {
        SelectCharacter(0);
    }

    //Girl被点击
    public void OnGirlClicked()
    {
        SelectCharacter(1);
    }

    //选择角色
    private void SelectCharacter(int index)
    {
        if (playerDetailsList == null || index >= playerDetailsList.Count)
            return;

        selectedPlayerIndex = index;

        //更新当前玩家数据
        currentPlayer.playerDetails = playerDetailsList[index];

        //更新玩家名字
        UpdatePlayerName();

        //更新缩放效果
        UpdateScaleEffect();

        //Debug.Log($"选择了角色: {currentPlayer.playerName}");
    }

    //更新缩放效果
    private void UpdateScaleEffect()
    {
        //停止之前的缩放协程
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        //开始新的缩放动画
        scaleCoroutine = StartCoroutine(ScaleCharacters());
    }

    private System.Collections.IEnumerator ScaleCharacters()
    {
        //计算目标缩放值
        Vector3 boyTargetScale = CalculateTargetScale(0);
        Vector3 girlTargetScale = CalculateTargetScale(1);

        //记录初始缩放
        Vector3 boyStartScale = boyImage != null ? boyImage.transform.localScale : Vector3.one;
        Vector3 girlStartScale = girlImage != null ? girlImage.transform.localScale : Vector3.one;

        float elapsedTime = 0f;

        while (elapsedTime < scaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / scaleDuration);
            float easedT = EaseInOutQuad(t);

            if (boyImage != null)
            {
                boyImage.transform.localScale = Vector3.Lerp(boyStartScale, boyTargetScale, easedT);
            }

            if (girlImage != null)
            {
                girlImage.transform.localScale = Vector3.Lerp(girlStartScale, girlTargetScale, easedT);
            }

            yield return null;
        }

        if (boyImage != null)
        {
            boyImage.transform.localScale = boyTargetScale;
        }

        if (girlImage != null)
        {
            girlImage.transform.localScale = girlTargetScale;
        }
    }

    //计算目标缩放值
    private Vector3 CalculateTargetScale(int characterIndex)
    {
        float scale = 1.0f; //默认缩放

        //如果是选中的角色
        if (characterIndex == selectedPlayerIndex)
        {
            scale = selectedScale;
        }
        //如果是悬停但不是选中的角色
        else if ((characterIndex == 0 && isBoyHovered) || (characterIndex == 1 && isGirlHovered))
        {
            scale = hoverScale;
        }

        return Vector3.one * scale;
    }

    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
    }

    //更新玩家名字
    private void UpdatePlayerName()
    {
        //如果没有选中任何角色，不更新玩家名字
        if (selectedPlayerIndex == -1) return;

        if (selectedPlayerIndex == 0 && boyNameInput != null)
        {
            currentPlayer.playerName = boyNameInput.text;
        }
        else if (selectedPlayerIndex == 1 && girlNameInput != null)
        {
            currentPlayer.playerName = girlNameInput.text;
        }
    }

    //Boy名字改变
    public void OnBoyNameChanged()
    {
        if (boyNameInput != null)
        {
            boyNameInput.text = boyNameInput.text.ToUpper();

            if (selectedPlayerIndex == 0)
            {
                currentPlayer.playerName = boyNameInput.text;
            }
        }
    }

    //Girl名字改变
    public void OnGirlNameChanged()
    {
        if (girlNameInput != null)
        {
            girlNameInput.text = girlNameInput.text.ToUpper();

            if (selectedPlayerIndex == 1)
            {
                currentPlayer.playerName = girlNameInput.text;
            }
        }
    }

    //检查是否已选择角色
    public bool IsCharacterSelected()
    {
        return selectedPlayerIndex >= 0 && selectedPlayerIndex < playerDetailsList.Count;
    }

    //获取当前选择的角色索引
    public int GetSelectedCharacterIndex()
    {
        return selectedPlayerIndex;
    }


    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(boyImage), boyImage);
        HelperUtilities.ValidateCheckNullValue(this, nameof(girlImage), girlImage);
        HelperUtilities.ValidateCheckNullValue(this, nameof(boyNameInput), boyNameInput);
        HelperUtilities.ValidateCheckNullValue(this, nameof(girlNameInput), girlNameInput);
    }
#endif
    #endregion
}